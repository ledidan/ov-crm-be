

namespace Data.Responses
{
    public record PaymentResponseCRM(string PayUrl, string QrCodeBase64, string Token, bool IsSuccess);
}
