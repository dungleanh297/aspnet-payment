using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Zynt.Payment.Interfaces;

namespace Zynt.Payment.DependencyInjection;

public interface IPaymentBuilder
{
    IServiceCollection Services { get; }

    IPaymentBuilder AddService<TService>(PaymentServiceDescriptor descriptor) where TService : class, IPaymentService;

    IPaymentBuilder AddHandlers(Assembly assembly);

    IPaymentBuilder AddLogger<T>() where T : class, IPaymentLogger;
}