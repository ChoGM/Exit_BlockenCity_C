using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TetrisGame;

[System.Serializable]
public class BlockPrefabEntry
{
    public BlockType type;
    public GameObject prefab;
}

// Project Settings > Script Execution Order ���� BlockPrefabBinder��
// Default Time ���� ���� ����(���� �켱����) �ǵ��� ����
public class BlockPrefabBinder : MonoBehaviour
{
    [SerializeField] private List<BlockPrefabEntry> prefabList;

    public static readonly Dictionary<BlockType, GameObject> Prefabs = new();

    private void Awake()
    {
        Prefabs.Clear();
        foreach (var entry in prefabList)
        {
            if (entry.type == BlockType.None) continue; // ������ġ
            if (entry.prefab == null) continue;

            if (!Prefabs.ContainsKey(entry.type))
                Prefabs.Add(entry.type, entry.prefab);
        }
    }
}
