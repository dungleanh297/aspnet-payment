using Zynt.Payment.Interfaces;

namespace Zynt.Payment.Test;

public interface IPaymentSuccessTestHandler : ITestHandler, IPaymentSuccessHandler
{
    Task IPaymentSuccessHandler.OnTransactionSuccessAsync(PaymentResult result)
    {
        State.MarkAsInvokedSuccessHandler(result);
        return Task.CompletedTask;
    }
}