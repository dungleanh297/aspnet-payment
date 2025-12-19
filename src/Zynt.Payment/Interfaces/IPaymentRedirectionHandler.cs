using Microsoft.AspNetCore.Http;

namespace Zynt.Payment.Interfaces;

public interface IPaymentRedirectionHandler
{
    Task OnRedirectionAsync(HttpContext context, PaymentResult result);
}