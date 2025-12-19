using Microsoft.Extensions.DependencyInjection;

namespace Zynt.Payment.Test;

[TestClass]
public sealed class PaymentProviderTest
{
    private static IServiceScope s_serviceScope = null!;
    private static IPaymentProvider s_paymentProvider = null!;

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        var services = Helpers.CreateServiceCollectionWithPayment();
        s_serviceScope = services.BuildServiceProvider().CreateScope();
        s_paymentProvider = s_serviceScope.ServiceProvider.GetRequiredService<IPaymentProvider>();
    }

    [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
    public static void Cleanup()
    {
        s_serviceScope.Dispose();
    }

    [TestMethod]
    public void CreatePaymentUrl()
    {
        var request1 = CreatePaymentRequest();
        var request2 = CreatePaymentRequest();
        
        // This is how payment service will get the redirect url from RedirectUrl property.
        // https://sample-page.com/api/payment/redirect/fake?alias=Default
        const string ExpectedRedirectUrl = $"{Constants.Hostname}{Constants.APIGateway}/redirect/{FakePaymentService.Scheme}?alias={PaymentRequest.DefaultRedirectAlias}";
        
        // Internal mechanism should set this value when request give to payment service.
        request2.RedirectUri = ExpectedRedirectUrl;
        
        bool created = s_paymentProvider.TryCreatePaymentUrlAsync(request1, out var paymentUrlAsTask);
        
        Assert.AreEqual(ExpectedRedirectUrl, request1.RedirectUri, "Initialized redirect url isn't correct");
        Assert.IsTrue(created, "Payment url was not created");
        Assert.IsNotNull(paymentUrlAsTask, "Payment url task returns null");
        
        var expectedResult = FakePaymentService.CreatePaymentUrl(request2);
        var actualResult = paymentUrlAsTask.Result;
        Assert.AreEqual(expectedResult, actualResult, "Actual redirect url isn't correct with excepted redirect url");
        
        return;

        static PaymentRequest CreatePaymentRequest()
        {
            return new PaymentRequest
            {
                Amount = 10000,
                Currency = "VND",
                HandlerName = FirstFakeHandler.HandlerName,
                Identifier = "1",
                SchemeName = FakePaymentService.Scheme,
                // Using default redirect alias
            };
        }
    }
}