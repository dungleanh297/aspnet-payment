namespace Zynt.Payment.Test;

public interface IAsyncDisposableTestHandler : ITestHandler, IAsyncDisposable
{
    ValueTask IAsyncDisposable.DisposeAsync()
    {
        State.MarkAsDisposed(true);
        return ValueTask.CompletedTask;
    }
}