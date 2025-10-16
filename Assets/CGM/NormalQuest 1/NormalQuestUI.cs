using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NormalQuestUI : MonoBehaviour
{
    public Text questNameText;
    public Text descriptionText;
    public int rewardText;
    public Text progressText;

    private int questID;

    private NormalQuest quest;

    // UI�� ����Ʈ ������ ���ε�
    public void SetQuest(NormalQuest quest)
    {
        this.quest = quest;
        questID = quest.questID;
        questNameText.text = quest.questName;
        descriptionText.text = quest.description;
        rewardText = quest.reward;
        progressText.text = $"{quest.currentCount}/{quest.targetCount}";
    }
    private void Update()
    {
        if (quest != null)
            progressText.text = $"{quest.currentCount}/{quest.targetCount}";
    }
    // �Ϸ� ��ư Ŭ�� �� ȣ��
    public void OnCompleteButton()
    {
        QuestManager.Instance.CompleteQuest(questID);
        Destroy(gameObject); // UI ����
    }
}