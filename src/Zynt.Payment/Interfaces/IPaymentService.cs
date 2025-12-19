using Microsoft.AspNetCore.Http;

namespace Zynt.Payment;

/// <summary>
/// Defines a contract for payment service operations, including payment URL creation,
/// handling payment gateway redirects and callbacks, and generating HTTP responses
/// based on payment processing results.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Creates a payment URL for the specified payment request.
    /// This URL can be used to redirect the user to an external payment gateway.
    /// </summary>
    /// <param name="request">The <see cref="PaymentRequest"/> containing payment details such as amount, identifier, and handler name.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to a string containing the payment URL.
    /// </returns>
    Task<string> CreatePaymentUrlAsync(PaymentRequest request);

    /// <summary>
    /// Handles the HTTP redirect request from the payment gateway after the user completes or cancels the payment.
    /// Parses the request to extract payment result information.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequest"/> received from the payment gateway redirect.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to a <see cref="PaymentResult"/> representing the outcome of the payment transaction.
    /// </returns>
    Task<bool> ValidateRedirectRequestAsync(HttpContext request);
}