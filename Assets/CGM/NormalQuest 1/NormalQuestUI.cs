using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NormalQuestUI : MonoBehaviour
{
    public TMP_Text questNameText;
    public TMP_Text descriptionText;
    public TMP_Text rewardText;
    public TMP_Text progressText;

    private int questID;

    private NormalQuest quest;

    [Header("요약 UI")]
    [SerializeField] private GameObject summaryPanel;
    [SerializeField] private TMP_Text summaryName;
    [SerializeField] private Slider summarySlider;

    [Header("상세 UI")]
    [SerializeField] private GameObject detailPanel;

    // UI에 퀘스트 데이터 바인딩
    public void SetQuest(NormalQuest quest)
    {
        this.quest = quest;

        questID = quest.questID;

        questNameText.text = quest.questName;
        descriptionText.text = quest.Description;
        rewardText.text = $"{quest.Reward} G";
        progressText.text = $"{quest.currentCount}/{quest.targetCount}";
    }
    private void Update()
    {
        if (quest == null)
            return;

        progressText.text = $"{quest.currentCount}/{quest.targetCount}";
        descriptionText.text = quest.Description;
        rewardText.text = $"{quest.Reward} G";

        summarySlider.maxValue = quest.targetCount;
        summarySlider.value = quest.currentCount;
    }
    // 완료 버튼 클릭 시 호출
    public void OnCompleteButton()
    {
        QuestManager.Instance.CompleteQuest(questID);
        Destroy(gameObject); // UI 제거
    }

    public void SetExpanded(bool expanded)
    {
        detailPanel.SetActive(expanded);
        summaryPanel.SetActive(!expanded);

        if (!expanded)
        {
            summaryName.text = quest.questName;

            summarySlider.maxValue = quest.targetCount;
            summarySlider.value = quest.currentCount;
        }
    }
}