namespace Zynt.Payment;

[Flags]
internal enum HandlerImplementationTypes
{
    None = 0,
    
    OnSuccess = 1,

    Disposable = 2,

    AsyncDisposable = 4,
    
}