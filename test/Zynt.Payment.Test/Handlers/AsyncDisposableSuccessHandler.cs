using Zynt.Payment.Attributes;
using Zynt.Payment.Interfaces;

namespace Zynt.Payment.Test;

[PaymentHandler(HandlerName)]
public sealed class AsyncDisposableSuccessHandler : IPaymentSuccessHandler, IAsyncDisposable
{
    public const string HandlerName = "async-disposable-success";

    private readonly HandlerState _state;

    public AsyncDisposableSuccessHandler(HandlerStateCollection states)
    {
        _state = states.Get(GetType());
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        _state.MarkAsDisposed();
        return ValueTask.CompletedTask;
    }

    public Task OnTransactionSuccessAsync(PaymentResult result)
    {
        _state.MarkAsInvokedSuccessHandler(result);
        return Task.CompletedTask;
    }
}