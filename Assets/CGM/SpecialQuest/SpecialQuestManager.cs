using System.Collections.Generic;
using TetrisGame;
using UnityEngine;

public class SpecialQuestManager : MonoBehaviour
{
    public static SpecialQuestManager Instance;

    public Transform questUIParent;
    public GameObject questUIPrefab;

    private readonly List<SpecialQuestInstance> activeQuests = new();
    private readonly List<SpecialQuestTimer> activeTimers = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        for (int i = 0; i < activeTimers.Count; i++)
        {
            activeTimers[i].Update(Time.deltaTime);
        }
    }

    // ===============================
    // 퀘스트 생성
    // ===============================
    public void AddQuest(SpecialQuestData questData)
    {
        var instance = new SpecialQuestInstance
        {
            data = questData
        };

        // UI 생성
        GameObject uiObj = Instantiate(questUIPrefab, questUIParent);
        instance.ui = uiObj.GetComponent<SpecialQuestUI>();
        instance.ui.SetQuest(questData);

        activeQuests.Add(instance);

        // 타이머 생성
        if (questData.HasTimeLimit())
        {
            var timer = new SpecialQuestTimer(instance);
            activeTimers.Add(timer);
        }

        // 타입별 초기화
        if (questData.questType == SpecialQuestType.HeightKeep)
        {
            instance.baseHeight = TetrisManager.Instance.tower.GetCurrentHeight();
        }

        if (questData.questType == SpecialQuestType.InputRestriction)
        {
            var inputs = questData.restrictedInput.Split(',');
            InputManager.Instance.SetRestrictedInputs(inputs);
        }

        Debug.Log($"[SpecialQuest] 시작: {questData.questName}");
    }

    // ===============================
    // 퀘스트 완료 / 실패
    // ===============================
    private void CompleteQuest(SpecialQuestInstance instance)
    {
        if (instance.isFinished) return;
        instance.isFinished = true;

        Debug.Log($"[SpecialQuest] 성공: {instance.data.questName}");

        if (instance.data.questType == SpecialQuestType.InputRestriction)
        {
            InputManager.Instance.ClearRestrictedInputs();
        }

        instance.ui?.ShowCompleted();

        Cleanup(instance);

    }

    private void FailQuest(SpecialQuestInstance instance)
    {
        if (instance.isFinished) return;
        instance.isFinished = true;

        Debug.Log($"[SpecialQuest] 실패: {instance.data.questName}");

        Destroy(instance.ui?.gameObject);

        Cleanup(instance);

    }

    private void Cleanup(SpecialQuestInstance instance)
    {
        activeTimers.RemoveAll(t => t.IsFor(instance));
        activeQuests.Remove(instance);
    }

    public void OnTimeExpired(SpecialQuestInstance instance)
    {
        if (instance.isFinished) return;

        Debug.Log(
            $"[SpecialQuest] 시간 종료 | 퀘스트:{instance.data.questName} | Role:{instance.data.timeRole}"
        );

        switch (instance.data.timeRole)
        {
            case QuestTimeRole.Survival:
                //시간 버티기 → 성공
                CompleteQuest(instance);
                break;

            case QuestTimeRole.TimeLimit:
                //제한 시간 초과 → 실패
                FailQuest(instance);
                break;

            case QuestTimeRole.None:
            default:
                // 안전장치로 실패 처리
                FailQuest(instance);
                break;
        }
    }

    public void OnBlockDestroyed(BlockType destroyedType)
    {
        foreach (var instance in activeQuests)
        {
            if (instance.isFinished) continue;
            if (instance.data.questType != SpecialQuestType.BlockBreak) continue;
            if (destroyedType != instance.data.blockType) continue;

            instance.breakCount++;

            instance.ui?.UpdateProgress(instance.breakCount);

            if (instance.breakCount >= instance.data.targetCount)
            {
                CompleteQuest(instance);
            }
        }
    }

    public void OnBlockDropped()
    {
        for (int i = activeTimers.Count - 1; i >= 0; i--)
        {
            activeTimers[i].OnBlockDropped();
        }
    }

    public void OnHeightChanged(int currentHeight)
    {
        for (int i = activeQuests.Count - 1; i >= 0; i--)
        {
            var instance = activeQuests[i];

            if (instance.isFinished) continue;

            // 👉 높이 관련 퀘스트만 UI 업데이트
            switch (instance.data.questType)
            {
                case SpecialQuestType.HeightKeep:
                case SpecialQuestType.HeightLimit:
                case SpecialQuestType.HeightAchievement:
                    instance.ui?.UpdateProgress(currentHeight);
                    break;
            }

            // 👉 판정 로직
            switch (instance.data.questType)
            {
                case SpecialQuestType.HeightKeep:
                    if (currentHeight != instance.baseHeight)
                        FailQuest(instance);
                    break;

                case SpecialQuestType.HeightLimit:
                    if (currentHeight + 1 > instance.data.targetHeight)
                        FailQuest(instance);
                    break;

                case SpecialQuestType.HeightAchievement:
                    if (currentHeight + 1 >= instance.data.targetHeight)
                        CompleteQuest(instance);
                    break;
            }
        }
    }

    public void OnBlockPlaced(Vector3Int pos, BlockType type)
    {
        foreach (var instance in activeQuests)
        {
            if (instance.isFinished) continue;
            if (instance.data.questType != SpecialQuestType.HeightSpecialBlock)
                continue;

            bool installed =
                pos.y + 1 == instance.data.targetHeight &&
                type == instance.data.blockType;

            instance.ui?.UpdateProgress(
            TetrisManager.Instance.tower.GetCurrentHeight(),
            installed
            );

            if (installed)
                CompleteQuest(instance);
        }
    }
}
