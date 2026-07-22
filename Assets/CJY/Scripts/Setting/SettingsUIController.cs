using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsResetButton : MonoBehaviour
{
    // 초기화 버튼 클릭 시 실행할 함수
    public void ResetAllSettings()
    {
        // 1. 사운드 초기화 (자동으로 UI 슬라이더가 50%로 움직임!)
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ResetSettings();
        }

        // 2. 스토리 초기화 (자동으로 Medium / Auto 이미지에 불이 들어옴!)
        if (StorySettingsManager.Instance != null)
        {
            StorySettingsManager.Instance.ResetSettings();
        }

        // 3. 변경 사항 디스크 저장
        PlayerPrefs.Save();

        Debug.Log("모든 데이터 초기화 및 UI 실시간 동기화 완료!");
    }
}