namespace Zynt.Payment.Test;

internal static class Constants
{
    public static readonly Dictionary<string, RedirectUri> RedirectUris = new Dictionary<string, RedirectUri>
    {
        { DefaultRedirectAlias, DefaultRedirectUrl },
        { MyWebApplicationRedirectAlias, MyWebApplicationRedirectUrl },
    };

    public const string DefaultRedirectAlias = PaymentRequest.DefaultRedirectAlias;
    
    public const string DefaultRedirectUrl = "/payment-redirect?test-parameter=some-value";

    public const string MyWebApplicationRedirectAlias = "MyWebApplication";

    public const string MyWebApplicationRedirectUrl = "myapp://payment-redirect";
    
    public const string Hostname = "https://sample-page.com";

    public const string APIGateway = "/api/payment";

    public const string VNDCurrencyCode = "VND";
    
    public const string USDCurrencyCode = "USD";
}