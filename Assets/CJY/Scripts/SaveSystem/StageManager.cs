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
        stageData.JeonSangYeonDelta += amount;
    }

    public void OverStage()
    {
        stageData.Reset();
    }

    public void ClearStage()
    {
        var save = Datamanager.Instance.saveData;

        int baseSalary =
            10000 + ((save.progress.currentStage - 1) / 3) * 5000;

        save.player.totalMoney += baseSalary + stageData.earnedMoney;

        save.relationship.danwol += stageData.danwolDelta;
        save.relationship.yaseo += stageData.yaseoDelta;
        save.relationship.macheon += stageData.macheonDelta;
        save.relationship.hongryeon += stageData.hongryeonDelta;
        save.relationship.JeonSangYeon += stageData.JeonSangYeonDelta;

        Datamanager.Instance.SaveGameData();

        stageData.Reset();
    }

    public void RestartStage()
    {
        StartStage();

        // 현재 열려있는 인게임 씬의 이름을 자동으로 가져와 다시 로드합니다.
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
    }
}