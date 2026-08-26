using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NewsUnlockManager
{
    [Header("데모 설정")]
    // true로 설정하면 세이브 데이터와 관계없이 도감에서 모든 신문이 해금된 상태로 보입니다.
    public static bool isDemoUnlockAll = false;

    // Datamanager 내의 NewsData 참조 가져오기
    private static NewsData CurrentNewsData
    {
        get
        {
            if (Datamanager.Instance != null && Datamanager.Instance.saveData != null)
            {
                return Datamanager.Instance.saveData.news;
            }
            return null;
        }
    }

    /// <summary>
    /// 신문 해금 처리 (Datamanager SaveData 연동)
    /// </summary>
    public static void UnlockNews(string newsId)
    {
        if (string.IsNullOrEmpty(newsId)) return;

        NewsData newsData = CurrentNewsData;
        if (newsData != null)
        {
            newsData.Unlock(newsId);

            Datamanager.Instance.SaveGameData();
        }
        else
        {
            Debug.LogWarning("[NewsUnlockManager] Datamanager 또는 SaveData가 null 상태입니다.");
        }
    }

    /// <summary>
    /// 신문 해금 여부 확인
    /// </summary>
    public static bool IsUnlocked(string newsId)
    {
        if (string.IsNullOrEmpty(newsId)) return false;

        // 데모 모드가 활성화되어 있다면 무조건 true 반환
        if (isDemoUnlockAll) return true;

        NewsData newsData = CurrentNewsData;
        if (newsData != null)
        {
            return newsData.IsUnlocked(newsId);
        }

        return false;
    }
}