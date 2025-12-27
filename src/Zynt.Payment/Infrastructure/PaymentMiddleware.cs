using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Zynt.Payment.Attributes;
using Zynt.Payment.DependencyInjection;
using Zynt.Payment.Interfaces;
using Zynt.Payment.Models;
using Zynt.Payment.Registries;

namespace Zynt.Payment.Infrastructure;

internal class PaymentMiddleware : IMiddleware
{
    private readonly ServiceRegistry _serviceRegistry;
    private readonly PaymentOptions _paymentOptions;

    public PaymentMiddleware(ServiceRegistry serviceRegistry, IOptions<PaymentOptions> options)
    {
        _serviceRegistry = serviceRegistry;
        _paymentOptions = options.Value;
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
            var accessor = context.RequestServices.GetRequiredService<IPaymentContextAccessor>();
            accessor.Value = CreatePaymentContext(context, _paymentOptions.BaseUrl);

            return _paymentOptions.PersistPaymentContext ? InjectWrappedServiceProviderAsync(context, next) : next(context);
        }

        if (endpoint.Metadata.GetMetadata<PaymentRedirectionHandlerAttribute>() is not null)
        {
            string? providerName = context.Request.Query["provider"];

            if (providerName is null || _serviceRegistry.TryGetServiceTypeInfo(providerName, out var serviceTypeInfo))
            {
                return TypedResults.BadRequest().ExecuteAsync(context);
            }

            var paymentService = (IPaymentService) context.RequestServices.GetRequiredService(serviceTypeInfo.Type);

            return SetPaymentResultFromRedirectionAsync(context, paymentService, next);
        }

        return next(context);

    }

    private static async Task InjectWrappedServiceProviderAsync(HttpContext context, RequestDelegate next)
    {
        var parentContext = context.RequestServices;
        context.RequestServices = new PersistentContextServiceProvider<IPaymentContext>(parentContext);
        
        try
        {
            await next(context);
        }
        finally
        {
            if (context.RequestServices is PersistentContextServiceProvider<IPaymentContext> wrappedServiceProvider)
            {
                context.RequestServices = wrappedServiceProvider.Unwrap();
            }
        }
    }

    private static IPaymentContext CreatePaymentContext(HttpContext httpContext, string? baseUrl)
    {
        return new PaymentContext
        {
            Connection = httpContext.Connection,
            BaseUrl = baseUrl ?? $"{httpContext.Request.Scheme}://${httpContext.Request.Host}",
        };
    }

    private static async Task SetPaymentResultFromRedirectionAsync(HttpContext context, IPaymentService paymentService, RequestDelegate next)
    {
        PaymentResult? result = await paymentService.GetResultFromRedirectionAsync(ConvertQueriesToDictionary(context.Request.Query));

        if (result is not null)
        {
            context.Features.Set(result);
        }

        await next(context);
    }

    private static Dictionary<string, string?> ConvertQueriesToDictionary(IQueryCollection keyValuePairs)
    {
        var result = new Dictionary<string, string?>(keyValuePairs.Count);
        
        foreach (var kv in keyValuePairs)
        {
            result.Add(kv.Key, kv.Value);
        }

        return result;
    }
}
