using Zynt.Payment.Primitives;

namespace Zynt.Payment.Interfaces;

public interface IPaymentFacade
{
    Task<PaymentRequestUrl> CreatePaymentUrlAsync(PaymentRequest request);

    IEnumerable<PaymentServiceDescriptor> GetAllServices();
}