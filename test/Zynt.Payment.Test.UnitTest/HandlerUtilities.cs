using System;
using System.Reflection;
using Zynt.Payment.Attributes;
using Zynt.Payment.Interfaces;

namespace Zynt.Payment.Test.UnitTest;

public static class HandlerUtilities
{
    public static string? GetHandlerNameFromType(Type type)
    {
        if (!type.IsAssignableTo(typeof(IPaymentSuccessHandler)))
        {
            return null;
        }

        return type.GetCustomAttribute<PaymentHandlerAttribute>()?.Name;
    }
}