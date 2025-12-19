using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Zynt.Payment.Test;

[TestClass]
public sealed class MinimalApiEndpointTest
{
    private static IServiceProvider s_serviceProvider = null!;

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        s_serviceProvider = Helpers.CreateServiceCollectionWithPayment().BuildServiceProvider();
    }

    [TestMethod]
    [DataRow(
        FakePaymentService.Scheme, 
        FirstFakeHandler.HandlerName, 
        Constants.VNDCurrencyCode, 
        Constants.DefaultRedirectAlias,
        typeof(FakePaymentServiceUrlCreator)
    )]
    public void HandleRedirectAsync_Valid(
        string schemeName, 
        string expectedHandlerName, 
        string expectedCurrency,
        string redirectAlias, 
        Type urlCreatorType
    )
    {
        using var serviceScope = s_serviceProvider.CreateScope();
        var options = serviceScope.ServiceProvider.GetRequiredService<IOptions<PaymentOptions>>();
        var paymentServiceProvider = serviceScope.ServiceProvider.GetRequiredService<PaymentServiceProvider>();
        var urlCreator = (IUrlCreator) ActivatorUtilities.CreateInstance(serviceScope.ServiceProvider, urlCreatorType);
        int expectedAmount = Random.Shared.Next(100, 300) * 100;
        string expectedIdentifier = Guid.NewGuid().ToString();

        var httpContext = Helpers.CreateHttpContextFromUrl(urlCreator.CreateRedirectUrl(), serviceScope.ServiceProvider);
        
        IResult httpResult = MinimalPaymentApiEndpoints.HandleRedirectRequest(schemeName, httpContext, redirectAlias, options, paymentServiceProvider).Result;
        httpResult.ExecuteAsync(httpContext).Wait();
        
        Assert.AreEqual(StatusCodes.Status302Found, httpContext.Response.StatusCode);
        
        string? redirectLocation = httpContext.Response.Headers.Location[0];
        Assert.IsNotNull(redirectLocation);
        string actualRedirectUrl = redirectLocation[..redirectLocation.LastIndexOf("?", StringComparison.CurrentCulture)];
        Assert.AreEqual(expectedRedirectUrl, actualRedirectUrl, "Incorrect redirect url");
        var hasResult = Helpers.TryGetPaymentResultFromRedirectUrl(redirectLocation, out var paymentResult);
        Assert.IsTrue(hasResult, "Payment result wasn't created");
        Assert.IsNotNull(paymentResult, "Payment result was null");
        Assert.AreEqual(expectedAmount, paymentResult.Amount, "Incorrect amount");
        Assert.AreEqual(expectedIdentifier, paymentResult.Identifier, "Incorrect identifier");
        Assert.AreEqual(expectedHandlerName, paymentResult.HandlerName, "Incorrect handler name");
    }
}