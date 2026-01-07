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
    private readonly bool _persistContext;
    
    public PaymentMiddleware(ServiceRegistry serviceRegistry, IOptions<PaymentOptions> options)
    {
        _serviceRegistry = serviceRegistry;
        _persistContext = options.Value.PersistPaymentContext;
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
            var contextFactory = context.RequestServices.GetRequiredService<IPaymentContextFactory>();
            
            accessor.Value = contextFactory.Create(context);

            return _persistContext ? InjectWrappedServiceProviderAsync(context, next) : next(context);
        }

        if (endpoint.Metadata.GetMetadata<PaymentRedirectionHandlerAttribute>() is not null)
        {
            string? providerName = context.Request.Query["provider"];

            if (providerName is null || _serviceRegistry.TryGetServiceTypeInfo(providerName, out var serviceTypeInfo))
            {
                return TypedResults.BadRequest().ExecuteAsync(context);
            }

            var paymentService = (IPaymentService) context.RequestServices.GetRequiredService(serviceTypeInfo.Type);

            return SetPaymentResultFromRedirection(context, paymentService, next);
        }

        return next(context);

    }

    private static async Task InjectWrappedServiceProviderAsync(HttpContext context, RequestDelegate next)
    {
        var parentContext = context.RequestServices;
        context.RequestServices = new PersistentContextServiceProvider<PaymentContext>(parentContext);
        
        // Restore the original service provider to avoid side effect when returning to another middleware
        try
        {
            await next(context);
        }
        finally
        {
            if (context.RequestServices is PersistentContextServiceProvider<PaymentContext> wrappedServiceProvider)
            {
                context.RequestServices = wrappedServiceProvider.Unwrap();
            }
        }
    }

    private static Task SetPaymentResultFromRedirection(HttpContext context, IPaymentService paymentService, RequestDelegate next)
    {
        Task<PaymentResult?> resultAsTask = paymentService.GetResultFromRedirectionAsync(ConvertQueriesToDictionary(context.Request.Query));

        if (resultAsTask.IsCompleted)
        {
            PaymentResult? result = resultAsTask.Result;
            context.Features.Set(result);
            return next(context);
        }

        return AwaitResult(resultAsTask, context, next);

        static async Task AwaitResult(Task<PaymentResult?> resultAsTask, HttpContext context, RequestDelegate next)
        {
            var result = await resultAsTask;
            context.Features.Set(result);
            await next(context);
        }
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
