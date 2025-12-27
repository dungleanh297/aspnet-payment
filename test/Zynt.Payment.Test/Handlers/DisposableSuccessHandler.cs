using Zynt.Payment.Attributes;
using Zynt.Payment.Interfaces;

namespace Zynt.Payment.Test;

[PaymentHandler(HandlerName)]
public sealed class DisposableSuccessHandler : IPaymentSuccessHandler, IDisposable
{
    public const string HandlerName = "disposable-success";

    private readonly HandlerState _state;

    public DisposableSuccessHandler(HandlerStateCollection states)
    {
        _state = states.Get(GetType());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _state.MarkAsDisposed();
    }

    public Task OnTransactionSuccessAsync(PaymentResult result)
    {
        _state.MarkAsInvokedSuccessHandler(result);
        return Task.CompletedTask;
    }
}