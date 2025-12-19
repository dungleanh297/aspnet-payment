using System;

namespace Zynt.Payment.Interfaces;

public interface IHandleActivator
{
    Task InvokeAsync(string handlerName, PaymentResult result);
}
