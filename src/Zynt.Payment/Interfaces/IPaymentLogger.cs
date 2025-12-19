namespace Zynt.Payment.Interfaces;

public interface IPaymentLogger
{
    Task LogAsync(PaymentResult result);
}