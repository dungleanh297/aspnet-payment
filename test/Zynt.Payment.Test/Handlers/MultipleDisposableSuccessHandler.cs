using Zynt.Payment.Attributes;
using Zynt.Payment.Interfaces;

namespace Zynt.Payment.Test;

[PaymentHandler(HandlerName)]
public sealed class MultipleDisposableSuccessHandler : TestHandler, IPaymentSuccessTestHandler, IDisposableTestHandler, IAsyncDisposableTestHandler
{
    public const string HandlerName = "multiple-disposable-success";
    
    public MultipleDisposableSuccessHandler(HandlerStateCollection states) : base(states) { }
}
