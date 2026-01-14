using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Zynt.Payment.Infrastructure;
using Zynt.Payment.Registries;

namespace Zynt.Payment.Extensions;

public static class PaymentMiddlewareExtensions
{
    public static void UsePayment<T>(this T app) where T : IApplicationBuilder, IEndpointRouteBuilder
    {
        var registry = app.ApplicationServices.GetService<ServiceRegistry>();

        if (registry is null)
        {
            throw new InvalidOperationException(
                "Unable to find required service for payment middleware. Please add all the required services by calling 'IServiceCollection.AddPayment()' in the application startup code");
        }
        
        app.UseMiddleware<PaymentMiddleware>();

        using (IServiceScope scope = app.ApplicationServices.CreateScope())
        {
            var webhookConfigurator = ActivatorUtilities.CreateInstance<WebhookConfigurator>(scope.ServiceProvider);
            webhookConfigurator.Configure(app);
        }
    }
}