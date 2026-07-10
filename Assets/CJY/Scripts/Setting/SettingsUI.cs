using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Debug.Log("OnOpen");
        StartCoroutine(ApplySettingsNextFrame());
    }

    private IEnumerator ApplySettingsNextFrame()
    {
        Debug.Log("Coroutine Start");

        yield return null;

        Debug.Log("Coroutine Apply");

        storySettingsUI.ApplyCurrentSettings();
    }

    private void OnClose()
    {
        // 설정 닫힐 때 필요한 처리
    }
}
