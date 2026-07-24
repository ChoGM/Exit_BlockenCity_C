using System;
using System.Collections.Generic;
using UnityEngine;

public enum StoryNodeType
{
    CharacterDialogue,  // 캐릭터 대사
    PlayerDialogue,     // 플레이어 대사
    Narration,          // 나레이션
    Choice,             // 선택지
    End,                // 끝
}

[Serializable]
public class StoryNodeData
{
    [Header("노드 기본 정보")]

    [Tooltip("스토리 안에서 중복되지 않는 노드 ID")]
    [SerializeField]
    private string nodeId;

    [SerializeField]
    private StoryNodeType nodeType;

    [Header("출력할 텍스트")]

    [TextArea(3, 10)]
    [SerializeField]
    private string text;

    [Header("캐릭터 대사 설정")]

    [Tooltip("캐릭터 대사일 때만 지정")]
    [SerializeField]
    private CharacterData character;

    [Tooltip("CharacterData에 등록된 표정 ID")]
    [SerializeField]
    private string portraitId;

    [Header("다음 노드")]

    [Tooltip("현재 노드가 끝난 뒤 이동할 노드 ID")]
    [SerializeField]
    private string nextNodeId;

    [Header("선택지")]

    [Tooltip("Choice 타입일 때 사용")]
    [SerializeField]
    private List<StoryChoiceData> choices = new();

    [Header("화면 출력 설정")]

    [Tooltip("현재 캐릭터 초상화를 어둡게 표시")]
    [SerializeField]
    private bool dimPortrait;

    [Tooltip("현재 캐릭터 초상화를 유지")]
    [SerializeField]
    private bool keepPortrait = true;

    [Tooltip("텍스트 타이핑 효과 사용")]
    [SerializeField]
    private bool useTypingEffect = true;

    [Tooltip("클릭 없이 자동으로 다음 노드 진행")]
    [SerializeField]
    private bool autoAdvance;

    [Min(0f)]
    [SerializeField]
    private float autoAdvanceDelay = 1f;

    [Header("연출")]

    [SerializeField]
    private List<StoryEffectData> effects = new();

    public string NodeId => nodeId;
    public StoryNodeType NodeType => nodeType;
    public string Text => text;

    public CharacterData Character => character;
    public string PortraitId => portraitId;

    public string NextNodeId => nextNodeId;

    public IReadOnlyList<StoryChoiceData> Choices =>
        choices;

    public bool DimPortrait => dimPortrait;
    public bool KeepPortrait => keepPortrait;
    public bool UseTypingEffect => useTypingEffect;

    public bool AutoAdvance => autoAdvance;
    public float AutoAdvanceDelay => autoAdvanceDelay;

    public IReadOnlyList<StoryEffectData> Effects =>
        effects;
}