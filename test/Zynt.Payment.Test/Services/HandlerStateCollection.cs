using System.Collections.Concurrent;

namespace Zynt.Payment.Test;

public class HandlerStateCollection
{
    private readonly ConcurrentDictionary<Type, HandlerState> _handlerStates = new();

    public int Count => _handlerStates.Count;

    public HandlerState Get(Type handlerType)
    {
        return _handlerStates.GetOrAdd(handlerType, _ => new HandlerState(handlerType));
    }

    public KeyValuePair<Type, HandlerState> Single()
    {
        return _handlerStates.Single();
    }
}