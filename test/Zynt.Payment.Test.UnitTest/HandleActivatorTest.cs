using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Zynt.Payment.Attributes;
using Zynt.Payment.Extensions;
using Zynt.Payment.Interfaces;

namespace Zynt.Payment.Test;

[TestClass]
public class HandleActivatorTest
{
    private IPaymentHandleActivator _handleActivator = null!; 
    private HandlerStateCollection _states = null!;
    
    [TestInitialize]
    public void CreateServiceProvider()
    {
        var serviceProvider = Helpers.CreateServiceCollectionWithPayment()
            .AddPayment()
            .AddHandlers(typeof(AssemblyReference).Assembly).Services.BuildServiceProvider();
        
        _handleActivator = serviceProvider.GetRequiredService<IPaymentHandleActivator>();
        _states = serviceProvider.GetRequiredService<HandlerStateCollection>();
    }

    [TestMethod]
    [DataRow(SuccessHandler.HandlerName, false, false)]
    [DataRow(AsyncDisposableSuccessHandler.HandlerName, true, true)]
    [DataRow(DisposableSuccessHandler.HandlerName, true, false)]
    [DataRow(MultipleDisposableSuccessHandler.HandlerName, true, true)]
    public async Task InvokeHandler_SuccessResult(string handlerName, bool isDisposable, bool shouldAsyncDisposable)
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
        Assert.AreEqual(handlerName, GetHandlerNameFromType(handlerState.HandlerType), "Activator invoked wrong handler");
        Assert.IsTrue(handlerState.InvokedSuccessHandler, "Handler name hasn't been invoked");
        Assert.AreEqual(result, handlerState.Result, $"Wrong payload {nameof(PaymentResult)} has been passed to handler");

        if (isDisposable)
        {
            Assert.IsTrue(handlerState.Disposed, "Handler hasn't disposed!");
        }

        if (shouldAsyncDisposable)
        {
            Assert.IsTrue(handlerState.DisposedWithAsyncDisposable, "Handler should be disposed with ASyncDisposable");
        }
    }

    [TestMethod]
    public async Task InvokeHandler_FailureResult()
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

        Assert.AreEqual(0, _states.Count, "No handler should be invoked when result's status is not success!");
    }

    private static string? GetHandlerNameFromType(Type type)
    {
        if (!type.IsAssignableTo(typeof(IPaymentSuccessHandler)))
        {
            return null;
        }

        return type.GetCustomAttribute<PaymentHandlerAttribute>()?.Name;
    }
}