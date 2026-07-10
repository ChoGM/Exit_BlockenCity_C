using System.Collections;
using System.Collections.Generic;
using TetrisGame;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/NormalQuest")]
public class NormalQuest : ScriptableObject
{
    [Header("기본 조건")]
    public int questID;            // 퀘스트 고유 ID
    public string questName;       // 퀘스트 이름
    [TextArea] public string description; // 퀘스트 내용

    [Header("목표 조건")]
    public BlockType targetBlockType; // 목표 블럭 타입
    public int targetCount;           // 필요 갯수
    public int currentCount;          // 현재 파괴한 개수

    public int Reward
    {
        get
        {
            const float minTarget = 5f;
            const float maxTarget = 20f;

            const float minReward = 100f;
            const float maxReward = 1000f;

            float t = (targetCount - minTarget) / (maxTarget - minTarget);

            t = Mathf.Pow(t, 1.2f);

            return Mathf.RoundToInt(Mathf.Lerp(minReward, maxReward, t));
        }
    }
    public string Description
    {
        get
        {
            return $"{targetBlockType} 블럭 {targetCount}개 파괴";
        }
    }

    [HideInInspector] public bool IsCompleted = false;

    // 진행 퍼센트 (UI 표시용)
    public int ProgressPercent => targetCount > 0 ? (int)((float)currentCount / targetCount * 100f) : 0;

    // 카운트 증가

    public void AddProgress(BlockType destroyedType)
    {
        if (IsCompleted) return;

        if (destroyedType == targetBlockType)
        { 
            currentCount++;

            Debug.Log($"[{questName}] {targetBlockType} 파괴 진행: {currentCount}/{targetCount}");

            if (currentCount >= targetCount)
            {
                IsCompleted = true;
                Debug.Log($"[{questName}] 퀘스트 완료!");
                QuestManager.Instance.CompleteQuest(questID);
            }

        }
    }
}