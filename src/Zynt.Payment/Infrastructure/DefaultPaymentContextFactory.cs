using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Zynt.Payment.DependencyInjection;
using Zynt.Payment.Models;

namespace Zynt.Payment.Infrastructure;

public class DefaultPaymentContextFactory : IPaymentContextFactory
{
    private readonly string? _baseUrl;

    public DefaultPaymentContextFactory(IOptions<PaymentOptions> options)
    {
        _baseUrl = options.Value.BaseUrl;
    }
    
    public PaymentContext Create(HttpContext context)
    {
        return new PaymentContext
        {
            RemoteIpAddress = context.Connection.RemoteIpAddress,
            BaseUrl = _baseUrl ?? $"{context.Request.Scheme}://{context.Request.Host}",
        };
    }
}