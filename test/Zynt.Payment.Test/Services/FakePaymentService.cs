using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using System.Net;
using Zynt.Payment.Interfaces;
using Zynt.Payment.Models;
using Zynt.Payment.Primitives;
using Zynt.Payment.Test.Common;

namespace Zynt.Payment.Test;

[Obsolete]
public class FakePaymentService : IPaymentService
{
    public static readonly string[] SupportedCurrencies = [Constants.VNDCurrencyCode, Constants.USDCurrencyCode];

    public const string ServiceName = "fake-service";
    
    public static readonly PaymentServiceDescriptor Descriptor = new PaymentServiceDescriptor()
    {
        Currencies = SupportedCurrencies,
        IconUrl = "https://git-scm.com/favicon.ico",
        DisplayName = "Fake Payment Service",
        Name = ServiceName,
    };
    
    public Task<PaymentRequestUrls> CreatePaymentUrlAsync(PaymentRequest request, PaymentContext context)
    {
        ThrowHelpers.ThrowIfRedirectUrlNotInitialized(request);
        return Task.FromResult(CreatePaymentUrl(request));
    }

    public Task<PaymentResult?> GetResultFromRedirectionAsync(Dictionary<string, string?> queryParameters)
    {
        return Task.FromResult<PaymentResult?>(CreatePaymentResultFromQueryParameters(queryParameters));
    }

    internal static PaymentResult CreatePaymentResultFromQueryParameters(IReadOnlyDictionary<string, string?> queryParameters)
    {
        int.TryParse(queryParameters["amount"], out var amount);
        DateTimeOffset.TryParse(queryParameters["createdTime"], out var createdTime);

        return new ()
        {
            Amount = amount,
            HandlerName = queryParameters["handlerName"]!,
            Currency = queryParameters["currency"]!,
            CreatedDate = createdTime,
            Identifier = queryParameters["identifier"]!,
            ServiceName = ServiceName,
        };
    } 
    
    private static PaymentRequestUrls CreatePaymentUrl(PaymentRequest request)
    {
        return new PaymentRequestUrls()
        {
            HttpUrl =
                $"https://fakepayment.com/pay?amount={request.Amount}&currency={request.Currency}&handler={request.HandlerName}&identifier={WebUtility.UrlEncode(request.Identifier)}&redirectTo={WebUtility.UrlEncode(request.RedirectUrl)}",
            Deeplink = $"fakepayment://pay?amount={request.Amount}&currency={request.Currency}&handler={request.HandlerName}&identifier={WebUtility.UrlEncode(request.Identifier)}&redirectTo={WebUtility.UrlEncode(request.RedirectUrl)}",
        };
    }

    public void ConfigureWebhook(IEndpointRouteBuilder routeBuilder)
    {
        throw new NotImplementedException();
    }
}

internal static class PaymentCallbackEndpoints
{
    public static async Task<Results<BadRequest, Ok>> HandleCallback(IPaymentHandleActivator handlerActivator, FakePaymentPayload payload)
    {
        if (!payload.IsValid)
        {
            return TypedResults.BadRequest();
        }

        await handlerActivator.InvokeAsync(new PaymentResult
        {
            Amount = payload.Amount,
            CreatedDate = payload.CreatedDate,
            Currency = payload.Currency,
            HandlerName = payload.HandlerName,
            Identifier = payload.Identifier,
            ServiceName = FakePaymentService.ServiceName,
            Status = PaymentStatus.Success,
        });

        return TypedResults.Ok();
    }
}
