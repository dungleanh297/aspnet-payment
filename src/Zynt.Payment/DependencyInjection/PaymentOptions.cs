using Microsoft.Extensions.Options;

namespace Zynt.Payment.DependencyInjection;

public class PaymentOptions
{
    public string? BaseUrl { get; set; }

    public bool PersistPaymentContext { get; set; }
}

internal class PaymentOptionsValidator : IValidateOptions<PaymentOptions>
{
    public ValidateOptionsResult Validate(string? name, PaymentOptions options)
    {
        if (options.BaseUrl is not null && !(Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)))
        {
            return ValidateOptionsResult.Fail($"Invalid BaseUrl: {options.BaseUrl}");
        }
        
        return ValidateOptionsResult.Success;
    }
}