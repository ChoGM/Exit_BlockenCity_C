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


    // =========================================================
    // 전체 StoryData Export
    // =========================================================

    [MenuItem("Story/CSV/Export All StoryData")]
    public static void ExportAllStoryData()
    {
        EnsureExportFolder();

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

        ExportStories(stories);
        ExportNodes(stories);
        ExportChoices(stories);
        ExportConditions(stories);
        ExportEffects(stories);

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

        EnsureExportFolder();

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
            stories,
            $"Stories_{safeId}.csv"
        );

        ExportNodes(
            stories,
            $"StoryNodes_{safeId}.csv"
        );

        ExportChoices(
            stories,
            $"StoryChoices_{safeId}.csv"
        );

        ExportConditions(
            stories,
            $"StoryConditions_{safeId}.csv"
        );

        ExportEffects(
            stories,
            $"StoryEffects_{safeId}.csv"
        );

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
                    string.Empty
                }
            );
        }

        WriteCsv(
            fileName,
            rows
        );
    }


    // =========================================================
    // StoryNodes.csv
    // =========================================================

    private static void ExportNodes(
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

                        string.Empty
                    }
                );
            }
        }

        WriteCsv(
            fileName,
            rows
        );
    }


    // =========================================================
    // StoryChoices.csv
    // =========================================================

    private static void ExportChoices(
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

        WriteCsv(
            fileName,
            rows
        );
    }


    // =========================================================
    // StoryConditions.csv
    // =========================================================

    private static void ExportConditions(
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

        WriteCsv(
            fileName,
            rows
        );
    }


    // =========================================================
    // StoryEffects.csv
    // =========================================================

    private static void ExportEffects(
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

        WriteCsv(
            fileName,
            rows
        );
    }


    // =========================================================
    // CSV 저장
    // =========================================================

    private static void WriteCsv(
        string fileName,
        IReadOnlyList<string[]> rows)
    {
        string path =
            Path.Combine(
                ExportFolder,
                fileName
            );

        StringBuilder builder =
            new StringBuilder();

        foreach (string[] row in rows)
        {
            for (int columnIndex = 0;
                 columnIndex < row.Length;
                 columnIndex++)
            {
                if (columnIndex > 0)
                {
                    builder.Append(',');
                }

                builder.Append(
                    EscapeCsv(
                        row[columnIndex]
                    )
                );
            }

            builder.AppendLine();
        }

        /*
         * BOM이 있는 UTF-8.
         *
         * Windows Excel에서 CSV를 열었을 때
         * 한글이 깨질 가능성을 줄입니다.
         */
        UTF8Encoding encoding =
            new UTF8Encoding(true);

        File.WriteAllText(
            path,
            builder.ToString(),
            encoding
        );

        Debug.Log(
            $"[StoryCsvExporter] 생성: {path}"
        );
    }


    // =========================================================
    // CSV 문자열 처리
    // =========================================================

    private static string EscapeCsv(
        string value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        /*
         * 줄바꿈도 그대로 보존합니다.
         *
         * CSV 규칙상
         *
         * "안녕하세요.
         * 반갑습니다."
         *
         * 형태로 저장됩니다.
         */

        bool requiresQuotes =
            value.Contains(",") ||
            value.Contains("\"") ||
            value.Contains("\n") ||
            value.Contains("\r");

        string escaped =
            value.Replace(
                "\"",
                "\"\""
            );

        if (requiresQuotes)
        {
            return $"\"{escaped}\"";
        }

        return escaped;
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

    private static void EnsureExportFolder()
    {
        if (!Directory.Exists(
                ExportFolder))
        {
            Directory.CreateDirectory(
                ExportFolder
            );
        }
    }


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