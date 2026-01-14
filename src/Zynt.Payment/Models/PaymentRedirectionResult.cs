using System.Diagnostics.CodeAnalysis;

namespace Zynt.Payment;

public class PaymentRedirectionResult
{

    internal static readonly PaymentRedirectionResult MissingServiceName = new PaymentRedirectionResult(
        error: PaymentRedirectionError.MissingServiceName,
        message: "Payment service name is missing in the redirection request."
    );

    internal static readonly PaymentRedirectionResult UnknownServiceName = new PaymentRedirectionResult(
        error: PaymentRedirectionError.UnknownServiceName,
        message: "Payment service name is unknown."
    );

    internal static readonly PaymentRedirectionResult InvalidRedirectionData = new PaymentRedirectionResult(
        error: PaymentRedirectionError.InvalidRedirectionData,
        message: "The redirection data provided is invalid."
    );

    internal PaymentRedirectionResult(PaymentRedirectionError error, string message)
    {
        if (error == PaymentRedirectionError.None)
        {
            throw new ArgumentException("Error must not be None for this constructor.", nameof(error));
        }

        Error = error;
        ErrorMessage = message;
    }

    internal PaymentRedirectionResult(PaymentResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        Result = result;
    }

    [MemberNotNullWhen(true, nameof(Result))]
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    public bool IsSuccess => Error == PaymentRedirectionError.None;

    public PaymentRedirectionError Error { get; init; } = PaymentRedirectionError.None;

    public string? ErrorMessage { get; init; }

    public PaymentResult? Result { get; }
}

public enum PaymentRedirectionError
{
    None = 0,
    MissingServiceName = 1,
    UnknownServiceName = 2,

    InvalidRedirectionData = 3,
}
