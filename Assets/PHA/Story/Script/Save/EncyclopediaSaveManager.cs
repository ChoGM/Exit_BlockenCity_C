using System.IO;
using UnityEngine;


/// <summary>
/// 데이터 확인하려면 이쪽으로
/// C:/Users/사용자명/AppData/LocalLow/DefaultCompany/MyGame
/// </summary>

public class EncyclopediaSaveManager : MonoBehaviour
{
    public static EncyclopediaSaveManager Instance;

    public EncyclopediaSaveData SaveData { get; private set; }

    private string SavePath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, "encyclopedia_unlock.json");
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);

            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Save & Load

    public void Save()
    {
        string json = JsonUtility.ToJson(SaveData, true);

        File.WriteAllText(SavePath, json);

        Debug.Log($"도감 저장 완료 : {SavePath}");
    }

    public void Load()
    {
        Debug.Log($"도감 저장 경로 : {SavePath}");

        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);

            SaveData = JsonUtility.FromJson<EncyclopediaSaveData>(json);

            Debug.Log("도감 데이터 불러오기 완료");
        }
        else
        {
            Debug.Log("도감 저장 파일 없음 → 새 파일 생성");

            CreateNewSaveData();

            Save();
        }
    }

    private void CreateNewSaveData()
    {
        SaveData = new EncyclopediaSaveData();
    }

    #endregion

    #region Character Unlock

    public CharacterUnlockData GetCharacterUnlockData(string characterId)
    {
        foreach (CharacterUnlockData data in SaveData.characters)
        {
            if (data.characterId == characterId)
            {
                return data;
            }
        }

        // 없으면 새로 생성
        CharacterUnlockData newData = new CharacterUnlockData();

        newData.characterId = characterId;
        newData.isCharacterUnlocked = false;

        newData.storyUnlocked = new bool[4];
        newData.relationUnlocked = new bool[3];

        SaveData.characters.Add(newData);

        Save();

        return newData;
    }

    public bool IsCharacterUnlocked(string characterId)
    {
        CharacterUnlockData data = GetCharacterUnlockData(characterId);

        return data.isCharacterUnlocked;
    }

    public void UnlockCharacter(string characterId)
    {
        CharacterUnlockData data = GetCharacterUnlockData(characterId);

        if (!data.isCharacterUnlocked)
        {
            data.isCharacterUnlocked = true;

            Save();

            Debug.Log($"캐릭터 해금 : {characterId}");
        }
    }

    #endregion

    #region Story Unlock

    public bool IsStoryUnlocked(string characterId, int storyIndex)
    {
        CharacterUnlockData data = GetCharacterUnlockData(characterId);

        if (storyIndex < 0 || storyIndex >= data.storyUnlocked.Length)
            return false;

        return data.storyUnlocked[storyIndex];
    }

    public void UnlockStory(string characterId, int storyIndex)
    {
        CharacterUnlockData data = GetCharacterUnlockData(characterId);

        if (storyIndex < 0 || storyIndex >= data.storyUnlocked.Length)
            return;

        if (!data.storyUnlocked[storyIndex])
        {
            data.storyUnlocked[storyIndex] = true;

            Save();

            Debug.Log($"스토리 해금 : {characterId} / Story {storyIndex}");
        }
    }

    #endregion

    #region Relation Unlock

    public bool IsRelationUnlocked(string characterId, int relationIndex)
    {
        CharacterUnlockData data = GetCharacterUnlockData(characterId);

        if (relationIndex < 0 || relationIndex >= data.relationUnlocked.Length)
            return false;

        return data.relationUnlocked[relationIndex];
    }

    public void UnlockRelation(string characterId, int relationIndex)
    {
        CharacterUnlockData data = GetCharacterUnlockData(characterId);

        if (relationIndex < 0 || relationIndex >= data.relationUnlocked.Length)
            return;

        if (!data.relationUnlocked[relationIndex])
        {
            data.relationUnlocked[relationIndex] = true;

            Save();

            Debug.Log($"관계 해금 : {characterId} / Relation {relationIndex}");
        }
    }

    #endregion

    #region Reset

    public void ResetAllData()
    {
        CreateNewSaveData();

        Save();

        Debug.Log("도감 데이터 초기화 완료");
    }

    #endregion
}