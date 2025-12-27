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

    public Task<PaymentRequestUrls> CreatePaymentUrlAsync(PaymentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        if (!_serviceRegistry.TryGetServiceTypeInfo(request.ServiceName, out var serviceTypeInfo))
        {
            throw new PaymentServiceNotFoundException(request, $"Unable to find payment service with service name: {request.ServiceName}");
        }

        if (!serviceTypeInfo.IsCurrencySupported(request.Currency))
        {
            throw new UnsupportedCurrencyException(request, $"{serviceTypeInfo.Type.Name} doesn't support with this currency type: {request.Currency}");
        }

        var paymentService = Unsafe.As<IPaymentService>(_serviceProvider.GetRequiredService(serviceTypeInfo.Type));
        var result = paymentService.CreatePaymentUrlAsync(request);

        return result;

    }

    public IEnumerable<PaymentServiceDescriptor> GetAllServices()
    {
        return _serviceRegistry.GetAllServices();
    }
}
