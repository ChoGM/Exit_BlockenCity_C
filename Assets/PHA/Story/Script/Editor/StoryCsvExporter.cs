using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class StoryCsvExporter
{
    private const string ExportFolder =
        "Assets/PHA/Story/Data/CSV";

    private const string StoriesFile =
        "Stories.csv";

    private const string NodesFile =
        "StoryNodes.csv";

    private const string ChoicesFile =
        "StoryChoices.csv";

    private const string ConditionsFile =
        "StoryConditions.csv";

    private const string EffectsFile =
        "StoryEffects.csv";

    private const string CharactersFile =
        "Characters.csv";


    // =========================================================
    // 전체 StoryData Export
    // =========================================================

    [MenuItem("Story/CSV/Export All StoryData")]
    public static void ExportAllStoryData()
    {
        var batch = new StoryCsvExportBatch(ExportFolder);

        string[] guids =
            AssetDatabase.FindAssets(
                "t:StoryData"
            );

        if (guids == null ||
            guids.Length == 0)
        {
            Debug.LogWarning(
                "[StoryCsvExporter] " +
                "StoryData ScriptableObject를 찾지 못했습니다."
            );

            return;
        }

        List<StoryData> stories =
            new List<StoryData>();

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(
                    guid
                );

            StoryData story =
                AssetDatabase.LoadAssetAtPath<StoryData>(
                    path
                );

            if (story != null)
            {
                stories.Add(story);
            }
        }

        if (stories.Count == 0)
        {
            Debug.LogWarning(
                "[StoryCsvExporter] " +
                "Export 가능한 StoryData가 없습니다."
            );

            return;
        }

        // StoryId 기준 정렬
        stories.Sort(
            (a, b) =>
                string.Compare(
                    a.StoryId,
                    b.StoryId,
                    StringComparison.Ordinal
                )
        );

        ExportStories(batch, stories);
        ExportNodes(batch, stories);
        ExportChoices(batch, stories);
        ExportConditions(batch, stories);
        ExportEffects(batch, stories);

        ExportCharacters(batch);
        ExportBackgrounds(batch);

        batch.Commit();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[StoryCsvExporter] Export 완료\n" +
            $"StoryData 수: {stories.Count}\n" +
            $"경로: {ExportFolder}"
        );
    }


    // =========================================================
    // 선택한 StoryData 하나만 Export
    // =========================================================

    [MenuItem(
        "Story/CSV/Export Selected StoryData",
        true
    )]
    private static bool ValidateExportSelectedStory()
    {
        return Selection.activeObject
            is StoryData;
    }


    [MenuItem(
        "Story/CSV/Export Selected StoryData"
    )]
    public static void ExportSelectedStory()
    {
        StoryData story =
            Selection.activeObject
            as StoryData;

        if (story == null)
        {
            Debug.LogWarning(
                "StoryData를 선택해주세요."
            );

            return;
        }

        var batch = new StoryCsvExportBatch(ExportFolder);

        List<StoryData> stories =
            new List<StoryData>
            {
                story
            };

        /*
         * 선택 Export는 별도 파일로 저장합니다.
         * 전체 CSV를 덮어쓰지 않기 위함입니다.
         */

        string safeId =
            MakeSafeFileName(
                story.StoryId
            );

        ExportStories(
            batch, stories,
            $"Stories_{safeId}.csv"
        );

        ExportNodes(
            batch, stories,
            $"StoryNodes_{safeId}.csv"
        );

        ExportChoices(
            batch, stories,
            $"StoryChoices_{safeId}.csv"
        );

        ExportConditions(
            batch, stories,
            $"StoryConditions_{safeId}.csv"
        );

        ExportEffects(
            batch, stories,
            $"StoryEffects_{safeId}.csv"
        );

        batch.Commit();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[StoryCsvExporter] " +
            $"선택 스토리 Export 완료: " +
            $"{story.StoryId}"
        );
    }


    // =========================================================
    // Stories.csv
    // =========================================================

    private static void ExportStories(
        StoryCsvExportBatch batch,
        IReadOnlyList<StoryData> stories,
        string fileName = StoriesFile)
    {
        List<string[]> rows =
            new List<string[]>();

        rows.Add(
            new[]
            {
                "StoryId",
                "StoryType",
                "StoryTitle",
                "Year",
                "Month",
                "Day",
                "Priority",
                "PlayOnce",
                "PlayTiming",
                "UnlockFactionId",
                "StartNodeId",
                "DefaultBackgroundId",
                "Memo"
            }
        );

        foreach (StoryData story
                 in stories)
        {
            if (story == null)
            {
                continue;
            }

            rows.Add(
                new[]
                {
                    story.StoryId,
                    story.StoryType.ToString(),
                    story.StoryTitle,
                    story.Year.ToString(),
                    story.Month.ToString(),
                    story.Day.ToString(),
                    story.Priority.ToString(),
                    ToCsvBool(
                        story.PlayOnce
                    ),
                    story.PlayTiming.ToString(),
                    story.UnlockFactionId,
                    story.StartNodeId,
                    story.DefaultBackgroundId,
                    string.Empty
                }
            );
        }

        batch.Stage(
            fileName,
            rows
        );
    }


    // =========================================================
    // StoryNodes.csv
    // =========================================================

    private static void ExportNodes(
        StoryCsvExportBatch batch,
        IReadOnlyList<StoryData> stories,
        string fileName = NodesFile)
    {
        List<string[]> rows =
            new List<string[]>();

        rows.Add(
            new[]
            {
                "StoryId",
                "NodeOrder",
                "NodeId",
                "NodeType",
                "Text",
                "CharacterId",
                "PortraitId",
                "NextNodeId",
                "DimPortrait",
                "KeepPortrait",
                "UseTypingEffect",
                "AutoAdvance",
                "AutoAdvanceDelay",
                "BackgroundId",
                "BackgroundTransition",
                "BackgroundTransitionDuration",
                "Branch/Scene",
                "Memo"
            }
        );

        foreach (StoryData story
                 in stories)
        {
            if (story == null ||
                story.Nodes == null)
            {
                continue;
            }

            for (int nodeIndex = 0;
                 nodeIndex < story.Nodes.Count;
                 nodeIndex++)
            {
                StoryNodeData node =
                    story.Nodes[nodeIndex];

                if (node == null)
                {
                    continue;
                }

                string characterId =
                    string.Empty;

                if (node.Character != null)
                {
                    characterId =
                        node.Character.CharacterId;
                }

                rows.Add(
                    new[]
                    {
                        story.StoryId,

                        /*
                         * Excel에서 정렬/검수하기 쉽게
                         * ScriptableObject List 순서를 기록합니다.
                         */
                        (nodeIndex + 1).ToString(),

                        node.NodeId,
                        node.NodeType.ToString(),
                        node.Text,
                        characterId,
                        node.PortraitId,
                        node.NextNodeId,

                        ToCsvBool(
                            node.DimPortrait
                        ),

                        ToCsvBool(
                            node.KeepPortrait
                        ),

                        ToCsvBool(
                            node.UseTypingEffect
                        ),

                        ToCsvBool(
                            node.AutoAdvance
                        ),

                        node.AutoAdvanceDelay
                            .ToString(
                                CultureInfo.InvariantCulture
                            ),
                        node.BackgroundId,
                        node.BackgroundTransition.ToString(),
                        node.BackgroundTransitionDuration.ToString(CultureInfo.InvariantCulture),
                        string.Empty,

                        string.Empty
                    }
                );
            }
        }

        batch.Stage(
            fileName,
            rows
        );
    }


    // =========================================================
    // StoryChoices.csv
    // =========================================================

    private static void ExportChoices(
        StoryCsvExportBatch batch,
        IReadOnlyList<StoryData> stories,
        string fileName = ChoicesFile)
    {
        List<string[]> rows =
            new List<string[]>();

        rows.Add(
            new[]
            {
                "StoryId",
                "NodeId",
                "ChoiceIndex",
                "ChoiceText",
                "TargetNodeId",
                "ResultKey",
                "ResultValue",
                "UseCondition",
                "RequiredKey",
                "RequiredValue",
                "HideWhenLocked",
                "LockedText",
                "Memo"
            }
        );

        foreach (StoryData story
                 in stories)
        {
            if (story == null ||
                story.Nodes == null)
            {
                continue;
            }

            foreach (StoryNodeData node
                     in story.Nodes)
            {
                if (node == null ||
                    node.NodeType !=
                    StoryNodeType.Choice ||
                    node.Choices == null)
                {
                    continue;
                }

                for (int choiceIndex = 0;
                     choiceIndex <
                     node.Choices.Count;
                     choiceIndex++)
                {
                    StoryChoiceData choice =
                        node.Choices[
                            choiceIndex
                        ];

                    if (choice == null)
                    {
                        continue;
                    }

                    rows.Add(
                        new[]
                        {
                            story.StoryId,
                            node.NodeId,
                            choiceIndex.ToString(),

                            choice.ChoiceText,
                            choice.TargetNodeId,

                            choice.ResultKey,
                            choice.ResultValue,

                            ToCsvBool(
                                choice.UseCondition
                            ),

                            choice.RequiredKey,
                            choice.RequiredValue,

                            ToCsvBool(
                                choice.HideWhenLocked
                            ),

                            choice.LockedText,

                            string.Empty
                        }
                    );
                }
            }
        }

        batch.Stage(
            fileName,
            rows
        );
    }


    // =========================================================
    // StoryConditions.csv
    // =========================================================

    private static void ExportConditions(
        StoryCsvExportBatch batch,
        IReadOnlyList<StoryData> stories,
        string fileName = ConditionsFile)
    {
        List<string[]> rows =
            new List<string[]>();

        rows.Add(
            new[]
            {
                "StoryId",
                "ConditionIndex",
                "ConditionType",
                "Key",
                "Value",
                "IntValue",
                "Memo"
            }
        );

        foreach (StoryData story
                 in stories)
        {
            if (story == null ||
                story.Conditions == null)
            {
                continue;
            }

            for (int i = 0;
                 i < story.Conditions.Count;
                 i++)
            {
                StoryConditionData condition =
                    story.Conditions[i];

                if (condition == null)
                {
                    continue;
                }

                rows.Add(
                    new[]
                    {
                        story.StoryId,
                        i.ToString(),

                        condition
                            .ConditionType
                            .ToString(),

                        condition.Key,
                        condition.Value,

                        condition.IntValue
                            .ToString(),

                        string.Empty
                    }
                );
            }
        }

        batch.Stage(
            fileName,
            rows
        );
    }


    // =========================================================
    // StoryEffects.csv
    // =========================================================

    private static void ExportEffects(
        StoryCsvExportBatch batch,
        IReadOnlyList<StoryData> stories,
        string fileName = EffectsFile)
    {
        List<string[]> rows =
            new List<string[]>();

        rows.Add(
            new[]
            {
                "StoryId",
                "NodeId",
                "EffectIndex",
                "EffectType",
                "Target",
                "Duration",
                "Strength",
                "WaitForCompletion",
                "Memo"
            }
        );

        foreach (StoryData story
                 in stories)
        {
            if (story == null ||
                story.Nodes == null)
            {
                continue;
            }

            foreach (StoryNodeData node
                     in story.Nodes)
            {
                if (node == null ||
                    node.Effects == null)
                {
                    continue;
                }

                for (int effectIndex = 0;
                     effectIndex <
                     node.Effects.Count;
                     effectIndex++)
                {
                    StoryEffectData effect =
                        node.Effects[
                            effectIndex
                        ];

                    if (effect == null)
                    {
                        continue;
                    }

                    rows.Add(
                        new[]
                        {
                            story.StoryId,
                            node.NodeId,
                            effectIndex.ToString(),

                            effect.EffectType
                                .ToString(),

                            effect.Target
                                .ToString(),

                            effect.Duration
                                .ToString(
                                    CultureInfo
                                        .InvariantCulture
                                ),

                            effect.Strength
                                .ToString(
                                    CultureInfo
                                        .InvariantCulture
                                ),

                            ToCsvBool(
                                effect
                                    .WaitForCompletion
                            ),

                            string.Empty
                        }
                    );
                }
            }
        }

        batch.Stage(
            fileName,
            rows
        );
    }


    // =========================================================
    // Characters.csv
    // =========================================================

    private static void ExportCharacters(
        StoryCsvExportBatch batch,
        string fileName = CharactersFile)
    {
        string[] guids =
            AssetDatabase.FindAssets(
                "t:CharacterData"
            );

        List<CharacterData> characters =
            new List<CharacterData>();

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(
                    guid
                );

            CharacterData character =
                AssetDatabase.LoadAssetAtPath<CharacterData>(
                    path
                );

            if (character != null)
            {
                characters.Add(character);
            }
        }

        // CharacterId 기준 정렬
        characters.Sort(
            (a, b) =>
                string.Compare(
                    a.CharacterId,
                    b.CharacterId,
                    StringComparison.Ordinal
                )
        );

        List<string[]> rows =
            new List<string[]>();

        rows.Add(
            new[]
            {
            "CharacterId",
            "CharacterName",
            "CharacterNameEng",
            "PortraitId",
            "PortraitLabel",
            "SpriteName"
            }
        );

        foreach (CharacterData character
                 in characters)
        {
            if (character == null)
            {
                continue;
            }

            // -------------------------------------------------
            // 기본 초상화
            // -------------------------------------------------

            rows.Add(
                new[]
                {
                character.CharacterId,
                character.CharacterName,
                character.CharacterNameEng,

                string.Empty,

                "Default",

                character.Portrait != null
                    ? character.Portrait.name
                    : string.Empty
                }
            );

            // -------------------------------------------------
            // 스토리용 표정 초상화
            // -------------------------------------------------

            if (character.StoryPortraits == null)
            {
                continue;
            }

            foreach (CharacterPortraitData portrait
                     in character.StoryPortraits)
            {
                if (portrait == null)
                {
                    continue;
                }

                rows.Add(
                    new[]
                    {
                    character.CharacterId,
                    character.CharacterName,
                    character.CharacterNameEng,

                    portrait.PortraitId,
                    portrait.DisplayName,

                    portrait.PortraitSprite != null
                        ? portrait.PortraitSprite.name
                        : string.Empty
                    }
                );
            }
        }

        batch.Stage(
            fileName,
            rows
        );

        Debug.Log(
            $"[StoryCsvExporter] Character Export 완료: " +
            $"{characters.Count}명"
        );
    }


    // =========================================================
    // CSV 저장
    // =========================================================

    private static void ExportBackgrounds(StoryCsvExportBatch batch)
    {
        var names = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            { StoryBackgroundCatalog.HideId, "배경 숨기기" }
        };
        var sprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
        foreach (string guid in AssetDatabase.FindAssets("t:StoryBackgroundCatalog"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var catalog = AssetDatabase.LoadAssetAtPath<StoryBackgroundCatalog>(path);
            if (catalog == null || catalog.Entries == null) continue;
            foreach (var entry in catalog.Entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Id) || entry.Sprite == null) continue;
                if (!catalog.TryGetSprite(entry.Id, out _) || entry.Id == StoryBackgroundCatalog.HideId)
                    throw new InvalidDataException($"{path}: 중복 또는 예약된 배경 ID '{entry.Id}'");
                if (sprites.TryGetValue(entry.Id, out var previous) && previous != entry.Sprite)
                    throw new InvalidDataException($"{path}: '{entry.Id}'가 다른 카탈로그에서 다른 Sprite를 가리킵니다.");
                sprites[entry.Id] = entry.Sprite;
                names[entry.Id] = entry.DisplayName;
            }
        }
        var rows = new List<string[]> { new[] { "BackgroundId", "DisplayName" } };
        foreach (var pair in names) rows.Add(new[] { pair.Key, pair.Value });
        batch.Stage("Backgrounds.csv", rows);
    }

    private static string ToCsvBool(
        bool value)
    {
        return value
            ? "TRUE"
            : "FALSE";
    }


    // =========================================================
    // 폴더
    // =========================================================

    private static string MakeSafeFileName(
        string value)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            return "Story";
        }

        foreach (char invalidChar
                 in Path.GetInvalidFileNameChars())
        {
            value =
                value.Replace(
                    invalidChar,
                    '_'
                );
        }

        return value;
    }
}
