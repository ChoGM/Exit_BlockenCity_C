using System.Collections.Generic;
using UnityEngine;

public enum StoryType
{
    Normal,     // 노말 스토리
    Force,      // 세력 스토리
    Unlock,     // 해금 스토리
    Branch,     // 분기 스토리
    Event,      // 세력 이벤트 스토리
}

[CreateAssetMenu(
    fileName = "StoryData",
    menuName = "Game/Story/Story Data"
)]
public class StoryData : ScriptableObject
{
    [Header("스토리 기본 정보")]

    [Tooltip("전체 게임에서 중복되지 않는 스토리 ID")]
    [SerializeField]
    private string storyId;

    [SerializeField]
    private StoryType storyType;

    [SerializeField]
    private string storyTitle;

    [Header("스토리 진행 날짜")]

    [Min(0)]
    [SerializeField]
    private int year;

    [Range(1, 12)]
    [SerializeField]
    private int month = 1;

    [Range(0, 31)]
    [SerializeField]
    private int day;

    [Header("스토리 노드")]

    [Tooltip("스토리를 시작할 노드 ID")]
    [SerializeField]
    private string startNodeId;

    [SerializeField]
    private List<StoryNodeData> nodes = new();

    public string StoryId => storyId;
    public StoryType StoryType => storyType;
    public string StoryTitle => storyTitle;

    public int Year => year;
    public int Month => month;
    public int Day => day;

    public string StartNodeId => startNodeId;

    public IReadOnlyList<StoryNodeData> Nodes =>
        nodes;

    public string GetTypeDisplayName()
    {
        return storyType switch
        {
            StoryType.Normal => "노말 스토리",
            StoryType.Force => "세력 스토리",
            StoryType.Unlock => "해금 스토리",
            StoryType.Branch => "분기 스토리",
            StoryType.Event => "이벤트 스토리",
            _ => storyType.ToString()
        };
    }

    public string GetDateDisplayText()
    {
        if (year <= 0 && day <= 0)
        {
            return $"{month}월";
        }

        if (year <= 0)
        {
            return $"{month}월 {day}일";
        }

        if (day <= 0)
        {
            return $"{year}년 {month}월";
        }

        return $"{year}년 {month}월 {day}일";
    }

    public StoryNodeData GetNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return null;
        }

        return nodes.Find(node =>
            node != null &&
            node.NodeId == nodeId
        );
    }

    private void OnValidate()
    {
        ValidateStory();
    }

    [ContextMenu("스토리 데이터 검사")]
    public void ValidateStory()
    {
        if (nodes == null)
        {
            nodes = new List<StoryNodeData>();
            return;
        }

        HashSet<string> nodeIds = new();

        foreach (StoryNodeData node in nodes)
        {
            if (node == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(node.NodeId))
            {
                Debug.LogWarning(
                    $"[{name}] ID가 비어 있는 노드가 있습니다.",
                    this
                );

                continue;
            }

            if (!nodeIds.Add(node.NodeId))
            {
                Debug.LogError(
                    $"[{name}] 중복 노드 ID: {node.NodeId}",
                    this
                );
            }
        }

        if (!string.IsNullOrWhiteSpace(startNodeId) &&
            !nodeIds.Contains(startNodeId))
        {
            Debug.LogError(
                $"[{name}] 시작 노드 '{startNodeId}'를 " +
                "찾을 수 없습니다.",
                this
            );
        }

        foreach (StoryNodeData node in nodes)
        {
            if (node == null)
            {
                continue;
            }

            ValidateNextNode(node, nodeIds);
            ValidateChoices(node, nodeIds);
        }
    }

    private void ValidateNextNode(
        StoryNodeData node,
        HashSet<string> nodeIds)
    {
        if (node.NodeType == StoryNodeType.Choice ||
            node.NodeType == StoryNodeType.End)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(node.NextNodeId))
        {
            Debug.LogWarning(
                $"[{name}] '{node.NodeId}' 노드의 " +
                "다음 노드가 비어 있습니다.",
                this
            );

            return;
        }

        if (!nodeIds.Contains(node.NextNodeId))
        {
            Debug.LogError(
                $"[{name}] 존재하지 않는 다음 노드: " +
                $"{node.NodeId} → {node.NextNodeId}",
                this
            );
        }
    }

    private void ValidateChoices(
        StoryNodeData node,
        HashSet<string> nodeIds)
    {
        if (node.NodeType != StoryNodeType.Choice)
        {
            return;
        }

        if (node.Choices == null ||
            node.Choices.Count == 0)
        {
            Debug.LogWarning(
                $"[{name}] 선택지 노드 '{node.NodeId}'에 " +
                "선택지가 없습니다.",
                this
            );

            return;
        }

        foreach (StoryChoiceData choice in node.Choices)
        {
            if (choice == null)
            {
                continue;
            }

            if (!nodeIds.Contains(choice.TargetNodeId))
            {
                Debug.LogError(
                    $"[{name}] 존재하지 않는 선택지 목적지: " +
                    $"{node.NodeId} → {choice.TargetNodeId}",
                    this
                );
            }
        }
    }
}