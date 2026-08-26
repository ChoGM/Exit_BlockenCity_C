using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleSoundData", menuName = "Obstacle/Sound Data")]
public class ObstacleSoundData : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public ObstacleType type;
        public AudioClip clip;
    }

    [SerializeField] private List<Entry> soundList;

    private Dictionary<ObstacleType, AudioClip> soundDict;

    public void Initialize()
    {
        soundDict = new Dictionary<ObstacleType, AudioClip>();
        foreach (var entry in soundList)
        {
            if (entry.clip != null && !soundDict.ContainsKey(entry.type))
            {
                soundDict.Add(entry.type, entry.clip);
            }
        }
    }

    public AudioClip GetClip(ObstacleType type)
    {
        if (soundDict == null) Initialize();
        soundDict.TryGetValue(type, out var clip);
        return clip;
    }
}