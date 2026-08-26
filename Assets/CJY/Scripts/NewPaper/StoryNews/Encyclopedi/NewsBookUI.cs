using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewsBookUI : MonoBehaviour
{
    [System.Serializable]
    public class NewsSlot
    {
        public StoryNewsData newsData;     // 신문 데이터
        public GameObject newsImageObject; // 미리 배치해 둔 해당 신문의 UI 오브젝트
    }

    [Header("도감에 연결된 전체 신문 오브젝트 목록")]
    [SerializeField] private List<NewsSlot> newsSlots;

    [Header("신문 스크랩 팝업 UI")]
    [SerializeField] private GameObject newsPanel; // 신문 UI 전체 부모 객체
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI dateText;

    private void OnEnable()
    {
        InitSlotButtons();
        UpdateBookUI();
    }

    private void InitSlotButtons()
    {
        if (newsSlots == null || newsSlots.Count == 0) return;

        foreach (var slot in newsSlots)
        {
            if (slot.newsImageObject == null) continue;

            Button button = slot.newsImageObject.GetComponentInChildren<Button>();
            if (button == null) continue;

            StoryNewsData data = slot.newsData;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                OpenNewsPopup(data);
            });
        }
    }

    /// <summary>
    /// 저장된 NewsData(또는 데모 상태)에 따라 해금된 신문 슬롯만 화면에 표시합니다.
    /// </summary>
    public void UpdateBookUI()
    {
        if (newsSlots == null) return;

        foreach (var slot in newsSlots)
        {
            if (slot.newsImageObject == null) continue;

            bool isUnlocked = false;

            if (slot.newsData != null)
            {
                // NewsUnlockManager를 통해 해금 여부 확인 (데모 옵션 반영됨)
                isUnlocked = NewsUnlockManager.IsUnlocked(slot.newsData.id);
            }

            // 해금된 신문만 활성화
            slot.newsImageObject.SetActive(isUnlocked);
        }
    }

    public void OpenNewsPopup(StoryNewsData data)
    {
        if (newsPanel == null) return;

        newsPanel.SetActive(true);
        newsPanel.transform.SetAsLastSibling();

        if (data != null)
        {
            if (titleText != null) titleText.text = data.title;
            if (contentText != null) contentText.text = data.content;

            if (iconImage != null)
            {
                iconImage.sprite = data.icon;
                iconImage.gameObject.SetActive(data.icon != null);
            }

            if (dateText != null)
            {
                dateText.text = $"{data.targetMonth:D2}.xx";
            }
        }
    }

    public void CloseNewsPopup()
    {
        if (newsPanel != null)
        {
            newsPanel.SetActive(false);
        }
    }
}