using Microsoft.Extensions.DependencyInjection;
using Zynt.Payment.Interfaces;

namespace Zynt.Payment.Test;

[TestClass]
public sealed class PaymentDependencyInjectionTests
{
    private static ServiceProvider s_globalServiceProvider = null!;

    private IServiceScope _serviceScope = null!;
    private IServiceProvider ServiceProvider => _serviceScope.ServiceProvider;

    [ClassInitialize]
    public static void InitializeServiceProvider(TestContext context)
    {
        s_globalServiceProvider = Helpers.CreateServiceCollectionWithPayment().BuildServiceProvider();
    }

    [TestInitialize]
    public void InitializeServiceScope()
    {
        _serviceScope = s_globalServiceProvider.CreateScope();
    }

    [TestMethod]
    public void PaymentContextAccessorIsRegisterd()
    {
        var paymentContextAccessor = ServiceProvider.GetService<IPaymentContextAccessor>();
        Assert.IsNotNull(paymentContextAccessor, $"{nameof(IPaymentContextAccessor)} hasn't been registered in DI");
    }

    [TestMethod]
    public void PaymentFacadeIsRegistered()
    {
        var paymentProvider = ServiceProvider.GetService<IPaymentFacade>();
        Assert.IsNotNull(paymentProvider, $"{nameof(IPaymentFacade)} hasn't been registered in DI");
    }

    [TestMethod]
    public void PaymentHandleActivatorIsRegistered()
    {
        var paymentHandleActivator = ServiceProvider.GetService<IPaymentHandleActivator>();
        Assert.IsNotNull(paymentHandleActivator, $"{nameof(IPaymentHandleActivator)} hasn't been registered in DI");
    }

    [TestCleanup]
    public void DisposeServiceScope()
    {
        _serviceScope.Dispose();
    }

    [ClassCleanup]
    public static void DisposeServiceProvider()
    {
        s_globalServiceProvider.Dispose();
    }
}