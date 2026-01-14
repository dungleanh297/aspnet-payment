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
using Zynt.Payment.Interfaces;
using Zynt.Payment.Models;
using Zynt.Payment.Registries;

namespace Zynt.Payment.Test.UnitTest;

[TestClass]
public class PaymentMiddlewareTest
{
    private const string FakePaymentServiceName = "moq-payment";
    private static readonly IPAddress s_expectedRemoteIpAddress = new IPAddress([192, 120, 10, 100]);

    private static readonly string s_fakeHostName = "very-example.com";
    private static readonly string s_protocol = "http";
    private static readonly string s_expectedBaseUrl = $"{s_protocol}://{s_fakeHostName}";
    private static readonly PaymentServiceDescriptor s_stubPaymentDescriptor = new PaymentServiceDescriptor
    {
        Currencies = ["VND"],
        DisplayName = FakePaymentServiceName,
        IconUrl = string.Empty,
        Name = FakePaymentServiceName
    };


    [TestMethod]
    public async Task Invoke_WithNullEndpoint_InvokesNext()
    {
        // Arrange
        var stubOptions = new Mock<IOptions<PaymentOptions>>();
        stubOptions.Setup(e => e.Value).Returns(new PaymentOptions());

        // No endpoint set on context
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.SetupGet(p => p.Features).Returns(new FeatureCollection());
        mockHttpContext.Setup(p => p.RequestServices).Returns(new Mock<IServiceProvider>().Object);

        var mockRequestDelegate = new Mock<RequestDelegate>();
        mockRequestDelegate
            .Setup(r => r.Invoke(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        var middleware = new PaymentMiddleware(new ServiceRegistry(), stubOptions.Object);

        // Act
        await middleware.InvokeAsync(mockHttpContext.Object, mockRequestDelegate.Object);

        // Assert
        AssertRequestDelegateWithHttpContext(mockRequestDelegate, mockHttpContext.Object);
    }

    [TestMethod]
    public async Task Invoke_WithoutSpecificedMetadata_InvokesNext()
    {
        // Arrange
        var stubOptions = new Mock<IOptions<PaymentOptions>>();
        stubOptions.Setup(e => e.Value).Returns(new PaymentOptions());

        var mockHttpContext = CreateFakeHttpContext();
        var mockRequestDelegate = new Mock<RequestDelegate>();
        mockRequestDelegate
            .Setup(r => r.Invoke(mockHttpContext.Object))
            .Returns(Task.CompletedTask);

        var mockMiddleware = new PaymentMiddleware(new ServiceRegistry(), stubOptions.Object);

        // Act
        await mockMiddleware.InvokeAsync(mockHttpContext.Object, mockRequestDelegate.Object);

        // Assert
        AssertRequestDelegateWithHttpContext(mockRequestDelegate, mockHttpContext.Object);
    }

    [TestMethod]
    public async Task Invoke_WithRequirePaymentContext_PaymentContextCreated()
    {
        // Arrange
        var stubOptions = new Mock<IOptions<PaymentOptions>>();
        stubOptions.Setup(e => e.Value).Returns(new PaymentOptions() { PersistPaymentContext = false });

        var mockPaymentContextAccessor = new PaymentContextAccessor(new ContextAccessor<PaymentContext>());

        var mockHttpContext = CreateFakeHttpContext(new RequirePaymentContextAttribute());

        mockHttpContext
            .Setup(h => h.RequestServices.GetService(typeof(IPaymentContextFactory)))
            .Returns(new DefaultPaymentContextFactory(stubOptions.Object));

        mockHttpContext
            .Setup(h => h.RequestServices.GetService(typeof(IPaymentContextAccessor)))
            .Returns(mockPaymentContextAccessor);

        var mockRequestDelegate = new Mock<RequestDelegate>();

        var middleware = new PaymentMiddleware(new ServiceRegistry(), stubOptions.Object);

        // Act
        await middleware.InvokeAsync(mockHttpContext.Object, mockRequestDelegate.Object);

        // Assert
        AssertRequestDelegateWithHttpContext(mockRequestDelegate, mockHttpContext.Object);
        AssertPaymentContext(mockPaymentContextAccessor.Value);
    }

    [TestMethod]
    public async Task Invoke_WithRequirePaymentContextAndPersistent_ContextPersistedAndCreated()
    {
        // Arrange
        var stubOptions = new Mock<IOptions<PaymentOptions>>();
        stubOptions
            .Setup(e => e.Value)
            .Returns(new PaymentOptions() { PersistPaymentContext = true });

        // Use a real ServiceCollection to handle scopes correctly
        var services = new ServiceCollection();
        services.AddScoped(typeof(IContextAccessor<>), typeof(ContextAccessor<>));
        services.AddScoped<IPaymentContextAccessor, PaymentContextAccessor>();
        services.AddSingleton<IPaymentContextFactory>(new DefaultPaymentContextFactory(stubOptions.Object));
        services.AddSingleton(stubOptions.Object);

        var serviceProvider = services.BuildServiceProvider();

        // Capture the accessor from the current scope
        var mainAccessor = serviceProvider.GetRequiredService<IPaymentContextAccessor>();

        PaymentContext? capturedInnerContext = null;
        IServiceProvider? capturedServiceProvider = null;

        // Create the HttpContext with the custom service provider
        var mockHttpContext = CreateFakeHttpContext(new RequirePaymentContextAttribute());
        mockHttpContext.Object.RequestServices = serviceProvider;

        var mockRequestDelegate = new Mock<RequestDelegate>();
        mockRequestDelegate.Setup(e => e.Invoke(mockHttpContext.Object))
            .Callback<HttpContext>(c => {
                capturedServiceProvider = c.RequestServices;

                // Verify that the service provider was wrapped (persisted) during execution
                // Captured by checking if we can create a new scope and still get the context
                using var scope = c.RequestServices.CreateScope();
                var accessor = scope.ServiceProvider.GetRequiredService<IPaymentContextAccessor>();
                capturedInnerContext = accessor.Value;
            })
            .Returns(Task.CompletedTask);

        var middleware = new PaymentMiddleware(new ServiceRegistry(), stubOptions.Object);

        // Act
        await middleware.InvokeAsync(mockHttpContext.Object, mockRequestDelegate.Object);

        // Assert
        AssertRequestDelegateWithHttpContext(mockRequestDelegate, mockHttpContext.Object);

        // Retrieve the main accessor to compare
        AssertPaymentContext(mainAccessor.Value);

        // Verify wrapping and persistence
        Assert.IsNotNull(capturedServiceProvider);
        Assert.AreNotSame(serviceProvider, capturedServiceProvider, "RequestServices should be wrapped during delegate execution");
        Assert.IsNotNull(capturedInnerContext, "Inner scope context was not captured");
        Assert.AreEqual(mainAccessor.Value, capturedInnerContext, "Inner scope should have the same PaymentContext");

        // Verify that the service provider was restored after execution
        Assert.AreSame(serviceProvider, mockHttpContext.Object.RequestServices, "RequestServices should be restored after execution");
    }

    [TestMethod]
    public async Task Invoke_WithPersistPaymentContext_ExceptionWhenInvokeRequestDelegate_ServiceProviderRestored()
    {
        // Arrange
        var stubOptions = new Mock<IOptions<PaymentOptions>>();
        stubOptions.Setup(e => e.Value).Returns(new PaymentOptions() { PersistPaymentContext = true });

        var mockHttpContext = CreateFakeHttpContext(new RequirePaymentContextAttribute());
        // Setup factories needed for context creation
        var fakePaymentContextAccessor = new PaymentContextAccessor(new ContextAccessor<PaymentContext>());

        var services = new ServiceCollection();
        services.AddSingleton<IPaymentContextFactory>(new DefaultPaymentContextFactory(stubOptions.Object));
        services.AddSingleton<IPaymentContextAccessor>(fakePaymentContextAccessor);
        var customProvider = services.BuildServiceProvider();

        mockHttpContext.SetupProperty(h => h.RequestServices, customProvider);
        var originalServiceProvider = customProvider;

        var mockRequestDelegate = new Mock<RequestDelegate>();
        mockRequestDelegate.Setup(r => r.Invoke(It.IsAny<HttpContext>())).ThrowsAsync(new InvalidOperationException("Something went wrong"));

        var middleware = new PaymentMiddleware(new ServiceRegistry(), stubOptions.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await middleware.InvokeAsync(mockHttpContext.Object, mockRequestDelegate.Object);
        }, "Exception should be thrown");

        // Verify that RequestServices was restored to the original one
        Assert.AreSame(originalServiceProvider, mockHttpContext.Object.RequestServices);
    }

    [TestMethod]
    public async Task Invoke_WithPaymentRedirectionHandler_NoPaymentServiceName_ErrorResultHasBeenSetToFeatureCollection()
    {
        // Arrange
        var stubOptions = new Mock<IOptions<PaymentOptions>>();
        stubOptions.Setup(e => e.Value).Returns(new PaymentOptions());

        var mockHttpContext = CreateFakeHttpContext(new PaymentRedirectionHandlerAttribute());
        mockHttpContext.SetupGet(e => e.Request.Query[It.IsAny<string>()]).Returns(StringValues.Empty);
        var mockRequestDelegate = new Mock<RequestDelegate>();

        var middleware = new PaymentMiddleware(new ServiceRegistry(), stubOptions.Object);

        // Act
        await middleware.InvokeAsync(mockHttpContext.Object, mockRequestDelegate.Object);

        // Assert
        AssertRequestDelegateWithHttpContext(mockRequestDelegate, mockHttpContext.Object);

        var feature = mockHttpContext.Object.Features.Get<PaymentRedirectionResult>();
        Assert.IsNotNull(feature, $"{nameof(PaymentRedirectionResult)} should be set in the FeatureCollection");
        Assert.AreEqual(PaymentRedirectionError.MissingServiceName, feature.Error, $"Error should be {nameof(PaymentRedirectionError.MissingServiceName)}");
        Assert.IsNull(feature.Result, "Result should be null when error occurs");
        Assert.IsNotNull(feature.ErrorMessage, "Error message should be set when error occurs");
    }

    [TestMethod]
    public async Task Invoke_WithPaymentRedirectionHandler_InvalidServiceName_ErrorResultHasBeenSetToFeatureCollection()
    {
        // Arrange
        var stubOptions = new Mock<IOptions<PaymentOptions>>();
        stubOptions.Setup(e => e.Value).Returns(new PaymentOptions());

        var mockHttpContext = CreateFakeHttpContext(new PaymentRedirectionHandlerAttribute());

        mockHttpContext
            .Setup(h => h.Request.Query["payservice"])
            .Returns("invalid-provider");

        var mockRequestDelegate = new Mock<RequestDelegate>();

        var middleware = new PaymentMiddleware(new ServiceRegistry(), stubOptions.Object);

        // Act
        await middleware.InvokeAsync(mockHttpContext.Object, mockRequestDelegate.Object);

        // Assert
        AssertRequestDelegateWithHttpContext(mockRequestDelegate, mockHttpContext.Object);

        var feature = mockHttpContext.Object.Features.Get<PaymentRedirectionResult>();
        Assert.IsNotNull(feature, $"{nameof(PaymentRedirectionResult)} should be set in the FeatureCollection");
        Assert.AreEqual(PaymentRedirectionError.UnknownServiceName, feature.Error, $"Error should be {nameof(PaymentRedirectionError.UnknownServiceName)}");
        Assert.IsNull(feature.Result, "Result should be null when error occurs");
        Assert.IsNotNull(feature.ErrorMessage, "Error message should be set when error occurs");
    }

    [TestMethod]
    public async Task Invoke_WithPaymentRedirection_ValidService_ReturnsResult()
    {
        // Arrange
        var stubOptions = new Mock<IOptions<PaymentOptions>>();
        stubOptions.Setup(e => e.Value).Returns(new PaymentOptions());

        var mockPaymentService = new Mock<IPaymentService>();
        var expectedResult = CreatePaymentResult();
        mockPaymentService
            .Setup(s => s.GetResultFromRedirectionAsync(It.IsAny<Dictionary<string, string?>>()))
            .Returns(Task.FromResult<PaymentResult?>(expectedResult));

        var serviceRegistry = new ServiceRegistry();
        serviceRegistry.AddService(mockPaymentService.Object.GetType(), s_stubPaymentDescriptor);

        var mockHttpContext = CreateFakeHttpContext(new PaymentRedirectionHandlerAttribute());

        mockHttpContext
            .SetupGet(h => h.Request.Query)
            .Returns(new QueryCollection(new Dictionary<string, StringValues>
            {
                ["payservice"] = FakePaymentServiceName,
                ["orderId"] = "123",
            }));

        mockHttpContext
            .Setup(s => s.RequestServices.GetService(mockPaymentService.Object.GetType()))
            .Returns(mockPaymentService.Object);

        var mockRequestDelegate = new Mock<RequestDelegate>();
        mockRequestDelegate
            .Setup(r => r.Invoke(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        var middleware = new PaymentMiddleware(serviceRegistry, stubOptions.Object);

        // Act
        await middleware.InvokeAsync(mockHttpContext.Object, mockRequestDelegate.Object);

        // Assert
        AssertRequestDelegateWithHttpContext(mockRequestDelegate, mockHttpContext.Object);

        var feature = mockHttpContext.Object.Features.Get<PaymentRedirectionResult>();
        Assert.IsNotNull(feature, $"{nameof(PaymentRedirectionResult)} should be set in the FeatureCollection");
        Assert.AreEqual(PaymentRedirectionError.None, feature.Error);
        Assert.AreEqual(expectedResult, feature.Result);
        Assert.IsNull(feature.ErrorMessage, "Error message shouldn't be set when there's no error here");
    }

    [TestMethod]
    public async Task Invoke_WithPaymentRedirection_ValidService_ReturnsNull()
    {
        // Arrange
        var stubOptions = new Mock<IOptions<PaymentOptions>>();
        stubOptions.Setup(e => e.Value).Returns(new PaymentOptions());

        var mockPaymentService = new Mock<IPaymentService>();

        var mockHttpContext = CreateFakeHttpContext(new PaymentRedirectionHandlerAttribute());

        mockHttpContext
            .SetupGet(h => h.Request.Query)
            .Returns(new QueryCollection(new Dictionary<string, StringValues>
            {
                ["payservice"] = FakePaymentServiceName,
            }));

        var expectedResult = CreatePaymentResult();

        mockPaymentService
            .Setup(s => s.GetResultFromRedirectionAsync(It.IsAny<Dictionary<string, string?>>()))
            .Returns(Task.FromResult<PaymentResult?>(null));

        var serviceRegistry = new ServiceRegistry();
        serviceRegistry.AddService(mockPaymentService.Object.GetType(), s_stubPaymentDescriptor);

        mockHttpContext
            .Setup(s => s.RequestServices.GetService(mockPaymentService.Object.GetType()))
            .Returns(mockPaymentService.Object);

        var mockRequestDelegate = new Mock<RequestDelegate>();
        mockRequestDelegate
            .Setup(r => r.Invoke(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        var middleware = new PaymentMiddleware(serviceRegistry, stubOptions.Object);

        // Act
        await middleware.InvokeAsync(mockHttpContext.Object, mockRequestDelegate.Object);

        // Assert
        AssertRequestDelegateWithHttpContext(mockRequestDelegate, mockHttpContext.Object);

        var feature = mockHttpContext.Object.Features.Get<PaymentRedirectionResult>();
        Assert.IsNotNull(feature, $"{nameof(PaymentRedirectionResult)} should be set in the FeatureCollection");
        Assert.AreEqual(PaymentRedirectionError.InvalidRedirectionData, feature.Error);
        Assert.IsNull(feature.Result, "Result should be null when error occurs");
        Assert.IsNotNull(feature.ErrorMessage, "Error message should be set when there's no error here");
    }

    [TestMethod]
    public async Task Invoke_WithPaymentRedirection_AsyncService_AwaitsResult()
    {
        // Arrange
        var stubOptions = new Mock<IOptions<PaymentOptions>>();
        stubOptions.Setup(e => e.Value).Returns(new PaymentOptions());

        var stubPaymentDescriptor = new PaymentServiceDescriptor
        {
            Currencies = ["VND"],
            DisplayName = FakePaymentServiceName,
            IconUrl = string.Empty,
            Name = FakePaymentServiceName
        };

        var mockPaymentService = new Mock<IPaymentService>();

        var expectedResult = CreatePaymentResult();
        var tcs = new TaskCompletionSource<PaymentResult?>();
        mockPaymentService
            .Setup(s => s.GetResultFromRedirectionAsync(It.IsAny<Dictionary<string, string?>>()))
            .Returns(tcs.Task);

        var serviceRegistry = new ServiceRegistry();
        serviceRegistry.AddService(mockPaymentService.Object.GetType(), stubPaymentDescriptor);

        var mockHttpContext = CreateFakeHttpContext(new PaymentRedirectionHandlerAttribute());

        mockHttpContext
            .SetupGet(h => h.Request.Query)
            .Returns(new QueryCollection(new Dictionary<string, StringValues>
            {
                ["payservice"] = FakePaymentServiceName,
            }));

        mockHttpContext
            .Setup(s => s.RequestServices.GetService(mockPaymentService.Object.GetType()))
            .Returns(mockPaymentService.Object);

        var mockRequestDelegate = new Mock<RequestDelegate>();
        mockRequestDelegate.Setup(r => r.Invoke(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        var middleware = new PaymentMiddleware(serviceRegistry, stubOptions.Object);

        // Act when result is not available
        var invokeTask = middleware.InvokeAsync(mockHttpContext.Object, mockRequestDelegate.Object);

        // Assert when result is not available
        Assert.IsFalse(invokeTask.IsCompleted, "Task should not be completed yet as service is pending");
        Assert.IsNull(mockHttpContext.Object.Features.Get<PaymentRedirectionResult>(), "Feature shouldn't be set when redirection result is not ready yet.");
        mockRequestDelegate.Verify(e => e.Invoke(It.IsAny<HttpContext>()), Times.Never, "Request delegate shouldn't be invoked when result is not ready yet.");

        // Act when result is ready
        tcs.SetResult(expectedResult);
        await invokeTask;

        // Assert when result is ready
        AssertRequestDelegateWithHttpContext(mockRequestDelegate, mockHttpContext.Object);

        var feature = mockHttpContext.Object.Features.Get<PaymentRedirectionResult>();
        Assert.IsNotNull(feature, $"{nameof(PaymentRedirectionResult)} should be set in the FeatureCollection");
        Assert.AreEqual(PaymentRedirectionError.None, feature.Error);
        Assert.AreEqual(expectedResult, feature.Result);
        Assert.IsNull(feature.ErrorMessage, "Error message shouldn't be set when there's no error here");
    }

    private static void AssertPaymentContext(PaymentContext? context)
    {
        Assert.IsNotNull(context, $"{nameof(PaymentContext)} hasn't been set");
        Assert.AreEqual(s_expectedRemoteIpAddress, context.RemoteIpAddress, $"{nameof(PaymentContext)}.{nameof(PaymentContext.RemoteIpAddress)} doesn't match");
        Assert.AreEqual(s_expectedBaseUrl, context.BaseUrl, $"{nameof(PaymentContext)}.{nameof(PaymentContext.BaseUrl)} doesn't match");
    }

    private static void AssertRequestDelegateWithHttpContext(Mock<RequestDelegate> mockRequestDelegate, HttpContext context)
    {
        mockRequestDelegate.Verify(e => e.Invoke(It.IsAny<HttpContext>()), Times.Once, "Request delegate should be invoked once");
        mockRequestDelegate.Verify(e => e.Invoke(context), Times.Once, "Request delegate should be invoked with the same context");
    }

    private static PaymentResult CreatePaymentResult()
    {
        return new PaymentResult
        {
            Amount = 12000,
            CreatedDate = new DateTimeOffset(new DateTime(2023, 07, 14, 12, 50, 10), TimeSpan.Zero),
            Currency = "VND",
            HandlerName = "mockhandler",
            Identifier = "1234",
            ServiceName = "mockservice",
        };
    }

    internal static Mock<HttpContext> CreateFakeHttpContext(object? metadata = null)
    {
        var metadataCollection = metadata is not null
            ? new EndpointMetadataCollection(metadata)
            : new EndpointMetadataCollection();

        var endpoint = new Endpoint(null, metadataCollection, "TestEndpoint");

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.SetupGet(h => h.Features).Returns(new FeatureCollection());

        // Setup some properties might need for creating PaymentContext
        mockHttpContext.Setup(h => h.Connection.RemoteIpAddress).Returns(s_expectedRemoteIpAddress);
        mockHttpContext.SetupProperty(h => h.Request.Scheme, s_protocol);
        mockHttpContext.SetupProperty(h => h.Request.Host, new HostString(s_fakeHostName));

        // Endpoint for deciding which behavior should middleware take
        mockHttpContext.Object.SetEndpoint(endpoint);

        // Setup service provider
        mockHttpContext.SetupProperty(e => e.RequestServices, Helpers.CreateServiceCollectionWithPayment().BuildServiceProvider());

        return mockHttpContext;
    }
}

