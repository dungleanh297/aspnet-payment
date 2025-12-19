using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Zynt.Payment.Registries;

internal sealed class HandlerRegistry
{
    private readonly Dictionary<string, HandlerImplementationInfo> _info = new ();

    public void AddFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        
        var info = GetAllImplementationInfo(assembly);

        foreach (var kvp in info)
        {
            if (!_info.TryAdd(kvp.Key, kvp.Value))
            {
                throw new InvalidOperationException($"A payment handler with the name '{kvp.Key}' is already registered.");
            }
        }
    }

    public bool TryGetImplementationInfo(string handlerName, [NotNullWhen(true)] out HandlerImplementationInfo? info)
    {
        return _info.TryGetValue(handlerName, out info);
    }

    private static IEnumerable<KeyValuePair<string, HandlerImplementationInfo>> GetAllImplementationInfo(Assembly assembly)
    {
        foreach (var type in assembly.DefinedTypes)
        {
            if (!type.IsClass)
            {
                continue;
            }

            var attribute = type.GetCustomAttribute<PaymentHandlerAttribute>();

            if (attribute is null)
            {
                continue;
            }

            var implementationTypes = GetImplementationTypes(type);

            if (!implementationTypes.HasFlag(HandlerImplementationTypes.OnSuccess))
            {
                throw new InvalidOperationException($"Payment handler '{type.FullName}' must implement required interface: {nameof(IPaymentSuccessHandler)}.");
            }

            yield return new KeyValuePair<string, HandlerImplementationInfo>(attribute.Name, new HandlerImplementationInfo
            {
                Type = type.AsType(),
                ImplementationTypes = implementationTypes
            });
        }
    }

    private static HandlerImplementationTypes GetImplementationTypes(Type type)
    {
        HandlerImplementationTypes types = 0;

        if (type.IsAssignableTo(typeof(IPaymentSuccessHandler)))
        {
            types |= HandlerImplementationTypes.OnSuccess;
        }

        if (type.IsAssignableTo(typeof(IAsyncDisposable)))
        {
            types |= HandlerImplementationTypes.AsyncDisposable;
        }
        else if (type.IsAssignableTo(typeof(IDisposable)))
        {
            types |= HandlerImplementationTypes.Disposable;
        }

        return types;
    }
}