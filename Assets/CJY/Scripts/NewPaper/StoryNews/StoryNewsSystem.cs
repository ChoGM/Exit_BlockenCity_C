using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryNewsSystem : MonoBehaviour
{
    public static StoryNewsSystem Instance { get; private set; }

    [Header("References")]
    [SerializeField] private StoryNewsDatabase database;
    [SerializeField] private StoryNewsUI newsUI; // 기존 날씨 뉴스 UI 스크립트 그대로 사용

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // LobbyMonthInitializer에서 월이 정해진 후 호출
    public void ShowStoryNews(int month)
    {
        // 데이터베이스에서 이번 달 기사 중 하나를 랜덤으로 가져옴
        StoryNewsData todayStory = database.GetRandomNews(month);

        // 기사가 존재한다면 기존 UI에 띄웁니다.
        if (todayStory != null)
        {
            newsUI.DisplayNews(todayStory.title, todayStory.content, todayStory.icon, month);
        }
        else
        {
            Debug.Log($"[StoryNewsSystem] {month}월에 해당하는 스토리 기사가 없습니다.");
        }
    }
}
