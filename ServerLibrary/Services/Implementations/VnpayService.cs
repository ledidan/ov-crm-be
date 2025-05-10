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

namespace ServerLibrary.Services.Implementations
{
    public class VnpayService : IPaymentStrategy
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly VnPayLibrary _vnPayLibrary;
        private readonly ILogger<VnpayService> _logger;

        public VnpayService(
            AppDbContext dbContext,
            IConfiguration configuration,
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<VnpayService> logger,
             VnPayLibrary vnPayLibrary)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _vnPayLibrary = vnPayLibrary;
            _logger = logger;
        }

        public async Task<PaymentResponseCRM> CreatePayment(AppPaymentRequest request)
        {
            try
            {
                // Tra cứu PartnerLicense
                var license = await _dbContext.PartnerLicenses
                    .Include(l => l.ApplicationPlan)
                    .FirstOrDefaultAsync(l => l.Id == request.PartnerLicenseId);
                if (license == null)
                {
                    _logger.LogWarning("CreatePayment: Không tìm thấy PartnerLicense {PartnerLicenseId}", request.PartnerLicenseId);
                    throw new Exception("License đâu mất tiêu rồi, kiếm lại nha!");
                }

                // Tính số tiền
                decimal amount = request.Amount ?? license.CustomPrice ?? (license.LicenceType == "Yearly"
                    ? license.ApplicationPlan.PriceYearly
                    : license.ApplicationPlan.PriceMonthly);

                // Validate BankCode
                var bankCode = string.IsNullOrEmpty(request.BankCode) ? null : request.BankCode.ToUpper();

                // Kiểm tra AutoRenew
                var token = license.AutoRenew
                    ? await _dbContext.PaymentTokens
                        .FirstOrDefaultAsync(t => t.PartnerLicenseId == license.Id && t.PaymentMethod == "vnpay")
                    : null;

                if (license.AutoRenew && bankCode == "VNPAYQR")
                {
                    _logger.LogWarning("CreatePayment: VNPAYQR không hỗ trợ AutoRenew");
                    throw new Exception("VNPAYQR không hỗ trợ AutoRenew nha, chọn VNBANK hoặc INTCARD!");
                }

                var ipAddr = _vnPayLibrary.GetIpAddress(_httpContextAccessor.HttpContext);
                var createDate = DateTime.Now;
                var expireDate = createDate.AddMinutes(15); // Hết hạn sau 15 phút

                var vnpRequest = new
                {
                    Version = "2.1.0",
                    TmnCode = _configuration["VNPAY:TmnCode"],
                    Amount = amount,
                    Command = token != null && license.AutoRenew ? "pay_with_token" : "pay",
                    CreateDate = createDate.ToString("yyyyMMddHHmmss"),
                    ExpireDate = expireDate.ToString("yyyyMMddHHmmss"),
                    CurrCode = "VND",
                    IpAddr = ipAddr,
                    Locale = "vn",
                    OrderInfo = $"Thanh toan license {license.Id} - {amount} VND", // Tiếng Việt không dấu
                    OrderType = "250000",
                    ReturnUrl = _configuration["VNPAY:ReturnUrl"],
                    TxnRef = DateTime.Now.Ticks.ToString(),
                    BankCode = bankCode,
                    Tokenize = license.AutoRenew && token == null,
                    Token = token?.Token
                };

                // Lưu giao dịch
                var transaction = new Transactions
                {
                    PartnerId = license.PartnerId,
                    PartnerLicenseId = license.Id,
                    ApplicationId = license.ApplicationId,
                    ApplicationPlanId = license.ApplicationPlanId,
                    Amount = amount,
                    Currency = "VND",
                    PaymentMethod = "vnpay",
                    Status = "pending",
                    TransactionId = vnpRequest.TxnRef,
                    Metadata = JsonConvert.SerializeObject(new
                    {
                        LicenceType = license.LicenceType,
                        PlanName = license.ApplicationPlan?.Name,
                        BankCode = bankCode,
                        AutoRenew = license.AutoRenew
                    })
                };
                _dbContext.Transactions.Add(transaction);
                await _dbContext.SaveChangesAsync();

                string payUrl = null;
                bool isSuccess = false;
                string responseToken = null;

                if (vnpRequest.Command == "pay_with_token")
                {
                    // Thanh toán bằng token
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
                        transaction.Status = "success";
                        license.Status = "Active";
                        license.LastRenewedAt = DateTime.UtcNow;
                        if (license.LicenceType == "Monthly")
                            license.EndDate = license.EndDate.AddMonths(1);
                        else if (license.LicenceType == "Yearly")
                            license.EndDate = license.EndDate.AddYears(1);
                        else if (license.LicenceType == "Lifetime")
                            license.EndDate = DateTime.MaxValue;

                        _dbContext.PartnerLicenses.Update(license);
                        await _dbContext.SaveChangesAsync();
                    }
                }
                else
                {
                    // Tạo PayUrl
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
                        _vnPayLibrary.AddRequestData("vnp_BankCode", vnpRequest.BankCode); // Thêm BankCode nếu có
                    if (vnpRequest.Tokenize)
                        _vnPayLibrary.AddRequestData("vnp_Tokenize", "1");

                    payUrl = _vnPayLibrary.CreateRequestUrl(
                        _configuration["VNPAY:PaymentUrl"],
                        _configuration["VNPAY:HashSecret"]);
                    isSuccess = !string.IsNullOrEmpty(payUrl);
                }

                if (!isSuccess)
                {
                    _logger.LogWarning("CreatePayment: VNPAY thất bại, không tạo được PayUrl hoặc token");
                    throw new Exception("VNPAY lằng nhằng gì đó, thử lại nha!");
                }

                string qrCodeBase64 = null;
                if (!string.IsNullOrEmpty(payUrl))
                {
                    var qrGenerator = new QRCodeGenerator();
                    var qrCodeData = qrGenerator.CreateQrCode(payUrl, QRCodeGenerator.ECCLevel.Q);
                    var qrCode = new PngByteQRCode(qrCodeData);
                    var qrCodeBytes = qrCode.GetGraphic(20);
                    qrCodeBase64 = $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
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
                    _logger.LogWarning("Webhook: Không tìm thấy giao dịch {OrderId}", response.OrderId);
                    throw new Exception("Giao dịch mất tiêu rồi, kiếm lại đi!");
                }

                if (response.VnPayResponseCode == "00")
                {
                    transaction.Status = "success";
                    transaction.PartnerLicense.Status = "Active";
                    transaction.PartnerLicense.LastRenewedAt = DateTime.UtcNow;

                    var license = transaction.PartnerLicense;
                    if (license.LicenceType == "Monthly")
                        license.EndDate = license.EndDate.AddMonths(1);
                    else if (license.LicenceType == "Yearly")
                        license.EndDate = license.EndDate.AddYears(1);
                    else if (license.LicenceType == "Lifetime")
                        license.EndDate = DateTime.MaxValue;

                    if (!string.IsNullOrEmpty(response.Token) && license.AutoRenew)
                    {
                        var paymentToken = new PaymentToken
                        {
                            PartnerId = transaction.PartnerId,
                            PartnerLicenseId = transaction.PartnerLicenseId,
                            PaymentMethod = "vnpay",
                            Token = response.Token,
                            CardNumber = response.CardNumber?.Length > 4
                                ? response.CardNumber.Substring(response.CardNumber.Length - 4)
                                : response.CardNumber,
                            ExpiryDate = response.ExpiryDate
                        };
                        _dbContext.PaymentTokens.Add(paymentToken);
                    }

                    _dbContext.Transactions.Update(transaction);
                    _dbContext.PartnerLicenses.Update(license);
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
                throw new Exception($"Webhook VNPAY lỗi rồi, chill đi tui fix: {ex.Message}");
            }
        }
    }
}