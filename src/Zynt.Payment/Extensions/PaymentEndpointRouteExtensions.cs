using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Zynt.Payment.Infrastructure;
using Zynt.Payment.Registries;

namespace Zynt.Payment.Extensions;

public static class PaymentEndpointRouteExtensions
{
    public static void UsePayment(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<PaymentMiddleware>();

        var registry = builder.ApplicationServices.GetRequiredService<ServiceRegistry>();

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