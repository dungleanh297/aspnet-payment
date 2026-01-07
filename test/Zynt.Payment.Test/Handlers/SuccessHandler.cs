using Zynt.Payment.Attributes;
using Zynt.Payment.Interfaces;

namespace Zynt.Payment.Test;

[PaymentHandler(HandlerName)]
public sealed class SuccessHandler : TestHandler, IPaymentSuccessTestHandler
{
    public const string HandlerName = "success";

    public SuccessHandler(HandlerStateCollection states) : base(states) { }
}