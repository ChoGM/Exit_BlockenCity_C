using UnityEngine;

public class QuestDisplayManager : MonoBehaviour
{
    private int lastCount = -1;

    [SerializeField] private Transform questList;

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Rotate();
        }

        if (lastCount != questList.childCount)
        {
            lastCount = questList.childCount;
            Refresh();
        }
    }

    public void Rotate()
    {
        if (questList.childCount <= 1)
            return;

        // 맨 아래를 맨 위로
        Transform last = questList.GetChild(questList.childCount - 1);
        last.SetAsFirstSibling();

        Refresh();
    }

    public void Refresh()
    {
        for (int i = 0; i < questList.childCount; i++)
        {
            Transform child = questList.GetChild(i);

            bool expanded = (i == 0);

            NormalQuestUI normal = child.GetComponent<NormalQuestUI>();
            if (normal != null)
            {
                normal.SetExpanded(expanded);
                continue;
            }

            SpecialQuestUI special = child.GetComponent<SpecialQuestUI>();
            if (special != null)
            {
                special.SetExpanded(expanded);
            }
        }
    }
}