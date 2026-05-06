using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EncyclopediaUIController : MonoBehaviour
{
    [Header("전체 데이터")]
    public FactionData[] factions;

    [Header("세력 UI")]
    public Button[] factionButtons;
    public TMP_Text factionNameKRText;
    public TMP_Text factionNameENText;

    public TMP_Text factionDescriptionText;
    public TMP_Text factionSubDescriptionText;

    public TMP_Text factionFoundingDateText;

    public Image factionIconImage;

    [Header("캐릭터 선택 버튼")]
    public Button[] characterButtons;
    public Image[] characterButtonImages;
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

    private int currentFactionIndex = 0;
    private int currentCharacterIndex = 0;

    private void Start()
    {
        InitFactionButtons();
        InitCharacterButtons();

        SelectFaction(0);
    }

    private void InitFactionButtons()
    {
        for (int i = 0; i < factionButtons.Length; i++)
        {
            int index = i;

            if (factionButtons[i] != null)
            {
                factionButtons[i].onClick.AddListener(() => SelectFaction(index));
            }
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

    public void SelectFaction(int factionIndex)
    {
        if (factionIndex < 0 || factionIndex >= factions.Length)
            return;

        if (factions[factionIndex] == null)
            return;

        currentFactionIndex = factionIndex;
        currentCharacterIndex = 0;

        RefreshFactionUI();
        RefreshCharacterButtonUI();

        SelectCharacter(0);
    }

    public void SelectCharacter(int characterIndex)
    {
        FactionData faction = factions[currentFactionIndex];

        if (characterIndex < 0 || characterIndex >= faction.characters.Length)
            return;

        if (faction.characters[characterIndex] == null)
            return;

        currentCharacterIndex = characterIndex;

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
    }

    private void RefreshCharacterButtonUI()
    {
        FactionData faction = factions[currentFactionIndex];

        for (int i = 0; i < characterButtons.Length; i++)
        {
            bool hasCharacter =
                i < faction.characters.Length &&
                faction.characters[i] != null;

            if (characterButtons[i] != null)
                characterButtons[i].gameObject.SetActive(hasCharacter);

            if (!hasCharacter)
                continue;

            CharacterData character = faction.characters[i];

            CharacterUnlockData unlockData =
                EncyclopediaSaveManager.Instance.GetCharacterUnlockData(character.characterId);

            if (unlockData.isCharacterUnlocked)
            {
                if (characterButtonImages[i] != null)
                    characterButtonImages[i].sprite = character.portrait;

                if (characterButtonNameTexts[i] != null)
                    characterButtonNameTexts[i].text = character.characterName;
            }
            else
            {
                if (characterButtonImages[i] != null)
                    characterButtonImages[i].sprite = character.silhouette;

                if (characterButtonNameTexts[i] != null)
                    characterButtonNameTexts[i].text = "???";
            }
        }
    }

    private void RefreshCharacterDetailUI()
    {
        CharacterData character =
            factions[currentFactionIndex].characters[currentCharacterIndex];

        CharacterUnlockData unlockData =
            EncyclopediaSaveManager.Instance.GetCharacterUnlockData(character.characterId);

        if (unlockData.isCharacterUnlocked)
        {
            if (characterPortraitImage != null)
                characterPortraitImage.sprite = character.portrait;

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
                characterPortraitImage.sprite = character.silhouette;

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

            bool unlocked =
                hasStory &&
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

            bool unlocked =
                hasRelation &&
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
        CharacterData character =
            factions[currentFactionIndex].characters[currentCharacterIndex];

        EncyclopediaSaveManager.Instance.UnlockCharacter(character.characterId);

        RefreshCharacterButtonUI();
        RefreshCharacterDetailUI();
    }

    public void TestUnlockStory0()
    {
        UnlockCurrentStory(0);
    }

    public void TestUnlockStory1()
    {
        UnlockCurrentStory(1);
    }

    public void TestUnlockStory2()
    {
        UnlockCurrentStory(2);
    }

    public void TestUnlockStory3()
    {
        UnlockCurrentStory(3);
    }

    public void UnlockCurrentStory(int storyIndex)
    {
        CharacterData character =
            factions[currentFactionIndex].characters[currentCharacterIndex];

        EncyclopediaSaveManager.Instance.UnlockStory(character.characterId, storyIndex);

        RefreshCharacterDetailUI();
    }

    public void TestUnlockRelation0()
    {
        UnlockCurrentRelation(0);
    }

    public void TestUnlockRelation1()
    {
        UnlockCurrentRelation(1);
    }

    public void TestUnlockRelation2()
    {
        UnlockCurrentRelation(2);
    }

    public void UnlockCurrentRelation(int relationIndex)
    {
        CharacterData character =
            factions[currentFactionIndex].characters[currentCharacterIndex];

        EncyclopediaSaveManager.Instance.UnlockRelation(character.characterId, relationIndex);

        RefreshCharacterDetailUI();
    }

    #endregion

    public string GetCurrentCharacterId()
    {
        CharacterData character =
            factions[currentFactionIndex].characters[currentCharacterIndex];

        if (character == null)
            return null;

        return character.characterId;
    }

    public void RefreshCurrentUI()
    {
        RefreshFactionUI();
        RefreshCharacterButtonUI();
        RefreshCharacterDetailUI();
    }
}