namespace Zynt.Payment.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class PaymentRedirectionHandlerAttribute : Attribute
{
    
}