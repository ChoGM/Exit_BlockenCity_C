using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NewsUnlockManager
{
    private const string SAVE_PREFIX = "Unlocked_News_";

    // 신문 해금 처리
    public static void UnlockNews(string newsId)
    {
        if (string.IsNullOrEmpty(newsId)) return;

        PlayerPrefs.SetInt(SAVE_PREFIX + newsId, 1);
        PlayerPrefs.Save();
    }

    // 신문 해금 여부 확인
    public static bool IsUnlocked(string newsId)
    {
        if (string.IsNullOrEmpty(newsId)) return false;

        return PlayerPrefs.GetInt(SAVE_PREFIX + newsId, 0) == 1;
    }
}
