
namespace PStrategies.ZoneRecovery
{
    /// <summary>
    /// An enumeration used for expressing the phase in which the Zone Recovery algorithm is at a given moment.
    /// </summary>
    public enum ZoneRecoveryStatus {
        Init,
        Winding,
        Unwinding,
        Finish,
        Alert,
        Undefined
    }

    /// <summary>
    /// An enumeration that represents the status of an order as known in the application.
    /// New     = Resting order
    /// Filled  = Filled order
    /// Cancel  = Canceled order
    /// Error   = Error occurred at the time the order was being transferred to the server.
    /// </summary>
    public enum ZoneRecoveryOrderStatus
    {
        New,
        Filled,
        PartiallyFilled,
        Canceled,
        Rejected,
        Error,
        Undefined
    }

    public enum ZoneRecoveryOrderBatchStatus
    {
        Init,               // Orders have just been created
        PartialResting,     // Orders have just been created, some are resting, other(s) to be evaluated
        Resting,            // All orders are resting
        OrderFilled,        // One or more orders is filled, action should be taken
        Alert,
        Undefined
    }

    public enum ZoneRecoveryBatchType
    {
        PeggedStart,
        WindingFirst,
        WindingUp,
        WindingDown,
        UnwindingUp,
        UnwindingDown,
        UnwindingLast
    }

    public enum ZoneRecoveryBatchStatus
    {
        Error,
        Working,
        Waiting,
        ReadyForNext,
        Closed
    }

    /// <summary>
    /// The direction in which profits (or lesser loss) can be made for the current positions.
    /// </summary>
    public enum ZoneRecoveryDirection
    {
        Undefined,
        Up,
        Down
    }

    /// <summary>
    /// An enumeration that represents the type of order within the Zone Recovery strategy.
    /// TP  = Take Profit in Profit
    /// TL  = Take Loss
    /// REV = Reverse
    /// </summary>
    public enum ZoneRecoveryOrderType
    {
        TP,     //Take profit
        TL,     //Take loss
        REV,    //Reverse
        FS      //First served
    }

    public enum ZoneRecoveryAccount
    {
        A,
        B
    }
}
