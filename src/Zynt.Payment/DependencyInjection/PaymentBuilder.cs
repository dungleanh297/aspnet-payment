using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Zynt.Payment.Interfaces;
using Zynt.Payment.Registries;

namespace Zynt.Payment.DependencyInjection;

internal class PaymentBuilder : IPaymentBuilder
{
    private readonly ServiceRegistry _serviceRegistry;
    private readonly HandlerRegistry _handlerRegistry;

    public IServiceCollection Services { get; }

    public PaymentBuilder(IServiceCollection serviceCollection, ServiceRegistry serviceRegistry, HandlerRegistry handlerRegistry)
    {
        Services = serviceCollection;
        _serviceRegistry = serviceRegistry;
        _handlerRegistry = handlerRegistry;
    }

    public IPaymentBuilder AddService<TService>(PaymentServiceDescriptor descriptor) where TService : class, IPaymentService
    {
        Services.TryAddTransient<TService>();
        _serviceRegistry.AddService<TService>(descriptor);
        return this;
    }

    public IPaymentBuilder AddHandlers(Assembly assembly)
    {
        _handlerRegistry.AddFromAssembly(assembly);
        return this;
    }

    public IPaymentBuilder AddLogger<T>() where T : class, IPaymentLogger
    {
        Services.AddTransient<IPaymentLogger, T>();
        return this;
    }
}