using System;
using UnityEngine;

public enum StoryEffectType
{
    None,

    PortraitFadeIn,
    PortraitFadeOut,

    PortraitShakeSmall,
    PortraitShakeStrong,

    PortraitZoomIn,
    PortraitZoomOut,

    ScreenFlash,
    ScreenDarken,

    TextShake
}

public enum StoryEffectTarget
{
    Portrait,
    DialoguePanel,
    DialogueText,
    Background,
    EntireScreen
}

[Serializable]
public class StoryEffectData
{
    [SerializeField]
    private StoryEffectType effectType;

    [SerializeField]
    private StoryEffectTarget target;

    [Min(0f)]
    [SerializeField]
    private float duration = 0.3f;

    [Min(0f)]
    [SerializeField]
    private float strength = 1f;

    [Tooltip("효과가 끝날 때까지 스토리 진행을 막을지 여부")]
    [SerializeField]
    private bool waitForCompletion;

    public StoryEffectType EffectType => effectType;
    public StoryEffectTarget Target => target;
    public float Duration => duration;
    public float Strength => strength;
    public bool WaitForCompletion => waitForCompletion;
}