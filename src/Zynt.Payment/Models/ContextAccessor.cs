using Zynt.Payment.Interfaces;

namespace Zynt.Payment.Models;

internal sealed class ContextAccessor<TContext> : IContextAccessor<TContext>
{
    public TContext? Value { get; set; }
}