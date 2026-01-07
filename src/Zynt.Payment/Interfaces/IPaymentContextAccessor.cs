using Zynt.Payment.Models;

public interface IPaymentContextAccessor
{
    PaymentContext? Value { get; set; }
}
