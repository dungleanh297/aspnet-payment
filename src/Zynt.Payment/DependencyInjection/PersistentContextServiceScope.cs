using Microsoft.Extensions.DependencyInjection;

namespace Zynt.Payment.DependencyInjection;

internal sealed class PersistentContextServiceScope<TContext> : IServiceScope where TContext : class
{
    private readonly IServiceScope  _serviceScope;
    private readonly PersistentContextServiceProvider<TContext> _serviceProvider;

    public IServiceProvider ServiceProvider => _serviceProvider;

    public PersistentContextServiceScope(IServiceScope serviceScope, TContext value)
    {
        var parentServiceScope = serviceScope.ServiceProvider;
        parentServiceScope.GetRequiredService<IContextAccessor<TContext>>().Value = value;

        _serviceScope = serviceScope;
        _serviceProvider = new PersistentContextServiceProvider<TContext>(parentServiceScope);
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}