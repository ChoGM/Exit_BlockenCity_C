using UnityEngine;


[CreateAssetMenu(
    fileName = "ItemData",
    menuName = "Item/Item Data"
)]

public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField]
    private GameItemId itemId;

    [SerializeField]
    private string itemName;

    [TextArea(2, 5)]
    [SerializeField]
    private string description;

    [SerializeField]
    private Sprite icon;

    [Header("상점 정보")]
    [Min(0)]
    [SerializeField]
    private int price;


    public GameItemId ItemId => itemId;

    public string ItemName => itemName;

    public string Description => description;

    public Sprite Icon => icon;

    public int Price => price;
}