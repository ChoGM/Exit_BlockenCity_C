using System;
using UnityEngine;

[Serializable]
public class StoryChoiceData
{
    [TextArea(2, 4)]
    [SerializeField]
    private string choiceText;

    [Tooltip("이 선택지를 골랐을 때 이동할 노드 ID")]
    [SerializeField]
    private string targetNodeId;

    [Tooltip("선택 결과를 저장할 키. 비워도 됩니다.")]
    [SerializeField]
    private string resultKey;

    [Tooltip("선택 결과로 저장할 값. 비워도 됩니다.")]
    [SerializeField]
    private string resultValue;

    public string ChoiceText => choiceText;
    public string TargetNodeId => targetNodeId;
    public string ResultKey => resultKey;
    public string ResultValue => resultValue;
}