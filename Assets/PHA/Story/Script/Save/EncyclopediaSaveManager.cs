using System.IO;
using UnityEngine;

public class EncyclopediaSaveManager : MonoBehaviour
{
    public static EncyclopediaSaveManager Instance;

    // Datamanager의 SaveData 내 도감 데이터 프로퍼티
    public EncyclopediaSaveData SaveData
    {
        get
        {
            if (Datamanager.Instance != null && Datamanager.Instance.saveData != null)
            {
                return Datamanager.Instance.saveData.encyclopedia;
            }
            return null;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Save & Load

    public void Save()
    {
        // 도감 개별 저장이 아닌, 전체 데이터 매니저를 통해 암호화 저장 호출
        if (Datamanager.Instance != null)
        {
            Datamanager.Instance.SaveGameData();
            Debug.Log("도감 데이터가 Datamanager를 통해 통합 저장되었습니다.");
        }
    }

    public void Load()
    {
        // Datamanager에서 이미 게임 데이터를 불러오므로 별도 개별 로드는 필요하지 않습니다.
        if (Datamanager.Instance != null)
        {
            Datamanager.Instance.LoadGameData();
        }
    }

    #endregion

    #region Character Unlock

    public CharacterUnlockData GetCharacterUnlockData(string characterId)
    {
        var saveData = SaveData;
        if (saveData == null) return null;

        foreach (CharacterUnlockData data in saveData.characters)
        {
            if (data.characterId == characterId)
            {
                return data;
            }
        }

        // 없으면 새로 생성하여 Datamanager 데이터에 추가
        CharacterUnlockData newData = new CharacterUnlockData
        {
            characterId = characterId,
            isCharacterUnlocked = false,
            storyUnlocked = new bool[4],
            relationUnlocked = new bool[3]
        };

        saveData.characters.Add(newData);
        Save();

        return newData;
    }

    public bool IsCharacterUnlocked(string characterId)
    {
        CharacterUnlockData data = GetCharacterUnlockData(characterId);
        return data != null && data.isCharacterUnlocked;
    }

    public void UnlockCharacter(string characterId)
    {
        CharacterUnlockData data = GetCharacterUnlockData(characterId);

        if (data != null && !data.isCharacterUnlocked)
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
        if (data == null || storyIndex < 0 || storyIndex >= data.storyUnlocked.Length)
            return false;

        return data.storyUnlocked[storyIndex];
    }

    public void UnlockStory(string characterId, int storyIndex)
    {
        CharacterUnlockData data = GetCharacterUnlockData(characterId);
        if (data == null || storyIndex < 0 || storyIndex >= data.storyUnlocked.Length)
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
        if (data == null || relationIndex < 0 || relationIndex >= data.relationUnlocked.Length)
            return false;

        return data.relationUnlocked[relationIndex];
    }

    public void UnlockRelation(string characterId, int relationIndex)
    {
        CharacterUnlockData data = GetCharacterUnlockData(characterId);
        if (data == null || relationIndex < 0 || relationIndex >= data.relationUnlocked.Length)
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
        if (SaveData != null)
        {
            SaveData.characters.Clear();
            Save();
            Debug.Log("도감 데이터 초기화 완료");
        }
    }

    #endregion
}