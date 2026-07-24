using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryRunner : MonoBehaviour
{
    [Header("스토리 UI")]
    [SerializeField]
    private StoryUI storyUI;

    [Header("테스트용 스토리")]
    [SerializeField]
    private StoryData testStory;

    [SerializeField]
    private bool playOnStart;

    [Header("플레이어 정보")]
    [Tooltip("저장된 이름을 찾지 못했을 때 사용할 기본 이름")]
    [SerializeField]
    private string defaultPlayerName = "서안";

    [SerializeField]
    private string playerJobTitle = "공작청 현장감독관";

    [Header("자동 진행")]
    [Min(0f)]
    [SerializeField]
    private float autoAdvanceDelay = 1.5f;

    [Header("스킵")]
    [Min(0f)]
    [SerializeField]
    private float skipAdvanceDelay = 0.05f;

    private StoryData currentStory;
    private StoryNodeData currentNode;

    private Coroutine nodeCoroutine;
    private Coroutine advanceCoroutine;

    private bool isStoryPlaying;
    private bool isWaitingForChoice;
    private bool isProcessingNode;

    private bool autoMode;
    private bool skipMode;

    private readonly Dictionary<string, string>
        storyResults = new();

    public bool IsStoryPlaying => isStoryPlaying;
    public bool AutoMode => autoMode;
    public bool SkipMode => skipMode;

    private string CurrentPlayerName
    {
        get
        {
            if (Datamanager.Instance == null)
            {
                return defaultPlayerName;
            }

            var saveData =
                Datamanager.Instance.saveData;

            if (saveData == null ||
                saveData.player == null ||
                string.IsNullOrWhiteSpace(
                    saveData.player.playerName))
            {
                return defaultPlayerName;
            }

            return saveData.player.playerName;
        }
    }

    private void Awake()
    {
        BindButtons();
    }

    private void Start()
    {
        if (storyUI == null)
        {
            Debug.LogError(
                "StoryUI가 연결되지 않았습니다.",
                this
            );

            return;
        }

        storyUI.SetAutoModeVisual(false);

        if (playOnStart &&
            testStory != null)
        {
            StartStory(testStory);
        }
    }

    private void BindButtons()
    {
        if (storyUI == null)
        {
            return;
        }

        Button autoButton =
            storyUI.GetAutoButton();

        Button skipButton =
            storyUI.GetSkipButton();

        Button logButton =
            storyUI.GetLogButton();

        if (autoButton != null)
        {
            autoButton.onClick.AddListener(
                ToggleAutoMode
            );
        }

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(
                ToggleSkipMode
            );
        }

        if (logButton != null)
        {
            logButton.onClick.AddListener(
                OpenStoryLog
            );
        }
    }

    public void StartStory(StoryData story)
    {
        if (story == null)
        {
            Debug.LogError(
                "실행할 StoryData가 없습니다.",
                this
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(
                story.StartNodeId))
        {
            Debug.LogError(
                $"[{story.name}] 시작 노드 ID가 없습니다.",
                story
            );

            return;
        }

        StopAllStoryCoroutines();

        currentStory = story;
        currentNode = null;

        isStoryPlaying = true;
        isWaitingForChoice = false;
        isProcessingNode = false;

        autoMode = false;
        skipMode = false;

        storyUI.Open();
        storyUI.SetAutoModeVisual(false);

        storyUI.ShowStoryTitle(
            story,
            () => MoveToNode(
                story.StartNodeId
            )
        );
    }

    public void OnClickNext()
    {
        if (!isStoryPlaying ||
            isWaitingForChoice)
        {
            return;
        }

        if (storyUI.IsTyping)
        {
            storyUI.CompleteTypingImmediately();
            return;
        }

        if (isProcessingNode)
        {
            return;
        }

        CancelScheduledAdvance();
        MoveToNextNode();
    }

    private void MoveToNode(string nodeId)
    {
        if (!isStoryPlaying)
        {
            return;
        }

        StoryNodeData nextNode =
            currentStory.GetNode(nodeId);

        if (nextNode == null)
        {
            Debug.LogError(
                $"[{currentStory.StoryId}] " +
                $"'{nodeId}' 노드를 찾을 수 없습니다.",
                currentStory
            );

            EndStory();
            return;
        }

        StopNodeCoroutine();

        currentNode = nextNode;

        nodeCoroutine = StartCoroutine(
            ProcessNode(currentNode)
        );
    }

    private IEnumerator ProcessNode(
        StoryNodeData node)
    {
        isProcessingNode = true;
        isWaitingForChoice = false;

        CancelScheduledAdvance();

        storyUI.HideChoices();
        storyUI.HideNextLineIcon();

        yield return ProcessEffects(node);

        switch (node.NodeType)
        {
            case StoryNodeType.CharacterDialogue:
                yield return ProcessCharacterDialogue(node);
                break;

            case StoryNodeType.PlayerDialogue:
                yield return ProcessPlayerDialogue(node);
                break;

            case StoryNodeType.Narration:
                yield return ProcessNarration(node);
                break;

            case StoryNodeType.Choice:
                ProcessChoice(node);
                break;

            case StoryNodeType.End:
                EndStory();
                break;
        }

        isProcessingNode = false;
        nodeCoroutine = null;

        if (!isStoryPlaying ||
            node.NodeType == StoryNodeType.Choice ||
            node.NodeType == StoryNodeType.End)
        {
            yield break;
        }

        storyUI.ShowNextLineIcon();

        ScheduleAutomaticAdvance(node);
    }

    private IEnumerator ProcessCharacterDialogue(
        StoryNodeData node)
    {
        CharacterData character =
            node.Character;

        if (character == null)
        {
            Debug.LogWarning(
                $"[{node.NodeId}] 캐릭터가 지정되지 않았습니다.",
                currentStory
            );

            storyUI.HideSpeaker();

            yield return ShowNodeText(node);
            yield break;
        }

        storyUI.SetCharacterDialogue(
            character,
            node.PortraitId
        );

        storyUI.SetCharacterDimmed(
            node.DimPortrait
        );

        yield return ShowNodeText(node);
    }

    private IEnumerator ProcessPlayerDialogue(
        StoryNodeData node)
    {
        storyUI.SetSpeaker(
            CurrentPlayerName,
            playerJobTitle
        );

        if (node.KeepPortrait)
        {
            storyUI.SetCharacterDimmed(true);
        }
        else
        {
            storyUI.HideCharacterImage();
        }

        yield return ShowNodeText(node);
    }

    private IEnumerator ProcessNarration(
        StoryNodeData node)
    {
        storyUI.HideSpeaker();

        if (node.KeepPortrait)
        {
            storyUI.SetCharacterDimmed(true);
        }
        else
        {
            storyUI.HideCharacterImage();
        }

        yield return ShowNodeText(node);
    }

    private IEnumerator ShowNodeText(
        StoryNodeData node)
    {
        string outputText =
            ReplaceTokens(node.Text);

        bool useTyping =
            node.UseTypingEffect &&
            !skipMode;

        yield return storyUI.ShowText(
            outputText,
            useTyping
        );
    }

    private void ProcessChoice(
        StoryNodeData node)
    {
        isWaitingForChoice = true;

        CancelScheduledAdvance();

        storyUI.HideSpeaker();
        storyUI.SetCharacterDimmed(true);
        storyUI.HideNextLineIcon();

        storyUI.ShowChoices(
            node.Choices,
            OnChoiceSelected
        );
    }

    private void OnChoiceSelected(
        StoryChoiceData choice)
    {
        if (!isWaitingForChoice ||
            choice == null)
        {
            return;
        }

        isWaitingForChoice = false;

        SaveChoiceResult(choice);

        storyUI.HideChoices();

        MoveToNode(
            choice.TargetNodeId
        );
    }

    private void SaveChoiceResult(
        StoryChoiceData choice)
    {
        if (string.IsNullOrWhiteSpace(
                choice.ResultKey))
        {
            return;
        }

        storyResults[choice.ResultKey] =
            choice.ResultValue;

        Debug.Log(
            $"선택 결과 저장: " +
            $"{choice.ResultKey} = " +
            $"{choice.ResultValue}"
        );
    }

    public string GetStoryResult(
        string key,
        string defaultValue = "")
    {
        return storyResults.TryGetValue(
            key,
            out string value)
            ? value
            : defaultValue;
    }

    private string ReplaceTokens(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return text
            .Replace(
                "{UserName}",
                CurrentPlayerName
            )
            .Replace(
                "{PlayerName}",
                CurrentPlayerName
            );
    }

    private void ScheduleAutomaticAdvance(
        StoryNodeData node)
    {
        bool shouldAdvance =
            skipMode ||
            autoMode ||
            node.AutoAdvance;

        if (!shouldAdvance)
        {
            return;
        }

        float delay;

        if (skipMode)
        {
            delay = skipAdvanceDelay;
        }
        else if (node.AutoAdvance)
        {
            delay = node.AutoAdvanceDelay;
        }
        else
        {
            delay = autoAdvanceDelay;
        }

        advanceCoroutine = StartCoroutine(
            AutomaticAdvanceRoutine(delay)
        );
    }

    private IEnumerator AutomaticAdvanceRoutine(
        float delay)
    {
        yield return new WaitForSecondsRealtime(
            delay
        );

        advanceCoroutine = null;

        if (!isStoryPlaying ||
            isWaitingForChoice)
        {
            yield break;
        }

        MoveToNextNode();
    }

    private void MoveToNextNode()
    {
        if (currentNode == null)
        {
            EndStory();
            return;
        }

        if (string.IsNullOrWhiteSpace(
                currentNode.NextNodeId))
        {
            EndStory();
            return;
        }

        MoveToNode(
            currentNode.NextNodeId
        );
    }

    public void ToggleAutoMode()
    {
        if (!isStoryPlaying)
        {
            return;
        }

        autoMode = !autoMode;

        if (autoMode)
        {
            skipMode = false;
        }

        storyUI.SetAutoModeVisual(
            autoMode
        );

        CancelScheduledAdvance();

        if (autoMode &&
            !isWaitingForChoice &&
            !storyUI.IsTyping &&
            !isProcessingNode)
        {
            ScheduleAutomaticAdvance(
                currentNode
            );
        }
    }

    public void ToggleSkipMode()
    {
        if (!isStoryPlaying)
        {
            return;
        }

        skipMode = !skipMode;

        if (skipMode)
        {
            autoMode = false;

            storyUI.SetAutoModeVisual(false);

            if (storyUI.IsTyping)
            {
                storyUI.CompleteTypingImmediately();
            }
        }

        CancelScheduledAdvance();

        if (skipMode &&
            !isWaitingForChoice &&
            !isProcessingNode)
        {
            ScheduleAutomaticAdvance(
                currentNode
            );
        }
    }

    private void OpenStoryLog()
    {
        Debug.Log(
            "스토리 로그 UI는 아직 연결되지 않았습니다."
        );
    }

    private IEnumerator ProcessEffects(
        StoryNodeData node)
    {
        foreach (StoryEffectData effect
                 in node.Effects)
        {
            if (effect == null ||
                effect.EffectType ==
                StoryEffectType.None)
            {
                continue;
            }

            Debug.Log(
                $"스토리 효과 요청: " +
                $"{effect.EffectType}"
            );

            if (effect.WaitForCompletion)
            {
                yield return new WaitForSecondsRealtime(
                    effect.Duration
                );
            }
        }
    }

    public void EndStory()
    {
        StopAllStoryCoroutines();

        isStoryPlaying = false;
        isWaitingForChoice = false;
        isProcessingNode = false;

        autoMode = false;
        skipMode = false;

        currentNode = null;
        currentStory = null;

        storyUI.SetAutoModeVisual(false);
        storyUI.Close();

        Debug.Log(
            "스토리가 종료되었습니다."
        );
    }

    private void CancelScheduledAdvance()
    {
        if (advanceCoroutine == null)
        {
            return;
        }

        StopCoroutine(
            advanceCoroutine
        );

        advanceCoroutine = null;
    }

    private void StopNodeCoroutine()
    {
        if (nodeCoroutine == null)
        {
            return;
        }

        StopCoroutine(
            nodeCoroutine
        );

        nodeCoroutine = null;
    }

    private void StopAllStoryCoroutines()
    {
        StopNodeCoroutine();
        CancelScheduledAdvance();
    }

    private void OnDestroy()
    {
        if (storyUI == null)
        {
            return;
        }

        Button autoButton =
            storyUI.GetAutoButton();

        Button skipButton =
            storyUI.GetSkipButton();

        Button logButton =
            storyUI.GetLogButton();

        if (autoButton != null)
        {
            autoButton.onClick.RemoveListener(
                ToggleAutoMode
            );
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(
                ToggleSkipMode
            );
        }

        if (logButton != null)
        {
            logButton.onClick.RemoveListener(
                OpenStoryLog
            );
        }
    }
}