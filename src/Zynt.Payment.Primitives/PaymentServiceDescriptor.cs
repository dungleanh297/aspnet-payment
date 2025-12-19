namespace Zynt.Payment;

/// <summary>
/// Represents metadata describing a payment service implementation,
/// including its unique scheme identifier, display name, and icon URL.
/// Used for service registration, discovery, and UI display.
/// </summary>
public sealed class PaymentServiceDescriptor
{
    /// <summary>
    /// Gets the unique scheme identifier for the payment service (e.g., "paypal", "stripe").
    /// </summary>
    public required string Scheme { get; init; }

    /// <summary>
    /// Gets the human-readable display name of the payment service (e.g., "PayPal", "Stripe").
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets the currency code (ISO 4217 format) that the payment service operates with (e.g., "USD", "EUR").
    /// </summary>
    public required string[]? Currencies { get; init; }

    /// <summary>
    /// Gets the URL of the icon representing the payment service, suitable for UI display.
    /// </summary>
    public required string IconUrl { get; init; }
}
