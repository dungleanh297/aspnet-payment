using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Zynt.Payment.Interfaces;
using Zynt.Payment.Registries;

internal class WebhookConfigurator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceRegistry _serviceRegistry;

    public WebhookConfigurator(IServiceProvider serviceProvider, ServiceRegistry serviceRegistry)
    {
        _serviceProvider = serviceProvider;
        _serviceRegistry = serviceRegistry;
    }

    public void Configure(IEndpointRouteBuilder routeBuilder)
    {
        foreach (var serviceTypeInfo in _serviceRegistry.ServicesTypes.Values)
        {
            var service = (IPaymentService) _serviceProvider.GetRequiredService(serviceTypeInfo.Type);
            service.ConfigureWebhook(routeBuilder);
        }
    }
}