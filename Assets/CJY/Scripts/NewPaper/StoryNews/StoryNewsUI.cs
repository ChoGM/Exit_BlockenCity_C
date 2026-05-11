using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryNewsUI : MonoBehaviour
{
    [SerializeField] private GameObject newsPanel; // 신문 UI 전체 부모 객체
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI dateText;

    // 시스템으로부터 데이터를 넘겨받아 화면에 세팅
    public void DisplayNews(string title, string content, Sprite icon, int month)
    {
        titleText.text = title;
        contentText.text = content;
        iconImage.sprite = icon;

        // {month:D2}는 숫자가 1자리일 경우 앞에 0을 붙여 "01", "02"로 만들어주는 포맷팅입니다.
        if (dateText != null)
        {
            dateText.text = $"{month:D2}.xx";
        }

        newsPanel.SetActive(true); // UI 켜기
    }

    // 닫기 버튼 등에 연결할 함수
    public void CloseNews()
    {
        newsPanel.SetActive(false);
    }
}
