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
        ApplyCurrentSettings();
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
        ApplyCurrentSettings();
    }

    public void SetTextMedium()
    {
        StorySettingsManager.Instance.SetTextSize(StoryTextSize.Medium);
        ApplyCurrentSettings();
    }

    public void SetTextLarge()
    {
        StorySettingsManager.Instance.SetTextSize(StoryTextSize.Large);
        ApplyCurrentSettings();
    }

    public void SetAutoAdvance()
    {
        StorySettingsManager.Instance.SetAdvanceMode(StoryAdvanceMode.Auto);
        ApplyCurrentSettings();
    }

    public void SetManualAdvance()
    {
        StorySettingsManager.Instance.SetAdvanceMode(StoryAdvanceMode.Manual);
        ApplyCurrentSettings();
    }
}
