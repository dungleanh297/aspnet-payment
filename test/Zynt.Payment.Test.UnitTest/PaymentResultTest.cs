using Zynt.Payment;

public class PaymentResultTest
{
    internal static PaymentResult CreatePaymentResult()
    {
        return new PaymentResult
        {
            Amount = 1000,
            Currency = "USD",
            CreatedDate = DateTimeOffset.UtcNow,
            HandlerName = "test-handler",
            Identifier = "test-identifier",
            ServiceName = "test-service",
            Status = PaymentStatus.Success,
        };
    }
}