using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryRunner : MonoBehaviour
{
    [Header("스토리 UI")]
    [SerializeField]
    private StoryUI storyUI;

    [Header("스토리 배경")]
    [SerializeField] private BackgroundTestController backgroundController;

    [Header("스토리 시퀀스")]
    [SerializeField]
    private StorySequenceController sequenceController;

    [Header("월 완료 처리")]
    [SerializeField]
    private StoryMonthCompletionController
    monthCompletionController;

    [Header("테스트용 스토리")]
    [SerializeField]
    private bool useTestStory;

    [SerializeField]
    private StoryData testStory;

    [SerializeField]
    private bool playOnStart;

    [Header("플레이어 정보")]
    [Tooltip("저장된 이름을 찾지 못했을 때 사용할 기본 이름")]
    [SerializeField]
    private string defaultPlayerName = "서안";

    [SerializeField]
    private string playerJobTitle =
        "공작청 현장감독관";

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


    /*
     * 현재 실행 중인 스토리 안에서 발생한
     * 선택 결과를 임시로 보관합니다.
     */
    private readonly Dictionary<string, string>
        storyResults = new();

    /*
     * 외부 시스템에서 임시로 등록하는 조건입니다.
     *
     * 예:
     * SetStoryUnlock("MetHongryeon", true);
     */
    private readonly Dictionary<string, string>
        storyConditions = new();

    /*
     * 완료 스토리, 세력 소개 여부,
     * 선택 결과 저장 등을 담당합니다.
     */
    private StoryProgressService progressService;

    public bool IsStoryPlaying =>
        isStoryPlaying;

    public bool AutoMode =>
        autoMode;

    public bool SkipMode =>
        skipMode;

    private string CurrentPlayerName
    {
        get
        {
            if (Datamanager.Instance == null)
            {
                return defaultPlayerName;
            }

            SaveData saveData =
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
        if (backgroundController == null)
            backgroundController = GetComponent<BackgroundTestController>();
        progressService =
            new StoryProgressService();

        FindSequenceController();
        FindMonthCompletionController();

        if (sequenceController != null)
        {
            sequenceController.Initialize(
                progressService
            );
        }

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

        ApplyAdvanceModeSetting();

        if (!playOnStart)
        {
            return;
        }

        /*
         * 테스트 모드가 켜져 있으면
         * 특정 StoryData만 실행합니다.
         */
        if (useTestStory)
        {
            if (testStory == null)
            {
                Debug.LogWarning(
                    "테스트용 StoryData가 연결되지 않았습니다.",
                    this
                );

                return;
            }

            StartStory(testStory);
            return;
        }

        /*
         * 실제 게임에서는 현재 월과 조건에 맞는
         * 스토리 시퀀스를 생성합니다.
         */
        StartCurrentStorySequence();
    }

    private void OnEnable()
    {
        if (StorySettingsManager.Instance != null)
        {
            StorySettingsManager.Instance
                .OnStorySettingsChanged +=
                ApplyAdvanceModeSetting;
        }
    }

    private void OnDisable()
    {
        if (backgroundController != null)
            backgroundController.CompleteTransition();
        if (StorySettingsManager.Instance != null)
        {
            StorySettingsManager.Instance
                .OnStorySettingsChanged -=
                ApplyAdvanceModeSetting;
        }
    }

    private void FindSequenceController()
    {
        if (sequenceController != null)
        {
            return;
        }

        sequenceController =
            GetComponent<StorySequenceController>();

        if (sequenceController == null)
        {
            Debug.LogError(
                "StorySequenceController가 연결되지 않았습니다. " +
                "StoryRunner와 같은 오브젝트에 추가하거나 " +
                "Inspector에서 직접 연결해주세요.",
                this
            );
        }
    }

    private void BindButtons()
    {
        if (storyUI == null)
        {
            return;
        }

        Button[] autoButtons =
            storyUI.GetAutoModeButtons();

        foreach (Button button in autoButtons)
        {
            if (button == null)
            {
                continue;
            }

            button.onClick.RemoveListener(
                ToggleAutoMode
            );

            button.onClick.AddListener(
                ToggleAutoMode
            );
        }

        Button skipButton =
            storyUI.GetSkipButton();

        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(
                ToggleSkipMode
            );

            skipButton.onClick.AddListener(
                ToggleSkipMode
            );
        }
    }

    /*
     * 현재 월과 저장된 조건을 기준으로
     * StorySequenceController에 스토리 목록 생성을 요청합니다.
     */
    public void StartCurrentStorySequence()
    {
        if (sequenceController == null)
        {
            FindSequenceController();
        }

        if (sequenceController == null)
        {
            return;
        }

        /*
         * 이전 테스트 스토리에서 남은
         * 현재 실행 중 선택 결과를 초기화합니다.
         */
        storyResults.Clear();

        bool sequenceCreated =
            sequenceController.BuildCurrentSequence();

        if (!sequenceCreated)
        {
            Debug.LogWarning(
                "현재 실행할 수 있는 스토리 시퀀스가 없습니다.",
                this
            );

            return;
        }

        PlayNextQueuedStory();
    }

    /*
     * StorySequenceController가 골라놓은
     * 다음 StoryData를 받아 실행합니다.
     */
    private void PlayNextQueuedStory()
    {
        if (sequenceController == null)
        {
            return;
        }

        bool hasNextStory =
            sequenceController.TryGetNextStory(
                out StoryData nextStory
            );

        if (!hasNextStory ||
            nextStory == null)
        {
            Debug.Log(
                "현재 시점의 모든 스토리를 완료했습니다.",
                this
            );

            return;
        }

        StartStory(nextStory);
    }

    /*
     * 전달받은 StoryData의 실제 재생을 시작합니다.
     */
    public void StartStory(
        StoryData story)
    {
        if (story == null)
        {
            Debug.LogError(
                "실행할 StoryData가 없습니다.",
                this
            );

            return;
        }

        if (storyUI == null)
        {
            Debug.LogError(
                "StoryUI가 연결되지 않았습니다.",
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

        autoMode =
            StorySettingsManager.Instance != null &&
            StorySettingsManager.Instance.IsAutoAdvance();

        skipMode = false;

        storyUI.ClearLogEntries();

        storyUI.Open();

        if (backgroundController != null)
            backgroundController.BeginStory(story);
        else if (!string.IsNullOrWhiteSpace(story.DefaultBackgroundId))
            Debug.LogError($"[{story.StoryId}] 시작 배경을 표시할 BackgroundTestController를 연결하세요.", this);

        storyUI.SetAutoModeVisual(
            autoMode
        );

        storyUI.ShowStoryTitle(
            story,
            () =>
            {
                MoveToNode(
                    story.StartNodeId
                );
            }
        );
    }

    /*
     * 대화창의 다음 버튼을 눌렀을 때 호출합니다.
     */
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

    private void MoveToNode(
        string nodeId)
    {
        if (!isStoryPlaying ||
            currentStory == null)
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
        if (node == null)
        {
            EndStory();
            yield break;
        }

        isProcessingNode = true;
        isWaitingForChoice = false;

        CancelScheduledAdvance();

        storyUI.HideChoices();
        storyUI.HideNextLineIcon();

        if (!string.IsNullOrWhiteSpace(node.BackgroundId))
        {
            if (backgroundController != null)
                yield return backgroundController.Show(node, currentStory.StoryId, skipMode);
            else
                Debug.LogError($"[{currentStory.StoryId}/{node.NodeId}] 배경을 표시할 BackgroundTestController를 연결하세요.", this);
        }
        yield return ProcessEffects(node);

        switch (node.NodeType)
        {
            case StoryNodeType.CharacterDialogue:
                yield return ProcessCharacterDialogue(
                    node
                );
                break;

            case StoryNodeType.PlayerDialogue:
                yield return ProcessPlayerDialogue(
                    node
                );
                break;

            case StoryNodeType.Narration:
                yield return ProcessNarration(
                    node
                );
                break;

            case StoryNodeType.Choice:
                ProcessChoice(node);
                break;

            case StoryNodeType.End:
                EndStory();
                break;

            default:
                Debug.LogWarning(
                    $"지원하지 않는 노드 타입: " +
                    $"{node.NodeType}",
                    currentStory
                );

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
                $"[{node.NodeId}] " +
                "캐릭터가 지정되지 않았습니다.",
                currentStory
            );

            storyUI.HideSpeaker();

            yield return ShowNodeText(node);

            AddNodeToLog(
                node,
                string.Empty
            );

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

        AddNodeToLog(
            node,
            character.CharacterName
        );
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
            storyUI.SetCharacterDimmed(
                node.DimPortrait
            );
        }
        else
        {
            storyUI.HideCharacterImage();
        }

        yield return ShowNodeText(node);

        AddNodeToLog(
            node,
            CurrentPlayerName
        );
    }

    private IEnumerator ProcessNarration(
        StoryNodeData node)
    {
        storyUI.HideSpeaker();

        if (node.KeepPortrait)
        {
            storyUI.SetCharacterDimmed(
                node.DimPortrait
            );
        }
        else
        {
            storyUI.HideCharacterImage();
        }

        yield return ShowNodeText(node);

        AddNodeToLog(
            node,
            string.Empty
        );
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

    /*
     * 외부 시스템에서 임시 스토리 조건을 등록할 때 사용합니다.
     *
     * 예:
     * SetStoryCondition("HasSpecialItem", "True");
     */
    public void SetStoryCondition(
        string key,
        string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            Debug.LogWarning(
                "스토리 조건 키가 비어 있습니다.",
                this
            );

            return;
        }

        storyConditions[key] =
            value ?? string.Empty;

        Debug.Log(
            $"스토리 조건 등록: {key} = {value}",
            this
        );
    }

    public void SetStoryUnlock(
        string key,
        bool unlocked)
    {
        SetStoryCondition(
            key,
            unlocked.ToString()
        );
    }

    private bool IsChoiceAvailable(
        StoryChoiceData choice)
    {
        if (choice == null)
        {
            return false;
        }

        if (!choice.UseCondition)
        {
            return true;
        }

        if (StoryChoiceRules.TryEvaluate(choice.RequiredKey, choice.RequiredValue, out bool available))
            return available;

        if (string.IsNullOrWhiteSpace(
                choice.RequiredKey))
        {
            Debug.LogWarning(
                $"선택지 '{choice.ChoiceText}'의 " +
                "Required Key가 비어 있습니다.",
                this
            );

            return false;
        }

        /*
         * 1. 현재 스토리에서 발생한 선택 결과를 확인합니다.
         */
        if (storyResults.TryGetValue(
                choice.RequiredKey,
                out string storyResultValue))
        {
            return string.Equals(
                storyResultValue,
                choice.RequiredValue,
                System.StringComparison
                    .OrdinalIgnoreCase
            );
        }

        /*
         * 2. 외부에서 등록한 임시 조건을 확인합니다.
         */
        if (storyConditions.TryGetValue(
                choice.RequiredKey,
                out string conditionValue))
        {
            return string.Equals(
                conditionValue,
                choice.RequiredValue,
                System.StringComparison
                    .OrdinalIgnoreCase
            );
        }

        /*
         * 3. 이전 스토리에서 영구 저장된
         * 선택 결과를 확인합니다.
         */
        if (progressService != null)
        {
            string savedValue =
                progressService.GetChoiceValue(
                    choice.RequiredKey
                );

            return string.Equals(
                savedValue,
                choice.RequiredValue,
                System.StringComparison
                    .OrdinalIgnoreCase
            );
        }

        return false;
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
            IsChoiceAvailable,
            OnChoiceSelected
        );
    }

    private void OnChoiceSelected(
        StoryChoiceData choice)
    {
        if (!isWaitingForChoice ||
            choice == null || !IsChoiceAvailable(choice))
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
        if (choice == null ||
            string.IsNullOrWhiteSpace(
                choice.ResultKey))
        {
            return;
        }

        /*
         * 현재 스토리 안에서 즉시 사용하기 위한 값입니다.
         */
        storyResults[choice.ResultKey] =
            choice.ResultValue;

        StoryChoiceRules.RecordResult(choice.ResultKey, choice.ResultValue);

        /*
         * 다음 스토리나 게임 재실행 후에도 사용하도록
         * SaveData에 저장합니다.
         */
        progressService?.SaveChoiceResult(
            choice.ResultKey,
            choice.ResultValue
        );
    }

    public string GetStoryResult(
        string key,
        string defaultValue = "")
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return defaultValue;
        }

        if (storyResults.TryGetValue(
                key,
                out string currentValue))
        {
            return currentValue;
        }

        if (progressService != null)
        {
            string savedValue =
                progressService.GetChoiceValue(key);

            if (!string.IsNullOrEmpty(savedValue))
            {
                return savedValue;
            }
        }

        return defaultValue;
    }

    private string ReplaceTokens(
        string text)
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
        if (node == null)
        {
            return;
        }

        /*
         * 자동 진행 버튼 또는 스킵이 켜졌을 때만
         * 다음 노드를 자동으로 예약합니다.
         */
        bool shouldAdvance =
            skipMode ||
            autoMode;

        if (!shouldAdvance)
        {
            return;
        }

        float delay;

        if (skipMode)
        {
            delay = skipAdvanceDelay;
        }
        else
        {
            /*
             * 노드에 별도 자동 진행 시간이 지정되어 있다면
             * 시간만 사용하고, 자동 진행 여부 자체는
             * autoMode가 결정합니다.
             */
            delay =
                node.AutoAdvance
                    ? node.AutoAdvanceDelay
                    : autoAdvanceDelay;
        }

        CancelScheduledAdvance();

        advanceCoroutine = StartCoroutine(
            AutomaticAdvanceRoutine(delay)
        );
    }

    private IEnumerator AutomaticAdvanceRoutine(
    float delay)
    {
        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, delay)
        );

        advanceCoroutine = null;

        if (!isStoryPlaying ||
            isWaitingForChoice)
        {
            yield break;
        }

        // 대기 중 자동 진행을 껐다면 진행 중단
        if (!autoMode &&
            !skipMode)
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

        Debug.Log(
    $"ToggleAutoMode 호출 / 현재: {autoMode} / 변경: {!autoMode}", this);

        bool nextAutoMode = !autoMode;

        // StoryRunner가 실제로 사용하는 상태를 즉시 변경
        autoMode = nextAutoMode;

        if (autoMode)
        {
            // 자동 모드와 스킵 모드는 동시에 사용하지 않음
            skipMode = false;
        }

        // 기존에 예약된 자동 진행을 즉시 중단
        CancelScheduledAdvance();

        // 버튼 UI 즉시 변경
        if (storyUI != null)
        {
            storyUI.SetAutoModeVisual(autoMode);
        }

        // 설정값도 현재 상태와 동일하게 저장
        if (StorySettingsManager.Instance != null)
        {
            StorySettingsManager.Instance.SetAdvanceMode(
                autoMode
                    ? StoryAdvanceMode.Auto
                    : StoryAdvanceMode.Manual
            );
        }

        // 자동 모드로 켠 경우에만 다시 자동 진행 예약
        if (autoMode &&
            currentNode != null &&
            !isWaitingForChoice &&
            storyUI != null &&
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
            if (backgroundController != null)
                backgroundController.CompleteTransition();
            autoMode = false;

            storyUI.SetAutoModeVisual(false);

            if (storyUI.IsTyping)
            {
                storyUI.CompleteTypingImmediately();
            }
        }

        CancelScheduledAdvance();

        if (skipMode &&
            currentNode != null &&
            !isWaitingForChoice &&
            !isProcessingNode)
        {
            ScheduleAutomaticAdvance(
                currentNode
            );
        }
    }

    private IEnumerator ProcessEffects(
        StoryNodeData node)
    {
        if (node == null ||
            node.Effects == null)
        {
            yield break;
        }

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
                $"{effect.EffectType}",
                this
            );

            /*
             * 추후 여기서 전용 StoryEffectController에
             * 효과 실행을 위임할 수 있습니다.
             */
            if (effect.WaitForCompletion)
            {
                yield return new WaitForSecondsRealtime(
                    Mathf.Max(
                        0f,
                        effect.Duration
                    )
                );
            }
        }
    }

    /*
     * 현재 StoryData를 종료하고,
     * 시퀀스에 다음 StoryData가 있으면 이어서 실행합니다.
     */
    public void EndStory()
    {
        StoryData finishedStory =
            currentStory;

        StopAllStoryCoroutines();

        isStoryPlaying = false;
        isWaitingForChoice = false;
        isProcessingNode = false;

        autoMode = false;
        skipMode = false;

        currentNode = null;
        currentStory = null;

        if (storyUI != null)
        {
            storyUI.SetAutoModeVisual(false);
            storyUI.Close();
        }

        // 방금 종료된 개별 StoryData 완료 저장
        if (finishedStory != null)
        {
            progressService?.CompleteStory(
                finishedStory
            );
        }

        /*
         * 아직 재생할 이벤트/월 스토리가 남아 있다면
         * 월을 증가시키지 않고 다음 스토리를 실행합니다.
         */
        if (sequenceController != null &&
            sequenceController.HasNextStory)
        {
            PlayNextQueuedStory();
            return;
        }

        /*
         * Queue의 모든 스토리가 끝났으므로
         * 이번 달 전체 스토리가 종료된 상태입니다.
         */
        if (sequenceController != null)
        {
            sequenceController.StopSequence();
        }

        Debug.Log(
            "이번 달의 모든 스토리가 종료되었습니다.",
            this
        );

        if (monthCompletionController == null)
        {
            FindMonthCompletionController();
        }

        if (monthCompletionController == null)
        {
            Debug.LogError(
                "월 완료 처리를 실행할 수 없습니다.",
                this
            );

            return;
        }

        monthCompletionController
            .CompleteMonthAndReturnToLobby();
    }

    private void AddNodeToLog(
        StoryNodeData node,
        string speakerName)
    {
        if (node == null ||
            storyUI == null)
        {
            return;
        }

        string outputText =
            ReplaceTokens(node.Text);

        if (string.IsNullOrWhiteSpace(
                outputText))
        {
            return;
        }

        storyUI.AddLogEntry(
            speakerName,
            outputText
        );
    }

    private void ApplyAdvanceModeSetting()
    {
        if (StorySettingsManager.Instance == null)
        {
            return;
        }

        autoMode =
            StorySettingsManager.Instance
                .IsAutoAdvance();

        if (autoMode)
        {
            skipMode = false;
        }

        if (storyUI != null)
        {
            storyUI.SetAutoModeVisual(
                autoMode
            );
        }

        /*
         * 이전 자동 진행 예약을 먼저 제거합니다.
         */
        CancelScheduledAdvance();

        /*
         * 자동 진행이 켜졌고 현재 대사가 모두 출력된
         * 상태라면 다시 자동 진행을 예약합니다.
         */
        if (autoMode &&
            isStoryPlaying &&
            currentNode != null &&
            !isWaitingForChoice &&
            storyUI != null &&
            !storyUI.IsTyping &&
            !isProcessingNode)
        {
            ScheduleAutomaticAdvance(
                currentNode
            );
        }
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
        if (backgroundController != null)
            backgroundController.CompleteTransition();
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

        Button[] autoButtons =
    storyUI.GetAutoModeButtons();

        foreach (Button button in autoButtons)
        {
            if (button == null)
            {
                continue;
            }

            button.onClick.RemoveListener(
                ToggleAutoMode
            );
        }

        Button skipButton =
            storyUI.GetSkipButton();

        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(
                ToggleSkipMode
            );
        }

    }

    private void FindMonthCompletionController()
    {
        if (monthCompletionController != null)
        {
            return;
        }

        monthCompletionController =
            GetComponent<StoryMonthCompletionController>();

        if (monthCompletionController == null)
        {
            Debug.LogError(
                "StoryMonthCompletionController가 " +
                "연결되지 않았습니다. " +
                "StoryRunner와 같은 오브젝트에 추가하거나 " +
                "Inspector에서 직접 연결해주세요.",
                this
            );
        }
    }
}
