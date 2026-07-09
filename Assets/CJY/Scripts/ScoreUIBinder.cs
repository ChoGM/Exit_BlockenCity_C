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

    public void Refresh()
    {
        var saveData = Datamanager.Instance.saveData;
        var stageData = StageManager.Instance.stageData;

        // 플레이어 이름
        playerNameText.text = saveData.player.playerName;
        playerNameText2.text = saveData.player.playerName;

        // 월급 (이번 스테이지 획득)
        salaryText.text = stageData.earnedMoney.ToString();

        // 총 자산 (현재 자산 + 이번 월급)
        int totalMoney = saveData.player.totalMoney + stageData.earnedMoney;
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
}