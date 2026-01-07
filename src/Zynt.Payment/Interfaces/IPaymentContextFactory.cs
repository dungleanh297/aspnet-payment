using Microsoft.AspNetCore.Http;
using Zynt.Payment.Models;

namespace Zynt.Payment.DependencyInjection;

public interface IPaymentContextFactory
{
    PaymentContext Create(HttpContext context);
}