using UnityEngine;

public class ItemDebugTester : MonoBehaviour
{
    private void Update()
    {
        if (!Application.isPlaying)
            return;


        // 5 = 마지막 설치 블록 되돌리기
        if (Input.GetKeyDown(KeyCode.Alpha5) ||
            Input.GetKeyDown(KeyCode.Keypad5))
        {
            TestUndoItem();
        }


        // 6 = 맨 밑 한 층 제거
        if (Input.GetKeyDown(KeyCode.Alpha6) ||
            Input.GetKeyDown(KeyCode.Keypad6))
        {
            TestBottomLayerClearItem();
        }


        // 7 = 폭탄
        if (Input.GetKeyDown(KeyCode.Alpha7) ||
            Input.GetKeyDown(KeyCode.Keypad7))
        {
            Debug.Log(
                "[ItemDebug] 7번키 - 폭탄 아이템은 다음 단계에서 연결합니다."
            );
        }
    }


    private void TestUndoItem()
    {
        if (ItemManager.Instance == null)
        {
            Debug.LogError(
                "[ItemDebug] ItemManager를 찾을 수 없습니다."
            );

            return;
        }


        int beforeCount =
            ItemManager.Instance.GetItemCount(
                GameItemId.UndoLastBlock
            );


        ItemUseResult result =
            ItemManager.Instance.TryUseItem(
                GameItemId.UndoLastBlock
            );


        int afterCount =
            ItemManager.Instance.GetItemCount(
                GameItemId.UndoLastBlock
            );


        Debug.Log(
            $"[ItemDebug] [5번키 - 되돌리기] " +
            $"Success: {result.Success} | " +
            $"Reason: {result.FailureReason} | " +
            $"Message: {result.Message} | " +
            $"Inventory: {beforeCount} -> {afterCount}"
        );
    }


    private void TestBottomLayerClearItem()
    {
        if (ItemManager.Instance == null)
        {
            Debug.LogError(
                "[ItemDebug] ItemManager를 찾을 수 없습니다."
            );

            return;
        }


        int beforeCount =
            ItemManager.Instance.GetItemCount(
                GameItemId.BottomLayerClear
            );


        ItemUseResult result =
            ItemManager.Instance.TryUseItem(
                GameItemId.BottomLayerClear
            );


        int afterCount =
            ItemManager.Instance.GetItemCount(
                GameItemId.BottomLayerClear
            );


        Debug.Log(
            $"[ItemDebug] [6번키 - 최하단 제거] " +
            $"Success: {result.Success} | " +
            $"Reason: {result.FailureReason} | " +
            $"Message: {result.Message} | " +
            $"Inventory: {beforeCount} -> {afterCount}"
        );
    }
}