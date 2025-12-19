using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Zynt.Payment;

internal class PaymentServiceProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceRegistry _serviceRegistry;

    public PaymentServiceProvider(IServiceProvider serviceProvider, ServiceRegistry serviceRegistry)
    {
        _serviceProvider = serviceProvider;
        _serviceRegistry = serviceRegistry;
    }

    public bool TryGetPaymentService(string scheme, [NotNullWhen(true)] out IPaymentService? paymentService)
    {
        if (!_serviceRegistry.TryGetServiceTypeInfo(scheme, out var serviceTypeInfo))
        {
            paymentService = null;
            return false;
        }

        paymentService = Unsafe.As<IPaymentService>(_serviceProvider.GetRequiredService(serviceTypeInfo.Type));
        return true;
    }
}
