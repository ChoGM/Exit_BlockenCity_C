using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SaveSystemTester : MonoBehaviour
{
    [Header("Input")]
    public TMP_InputField playerNameInput;

    [Header("State Text")]
    public TMP_Text stageText;
    public TMP_Text moneyText;

    [Header("UI Binder (선택)")]
    public ScoreUIBinder scoreUIBinder;

    private void Start()
    {
        Datamanager.Instance.LoadGameData();

        playerNameInput.text =
            Datamanager.Instance.saveData.player.playerName;

        RefreshStateText();
        //scoreUIBinder?.Refresh();
    }

    // 이름 저장
    public void OnClickSaveName()
    {
        Datamanager.Instance.saveData.player.playerName =
            playerNameInput.text;

        Datamanager.Instance.SaveGameData();
        RefreshAll();
    }

    // 돈 +100
    public void AddMoney()
    {
        Datamanager.Instance.saveData.player.totalMoney += 100;
        Datamanager.Instance.SaveGameData();
        RefreshAll();
    }

    // 스테이지 증가
    public void NextStage()
    {
        var progress = Datamanager.Instance.saveData.progress;

        if (progress.currentStage < 12)
        {
            progress.currentStage++;
            Datamanager.Instance.SaveGameData();
            RefreshAll();
        }
    }

    // UI 갱신 묶음
    private void RefreshAll()
    {
        RefreshStateText();
        //scoreUIBinder?.Refresh();
    }

    // Tester 자체 표시 갱신
    private void RefreshStateText()
    {
        var data = Datamanager.Instance.saveData;

        stageText.text =
            $"Stage : {data.progress.currentStage}";

        moneyText.text =
            $"Money : {data.player.totalMoney}";
    }
}
