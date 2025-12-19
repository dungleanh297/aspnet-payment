namespace Zynt.Payment.Exceptions;

public class PaymentHandlerNotFoundException : PaymentException
{
    public PaymentHandlerNotFoundException(PaymentRequest? request, PaymentResult? result, string message) : base(request, result, message)
    {
        
    }
}
