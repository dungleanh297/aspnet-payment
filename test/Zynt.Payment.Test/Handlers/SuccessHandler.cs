using Zynt.Payment.Attributes;
using Zynt.Payment.Interfaces;

namespace Zynt.Payment.Test;

[PaymentHandler(HandlerName)]
public sealed class SuccessHandler : IPaymentSuccessHandler
{
    public const string HandlerName = "success";

    private HandlerState _state;

    public SuccessHandler(HandlerStateCollection states)
    {
        _state = states.Get(GetType());
    }

    public Task OnTransactionSuccessAsync(PaymentResult result)
    {
        _state.MarkAsInvokedSuccessHandler(result);
        return Task.CompletedTask;
    }
}