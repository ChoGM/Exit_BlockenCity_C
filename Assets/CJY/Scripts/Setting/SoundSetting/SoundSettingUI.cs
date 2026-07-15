using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundSettingUI : MonoBehaviour
{
    [Header("Volume Sliders")]
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    private void Start()
    {
        // ===== [추가] 매니저의 값 변경 이벤트를 구독 =====
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.OnSoundSettingsChanged += ApplyCurrentSettings;
        }

        // 시작할 때 화면 동기화
        ApplyCurrentSettings();

        // 슬라이더 이벤트 등록
        masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
    }

    // 오브젝트가 파괴될 때 이벤트 구독을 해제해 줍니다 (메모리 누수 방지)
    private void OnDestroy()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.OnSoundSettingsChanged -= ApplyCurrentSettings;
        }
    }

    // ===== [추가] 현재 사운드 매니저의 값을 UI에 실시간 반영 =====
    public void ApplyCurrentSettings()
    {
        if (SoundManager.Instance != null)
        {
            // SetValueWithoutNotify를 쓰면 슬라이더 값 변경 시 이벤트 루프(무한 반복)에 빠지지 않고 화면만 딱 바꿀 수 있어!
            masterSlider.SetValueWithoutNotify(SoundManager.Instance.masterVolume);
            bgmSlider.SetValueWithoutNotify(SoundManager.Instance.bgmVolume);
            sfxSlider.SetValueWithoutNotify(SoundManager.Instance.sfxVolume);
        }
    }

    public void OnMasterSliderChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMasterVolume(value);
        }
    }

    public void OnBGMSliderChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetBGMVolume(value);
        }
    }

    public void OnSFXSliderChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(value);
        }
    }
}