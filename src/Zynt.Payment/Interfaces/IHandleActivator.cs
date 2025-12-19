using System;

namespace Zynt.Payment.Interfaces;

public interface IPaymentHandleActivator
{
    Task InvokeAsync(string handlerName, PaymentResult result);
}
