using System;
using System.Collections.Generic;
using UnityEngine;


public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }


    [Header("아이템 인벤토리")]
    [SerializeField]
    private ItemInventory inventory;


    [Header("아이템 효과")]
    [SerializeField]
    private List<ItemEffect> itemEffects
        = new List<ItemEffect>();


    private readonly Dictionary<GameItemId, ItemEffect> effectMap
        = new Dictionary<GameItemId, ItemEffect>();


    /// <summary>
    /// 아이템 사용 성공 이벤트.
    /// UI/VFX 등에 연결 가능.
    /// </summary>
    public event Action<GameItemId> OnItemUseSucceeded;


    /// <summary>
    /// 아이템 사용 실패 이벤트.
    /// 나중에 경고 팝업에 연결.
    /// </summary>
    public event Action<GameItemId, ItemUseResult> OnItemUseFailed;


    public ItemInventory Inventory => inventory;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        BuildEffectMap();
    }


    private void BuildEffectMap()
    {
        effectMap.Clear();

        foreach (ItemEffect effect in itemEffects)
        {
            if (effect == null)
                continue;

            if (effectMap.ContainsKey(effect.ItemId))
            {
                Debug.LogWarning(
                    $"[ItemManager] {effect.ItemId}의 ItemEffect가 중복 등록되어 있습니다."
                );

                continue;
            }

            effectMap.Add(effect.ItemId, effect);
        }
    }


    /// <summary>
    /// 아이템 사용 요청.
    ///
    /// 핵심 규칙:
    /// 효과 성공 -> 수량 차감
    /// 효과 실패 -> 수량 유지
    /// </summary>
    public ItemUseResult TryUseItem(GameItemId itemId)
    {
        if (inventory == null)
        {
            ItemUseResult result = ItemUseResult.Fail(
                ItemUseFailureReason.InvalidState,
                "아이템 인벤토리가 연결되어 있지 않습니다."
            );

            OnItemUseFailed?.Invoke(itemId, result);

            return result;
        }


        // 1. 보유 여부 검사
        if (!inventory.HasItem(itemId))
        {
            ItemUseResult result = ItemUseResult.Fail(
                ItemUseFailureReason.NotOwned,
                "보유하고 있지 않은 아이템입니다."
            );

            OnItemUseFailed?.Invoke(itemId, result);

            return result;
        }


        // 2. 효과 존재 여부 검사
        if (!effectMap.TryGetValue(itemId, out ItemEffect effect)
            || effect == null)
        {
            ItemUseResult result = ItemUseResult.Fail(
                ItemUseFailureReason.EffectNotFound,
                "아이템 효과가 등록되어 있지 않습니다."
            );

            OnItemUseFailed?.Invoke(itemId, result);

            return result;
        }


        // 3. 실제 효과 실행
        ItemUseResult useResult = effect.TryUse();


        // 4. 효과 실패
        if (!useResult.Success)
        {
            // 인벤토리 차감하지 않음
            OnItemUseFailed?.Invoke(itemId, useResult);

            return useResult;
        }


        // 5. 효과 성공했을 때만 수량 차감
        bool consumed = inventory.TryConsume(itemId, 1);

        if (!consumed)
        {
            // 정상적인 흐름에서는 발생하면 안 됨.
            Debug.LogError(
                $"[ItemManager] {itemId} 효과는 성공했지만 아이템 차감에 실패했습니다."
            );
        }


        OnItemUseSucceeded?.Invoke(itemId);

        return useResult;
    }


    /// <summary>
    /// 현재 보유 수량 조회.
    /// UI에서 사용 가능.
    /// </summary>
    public int GetItemCount(GameItemId itemId)
    {
        if (inventory == null)
            return 0;

        return inventory.GetCount(itemId);
    }
}