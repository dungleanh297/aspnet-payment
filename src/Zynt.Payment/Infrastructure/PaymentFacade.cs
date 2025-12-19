using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Zynt.Payment.Exceptions;
using Zynt.Payment.Interfaces;
using Zynt.Payment.Primitives;
using Zynt.Payment.Registries;

namespace Zynt.Payment;

internal class PaymentFacade : IPaymentFacade
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceRegistry _serviceRegistry;

    public PaymentFacade(IServiceProvider serviceProvider, ServiceRegistry serviceRegistry)
    {
        _serviceProvider = serviceProvider;
        _serviceRegistry = serviceRegistry;
    }

    public Task<PaymentRequestUrl> CreatePaymentUrlAsync(PaymentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        if (!_serviceRegistry.TryGetServiceTypeInfo(request.SchemeName, out var serviceTypeInfo))
        {
            throw new PaymentServiceNotFoundException(request, $"Unable to find payment service with scheme name: {request.SchemeName}");
        }

        if (!serviceTypeInfo.IsCurrencySupported(request.Currency))
        {
            throw new UnsupportedCurrencyException(request, $"{serviceTypeInfo.Type.Name} doesn't support with this currency type: {request.Currency}");
        }

        var paymentService = Unsafe.As<IPaymentService>(_serviceProvider.GetRequiredService(serviceTypeInfo.Type));

        return paymentService.CreatePaymentUrlAsync(request);

    }

    public IEnumerable<PaymentServiceDescriptor> GetAllServices()
    {
        return _serviceRegistry.GetAllServices();
    }
}
