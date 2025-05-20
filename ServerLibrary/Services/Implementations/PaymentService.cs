using Data.DTOs;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Web;
using QRCoder;
using Data.Entities;
using Newtonsoft.Json;
using ServerLibrary.Data;
using ServerLibrary.Libraries;
using ServerLibrary.Services.Interfaces;
using Data.Enums;
using ServerLibrary.Helpers;

namespace ServerLibrary.Services.Implementations
{
    public class PaymentService : IPaymentStrategy
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly VnPayLibrary _vnPayLibrary;
        private readonly ILogger<PaymentService> _logger;

        private readonly IUserService _userService;

        private readonly IEmailService _emailService;
        private readonly ILicenseCenterService _licenseCenterService;
        public PaymentService(
            AppDbContext dbContext,
            IConfiguration configuration,
            HttpClient httpClient,
            IUserService userService,
            ILicenseCenterService licenseCenterService,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService,
            ILogger<PaymentService> logger,
             VnPayLibrary vnPayLibrary)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _vnPayLibrary = vnPayLibrary;
            _userService = userService;
            _licenseCenterService = licenseCenterService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<PaymentResponseCRM> CreatePayment(AppPaymentRequest request)
        {
            try
            {
                // 1. Validate request
                var user = await ValidateRequest(request);

                // Tra cứu hoặc tạo PartnerLicense
                var licenses = await _licenseCenterService.GetOrCreateLicenses(request);

                // 3. Tính số tiền
                // var amount = CalculateAmount(request, licenses);

                // 4. Kiểm tra AutoRenew và token (dùng license đầu tiên nếu có AutoRenew)
                var token = licenses.Any(l => l.AutoRenew)
                    ? await _dbContext.PaymentTokens
                        .FirstOrDefaultAsync(t => t.PartnerLicenseId == licenses.First(l => l.AutoRenew).Id && t.PaymentMethod == "vnpay")
                    : null;

                var isTransfer = string.IsNullOrEmpty(request.BankCode);
                if (isTransfer && licenses.Any(l => l.AutoRenew))
                {
                    _logger.LogWarning("CreatePayment: Chuyển khoản không hỗ trợ AutoRenew");
                    throw new Exception("Chuyển khoản không hỗ trợ AutoRenew nha, chọn VNBANK hoặc INTCARD!");
                }

                // 5. Tạo thông tin chuyển khoản (nếu cần)
                var (orderInfo, transferInfo, uniqueId) = GenerateTransferInfo(licenses, request.Amount, isTransfer);

                // ** Get Ip
                var ipAddr = _vnPayLibrary.GetIpAddress(_httpContextAccessor.HttpContext);
                var vnTimeZone = GetRegionTimeZone.GetVietnamTimeZone(); // ** Set Timezone for vnpay transaction
                var createdDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
                var expireDate = createdDate.AddMinutes(20); // Hết hạn sau 15 phút
                // 2. Kiểm tra giao dịch cũ
                var existingTransaction = await _dbContext.Transactions
                    .FirstOrDefaultAsync(t => t.UserId == request.UserId
                        && t.PartnerLicenseId == request.PartnerLicenseId
                        && t.Status == "pending");
                if (existingTransaction != null)
                {
                    var createDate = DateTime.ParseExact(
                        existingTransaction.Metadata.Contains("CreateDate")
                            ? JsonConvert.DeserializeObject<dynamic>(existingTransaction.Metadata).CreateDate
                            : DateTime.Now.ToString("yyyyMMddHHmmss"),
                        "yyyyMMddHHmmss",
                        null);
                    if (DateTime.Now > expireDate)
                    {
                        existingTransaction.Status = "expired";
                        _dbContext.Transactions.Update(existingTransaction);
                        await _dbContext.SaveChangesAsync();
                        _logger.LogInformation("CreatePaymentForExistedLicense: Đánh dấu giao dịch {TransactionId} là expired",
                        existingTransaction.TransactionId);
                    }
                    else
                    {
                        _logger.LogWarning("CreatePaymentForExistedLicense: Giao dịch {TransactionId} vẫn đang pending",
                         existingTransaction.TransactionId);
                        throw new Exception("Giao dịch đang chờ xử lý, đợi tí nha!");
                    }
                }
                // 6. Tạo request VNPAY
                var vnpRequest = new
                {
                    Version = "2.1.0",
                    TmnCode = _configuration["VNPAY:TmnCode"],
                    Amount = request.Amount,
                    Command = token != null && licenses.Any(l => l.AutoRenew) && !isTransfer ? "pay_with_token" : "pay",
                    CreateDate = createdDate.ToString("yyyyMMddHHmmss"),
                    ExpireDate = expireDate.ToString("yyyyMMddHHmmss"),
                    CurrCode = "VND",
                    IpAddr = ipAddr,
                    Locale = "vn",
                    OrderInfo = orderInfo,
                    OrderType = "250000",
                    ReturnUrl = _configuration["VNPAY:ReturnUrl"],
                    TxnRef = DateTime.Now.Ticks.ToString(),
                    BankCode = request.BankCode?.ToUpper(),
                    Tokenize = licenses.Any(l => l.AutoRenew) && token == null && !isTransfer,
                    Token = token?.Token
                };

                // 7. Lưu giao dịch
                var transaction = await CreateTransaction(request, licenses, vnpRequest, request.Amount, isTransfer, uniqueId, transferInfo);

                // 8. Tạo PayUrl hoặc xử lý pay_with_token
                string payUrl = null;
                bool isSuccess = false;
                string responseToken = null;

                if (isTransfer)
                {
                    isSuccess = true;
                }
                else
                {
                    (payUrl, isSuccess, responseToken) = await GenerateVnpayUrl(vnpRequest, token, transaction, licenses, user);
                }

                // 9. Tạo QR code nếu có PayUrl
                var qrCodeBase64 = GenerateQrCode(payUrl);

                if (!isSuccess)
                {
                    _logger.LogWarning("CreatePayment: VNPAY thất bại, không tạo được PayUrl hoặc token");
                    throw new Exception("VNPAY lằng nhằng gì đó, thử lại nha!");
                }
                return new PaymentResponseCRM(
                    PayUrl: payUrl ?? "",
                    QrCodeBase64: qrCodeBase64 ?? "",
                    Token: responseToken ?? "",
                    IsSuccess: isSuccess
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreatePayment lỗi: {Message}", ex.Message);
                throw new Exception($"Lỗi tạo thanh toán VNPAY: {ex.Message}");
            }
        }


        private async Task<ApplicationUser> ValidateRequest(AppPaymentRequest request)
        {
            var user = await _dbContext.ApplicationUsers
                .FirstOrDefaultAsync(u => u.Id == request.UserId);
            if (user == null)
            {
                _logger.LogWarning("CreatePaymentForExistedLicense: Không tìm thấy User {UserId}", request.UserId);
                throw new Exception("User đâu mất tiêu rồi, kiếm lại nha!");
            }

            var validBanks = new[] { "VNBANK", "INTCARD" };
            if (!string.IsNullOrEmpty(request.BankCode) && !validBanks.Contains(request.BankCode.ToUpper()))
            {
                _logger.LogWarning("CreatePaymentForExistedLicense: BankCode {BankCode} không hợp lệ", request.BankCode);
                throw new Exception($"BankCode {request.BankCode} không hỗ trợ nha, chọn VNBANK hoặc INTCARD!");
            }

            if (request.AppItems == null || !request.AppItems.Any())
            {
                _logger.LogWarning("CreatePaymentForExistedLicense: Danh sách AppItems rỗng");
                throw new Exception("Chọn ít nhất một app để thanh toán nha!");
            }

            foreach (var item in request.AppItems)
            {
                var plan = await _dbContext.ApplicationPlans
                    .FirstOrDefaultAsync(p => p.Id == item.ApplicationPlanId);
                if (plan == null)
                {
                    _logger.LogWarning("CreatePaymentForExistedLicense: Không tìm thấy ApplicationPlan {ApplicationPlanId}", item.ApplicationPlanId);
                    throw new Exception($"Plan {item.ApplicationPlanId} không tồn tại, chọn lại nha!");
                }
            
                if ((item.LicenceType == "Monthly" || item.LicenceType == "Yearly") && (!item.Duration.HasValue || item.Duration <= 0))
                {
                    _logger.LogWarning("CreatePaymentForExistedLicense: Duration phải lớn hơn 0 cho LicenceType {LicenceType}", item.LicenceType);
                    throw new Exception($"Chọn {item.LicenceType} thì phải nhập số tháng/năm lớn hơn 0 nha!");
                }
            }

            return user;
        }

        private (string orderInfo, object transferInfo, string uniqueId) GenerateTransferInfo(List<PartnerLicense> licenses,
        decimal amount, bool isTransfer)
        {
            string uniqueId = isTransfer ? DateTime.Now.Ticks.ToString().Substring(8) : null;
            string licenseIds = string.Join(",", licenses.Select(l => l.Id));
            string orderInfo = isTransfer
                ? $"Chuyen khoan licenses {licenseIds} - {uniqueId} - ACB 1234567890"
                : $"Thanh toan licenses {licenseIds} - {amount} VND";

            if (orderInfo.Length > 255)
            {
                _logger.LogWarning("CreatePaymentForExistedLicense: vnp_OrderInfo quá dài ({Length} ký tự)", orderInfo.Length);
                orderInfo = $"Chuyen khoan licenses {licenseIds} - {uniqueId}";
            }

            object transferInfo = isTransfer ? new
            {
                TransferNote = $"Licenses {licenseIds} - {uniqueId}"
            } : null;

            return (orderInfo, transferInfo, uniqueId);
        }

        private async Task<Transactions> CreateTransaction(
            AppPaymentRequest request,
            List<PartnerLicense> licenses,
            dynamic vnpRequest,
            decimal amount,
            bool isTransfer,
            string uniqueId,
            object transferInfo)
        {
            var transaction = new Transactions
            {
                TransactionId = vnpRequest.TxnRef,
                UserId = request.UserId,
                PartnerId = licenses.FirstOrDefault()?.PartnerId,
                PartnerLicenseId = licenses.FirstOrDefault()?.Id ?? 0, // Lưu license đầu tiên (hoặc để null)
                ApplicationId = licenses.FirstOrDefault()?.ApplicationId ?? 0,
                ApplicationPlanId = licenses.FirstOrDefault()?.ApplicationPlanId,
                Amount = amount,
                Currency = "VND",
                PaymentMethod = isTransfer ? "transfer" : "vnpay",
                Status = isTransfer ? "pending_manual" : "pending",
                Metadata = JsonConvert.SerializeObject(new
                {
                    LicenseIds = licenses.Select(l => l.Id).ToList(),
                    AppItems = request.AppItems,
                    BankCode = request.BankCode,
                    AutoRenew = licenses.Any(l => l.AutoRenew),
                    UniqueId = uniqueId,
                    TransferInfo = transferInfo
                })
            };
            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();
            return transaction;
        }
        private async Task<(string payUrl, bool isSuccess, string responseToken)> GenerateVnpayUrl(
           dynamic vnpRequest,
           PaymentToken token,
           Transactions transaction,
           List<PartnerLicense> licenses,
           ApplicationUser user)
        {
            string payUrl = null;
            bool isSuccess = false;
            string responseToken = null;

            if (vnpRequest.Command == "pay_with_token")
            {
                _vnPayLibrary.AddRequestData("vnp_Version", vnpRequest.Version);
                _vnPayLibrary.AddRequestData("vnp_TmnCode", vnpRequest.TmnCode);
                _vnPayLibrary.AddRequestData("vnp_Amount", ((long)(vnpRequest.Amount * 100)).ToString());
                _vnPayLibrary.AddRequestData("vnp_Command", vnpRequest.Command);
                _vnPayLibrary.AddRequestData("vnp_CreateDate", vnpRequest.CreateDate);
                _vnPayLibrary.AddRequestData("vnp_CurrCode", vnpRequest.CurrCode);
                _vnPayLibrary.AddRequestData("vnp_IpAddr", vnpRequest.IpAddr);
                _vnPayLibrary.AddRequestData("vnp_Locale", vnpRequest.Locale);
                _vnPayLibrary.AddRequestData("vnp_OrderInfo", vnpRequest.OrderInfo);
                _vnPayLibrary.AddRequestData("vnp_ReturnUrl", vnpRequest.ReturnUrl);
                _vnPayLibrary.AddRequestData("vnp_TxnRef", vnpRequest.TxnRef);
                _vnPayLibrary.AddRequestData("vnp_Token", vnpRequest.Token);

                var vnpParams = _vnPayLibrary.CreateRequestUrl(
                    "https://sandbox.vnpayment.vn/vnpaygw/api/v1/token",
                    _configuration["VNPAY:HashSecret"]).Split('?')[1]
                    .Split('&').ToDictionary(k => k.Split('=')[0], v => HttpUtility.UrlDecode(v.Split('=')[1]));

                var content = new FormUrlEncodedContent(vnpParams);
                var response = await _httpClient.PostAsync("https://sandbox.vnpayment.vn/vnpaygw/api/v1/token", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
                isSuccess = result != null && result.GetValueOrDefault("vnp_ResponseCode") == "00";
                responseToken = vnpRequest.Token;

                if (isSuccess)
                {
                    await UpdateLicenseAndTransactionAfterSuccessPayment(transaction, licenses, user);
                }
            }
            else
            {
                _vnPayLibrary.AddRequestData("vnp_Version", vnpRequest.Version);
                _vnPayLibrary.AddRequestData("vnp_TmnCode", vnpRequest.TmnCode);
                _vnPayLibrary.AddRequestData("vnp_Amount", ((long)(vnpRequest.Amount * 100)).ToString());
                _vnPayLibrary.AddRequestData("vnp_Command", vnpRequest.Command);
                _vnPayLibrary.AddRequestData("vnp_CreateDate", vnpRequest.CreateDate);
                _vnPayLibrary.AddRequestData("vnp_ExpireDate", vnpRequest.ExpireDate);
                _vnPayLibrary.AddRequestData("vnp_CurrCode", vnpRequest.CurrCode);
                _vnPayLibrary.AddRequestData("vnp_IpAddr", vnpRequest.IpAddr);
                _vnPayLibrary.AddRequestData("vnp_Locale", vnpRequest.Locale);
                _vnPayLibrary.AddRequestData("vnp_OrderInfo", vnpRequest.OrderInfo);
                _vnPayLibrary.AddRequestData("vnp_OrderType", vnpRequest.OrderType);
                _vnPayLibrary.AddRequestData("vnp_ReturnUrl", vnpRequest.ReturnUrl);
                _vnPayLibrary.AddRequestData("vnp_TxnRef", vnpRequest.TxnRef);
                if (!string.IsNullOrEmpty(vnpRequest.BankCode))
                    _vnPayLibrary.AddRequestData("vnp_BankCode", vnpRequest.BankCode);
                if (vnpRequest.Tokenize)
                    _vnPayLibrary.AddRequestData("vnp_Tokenize", "1");

                payUrl = _vnPayLibrary.CreateRequestUrl(
                    _configuration["VNPAY:PaymentUrl"],
                    _configuration["VNPAY:HashSecret"]);
                isSuccess = !string.IsNullOrEmpty(payUrl);
            }

            return (payUrl, isSuccess, responseToken);
        }

        private string GenerateQrCode(string payUrl)
        {
            if (string.IsNullOrEmpty(payUrl))
                return null;

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(payUrl, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);
            return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
        }

        private async Task UpdateLicenseAndTransactionAfterSuccessPayment(Transactions transaction, List<PartnerLicense> licenses, ApplicationUser user)
        {

            _logger.LogInformation("===> Start updating licenses for user {UserId} with transaction {TransactionId}", user.Id, transaction.Id);

            if (string.IsNullOrWhiteSpace(transaction.Metadata))
            {
                _logger.LogError("Transaction {TransactionId} metadata is null or empty", transaction.Id);
                return;
            }
            transaction.Status = "success";
            var metadata = JsonConvert.DeserializeObject<dynamic>(transaction.Metadata);

            var appItems = JsonConvert.DeserializeObject<List<AppItem>>(((object)metadata.AppItems).ToString());
            var licenseAppIds = licenses.Select(l => l.ApplicationId).ToList();

            var hasActiveLicenses = await _dbContext.PartnerLicenses
            .AnyAsync(l => l.UserId == user.Id && (l.Status == "Active" || l.Status == "Expired") && licenseAppIds.Contains(l.ApplicationId));

            _logger.LogInformation("User {UserId} has active licenses: {HasActive}", user.Id, hasActiveLicenses);

            for (int i = 0; i < licenses.Count; i++)
            {
                var license = licenses[i];
                var appItem = appItems[i];
                if (appItem == null)
                {
                    _logger.LogWarning("AppItem not found for license index {Index}", i);
                    continue;
                }

                if (hasActiveLicenses)
                {
                    _logger.LogInformation("Updating existing license for AppId {AppId}", license.ApplicationId);
                    license.LicenceType = appItem.LicenceType;
                    license.AutoRenew = appItem.AutoRenew;
                    license.LastRenewedAt = DateTime.UtcNow;
                    license.ApplicationPlanId = appItem.ApplicationPlanId;

                    // Query existingLicense cho app hiện tại
                    var existingLicense = await _dbContext.PartnerLicenses
                        .FirstOrDefaultAsync(l => l.UserId == user.Id
                        && l.ApplicationId == license.ApplicationId && l.Status == "Active");

                    // Tính ngày trial còn lại
                    var remainingTrialDays = existingLicense != null && existingLicense.EndDate > DateTime.UtcNow
                        ? (existingLicense.EndDate - DateTime.UtcNow).Days
                        : 0;

                    int duration = appItem.LicenceType == "Lifetime" ? 0 : (appItem.Duration ?? 1);
                    if (appItem.LicenceType == "Monthly")
                    {
                        license.EndDate = DateTime.UtcNow.AddMonths(duration).AddDays(remainingTrialDays);
                    }
                    else if (appItem.LicenceType == "Yearly")
                    {
                        license.EndDate = DateTime.UtcNow.AddYears(duration).AddDays(remainingTrialDays);
                    }
                    else if (appItem.LicenceType == "Lifetime")
                    {
                        license.EndDate = DateTime.MaxValue;
                    }
                    _logger.LogInformation("License {LicenseId} updated. New EndDate: {EndDate}", license.Id, license.EndDate);

                }
                else
                {
                    // ** Handle User buy new app
                    _logger.LogInformation("User buy new app", user.Id);
                    var activationCode = Guid.NewGuid().ToString().Substring(0, 8);
                    var appNames = string.Join(",", appItems.Select(item => item.ApplicationName ?? "Unknown App"));

                    if (string.IsNullOrEmpty(appNames))
                    {
                        appNames = "Ứng dụng không xác định";
                        _logger.LogWarning("No valid application names found for licenses in transaction {TransactionId}", transaction.Id);
                    }
                    await SendActivationEmailAsync(
                        user.Email,
                        user.FullName ?? "User",
                        activationCode,
                        appNames
                    );
                    _logger.LogInformation("Đã gửi email kích hoạt cho user {UserId}", user.Id);

                    license.ActivationCode = activationCode;
                    license.LastRenewedAt = DateTime.UtcNow;
                    license.LicenceType = appItem.LicenceType;
                    license.ApplicationPlanId = appItem.ApplicationPlanId;
                    license.UserId = user.Id;
                    int duration = appItem.LicenceType == "Lifetime" ? 0 : (appItem.Duration ?? 1);

                    if (appItem.LicenceType == "Monthly")
                    {
                        license.EndDate = DateTime.UtcNow.AddMonths(duration).AddDays(15); // Tặng 15 ngày trial
                    }
                    else if (appItem.LicenceType == "Yearly")
                    {
                        license.EndDate = DateTime.UtcNow.AddYears(duration).AddDays(15); // Tặng 15 ngày trial
                    }
                    else if (appItem.LicenceType == "Lifetime")
                    {
                        license.EndDate = DateTime.MaxValue;
                    }
                    _logger.LogInformation("New license {LicenseId} created. EndDate: {EndDate}", license.Id, license.EndDate);

                }

                _dbContext.PartnerLicenses.Update(license);
            }

            _dbContext.Transactions.Update(transaction);
            _dbContext.ApplicationUsers.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task HandleWebhook(object webhookData)
        {
            try
            {
                var response = _vnPayLibrary.GetFullResponseData(
                    _httpContextAccessor.HttpContext.Request.Query,
                    _configuration["VNPAY:HashSecret"]);

                if (!response.Success)
                {
                    _logger.LogWarning("Webhook thất bại: {Message}, ResponseCode: {ResponseCode}", response.Message, response.VnPayResponseCode);
                    throw new Exception(response.Message);
                }

                var transaction = await _dbContext.Transactions
                    .Include(t => t.PartnerLicense)
                    .FirstOrDefaultAsync(t => t.TransactionId == response.OrderId);
                if (transaction == null)
                {
                    _logger.LogError("Webhook: Không tìm thấy giao dịch với OrderId {OrderId}", response.OrderId);
                    throw new Exception($"Không tìm thấy giao dịch với OrderId: {response.OrderId}");
                }
                var user = await _userService.GetApplicationUserByIdAsync(transaction.UserId);

                if (user == null)
                {
                    _logger.LogError("Webhook: Không tìm thấy user với UserId {UserId} cho giao dịch {OrderId}", transaction.UserId, response.OrderId);
                    throw new Exception($"Không tìm thấy user với UserId: {transaction.UserId}");
                }
                if (transaction == null)
                {
                    _logger.LogWarning("Webhook: Không tìm thấy giao dịch {OrderId}", response.OrderId);
                    throw new Exception("Giao dịch mất tiêu rồi, kiếm lại đi!");
                }

                if (response.VnPayResponseCode == "00")
                {
                    if (string.IsNullOrEmpty(transaction.Metadata))
                    {
                        _logger.LogError("Webhook: Metadata rỗng cho giao dịch {OrderId}", response.OrderId);
                        throw new Exception("Metadata giao dịch rỗng");
                    }

                    TransactionMetadata metadata;
                    try
                    {
                        metadata = JsonConvert.DeserializeObject<TransactionMetadata>(transaction.Metadata);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Webhook: Lỗi deserialize metadata cho giao dịch {OrderId}", response.OrderId);
                        throw new Exception($"Lỗi deserialize metadata: {ex.Message}");
                    }
                    var licenseIds = metadata?.LicenseIds;
                    if (licenseIds == null || !licenseIds.Any())
                    {
                        _logger.LogError("Webhook: LicenseIds rỗng hoặc null trong metadata cho giao dịch {OrderId}", response.OrderId);
                        throw new Exception("LicenseIds không hợp lệ trong metadata");
                    }
                    var licenses = await _dbContext.PartnerLicenses
                        .Where(l => licenseIds.Contains(l.Id))
                        .ToListAsync();

                    if (!licenses.Any())
                    {
                        _logger.LogError("Webhook: Không tìm thấy license nào với LicenseIds {LicenseIds} cho giao dịch {OrderId}", string.Join(",", licenseIds), response.OrderId);
                        throw new Exception($"Không tìm thấy license với LicenseIds: {string.Join(",", licenseIds)}");
                    }
                    await UpdateLicenseAndTransactionAfterSuccessPayment(transaction, licenses, user);
                    if (!string.IsNullOrEmpty(response.Token) && licenses.Any(l => l.AutoRenew))
                    {
                        var paymentToken = new PaymentToken
                        {
                            PartnerId = transaction.PartnerId ?? 0,
                            PartnerLicenseId = licenses.First(l => l.AutoRenew).Id,
                            PaymentMethod = "vnpay",
                            Token = response.Token,
                            CardNumber = response.CardNumber?.Length > 4
                                ? response.CardNumber.Substring(response.CardNumber.Length - 4)
                                : response.CardNumber,
                            ExpiryDate = response.ExpiryDate
                        };
                        _dbContext.PaymentTokens.Add(paymentToken);
                    }
                }
                else
                {
                    transaction.Status = "failed";
                    _dbContext.Transactions.Update(transaction);
                }

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Webhook xử lý giao dịch {OrderId} thành công", response.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook VNPAY lỗi: {Message}", ex.Message);
                throw new Exception($"Webhook VNPAY lỗi: {ex.Message}");
            }
        }
        private async Task SendActivationEmailAsync(string email, string fullName, string activationCode, string appName)
        {
            try
            {
                // Validate configuration
                var baseUrl = _configuration["Frontend:BaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new InvalidOperationException("Frontend:BaseUrl is not configured.");
                }

                // Construct activation link
                var activationLink = $"{baseUrl}/vi/auth/activate-license?code={Uri.EscapeDataString(activationCode)}&email={Uri.EscapeDataString(email)}&appname={Uri.EscapeDataString(appName)}";

                // Create the model for the email template
                var model = new ActivationEmailModel
                {
                    FullName = fullName,
                    VerificationLink = activationLink,
                    Email = email,
                    ActivationCode = activationCode,
                    AppName = appName
                };

                var templateName = "ActivationEmail.cshtml";
                var emailBody = await _emailService.GetActivateEmailTemplateAsync(model, templateName);

                await _emailService.SendEmailAsync(email, $"Kích hoạt tài khoản {appName}", emailBody);

                _logger.LogInformation($"Activation email sent to {email}");
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex, $"Failed to send activation email to {email}");
                throw new Exception($"Failed to send activation email to {email}", ex);
            }
        }
    }
}