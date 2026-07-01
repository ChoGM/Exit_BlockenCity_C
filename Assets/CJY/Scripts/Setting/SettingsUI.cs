using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class SettingsUI : MonoBehaviour
{
    [Header("Settings Panel")]
    public GameObject settingsPanel;

    [Header("Story Settings UI")]
    public StorySettingsUI storySettingsUI;

    private bool isOpen = false;

    private void Start()
    {
        // 시작할 때 설정창 닫아두기
        settingsPanel.SetActive(false);
        isOpen = false;
    }

    public void ToggleSettings()
    {
        isOpen = !isOpen;
        settingsPanel.SetActive(isOpen);

        if (isOpen)
            OnOpen();
        else
            OnClose();
    }

    public void OpenSettings()
    {
        isOpen = true;
        settingsPanel.SetActive(true);
        OnOpen();
    }

    public void CloseSettings()
    {
        isOpen = false;
        settingsPanel.SetActive(false);
        OnClose();
    }

    //private void OnOpen()
    //{
    //    // 설정 열릴 때 필요한 처리
    //    if (storySettingsUI != null)
    //        storySettingsUI.ApplyCurrentSettings();

    //}

    private void OnOpen()
    {
        StartCoroutine(ApplySettingsNextFrame());
    }

    private IEnumerator ApplySettingsNextFrame()
    {
        yield return null;   // 한 프레임 대기

        if (storySettingsUI != null)
            storySettingsUI.ApplyCurrentSettings();
    }

    private void OnClose()
    {
        // 설정 닫힐 때 필요한 처리
    }
}
