using Zynt.Payment.Interfaces;

namespace Zynt.Payment.DependencyInjection;

internal sealed class PaymentContextAccessor : IPaymentContextAccessor
{
    private readonly IContextAccessor<IPaymentContext> _accessor;

    public IPaymentContext? Value
    {
        get => _accessor.Value;
        set => _accessor.Value = value;
    }

    public PaymentContextAccessor(IContextAccessor<IPaymentContext> accessor)
    {
        _accessor = accessor;
    }
}
