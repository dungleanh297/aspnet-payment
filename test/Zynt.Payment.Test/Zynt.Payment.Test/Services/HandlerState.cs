namespace Zynt.Payment.Test;

public sealed class HandlerState<THandler> : IEquatable<HandlerState<THandler>>
{
    private bool _invokedSuccessHandler;
    private bool _disposed;

    public bool InvokedSuccessHandler
    {
        get => _invokedSuccessHandler;
        init => _invokedSuccessHandler = value;
    }

    public bool Disposed
    {
        get => _disposed;
        init => _disposed = value;
    }

    public void MarkAsDisposed()
    {
        CheckDisposed();
        _disposed = true;
    }

    public void MarkAsInvokedSuccessHandler()
    {
        CheckDisposed();
        
        if (InvokedSuccessHandler)
        {
            Assert.Fail("Handler cannot be invoked more than once");
        }
        
        _invokedSuccessHandler = true;
    }

    private void CheckDisposed()
    {
        if (Disposed)
        {
            Assert.Fail("Handler dispose method cannot be invoked more than once");
        }
    }

    public override bool Equals(object? obj)
    {
        if (object.ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj is HandlerState<THandler> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(InvokedSuccessHandler, Disposed);
    }

    public bool Equals(HandlerState<THandler>? other)
    {
        if (other is null)
        {
            return false;
        }
        
        return InvokedSuccessHandler == other.InvokedSuccessHandler && Disposed == other.Disposed;
    }
}