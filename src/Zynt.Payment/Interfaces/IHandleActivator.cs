namespace Zynt.Payment.Interfaces;

public interface IPaymentHandleActivator
{
    Task InvokeAsync(PaymentResult result);
}