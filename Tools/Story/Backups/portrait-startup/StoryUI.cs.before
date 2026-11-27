using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryUI : MonoBehaviour
{
    [Header("스토리 전체")]
    [SerializeField] private GameObject dialogueRoot;

    [Header("스토리 제목")]
    [SerializeField] private GameObject titleRoot;
    [SerializeField] private Text yearText;
    [SerializeField] private Text dateText;
    [SerializeField] private Text titleNameText;
    [SerializeField] private Button titleConfirmButton;
    [SerializeField] private CutScVFXController titleTypingEffect;

    [Header("캐릭터")]
    [SerializeField] private Image characterImage;


    [Header("대화창")]
    [SerializeField] private GameObject chatBox;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text characterInfoText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("추가 캐릭터 정보")]
    [SerializeField] private Image factionImage;

    [Header("다음 대사 표시")]
    [SerializeField] private GameObject nextLineIcon;

    [Header("선택지")]
    [SerializeField] private GameObject choiceBox;
    [SerializeField] private Button[] choiceButtons;
    [SerializeField] private TMP_Text[] choiceTexts;

    [Header("기능 버튼")]
    [SerializeField] private Button autoButton;
    [SerializeField] private GameObject autoInactiveObject;
    [SerializeField] private GameObject autoActiveObject;

    [SerializeField] private Button skipButton;
    [SerializeField] private Button logButton;

    [Header("스토리 로그")]
    [SerializeField] private Transform logContent;
    [SerializeField] private StoryLogItemUI logTemplate;

    [Header("타이핑 설정")]
    [Min(0.001f)]
    [SerializeField] private float typingInterval = 0.03f;

    private Coroutine typingCoroutine;

    private string currentFullText = string.Empty;
    private bool isTyping;
    private bool skipTypingRequested;

    public bool IsTyping => isTyping;

    private Action titleConfirmAction;

    private void Awake()
    {
        ValidateChoiceObjects();

        if (logTemplate != null)
        {
            logTemplate.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        ApplyTextSizeSetting();

        if (StorySettingsManager.Instance != null)
        {
            StorySettingsManager.Instance
                .OnStorySettingsChanged +=
                ApplyTextSizeSetting;
        }
    }

    private void OnDisable()
    {
        if (StorySettingsManager.Instance != null)
        {
            StorySettingsManager.Instance
                .OnStorySettingsChanged -=
                ApplyTextSizeSetting;
        }
    }

    public void Open()
    {
        StopTyping();

        // 스토리 시작 직후에는 제목과 대화창을 모두 정리
        if (dialogueRoot != null)
        {
            dialogueRoot.SetActive(false);
        }

        if (titleRoot != null)
        {
            titleRoot.SetActive(false);
        }

        HideChoices();
        HideNextLineIcon();

        if (dialogueText != null)
        {
            dialogueText.text = string.Empty;
        }
    }

    public void Close()
    {
        StopTyping();

        HideChoices();
        HideNextLineIcon();

        if (dialogueRoot != null)
        {
            dialogueRoot.SetActive(false);
        }

        if (titleRoot != null)
        {
            titleRoot.SetActive(false);
        }
    }

    #region 제목

    public void ShowStoryTitle(
    StoryData story,
    Action onConfirmed)
    {
        if (story == null)
        {
            Debug.LogError(
                "표시할 StoryData가 없습니다.",
                this
            );

            onConfirmed?.Invoke();
            return;
        }

        titleConfirmAction = onConfirmed;

        if (dialogueRoot != null)
        {
            dialogueRoot.SetActive(false);
        }

        if (titleRoot == null)
        {
            Debug.LogWarning(
                "Title Root가 연결되지 않아 바로 시작합니다.",
                this
            );

            ConfirmStoryTitle();
            return;
        }

        titleRoot.SetActive(true);

        string year =
            GetYearText(story);

        string date =
            GetDateText(story);

        string title =
            story.StoryTitle ?? string.Empty;

        if (titleTypingEffect != null)
        {
            titleTypingEffect.PlaySequence(
                year,
                date,
                title
            );
        }
        else
        {
            if (yearText != null)
            {
                yearText.text = year;
            }

            if (dateText != null)
            {
                dateText.text = date;
            }

            if (titleNameText != null)
            {
                titleNameText.text = title;
            }
        }

        if (titleConfirmButton == null)
        {
            Debug.LogWarning(
                "Title Confirm Button이 연결되지 않았습니다.",
                this
            );

            ConfirmStoryTitle();
            return;
        }

        titleConfirmButton.onClick.RemoveListener(
            ConfirmStoryTitle
        );

        titleConfirmButton.onClick.AddListener(
            ConfirmStoryTitle
        );
    }

    private void ConfirmStoryTitle()
    {
        if (titleConfirmButton != null)
        {
            titleConfirmButton.onClick.RemoveListener(
                ConfirmStoryTitle
            );
        }

        if (titleTypingEffect != null)
        {
            titleTypingEffect.StopSequence();
        }

        HideTitle();
        ShowDialogue();

        Action callback =
            titleConfirmAction;

        titleConfirmAction = null;

        callback?.Invoke();
    }

    public void ShowDialogue()
    {
        if (titleRoot != null)
        {
            titleRoot.SetActive(false);
        }

        if (dialogueRoot != null)
        {
            dialogueRoot.SetActive(true);
        }
    }

    public void HideTitle()
    {
        if (titleRoot != null)
        {
            titleRoot.SetActive(false);
        }
    }

    private string GetYearText(StoryData story)
    {
        if (story.Year <= 0)
        {
            return string.Empty;
        }

        return $"{story.Year}년";
    }

    private string GetDateText(StoryData story)
    {
        if (story.Day <= 0)
        {
            return $"{story.Month}월";
        }

        return $"{story.Month}월 {story.Day}일";
    }

    #endregion

    #region 대화 정보

    public void SetCharacterDialogue(
        CharacterData character,
        string portraitId)
    {
        if (character == null)
        {
            HideSpeaker();
            return;
        }

        SetSpeaker(
            character.CharacterName,
            character.Job
        );

        SetCharacterImage(
            character.GetStoryPortrait(portraitId)
        );

        SetCharacterDimmed(false);
    }

    public void SetSpeaker(
        string characterName,
        string characterInfo)
    {
        if (chatBox != null)
        {
            chatBox.SetActive(true);
        }

        if (characterNameText != null)
        {
            characterNameText.gameObject.SetActive(true);
            characterNameText.text =
                characterName ?? string.Empty;
        }

        if (characterInfoText != null)
        {
            bool hasInfo =
                !string.IsNullOrWhiteSpace(characterInfo);

            characterInfoText.gameObject.SetActive(hasInfo);
            characterInfoText.text =
                characterInfo ?? string.Empty;
        }
    }

    public void HideSpeaker()
    {
        if (characterNameText != null)
        {
            characterNameText.text = string.Empty;
            characterNameText.gameObject.SetActive(false);
        }

        if (characterInfoText != null)
        {
            characterInfoText.text = string.Empty;
            characterInfoText.gameObject.SetActive(false);
        }
    }

    public void SetCharacterImage(Sprite sprite)
    {
        if (characterImage == null)
        {
            return;
        }

        characterImage.sprite = sprite;
        characterImage.gameObject.SetActive(sprite != null);
    }

    public void HideCharacterImage()
    {
        if (characterImage == null)
        {
            return;
        }

        characterImage.sprite = null;
        characterImage.gameObject.SetActive(false);

        SetCharacterDimmed(false);
    }

    public void SetCharacterDimmed(bool dimmed)
    {
        if (characterImage == null)
        {
            return;
        }

        characterImage.color = dimmed
            ? new Color(0.45f, 0.45f, 0.45f, 1f)
            : Color.white;
    }

    public void SetFactionImage(Sprite sprite)
    {
        if (factionImage == null)
        {
            return;
        }

        factionImage.sprite = sprite;
        factionImage.gameObject.SetActive(sprite != null);
    }

    #endregion

    #region 타이핑

    public IEnumerator ShowText(
        string text,
        bool useTypingEffect)
    {
        StopTyping();

        currentFullText = text ?? string.Empty;
        skipTypingRequested = false;

        HideNextLineIcon();

        if (!useTypingEffect)
        {
            dialogueText.text = currentFullText;
            yield break;
        }

        isTyping = true;
        dialogueText.text = string.Empty;

        dialogueText.maxVisibleCharacters = 0;
        dialogueText.text = currentFullText;

        dialogueText.ForceMeshUpdate();

        int totalCharacters =
            dialogueText.textInfo.characterCount;

        for (int i = 0; i <= totalCharacters; i++)
        {
            if (skipTypingRequested)
            {
                break;
            }

            dialogueText.maxVisibleCharacters = i;

            yield return new WaitForSecondsRealtime(
                typingInterval
            );
        }

        dialogueText.maxVisibleCharacters =
            totalCharacters;

        isTyping = false;
        skipTypingRequested = false;
        typingCoroutine = null;
    }

    public void StartTyping(
        string text,
        bool useTypingEffect,
        Action onComplete = null)
    {
        StopTyping();

        typingCoroutine = StartCoroutine(
            TypingRoutine(
                text,
                useTypingEffect,
                onComplete
            )
        );
    }

    private IEnumerator TypingRoutine(
        string text,
        bool useTypingEffect,
        Action onComplete)
    {
        yield return ShowText(
            text,
            useTypingEffect
        );

        onComplete?.Invoke();
    }

    public void CompleteTypingImmediately()
    {
        if (!isTyping)
        {
            return;
        }

        skipTypingRequested = true;

        dialogueText.text = currentFullText;
        dialogueText.ForceMeshUpdate();

        dialogueText.maxVisibleCharacters =
            dialogueText.textInfo.characterCount;
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        skipTypingRequested = false;
        isTyping = false;

        if (dialogueText != null)
        {
            dialogueText.maxVisibleCharacters =
                int.MaxValue;
        }
    }

    #endregion

    #region 다음 줄 아이콘

    public void ShowNextLineIcon()
    {
        if (nextLineIcon != null)
        {
            nextLineIcon.SetActive(true);
        }
    }

    public void HideNextLineIcon()
    {
        if (nextLineIcon != null)
        {
            nextLineIcon.SetActive(false);
        }
    }

    #endregion

    #region 선택지

    public void ShowChoices(
    IReadOnlyList<StoryChoiceData> choices,
    Func<StoryChoiceData, bool> isChoiceAvailable,
    Action<StoryChoiceData> onSelected)
    {
        HideChoices();

        if (choices == null || choices.Count == 0)
        {
            Debug.LogWarning("표시할 선택지가 없습니다.");
            return;
        }

        if (choiceBox == null)
        {
            Debug.LogError(
                "Choice Box가 연결되지 않았습니다.",
                this
            );

            return;
        }

        choiceBox.SetActive(true);

        int uiIndex = 0;

        for (int choiceIndex = 0;
             choiceIndex < choices.Count;
             choiceIndex++)
        {
            StoryChoiceData currentChoice =
                choices[choiceIndex];

            if (currentChoice == null)
            {
                continue;
            }

            bool isAvailable =
                isChoiceAvailable == null ||
                isChoiceAvailable(currentChoice);

            // 잠긴 선택지를 숨기는 설정
            if (!isAvailable &&
                currentChoice.HideWhenLocked)
            {
                continue;
            }

            if (uiIndex >= choiceButtons.Length ||
                uiIndex >= choiceTexts.Length)
            {
                Debug.LogWarning(
                    "표시할 선택지가 현재 선택지 UI 수보다 많습니다."
                );

                break;
            }

            Button button =
                choiceButtons[uiIndex];

            TMP_Text choiceText =
                choiceTexts[uiIndex];

            button.gameObject.SetActive(true);
            button.interactable = isAvailable;

            if (!isAvailable &&
                !string.IsNullOrWhiteSpace(
                    currentChoice.LockedText))
            {
                choiceText.text =
                    currentChoice.LockedText;
            }
            else
            {
                choiceText.text =
                    currentChoice.ChoiceText;
            }

            button.onClick.RemoveAllListeners();

            if (isAvailable)
            {
                StoryChoiceData capturedChoice =
                    currentChoice;

                button.onClick.AddListener(() =>
                {
                    DisableAllChoiceButtons();

                    onSelected?.Invoke(
                        capturedChoice
                    );
                });
            }

            uiIndex++;
        }

        if (uiIndex == 0)
        {
            Debug.LogWarning(
                "조건을 만족하는 선택지가 하나도 없습니다.",
                this
            );

            choiceBox.SetActive(false);
        }
    }

    public void HideChoices()
    {
        if (choiceBox != null)
        {
            choiceBox.SetActive(false);
        }

        if (choiceButtons == null)
        {
            return;
        }

        foreach (Button button in choiceButtons)
        {
            if (button == null)
            {
                continue;
            }

            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false);
        }
    }

    private void DisableAllChoiceButtons()
    {
        foreach (Button button in choiceButtons)
        {
            if (button != null)
            {
                button.interactable = false;
            }
        }
    }

    private void ValidateChoiceObjects()
    {
        if (choiceButtons == null ||
            choiceTexts == null)
        {
            return;
        }

        if (choiceButtons.Length != choiceTexts.Length)
        {
            Debug.LogError(
                "Choice Buttons와 Choice Texts의 수가 다릅니다.",
                this
            );
        }
    }

    #endregion

    #region 자동 진행 버튼

    public void SetAutoModeVisual(bool isAuto)
    {
        if (autoInactiveObject != null)
        {
            autoInactiveObject.SetActive(!isAuto);
        }

        if (autoActiveObject != null)
        {
            autoActiveObject.SetActive(isAuto);
        }
    }

    public Button GetAutoButton()
    {
        return autoButton;
    }

    public Button[] GetAutoModeButtons()
    {
        List<Button> buttons = new List<Button>();

        AddButtonIfValid(
            buttons,
            autoButton
        );

        if (autoInactiveObject != null)
        {
            Button inactiveButton =
                autoInactiveObject.GetComponent<Button>();

            if (inactiveButton == null)
            {
                inactiveButton =
                    autoInactiveObject
                        .GetComponentInChildren<Button>(true);
            }

            AddButtonIfValid(
                buttons,
                inactiveButton
            );
        }

        if (autoActiveObject != null)
        {
            Button activeButton =
                autoActiveObject.GetComponent<Button>();

            if (activeButton == null)
            {
                activeButton =
                    autoActiveObject
                        .GetComponentInChildren<Button>(true);
            }

            AddButtonIfValid(
                buttons,
                activeButton
            );
        }

        return buttons.ToArray();
    }

    private void AddButtonIfValid(
        List<Button> buttons,
        Button button)
    {
        if (button == null)
        {
            return;
        }

        if (buttons.Contains(button))
        {
            return;
        }

        buttons.Add(button);
    }

    public Button GetSkipButton()
    {
        return skipButton;
    }

    public Button GetLogButton()
    {
        return logButton;
    }

    #endregion

    private void OnDestroy()
    {
        if (titleConfirmButton != null)
        {
            titleConfirmButton.onClick.RemoveListener(
                ConfirmStoryTitle
            );
        }

        titleConfirmAction = null;
    }

    private void OnClickStoryTitle()
    {
        if (titleTypingEffect != null &&
            titleTypingEffect.IsPlaying)
        {
            titleTypingEffect.CompleteImmediately();
            return;
        }

        ConfirmStoryTitle();
    }

    #region 스토리 로그

    public void AddLogEntry(
    string speakerName,
    string dialogue)
    {
        if (logContent == null)
        {
            Debug.LogWarning(
                "Log Content가 연결되지 않았습니다.",
                this
            );

            return;
        }

        if (logTemplate == null)
        {
            Debug.LogWarning(
                "Log Template이 연결되지 않았습니다.",
                this
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(dialogue))
        {
            return;
        }

        StoryLogItemUI newLog =
            Instantiate(
                logTemplate,
                logContent
            );

        newLog.gameObject.SetActive(true);

        float fontSize =
            GetConfiguredTextSize();

        newLog.Setup(
            speakerName,
            dialogue,
            fontSize
        );
    }

    public void ClearLogEntries()
    {
        if (logContent == null)
        {
            return;
        }

        for (int i = logContent.childCount - 1;
             i >= 0;
             i--)
        {
            Transform child =
                logContent.GetChild(i);

            if (logTemplate != null &&
                child == logTemplate.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    #endregion

    private float GetConfiguredTextSize()
    {
        if (StorySettingsManager.Instance == null)
        {
            return 30f;
        }

        StoryTextSize size =
            StorySettingsManager.Instance.GetTextSize();

        switch (size)
        {
            case StoryTextSize.Small:
                return 24f;

            case StoryTextSize.Large:
                return 36f;

            case StoryTextSize.Medium:
            default:
                return 30f;
        }
    }

    public void ApplyTextSizeSetting()
    {
        float fontSize =
            GetConfiguredTextSize();

        if (dialogueText != null)
        {
            dialogueText.fontSize =
                fontSize;
        }

        if (logContent == null)
        {
            return;
        }

        foreach (
            StoryLogItemUI logItem
            in logContent.GetComponentsInChildren<
                StoryLogItemUI>(true))
        {
            if (logTemplate != null &&
                logItem == logTemplate)
            {
                continue;
            }

            logItem.ApplyFontSize(
                fontSize
            );
        }
    }
}