using Microsoft.Extensions.DependencyInjection;
using Zynt.Payment.Extensions;
using Zynt.Payment.Interfaces;
using Zynt.Payment.Test.UnitTest;

namespace Zynt.Payment.Test;

[TestClass]
public class HandleActivatorTest
{
    private ServiceProvider _serviceProvider = null!;
    private IServiceScope _serviceScope = null!;
    private IPaymentHandleActivator _handleActivator = null!; 
    private HandlerStateCollection _states = null!;
    public IServiceProvider ServiceProvider => _serviceScope.ServiceProvider;

    [TestInitialize]
    public void CreateServiceProvider()
    {
        var serviceCollection = Helpers.CreateServiceCollectionWithPayment()
            .AddPayment()
            .AddHandlers(typeof(AssemblyReference).Assembly).Services;

        _serviceProvider = serviceCollection.BuildServiceProvider();
        _serviceScope = _serviceProvider.CreateScope();
        _handleActivator = ServiceProvider.GetRequiredService<IPaymentHandleActivator>();
        _states = ServiceProvider.GetRequiredService<HandlerStateCollection>();
    }

    [TestMethod]
    [DataRow(SuccessHandler.HandlerName, false)]
    [DataRow(AsyncDisposableSuccessHandler.HandlerName, true)]
    [DataRow(DisposableSuccessHandler.HandlerName, true)]
    [DataRow(MultipleDisposableSuccessHandler.HandlerName, true)]
    public async Task InvokeHandler_SuccessResult(string handlerName, bool isDisposable)
    { 
        var result = new PaymentResult
        {
            Amount = 12000,
            Currency = "VND",
            CreatedDate = DateTime.Now,
            HandlerName = handlerName,
            Identifier = "120000",
            ServiceName = "test-service",
            Status = PaymentStatus.Success,
        };

        await _handleActivator.InvokeAsync(result);

        var handlerState = _states.Single().Value;

        Assert.AreEqual(1, _states.Count, "Two or more handler has been created at the same time");
        Assert.AreEqual(handlerName, HandlerUtilities.GetHandlerNameFromType(handlerState.HandlerType), "Activator invoked wrong handler");
        Assert.IsTrue(handlerState.InvokedSuccessHandler, "Handler name hasn't been invoked");
        Assert.AreEqual(result, handlerState.Result, $"Wrong payload {nameof(PaymentResult)} has been passed to handler");

        if (isDisposable)
        {
            Assert.IsTrue(handlerState.Disposed, "Handler hasn't disposed!");
        }
    }

    [TestMethod]
    public async Task InvokeHandler_FailureResult()
    {
        try
        {
            foreach (var status in Enum.GetValues<PaymentStatus>().Where(e => e != PaymentStatus.Success).ToArray())
            {
                await _handleActivator.InvokeAsync(new PaymentResult
                {
                    Amount = 12000,
                    Currency = "VND",
                    CreatedDate = DateTime.Now,
                    HandlerName = SuccessHandler.HandlerName,
                    Identifier = "120000",
                    ServiceName = "test-service",
                    Status = status,
                });
            }

            if (_states.Count != 0)
            {
                Assert.Fail("No handler should be invoked");
            }

        } catch (Exception)
        {
            
        }
    }

    [TestCleanup]
    public void DisposeServiceProvider()
    {
        _serviceProvider.Dispose();
        _serviceScope.Dispose();
    }
}
