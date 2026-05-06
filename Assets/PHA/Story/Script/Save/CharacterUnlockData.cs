using System;

[Serializable]
public class CharacterUnlockData
{
    public string characterId;

    public bool isCharacterUnlocked;

    public bool[] storyUnlocked = new bool[4];

    public bool[] relationUnlocked = new bool[3];
}