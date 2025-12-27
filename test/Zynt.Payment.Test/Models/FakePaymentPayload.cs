namespace Zynt.Payment.Test;

public class FakePaymentPayload
{
    public required int Amount { get; set; }

    public required string Currency { get; set; }

    public required DateTimeOffset CreatedDate { get; set; }

    public required string Identifier { get; set; }

    public required string HandlerName { get; set; }

    public bool IsValid { get; set; }
    
}
