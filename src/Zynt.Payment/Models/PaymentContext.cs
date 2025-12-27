using Microsoft.AspNetCore.Http;
using Zynt.Payment.Interfaces;

namespace Zynt.Payment.Models;

internal class PaymentContext : IPaymentContext
{
    public required ConnectionInfo Connection { get; set; }

    public required string BaseUrl { get; set; }
}