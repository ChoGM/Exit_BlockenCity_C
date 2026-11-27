using System;

/// <summary>엑셀 RequiredKey/ResultKey의 예약 접두사. 기존 일반 키는 기존 방식으로 처리한다.</summary>
public static class StoryChoiceRules
{
    public const string HistoryPrefix = "history:";
    public const string AnyHistoryPrefix = "history.any:";
    public const string RelationshipPrefix = "relationship.positive:";

    public static bool TryEvaluate(string key, string value, out bool available)
    {
        return TryEvaluate(key, value, faction =>
        {
            Datamanager.Instance.EnsureLoaded();
            return GameDataManager.Instance.GetRelationship(faction);
        }, StoryChoiceHistory.HasEverChosen, out available);
    }

    // 조회 함수를 분리하여 우호도 경계값과 다회차 조건을 독립적으로 검증할 수 있다.
    public static bool TryEvaluate(string key, string value,
        Func<RelationshipType, float> relationship,
        Func<string, string, bool> history, out bool available)
    {
        available = false;
        if (string.IsNullOrWhiteSpace(key)) return false;
        if (key.StartsWith(RelationshipPrefix, StringComparison.Ordinal))
        {
            string name = key.Substring(RelationshipPrefix.Length);
            if (Enum.TryParse(name, false, out RelationshipType faction) &&
                Enum.IsDefined(typeof(RelationshipType), faction) && faction.ToString() == name)
                available = relationship(faction) > 0f;
            return true;
        }
        if (key.StartsWith(AnyHistoryPrefix, StringComparison.Ordinal))
        {
            foreach (string part in key.Substring(AnyHistoryPrefix.Length).Split('|'))
                if (!string.IsNullOrWhiteSpace(part) && history(part.Trim(), value))
                {
                    available = true;
                    break;
                }
            return true;
        }
        if (key.StartsWith(HistoryPrefix, StringComparison.Ordinal))
        {
            available = history(key.Substring(HistoryPrefix.Length), value);
            return true;
        }
        return false;
    }

    public static void RecordResult(string key, string value)
    {
        if (key != null && key.StartsWith(HistoryPrefix, StringComparison.Ordinal))
            StoryChoiceHistory.Remember(key.Substring(HistoryPrefix.Length), value);
    }
}
