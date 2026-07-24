using System.Collections.Generic;
using UnityEngine;

public class QuestDisplayManager : MonoBehaviour
{
    public static QuestDisplayManager Instance;

    [Header("부모")]
    [SerializeField] private Transform detailRoot;
    [SerializeField] private Transform summaryRoot;

    // 현재 퀘스트 순서
    private List<MonoBehaviour> questList = new List<MonoBehaviour>();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Rotate();
        }
    }

    //--------------------------------------------------
    // 등록
    //--------------------------------------------------

    public void Register(MonoBehaviour ui)
    {
        questList.Add(ui);

        Refresh();
    }

    //--------------------------------------------------
    // 제거
    //--------------------------------------------------

    public void Remove(MonoBehaviour ui)
    {
        questList.Remove(ui);

        Refresh();
    }

    //--------------------------------------------------
    // TAB
    //--------------------------------------------------

    public void Rotate()
    {
        if (questList.Count <= 1)
            return;

        MonoBehaviour last = questList[questList.Count - 1];

        questList.RemoveAt(questList.Count - 1);

        questList.Insert(0, last);

        Refresh();
    }

    //--------------------------------------------------
    // 새로 배치
    //--------------------------------------------------

    private void Refresh()
    {
        for (int i = 0; i < questList.Count; i++)
        {
            bool detail = (i == 0);

            MonoBehaviour ui = questList[i];

            if (detail)
                ui.transform.SetParent(detailRoot, false);
            else
                ui.transform.SetParent(summaryRoot, false);

            NormalQuestUI normal = ui as NormalQuestUI;

            if (normal != null)
            {
                normal.SetExpanded(detail);
                continue;
            }

            SpecialQuestUI special = ui as SpecialQuestUI;

            if (special != null)
            {
                special.SetExpanded(detail);
            }
        }
    }
}