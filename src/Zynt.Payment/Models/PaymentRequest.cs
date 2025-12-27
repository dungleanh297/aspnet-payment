namespace Zynt.Payment;

public sealed class PaymentRequest
{
    private long _amount;

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

    public required string Currency { get; init; }
    
    public string? Description { get; init; }
    
    public required string HandlerName { get; init; }
    
    public required string Identifier { get; init; }
    
    public required string ServiceName { get; init; }

    public required string RedirectUrl { get; set; }
    
}