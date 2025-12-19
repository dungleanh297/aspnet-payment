using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using Zynt.Payment.Exceptions;
using Zynt.Payment.Infrastructure;

namespace Zynt.Payment;

internal class PaymentProvider : IPaymentProvider
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<string> CreatePaymentUrlAsync(PaymentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var serviceRegistry = _serviceProvider.GetRequiredService<ServiceRegistry>();
        var options = _serviceProvider.GetRequiredService<IOptions<PaymentOptions>>().Value;

        if (!serviceRegistry.TryGetServiceTypeInfo(request.SchemeName, out var serviceTypeInfo))
        {
            throw new PaymentServiceNotFoundException(request, $"Unable to find payment service with scheme name: {request.SchemeName}");
        }

        if (!serviceTypeInfo.IsCurrencySupported(request.Currency))
        {
            throw new UnsupportedCurrencyException(request, $"{serviceTypeInfo.Type.Name} doesn't support with this currency type: {request.Currency}");
        }

        var paymentService = Unsafe.As<IPaymentService>(_serviceRegistry.GetRequiredService(serviceTypeInfo.Type));

        // NOTE: Payment service use this property to create 
        request.RedirectUri = PaymentUrlHelpers.CreateRedirectUrl(options, request);

        return paymentService.CreatePaymentUrlAsync(request);
    }

    public IEnumerable<PaymentServiceDescriptor> GetAllServices()
    {
        return _serviceRegistry.GetAllServices();
    }
}
