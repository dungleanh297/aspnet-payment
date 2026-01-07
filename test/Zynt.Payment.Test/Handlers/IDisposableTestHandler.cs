namespace Zynt.Payment.Test;

public interface IDisposableTestHandler : ITestHandler, IDisposable
{
    void IDisposable.Dispose()
    {
        State.MarkAsDisposed();
    }
}