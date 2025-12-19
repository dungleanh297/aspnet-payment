namespace Zynt.Payment;

/// <summary>
/// Defines a contract for handling payment when a transaction completes successfully.
/// </summary>
public interface IPaymentSuccessHandler
{
    /// <summary>
    /// Called when a payment transaction completes successfully (<see cref="PaymentStatus.Success"/>).
    /// Implement logic here for order fulfillment, sending receipts, or updating records.
    /// </summary>
    /// <param name="result">The <see cref="PaymentResult"/> containing transaction details and current status.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to a <see cref="PaymentHandlerResult"/> indicating the outcome of the operation.
    /// </returns>
    Task OnTransactionSuccessAsync(PaymentResult result);
}