using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class StoryCsvImporter
{
    // =========================================================
    // 경로
    // =========================================================

    private const string CsvFolder =
        "Assets/PHA/Story/Data/CSV";

    private const string StoryAssetFolder =
        "Assets/PHA/Story/Data/StoryAssets";

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
    // CSV 데이터용 내부 클래스
    // =========================================================

    private class StoryCsvData
    {
        public string storyId;
        public StoryType storyType;
        public string storyTitle;

        public int year;
        public int month;
        public int day;

        public int priority;
        public bool playOnce;

        public StoryPlayTiming playTiming;

        public string unlockFactionId;
        public string startNodeId;
    }


    private class NodeCsvData
    {
        public string storyId;

        public int order;

        public string nodeId;

        public StoryNodeType nodeType;

        public string text;

        public string characterId;
        public string portraitId;

        public string nextNodeId;

        public bool dimPortrait;
        public bool keepPortrait;

        public bool useTypingEffect;

        public bool autoAdvance;
        public float autoAdvanceDelay;
    }


    private class ChoiceCsvData
    {
        public string storyId;
        public string nodeId;

        public int choiceIndex;

        public string choiceText;
        public string targetNodeId;

        public string resultKey;
        public string resultValue;

        public bool useCondition;

        public string requiredKey;
        public string requiredValue;

        public bool hideWhenLocked;
        public string lockedText;
    }


    private class ConditionCsvData
    {
        public string storyId;

        public int conditionIndex;

        public StoryConditionType conditionType;

        public string key;
        public string value;

        public int intValue;
    }


    private class EffectCsvData
    {
        public string storyId;
        public string nodeId;

        public int effectIndex;

        public StoryEffectType effectType;
        public StoryEffectTarget target;

        public float duration;
        public float strength;

        public bool waitForCompletion;
    }


    // =========================================================
    // 메뉴
    // =========================================================

    [MenuItem("Story/CSV/Import All StoryData")]
    public static void ImportAllStoryData()
    {
        if (!ValidateCsvFiles())
        {
            return;
        }

        EnsureStoryAssetFolder();

        try
        {
            List<StoryCsvData> stories =
                LoadStories();

            List<NodeCsvData> nodes =
                LoadNodes();

            List<ChoiceCsvData> choices =
                LoadChoices();

            List<ConditionCsvData> conditions =
                LoadConditions();

            List<EffectCsvData> effects =
                LoadEffects();


            if (stories.Count == 0)
            {
                Debug.LogWarning(
                    "[StoryCsvImporter] " +
                    "Stories.csv에 StoryData가 없습니다."
                );

                return;
            }


            Dictionary<string, CharacterData>
                characters =
                    BuildCharacterDatabase();


            Dictionary<string, StoryData>
                existingStories =
                    BuildExistingStoryDatabase();


            int createdCount = 0;
            int updatedCount = 0;
            int errorCount = 0;


            foreach (StoryCsvData storyCsv
                     in stories)
            {
                if (string.IsNullOrWhiteSpace(
                        storyCsv.storyId))
                {
                    Debug.LogError(
                        "[StoryCsvImporter] " +
                        "StoryId가 비어 있는 행이 있습니다."
                    );

                    errorCount++;

                    continue;
                }


                StoryData storyAsset;

                bool isNew =
                    !existingStories.TryGetValue(
                        storyCsv.storyId,
                        out storyAsset
                    );


                if (isNew)
                {
                    storyAsset =
                        CreateStoryAsset(
                            storyCsv.storyId
                        );

                    if (storyAsset == null)
                    {
                        errorCount++;
                        continue;
                    }

                    existingStories[
                        storyCsv.storyId
                    ] = storyAsset;

                    createdCount++;
                }
                else
                {
                    updatedCount++;
                }


                bool success =
                    ApplyStoryData(
                        storyAsset,
                        storyCsv,
                        nodes,
                        choices,
                        conditions,
                        effects,
                        characters
                    );


                if (!success)
                {
                    errorCount++;
                }
            }


            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();


            Debug.Log(
                "[StoryCsvImporter] Import 완료\n" +
                $"생성: {createdCount}\n" +
                $"업데이트: {updatedCount}\n" +
                $"오류: {errorCount}"
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[StoryCsvImporter] Import 중 예외 발생\n" +
                exception
            );
        }
    }


    // =========================================================
    // StoryData 적용
    // =========================================================

    private static bool ApplyStoryData(
        StoryData storyAsset,
        StoryCsvData storyCsv,
        List<NodeCsvData> allNodes,
        List<ChoiceCsvData> allChoices,
        List<ConditionCsvData> allConditions,
        List<EffectCsvData> allEffects,
        Dictionary<string, CharacterData> characters)
    {
        SerializedObject serializedStory =
            new SerializedObject(
                storyAsset
            );


        // -----------------------------------------------------
        // 기본 정보
        // -----------------------------------------------------

        SetString(
            serializedStory,
            "storyId",
            storyCsv.storyId
        );

        SetEnum(
            serializedStory,
            "storyType",
            (int)storyCsv.storyType
        );

        SetString(
            serializedStory,
            "storyTitle",
            storyCsv.storyTitle
        );

        SetInt(
            serializedStory,
            "year",
            storyCsv.year
        );

        SetInt(
            serializedStory,
            "month",
            storyCsv.month
        );

        SetInt(
            serializedStory,
            "day",
            storyCsv.day
        );

        SetInt(
            serializedStory,
            "priority",
            storyCsv.priority
        );

        SetBool(
            serializedStory,
            "playOnce",
            storyCsv.playOnce
        );

        SetEnum(
            serializedStory,
            "playTiming",
            (int)storyCsv.playTiming
        );

        SetString(
            serializedStory,
            "unlockFactionId",
            storyCsv.unlockFactionId
        );

        SetString(
            serializedStory,
            "startNodeId",
            storyCsv.startNodeId
        );


        // -----------------------------------------------------
        // Conditions
        // -----------------------------------------------------

        List<ConditionCsvData> storyConditions =
            allConditions.FindAll(
                data =>
                    data.storyId ==
                    storyCsv.storyId
            );

        storyConditions.Sort(
            (a, b) =>
                a.conditionIndex.CompareTo(
                    b.conditionIndex
                )
        );


        SerializedProperty conditionsProperty =
            serializedStory.FindProperty(
                "conditions"
            );

        if (conditionsProperty != null)
        {
            conditionsProperty.ClearArray();

            foreach (ConditionCsvData condition
                     in storyConditions)
            {
                int index =
                    conditionsProperty.arraySize;

                conditionsProperty
                    .InsertArrayElementAtIndex(
                        index
                    );

                SerializedProperty element =
                    conditionsProperty
                        .GetArrayElementAtIndex(
                            index
                        );

                SetRelativeEnum(
                    element,
                    "conditionType",
                    (int)condition.conditionType
                );

                SetRelativeString(
                    element,
                    "key",
                    condition.key
                );

                SetRelativeString(
                    element,
                    "value",
                    condition.value
                );

                SetRelativeInt(
                    element,
                    "intValue",
                    condition.intValue
                );
            }
        }


        // -----------------------------------------------------
        // Nodes
        // -----------------------------------------------------

        List<NodeCsvData> storyNodes =
            allNodes.FindAll(
                data =>
                    data.storyId ==
                    storyCsv.storyId
            );

        storyNodes.Sort(
            (a, b) =>
                a.order.CompareTo(
                    b.order
                )
        );


        SerializedProperty nodesProperty =
            serializedStory.FindProperty(
                "nodes"
            );

        if (nodesProperty == null)
        {
            Debug.LogError(
                $"[{storyCsv.storyId}] " +
                "StoryData에서 'nodes' 필드를 " +
                "찾지 못했습니다.",
                storyAsset
            );

            return false;
        }


        nodesProperty.ClearArray();


        foreach (NodeCsvData nodeCsv
                 in storyNodes)
        {
            int index =
                nodesProperty.arraySize;

            nodesProperty
                .InsertArrayElementAtIndex(
                    index
                );

            SerializedProperty nodeProperty =
                nodesProperty
                    .GetArrayElementAtIndex(
                        index
                    );


            SetRelativeString(
                nodeProperty,
                "nodeId",
                nodeCsv.nodeId
            );

            SetRelativeEnum(
                nodeProperty,
                "nodeType",
                (int)nodeCsv.nodeType
            );

            SetRelativeString(
                nodeProperty,
                "text",
                nodeCsv.text
            );


            // Character
            SerializedProperty characterProperty =
                nodeProperty
                    .FindPropertyRelative(
                        "character"
                    );

            if (characterProperty != null)
            {
                characterProperty.objectReferenceValue =
                    FindCharacter(
                        characters,
                        nodeCsv.characterId,
                        storyCsv.storyId,
                        nodeCsv.nodeId
                    );
            }


            SetRelativeString(
                nodeProperty,
                "portraitId",
                nodeCsv.portraitId
            );

            SetRelativeString(
                nodeProperty,
                "nextNodeId",
                nodeCsv.nextNodeId
            );

            SetRelativeBool(
                nodeProperty,
                "dimPortrait",
                nodeCsv.dimPortrait
            );

            SetRelativeBool(
                nodeProperty,
                "keepPortrait",
                nodeCsv.keepPortrait
            );

            SetRelativeBool(
                nodeProperty,
                "useTypingEffect",
                nodeCsv.useTypingEffect
            );

            SetRelativeBool(
                nodeProperty,
                "autoAdvance",
                nodeCsv.autoAdvance
            );

            SetRelativeFloat(
                nodeProperty,
                "autoAdvanceDelay",
                nodeCsv.autoAdvanceDelay
            );


            // -------------------------------------------------
            // Choices
            // -------------------------------------------------

            SerializedProperty choicesProperty =
                nodeProperty
                    .FindPropertyRelative(
                        "choices"
                    );

            if (choicesProperty != null)
            {
                choicesProperty.ClearArray();


                List<ChoiceCsvData> nodeChoices =
                    allChoices.FindAll(
                        choice =>
                            choice.storyId ==
                            storyCsv.storyId &&
                            choice.nodeId ==
                            nodeCsv.nodeId
                    );


                nodeChoices.Sort(
                    (a, b) =>
                        a.choiceIndex.CompareTo(
                            b.choiceIndex
                        )
                );


                foreach (ChoiceCsvData choiceCsv
                         in nodeChoices)
                {
                    int choiceIndex =
                        choicesProperty.arraySize;

                    choicesProperty
                        .InsertArrayElementAtIndex(
                            choiceIndex
                        );

                    SerializedProperty choice =
                        choicesProperty
                            .GetArrayElementAtIndex(
                                choiceIndex
                            );


                    SetRelativeString(
                        choice,
                        "choiceText",
                        choiceCsv.choiceText
                    );

                    SetRelativeString(
                        choice,
                        "targetNodeId",
                        choiceCsv.targetNodeId
                    );

                    SetRelativeString(
                        choice,
                        "resultKey",
                        choiceCsv.resultKey
                    );

                    SetRelativeString(
                        choice,
                        "resultValue",
                        choiceCsv.resultValue
                    );

                    SetRelativeBool(
                        choice,
                        "useCondition",
                        choiceCsv.useCondition
                    );

                    SetRelativeString(
                        choice,
                        "requiredKey",
                        choiceCsv.requiredKey
                    );

                    SetRelativeString(
                        choice,
                        "requiredValue",
                        choiceCsv.requiredValue
                    );

                    SetRelativeBool(
                        choice,
                        "hideWhenLocked",
                        choiceCsv.hideWhenLocked
                    );

                    SetRelativeString(
                        choice,
                        "lockedText",
                        choiceCsv.lockedText
                    );
                }
            }


            // -------------------------------------------------
            // Effects
            // -------------------------------------------------

            SerializedProperty effectsProperty =
                nodeProperty
                    .FindPropertyRelative(
                        "effects"
                    );

            if (effectsProperty != null)
            {
                effectsProperty.ClearArray();


                List<EffectCsvData> nodeEffects =
                    allEffects.FindAll(
                        effect =>
                            effect.storyId ==
                            storyCsv.storyId &&
                            effect.nodeId ==
                            nodeCsv.nodeId
                    );


                nodeEffects.Sort(
                    (a, b) =>
                        a.effectIndex.CompareTo(
                            b.effectIndex
                        )
                );


                foreach (EffectCsvData effectCsv
                         in nodeEffects)
                {
                    int effectIndex =
                        effectsProperty.arraySize;

                    effectsProperty
                        .InsertArrayElementAtIndex(
                            effectIndex
                        );

                    SerializedProperty effect =
                        effectsProperty
                            .GetArrayElementAtIndex(
                                effectIndex
                            );


                    SetRelativeEnum(
                        effect,
                        "effectType",
                        (int)effectCsv.effectType
                    );

                    SetRelativeEnum(
                        effect,
                        "target",
                        (int)effectCsv.target
                    );

                    SetRelativeFloat(
                        effect,
                        "duration",
                        effectCsv.duration
                    );

                    SetRelativeFloat(
                        effect,
                        "strength",
                        effectCsv.strength
                    );

                    SetRelativeBool(
                        effect,
                        "waitForCompletion",
                        effectCsv.waitForCompletion
                    );
                }
            }
        }


        serializedStory
            .ApplyModifiedProperties();


        EditorUtility.SetDirty(
            storyAsset
        );


        Debug.Log(
            $"[StoryCsvImporter] " +
            $"적용 완료: {storyCsv.storyId}",
            storyAsset
        );


        return true;
    }


    // =========================================================
    // 기존 StoryData 검색
    // =========================================================

    private static Dictionary<string, StoryData>
        BuildExistingStoryDatabase()
    {
        Dictionary<string, StoryData> result =
            new Dictionary<string, StoryData>();


        string[] guids =
            AssetDatabase.FindAssets(
                "t:StoryData"
            );


        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(
                    guid
                );

            StoryData story =
                AssetDatabase
                    .LoadAssetAtPath<StoryData>(
                        path
                    );


            if (story == null ||
                string.IsNullOrWhiteSpace(
                    story.StoryId))
            {
                continue;
            }


            if (result.ContainsKey(
                    story.StoryId))
            {
                Debug.LogError(
                    "[StoryCsvImporter] " +
                    $"중복 StoryId 발견: " +
                    $"{story.StoryId}\n" +
                    $"Asset: {path}"
                );

                continue;
            }


            result.Add(
                story.StoryId,
                story
            );
        }


        return result;
    }


    // =========================================================
    // 신규 StoryData 생성
    // =========================================================

    private static StoryData CreateStoryAsset(
        string storyId)
    {
        StoryData story =
            ScriptableObject
                .CreateInstance<StoryData>();


        string fileName =
            $"Story_{MakeSafeFileName(storyId)}.asset";


        string path =
            Path.Combine(
                StoryAssetFolder,
                fileName
            )
            .Replace("\\", "/");


        /*
         * 혹시 같은 파일명이 이미 존재한다면
         * Unity가 고유 경로를 생성하도록 합니다.
         *
         * 일반적으로 StoryId 검색에서 먼저
         * 기존 asset을 찾으므로 여기까지 오는 경우는
         * 신규 StoryData입니다.
         */
        path =
            AssetDatabase
                .GenerateUniqueAssetPath(
                    path
                );


        AssetDatabase.CreateAsset(
            story,
            path
        );


        Debug.Log(
            $"[StoryCsvImporter] " +
            $"새 StoryData 생성: {path}",
            story
        );


        return story;
    }


    // =========================================================
    // CharacterData 검색
    // =========================================================

    private static Dictionary<string, CharacterData>
        BuildCharacterDatabase()
    {
        Dictionary<string, CharacterData> result =
            new Dictionary<string, CharacterData>();


        string[] guids =
            AssetDatabase.FindAssets(
                "t:CharacterData"
            );


        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(
                    guid
                );

            CharacterData character =
                AssetDatabase
                    .LoadAssetAtPath<CharacterData>(
                        path
                    );


            if (character == null ||
                string.IsNullOrWhiteSpace(
                    character.CharacterId))
            {
                continue;
            }


            if (result.ContainsKey(
                    character.CharacterId))
            {
                Debug.LogWarning(
                    "[StoryCsvImporter] " +
                    "중복 CharacterId: " +
                    character.CharacterId,
                    character
                );

                continue;
            }


            result.Add(
                character.CharacterId,
                character
            );
        }


        return result;
    }


    private static CharacterData FindCharacter(
        Dictionary<string, CharacterData> database,
        string characterId,
        string storyId,
        string nodeId)
    {
        if (string.IsNullOrWhiteSpace(
                characterId))
        {
            return null;
        }


        if (database.TryGetValue(
                characterId,
                out CharacterData character))
        {
            return character;
        }


        Debug.LogWarning(
            "[StoryCsvImporter] " +
            $"CharacterData를 찾지 못했습니다.\n" +
            $"Story: {storyId}\n" +
            $"Node: {nodeId}\n" +
            $"CharacterId: {characterId}"
        );


        return null;
    }


    // =========================================================
    // CSV Load
    // =========================================================

    private static List<StoryCsvData>
        LoadStories()
    {
        List<Dictionary<string, string>> rows =
            ReadCsv(
                GetCsvPath(StoriesFile)
            );


        List<StoryCsvData> result =
            new List<StoryCsvData>();


        foreach (var row in rows)
        {
            string storyId =
                Get(row, "StoryId");


            if (string.IsNullOrWhiteSpace(
                    storyId))
            {
                continue;
            }


            result.Add(
                new StoryCsvData
                {
                    storyId = storyId,

                    storyType =
                        ParseEnum(
                            Get(row, "StoryType"),
                            StoryType.Normal
                        ),

                    storyTitle =
                        Get(row, "StoryTitle"),

                    year =
                        ParseInt(
                            Get(row, "Year")
                        ),

                    month =
                        ParseInt(
                            Get(row, "Month")
                        ),

                    day =
                        ParseInt(
                            Get(row, "Day")
                        ),

                    priority =
                        ParseInt(
                            Get(row, "Priority"),
                            100
                        ),

                    playOnce =
                        ParseBool(
                            Get(row, "PlayOnce"),
                            true
                        ),

                    playTiming =
                        ParseEnum(
                            Get(row, "PlayTiming"),
                            StoryPlayTiming.Monthly
                        ),

                    unlockFactionId =
                        Get(
                            row,
                            "UnlockFactionId"
                        ),

                    startNodeId =
                        Get(
                            row,
                            "StartNodeId"
                        )
                }
            );
        }


        return result;
    }


    private static List<NodeCsvData>
        LoadNodes()
    {
        List<Dictionary<string, string>> rows =
            ReadCsv(
                GetCsvPath(NodesFile)
            );


        List<NodeCsvData> result =
            new List<NodeCsvData>();


        foreach (var row in rows)
        {
            string storyId =
                Get(row, "StoryId");

            string nodeId =
                Get(row, "NodeId");


            if (string.IsNullOrWhiteSpace(
                    storyId) ||
                string.IsNullOrWhiteSpace(
                    nodeId))
            {
                continue;
            }


            result.Add(
                new NodeCsvData
                {
                    storyId = storyId,

                    order =
                        ParseInt(
                            Get(row, "NodeOrder")
                        ),

                    nodeId = nodeId,

                    nodeType =
                        ParseEnum(
                            Get(row, "NodeType"),
                            StoryNodeType.Narration
                        ),

                    text =
                        Get(row, "Text"),

                    characterId =
                        Get(row, "CharacterId"),

                    portraitId =
                        Get(row, "PortraitId"),

                    nextNodeId =
                        Get(row, "NextNodeId"),

                    dimPortrait =
                        ParseBool(
                            Get(row, "DimPortrait")
                        ),

                    keepPortrait =
                        ParseBool(
                            Get(row, "KeepPortrait"),
                            true
                        ),

                    useTypingEffect =
                        ParseBool(
                            Get(row, "UseTypingEffect"),
                            true
                        ),

                    autoAdvance =
                        ParseBool(
                            Get(row, "AutoAdvance")
                        ),

                    autoAdvanceDelay =
                        ParseFloat(
                            Get(
                                row,
                                "AutoAdvanceDelay"
                            ),
                            1.5f
                        )
                }
            );
        }


        return result;
    }


    private static List<ChoiceCsvData>
        LoadChoices()
    {
        List<Dictionary<string, string>> rows =
            ReadCsv(
                GetCsvPath(ChoicesFile)
            );


        List<ChoiceCsvData> result =
            new List<ChoiceCsvData>();


        foreach (var row in rows)
        {
            string storyId =
                Get(row, "StoryId");

            string nodeId =
                Get(row, "NodeId");


            if (string.IsNullOrWhiteSpace(
                    storyId) ||
                string.IsNullOrWhiteSpace(
                    nodeId))
            {
                continue;
            }


            result.Add(
                new ChoiceCsvData
                {
                    storyId = storyId,
                    nodeId = nodeId,

                    choiceIndex =
                        ParseInt(
                            Get(row, "ChoiceIndex")
                        ),

                    choiceText =
                        Get(row, "ChoiceText"),

                    targetNodeId =
                        Get(row, "TargetNodeId"),

                    resultKey =
                        Get(row, "ResultKey"),

                    resultValue =
                        Get(row, "ResultValue"),

                    useCondition =
                        ParseBool(
                            Get(row, "UseCondition")
                        ),

                    requiredKey =
                        Get(row, "RequiredKey"),

                    requiredValue =
                        Get(row, "RequiredValue"),

                    hideWhenLocked =
                        ParseBool(
                            Get(row, "HideWhenLocked"),
                            true
                        ),

                    lockedText =
                        Get(row, "LockedText")
                }
            );
        }


        return result;
    }


    private static List<ConditionCsvData>
        LoadConditions()
    {
        string path =
            GetCsvPath(
                ConditionsFile
            );


        if (!File.Exists(path))
        {
            return new List<ConditionCsvData>();
        }


        List<Dictionary<string, string>> rows =
            ReadCsv(path);


        List<ConditionCsvData> result =
            new List<ConditionCsvData>();


        foreach (var row in rows)
        {
            string storyId =
                Get(row, "StoryId");


            if (string.IsNullOrWhiteSpace(
                    storyId))
            {
                continue;
            }


            result.Add(
                new ConditionCsvData
                {
                    storyId = storyId,

                    conditionIndex =
                        ParseInt(
                            Get(
                                row,
                                "ConditionIndex"
                            )
                        ),

                    conditionType =
                        ParseEnum(
                            Get(
                                row,
                                "ConditionType"
                            ),
                            StoryConditionType.None
                        ),

                    key =
                        Get(row, "Key"),

                    value =
                        Get(row, "Value"),

                    intValue =
                        ParseInt(
                            Get(row, "IntValue")
                        )
                }
            );
        }


        return result;
    }


    private static List<EffectCsvData>
        LoadEffects()
    {
        string path =
            GetCsvPath(
                EffectsFile
            );


        if (!File.Exists(path))
        {
            return new List<EffectCsvData>();
        }


        List<Dictionary<string, string>> rows =
            ReadCsv(path);


        List<EffectCsvData> result =
            new List<EffectCsvData>();


        foreach (var row in rows)
        {
            string storyId =
                Get(row, "StoryId");

            string nodeId =
                Get(row, "NodeId");


            if (string.IsNullOrWhiteSpace(
                    storyId) ||
                string.IsNullOrWhiteSpace(
                    nodeId))
            {
                continue;
            }


            result.Add(
                new EffectCsvData
                {
                    storyId = storyId,
                    nodeId = nodeId,

                    effectIndex =
                        ParseInt(
                            Get(row, "EffectIndex")
                        ),

                    effectType =
                        ParseEnum(
                            Get(row, "EffectType"),
                            StoryEffectType.None
                        ),

                    target =
                        ParseEnum(
                            Get(row, "Target"),
                            StoryEffectTarget.Portrait
                        ),

                    duration =
                        ParseFloat(
                            Get(row, "Duration")
                        ),

                    strength =
                        ParseFloat(
                            Get(row, "Strength"),
                            1f
                        ),

                    waitForCompletion =
                        ParseBool(
                            Get(
                                row,
                                "WaitForCompletion"
                            )
                        )
                }
            );
        }


        return result;
    }


    // =========================================================
    // CSV Parser
    // =========================================================

    /*
     * 단순 Split(',')를 사용하지 않습니다.
     *
     * 대사에 쉼표가 있거나,
     * Excel 셀 안에 줄바꿈이 들어가 있어도
     * 정상적으로 읽기 위한 CSV Parser입니다.
     */
    private static List<Dictionary<string, string>>
        ReadCsv(string path)
    {
        string csv =
            File.ReadAllText(
                path,
                Encoding.UTF8
            );


        List<List<string>> rawRows =
            ParseCsv(csv);


        List<Dictionary<string, string>> result =
            new List<Dictionary<string, string>>();


        if (rawRows.Count == 0)
        {
            return result;
        }


        List<string> headers =
            rawRows[0];


        for (int rowIndex = 1;
             rowIndex < rawRows.Count;
             rowIndex++)
        {
            List<string> row =
                rawRows[rowIndex];


            bool hasValue = false;

            foreach (string value in row)
            {
                if (!string.IsNullOrWhiteSpace(
                        value))
                {
                    hasValue = true;
                    break;
                }
            }


            if (!hasValue)
            {
                continue;
            }


            Dictionary<string, string>
                dictionary =
                    new Dictionary<string, string>(
                        StringComparer
                            .OrdinalIgnoreCase
                    );


            for (int column = 0;
                 column < headers.Count;
                 column++)
            {
                string header =
                    headers[column]
                        .Trim()
                        .TrimStart('\uFEFF');


                string value =
                    column < row.Count
                        ? row[column]
                        : string.Empty;


                dictionary[header] =
                    value;
            }


            result.Add(
                dictionary
            );
        }


        return result;
    }


    private static List<List<string>>
        ParseCsv(string csv)
    {
        List<List<string>> rows =
            new List<List<string>>();

        List<string> currentRow =
            new List<string>();

        StringBuilder currentField =
            new StringBuilder();


        bool insideQuotes = false;


        for (int i = 0;
             i < csv.Length;
             i++)
        {
            char c =
                csv[i];


            if (insideQuotes)
            {
                if (c == '"')
                {
                    /*
                     * "" 는 CSV 내부에서
                     * 실제 " 문자를 의미합니다.
                     */
                    if (i + 1 < csv.Length &&
                        csv[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++;
                    }
                    else
                    {
                        insideQuotes = false;
                    }
                }
                else
                {
                    currentField.Append(c);
                }


                continue;
            }


            if (c == '"')
            {
                insideQuotes = true;
            }
            else if (c == ',')
            {
                currentRow.Add(
                    currentField.ToString()
                );

                currentField.Clear();
            }
            else if (c == '\r')
            {
                /*
                 * Windows CRLF는
                 * 다음 \n에서 처리합니다.
                 */
            }
            else if (c == '\n')
            {
                currentRow.Add(
                    currentField.ToString()
                );

                currentField.Clear();


                rows.Add(
                    currentRow
                );

                currentRow =
                    new List<string>();
            }
            else
            {
                currentField.Append(c);
            }
        }


        /*
         * 마지막 행이 개행으로 끝나지 않는 경우
         */
        if (currentField.Length > 0 ||
            currentRow.Count > 0)
        {
            currentRow.Add(
                currentField.ToString()
            );

            rows.Add(
                currentRow
            );
        }


        return rows;
    }


    // =========================================================
    // SerializedProperty Helper
    // =========================================================

    private static void SetString(
        SerializedObject obj,
        string propertyName,
        string value)
    {
        SerializedProperty property =
            obj.FindProperty(propertyName);

        if (property != null)
        {
            property.stringValue =
                value ?? string.Empty;
        }
    }


    private static void SetInt(
        SerializedObject obj,
        string propertyName,
        int value)
    {
        SerializedProperty property =
            obj.FindProperty(propertyName);

        if (property != null)
        {
            property.intValue = value;
        }
    }


    private static void SetBool(
        SerializedObject obj,
        string propertyName,
        bool value)
    {
        SerializedProperty property =
            obj.FindProperty(propertyName);

        if (property != null)
        {
            property.boolValue = value;
        }
    }


    private static void SetEnum(
        SerializedObject obj,
        string propertyName,
        int value)
    {
        SerializedProperty property =
            obj.FindProperty(propertyName);

        if (property != null)
        {
            property.enumValueIndex = value;
        }
    }


    private static void SetRelativeString(
        SerializedProperty parent,
        string name,
        string value)
    {
        SerializedProperty property =
            parent.FindPropertyRelative(name);

        if (property != null)
        {
            property.stringValue =
                value ?? string.Empty;
        }
    }


    private static void SetRelativeInt(
        SerializedProperty parent,
        string name,
        int value)
    {
        SerializedProperty property =
            parent.FindPropertyRelative(name);

        if (property != null)
        {
            property.intValue = value;
        }
    }


    private static void SetRelativeFloat(
        SerializedProperty parent,
        string name,
        float value)
    {
        SerializedProperty property =
            parent.FindPropertyRelative(name);

        if (property != null)
        {
            property.floatValue = value;
        }
    }


    private static void SetRelativeBool(
        SerializedProperty parent,
        string name,
        bool value)
    {
        SerializedProperty property =
            parent.FindPropertyRelative(name);

        if (property != null)
        {
            property.boolValue = value;
        }
    }


    private static void SetRelativeEnum(
        SerializedProperty parent,
        string name,
        int value)
    {
        SerializedProperty property =
            parent.FindPropertyRelative(name);

        if (property != null)
        {
            property.enumValueIndex = value;
        }
    }


    // =========================================================
    // Parsing Helper
    // =========================================================

    private static string Get(
        Dictionary<string, string> row,
        string key)
    {
        if (row.TryGetValue(
                key,
                out string value))
        {
            return value?.Trim()
                ?? string.Empty;
        }

        return string.Empty;
    }


    private static int ParseInt(
        string value,
        int defaultValue = 0)
    {
        if (int.TryParse(
                value,
                out int result))
        {
            return result;
        }

        return defaultValue;
    }


    private static float ParseFloat(
        string value,
        float defaultValue = 0f)
    {
        if (float.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float result))
        {
            return result;
        }

        return defaultValue;
    }


    private static bool ParseBool(
        string value,
        bool defaultValue = false)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            return defaultValue;
        }


        if (bool.TryParse(
                value,
                out bool result))
        {
            return result;
        }


        if (value == "1")
        {
            return true;
        }

        if (value == "0")
        {
            return false;
        }


        return defaultValue;
    }


    private static T ParseEnum<T>(
        string value,
        T defaultValue)
        where T : struct
    {
        if (Enum.TryParse(
                value,
                true,
                out T result))
        {
            return result;
        }

        return defaultValue;
    }


    // =========================================================
    // 경로
    // =========================================================

    private static bool ValidateCsvFiles()
    {
        string stories =
            GetCsvPath(
                StoriesFile
            );

        string nodes =
            GetCsvPath(
                NodesFile
            );

        string choices =
            GetCsvPath(
                ChoicesFile
            );


        if (!File.Exists(stories))
        {
            Debug.LogError(
                $"CSV가 없습니다: {stories}"
            );

            return false;
        }


        if (!File.Exists(nodes))
        {
            Debug.LogError(
                $"CSV가 없습니다: {nodes}"
            );

            return false;
        }


        if (!File.Exists(choices))
        {
            Debug.LogError(
                $"CSV가 없습니다: {choices}"
            );

            return false;
        }


        return true;
    }


    private static string GetCsvPath(
        string fileName)
    {
        return Path.Combine(
            CsvFolder,
            fileName
        );
    }


    private static void EnsureStoryAssetFolder()
    {
        if (!Directory.Exists(
                StoryAssetFolder))
        {
            Directory.CreateDirectory(
                StoryAssetFolder
            );

            AssetDatabase.Refresh();
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


        foreach (char invalid
                 in Path.GetInvalidFileNameChars())
        {
            value =
                value.Replace(
                    invalid,
                    '_'
                );
        }


        return value;
    }
}