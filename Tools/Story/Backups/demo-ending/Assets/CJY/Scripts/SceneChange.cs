using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    [Header("Title Scene Navigation")]
    [SerializeField] private string nicknameSceneName = "Nickname";
    [SerializeField] private string lobbySceneName = "Lobby";


    // 일반 씬 전환
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadInGameScene()
    {
        SceneManager.LoadScene("InGame");
    }

    // =========================
    // Title
    // =========================

    public void ContinueGame()
    {
        if (!Datamanager.Instance.HasValidSaveData())
        {
            Debug.LogWarning(
                "이어하기 가능한 세이브 데이터가 없습니다.");

            return;
        }

        Datamanager.Instance.LoadGameData();

        SceneManager.LoadScene(lobbySceneName);
    }


    public void NewGame()
    {
        Datamanager.Instance.DeleteGameData();

        SceneManager.LoadScene(nicknameSceneName);
    }


    // =========================
    // Retry
    // =========================

    public void RetryCurrentStage()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.RestartStage();
        }
    }


    // =========================
    // Quit
    // =========================

    public void QuitGame()
    {
        Debug.Log("게임 종료");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}