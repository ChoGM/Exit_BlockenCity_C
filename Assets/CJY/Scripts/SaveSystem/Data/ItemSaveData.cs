using System;
using UnityEngine;

[Serializable]
public class ItemSaveData
{
    public int undoLastBlockCount = 0;

    public int bottomLayerClearCount = 0;

    public int bomb3x3Count = 0;


    public int GetCount(GameItemId itemId)
    {
        switch (itemId)
        {
            case GameItemId.UndoLastBlock:
                return undoLastBlockCount;

            case GameItemId.BottomLayerClear:
                return bottomLayerClearCount;

            case GameItemId.Bomb3x3:
                return bomb3x3Count;

            default:
                return 0;
        }
    }


    public void SetCount(
        GameItemId itemId,
        int count)
    {
        int safeCount =
            Mathf.Max(0, count);


        switch (itemId)
        {
            case GameItemId.UndoLastBlock:

                undoLastBlockCount =
                    safeCount;

                break;


            case GameItemId.BottomLayerClear:

                bottomLayerClearCount =
                    safeCount;

                break;


            case GameItemId.Bomb3x3:

                bomb3x3Count =
                    safeCount;

                break;
        }
    }
}