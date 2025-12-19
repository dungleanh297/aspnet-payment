namespace Zynt.Payment;

/// <summary>
/// Defines a contract for logging payment transaction results.
/// </summary>
public interface IPaymentLogger
{
    /// <summary>
    /// Asynchronously logs the specified payment result.
    /// </summary>
    /// <param name="result">The <see cref="PaymentResult"/> to log.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogAsync(PaymentResult result);
}