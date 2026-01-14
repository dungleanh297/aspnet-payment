using System.Diagnostics.CodeAnalysis;
using Zynt.Payment.Interfaces;

namespace Zynt.Payment.Registries;

internal sealed class ServiceRegistry
{
    internal Dictionary<string, ServiceTypeInfo> ServicesTypes { get; } = [];

    internal List<PaymentServiceDescriptor> Descriptors { get; } = [];

    public void AddService<TService>(PaymentServiceDescriptor descriptor) where TService : IPaymentService
    {
        AddService(typeof(TService), descriptor);
    }

    public void AddService(Type serviceType, PaymentServiceDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor, nameof(descriptor));

        if (!serviceType.IsAssignableTo(typeof(IPaymentService)))
        {
            throw new ArgumentException($"{serviceType.FullName} registered as payment service. But it is not implement {nameof(IPaymentService)}");
        }

        Descriptors.Add(descriptor);

        var serviceTypeInfo = new ServiceTypeInfo
        {
            Currencies = descriptor.Currencies,
            Type = serviceType,
        };

        ServicesTypes.Add(descriptor.Name, serviceTypeInfo);
    }

    public bool TryGetServiceTypeInfo(string serviceName, [NotNullWhen(true)] out ServiceTypeInfo serviceTypeInfo)
    {
        return ServicesTypes.TryGetValue(serviceName, out serviceTypeInfo);
    }

    public IEnumerable<PaymentServiceDescriptor> GetAllServices()
    {
        return Descriptors;
    }
}

internal readonly struct ServiceTypeInfo
{
    public string[]? Currencies { get; init; }

    public Type Type { get; init; }

    public bool IsCurrencySupported([NotNullWhen(true)] string? currency)
    {
        if (string.IsNullOrEmpty(currency))
        {
            return false;
        }

        if (Currencies is null || Currencies.Length == 0)
        {
            return true;
        }

        foreach (var c in Currencies)
        {
            if (c.Equals(currency))
            {
                return true;
            }
        }

        return false;
    }
}