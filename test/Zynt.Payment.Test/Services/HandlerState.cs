namespace Zynt.Payment.Test;

public sealed class HandlerState
{
    private PaymentResult? _result;
    private bool _disposable;
    private bool _asyncDisposable;

    public PaymentResult? Result => _result;

    public bool Disposed => _disposable || _asyncDisposable;
    
    public bool DisposedWithAsyncDisposable => _asyncDisposable;

    public bool InvokedSuccessHandler => _result is not null;

    public Type HandlerType { get; }

    public HandlerState(Type handlerType)
    {
        HandlerType = handlerType;
    }

    public void MarkAsDisposed(bool isAsyncDisposable = false)
    {
        CheckDisposed();
        
        if (isAsyncDisposable)
        {
            _asyncDisposable = true;
        }
        else
        {
            _disposable = true;
        }
        
    }

    public void MarkAsInvokedSuccessHandler(PaymentResult result)
    {
        ArgumentNullException.ThrowIfNull(result, nameof(result));
        
        CheckDisposed();
        
        if (InvokedSuccessHandler)
        {
            throw new InvalidOperationException("Object has been marked for invocation");
        }
        
        _result = result;
    }

    private void CheckDisposed()
    {
        if (Disposed)
        {
            throw new InvalidDataException("Object has been marked for disposition");
        }
    }

    public override bool Equals(object? obj)
    {
        if (object.ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj is HandlerState other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(InvokedSuccessHandler, Disposed, HandlerType);
    }

    public bool Equals(HandlerState? other)
    {
        if (other is null)
        {
            return false;
        }

        if (object.ReferenceEquals(this, other))
        {
            return true;
        }
        
        return InvokedSuccessHandler == other.InvokedSuccessHandler 
               && Disposed == other.Disposed
               && DisposedWithAsyncDisposable == other.DisposedWithAsyncDisposable 
               && HandlerType == other.HandlerType;
    }
}