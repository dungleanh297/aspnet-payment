using Zynt.Payment.Attributes;
using Zynt.Payment.Interfaces;

namespace Zynt.Payment.Test;

[PaymentHandler(HandlerName)]
public sealed class DisposableSuccessHandler : TestHandler, IPaymentSuccessTestHandler, IDisposableTestHandler
{
    public const string HandlerName = "disposable-success";

    public DisposableSuccessHandler(HandlerStateCollection states) : base(states) { }
    
}