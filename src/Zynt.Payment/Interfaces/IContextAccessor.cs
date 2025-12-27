namespace Zynt.Payment.Interfaces;

internal interface IContextAccessor<TContext>
{
    public TContext? Value { get; set; }
}