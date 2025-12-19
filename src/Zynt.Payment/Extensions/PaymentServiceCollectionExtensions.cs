using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Zynt.Payment.DependencyInjection;
using Zynt.Payment.Infrastructure;
using Zynt.Payment.Registries;

namespace Zynt.Payment.Extensions;

public static class PaymentDependencyInjectionExtensions
{
    public static IPaymentBuilder AddPayment(this IServiceCollection services, Action<PaymentOptions>? configure = null)
    {
        services.AddOptions<PaymentOptions>().ValidateOnStart();
        
        if (configure is not null)
        {
            services.Configure(configure);
        }
        
        services.AddSingleton<IValidateOptions<PaymentOptions>, PaymentOptionsValidator>();
        
        services.AddScoped<HandlerInvoker>();
        services.AddScoped<IPaymentProvider, PaymentProvider>();
        services.AddScoped<PaymentServiceProvider>();
        services.AddSingleton<PaymentMiddleware>();

        var serviceRegistry = GetServiceFromCollection<ServiceRegistry>(services);
        var handlerRegistry = GetServiceFromCollection<HandlerRegistry>(services);

        if (serviceRegistry is null)
        {
            serviceRegistry = new ServiceRegistry();
            services.AddSingleton(serviceRegistry);
        }

        if (handlerRegistry is null)
        {
            handlerRegistry = new HandlerRegistry();
            services.AddSingleton(handlerRegistry);
        }

        return new PaymentBuilder(services, serviceRegistry, handlerRegistry);
    }

    private static TService? GetServiceFromCollection<TService>(IServiceCollection services)
    {
        return (TService?) services
            .FirstOrDefault(e => e.ImplementationType == typeof(TService))
            ?.ImplementationInstance;
    }
}
