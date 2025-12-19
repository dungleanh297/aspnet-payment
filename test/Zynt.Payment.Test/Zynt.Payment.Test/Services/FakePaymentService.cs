using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using static Zynt.Payment.PaymentHandlerResult;

namespace Zynt.Payment.Test;

internal class FakePaymentService : IPaymentService
{
    
    public static readonly string[] SupportedCurrencies = [Constants.VNDCurrencyCode, Constants.USDCurrencyCode];

    public const string Scheme = "fake";
    
    public static readonly PaymentServiceDescriptor Descriptor = new PaymentServiceDescriptor()
    {
        Currencies = SupportedCurrencies,
        IconUrl = "https://git-scm.com/favicon.ico",
        DisplayName = "Fake Payment Service",
        Scheme = Scheme,
    };
    
    public Task<string> CreatePaymentUrlAsync(PaymentRequest request)
    {
        #pragma warning disable MSTEST0032
        Assert.IsNotNull(request.RedirectUri, "Redirect uri isn't initialized when passed to service");
        return Task.FromResult(CreatePaymentUrl(request));
        #pragma warning restore MSTEST0032
    }

    public Task<RedirectResponse> HandleRedirectRequestAsync(HttpRequest request)
    {
        if (request.Query.ContainsKey("valid") && TryGetPaymentResultFromQueries(request.Query, out var result))
        {
            return Task.FromResult(new RedirectResponse(result));
        }
        
        return Task.FromResult(RedirectResponse.Failed);
    }

    public Task<CallbackResponse> HandleCallbackRequestAsync(HttpRequest request)
    {
        if (request.Query.ContainsKey("valid") && TryGetPaymentResultFromQueries(request.Query, out var result))
        {
            return Task.FromResult(new CallbackResponse(result));
        }
        
        return Task.FromResult(new CallbackResponse(TypedResults.BadRequest()));
    }

    public IResult GetCallbackResponse(PaymentHandlerResult handlerResult)
    {
        return handlerResult switch
        {
            Success => TypedResults.NoContent(),
            IncorrectAmount => TypedResults.BadRequest(),
            NotFound or NotSupported => TypedResults.NotFound(),
            HasBeenProceeded => TypedResults.Conflict(),
            _ => TypedResults.InternalServerError(),
        };
    }
    
    private static bool TryGetPaymentResultFromQueries(IQueryCollection query, [NotNullWhen(true)] out PaymentResult? result)
    {
        if (!long.TryParse(query["amount"][0], out var amount))
        {
            result = null;
            return false;
        }

        if (!DateTimeOffset.TryParseExact(query["createdDate"][0], "yyyy-MM-dd\Thh:mm null, DateTimeStyles.None,  out var createdDate)
        
        result = new PaymentResult
        {
            Amount = amount,
            CreatedDate = query
            Currency = query["currency"][0] ?? string.Empty,
            HandlerName = query["handler"][0] ?? string.Empty,
            Identifier = query["identifier"][0] ?? string.Empty,
            Scheme = Scheme,
        };
        return true;
    }
    
    public static string CreatePaymentUrl(PaymentRequest request)
    {
        return $"https://payment.example.com/pay?amount={request.Amount}&currency={request.Currency}&handler={request.HandlerName}&identifier={WebUtility.UrlEncode(request.Identifier)}&redirectTo={WebUtility.UrlEncode(request.RedirectUri)}";
    }
}

internal static class FakePaymentServiceExtensions
{
    public static IPaymentBuilder AddFakePayment(this IPaymentBuilder builder)
    {
        return builder.AddPaymentService<FakePaymentService>(FakePaymentService.Descriptor);
    }
}

public class FakePaymentServiceUrlCreator : IUrlCreator<FakePaymentService>
{
    private readonly PaymentOptions _options;

    public FakePaymentServiceUrlCreator(IOptions<PaymentOptions> options)
    {
        _options = options.Value;
    }
    
    public string CreateRedirectUrl(string redirectAlias, PaymentResult paymentResult, bool isValid)
    {
        return $"{_options.Hostname}{Constants.APIGateway}/redirect/{FirstFakeHandler.HandlerName}?alias={redirectAlias}&{CreatePaymentResultQueryParameters(paymentResult, isValid)}";
    }

    public string CreateCallbackUrl(PaymentResult paymentResult, bool isValid)
    {
        return $"{_options.Hostname}{Constants.APIGateway}/callback/{FirstFakeHandler.HandlerName}?{CreatePaymentResultQueryParameters(paymentResult, isValid)}";
    }

    private static string CreatePaymentResultQueryParameters(PaymentResult paymentResult, bool isValid)
    {
        return
            "amount=" + paymentResult.Amount
                      + "&createdDate="
                      + paymentResult.CreatedDate.UtcDateTime.ToString(@"yyyy-MM-dd\Thh\%\3\Amm\%\3\Ass\%\2\B00\%\3\A00")
                      + "&currency=" + paymentResult.Currency
                      + "&handler=" + paymentResult.HandlerName
                      + "&identifier=" + WebUtility.UrlEncode(paymentResult.HandlerName)
                      + "&scheme=" + FakePaymentService.Scheme
                      + "&status=" + (int) paymentResult.Status
                      + (isValid ? "&valid" : string.Empty);
    }
}