using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIButtonHandler : MonoBehaviour
{
    public enum ButtonType
    {
        GoToLobby,
        RetryStage,
        NextResultScene,
        PauseGame,
        ResumeGame
    }

    public ButtonType buttonType;

    [Header("UI Window (Pause/Resume 시 켜고 끌 창)")]
    public GameObject settingsWindow;

    private void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn == null) return;

        btn.onClick.AddListener(() => {
            switch (buttonType)
            {
                case ButtonType.GoToLobby:
                    SceneManager.LoadScene("Lobby");
                    break;

                case ButtonType.RetryStage:
                    if (StageManager.Instance != null)
                        StageManager.Instance.RestartStage();
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.ResetGame();
                    }

                    break;

                case ButtonType.NextResultScene:
                    if (GameManager.Instance != null && GameManager.Instance.scoreManager != null)
                    {
                        GameManager.Instance.scoreManager.OnNextButtonClick();
                    }
                    break;

                case ButtonType.PauseGame:
                    PauseGame();
                    break;

                case ButtonType.ResumeGame:
                    ResumeGame();
                    break;
            }
        });
    }

    public void PauseGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }

        if (settingsWindow != null)
        {
            settingsWindow.SetActive(true);
        }

        Debug.Log("[PauseGame] 일시정지 완료");
    }

    public void ResumeGame()
    {
        if (settingsWindow != null)
        {
            settingsWindow.SetActive(false);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }

        Debug.Log("[ResumeGame] 게임 재개");
    }
}