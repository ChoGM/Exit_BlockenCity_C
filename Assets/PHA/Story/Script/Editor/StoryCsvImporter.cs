using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class StoryCsvImporter
{
    private const string CsvFolder = "Assets/PHA/Story/Data/CSV";
    private const string StoryAssetFolder = "Assets/PHA/Story/Data/StoryAssets";

    [MenuItem("Story/CSV/Validate and Preview StoryData")]
    public static void ValidateAllStoryData() => Run(false);

    [MenuItem("Story/CSV/Import All StoryData")]
    public static void ImportAllStoryData() => Run(true);

    private sealed class Draft
    {
        public StoryData Existing, Data;
        public bool Changed;
    }

    private static void Run(bool import)
    {
        var drafts = new List<Draft>();
        try
        {
            var input = new Input();
            var existing = AssetsById<StoryData>(story => story.StoryId);
            var characters = AssetsById<CharacterData>(character => character.CharacterId);
            var catalogs = LoadAssets<StoryBackgroundCatalog>().ToList();
            foreach (var row in input.Stories)
            {
                string id = row.Required("StoryId");
                existing.TryGetValue(id, out var original);
                var data = original ? Object.Instantiate(original) : ScriptableObject.CreateInstance<StoryData>();
                data.name = original ? original.name : "Story_" + string.Join("_", id.Split(Path.GetInvalidFileNameChars()));
                var draft = new Draft { Existing = original, Data = data };
                drafts.Add(draft);
                input.Build(row, data, original, characters, catalogs);
                draft.Changed = !original || EditorJsonUtility.ToJson(original) != EditorJsonUtility.ToJson(data);
            }
            var changed = drafts.Where(d => d.Changed).ToList();
            var report = new StringBuilder("스토리 CSV 검증 완료\n");
            foreach (var draft in changed)
            {
                var old = draft.Existing;
                report.AppendLine($"{draft.Data.StoryId}: {(old ? "수정" : "생성")} · 노드 {old?.Nodes.Count ?? 0} → {draft.Data.Nodes.Count}, 선택지 {ChoiceCount(old)} → {ChoiceCount(draft.Data)}");
            }
            report.Append($"변경 {changed.Count}개 / 입력 {drafts.Count}개. CSV에 없는 스토리는 유지합니다.");
            Debug.Log(report.ToString());
            if (import && changed.Count > 0)
            {
                Apply(changed);
                Debug.Log($"스토리 에셋 {changed.Count}개 반영 완료.");
            }
        }
        catch (Exception error) { Debug.LogError($"스토리 CSV 처리 실패\n{error}"); }
        finally
        {
            foreach (var draft in drafts)
                if (draft.Data && !EditorUtility.IsPersistent(draft.Data)) Object.DestroyImmediate(draft.Data);
        }
    }

    private static int ChoiceCount(StoryData data) =>
        data ? data.Nodes.Where(n => n != null && n.NodeType == StoryNodeType.Choice)
                         .Sum(n => n.Choices?.Count ?? 0) : 0;

    private static IEnumerable<T> LoadAssets<T>() where T : Object =>
        AssetDatabase.FindAssets("t:" + typeof(T).Name).Select(AssetDatabase.GUIDToAssetPath)
            .Select(path => AssetDatabase.LoadAssetAtPath<T>(path)).Where(asset => asset != null);

    private static Dictionary<string, T> AssetsById<T>(Func<T, string> key) where T : Object
    {
        var assets = new Dictionary<string, T>(StringComparer.Ordinal);
        foreach (var asset in LoadAssets<T>())
        {
            string id = key(asset);
            if (string.IsNullOrWhiteSpace(id)) continue;
            if (assets.TryGetValue(id, out var first))
                throw new InvalidDataException($"중복 {typeof(T).Name} ID '{id}': {AssetDatabase.GetAssetPath(first)}, {AssetDatabase.GetAssetPath(asset)}");
            assets.Add(id, asset);
        }
        return assets;
    }

    // Drafts are fully materialized and compared before entering this method.
    private static void Apply(List<Draft> changed)
    {
        var created = new List<string>();
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Import Story CSV");
        try
        {
            foreach (var draft in changed)
            {
                if (draft.Existing)
                {
                    Undo.RegisterCompleteObjectUndo(draft.Existing, "Import Story CSV");
                    EditorUtility.CopySerialized(draft.Data, draft.Existing);
                    EditorUtility.SetDirty(draft.Existing);
                }
                else
                {
                    if (!AssetDatabase.IsValidFolder(StoryAssetFolder))
                    {
                        Directory.CreateDirectory(StoryAssetFolder);
                        AssetDatabase.Refresh();
                    }
                    string path = AssetDatabase.GenerateUniqueAssetPath($"{StoryAssetFolder}/{draft.Data.name}.asset");
                    AssetDatabase.CreateAsset(draft.Data, path);
                    if (AssetDatabase.GetAssetPath(draft.Data) != path) throw new IOException($"에셋 생성 실패: {path}");
                    created.Add(path);
                }
            }
            foreach (var draft in changed)
                AssetDatabase.SaveAssetIfDirty(draft.Existing ? draft.Existing : draft.Data);
            Undo.CollapseUndoOperations(group);
        }
        catch
        {
            Undo.RevertAllDownToGroup(group);
            foreach (string path in created) AssetDatabase.DeleteAsset(path);
            foreach (var draft in changed)
                if (draft.Existing) AssetDatabase.SaveAssetIfDirty(draft.Existing);
            throw;
        }
    }

    private sealed class Input
    {
        private const string StoryHeaders = "StoryId StoryType StoryTitle Year Month Day Priority PlayOnce PlayTiming UnlockFactionId StartNodeId";
        private const string NodeHeaders = "StoryId NodeOrder NodeId NodeType Text CharacterId PortraitId NextNodeId DimPortrait KeepPortrait UseTypingEffect AutoAdvance AutoAdvanceDelay";
        private const string ChoiceHeaders = "StoryId NodeId ChoiceIndex ChoiceText TargetNodeId ResultKey ResultValue UseCondition RequiredKey RequiredValue HideWhenLocked LockedText";
        private const string ConditionHeaders = "StoryId ConditionIndex ConditionType Key Value IntValue";
        private const string EffectHeaders = "StoryId NodeId EffectIndex EffectType Target Duration Strength WaitForCompletion";

        public readonly List<StoryCsvRow> Stories;
        private readonly ILookup<string, StoryCsvRow> nodes, conditions;
        private readonly ILookup<(string, string), StoryCsvRow> choices, effects;
        private readonly bool hasConditions, hasEffects;
        private readonly HashSet<string> validBackgrounds = new HashSet<string>(StringComparer.Ordinal);

        public Input()
        {
            Stories = Read("Stories.csv", StoryHeaders);
            if (Stories.Count == 0) throw new InvalidDataException("Stories.csv: 스토리가 없습니다.");
            var storyIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in Stories)
                if (!storyIds.Add(row.Required("StoryId"))) throw row.Error("중복 StoryId: " + row.Id("StoryId"));

            var nodeRows = Read("StoryNodes.csv", NodeHeaders);
            var choiceRows = Read("StoryChoices.csv", ChoiceHeaders);
            hasConditions = File.Exists(Path.Combine(CsvFolder, "StoryConditions.csv"));
            hasEffects = File.Exists(Path.Combine(CsvFolder, "StoryEffects.csv"));
            var conditionRows = hasConditions ? Read("StoryConditions.csv", ConditionHeaders) : new List<StoryCsvRow>();
            var effectRows = hasEffects ? Read("StoryEffects.csv", EffectHeaders) : new List<StoryCsvRow>();
            foreach (var row in nodeRows.Concat(choiceRows).Concat(conditionRows).Concat(effectRows))
                if (!storyIds.Contains(row.Required("StoryId"))) throw row.Error("등록되지 않은 StoryId: " + row.Id("StoryId"));
            var nodeKeys = new HashSet<(string, string)>();
            foreach (var row in nodeRows)
                if (!nodeKeys.Add((row.Id("StoryId"), row.Required("NodeId"))))
                    throw row.Error("스토리 내부의 중복 NodeId: " + row.Id("NodeId"));
            foreach (var row in choiceRows.Concat(effectRows))
                if (!nodeKeys.Contains((row.Id("StoryId"), row.Required("NodeId"))))
                    throw row.Error("소속 노드 없음: " + row.Id("NodeId"));

            nodes = nodeRows.ToLookup(r => r.Id("StoryId"));
            conditions = conditionRows.ToLookup(r => r.Id("StoryId"));
            choices = choiceRows.ToLookup(r => (r.Id("StoryId"), r.Id("NodeId")));
            effects = effectRows.ToLookup(r => (r.Id("StoryId"), r.Id("NodeId")));
        }

        private static List<StoryCsvRow> Read(string file, string headers) => StoryCsvRow.Read(CsvFolder, file, headers);

        private static List<StoryCsvRow> Ordered(IEnumerable<StoryCsvRow> rows, string key)
        {
            var ordered = new SortedDictionary<int, StoryCsvRow>();
            foreach (var r in rows)
            {
                int index = r.Integer(key, min: 0);
                if (ordered.ContainsKey(index)) throw r.Error($"중복 {key}: {index}");
                ordered.Add(index, r);
            }
            return ordered.Values.ToList();
        }

        public void Build(StoryCsvRow story, StoryData draft, StoryData original,
            Dictionary<string, CharacterData> characters, List<StoryBackgroundCatalog> catalogs)
        {
            string id = story.Id("StoryId");
            using var obj = new SerializedObject(draft);
            var nodeRows = Ordered(nodes[id], "NodeOrder");
            var ids = new HashSet<string>(nodeRows.Select(n => n.Id("NodeId")), StringComparer.Ordinal);
            string start = story.Required("StartNodeId");
            if (!ids.Contains(start)) throw story.Error($"시작 노드 '{start}' 없음 ({id})");
            obj.FindProperty("storyId").stringValue = id;
            story.Required("StoryType");
            obj.FindProperty("storyType").intValue = (int)story.EnumValue("StoryType", StoryType.Normal);
            obj.FindProperty("storyTitle").stringValue = story.Raw("StoryTitle");
            obj.FindProperty("year").intValue = story.Integer("Year", 0, 0);
            obj.FindProperty("month").intValue = story.Integer("Month", 0, 0, 12);
            obj.FindProperty("day").intValue = story.Integer("Day", 0, 0, 31);
            obj.FindProperty("priority").intValue = story.Integer("Priority", 100);
            obj.FindProperty("playOnce").boolValue = story.Flag("PlayOnce", true);
            obj.FindProperty("playTiming").intValue = (int)story.EnumValue("PlayTiming", StoryPlayTiming.Monthly);
            obj.FindProperty("unlockFactionId").stringValue = story.Id("UnlockFactionId");
            obj.FindProperty("startNodeId").stringValue = start;
            obj.FindProperty("defaultBackgroundId").stringValue = Background(story, "DefaultBackgroundId", catalogs);

            // Missing optional files used to be interpreted as an empty list. Avoid erasing authored data.
            if (!hasConditions && original && original.Conditions?.Count > 0)
                throw story.Error("기존 조건이 있으나 StoryConditions.csv가 없습니다. 조건을 삭제하려면 헤더가 있는 빈 파일을 사용하세요.");
            if (!hasEffects && original && original.Nodes.Any(n => n?.Effects?.Count > 0))
                throw story.Error("기존 효과가 있으나 StoryEffects.csv가 없습니다. 효과를 삭제하려면 헤더가 있는 빈 파일을 사용하세요.");

            Fill(obj.FindProperty("conditions"), Ordered(conditions[id], "ConditionIndex"), WriteCondition);
            var adjacency = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            Fill(obj.FindProperty("nodes"), nodeRows, (p, row) =>
            {
                string nodeId = row.Id("NodeId");
                var edges = new List<string>();
                adjacency.Add(nodeId, edges);
                row.Required("NodeType");
                var type = row.EnumValue("NodeType", StoryNodeType.Narration);
                p.FindPropertyRelative("nodeType").intValue = (int)type;
                p.FindPropertyRelative("nodeId").stringValue = nodeId;
                p.FindPropertyRelative("text").stringValue = row.Raw("Text");
                string characterId = row.Id("CharacterId");
                CharacterData character = null;
                if (characterId.Length > 0 && !characters.TryGetValue(characterId, out character))
                    throw row.Error("등록되지 않은 CharacterId: " + characterId);
                p.FindPropertyRelative("character").objectReferenceValue = character;
                string portraitId = row.Id("PortraitId");
                p.FindPropertyRelative("portraitId").stringValue = portraitId;
                if (type == StoryNodeType.CharacterDialogue && character && portraitId.Length > 0 &&
                    (character.StoryPortraits == null || !character.StoryPortraits.Any(s => s != null && s.PortraitId == portraitId && s.PortraitSprite)))
                    Debug.LogWarning($"{row.Location}: {characterId}의 '{portraitId}' 표정이 없어 기존 기본 표정 처리를 사용합니다.");
                string next = row.Id("NextNodeId");
                if (next.Length == 0 && type != StoryNodeType.Choice && type != StoryNodeType.End)
                    throw row.Error($"'{nodeId}'의 NextNodeId가 비었습니다. 마지막 로그는 End 노드로 연결하세요.");
                if (next.Length > 0 && !ids.Contains(next)) throw row.Error($"NextNodeId '{next}' 없음 ({id}/{nodeId})");
                if (next.Length > 0 && type != StoryNodeType.Choice && type != StoryNodeType.End) edges.Add(next);
                p.FindPropertyRelative("nextNodeId").stringValue = next;
                p.FindPropertyRelative("dimPortrait").boolValue = row.Flag("DimPortrait");
                p.FindPropertyRelative("keepPortrait").boolValue = row.Flag("KeepPortrait", true);
                p.FindPropertyRelative("useTypingEffect").boolValue = row.Flag("UseTypingEffect", true);
                p.FindPropertyRelative("autoAdvance").boolValue = row.Flag("AutoAdvance");
                p.FindPropertyRelative("autoAdvanceDelay").floatValue = row.Seconds("AutoAdvanceDelay", 1.5f);
                p.FindPropertyRelative("backgroundId").stringValue = Background(row, "BackgroundId", catalogs);
                p.FindPropertyRelative("backgroundTransition").intValue = (int)row.EnumValue("BackgroundTransition", StoryBackgroundTransition.Instant);
                p.FindPropertyRelative("backgroundTransitionDuration").floatValue = row.Seconds("BackgroundTransitionDuration", 0.3f);

                var options = Ordered(choices[(id, nodeId)], "ChoiceIndex");
                if (type != StoryNodeType.Choice && options.Count > 0)
                    throw options[0].Error($"'{nodeId}'는 Choice 노드가 아닙니다.");
                if (type == StoryNodeType.Choice && (options.Count == 0 || options.Count > 3))
                    throw row.Error($"Choice '{nodeId}'의 선택지 개수 {options.Count} (허용: 1~3)");
                if (options.Count > 0 && options.All(c => c.Flag("UseCondition")))
                    Debug.LogWarning($"{row.Location}: '{nodeId}'의 모든 선택지에 조건이 있습니다. 모두 잠기는 진행 경로를 확인하세요.");
                Fill(p.FindPropertyRelative("choices"), options, (choice, option) =>
                {
                    string target = option.Required("TargetNodeId");
                    if (!ids.Contains(target)) throw option.Error($"이동 대상 '{target}' 없음 ({id}/{nodeId})");
                    edges.Add(target);
                    WriteChoice(choice, option);
                });
                Fill(p.FindPropertyRelative("effects"), Ordered(effects[(id, nodeId)], "EffectIndex"), WriteEffect);
            });

            var reached = new HashSet<string>();
            var pending = new Stack<string>();
            pending.Push(start);
            while (pending.Count > 0)
            {
                string current = pending.Pop();
                if (reached.Add(current)) foreach (var target in adjacency[current]) pending.Push(target);
            }
            foreach (var row in nodeRows.Where(n => !reached.Contains(n.Id("NodeId"))))
                Debug.LogWarning($"{row.Location}: 시작 노드에서 도달할 수 없습니다 ({id}/{row.Id("NodeId")}).");
            obj.ApplyModifiedPropertiesWithoutUndo();
        }

        private string Background(StoryCsvRow row, string column, List<StoryBackgroundCatalog> catalogs)
        {
            string id = row.Id(column);
            if (id.Length == 0 || id == StoryBackgroundCatalog.HideId || validBackgrounds.Contains(id)) return id;
            if (!catalogs.Any(c => c.TryGetSprite(id, out var sprite) && sprite != null))
                throw row.Error($"{column}: 카탈로그에 등록된 Sprite가 없습니다 ('{id}').");
            validBackgrounds.Add(id);
            return id;
        }

        private static void Fill(SerializedProperty list, IReadOnlyList<StoryCsvRow> rows,
            Action<SerializedProperty, StoryCsvRow> write)
        {
            list.ClearArray();
            list.arraySize = rows.Count;
            for (int i = 0; i < rows.Count; i++) write(list.GetArrayElementAtIndex(i), rows[i]);
        }

        private static void WriteCondition(SerializedProperty p, StoryCsvRow row)
        {
            row.Required("ConditionType");
            var type = row.EnumValue("ConditionType", StoryConditionType.None);
            if (type != StoryConditionType.None) row.Required("Key");
            p.FindPropertyRelative("conditionType").intValue = (int)type;
            p.FindPropertyRelative("key").stringValue = row.Id("Key");
            p.FindPropertyRelative("value").stringValue = row.Id("Value");
            p.FindPropertyRelative("intValue").intValue = row.Integer("IntValue", 0);
        }

        private static void WriteChoice(SerializedProperty p, StoryCsvRow row)
        {
            row.Required("ChoiceText");
            if (row.Flag("UseCondition")) row.Required("RequiredKey");
            p.FindPropertyRelative("choiceText").stringValue = row.Raw("ChoiceText");
            p.FindPropertyRelative("targetNodeId").stringValue = row.Id("TargetNodeId");
            p.FindPropertyRelative("resultKey").stringValue = row.Id("ResultKey");
            p.FindPropertyRelative("resultValue").stringValue = row.Id("ResultValue");
            p.FindPropertyRelative("useCondition").boolValue = row.Flag("UseCondition");
            p.FindPropertyRelative("requiredKey").stringValue = row.Id("RequiredKey");
            p.FindPropertyRelative("requiredValue").stringValue = row.Id("RequiredValue");
            p.FindPropertyRelative("hideWhenLocked").boolValue = row.Flag("HideWhenLocked", true);
            p.FindPropertyRelative("lockedText").stringValue = row.Raw("LockedText");
        }

        private static void WriteEffect(SerializedProperty p, StoryCsvRow row)
        {
            row.Required("EffectType");
            p.FindPropertyRelative("effectType").intValue = (int)row.EnumValue("EffectType", StoryEffectType.None);
            p.FindPropertyRelative("target").intValue = (int)row.EnumValue("Target", StoryEffectTarget.Portrait);
            p.FindPropertyRelative("duration").floatValue = row.Seconds("Duration", 0);
            p.FindPropertyRelative("strength").floatValue = row.Seconds("Strength", 1);
            p.FindPropertyRelative("waitForCompletion").boolValue = row.Flag("WaitForCompletion");
        }
    }
}
