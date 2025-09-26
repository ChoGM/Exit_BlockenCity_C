using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewQuestUI : MonoBehaviour
{
    public Text questNameText;
    public Text descriptionText;
    public Text rewardText;
    public Text progressText;
    public Image factionImage;

    public void SetQuest(NewQuestData quest)
    {
        questNameText.text = quest.questName;
        descriptionText.text = quest.description;
        rewardText.text = $"����: {quest.reward}";
        progressText.text = $"���൵: {(quest.progress * 1f):0}%";

        if (quest.factionBackgroundImage != null)
        {
            factionImage.sprite = quest.factionBackgroundImage;
            factionImage.color = Color.white; // ���� ����
        }
        else  
        {
            factionImage.color = Color.clear; // �̹��� ���� ��� ����
        }

    }
}