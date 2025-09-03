using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/NormalQuest")]
public class NormalQuest : ScriptableObject
{
    public int questID;            // ����Ʈ ���� ID
    public string questName;       // ����Ʈ �̸�
    [TextArea] public string description; // ����Ʈ ����
    public int progress;           // ���൵ (0 ~ ��ǥġ)
    public int reward;          // ���� ����

    // ���� ���� üũ
    public bool IsCompleted => progress >= 100; // ����: 100% ���� �Ϸ�
}