

using Data.DTOs;
using Data.Responses;

namespace ServerLibrary.Patterns {
    public interface IPaymentStrategy {
        Task<PaymentResponseCRM> CreatePayment(AppPaymentRequest request);

        Task HandleWebhook(object webhookData);
    }
}