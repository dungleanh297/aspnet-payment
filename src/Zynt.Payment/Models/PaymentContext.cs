using System.Net;

namespace Zynt.Payment.Models;

public class PaymentContext
{
    public IPAddress? RemoteIpAddress { get; set; }

    public string BaseUrl { get; set; } = null!;
}