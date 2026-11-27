using System;
using System.Collections.Generic;
using UnityEngine;

public enum StoryBackgroundTransition
{
    Instant = 0,
    Fade = 1
}

[CreateAssetMenu(menuName = "Game/Story/Background Catalog", fileName = "StoryBackgroundCatalog")]
public sealed class StoryBackgroundCatalog : ScriptableObject
{
    public const string HideId = "__HIDE__";

    [Serializable]
    public sealed class Entry
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite sprite;

        public string Id => id == null ? string.Empty : id.Trim();
        public string DisplayName => displayName;
        public Sprite Sprite => sprite;
    }

    [SerializeField] private List<Entry> entries = new();
    public IReadOnlyList<Entry> Entries => entries;

    public bool TryGetSprite(string id, out Sprite sprite)
    {
        sprite = null;
        if (entries == null || string.IsNullOrWhiteSpace(id)) return false;
        id = id.Trim();
        foreach (Entry entry in entries)
        {
            if (entry == null || !string.Equals(entry.Id, id, StringComparison.Ordinal)) continue;
            if (sprite != null || entry.Sprite == null) return false;
            sprite = entry.Sprite;
        }
        return sprite != null;
    }

    private void OnValidate()
    {
        if (entries == null) return;
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (Entry entry in entries)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.Id) || entry.Sprite == null)
                Debug.LogWarning("배경 목록의 각 항목에 ID와 Sprite를 지정하세요.", this);
            else if (entry.Id == HideId || !ids.Add(entry.Id))
                Debug.LogError($"배경 ID '{entry.Id}'는 중복되었거나 예약된 값입니다.", this);
        }
    }
}
