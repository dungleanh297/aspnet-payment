using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Zynt.Payment.Attributes;
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
        var endpoint = context.GetEndpoint();

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
        bool isValid = await paymentService.ValidateRedirectRequestAsync(context);

        if (!isValid)
        {
            await TypedResults.BadRequest().ExecuteAsync(context);
            return;
        }

        await next(context);
    }
}
