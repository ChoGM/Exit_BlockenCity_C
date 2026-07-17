using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public class Datamanager : MonoBehaviour
{
    static GameObject container;
    static Datamanager instance;

    private const string SaveFolder = "Save";
    private const string GameDataFileName = "SaveData.dat";

    private string SavePath
    {
        get
        {
            string folder = Path.Combine(Application.persistentDataPath, SaveFolder);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return Path.Combine(folder, GameDataFileName);
        }
    }

    public static Datamanager Instance
    {
        get
        {
            if (!instance)
            {
                container = new GameObject("Datamanager");
                instance = container.AddComponent<Datamanager>();
                DontDestroyOnLoad(container);
            }
            return instance;
        }
    }

    public SaveData saveData = new SaveData();

    public void LoadGameData()
    {
        string path = SavePath;

        Debug.Log(path);

        if (!File.Exists(path))
        {
            Debug.Log("세이브 없음. 새 데이터 생성.");
            saveData = new SaveData();
            return;
        }

        try
        {
            string wrapperJson = File.ReadAllText(path);

            if (string.IsNullOrEmpty(wrapperJson))
                throw new Exception("파일이 비어 있음");

            SaveFileWrapper wrapper = JsonUtility.FromJson<SaveFileWrapper>(wrapperJson);

            if (wrapper == null || wrapper.data == null)
                throw new Exception("Wrapper 구조 손상");

            string hashCheck = SaveCrypto.ComputeHash(wrapper.data);
            if (hashCheck != wrapper.hash)
                throw new Exception("해시 불일치");

            string json = SaveCrypto.Decrypt(wrapper.data);
            saveData = JsonUtility.FromJson<SaveData>(json);

            Debug.Log("세이브 로드 성공");
        }
        catch (Exception e)
        {
            Debug.LogWarning("세이브 손상 감지 → 초기화: " + e.Message);

            saveData = new SaveData();

            // 초기화 후 바로 저장
            SaveGameData();
        }
    }



    public void SaveGameData()
    {
        string json = JsonUtility.ToJson(saveData, true);

        byte[] encrypted = SaveCrypto.Encrypt(json);
        string hash = SaveCrypto.ComputeHash(encrypted);

        SaveFileWrapper wrapper = new SaveFileWrapper
        {
            data = encrypted,
            hash = hash
        };

        string wrapperJson = JsonUtility.ToJson(wrapper, true);
        string path = SavePath;

        File.WriteAllText(path, wrapperJson);
        Debug.Log("암호화 저장 완료");
    }

}
