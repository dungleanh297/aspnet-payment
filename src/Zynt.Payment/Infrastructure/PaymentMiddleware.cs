using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Zynt.Payment.Attributes;
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

        if (endpoint == null || endpoint.Metadata.GetMetadata<PaymentRedirectionHandlerAttribute>() is null)
        {
            return next(context);
        }

        string? providerName = context.Request.Query["provider"];

        if (providerName is null || _serviceRegistry.TryGetServiceTypeInfo(providerName, out var serviceTypeInfo))
        {
            TypedResults.BadRequest().ExecuteAsync(context);
            
            return Task.CompletedTask;
        }

        var paymentService = (IPaymentService) context.RequestServices.GetRequiredService(serviceTypeInfo.Type);

        return InvokeAsyncCore(context, paymentService, next);
    }

    public static async Task InvokeAsyncCore(HttpContext context, IPaymentService paymentService, RequestDelegate next)
    {
        PaymentResult? result = await paymentService.GetResultFromRedirectionAsync(context.Request.Query.Select(kv => new KeyValuePair<string, string?>(kv.Key, kv.Value)));

        if (result is not null)
        {
            context.Features.Set(result);
        }

        await next(context);
    }
}
