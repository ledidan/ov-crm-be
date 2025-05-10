



namespace Data.DTOs
{
    public class BankTransferWebhook
    {
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public string Amount { get; set; }
        public string BankCode { get; set; }
        public string ResponseCode { get; set; }
        public string Message { get; set; }
        public string PayDate { get; set; }
        public string Signature { get; set; }
    }
}