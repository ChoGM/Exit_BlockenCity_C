using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    // ===== [추가] 볼륨이 바뀔 때 외부에 알려줄 이벤트 =====
    public event Action OnSoundSettingsChanged;

    [Header("Audio Mixer")]
    public AudioMixer mainMixer;

    private const string MASTER_KEY = "Master";
    private const string BGM_KEY = "BGM";
    private const string SFX_KEY = "SFX";

    public float masterVolume = 1f;
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadVolumeSettings();
        ApplyAllVolumes();
    }

    private void Start()
    {
        ApplyAllVolumes();
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = value;
        SetMixerVolume("Master", value);
        PlayerPrefs.SetFloat(MASTER_KEY, value);

        // ===== [추가] 데이터가 변경되었음을 UI에 알림 =====
        OnSoundSettingsChanged?.Invoke();
    }

    public void SetBGMVolume(float value)
    {
        bgmVolume = value;
        SetMixerVolume("BGM", value);
        PlayerPrefs.SetFloat(BGM_KEY, value);

        // ===== [추가] 데이터가 변경되었음을 UI에 알림 =====
        OnSoundSettingsChanged?.Invoke();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        SetMixerVolume("SFX", value);
        PlayerPrefs.SetFloat(SFX_KEY, value);

        // ===== [추가] 데이터가 변경되었음을 UI에 알림 =====
        OnSoundSettingsChanged?.Invoke();
    }

    private void SetMixerVolume(string exposedParam, float value01)
    {
        if (value01 <= 0.0001f)
        {
            mainMixer.SetFloat(exposedParam, -80f);
        }
        else
        {
            float dB = Mathf.Log10(value01) * 20f;
            mainMixer.SetFloat(exposedParam, dB);
        }
    }

    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(MASTER_KEY, 1f);
        bgmVolume = PlayerPrefs.GetFloat(BGM_KEY, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_KEY, 1f);
    }

    private void ApplyAllVolumes()
    {
        SetMixerVolume("Master", masterVolume);
        SetMixerVolume("BGM", bgmVolume);
        SetMixerVolume("SFX", sfxVolume);
    }

    // ===== 사운드 설정 초기화 =====
    public void ResetSettings()
    {
        SetMasterVolume(0.5f);
        SetBGMVolume(0.5f);
        SetSFXVolume(0.5f);
    }
}