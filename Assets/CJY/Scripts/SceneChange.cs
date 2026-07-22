using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    // 버튼에서 호출할 함수
    public void LoadScene(string sceneName)
    {
        // 씬 전환
        SceneManager.LoadScene(sceneName);
    }

    // 게임오버 창의 '다시 하기' 버튼이 호출할 함수
    public void RetryCurrentStage()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.RestartStage();
        }
    }
}
