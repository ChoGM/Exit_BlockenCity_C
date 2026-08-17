using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemUseFailureReason
{
    None,

    // °øÅë
    NotOwned,
    EffectNotFound,
    InvalidState,

    // µÇµ¹¸®±â
    NoUndoTarget,
    AlreadyUndone,
    BoardChanged,

    // ÆøÅº
    NoActiveBlock,

    // ±âÅ¸
    Unknown
}


public struct ItemUseResult
{
    public bool Success { get; }

    public ItemUseFailureReason FailureReason { get; }

    public string Message { get; }


    private ItemUseResult(
        bool success,
        ItemUseFailureReason failureReason,
        string message)
    {
        Success = success;
        FailureReason = failureReason;
        Message = message;
    }


    public static ItemUseResult Succeed(string message = "")
    {
        return new ItemUseResult(
            true,
            ItemUseFailureReason.None,
            message
        );
    }


    public static ItemUseResult Fail(
        ItemUseFailureReason reason,
        string message)
    {
        return new ItemUseResult(
            false,
            reason,
            message
        );
    }
}
