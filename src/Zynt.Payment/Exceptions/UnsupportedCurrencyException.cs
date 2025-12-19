namespace Zynt.Payment.Exceptions;

public class UnsupportedCurrencyException : PaymentException
{
    public UnsupportedCurrencyException(PaymentRequest request, string message = "The currency is not supported by the payment service") : base(request, null, message)
    {
        
    }
}
