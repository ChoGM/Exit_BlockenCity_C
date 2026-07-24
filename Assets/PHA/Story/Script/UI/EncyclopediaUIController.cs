using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EncyclopediaUIController : MonoBehaviour
{
    [Header("카테고리 버튼")]
    [SerializeField] private Button[] categoryButtons;

    private int previousCategory = -1;
    private int currentCategory = -1;

    [SerializeField] private GameObject factionPopUps;
    [SerializeField] private GameObject newspaperPopUps;
    [SerializeField] private GameObject detailReport; // 상세 보고서 오브젝트

    [Header("전체 데이터")]
    public FactionData[] factions;

    [Header("좌측 세력 이미지")]
    public Image pageLeftImage;
    public Sprite[] factionPageLeftSprites;

    [Header("세력 UI")]
    public Button[] factionButtons;
    private int previousFactionIndex = -1;
    private int currentFactionIndex = -1;

    public TMP_Text factionNameKRText;
    public TMP_Text factionNameENText;

    public TMP_Text factionDescriptionText;
    public TMP_Text factionSubDescriptionText;

    public TMP_Text factionFoundingDateText;

    public Image factionIconImage;

    [Header("캐릭터 상세 페이지")]
    [SerializeField] private GameObject characterDetailPage;

    [Header("캐릭터 선택 버튼")]
    public Button[] characterButtons;

    public TMP_Text[] characterButtonNameTexts;

    [Header("캐릭터 기본 UI")]
    public Image characterPortraitImage;
    public TMP_Text characterNameText;
    public TMP_Text characterEnglishNameText;
    public TMP_Text dialogueText;
    public TMP_Text ageText;
    public TMP_Text jobText;
    public TMP_Text heightText;
    public TMP_Text weightText;
    public TMP_Text notesText;

    [Header("스토리 UI")]
    public TMP_Text[] storyTitleTexts;
    public TMP_Text[] storyContentTexts;

    [Header("관계 UI")]
    public TMP_Text[] relationNameTexts;
    public TMP_Text[] relationCommentTexts;

    private int currentCharacterIndex = 0;
    private bool isInitialized = false;

    private void Awake()
    {
        InitCategoryButtons();
        InitFactionButtons();
        InitCharacterButtons();
        isInitialized = true;
    }

    private void OnEnable()
    {
        if (!isInitialized)
            return;

        ResetToDefault();
    }

    private void Start()
    {
        ResetToDefault();
    }

    /// <summary>
    /// 도감을 처음 열거나 껐다 켤 때 기본 상태(카테고리 0번, 세력 0번)로 리셋합니다.
    /// </summary>
    public void ResetToDefault()
    {
        if (characterDetailPage != null)
            characterDetailPage.SetActive(false);

        previousCategory = -1;
        currentCategory = -1;

        previousFactionIndex = -1;
        currentFactionIndex = -1;

        SelectCategory(0);
    }

    private void InitCategoryButtons()
    {
        for (int i = 0; i < categoryButtons.Length; i++)
        {
            int index = i;

            if (categoryButtons[i] == null)
                continue;

            categoryButtons[i].onClick.AddListener(() => SelectCategory(index));
        }
    }

    private void InitFactionButtons()
    {
        for (int i = 0; i < factionButtons.Length; i++)
        {
            int index = i;

            if (factionButtons[i] == null)
                continue;

            // 마천교(3), 전상연(4)은 선택 불가
            if (index >= 3)
            {
                factionButtons[i].interactable = false;
                continue;
            }

            factionButtons[i].onClick.AddListener(() => SelectFaction(index));
        }
    }

    private void InitCharacterButtons()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;

            if (characterButtons[i] != null)
            {
                characterButtons[i].onClick.AddListener(() => SelectCharacter(index));
            }
        }
    }

    public void SelectCategory(int categoryIndex)
    {
        if (categoryIndex < 0 || categoryIndex >= categoryButtons.Length)
            return;

        // 이미 선택된 카테고리를 눌렀으면 무시 (단, 초기 진입시 -1일 때는 실행)
        if (categoryIndex == currentCategory && previousCategory != -1)
            return;

        previousCategory = currentCategory;
        currentCategory = categoryIndex;

        // 1. 이전 카테고리 버튼 복구 (Inactive)
        if (previousCategory != -1 && previousCategory != currentCategory)
        {
            SetCategoryButtonState(previousCategory, false);
        }

        // 2. 현재 카테고리 버튼 선택 (Active)
        SetCategoryButtonState(currentCategory, true);

        // 팝업 및 상세보고서 전환 (0: 세력 카테고리, 1: 신문 카테고리)
        if (factionPopUps != null)
            factionPopUps.SetActive(currentCategory == 0);

        if (newspaperPopUps != null)
            newspaperPopUps.SetActive(currentCategory == 1);

        if (detailReport != null)
            detailReport.SetActive(currentCategory == 0);

        // 세력 카테고리(0번)로 들어올 때, 세력 버튼들의 Visual 상태를 단월국(0번) 기준으로 완벽 동기화
        if (currentCategory == 0)
        {
            ResetFactionButtonsToDefault();
        }
    }

    /// <summary>
    /// 세력 카테고리로 전환될 때 단월국(0번) 세력을 기본 선택하고
    /// 모든 세력 버튼 visual 상태를 0번 Active / 나머지 Inactive로 확실하게 강제 정돈합니다.
    /// </summary>
    private void ResetFactionButtonsToDefault()
    {
        currentFactionIndex = 0;
        previousFactionIndex = -1;
        currentCharacterIndex = 0;

        if (characterDetailPage != null)
            characterDetailPage.SetActive(false);

        for (int i = 0; i < factionButtons.Length; i++)
        {
            if (factionButtons[i] == null)
                continue;

            // 0번(단월국)만 Active, 나머지는 Inactive로 확실하게 설정
            SetFactionButtonState(i, i == 0);
        }

        RefreshFactionUI();
        RefreshCharacterButtonUI();
    }

    private void SetCategoryButtonState(int index, bool isActive)
    {
        if (index < 0 || index >= categoryButtons.Length || categoryButtons[index] == null)
            return;

        GameObject root = categoryButtons[index].transform.parent.gameObject;
        Animator animator = root.GetComponent<Animator>();

        Transform active = root.transform.Find("BTN_Active");
        Transform inactive = root.transform.Find("BTN_Inactive");

        if (animator != null)
            animator.Play(isActive ? "Active" : "Inactive");

        if (active != null)
            active.gameObject.SetActive(!isActive);

        if (inactive != null)
            inactive.gameObject.SetActive(isActive);
    }

    public void SelectFaction(int factionIndex)
    {
        if (factionIndex < 0 || factionIndex >= factions.Length)
            return;

        if (factions[factionIndex] == null)
            return;

        // 이미 선택되어 있는 세력 버튼을 또 클릭한 경우
        if (factionIndex == currentFactionIndex && previousFactionIndex != -1)
        {
            if (characterDetailPage != null)
                characterDetailPage.SetActive(false);
            return;
        }

        previousFactionIndex = currentFactionIndex;
        currentFactionIndex = factionIndex;
        currentCharacterIndex = 0;

        if (characterDetailPage != null)
            characterDetailPage.SetActive(false);

        // 핀포인트 전환:
        // 1. 이전 세력 버튼만 원래 상태로 복구 (Inactive)
        if (previousFactionIndex != -1 && previousFactionIndex != currentFactionIndex)
        {
            SetFactionButtonState(previousFactionIndex, false);
        }

        // 2. 현재 선택한 세력 버튼만 활성화 (Active)
        SetFactionButtonState(currentFactionIndex, true);

        RefreshFactionUI();
        RefreshCharacterButtonUI();
    }

    private void SetFactionButtonState(int index, bool isActive)
    {
        if (index < 0 || index >= factionButtons.Length || factionButtons[index] == null)
            return;

        GameObject root = factionButtons[index].transform.parent.gameObject;
        Animator animator = root.GetComponent<Animator>();

        Transform active = root.transform.Find("BTN_Active");
        Transform inactive = root.transform.Find("BTN_Inactive");

        if (animator != null)
            animator.Play(isActive ? "Active" : "Inactive");

        if (active != null)
            active.gameObject.SetActive(!isActive);

        if (inactive != null)
            inactive.gameObject.SetActive(isActive);
    }

    public void SelectCharacter(int characterIndex)
    {
        if (currentCharacterIndex == characterIndex && characterDetailPage.activeSelf)
        {
            characterDetailPage.SetActive(false);
            return;
        }

        FactionData faction = factions[currentFactionIndex];

        if (characterIndex < 0 || characterIndex >= faction.characters.Length)
            return;

        if (faction.characters[characterIndex] == null)
            return;

        currentCharacterIndex = characterIndex;
        characterDetailPage.SetActive(true);

        RefreshCharacterDetailUI();
    }

    private void RefreshFactionUI()
    {
        FactionData faction = factions[currentFactionIndex];

        if (factionNameKRText != null)
            factionNameKRText.text = faction.factionName;

        if (factionNameENText != null)
            factionNameENText.text = faction.factionName_eng;

        if (factionDescriptionText != null)
            factionDescriptionText.text = faction.description;

        if (factionSubDescriptionText != null)
            factionSubDescriptionText.text = faction.subDescription;

        if (factionFoundingDateText != null)
            factionFoundingDateText.text = faction.foundingDate;

        if (factionIconImage != null)
            factionIconImage.sprite = faction.factionIcon;

        if (pageLeftImage != null && currentFactionIndex < factionPageLeftSprites.Length)
        {
            pageLeftImage.sprite = factionPageLeftSprites[currentFactionIndex];
        }
    }

    private void RefreshCharacterButtonUI()
    {
        FactionData faction = factions[currentFactionIndex];

        for (int i = 0; i < characterButtons.Length; i++)
        {
            bool hasCharacter = i < faction.characters.Length && faction.characters[i] != null;

            if (characterButtons[i] != null)
                characterButtons[i].gameObject.SetActive(hasCharacter);

            if (!hasCharacter)
                continue;

            CharacterData character = faction.characters[i];

            CharacterUnlockData unlockData =
                EncyclopediaSaveManager.Instance.GetCharacterUnlockData(character.characterId);

            if (unlockData.isCharacterUnlocked)
            {
                if (characterButtonNameTexts[i] != null)
                    characterButtonNameTexts[i].text = character.characterName;
            }
            else
            {
                if (characterButtonNameTexts[i] != null)
                    characterButtonNameTexts[i].text = "???";
            }
        }
    }

    private void RefreshCharacterDetailUI()
    {
        CharacterData character = factions[currentFactionIndex].characters[currentCharacterIndex];

        CharacterUnlockData unlockData =
            EncyclopediaSaveManager.Instance.GetCharacterUnlockData(character.characterId);

        if (unlockData.isCharacterUnlocked)
        {
            if (characterPortraitImage != null)
            {
                characterPortraitImage.sprite = character.fullBodyImage;
                characterPortraitImage.color = Color.white;
            }

            if (characterNameText != null)
                characterNameText.text = character.characterName;

            if (characterEnglishNameText != null)
                characterEnglishNameText.text = character.characterName_eng;

            if (dialogueText != null)
                dialogueText.text = character.dialogue;

            if (ageText != null)
                ageText.text = character.age;

            if (jobText != null)
                jobText.text = character.job;

            if (heightText != null)
                heightText.text = character.height.ToString("0") + "cm";

            if (weightText != null)
                weightText.text = character.weight.ToString("0") + "kg";

            if (notesText != null)
                notesText.text = character.notes;
        }
        else
        {
            if (characterPortraitImage != null)
            {
                characterPortraitImage.sprite = character.fullBodyImage;
                characterPortraitImage.color = Color.black;
            }

            if (characterNameText != null)
                characterNameText.text = "???";

            if (characterEnglishNameText != null)
                characterEnglishNameText.text = "Unknown";

            if (dialogueText != null)
                dialogueText.text = "아직 해금되지 않은 인물입니다.";

            if (ageText != null)
                ageText.text = "???";

            if (jobText != null)
                jobText.text = "???";

            if (heightText != null)
                heightText.text = "???";

            if (weightText != null)
                weightText.text = "???";

            if (notesText != null)
                notesText.text = "도감 정보가 잠겨 있습니다.";
        }

        RefreshStoryUI(character, unlockData);
        RefreshRelationUI(character, unlockData);
    }

    private void RefreshStoryUI(CharacterData character, CharacterUnlockData unlockData)
    {
        for (int i = 0; i < storyTitleTexts.Length; i++)
        {
            bool hasStory = i < character.stories.Count;

            bool unlocked = hasStory &&
                            i < unlockData.storyUnlocked.Length &&
                            unlockData.storyUnlocked[i];

            if (unlocked)
            {
                if (storyTitleTexts[i] != null)
                    storyTitleTexts[i].text = character.stories[i].title;

                if (storyContentTexts[i] != null)
                    storyContentTexts[i].text = character.stories[i].content;
            }
            else
            {
                if (storyTitleTexts[i] != null)
                    storyTitleTexts[i].text = "???";

                if (storyContentTexts[i] != null)
                    storyContentTexts[i].text = "아직 해금되지 않은 이야기입니다.";
            }
        }
    }

    private void RefreshRelationUI(CharacterData character, CharacterUnlockData unlockData)
    {
        for (int i = 0; i < relationNameTexts.Length; i++)
        {
            bool hasRelation = i < character.relations.Count;

            bool unlocked = hasRelation &&
                            i < unlockData.relationUnlocked.Length &&
                            unlockData.relationUnlocked[i];

            if (unlocked)
            {
                CharacterRelation relation = character.relations[i];

                if (relationNameTexts[i] != null)
                    relationNameTexts[i].text = relation.characterName;

                if (relationCommentTexts[i] != null)
                    relationCommentTexts[i].text = relation.comment;
            }
            else
            {
                if (relationNameTexts[i] != null)
                    relationNameTexts[i].text = "???";

                if (relationCommentTexts[i] != null)
                    relationCommentTexts[i].text = "아직 해금되지 않은 관계입니다.";
            }
        }
    }

    #region Test Unlock Buttons

    public void TestUnlockCurrentCharacter()
    {
        CharacterData character = factions[currentFactionIndex].characters[currentCharacterIndex];
        EncyclopediaSaveManager.Instance.UnlockCharacter(character.characterId);

        RefreshCharacterButtonUI();
        RefreshCharacterDetailUI();
    }

    public void TestUnlockStory0() { UnlockCurrentStory(0); }
    public void TestUnlockStory1() { UnlockCurrentStory(1); }
    public void TestUnlockStory2() { UnlockCurrentStory(2); }
    public void TestUnlockStory3() { UnlockCurrentStory(3); }

    public void UnlockCurrentStory(int storyIndex)
    {
        CharacterData character = factions[currentFactionIndex].characters[currentCharacterIndex];
        EncyclopediaSaveManager.Instance.UnlockStory(character.characterId, storyIndex);

        RefreshCharacterDetailUI();
    }

    public void TestUnlockRelation0() { UnlockCurrentRelation(0); }
    public void TestUnlockRelation1() { UnlockCurrentRelation(1); }
    public void TestUnlockRelation2() { UnlockCurrentRelation(2); }

    public void UnlockCurrentRelation(int relationIndex)
    {
        CharacterData character = factions[currentFactionIndex].characters[currentCharacterIndex];
        EncyclopediaSaveManager.Instance.UnlockRelation(character.characterId, relationIndex);

        RefreshCharacterDetailUI();
    }

    #endregion

    public string GetCurrentCharacterId()
    {
        CharacterData character = factions[currentFactionIndex].characters[currentCharacterIndex];
        return character != null ? character.characterId : null;
    }

    public void RefreshCurrentUI()
    {
        RefreshFactionUI();
        RefreshCharacterButtonUI();
        RefreshCharacterDetailUI();
    }
}