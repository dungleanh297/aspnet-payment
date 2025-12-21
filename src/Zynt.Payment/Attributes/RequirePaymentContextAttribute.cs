/// <summary>
/// Tell the payment middleware to initialize the <see cref="PaymentContext" /> to the <see cref="PaymentContextAccessor" />
/// The context allows the payment service gather additional information about the payment request without using <see cref="IHttpContextAccessor"/>    
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePaymentContextAttribute : Attribute
{
    
}