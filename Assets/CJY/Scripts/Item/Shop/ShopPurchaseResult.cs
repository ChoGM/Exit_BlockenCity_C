public enum ShopPurchaseFailureReason
{
    None,

    NotEnoughMoney,
    InvalidItem,
    InvalidFaction,
    InvalidState
}


public struct ShopPurchaseResult
{
    public bool Success { get; }

    public ShopPurchaseFailureReason FailureReason { get; }

    public string Message { get; }


    private ShopPurchaseResult(
        bool success,
        ShopPurchaseFailureReason failureReason,
        string message)
    {
        Success = success;
        FailureReason = failureReason;
        Message = message;
    }


    public static ShopPurchaseResult Succeed(
        string message = "")
    {
        return new ShopPurchaseResult(
            true,
            ShopPurchaseFailureReason.None,
            message
        );
    }


    public static ShopPurchaseResult Fail(
        ShopPurchaseFailureReason reason,
        string message)
    {
        return new ShopPurchaseResult(
            false,
            reason,
            message
        );
    }
}