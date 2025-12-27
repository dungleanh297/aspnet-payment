namespace Zynt.Payment.Primitives;

public readonly struct PaymentRequestUrls
{
    public required string HttpUrl { get; init; }

    public string? Deeplink { get; init; }
}