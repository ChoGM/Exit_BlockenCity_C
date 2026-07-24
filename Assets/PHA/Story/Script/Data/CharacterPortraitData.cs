using System;
using UnityEngine;

[Serializable]
public class CharacterPortraitData
{
    [Tooltip("스토리에서 사용할 표정 ID. 예: neutral, smile, serious")]
    [SerializeField] private string portraitId;

    [Tooltip("인스펙터에서 확인하기 위한 한글 이름")]
    [SerializeField] private string displayName;

    [SerializeField] private Sprite portraitSprite;

    public string PortraitId => portraitId;
    public string DisplayName => displayName;
    public Sprite PortraitSprite => portraitSprite;
}