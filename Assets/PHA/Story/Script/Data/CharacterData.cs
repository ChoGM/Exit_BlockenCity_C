using System;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
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

    private void OnValidate()
    {
        if (stories.Count > 4)
            stories.RemoveRange(4, stories.Count - 4);

        if (relations.Count > 3)
            relations.RemoveRange(3, relations.Count - 3);
    }
}