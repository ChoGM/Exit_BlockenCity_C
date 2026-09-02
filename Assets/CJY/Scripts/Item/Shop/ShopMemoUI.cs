using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopMemoUI : MonoBehaviour
{
    [Header("Money")]
    [SerializeField]
    private TMP_Text moneyText;


    [Header("Item Count")]
    [SerializeField]
    private TMP_Text undoCountText;

    [SerializeField]
    private TMP_Text bottomCountText;

    [SerializeField]
    private TMP_Text bombCountText;


    [Header("References")]
    [SerializeField]
    private ShopManager shopManager;

    [SerializeField]
    private ItemInventory inventory;


    private bool isBound = false;


    private void OnEnable()
    {
        TryBind();

        RefreshAll();
    }


    private void Start()
    {
        TryBind();

        RefreshAll();
    }


    private void OnDisable()
    {
        Unbind();
    }


    private void TryBind()
    {
        if (isBound)
            return;


        if (shopManager == null)
        {
            shopManager =
                FindObjectOfType<ShopManager>();
        }


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


        if (shopManager != null)
        {
            shopManager.OnMoneyChanged
                += HandleMoneyChanged;
        }


        if (inventory != null)
        {
            inventory.OnItemCountChanged
                += HandleItemCountChanged;
        }


        isBound = true;
    }


    private void Unbind()
    {
        if (!isBound)
            return;


        if (shopManager != null)
        {
            shopManager.OnMoneyChanged
                -= HandleMoneyChanged;
        }


        if (inventory != null)
        {
            inventory.OnItemCountChanged
                -= HandleItemCountChanged;
        }


        isBound = false;
    }


    private void HandleMoneyChanged(
        int newMoney)
    {
        RefreshMoney(
            newMoney
        );
    }


    private void HandleItemCountChanged(
        GameItemId itemId,
        int newCount)
    {
        switch (itemId)
        {
            case GameItemId.UndoLastBlock:

                SetCountText(
                    undoCountText,
                    newCount
                );

                break;


            case GameItemId.BottomLayerClear:

                SetCountText(
                    bottomCountText,
                    newCount
                );

                break;


            case GameItemId.Bomb3x3:

                SetCountText(
                    bombCountText,
                    newCount
                );

                break;
        }
    }


    public void RefreshAll()
    {
        // -------------------------
        // 재화
        // -------------------------

        int money = 0;


        if (Datamanager.Instance != null &&
            Datamanager.Instance.saveData != null &&
            Datamanager.Instance.saveData.player != null)
        {
            money =
                Datamanager.Instance
                    .saveData
                    .player
                    .totalMoney;
        }


        RefreshMoney(
            money
        );


        // -------------------------
        // 아이템
        // -------------------------

        if (inventory == null)
            return;


        SetCountText(
            undoCountText,
            inventory.GetCount(
                GameItemId.UndoLastBlock
            )
        );


        SetCountText(
            bottomCountText,
            inventory.GetCount(
                GameItemId.BottomLayerClear
            )
        );


        SetCountText(
            bombCountText,
            inventory.GetCount(
                GameItemId.Bomb3x3
            )
        );
    }


    private void RefreshMoney(
        int money)
    {
        if (moneyText == null)
            return;


        moneyText.text =
            money.ToString("N0");
    }


    private void SetCountText(
        TMP_Text text,
        int count)
    {
        if (text == null)
            return;


        text.text =
            count.ToString();
    }
}