using System; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorySettingsManager : MonoBehaviour
{
    public static StorySettingsManager Instance;

    // ===== [추가] 스토리 설정이 바뀔 때 외부에 알려줄 이벤트 =====
    public event Action OnStorySettingsChanged;

    [Header("Current Settings")]
    public StoryTextSize textSize = StoryTextSize.Medium;
    public StoryAdvanceMode advanceMode = StoryAdvanceMode.Manual;

    private const string TEXT_SIZE_KEY = "Story_TextSize";
    private const string ADVANCE_MODE_KEY = "Story_AdvanceMode";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    public void SetTextSize(StoryTextSize size)
    {
        textSize = size;
        PlayerPrefs.SetInt(TEXT_SIZE_KEY, (int)size);

        // ===== [추가] 데이터가 변경되었음을 UI에 알림 =====
        OnStorySettingsChanged?.Invoke();
    }

    public void SetAdvanceMode(StoryAdvanceMode mode)
    {
        advanceMode = mode;
        PlayerPrefs.SetInt(ADVANCE_MODE_KEY, (int)mode);

        // ===== [추가] 데이터가 변경되었음을 UI에 알림 =====
        OnStorySettingsChanged?.Invoke();
    }

    public StoryTextSize GetTextSize()
    {
        return textSize;
    }

    public bool IsAutoAdvance()
    {
        return advanceMode == StoryAdvanceMode.Auto;
    }

    private void LoadSettings()
    {
        textSize = (StoryTextSize)PlayerPrefs.GetInt(
            TEXT_SIZE_KEY, (int)StoryTextSize.Medium);

        advanceMode = (StoryAdvanceMode)PlayerPrefs.GetInt(
            ADVANCE_MODE_KEY, (int)StoryAdvanceMode.Manual);
    }

    // ===== 스토리 설정 초기화 =====
    public void ResetSettings()
    {
        SetTextSize(StoryTextSize.Medium);
        SetAdvanceMode(StoryAdvanceMode.Auto);
    }
}