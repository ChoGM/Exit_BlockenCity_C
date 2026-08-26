using UnityEngine;

public class ItemHotkeyInput : MonoBehaviour
{
    [Header("Items")]
    [SerializeField]
    private ItemData undoItem;

    [SerializeField]
    private ItemData bottomClearItem;

    [SerializeField]
    private ItemData bombItem;


    private void Update()
    {
        // 게임 종료
        if (GameManager.Instance != null &&
            GameManager.Instance.isGameEnded)
        {
            return;
        }


        // 확인창이 열려있다면
        // 5 / 6 / 7 추가 입력 차단
        if (ItemUseConfirmUI.Instance != null &&
            ItemUseConfirmUI.Instance.IsConfirmOpen)
        {
            return;
        }


        // 다른 시스템의 Pause 중에는
        // 아이템 단축키 사용 차단
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGamePaused())
        {
            return;
        }


        // --------------------------
        // 5 : Undo
        // --------------------------

        if (Input.GetKeyDown(KeyCode.Alpha5) ||
            Input.GetKeyDown(KeyCode.Keypad5))
        {
            RequestItem(undoItem);
        }


        // --------------------------
        // 6 : Bottom Clear
        // --------------------------

        if (Input.GetKeyDown(KeyCode.Alpha6) ||
            Input.GetKeyDown(KeyCode.Keypad6))
        {
            RequestItem(bottomClearItem);
        }


        // --------------------------
        // 7 : Bomb
        // --------------------------

        if (Input.GetKeyDown(KeyCode.Alpha7) ||
            Input.GetKeyDown(KeyCode.Keypad7))
        {
            RequestItem(bombItem);
        }
    }


    private void RequestItem(
        ItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogWarning(
                "[ItemHotkeyInput] ItemData가 연결되지 않았습니다."
            );

            return;
        }


        if (ItemUseConfirmUI.Instance == null)
        {
            Debug.LogError(
                "[ItemHotkeyInput] ItemUseConfirmUI가 없습니다."
            );

            return;
        }


        ItemUseConfirmUI.Instance
            .RequestUseItem(itemData);
    }
}