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
    }
    // 완료 버튼 클릭 시 호출
    public void OnCompleteButton()
    {
        QuestManager.Instance.CompleteQuest(questID);
        Destroy(gameObject); // UI 제거
    }
}