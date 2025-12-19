namespace Zynt.Payment.Test;

public interface IUrlCreator
{
    string CreateRedirectUrl(string redirectAlias, PaymentResult paymentResult, bool isValid);

    string CreateCallbackUrl(PaymentResult paymentResult, bool isValid);
}

public interface IUrlCreator<TService> : IUrlCreator { }