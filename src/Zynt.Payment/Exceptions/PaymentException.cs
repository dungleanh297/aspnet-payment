namespace Zynt.Payment.Exceptions;

public abstract class PaymentException : Exception
{
    public PaymentRequest? Request { get; }

    public PaymentResult? Result { get; }

    public PaymentException(PaymentRequest? request, PaymentResult? result, string message) : base(message)
    {
        Request = request;
        Result = result;
    }
}
