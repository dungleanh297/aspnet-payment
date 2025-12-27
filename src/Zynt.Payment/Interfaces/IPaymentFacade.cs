using Zynt.Payment.Primitives;

namespace Zynt.Payment.Interfaces;

public interface IPaymentFacade
{
    Task<PaymentRequestUrls> CreatePaymentUrlAsync(PaymentRequest request);

    IEnumerable<PaymentServiceDescriptor> GetAllServices();
}