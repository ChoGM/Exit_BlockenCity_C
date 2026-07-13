using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreUIBinder : MonoBehaviour
{
    [Header("Player")]
    public TMP_Text playerNameText;
    public TMP_Text playerNameText2;

    [Header("Money")]
    public TMP_Text totalMoneyText;
    public TMP_Text salaryText;

    [Header("Relationship - Danwol")]
    public Slider danwolSlider;
    public TMP_Text danwolStateText;

    [Header("Relationship - Yaseo")]
    public Slider yaseoSlider;
    public TMP_Text yaseoStateText;

    [Header("Relationship - Macheon")]
    public Slider macheonSlider;
    public TMP_Text macheonStateText;

    [Header("Relationship - Hongryeon")]
    public Slider hongryeonSlider;
    public TMP_Text hongryeonStateText;

    [Header("Relationship - JeonSangYeon")]
    public Slider jeonsangyeonSlider;
    public TMP_Text jeonsangyeonStateText;

    [Header("UI Window Object")]
    public GameObject scoreWindowObject;

    [Header("Date")]
    public TMP_Text dateText;

    //public static ScoreUIBinder Instance;

    public void Refresh()
    {
        var saveData = Datamanager.Instance.saveData;
        var stageData = StageManager.Instance.stageData;

        // 날짜 표시 (예: 01.00)
        dateText.text = $"{saveData.progress.currentStage:00}.00.";

        // 플레이어 이름
        playerNameText.text = saveData.player.playerName;
        playerNameText2.text = saveData.player.playerName;

        int baseSalary = GetBaseSalary(saveData.progress.currentStage);

        int totalSalary = baseSalary + stageData.earnedMoney;

        // 월급 표시
        //salaryText.text = $"{baseSalary} + {stageData.earnedMoney}";

        // 월급 표시
        salaryText.text = totalSalary.ToString();

        // 총 자산 표시
        int totalMoney = saveData.player.totalMoney + totalSalary;
        totalMoneyText.text = totalMoney.ToString();

        // ===== 세력 =====
        RefreshRelationship(
            danwolSlider,
            danwolStateText,
            saveData.relationship.danwol + stageData.danwolDelta);

        RefreshRelationship(
            yaseoSlider,
            yaseoStateText,
            saveData.relationship.yaseo + stageData.yaseoDelta);

        RefreshRelationship(
            macheonSlider,
            macheonStateText,
            saveData.relationship.macheon + stageData.macheonDelta);

        RefreshRelationship(
            hongryeonSlider,
            hongryeonStateText,
            saveData.relationship.hongryeon + stageData.hongryeonDelta);

        RefreshRelationship(
            jeonsangyeonSlider,
            jeonsangyeonStateText,
            saveData.relationship.JeonSangYeon + stageData.JeonSangYeonDelta);
    }

    private void RefreshRelationship(Slider slider, TMP_Text stateText, float value)
    {
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = Mathf.Clamp(value, 0, 100);

        stateText.text = GetRelationshipState(value);
    }

    private string GetRelationshipState(float value)
    {
        if (value < 0)
            return "적대";

        if (value <= 30)
            return "무관심";

        if (value <= 60)
            return "중립";

        if (value <= 80)
            return "호의";

        if (value <= 99)
            return "친밀";

        return "동맹";
    }

    // ===== 타이머 종료 시 매니저가 호출할 함수 =====
    public void ToggleScoreUI(bool isActive)
    {
        // 1. 매니저가 가진 구멍에 연결된 실제 UI 오브젝트를 켜거나 끕니다.
        if (scoreWindowObject != null)
        {
            scoreWindowObject.SetActive(isActive);
        }

        // 2. 켜는 거라면 데이터도 최신으로 갱신해 줍니다.
        if (isActive)
        {
            Refresh();
        }
    }

    private int GetBaseSalary(int currentStage)
    {
        // 1~3월 : 10000
        // 4~6월 : 15000
        // 7~9월 : 20000
        // 10~12월 : 25000

        return 10000 + ((currentStage - 1) / 3) * 5000;
    }
}