namespace Zynt.Payment.Exceptions;

public class PaymentServiceNotFoundException : PaymentException
{
    public PaymentServiceNotFoundException(PaymentRequest request, string message) : base(request, null, message)
    {
        
    }
}
