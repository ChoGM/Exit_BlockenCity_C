using System.Collections;
using System.Collections.Generic;
using TetrisGame;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/NormalQuest")]
public class NormalQuest : ScriptableObject
{
    [Header("�⺻ ����")]
    public int questID;            // ����Ʈ ���� ID
    public string questName;       // ����Ʈ �̸�
    [TextArea] public string description; // ����Ʈ ����

    [Header("��ǥ ����")]
    public BlockType targetBlockType; // ��ǥ �� Ÿ��
    public int targetCount;           // �ʿ� ����
    public int currentCount;          // ���� �ı��� ����

    [Header("����")]
    public int reward;             // ����
                                   // �Ϸ� ����
    [HideInInspector] public bool IsCompleted = false;

    // ���� �ۼ�Ʈ (UI ǥ�ÿ�)
    public int ProgressPercent => targetCount > 0 ? (int)((float)currentCount / targetCount * 100f) : 0;

    // ī��Ʈ ����

    public void AddProgress(BlockType destroyedType)
    {
        if (IsCompleted) return;

        if (destroyedType == targetBlockType)
        { 
            currentCount++;

            Debug.Log($"[{questName}] {targetBlockType} �ı� ����: {currentCount}/{targetCount}");

            if (currentCount >= targetCount)
            {
                IsCompleted = true;
                Debug.Log($"[{questName}] ����Ʈ �Ϸ�!");
                QuestManager.Instance.CompleteQuest(questID);
            }

        }
    }
}