using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    // ��ư���� ȣ���� �Լ�
    public void LoadScene(string sceneName)
    {
        // �� ��ȯ
        SceneManager.LoadScene(sceneName);
    }
}
