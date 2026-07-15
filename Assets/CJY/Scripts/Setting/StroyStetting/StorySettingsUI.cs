using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorySettingsUI : MonoBehaviour
{
    [Header("Text Size State Images")]
    public GameObject textSmallSelected;
    public GameObject textMediumSelected;
    public GameObject textLargeSelected;

    [Header("Advance Mode State Images")]
    public GameObject autoAdvanceSelected;
    public GameObject manualAdvanceSelected;

    private void Start()
    {
        // ===== [추가] 매니저의 값 변경 이벤트를 구독 =====
        if (StorySettingsManager.Instance != null)
        {
            StorySettingsManager.Instance.OnStorySettingsChanged += ApplyCurrentSettings;
        }

        ApplyCurrentSettings();
    }

    // 오브젝트가 파괴될 때 이벤트 구독을 해제해 줍니다 (메모리 누수 방지)
    private void OnDestroy()
    {
        if (StorySettingsManager.Instance != null)
        {
            StorySettingsManager.Instance.OnStorySettingsChanged -= ApplyCurrentSettings;
        }
    }

    public void ApplyCurrentSettings()
    {
        var manager = StorySettingsManager.Instance;
        if (manager == null) return;

        textSmallSelected.SetActive(manager.textSize == StoryTextSize.Small);
        textMediumSelected.SetActive(manager.textSize == StoryTextSize.Medium);
        textLargeSelected.SetActive(manager.textSize == StoryTextSize.Large);

        autoAdvanceSelected.SetActive(manager.advanceMode == StoryAdvanceMode.Auto);
        manualAdvanceSelected.SetActive(manager.advanceMode == StoryAdvanceMode.Manual);
    }

    public void SetTextSmall()
    {
        StorySettingsManager.Instance.SetTextSize(StoryTextSize.Small);
    }

    public void SetTextMedium()
    {
        StorySettingsManager.Instance.SetTextSize(StoryTextSize.Medium);
    }

    public void SetTextLarge()
    {
        StorySettingsManager.Instance.SetTextSize(StoryTextSize.Large);
    }

    public void SetAutoAdvance()
    {
        StorySettingsManager.Instance.SetAdvanceMode(StoryAdvanceMode.Auto);
    }

    public void SetManualAdvance()
    {
        StorySettingsManager.Instance.SetAdvanceMode(StoryAdvanceMode.Manual);
    }
}