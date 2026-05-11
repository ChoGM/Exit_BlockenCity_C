using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    // 어디서든 접근 가능하게 싱글톤 설정
    public static UIManager Instance;

    [Header("관리할 모든 캔버스 리스트 (선택사항)")]
    public List<GameObject> allCanvases;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 특정 캔버스를 활성화하는 함수
    public void ShowCanvas(GameObject canvasToShow)
    {
        // 1. (선택) 기존에 켜져 있는 다른 캔버스를 끄고 싶다면 호출
        // HideAllCanvases();

        // 2. 대상 캔버스 활성화
        if (canvasToShow != null)
        {
            canvasToShow.SetActive(true);

            // UI가 뜨면 마우스 커서가 자유롭게 움직이도록 설정 (1인칭일 경우 필요)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // 모든 캔버스를 숨기는 기능 (닫기 버튼 등에 사용)
    public void HideAllCanvases()
    {
        foreach (var canvas in allCanvases)
        {
            if (canvas != null) canvas.SetActive(false);
        }
    }
}