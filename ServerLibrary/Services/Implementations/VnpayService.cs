

using Data.DTOs;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ServerLibrary.Data;
using Data.Entities;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Net;
using Data.ThirdPartyModels;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using QRCoder;

namespace ServerLibrary.Patterns
{
    public class VnpayService : IPaymentStrategy
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VnpayService(AppDbContext dbContext, IConfiguration configuration, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
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
                    throw new Exception("License đâu mất tiêu rồi, kiếm lại nha!");

                // Tính số tiền
                decimal amount = request.Amount ?? license.CustomPrice ?? (license.LicenceType == "Yearly"
                    ? license.ApplicationPlan.PriceYearly
                    : license.ApplicationPlan.PriceMonthly);

                var token = license.AutoRenew
                    ? await _dbContext.PaymentTokens
                        .FirstOrDefaultAsync(t => t.PartnerLicenseId == license.Id && t.PaymentMethod == "vnpay")
                    : null;

                var bankCode = string.IsNullOrEmpty(request.BankCode) ? "NCB" : request.BankCode;
                if (license.AutoRenew && bankCode == "VNPAYQR")
                    throw new Exception("VNPAYQR không hỗ trợ AutoRenew nha, chọn NCB hoặc VISA đi!");

                var ipAddr = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

                var vnpRequest = new VnpayRequest
                {
                    TmnCode = _configuration["VNPAY:TmnCode"],
                    Amount = amount,
                    CreateDate = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                    IpAddr = ipAddr,
                    OrderInfo = $"Thanh toán license {license.Id} - {amount} VND",
                    ReturnUrl = _configuration["VNPAY:ReturnUrl"],
                    TxnRef = DateTime.Now.Ticks.ToString(),
                    BankCode = bankCode,
                    Tokenize = license.AutoRenew && token == null,
                    Token = token?.Token,
                    Command = token != null && license.AutoRenew ? "pay_with_token" : "pay"
                };

                // Gọi API VNPAY
                var vnpParams = vnpRequest.ToQueryParams(_configuration["VNPAY:HashSecret"]);
                string payUrl = null;
                bool isSuccess = false;
                string responseToken = null;

                if (vnpRequest.Command == "pay_with_token")
                {
                    // Thanh toán bằng token
                    var content = new FormUrlEncodedContent(vnpParams);
                    var response = await _httpClient.PostAsync("https://sandbox.vnpayment.vn/vnpaygw/api/v1/token", content);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
                    isSuccess = result != null && result.GetValueOrDefault("vnp_ResponseCode") == "00";
                    responseToken = vnpRequest.Token;
                }
                else
                {
                    // Tạo PayUrl
                    var query = string.Join("&", vnpParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
                    payUrl = $"{_configuration["VNPAY:PaymentUrl"]}?{query}";
                    isSuccess = !string.IsNullOrEmpty(payUrl);
                }

                if (!isSuccess)
                    throw new Exception("VNPAY lằng nhằng gì đó, thử lại nha!");

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
                    Status = vnpRequest.Command == "pay_with_token" && isSuccess ? "success" : "pending",
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

                if (vnpRequest.Command == "pay_with_token" && isSuccess)
                {
                    license.Status = "Active";
                    license.LastRenewedAt = DateTime.UtcNow;
                    if (license.LicenceType == "Monthly")
                        license.EndDate = license.EndDate.AddMonths(1);
                    else if (license.LicenceType == "Yearly")
                        license.EndDate = license.EndDate.AddYears(1);
                    else if (license.LicenceType == "Lifetime")
                        license.EndDate = DateTime.MaxValue;

                    _dbContext.PartnerLicenses.Update(license);
                }

                await _dbContext.SaveChangesAsync();

                // Tạo QR code nếu có PayUrl
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
                throw new Exception($"Lỗi tạo thanh toán VNPAY: {ex.Message}");
            }
        }


        public async Task HandleWebhook(object webhookData)
        {
            try
            {
                var inputData = webhookData as Dictionary<string, string>;
                if (inputData == null)
                    throw new Exception("Webhook data kiểu gì kỳ vậy!");

                var vnpRequest = VnpayRequest.FromWebhookData(inputData);
                if (string.IsNullOrEmpty(vnpRequest.SecureHash))
                    throw new Exception("Webhook thiếu SecureHash, VNPAY chơi kỳ vậy!");

                var hashSecret = _configuration["VNPAY:HashSecret"];
                var queryParams = vnpRequest.ToQueryParams(hashSecret);
                queryParams.Remove("vnp_SecureHash"); 
                var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
                var isValid = vnpRequest.SecureHash == ComputeHmacSha512(hashSecret, queryString);
                if (!isValid)
                    throw new Exception("Chữ ký VNPAY sai rồi, đừng hack tui nha!");

                // Lấy giao dịch
                var transaction = await _dbContext.Transactions
                    .Include(t => t.PartnerLicense)
                    .FirstOrDefaultAsync(t => t.TransactionId == vnpRequest.TxnRef);

                if (transaction == null)
                    throw new Exception("Giao dịch mất tiêu rồi, kiếm lại đi!");

                if (vnpRequest.ResponseCode == "00")
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

                    // Lưu token nếu có
                    if (!string.IsNullOrEmpty(vnpRequest.Token) && license.AutoRenew)
                    {
                        var paymentToken = new PaymentToken
                        {
                            PartnerId = transaction.PartnerId,
                            PartnerLicenseId = transaction.PartnerLicenseId,
                            PaymentMethod = "vnpay",
                            Token = vnpRequest.Token,
                            CardNumber = vnpRequest.CardNumber?.Length > 4
                                ? vnpRequest.CardNumber.Substring(vnpRequest.CardNumber.Length - 4)
                                : vnpRequest.CardNumber,
                            ExpiryDate = vnpRequest.ExpiryDate
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
            }
            catch (Exception ex)
            {
                throw new Exception($"Webhook VNPAY lỗi rồi, chill đi tui fix: {ex.Message}");
            }
        }

        private string ComputeHmacSha512(string key, string data)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}