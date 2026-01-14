using System.Net;
using Microsoft.AspNetCore.Routing;
using Zynt.Payment.Interfaces;
using Zynt.Payment.Models;
using Zynt.Payment.Primitives;
using Zynt.Payment.Test.Common;

namespace Zynt.Payment.Test;

[Obsolete]
internal class AnotherFakePaymentService : IPaymentService
{
    public static readonly string[] SupportedCurrencies = [Constants.USDCurrencyCode, Constants.EURCurrencyCode];

    public const string ServiceName = "another-fake-service";

    public static readonly PaymentServiceDescriptor Descriptor = new PaymentServiceDescriptor()
    {
        Currencies = SupportedCurrencies,
        IconUrl = "https://github.com/favicon.ico",
        DisplayName = "Another Fake Payment Service",
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

        return new()
        {
            Amount = amount,
            HandlerName = queryParameters["handlerName"]!,
            Currency = queryParameters["currency"]!,
            CreatedDate = createdTime,
            Identifier = queryParameters["identifier"]!,
            ServiceName = AnotherFakePaymentService.ServiceName,
        };
    }

    private static PaymentRequestUrls CreatePaymentUrl(PaymentRequest request)
    {
        return new PaymentRequestUrls()
        {
            HttpUrl =
                $"https://anotherfakepayment.io/checkout?amount={request.Amount}&currency={request.Currency}&handler={request.HandlerName}&identifier={WebUtility.UrlEncode(request.Identifier)}&return={WebUtility.UrlEncode(request.RedirectUrl)}",
            Deeplink = $"anotherfakepayment://checkout?amount={request.Amount}&currency={request.Currency}&handler={request.HandlerName}&identifier={WebUtility.UrlEncode(request.Identifier)}&return={WebUtility.UrlEncode(request.RedirectUrl)}",
        };

    }

    public void ConfigureWebhook(IEndpointRouteBuilder routeBuilder)
    {
        throw new NotImplementedException();
    }
}

public class AnotherFakePaymentServiceExtensions
{
    
}