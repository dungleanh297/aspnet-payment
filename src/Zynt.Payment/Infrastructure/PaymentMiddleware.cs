using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Zynt.Payment.Attributes;
using Zynt.Payment.DependencyInjection;
using Zynt.Payment.Interfaces;
using Zynt.Payment.Registries;

namespace Zynt.Payment.Infrastructure;

internal class PaymentMiddleware : IMiddleware
{
    private readonly ServiceRegistry _serviceRegistry;

    public PaymentMiddleware(ServiceRegistry serviceRegistry)
    {
        _serviceRegistry = serviceRegistry;
    }

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        Endpoint? endpoint = context.GetEndpoint();

        if (endpoint == null)
        {
            return next(context);
        }

        if (endpoint.Metadata.GetMetadata<RequirePaymentContextAttribute>() is not null)
        {
            return InjectWrappedServiceProviderAsync(context, next);
        }

        if (endpoint.Metadata.GetMetadata<PaymentRedirectionHandlerAttribute>() is not null)
        {
            string? providerName = context.Request.Query["provider"];

            if (providerName is null || _serviceRegistry.TryGetServiceTypeInfo(providerName, out var serviceTypeInfo))
            {
                TypedResults.BadRequest().ExecuteAsync(context);
                
                return Task.CompletedTask;
            }

            var paymentService = (IPaymentService) context.RequestServices.GetRequiredService(serviceTypeInfo.Type);

            return InvokeAsyncCore(context, paymentService, next);
        }

        return next(context);

    }

    private static async Task InjectWrappedServiceProviderAsync(HttpContext context, RequestDelegate next)
    {
        var parentContext = context.RequestServices;
        context.RequestServices = new PersistentContextServiceProvider<PaymentContext>(parentContext);

        try
        {
            await next(context);
        }
        finally
        {
            // If somewhere else tamped and forgot to restore the original IServiceProvider, that will probably screw up!
            if (context.RequestServices is PersistentContextServiceProvider<PaymentContext> wrappedServiceProvider)
            {
                context.RequestServices = wrappedServiceProvider.Unwrap();
            }
        }
    }

    public static async Task InvokeAsyncCore(HttpContext context, IPaymentService paymentService, RequestDelegate next)
    {
        PaymentResult? result = await paymentService.GetResultFromRedirectionAsync(context.Request.Query.Select(kv => new KeyValuePair<string, string?>(kv.Key, kv.Value)));

        if (result is not null)
        {
            context.Features.Set(result);
        }
    }
}
