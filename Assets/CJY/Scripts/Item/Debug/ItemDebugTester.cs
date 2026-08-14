using UnityEngine;

public class ItemDebugTester : MonoBehaviour
{
    private void Update()
    {
        if (!Application.isPlaying)
            return;

        // 숫자 5 또는 숫자패드 5
        if (Input.GetKeyDown(KeyCode.Alpha5) ||
            Input.GetKeyDown(KeyCode.Keypad5))
        {
            TestUndoItem();
        }

        // 다음 단계: 최하단 한 층 제거
        if (Input.GetKeyDown(KeyCode.Alpha6) ||
            Input.GetKeyDown(KeyCode.Keypad6))
        {
            Debug.Log(
                "[ItemDebug] 6번키 - 최하단 제거 아이템은 다음 단계에서 연결합니다."
            );
        }

        // 이후 단계: 3x3x3 폭탄
        if (Input.GetKeyDown(KeyCode.Alpha7) ||
            Input.GetKeyDown(KeyCode.Keypad7))
        {
            Debug.Log(
                "[ItemDebug] 7번키 - 폭탄 아이템은 이후 단계에서 연결합니다."
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
}