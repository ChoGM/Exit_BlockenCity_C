using System;
using System.Collections.Generic;

[Serializable]
public class NewsData
{
    // 획득한 신문들의 고유 ID 리스트
    public List<string> unlockedNewsIds = new List<string>();

    // 해당 신문이 해금되었는지 확인
    public bool IsUnlocked(string newsId)
    {
        if (string.IsNullOrEmpty(newsId)) return false;
        return unlockedNewsIds.Contains(newsId);
    }

    // 신문 해금 추가
    public void Unlock(string newsId)
    {
        if (string.IsNullOrEmpty(newsId)) return;

        if (!unlockedNewsIds.Contains(newsId))
        {
            unlockedNewsIds.Add(newsId);
        }
    }
}