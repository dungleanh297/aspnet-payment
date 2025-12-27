using Zynt.Payment.Attributes;
using Zynt.Payment.Interfaces;

namespace Zynt.Payment.Test;

[PaymentHandler(HandlerName)]
public sealed class MultipleDisposableSuccessHandler : IPaymentSuccessHandler, IDisposable, IAsyncDisposable
{
    public const string HandlerName = "multiple-disposable-success";

    private readonly HandlerState _state;

    public MultipleDisposableSuccessHandler(HandlerStateCollection states)
    {
        _state = states.Get(GetType());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _state.MarkAsDisposed();
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
