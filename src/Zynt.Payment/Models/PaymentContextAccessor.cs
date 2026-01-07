using Zynt.Payment.Interfaces;
using Zynt.Payment.Models;

namespace Zynt.Payment.DependencyInjection;

internal sealed class PaymentContextAccessor : IPaymentContextAccessor
{
    private readonly IContextAccessor<PaymentContext> _accessor;

    public PaymentContext? Value
    {
        get => _accessor.Value;
        set => _accessor.Value = value;
    }

    public PaymentContextAccessor(IContextAccessor<PaymentContext> accessor)
    {
        _accessor = accessor;
    }
}
