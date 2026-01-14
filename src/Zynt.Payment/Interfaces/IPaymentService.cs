using Microsoft.AspNetCore.Routing;
using Zynt.Payment.Models;
using Zynt.Payment.Primitives;

namespace Zynt.Payment.Interfaces;

public interface IPaymentService
{
    Task<PaymentRequestUrls> CreatePaymentUrlAsync(PaymentRequest request, PaymentContext context);

    Task<PaymentResult?> GetResultFromRedirectionAsync(Dictionary<string, string?> queryParameters);

    void ConfigureWebhook(IEndpointRouteBuilder routeBuilder);
}