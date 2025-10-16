using System.Collections;
using System.Collections.Generic;
using TetrisGame;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/NormalQuest")]
public class NormalQuest : ScriptableObject
{
    public int questID;            // ����Ʈ ���� ID
    public string questName;       // ����Ʈ �̸�
    [TextArea] public string description; // ����Ʈ ����
    public int reward;             // ����

    [Header("��ǥ ����")]
    public BlockType targetBlockType; // ��ǥ �� Ÿ��
    public int targetCount;           // �ʿ� ����

    [HideInInspector] public int currentCount; // ���� ���൵

    // �Ϸ� ����
    public bool IsCompleted => currentCount >= targetCount;

    // ���� �ۼ�Ʈ (UI ǥ�ÿ�)
    public int ProgressPercent => targetCount > 0 ? (int)((float)currentCount / targetCount * 100f) : 0;

    // ī��Ʈ ����
    public void AddProgress(BlockType type)
    {
        if (type == targetBlockType && !IsCompleted)
        {
            currentCount++;
        }
    }
}