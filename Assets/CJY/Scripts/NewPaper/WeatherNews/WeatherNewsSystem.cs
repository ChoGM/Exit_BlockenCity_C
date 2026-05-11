using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherNewsSystem : MonoBehaviour
{
    public static WeatherNewsSystem Instance { get; private set; }

    [Header("References")]
    [SerializeField] private WeatherNewsDatabase database;
    [SerializeField] private WeatherNewsUI newsUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 월이 바뀌고 날씨가 결정된 직후에 이 함수를 호출
    public void ShowTodayNews()
    {
        // Entry를 먼저 받고, 거기서 type(ObstacleType)을 추출
        var currentEntry = GameObstacleSystem.Instance.GetSelectedObstacle();
        ObstacleType currentObstacle = currentEntry.type;

        // 데이터베이스에서 해당 날씨의 기사 검색
        WeatherNewsData todayNews = database.GetNewsData(currentObstacle);

        // UI에 데이터 전달
        if (todayNews != null)
        {
            newsUI.DisplayNews(todayNews.title, todayNews.content, todayNews.icon);
        }
        else
        {
            Debug.LogWarning($"[WeatherNewsSystem] {currentObstacle}에 해당하는 기사 데이터가 없습니다!");
        }
    }
}