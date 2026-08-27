using UnityEngine;
using TMPro;

public class ShopPriceUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private ShopManager shopManager;


    [Header("Item Data")]
    [SerializeField]
    private ItemData undoItem;

    [SerializeField]
    private ItemData bottomLayerClearItem;

    [SerializeField]
    private ItemData bombItem;


    [Header("Price Text")]
    [SerializeField]
    private TMP_Text undoPriceText;

    [SerializeField]
    private TMP_Text bottomPriceText;

    [SerializeField]
    private TMP_Text bombPriceText;

    [SerializeField]
    private TMP_Text favorPriceText;


    private void Awake()
    {
        FindReferences();
    }


    private void OnEnable()
    {
        FindReferences();

        RefreshPrices();
    }


    private void Start()
    {
        RefreshPrices();
    }


    private void FindReferences()
    {
        if (shopManager == null)
        {
            shopManager =
                FindObjectOfType<ShopManager>();
        }
    }


    public void RefreshPrices()
    {
        // -------------------------
        // 登倒府扁
        // -------------------------

        if (undoPriceText != null &&
            undoItem != null)
        {
            undoPriceText.text =
                undoItem.Price.ToString("N0");
        }


        // -------------------------
        // 盖关 力芭
        // -------------------------

        if (bottomPriceText != null &&
            bottomLayerClearItem != null)
        {
            bottomPriceText.text =
                bottomLayerClearItem.Price.ToString("N0");
        }


        // -------------------------
        // 气藕
        // -------------------------

        if (bombPriceText != null &&
            bombItem != null)
        {
            bombPriceText.text =
                bombItem.Price.ToString("N0");
        }


        // -------------------------
        // 快龋档 惑前
        // -------------------------

        if (favorPriceText != null &&
            shopManager != null)
        {
            favorPriceText.text =
                shopManager.FavorPrice.ToString("N0");
        }
    }
}