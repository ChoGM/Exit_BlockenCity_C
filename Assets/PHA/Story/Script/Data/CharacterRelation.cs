using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

[System.Serializable]
public class CharacterRelation
{
    public CharacterData targetCharacter;

    public string characterName;

    [TextArea(2, 3)]
    public string comment;  // (ÃÖ´ë 47ÀÚ)
}