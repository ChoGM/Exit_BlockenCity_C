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
        // public string slotName;          // 인스펙터 구분용 이름 (선택사항)[cite: 14]
        public StoryNewsData newsData;     // 신문 데이터[cite: 14]
        public GameObject newsImageObject; // 미리 배치해 둔 해당 신문의 UI 오브젝트[cite: 14]
    }

    [Header("도감에 연결된 전체 신문 오브젝트 목록")]
    [SerializeField] private List<NewsSlot> newsSlots;

    [Header("신문 스크랩 팝업 UI")]
    [SerializeField] private GameObject newsPanel; // 신문 UI 전체 부모 객체
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI dateText;

    private void Awake()
    {
        // 버튼 클릭 이벤트 자동 바인딩
        InitSlotButtons();
    }

    private void OnEnable()
    {
        // 도감 창이 활성화될 때마다 도감 상태 갱신
        UpdateBookUI();
    }

    /// <summary>
    /// 각 신문 오브젝트의 Button 컴포넌트에 클릭 이벤트를 등록합니다.
    /// </summary>
    private void InitSlotButtons()
    {
        foreach (var slot in newsSlots)
        {
            if (slot.newsData == null || slot.newsImageObject == null) continue;

            // newsImageObject에 Button 컴포넌트가 있는지 확인
            Button button = slot.newsImageObject.GetComponent<Button>();
            if (button != null)
            {
                StoryNewsData data = slot.newsData; // 람다식 캡처용 지역 변수
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OpenNewsPopup(data));
            }
        }
    }

    /// <summary>
    /// 저장 상태를 확인하여 획득한 신문 오브젝트만 활성화합니다.
    /// </summary>
    public void UpdateBookUI()
    {
        foreach (var slot in newsSlots)
        {
            if (slot.newsData == null || slot.newsImageObject == null) continue;

            // 해당 신문이 해금되었는지 체크
            bool isUnlocked = NewsUnlockManager.IsUnlocked(slot.newsData.id);

            // 획득 시 오브젝트 SetActive(true), 미획득 시 SetActive(false)
            slot.newsImageObject.SetActive(isUnlocked); 
        }
    }

    /// <summary>
    /// 클릭한 신문의 데이터를 팝업 UI에 채우고 켜줍니다.
    /// </summary>
    public void OpenNewsPopup(StoryNewsData data)
    {
        if (data == null) return;

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

        if (newsPanel != null)
        {
            newsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 팝업 닫기 버튼에 연결할 함수입니다.
    /// </summary>
    public void CloseNewsPopup()
    {
        if (newsPanel != null)
        {
            newsPanel.SetActive(false);
        }
    }
}