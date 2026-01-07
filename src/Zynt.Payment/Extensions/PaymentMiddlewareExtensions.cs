using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Zynt.Payment.Infrastructure;
using Zynt.Payment.Registries;

namespace Zynt.Payment.Extensions;

public static class PaymentMiddlewareExtensions
{
    public static void UsePayment(this IApplicationBuilder builder)
    {
        var registry = builder.ApplicationServices.GetService<ServiceRegistry>();

        if (registry is null)
        {
            throw new InvalidOperationException(
                "Unable to find required service for payment middleware. Please add all the required services by calling 'IServiceCollection.AddPayment' in the application startup code");
        }
        
        builder.UseMiddleware<PaymentMiddleware>();

        if (builder is IEndpointRouteBuilder routeBuilder)
        {
            registry.ConfigureWebhook(routeBuilder);
        }
        else
        {
            builder.UseEndpoints(registry.ConfigureWebhook);
        }
    }
}