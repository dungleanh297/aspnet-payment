using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Primitives;
using Zynt.Payment.Extensions;

namespace Zynt.Payment.Test;

internal static class Helpers
{
    private static void ConfigurePayment(PaymentOptions options)
    {
        options.Hostname = Constants.Hostname;
        options.APIGateway = Constants.APIGateway;
        options.RedirectUrls = Constants.RedirectUris;
    }

    public static IServiceCollection CreateServiceCollectionWithPayment()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IWebHostEnvironment>(new FakeWebHostEnviroment());
        serviceCollection.AddPayment(ConfigurePayment).AddFakePayment();
        serviceCollection.AddLogging();
        serviceCollection.Add(new ServiceDescriptor(typeof(HandlerState<>), typeof(HandlerState<>), ServiceLifetime.Scoped));
        return serviceCollection;
    }

    public static bool TryGetPaymentResultFromRedirectUrl(string redirectUrl, [NotNullWhen(true)] out PaymentResult? result)
    {
        NameValueCollection queries = HttpUtility.ParseQueryString(redirectUrl);
        if (!long.TryParse(queries[nameof(PaymentResult.Amount)], out var amount)
            || !DateTimeOffset.TryParseExact(queries[nameof(PaymentResult.CreatedDate)], "yyyy-MM-ddThh:mm:ss+zzz", formatProvider: null, DateTimeStyles.None, out DateTimeOffset createdDate)
            || int.TryParse(queries[nameof(PaymentResult.Status)], out var status)
        )
        {
            result = null;
            return false;
        }
        
        result = new ()
        {
            Amount = amount,
            Currency = queries[nameof(PaymentResult.Currency)]!,
            CreatedDate = createdDate,
            Scheme = queries[nameof(PaymentResult.Scheme)]!,
            Identifier = queries[nameof(PaymentResult.Identifier)]!,
            HandlerName = queries[nameof(PaymentResult.HandlerName)]!,
            Status = (PaymentStatus) status,
        };

        return true;
    }

    public static HttpContext CreateHttpContextFromUrl(string uriAsString, IServiceProvider serviceProvider)
    {
        var uri = new Uri(uriAsString);
        
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                QueryString = QueryString.FromUriComponent(uri),
                Query = uri.GetQueryCollection(),
                Host = HostString.FromUriComponent(uri.Host),
                Scheme = uri.Scheme,
                Path = uri.AbsolutePath,
                Method = HttpMethods.Get,
            },
            RequestServices = serviceProvider,
        };
        
        return httpContext;
    }

    private static QueryCollection GetQueryCollection(this Uri uri)
    {
        NameValueCollection queries = HttpUtility.ParseQueryString(uri.Query);
        var queryCollectionStore = new Dictionary<string, StringValues>();
        
        foreach (var key in queries.AllKeys)
        {
            if (key is null)
            {
                continue;
            }
            
            var values = queries.GetValues(key);
            
            if (values is null || values.Length == 0)
            {
                queryCollectionStore.Add(key, StringValues.Empty);
            }
            else if (values.Length == 1)
            {
                queryCollectionStore.Add(key, values[0]);
            }
            else
            {
                queryCollectionStore.Add(key, values);
            }
        }

        return new QueryCollection(queryCollectionStore);
    }
}