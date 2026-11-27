using UnityEngine;

public class SettingInGameManager : MonoBehaviour
{
    [Header("UI Button Handler")]
    [SerializeField] private UIButtonHandler uiButtonHandler;

    private const string PAUSE_REASON = "Settings";

    private void Start()
    {
        // 인스펙터에 직접 할당하지 않은 경우 씬 내에서 찾습니다.
        if (uiButtonHandler == null)
        {
            uiButtonHandler = FindObjectOfType<UIButtonHandler>();
        }
    }

    private void Update()
    {
        // ESC 키 입력 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    /// <summary>
    /// 일시정지 상태를 토글(ON/OFF)합니다.
    /// </summary>
    public void TogglePause()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.isGameEnded) return;

        if (GameManager.Instance.IsPauseRequested(PAUSE_REASON))
        {
            ResumeSettings();
        }
        else
        {
            PauseSettings();
        }
    }
    public void PauseSettings()
    {
        GameManager.Instance.AddPauseReason(PAUSE_REASON);

        if (uiButtonHandler != null &&
            uiButtonHandler.settingsWindow != null)
        {
            uiButtonHandler.settingsWindow.SetActive(true);
        }
    }

    public void ResumeSettings()
    {
        if (uiButtonHandler != null &&
            uiButtonHandler.settingsWindow != null)
        {
            uiButtonHandler.settingsWindow.SetActive(false);
        }

        GameManager.Instance.RemovePauseReason(PAUSE_REASON);
    }

}
