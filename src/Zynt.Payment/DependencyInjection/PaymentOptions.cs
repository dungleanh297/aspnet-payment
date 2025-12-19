using Microsoft.Extensions.Options;

namespace Zynt.Payment.DependencyInjection;

public class PaymentOptions
{
    public string Hostname { get; set; } = null!;
}

internal class PaymentOptionsValidator : IValidateOptions<PaymentOptions>
{
    public ValidateOptionsResult Validate(string? name, PaymentOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Hostname))
        {
            return ValidateOptionsResult.Fail("Hostname must be provided.");
        }
        return ValidateOptionsResult.Success;
    }
}