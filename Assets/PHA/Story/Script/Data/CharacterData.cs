using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("기본 정보")]
    public string characterId;
    public string characterName;
    public string characterName_eng;

    [TextArea(2, 3)]
    public string dialogue; // 캐릭터 한마디 (최대 55자)

    [Header("이미지")]
    public Sprite portrait;        // 도감용 기본 이미지
    public Sprite fullBodyImage;   // 전신 이미지 (선택)
    public Sprite silhouette; // 잠금 상태용 그림자 이미지
    public Sprite fullBodysilhouette; // 잠금 상태용  전신 그림자 이미지


    [Header("스토리 초상화")]
    [SerializeField]
    private List<CharacterPortraitData> storyPortraits = new();


    [Header("프로필")]
    public string age;
    public string job;

    public float height; // cm
    public float weight; // kg

    [TextArea(3, 5)]
    public string notes; // (최대 130자)

    [Header("스토리 (최대 4개)")]
    public List<CharacterStory> stories = new List<CharacterStory>(4);

    [Header("관계 (최대 3개)")]
    public List<CharacterRelation> relations = new List<CharacterRelation>(3);

    public string CharacterId => characterId;
    public string CharacterName => characterName;
    public string CharacterNameEng => characterName_eng;
    public string Dialogue => dialogue;

    public Sprite Portrait => portrait;
    public Sprite FullBodyImage => fullBodyImage;
    public Sprite Silhouette => silhouette;

    public string Age => age;
    public string Job => job;
    public float Height => height;
    public float Weight => weight;
    public string Notes => notes;

    public IReadOnlyList<CharacterPortraitData> StoryPortraits =>
        storyPortraits;

    /// <summary>
    /// 표정 ID에 해당하는 스토리 초상화를 반환합니다.
    /// 찾지 못하면 도감용 기본 초상화를 반환합니다.
    /// </summary>
    public Sprite GetStoryPortrait(string portraitId)
    {
        if (string.IsNullOrWhiteSpace(portraitId))
        {
            return portrait;
        }

        CharacterPortraitData portraitData =
            storyPortraits.Find(data =>
                data != null &&
                data.PortraitId == portraitId
            );

        if (portraitData != null &&
            portraitData.PortraitSprite != null)
        {
            return portraitData.PortraitSprite;
        }

        Debug.LogWarning(
            $"[{characterName}] 캐릭터에 " +
            $"'{portraitId}' 표정 이미지가 없습니다. " +
            "기본 초상화를 사용합니다.",
            this
        );

        return portrait;
    }

    private void OnValidate()
    {
        if (stories == null)
        {
            stories = new List<CharacterStory>();
        }

        if (relations == null)
        {
            relations = new List<CharacterRelation>();
        }

        if (storyPortraits == null)
        {
            storyPortraits =
                new List<CharacterPortraitData>();
        }

        if (stories.Count > 4)
        {
            stories.RemoveRange(
                4,
                stories.Count - 4
            );
        }

        if (relations.Count > 3)
        {
            relations.RemoveRange(
                3,
                relations.Count - 3
            );
        }

        ValidatePortraitIds();
    }

    private void ValidatePortraitIds()
    {
        HashSet<string> usedIds = new();

        foreach (CharacterPortraitData portraitData
                 in storyPortraits)
        {
            if (portraitData == null ||
                string.IsNullOrWhiteSpace(
                    portraitData.PortraitId))
            {
                continue;
            }

            if (!usedIds.Add(portraitData.PortraitId))
            {
                Debug.LogWarning(
                    $"[{characterName}] 중복된 표정 ID: " +
                    portraitData.PortraitId,
                    this
                );
            }
        }
    }
}
