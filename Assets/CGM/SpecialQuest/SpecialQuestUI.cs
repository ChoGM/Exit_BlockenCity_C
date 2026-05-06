using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TetrisGame;

public class SpecialQuestUI : MonoBehaviour
{
    public static SpecialQuestUI CurrentUI;

    [Header("텍스트")]
    public TMP_Text questNameText;
    public TMP_Text descriptionText;
    public TMP_Text rewardText;
    public TMP_Text progressText;

    [Header("슬라이더")]
    public Slider time;
    public Image sliderFillImage;

    [Header("배경")]
    public Image backgroundImage;
    [SerializeField] private Sprite defaultBackground;

    private SpecialQuestData quest;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

    // =========================
    // 퀘스트 세팅
    // =========================
    public void SetQuest(SpecialQuestData quest)
    {
        CurrentUI = this;
        this.quest = quest;

        questNameText.text = quest.questName;
        descriptionText.text = quest.description;
        rewardText.text = $"+{quest.reward}";

        // 배경
        backgroundImage.sprite =
            quest.background != null ? quest.background : defaultBackground;

        // 슬라이더
        time.maxValue = quest.timeValue;
        time.value = quest.timeValue;
        sliderFillImage.color = quest.sliderColor;

        // 초기 진행도
        progressText.text = GetProgressText(0, false);

        Show();
    }

    // =========================
    // 진행도 업데이트 (핵심)
    // =========================
    public void UpdateProgress(int value, bool flag = false)
    {
        if (quest == null) return;

        progressText.text = GetProgressText(value, flag);
    }

    // =========================
    // 텍스트 생성 (핵심 로직)
    // =========================
    private string GetProgressText(int value, bool flag)
    {
        switch (quest.questType)
        {
            case SpecialQuestType.BlockBreak:
                return $"{value} / {quest.targetCount}";

            case SpecialQuestType.BlockNoBreak:
                return $"{quest.blockType} 블럭 파괴 시 실패";

            case SpecialQuestType.HeightLimit:
            case SpecialQuestType.HeightAchievement:
                return $"현재 높이 : {value + 1} / {quest.targetHeight}";

            case SpecialQuestType.HeightKeep:
                return $"높이 {value + 1} 유지 중";

            case SpecialQuestType.HeightSpecialBlock:
                return $"높이 : {value + 1} / {quest.targetHeight}\n" +
                       $"{quest.blockType} {(flag ? "✔" : "✘")}";

            case SpecialQuestType.InputRestriction:
                return $"{quest.restrictedInput} 제한됨";

            case SpecialQuestType.ViewObstruction:
                return $"시야 제한 높이 : {quest.targetHeight}";

            default:
                return "";
        }
    }

    // =========================
    // 타이머
    // =========================
    public void UpdateTimer(float value)
    {
        time.value = value;
    }

    // =========================
    // 완료
    // =========================
    public void ShowCompleted()
    {
        progressText.text = "퀘스트 완료!";
        StartCoroutine(AutoClose());
    }

    private IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
        CurrentUI = null;
    }

    private void Show()
    {
        StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        yield return new WaitForEndOfFrame();
        canvasGroup.alpha = 1f;
    }
}