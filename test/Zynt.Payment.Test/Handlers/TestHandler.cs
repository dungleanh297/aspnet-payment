namespace Zynt.Payment.Test;

public abstract class TestHandler : ITestHandler
{
    public HandlerState State { get; }

    protected TestHandler(HandlerStateCollection collection)
    {
        State = collection.Get(GetType());
    }
}