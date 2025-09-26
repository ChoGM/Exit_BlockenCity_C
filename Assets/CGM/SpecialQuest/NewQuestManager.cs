using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewQuestManager : MonoBehaviour
{
    public static NewQuestManager Instance;

    public Transform questUIParent;          // ����Ʈ UI�� ���� �θ� ������Ʈ.
    public GameObject questUIPrefab;         // ����Ʈ�� ǥ���� ������ (NewQuestUI ��ũ��Ʈ ����)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        //else Destroy(gameObject);
    }

    public void AddQuest(NewQuestData quest)
    {
        GameObject questUIObj = Instantiate(questUIPrefab, questUIParent);

        // NewQuestUI ������Ʈ�� �����ؼ� ����
        NewQuestUI questUI = questUIObj.GetComponent<NewQuestUI>();
        if (questUI != null)
        {
            questUI.SetQuest(quest);
        }
        else
        {
            Debug.LogWarning("NewQuestUI ������Ʈ�� ã�� �� �����ϴ�!");
        }
    }
}