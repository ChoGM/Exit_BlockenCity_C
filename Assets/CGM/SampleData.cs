using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleData : MonoBehaviour
{
    void Start()
    {
        DataManager.Instance.LoadGameData();

        Debug.Log("���� ������ ���� ���: " + Application.persistentDataPath);

    }

    void Update()
    {
    }

    //������ �����ϸ� �ڵ�����
    private void OnApplicationQuit()
    {
        DataManager.Instance.SaveGameData();
    }

}
