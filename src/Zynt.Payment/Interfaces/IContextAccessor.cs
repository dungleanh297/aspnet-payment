public interface IContextAccessor<TContext> where TContext : class
{
    public TContext? Value { get; set; }
}