using Zynt.Payment.Interfaces;

namespace Zynt.Payment.Test;

public interface ITestHandler
{
    HandlerState State { get; }
}
