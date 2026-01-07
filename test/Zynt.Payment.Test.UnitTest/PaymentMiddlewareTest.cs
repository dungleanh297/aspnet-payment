using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Zynt.Payment.Attributes;
using Zynt.Payment.DependencyInjection;
using Zynt.Payment.Infrastructure;
using Zynt.Payment.Models;
using Zynt.Payment.Registries;

namespace Zynt.Payment.Test.UnitTest;

[TestClass]
public class PaymentMiddlewareTest
{
    private static readonly IPAddress s_fakeRemoteIpAddress = new IPAddress([192, 120, 10, 100]);

    private static readonly string s_fakeHostName = "very-example.com";
    private static readonly string s_protocol = "http";
    private static readonly string s_expectedBaseUrl = $"{s_protocol}://{s_fakeHostName}";

    [TestMethod]
    public void Invoke_WithoutRequirePaymentContext_NothingChanged()
    {
        // Arrange
        var options = new Mock<IOptions<PaymentOptions>>();
        options.Setup(e => e.Value).Returns(new PaymentOptions());

        var mockHttpContext = CreateFakeHttpContext();

        var fakeRequestDelegate = new Mock<RequestDelegate>();
        var delegateInvocationResult = new TaskCompletionSource().Task;

        fakeRequestDelegate.Setup(r => r.Invoke(mockHttpContext.Object)).Returns(delegateInvocationResult);

        var mockMiddleware = new PaymentMiddleware(new ServiceRegistry(), options.Object);

        // Act
        var invocationResult = mockMiddleware.InvokeAsync(mockHttpContext.Object, fakeRequestDelegate.Object);

        // Assert
        fakeRequestDelegate.Verify(e => e.Invoke(It.IsAny<HttpContext>()), Times.Once, "Delegate should be invoked only once");
        Assert.AreEqual(delegateInvocationResult, invocationResult, "Async scope shouldn't be created and invocation result must be the result when invoking context(next)");
    }

    [TestMethod]
    public void Invoke_WithRequirePaymentContext_PaymentContextCreated()
    {
        // Arrange
        var stubOptions = new Mock<IOptions<PaymentOptions>>();
        stubOptions.Setup(e => e.Value).Returns(new PaymentOptions() { PersistPaymentContext = false });

        var fakePaymentContextAccessor = new PaymentContextAccessor(new ContextAccessor<PaymentContext>());

        var fakeHttpContext = CreateFakeHttpContext(new RequirePaymentContextAttribute());

        fakeHttpContext.Setup(h => h.RequestServices.GetService(typeof(IPaymentContextFactory))).Returns(new DefaultPaymentContextFactory(stubOptions.Object));
        fakeHttpContext.Setup(h => h.RequestServices.GetService(typeof(IPaymentContextAccessor))).Returns(fakePaymentContextAccessor);

        var requestDelegateInvocationResult = new TaskCompletionSource().Task;
        var fakeRequestDelegate = new Mock<RequestDelegate>();
        fakeRequestDelegate.Setup(e => e.Invoke(fakeHttpContext.Object)).Returns(requestDelegateInvocationResult);

        var middleware = new PaymentMiddleware(new ServiceRegistry(), stubOptions.Object);

        // Act
        var middlewareInvocationResult = middleware.InvokeAsync(fakeHttpContext.Object, fakeRequestDelegate.Object);

        // Assert
        fakeRequestDelegate.Verify(e => e.Invoke(It.IsAny<HttpContext>()), Times.Once, $"{nameof(RequestDelegate)} should be invoked once");
        Assert.AreEqual(requestDelegateInvocationResult, middlewareInvocationResult, $"Initialize {nameof(PaymentContext)} is creating another async scope, which is not neccessary");
        Assert.IsNotNull(fakePaymentContextAccessor.Value, $"{nameof(PaymentContext)} value hasn't been set");
        Assert.AreEqual(s_fakeRemoteIpAddress, fakePaymentContextAccessor.Value.RemoteIpAddress, $"Incorrect value: {nameof(PaymentContext)}.{nameof(PaymentContext.RemoteIpAddress)}");
        Assert.AreEqual(s_expectedBaseUrl, fakePaymentContextAccessor.Value.BaseUrl, $"Incorrect value: {nameof(PaymentContext)}.{nameof(PaymentContext.RemoteIpAddress)}");
    }

    public void Invoke_WithPersistPaymentContext_ContextPersistedAndCreated()
    {
        // Arrange
        var stubOptions = new Mock<IOptions<PaymentOptions>>();
        stubOptions.Setup(e => e.Value).Returns(new PaymentOptions() { PersistPaymentContext = true });

        var fakePaymentContextAccessor = new PaymentContextAccessor(new ContextAccessor<PaymentContext>());

        var fakeHttpContext = CreateFakeHttpContext(new RequirePaymentContextAttribute());

        fakeHttpContext.Setup(h => h.RequestServices.GetService(typeof(IPaymentContextFactory))).Returns(new DefaultPaymentContextFactory(stubOptions.Object));
        fakeHttpContext.Setup(h => h.RequestServices.GetService(typeof(IPaymentContextAccessor))).Returns(fakePaymentContextAccessor);

        var requestDelegateInvocationResult = new TaskCompletionSource().Task;
        var fakeRequestDelegate = new Mock<RequestDelegate>();
        fakeRequestDelegate.Setup(e => e.Invoke(fakeHttpContext.Object)).Returns(requestDelegateInvocationResult);

        var middleware = new PaymentMiddleware(new ServiceRegistry(), stubOptions.Object);

        // Act
        var middlewareInvocationResult = middleware.InvokeAsync(fakeHttpContext.Object, fakeRequestDelegate.Object);

        // Assert
        fakeRequestDelegate.Verify(e => e.Invoke(It.IsAny<HttpContext>()), Times.Once, $"{nameof(RequestDelegate)} should be invoked once");
        Assert.IsNotNull(fakePaymentContextAccessor.Value, $"{nameof(PaymentContext)} value hasn't been set");
        Assert.AreEqual(s_fakeRemoteIpAddress, fakePaymentContextAccessor.Value.RemoteIpAddress, $"Incorrect value: {nameof(PaymentContext)}.{nameof(PaymentContext.RemoteIpAddress)}");
    }

    internal static Mock<HttpContext> CreateFakeHttpContext(object? metadata = null)
    {
        var metadataCollection = metadata is not null
            ? new EndpointMetadataCollection(metadata)
            : new EndpointMetadataCollection();

        var endpoint = new Endpoint(null, metadataCollection, "TestEndpoint");

        var fakeHttpContext = new Mock<HttpContext>();
        fakeHttpContext.SetupGet(h => h.Features).Returns(new FeatureCollection());
        fakeHttpContext.Setup(h => h.Connection.RemoteIpAddress).Returns(s_fakeRemoteIpAddress);
        fakeHttpContext.SetupProperty(h => h.Request.Scheme, s_protocol);
        fakeHttpContext.SetupProperty(h => h.Request.Host, new HostString(s_fakeHostName));
        fakeHttpContext.Object.SetEndpoint(endpoint);

        return fakeHttpContext;
    }
}
