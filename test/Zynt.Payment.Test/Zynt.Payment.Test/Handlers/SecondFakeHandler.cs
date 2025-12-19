namespace Zynt.Payment.Test;

[PaymentHandler(HandlerName)]
public class SecondFakeHandler : IPaymentSuccessHandler
{
    internal const string HandlerName = "second";

    public bool InvokedSuccessHandler { get; private set; }
    
    public Task<PaymentHandlerResult> OnTransactionSuccessAsync(PaymentResult result)
    {
        InvokedSuccessHandler = true;
        return Task.FromResult(PaymentHandlerResult.Success);
    }
}