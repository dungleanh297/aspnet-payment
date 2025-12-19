using Microsoft.Extensions.Options;

namespace Zynt.Payment.Test;

[PaymentHandler(HandlerName)]
public class FirstFakeHandler : IPaymentSuccessHandler, IDisposable
{
    internal const string HandlerName = "first";

    private readonly HandlerState<FirstFakeHandler> _state;

    public FirstFakeHandler(HandlerState<FirstFakeHandler> state)
    {
        _state = state;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _state.MarkAsDisposed();
    }

    public Task<PaymentHandlerResult> OnTransactionSuccessAsync(PaymentResult result)
    {
        _state.MarkAsInvokedSuccessHandler();
        return Task.FromResult(PaymentHandlerResult.Success);
    }
}