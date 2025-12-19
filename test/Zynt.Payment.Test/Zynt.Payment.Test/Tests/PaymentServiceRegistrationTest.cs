using Microsoft.Extensions.DependencyInjection;

namespace Zynt.Payment.Test;

[TestClass]
public sealed class PaymentServiceRegistrationTest
{
    private static ServiceRegistry s_paymentSvcRegistry = null!;
    
    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        s_paymentSvcRegistry = Helpers
            .CreateServiceCollectionWithPayment()
            .BuildServiceProvider()
            .GetRequiredService<ServiceRegistry>();
    }
    
    [TestMethod]
    public void PaymentServicesAreRegistered()
    {
        Assert.IsTrue(s_paymentSvcRegistry.TryGetServiceTypeInfo(FakePaymentService.Scheme, out var typeInfo), $"{nameof(ServiceRegistry)} cannot found service that added to the builder");
        Assert.AreEqual(typeInfo.Type, typeof(FakePaymentService), $"{nameof(ServiceTypeInfo)} has wrong type of registered service type");
        Assert.IsTrue(FakePaymentService.SupportedCurrencies.SequenceEqual(typeInfo.Currencies ?? Enumerable.Empty<string>()), $"{nameof(ServiceTypeInfo)} has wrong currencies");
    }

    [TestMethod]
    public void RequestPaymentServiceThatDoesntExist()
    {
        const string PaymentSchemeThatDoesntExist = "something";
        Assert.IsFalse(s_paymentSvcRegistry.TryGetServiceTypeInfo(PaymentSchemeThatDoesntExist, out _));
    }
}