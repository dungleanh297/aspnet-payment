using Microsoft.Extensions.DependencyInjection;

namespace Zynt.Payment.Test;

[TestClass]
public sealed class PaymentServiceCollectionExtensionTests
{
    private static IServiceProvider s_serviceProvider = null!;

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        s_serviceProvider = Helpers.CreateServiceCollectionWithPayment().BuildServiceProvider();
    }
    
    [TestMethod]
    public void DIServicesAreRegistered()
    {
        using var serviceScope = s_serviceProvider.CreateScope();
        var paymentProvider = serviceScope.ServiceProvider.GetService<IPaymentProvider>();
        Assert.IsNotNull(paymentProvider, "PaymentProvider isn't registered");
    }
}