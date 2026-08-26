using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryNewsSystem : MonoBehaviour
{
    public static StoryNewsSystem Instance { get; private set; }

    [Header("References")]
    [SerializeField] private StoryNewsDatabase database;
    [SerializeField] private StoryNewsUI newsUI;

    // 이번 달에 선정된 신문 데이터를 저장해두는 변수
    private StoryNewsData currentMonthNews;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 신문 버튼 클릭 시 호출
    /// </summary>
    public void ShowStoryNews(int month)
    {
        // 이번 달 기사가 아직 뽑히지 않았거나, 월이 바뀌었다면 새로운 기사를 랜덤 추첨
        if (currentMonthNews == null || currentMonthNews.targetMonth != month)
        {
            currentMonthNews = database.GetRandomNews(month);
        }

        // 기사가 존재할 경우 UI 출력 및 해금 등록
        if (currentMonthNews != null)
        {
            // 버튼을 눌러 신문 UI를 '확인'하는 시점에 도감에 해금(저장)
            NewsUnlockManager.UnlockNews(currentMonthNews.id);

            newsUI.DisplayNews(currentMonthNews.title, currentMonthNews.content, currentMonthNews.icon, month);
        }
        else
        {
            Debug.Log($"[StoryNewsSystem] {month}월에 해당하는 스토리 기사가 없습니다.");
        }
    }
}