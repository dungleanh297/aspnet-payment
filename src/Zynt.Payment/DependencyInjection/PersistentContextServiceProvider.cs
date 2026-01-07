using Microsoft.Extensions.DependencyInjection;

namespace Zynt.Payment.DependencyInjection;

internal sealed class PersistentContextServiceProvider<TContext> : IServiceProvider, IDecorating<IServiceProvider> where TContext : class
{
    private readonly IServiceProvider _serviceProvider;

    public PersistentContextServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IServiceProvider))
        {
            return this;
        }

        if (serviceType == typeof(IServiceScopeFactory))
        {
            return new PersistentContextServiceScopeFactory<TContext>(_serviceProvider as PersistentContextServiceProvider<TContext> ?? this);
        }

        return _serviceProvider.GetService(serviceType);
    }

    public IServiceProvider Unwrap()
    {
        return _serviceProvider;
    }
}