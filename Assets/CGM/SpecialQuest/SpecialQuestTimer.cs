using UnityEngine;
public class SpecialQuestTimer
{
    private readonly SpecialQuestInstance quest;

    public SpecialQuestTimer(SpecialQuestInstance quest)
    {
        this.quest = quest;

        if (quest.data.timeType == TimeType.Seconds)
            quest.remainSeconds = quest.data.timeValue;

        if (quest.data.timeType == TimeType.BlockDropCount)
            quest.remainDropCount = quest.data.timeValue;
    }

    public void Update(float deltaTime)
    {
        if (quest.isFinished) return;
        if (quest.data.timeType != TimeType.Seconds) return;

        // 게임이 Pause 상태라면 타이머 정지
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused())
        {
            return;
        }

        quest.remainSeconds -= deltaTime;

        quest.ui?.UpdateTimer(quest.remainSeconds);

        if (quest.remainSeconds <= 0)
        {
            quest.remainSeconds = 0;
            Finish();
        }
    }
    public void OnBlockDropped()
    {
        if (quest.isFinished) return;
        if (quest.data.timeType != TimeType.BlockDropCount) return;

        quest.remainDropCount--;
        quest.ui?.UpdateTimer(quest.remainDropCount);

        if (quest.remainDropCount <= 0)
            Finish();
    }

    private void Finish()
    {
        SpecialQuestManager.Instance.OnTimeExpired(quest);

        Debug.Log( $"[SpecialQuestTimer] 타이머 종료 | 퀘스트: {quest.data.questName}");
    }

    public bool IsFor(SpecialQuestInstance instance)
    {
        return quest == instance;
    }
}
