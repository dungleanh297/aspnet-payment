namespace Zynt.Payment;

/// <summary>
/// Represents information about a payment handler implementation (ex: OnSuccess, Disposable).
/// </summary>
internal class HandlerImplementationInfo
{
    public required HandlerImplementationTypes ImplementationTypes { get; init; }
    
    public required Type Type { get; init; }
}