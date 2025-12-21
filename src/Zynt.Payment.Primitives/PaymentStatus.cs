namespace Zynt.Payment;

public enum PaymentStatus : int
{
    Success = 0,

    Pending = 1,

    CancelledByUser = 2,

    Expired = 3,
    
    Failure = 0x0FFF_FFFF,
}