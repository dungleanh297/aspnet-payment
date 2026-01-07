using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Routing;
using Zynt.Payment.Interfaces;
using Zynt.Payment.Registries;

namespace Zynt.Payment.Registries;

internal sealed class ServiceRegistry
{
    private readonly List<Action<IEndpointRouteBuilder>> _webhookConfiguring = [];

    internal Dictionary<string, ServiceTypeInfo> ServicesTypes { get; } = [];

    internal List<PaymentServiceDescriptor> Descriptors { get; } = [];

    public void AddService<TService>(PaymentServiceDescriptor descriptor, Action<IEndpointRouteBuilder> webhookConfiguring) where TService : IPaymentService
    {
        ArgumentNullException.ThrowIfNull(descriptor, nameof(descriptor));
        ArgumentNullException.ThrowIfNull(webhookConfiguring, nameof(webhookConfiguring));

        Descriptors.Add(descriptor);
        _webhookConfiguring.Add(webhookConfiguring);

        var serviceTypeInfo = new ServiceTypeInfo
        {
            Currencies = descriptor.Currencies,
            Type = typeof(TService),
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

    public void ConfigureWebhook(IEndpointRouteBuilder routeBuilder)
    {
        foreach (var config in _webhookConfiguring)
        {
            config(routeBuilder);
        }
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