using System;
using System.Diagnostics.CodeAnalysis;

namespace Zynt.Payment.Test.Common;

public class ThrowHelpers
{
    public static void ThrowIfRedirectUrlNotInitialized(PaymentRequest request)
    {
        if (request.RedirectUrl is null)
        {
            throw new InvalidOperationException("Redirect URL isn't initialized");
        }
    } 
}
