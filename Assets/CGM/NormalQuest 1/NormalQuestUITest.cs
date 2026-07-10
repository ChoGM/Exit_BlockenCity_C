using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NormalQuestUITest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Test1()
    {
        QuestManager.Instance.CompleteQuest(1);
    }

    public void Test2()
    {
        QuestManager.Instance.CompleteQuest(2);
    }

    public void Test3()
    {
        QuestManager.Instance.CompleteQuest(3);
    }

    public void Test4()
    {
        QuestManager.Instance.CompleteQuest(4);
    }

    public void Test5()
    {
        QuestManager.Instance.CompleteQuest(5);
    }

    public void InitializeGold()
    {
        //DataManager.Instance.data.Gold = 0;
        Debug.Log("∞ÒµÂ √ ±‚»≠");
    }

}
