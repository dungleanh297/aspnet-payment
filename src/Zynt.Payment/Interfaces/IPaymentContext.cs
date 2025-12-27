using Microsoft.AspNetCore.Http;

/// <summary>
/// Encapsulate information about the payment request, indirect from <see cref="HttpContext" /> 
/// </summary>
/// 
namespace Zynt.Payment.Interfaces;

public interface IPaymentContext
{
    ConnectionInfo Connection { get; }

    string BaseUrl { get; }
}