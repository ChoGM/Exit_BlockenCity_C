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

    [Header("캐릭터")]
    [SerializeField] private Image characterImage;

    [Tooltip("캐릭터 이미지를 어둡게 만들 때 활성화할 오브젝트")]
    [SerializeField] private GameObject characterDarkMask;

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
    }

    public void Open()
    {
        dialogueRoot.SetActive(true);

        HideChoices();
        HideNextLineIcon();

        dialogueText.text = string.Empty;
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

    public void ShowStoryTitle(StoryData story, Action onConfirmed)
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

        if (titleRoot == null)
        {
            Debug.LogWarning(
                "Title Root가 연결되지 않아 " +
                "바로 스토리를 시작합니다.",
                this
            );

            ConfirmStoryTitle();
            return;
        }

        titleRoot.SetActive(true);

        if (yearText != null)
        {
            yearText.text = GetYearText(story);
        }

        if (dateText != null)
        {
            dateText.text = GetDateText(story);
        }

        if (titleNameText != null)
        {
            titleNameText.text = story.StoryTitle;
        }

        if (titleConfirmButton == null)
        {
            Debug.LogWarning(
                "제목 확인 버튼이 연결되지 않아 " +
                "바로 스토리를 시작합니다.",
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

        HideTitle();

        Action callback = titleConfirmAction;
        titleConfirmAction = null;

        callback?.Invoke();
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
        if (characterDarkMask != null)
        {
            characterDarkMask.SetActive(dimmed);
            return;
        }

        // Mask 오브젝트가 연결되지 않았을 경우를 위한 예비 처리
        if (characterImage != null)
        {
            float brightness = dimmed ? 0.4f : 1f;

            characterImage.color = new Color(
                brightness,
                brightness,
                brightness,
                1f
            );
        }
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
        Action<StoryChoiceData> onSelected)
    {
        HideChoices();

        if (choices == null || choices.Count == 0)
        {
            Debug.LogWarning("표시할 선택지가 없습니다.");
            return;
        }

        choiceBox.SetActive(true);

        int visibleCount = Mathf.Min(
            choices.Count,
            choiceButtons.Length
        );

        if (choices.Count > choiceButtons.Length)
        {
            Debug.LogWarning(
                $"선택지는 {choices.Count}개이지만 " +
                $"현재 UI에는 {choiceButtons.Length}개까지만 " +
                "표시할 수 있습니다."
            );
        }

        for (int i = 0; i < visibleCount; i++)
        {
            int index = i;
            StoryChoiceData currentChoice = choices[index];

            choiceButtons[index].gameObject.SetActive(true);
            choiceButtons[index].interactable = true;

            choiceTexts[index].text =
                currentChoice.ChoiceText;

            choiceButtons[index].onClick.RemoveAllListeners();

            choiceButtons[index].onClick.AddListener(() =>
            {
                DisableAllChoiceButtons();
                onSelected?.Invoke(currentChoice);
            });
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
}