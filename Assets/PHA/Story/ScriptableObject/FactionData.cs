using System;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

[CreateAssetMenu(fileName = "FactionData", menuName = "Game/Faction Data")]
public class FactionData : ScriptableObject
{
    [Header("기본 정보")]
    public string factionId;
    public string factionName;
    public string factionName_eng;
    public Sprite factionIcon;

    [Header("설립일")]
    public string foundingDate; // "YYYY.MM.DD" 형태 추천

    [Header("설명")]
    [TextArea(3, 5)]
    public string description; // 메인 설명 (최대 155자)

    [TextArea(2, 4)]
    public string subDescription; // 추가 설명 (최대 142자)

    [Header("소속 캐릭터 (최대 4명)")]
    public CharacterData[] characters = new CharacterData[4];
}