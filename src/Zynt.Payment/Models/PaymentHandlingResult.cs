using System.Runtime.ExceptionServices;
using Zynt.Payment;

public class PaymentHandlingResult
{
    public required PaymentResult Result { get; init; }

    public ExceptionDispatchInfo? Exception { get; init; }
}