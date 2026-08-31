using UnityEngine;

[DefaultExecutionOrder(-500)]
public class ItemInventorySaveBinder : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField]
    private ItemInventory inventory;


    [Header("Load")]
    [Tooltip(
        "Scene 시작 시 Datamanager에서 저장 데이터를 불러옵니다."
    )]
    [SerializeField]
    private bool loadGameDataOnStart = true;


    private bool isBound = false;


    private void Awake()
    {
        FindInventory();
    }


    private void Start()
    {
        Initialize();
    }


    private void OnDestroy()
    {
        Unbind();
    }


    // =====================================================
    // 초기화
    // =====================================================

    private void Initialize()
    {
        FindInventory();


        if (inventory == null)
        {
            Debug.LogError(
                "[ItemSave] ItemInventory를 찾을 수 없습니다."
            );

            return;
        }


        // ------------------------------
        // 기존 SaveData 로드
        // ------------------------------

        if (loadGameDataOnStart)
        {
            Datamanager.Instance
                .LoadGameData();
        }


        SaveData saveData =
            Datamanager.Instance.saveData;


        if (saveData == null)
        {
            Debug.LogError(
                "[ItemSave] SaveData가 없습니다."
            );

            return;
        }


        // ------------------------------
        // 기존 세이브 호환
        // ------------------------------

        bool createdNewItemData = false;


        if (saveData.items == null)
        {
            saveData.items =
                new ItemSaveData();

            createdNewItemData = true;
        }


        // ------------------------------
        // Save → Runtime Inventory
        // ------------------------------

        LoadInventoryFromSave();


        // 로드 후에 이벤트 연결
        //
        // SetCount()도 이벤트를 발생시키기 때문에
        // 먼저 이벤트를 연결하면 로드 순간마다
        // SaveGameData가 실행될 수 있음.
        Bind();


        // 예전 저장 파일에 ItemSaveData가
        // 존재하지 않았다면 새 구조를 한번 저장
        if (createdNewItemData)
        {
            Datamanager.Instance
                .SaveGameData();
        }


        Debug.Log(
            "[ItemSave] 아이템 인벤토리 로드 완료"
        );
    }


    // =====================================================
    // Inventory 찾기
    // =====================================================

    private void FindInventory()
    {
        if (inventory != null)
            return;


        inventory =
            GetComponent<ItemInventory>();


        if (inventory == null)
        {
            inventory =
                FindObjectOfType<ItemInventory>();
        }
    }


    // =====================================================
    // Save → Inventory
    // =====================================================

    private void LoadInventoryFromSave()
    {
        SaveData saveData =
            Datamanager.Instance.saveData;


        if (saveData == null ||
            saveData.items == null ||
            inventory == null)
        {
            return;
        }


        inventory.SetCount(
            GameItemId.UndoLastBlock,
            saveData.items.GetCount(
                GameItemId.UndoLastBlock
            )
        );


        inventory.SetCount(
            GameItemId.BottomLayerClear,
            saveData.items.GetCount(
                GameItemId.BottomLayerClear
            )
        );


        inventory.SetCount(
            GameItemId.Bomb3x3,
            saveData.items.GetCount(
                GameItemId.Bomb3x3
            )
        );
    }


    // =====================================================
    // Inventory 변경 이벤트 연결
    // =====================================================

    private void Bind()
    {
        if (isBound ||
            inventory == null)
        {
            return;
        }


        inventory.OnItemCountChanged
            += HandleItemCountChanged;


        isBound = true;
    }


    private void Unbind()
    {
        if (!isBound ||
            inventory == null)
        {
            return;
        }


        inventory.OnItemCountChanged
            -= HandleItemCountChanged;


        isBound = false;
    }


    // =====================================================
    // Inventory → Save
    // =====================================================

    private void HandleItemCountChanged(
        GameItemId itemId,
        int newCount)
    {
        SaveData saveData =
            Datamanager.Instance.saveData;


        if (saveData == null)
            return;


        if (saveData.items == null)
        {
            saveData.items =
                new ItemSaveData();
        }


        saveData.items.SetCount(
            itemId,
            newCount
        );


        Datamanager.Instance
            .SaveGameData();


        Debug.Log(
            $"[ItemSave] 저장 | " +
            $"{itemId} = {newCount}"
        );
    }
}