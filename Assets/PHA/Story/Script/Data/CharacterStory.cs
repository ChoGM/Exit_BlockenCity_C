using UnityEngine;

[System.Serializable]
public class CharacterStory
{
    public string title;

    [TextArea(3, 6)]
    public string content;
}