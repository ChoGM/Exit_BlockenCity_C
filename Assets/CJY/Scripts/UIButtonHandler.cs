using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class UIButtonHandler : MonoBehaviour
{
    public enum ButtonType
    {
        GoToLobby,
        RetryStage,
        NextResultScene // 추가
    }

    public ButtonType buttonType;

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
                    break;

                case ButtonType.NextResultScene: // 클리어/오버 분기 버튼
                    if (GameManager.Instance != null && GameManager.Instance.scoreManager != null)
                    {
                        GameManager.Instance.scoreManager.OnNextButtonClick();
                    }
                    break;
            }
        });
    }
}