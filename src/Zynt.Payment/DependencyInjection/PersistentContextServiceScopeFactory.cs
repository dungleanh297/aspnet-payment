using Microsoft.Extensions.DependencyInjection;

namespace Zynt.Payment.DependencyInjection;

internal sealed class PersistentContextServiceScopeFactory<TContext> : IServiceScopeFactory where TContext : class
{
    private readonly PersistentContextServiceProvider<TContext> _serviceProvider;

    public PersistentContextServiceScopeFactory(PersistentContextServiceProvider<TContext> serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IServiceScope CreateScope()
    {
        IServiceScope scope = _serviceProvider.Unwrap().CreateScope();

        var contextValue = _serviceProvider.GetService<IContextAccessor<TContext>>()?.Value;
        
        if (contextValue is null)
        {
            return scope;
        }

        return new PersistentContextServiceScope<TContext>(scope, contextValue);
    }
}