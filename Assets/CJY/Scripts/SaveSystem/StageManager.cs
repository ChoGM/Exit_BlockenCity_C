using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public StageData stageData = new StageData();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartStage()
    {
        stageData.Reset();
    }

    public void AddMoney(int amount)
    {
        stageData.earnedMoney += amount;
    }

    public void AddDanwol(float amount)
    {
        stageData.danwolDelta += amount;
    }

    public void AddYaseo(float amount)
    {
        stageData.yaseoDelta += amount;
    }

    public void AddMacheon(float amount)
    {
        stageData.macheonDelta += amount;
    }

    public void AddHongryeon(float amount)
    {
        stageData.hongryeonDelta += amount;
    }

    public void AddMerchant(float amount)
    {
        stageData.merchantUnionDelta += amount;
    }

    public void ClearStage()
    {
        var save = Datamanager.Instance.saveData;

        save.player.totalMoney += stageData.earnedMoney;

        save.relationship.danwol += stageData.danwolDelta;
        save.relationship.yaseo += stageData.yaseoDelta;
        save.relationship.macheon += stageData.macheonDelta;
        save.relationship.hongryeon += stageData.hongryeonDelta;
        save.relationship.merchantUnion += stageData.merchantUnionDelta;

        Datamanager.Instance.SaveGameData();

        stageData.Reset();
    }
}