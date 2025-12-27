namespace Zynt.Payment;

public sealed class PaymentResult : IEquatable<PaymentResult>
{
    private readonly long _amount;
    
    public required long Amount
    {
        get => _amount;
        init
        {
            if (value <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero");
            }
            _amount = value;
        }
    }

    public DateTimeOffset CreatedDate { get; init; }
    
    public required string Currency { get; init; }

    public required string HandlerName { get; init; }
    
    public required string Identifier { get; init; }
    
    public required string ServiceName { get; init; }
    
    public PaymentStatus Status { get; init; }

    public override int GetHashCode()
    {
        return HashCode.Combine(_amount, CreatedDate, Currency, HandlerName, Identifier, ServiceName, (int)Status);
    }
    
    public bool Equals(PaymentResult? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return _amount == other._amount 
               && CreatedDate.Equals(other.CreatedDate)
               && Currency == other.Currency
               && HandlerName == other.HandlerName
               && Identifier == other.Identifier
               && ServiceName == other.ServiceName
               && Status == other.Status;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is PaymentResult other && Equals(other));
    }
}
