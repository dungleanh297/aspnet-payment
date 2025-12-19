namespace Zynt.Payment;

public interface IPaymentProvider
{
    Task<string> CreatePaymentUrlAsync(PaymentRequest request);

    IEnumerable<PaymentServiceDescriptor> GetAllServices();
}