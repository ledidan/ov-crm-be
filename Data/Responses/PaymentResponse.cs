

namespace Data.Responses
{
    public record PaymentResponseCRM(string PayUrl, string QrCodeBase64, string Token, bool IsSuccess);
    public class PaymentResponseModel
    {
        public bool Success { get; set; }
        public string PaymentMethod { get; set; }
        public string OrderDescription { get; set; }
        public string OrderId { get; set; }
        public string PaymentId { get; set; }
        public string TransactionId { get; set; }
        public string CardNumber { get; set; }

        public string ExpiryDate { get; set; }
        public string Token { get; set; }
        public string VnPayResponseCode { get; set; }
        public string Message { get; set; }
    }
}
