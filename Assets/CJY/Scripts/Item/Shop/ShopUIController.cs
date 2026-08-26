using UnityEngine;

public class ShopUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private ShopManager shopManager;

    [SerializeField]
    private ShopCharacterPresenter characterPresenter;


    [Header("Normal Items")]
    [SerializeField]
    private ItemData undoItem;

    [SerializeField]
    private ItemData bottomLayerClearItem;

    [SerializeField]
    private ItemData bombItem;


    private void Awake()
    {
        FindReferences();
    }


    private void FindReferences()
    {
        if (shopManager == null)
        {
            shopManager =
                FindObjectOfType<ShopManager>();
        }


        if (characterPresenter == null)
        {
            characterPresenter =
                FindObjectOfType<ShopCharacterPresenter>();
        }
    }


    // =====================================================
    // 일반 아이템
    // =====================================================

    public void OnClickBuyUndo()
    {
        TryPurchaseItem(
            undoItem
        );
    }


    public void OnClickBuyBottomLayerClear()
    {
        TryPurchaseItem(
            bottomLayerClearItem
        );
    }


    public void OnClickBuyBomb()
    {
        TryPurchaseItem(
            bombItem
        );
    }


    private void TryPurchaseItem(
        ItemData itemData)
    {
        FindReferences();


        if (shopManager == null)
        {
            Debug.LogError(
                "[ShopUI] ShopManager를 찾을 수 없습니다."
            );

            return;
        }


        ShopPurchaseResult result =
            shopManager.TryPurchaseItem(
                itemData
            );


        if (result.Success)
        {
            if (characterPresenter != null)
            {
                characterPresenter
                    .PlayItemPurchaseSuccess(
                        itemData
                    );
            }
        }
        else
        {
            if (characterPresenter != null)
            {
                characterPresenter
                    .PlayItemPurchaseFailed(
                        itemData
                    );
            }


            Debug.LogWarning(
                $"[ShopUI] 구매 실패 | " +
                $"Reason: {result.FailureReason} | " +
                $"{result.Message}"
            );
        }
    }


    // =====================================================
    // 세력 우호도 구매
    // =====================================================

    public void OnClickDanwol()
    {
        TryPurchaseFavor(
            ShopFaction.Danwol
        );
    }


    public void OnClickYaseo()
    {
        TryPurchaseFavor(
            ShopFaction.Yaseo
        );
    }


    public void OnClickMacheon()
    {
        TryPurchaseFavor(
            ShopFaction.Macheon
        );
    }


    public void OnClickHongryeon()
    {
        TryPurchaseFavor(
            ShopFaction.Hongryeon
        );
    }


    public void OnClickJeonSangYeon()
    {
        TryPurchaseFavor(
            ShopFaction.JeonSangYeon
        );
    }


    private void TryPurchaseFavor(
        ShopFaction faction)
    {
        FindReferences();


        if (shopManager == null)
        {
            Debug.LogError(
                "[ShopUI] ShopManager를 찾을 수 없습니다."
            );

            return;
        }


        ShopPurchaseResult result =
            shopManager.TryPurchaseFavor(
                faction
            );


        if (result.Success)
        {
            if (characterPresenter != null)
            {
                characterPresenter
                    .PlayFavorPurchaseSuccess();
            }
        }
        else
        {
            if (characterPresenter != null)
            {
                characterPresenter
                    .PlayFavorPurchaseFailed();
            }


            Debug.LogWarning(
                $"[ShopUI] 우호도 구매 실패 | " +
                $"Faction: {faction} | " +
                $"Reason: {result.FailureReason} | " +
                $"{result.Message}"
            );
        }
    }
}