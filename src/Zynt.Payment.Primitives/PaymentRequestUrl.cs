namespace Zynt.Payment.Primitives;

public readonly struct PaymentRequestUrl
{
    public required string HttpUrl { get; init; }

    public string? Deeplink { get; init; }
}