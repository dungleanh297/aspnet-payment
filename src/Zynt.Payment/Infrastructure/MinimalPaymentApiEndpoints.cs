using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Zynt.Payment;

internal static class MinimalPaymentApiEndpoints
{
    public static IEnumerable<PaymentServiceDescriptor> GetPaymentServiceDescriptors([FromServices] ServiceRegistry serviceRegistry)
    {
        return serviceRegistry.GetAllServices();
    }

    public static async Task<IResult> HandleRedirectRequest(
        string scheme,
        HttpContext httpContext,
        [FromQuery, BindRequired] string alias,
        [FromServices] IOptions<PaymentOptions> options,
        [FromServices] PaymentServiceProvider paymentServiceProvider
    )
    {
        var optionsValue = options.Value;

        if (!paymentServiceProvider.TryGetPaymentService(scheme, out var paymentService) ||
            !optionsValue.RedirectUrls.TryGetValue(alias, out var redirectUrl))
        {
            return TypedResults.NotFound();
        }

        RedirectResponse response = await paymentService.HandleRedirectRequestAsync(httpContext.Request);

        if (!response.IsSuccess)
        {
            return TypedResults.BadRequest();
        }

        return RedirectHelpers.CreateRedirectResult(redirectUrl, response.Result);
    }

    public static async Task<IResult> HandleCallbackRequest(
        string scheme,
        HttpContext context,
        [FromServices] IServiceProvider serviceProvider,
        [FromServices] PaymentServiceProvider paymentServiceProvider
    )
    {
        if (!paymentServiceProvider.TryGetPaymentService(scheme, out var paymentService))
        {
            return TypedResults.NotFound();
        }

        CallbackResponse response = await paymentService.HandleCallbackRequestAsync(context.Request);

        // Request was invalid (invalid signature, not allowed method, bad request, etc...), so result won't available.
        // Just return the HTTP result without invoking the handler.
        if (!response.IsValid)
        {
            return response.HttpResult;
        }

        PaymentResult paymentResult = response.PaymentResult;

        if (!serviceProvider.GetRequiredService<HandlerInvoker>()
                .TryInvokeHandlerAsync(paymentResult, out var handlerResultAsTask))
        {
            return TypedResults.NotFound();
        }

        var handlerResult = await handlerResultAsTask;

        if (handlerResult == PaymentHandlerResult.Success)
        {
            IPaymentLogger? logger = serviceProvider.GetService<IPaymentLogger>();

            if (logger is not null)
            {
                await logger.LogAsync(paymentResult);
            }
        }

        return paymentService.GetCallbackResponse(handlerResult);
    }
}