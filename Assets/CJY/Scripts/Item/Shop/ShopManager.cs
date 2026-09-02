using System;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance
    {
        get;
        private set;
    }


    [Header("References")]
    [SerializeField]
    private ItemInventory inventory;


    [Header("Favor Product")]
    [Min(0)]
    [SerializeField]
    private int favorPrice = 1000;

    [Min(0f)]
    [SerializeField]
    private float favorAmount = 10f;


    /// <summary>
    /// 상점에서 재화가 변경되었을 때 발생.
    /// ShopMemoUI 갱신에 사용.
    /// </summary>
    public event Action<int> OnMoneyChanged;


    public int FavorPrice => favorPrice;

    public float FavorAmount => favorAmount;


    public int CurrentMoney
    {
        get
        {
            if (Datamanager.Instance == null ||
                Datamanager.Instance.saveData == null ||
                Datamanager.Instance.saveData.player == null)
            {
                return 0;
            }


            return Datamanager.Instance
                .saveData
                .player
                .totalMoney;
        }
    }


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


        FindReferences();
    }


    private void Start()
    {
        FindReferences();
    }


    private void FindReferences()
    {
        if (inventory == null &&
            ItemManager.Instance != null)
        {
            inventory =
                ItemManager.Instance.Inventory;
        }


        if (inventory == null)
        {
            inventory =
                FindObjectOfType<ItemInventory>();
        }
    }


    // =====================================================
    // 일반 아이템 구매
    // =====================================================

    public ShopPurchaseResult TryPurchaseItem(
        ItemData itemData)
    {
        FindReferences();


        if (itemData == null)
        {
            return ShopPurchaseResult.Fail(
                ShopPurchaseFailureReason.InvalidItem,
                "구매할 아이템 정보가 없습니다."
            );
        }


        if (inventory == null)
        {
            return ShopPurchaseResult.Fail(
                ShopPurchaseFailureReason.InvalidState,
                "아이템 인벤토리를 찾을 수 없습니다."
            );
        }


        if (Datamanager.Instance == null ||
            Datamanager.Instance.saveData == null ||
            Datamanager.Instance.saveData.player == null)
        {
            return ShopPurchaseResult.Fail(
                ShopPurchaseFailureReason.InvalidState,
                "플레이어 저장 데이터를 찾을 수 없습니다."
            );
        }


        int price =
            Mathf.Max(0, itemData.Price);


        int currentMoney =
            Datamanager.Instance
                .saveData
                .player
                .totalMoney;


        // -------------------------
        // 재화 부족
        // -------------------------

        if (currentMoney < price)
        {
            return ShopPurchaseResult.Fail(
                ShopPurchaseFailureReason.NotEnoughMoney,
                "보유 재화가 부족합니다."
            );
        }


        // -------------------------
        // 재화 차감
        // -------------------------

        int newMoney =
            currentMoney - price;


        Datamanager.Instance
            .saveData
            .player
            .totalMoney = newMoney;


        // -------------------------
        // 아이템 +1
        // -------------------------

        inventory.AddItem(
            itemData.ItemId,
            1
        );


        // -------------------------
        // UI 갱신 이벤트
        // -------------------------

        OnMoneyChanged?.Invoke(
            newMoney
        );


        Debug.Log(
            $"[Shop] 아이템 구매 성공 | " +
            $"{itemData.ItemName} | " +
            $"가격: {price} | " +
            $"남은 재화: {newMoney}"
        );


        // 중요:
        // 현재 단계에서는 SaveData 구조를 수정하지 않으므로
        // 여기서 SaveGameData()를 강제로 호출하지 않는다.
        //
        // 아이템 수량 저장은 7단계에서 함께 연결.


        return ShopPurchaseResult.Succeed(
            $"{itemData.ItemName} 구매 완료"
        );
    }


    // =====================================================
    // 우호도 구매
    // =====================================================

    public ShopPurchaseResult TryPurchaseFavor(
        ShopFaction faction)
    {
        if (Datamanager.Instance == null ||
            Datamanager.Instance.saveData == null ||
            Datamanager.Instance.saveData.player == null ||
            Datamanager.Instance.saveData.relationship == null)
        {
            return ShopPurchaseResult.Fail(
                ShopPurchaseFailureReason.InvalidState,
                "저장 데이터를 찾을 수 없습니다."
            );
        }


        if (!IsValidFaction(faction))
        {
            return ShopPurchaseResult.Fail(
                ShopPurchaseFailureReason.InvalidFaction,
                "잘못된 세력입니다."
            );
        }


        int currentMoney =
            Datamanager.Instance
                .saveData
                .player
                .totalMoney;


        // -------------------------
        // 재화 부족
        // -------------------------

        if (currentMoney < favorPrice)
        {
            return ShopPurchaseResult.Fail(
                ShopPurchaseFailureReason.NotEnoughMoney,
                "보유 재화가 부족합니다."
            );
        }


        // -------------------------
        // 재화 차감
        // -------------------------

        int newMoney =
            currentMoney - favorPrice;


        Datamanager.Instance
            .saveData
            .player
            .totalMoney = newMoney;


        // -------------------------
        // 선택 세력 우호도 증가
        // -------------------------

        AddFavor(
            faction,
            favorAmount
        );

        // 재화 + 우호도 저장
        Datamanager.Instance
            .SaveGameData();

        OnMoneyChanged?.Invoke(
            newMoney
        );


        Debug.Log(
            $"[Shop] 우호도 구매 성공 | " +
            $"Faction: {faction} | " +
            $"Favor +{favorAmount} | " +
            $"남은 재화: {newMoney}"
        );


        return ShopPurchaseResult.Succeed(
            "우호도 거래가 완료되었습니다."
        );
    }


    // =====================================================
    // 우호도 실제 적용
    // =====================================================

    private void AddFavor(
        ShopFaction faction,
        float amount)
    {
        RelationshipData relationship =
            Datamanager.Instance
                .saveData
                .relationship;


        switch (faction)
        {
            case ShopFaction.Danwol:

                relationship.danwol
                    += amount;

                break;


            case ShopFaction.Yaseo:

                relationship.yaseo
                    += amount;

                break;


            case ShopFaction.Macheon:

                relationship.macheon
                    += amount;

                break;


            case ShopFaction.Hongryeon:

                relationship.hongryeon
                    += amount;

                break;


            case ShopFaction.JeonSangYeon:

                relationship.JeonSangYeon
                    += amount;

                break;
        }
    }


    private bool IsValidFaction(
        ShopFaction faction)
    {
        switch (faction)
        {
            case ShopFaction.Danwol:
            case ShopFaction.Yaseo:
            case ShopFaction.Macheon:
            case ShopFaction.Hongryeon:
            case ShopFaction.JeonSangYeon:

                return true;


            default:

                return false;
        }
    }
}