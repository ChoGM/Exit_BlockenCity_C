using System;
using UnityEngine;

[Serializable]
public class StageData
{
    // 이번 스테이지에서 획득한 재화
    public int earnedMoney = 0;

    // 이번 스테이지에서 변화한 세력
    public float danwolDelta = 0;
    public float yaseoDelta = 0;
    public float macheonDelta = 0;
    public float hongryeonDelta = 0;
    public float merchantUnionDelta = 0;

    public void Reset()
    {
        earnedMoney = 0;

        danwolDelta = 0;
        yaseoDelta = 0;
        macheonDelta = 0;
        hongryeonDelta = 0;
        merchantUnionDelta = 0;
    }
}