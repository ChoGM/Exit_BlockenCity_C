using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogEvent : MonoBehaviour
{
    public DialogUI dialogUI;

    public void Evnet10()
    {
        dialogUI.StartDialog(10); // branch 2 ����
    }
    public void Evnet1000()
    {
        dialogUI.StartDialog(1000); // branch 2 ����
    }
    public void accept()
    {
        Debug.Log("����"); // branch 2 ����
    }
    public void refuse()
    {
        Debug.Log("����"); // branch 2 ����
    }
}