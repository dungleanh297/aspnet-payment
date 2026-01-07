using Zynt.Payment.Attributes;

namespace Zynt.Payment.Test;

[PaymentHandler(HandlerName)]
public sealed class AsyncDisposableSuccessHandler : TestHandler, IPaymentSuccessTestHandler, IAsyncDisposableTestHandler
{
    public const string HandlerName = "async-disposable-success";

    public AsyncDisposableSuccessHandler(HandlerStateCollection states) : base(states) { }
}