using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class ItemInventoryEntry
{
    public ItemData item;

    [Min(0)]
    public int count;
}


public class ItemInventory : MonoBehaviour
{
    [Header("임시 시작 아이템")]
    [Tooltip("SaveData 연동 전 테스트용 수량입니다.")]
    [SerializeField]
    private List<ItemInventoryEntry> initialItems
        = new List<ItemInventoryEntry>();


    private readonly Dictionary<GameItemId, int> itemCounts
        = new Dictionary<GameItemId, int>();


    /// <summary>
    /// 아이템 수량이 변경됐을 때 발생.
    /// 나중에 인게임 UI 아이콘/수량 갱신에 사용.
    /// </summary>
    public event Action<GameItemId, int> OnItemCountChanged;


    private void Awake()
    {
        InitializeInventory();
    }


    private void InitializeInventory()
    {
        itemCounts.Clear();

        foreach (ItemInventoryEntry entry in initialItems)
        {
            if (entry == null || entry.item == null)
                continue;

            GameItemId itemId = entry.item.ItemId;
            int count = Mathf.Max(0, entry.count);

            // 같은 아이템이 실수로 여러 번 들어갔을 경우 합산
            if (itemCounts.ContainsKey(itemId))
            {
                itemCounts[itemId] += count;
            }
            else
            {
                itemCounts.Add(itemId, count);
            }
        }
    }


    /// <summary>
    /// 해당 아이템의 현재 수량 반환.
    /// </summary>
    public int GetCount(GameItemId itemId)
    {
        if (itemCounts.TryGetValue(itemId, out int count))
        {
            return count;
        }

        return 0;
    }


    /// <summary>
    /// 아이템을 필요한 수량 이상 가지고 있는지 확인.
    /// </summary>
    public bool HasItem(GameItemId itemId, int amount = 1)
    {
        if (amount <= 0)
            return true;

        return GetCount(itemId) >= amount;
    }


    /// <summary>
    /// 아이템 추가.
    /// 상점 구매 시 사용 예정.
    /// </summary>
    public void AddItem(GameItemId itemId, int amount = 1)
    {
        if (amount <= 0)
            return;

        int newCount = GetCount(itemId) + amount;

        itemCounts[itemId] = newCount;

        OnItemCountChanged?.Invoke(itemId, newCount);
    }


    /// <summary>
    /// 아이템 소비 시도.
    /// 수량이 부족하면 변경하지 않고 false 반환.
    /// </summary>
    public bool TryConsume(GameItemId itemId, int amount = 1)
    {
        if (amount <= 0)
            return true;

        int currentCount = GetCount(itemId);

        if (currentCount < amount)
            return false;

        int newCount = currentCount - amount;

        itemCounts[itemId] = newCount;

        OnItemCountChanged?.Invoke(itemId, newCount);

        return true;
    }


    /// <summary>
    /// 외부 저장 데이터 로드용.
    /// SaveData 연결 단계에서 사용.
    /// </summary>
    public void SetCount(GameItemId itemId, int count)
    {
        int safeCount = Mathf.Max(0, count);

        itemCounts[itemId] = safeCount;

        OnItemCountChanged?.Invoke(itemId, safeCount);
    }
}