using Zynt.Payment.Interfaces;

public interface IPaymentContextAccessor
{
    IPaymentContext? Value { get; set; }
}
