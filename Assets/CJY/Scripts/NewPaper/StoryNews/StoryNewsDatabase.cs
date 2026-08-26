using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StoryNewsDatabase", menuName = "NewsSystem/StoryNewsDatabase")]
public class StoryNewsDatabase : ScriptableObject
{
    public List<StoryNewsData> newsList;

    // 해당 월(month)의 기사 중 하나를 랜덤으로 뽑아주는 함수 (GC Alloc 최적화)
    public StoryNewsData GetRandomNews(int month)
    {
        if (newsList == null || newsList.Count == 0) return null;

        // 해당 월에 일치하는 기사들을 담을 임시 리스트 (미리 필요한 크기만큼 검사)
        List<StoryNewsData> monthNewsList = new List<StoryNewsData>();

        for (int i = 0; i < newsList.Count; i++)
        {
            if (newsList[i] != null && newsList[i].targetMonth == month)
            {
                monthNewsList.Add(newsList[i]);
            }
        }

        if (monthNewsList.Count == 0) return null;

        int randomIndex = Random.Range(0, monthNewsList.Count);
        return monthNewsList[randomIndex];
    }
}