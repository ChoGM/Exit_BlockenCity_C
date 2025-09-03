using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "NewQuest/QuestData")]
public class NewQuestData : ScriptableObject
{
    public int branchID;    //  �귱ġ ��ȣ (acceptBranch�� ��ġ)
    public string questName;    //  �̸�
    [TextArea] public string description;   //  ����Ʈ ����
    public string reward;   //  ���� ����
    public float progress;  //  ���൵ 
    public Sprite factionBackgroundImage; // �Ҽ� ���� ��� �̹���
}