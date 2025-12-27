using Zynt.Payment.Primitives;

namespace Zynt.Payment.Interfaces;

public interface IPaymentService
{
    Task<PaymentRequestUrls> CreatePaymentUrlAsync(PaymentRequest request);

    Task<PaymentResult?> GetResultFromRedirectionAsync(Dictionary<string, string?> queryParameters);
}