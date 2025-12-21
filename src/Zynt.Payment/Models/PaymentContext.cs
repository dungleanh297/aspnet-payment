using System.Security.Claims;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Encapsulate information about the payment request, indirect from <see cref="HttpContext" /> 
/// </summary>
public sealed class PaymentContext
{
    public required ConnectionInfo Connection { get; init; }

    public required ClaimsPrincipal? User { get; init; }
}