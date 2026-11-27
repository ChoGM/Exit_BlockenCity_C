using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>선택한 결과의 누적 이력. 회차 SaveData와 분리하여 새 게임에서도 보존한다.</summary>
[Serializable]
public class StoryChoiceHistoryData
{
    public int version = 1;
    public List<StoryChoiceHistoryEntry> results = new List<StoryChoiceHistoryEntry>();
}

[Serializable]
public class StoryChoiceHistoryEntry
{
    public string key;
    public string value;
}

public static class StoryChoiceHistory
{
    // 로컬 플레이어 전체에 적용된다. 일반 새 게임/회차 세이브 삭제와 무관하다.
    private const string StorageKey = "Story.ChoiceHistory.v1";

    private static StoryChoiceHistoryData Load()
    {
        if (!PlayerPrefs.HasKey(StorageKey)) return new StoryChoiceHistoryData();
        try
        {
            var data = JsonUtility.FromJson<StoryChoiceHistoryData>(PlayerPrefs.GetString(StorageKey));
            if (data == null || data.version != 1)
                throw new InvalidOperationException("Unsupported story choice history data.");
            if (data.results == null) data.results = new List<StoryChoiceHistoryEntry>();
            return data;
        }
        catch (Exception exception)
        {
            // 읽을 수 없는 기존 이력을 빈 데이터로 덮어쓰지 않는다.
            Debug.LogError("Story choice history could not be loaded: " + exception.Message);
            return null;
        }
    }

    public static bool HasEverChosen(string key, string value)
    {
        var data = Load();
        return data != null && data.results.Exists(entry => entry != null &&
            string.Equals(entry.key, key, StringComparison.Ordinal) &&
            string.Equals(entry.value, value, StringComparison.OrdinalIgnoreCase));
    }

    public static void Remember(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        var data = Load();
        if (data == null || data.results.Exists(entry => entry != null &&
            string.Equals(entry.key, key, StringComparison.Ordinal) &&
            string.Equals(entry.value, value, StringComparison.OrdinalIgnoreCase))) return;
        data.results.Add(new StoryChoiceHistoryEntry { key = key, value = value });
        PlayerPrefs.SetString(StorageKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }
}
