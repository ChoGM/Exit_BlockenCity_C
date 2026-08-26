using TetrisGame;
using UnityEngine;

public class UndoLastBlockEffect : ItemEffect
{
    [Header("References")]
    [SerializeField]
    private LastPlacedBlockTracker tracker;

    [SerializeField]
    private TetrisTower tower;


    public override GameItemId ItemId
        => GameItemId.UndoLastBlock;


    private void Awake()
    {
        FindReferences();
    }


    private void FindReferences()
    {
        // 같은 ItemSystem에 Tracker가 있다면 자동 탐색
        if (tracker == null)
        {
            tracker = GetComponent<LastPlacedBlockTracker>();
        }


        if (tracker == null)
        {
            tracker = FindObjectOfType<LastPlacedBlockTracker>();
        }


        if (tower == null &&
            TetrisManager.Instance != null)
        {
            tower = TetrisManager.Instance.tower;
        }


        if (tower == null)
        {
            tower = FindObjectOfType<TetrisTower>();
        }
    }


    public override ItemUseResult TryUse()
    {
        FindReferences();


        // 시스템 참조 검사
        if (tracker == null || tower == null)
        {
            return ItemUseResult.Fail(
                ItemUseFailureReason.InvalidState,
                "되돌리기 시스템이 준비되어 있지 않습니다."
            );
        }


        if (TetrisManager.Instance == null)
        {
            return ItemUseResult.Fail(
                ItemUseFailureReason.InvalidState,
                "테트리스 시스템을 찾을 수 없습니다."
            );
        }


        // Undo 대상 요청
        if (!tracker.TryGetUndoTarget(
                out TetriminoBlock target,
                out UndoTargetState state))
        {
            return CreateFailureResult(state);
        }


        if (target == null)
        {
            return ItemUseResult.Fail(
                ItemUseFailureReason.NoUndoTarget,
                "되돌릴 수 있는 블록이 없습니다."
            );
        }


        if (!target.IsLocked)
        {
            return ItemUseResult.Fail(
                ItemUseFailureReason.InvalidState,
                "마지막 블록이 아직 설치 상태가 아닙니다."
            );
        }


        // 마지막 Piece를 구성하는 실제 Cell들
        TetriminoBlockChild[] children =
            target.GetComponentsInChildren<TetriminoBlockChild>(true);


        if (children == null || children.Length == 0)
        {
            tracker.NotifyBoardStructureChanged();

            return ItemUseResult.Fail(
                ItemUseFailureReason.BoardChanged,
                "마지막 블록의 상태가 변경되어 되돌릴 수 없습니다."
            );
        }


        // ------------------------------------------------
        // 먼저 모든 Cell이 실제 Board에 그대로 존재하는지 검증
        // ------------------------------------------------

        foreach (TetriminoBlockChild child in children)
        {
            if (child == null ||
                child.PendingDestroy)
            {
                tracker.NotifyBoardStructureChanged();

                return ItemUseResult.Fail(
                    ItemUseFailureReason.BoardChanged,
                    "마지막 블록의 상태가 변경되어 되돌릴 수 없습니다."
                );
            }


            Vector3Int position = child.GridPosition;


            if (!tower.IsInsideTower(position))
            {
                tracker.NotifyBoardStructureChanged();

                return ItemUseResult.Fail(
                    ItemUseFailureReason.BoardChanged,
                    "마지막 블록의 위치가 변경되어 되돌릴 수 없습니다."
                );
            }


            if (!tower.IsFilled(position))
            {
                tracker.NotifyBoardStructureChanged();

                return ItemUseResult.Fail(
                    ItemUseFailureReason.BoardChanged,
                    "마지막 블록의 일부가 이미 사라져 되돌릴 수 없습니다."
                );
            }
        }


        // ------------------------------------------------
        // 여기까지 왔으면 모든 검증 성공
        // 이제 실제 삭제 시작
        // ------------------------------------------------

        BlockType blockType = target.blockType;


        foreach (TetriminoBlockChild child in children)
        {
            if (child == null)
                continue;


            Vector3Int position = child.GridPosition;


            // Tower Grid에서 제거
            tower.RemoveBlockFromTower(position);


            // 설치 당시 증가했던 타입별 블록 수 원상복구
            TetrisManager.Instance.DecreaseTypeBlockCount(blockType);
        }


        // Piece 전체 비활성화 후 제거.
        // DeletBlock()을 일부러 호출하지 않는다.
        target.gameObject.SetActive(false);

        Destroy(target.gameObject);


        // 같은 상태에서 다시 Undo하지 못하게 처리
        tracker.NotifyUndoSucceeded();


        return ItemUseResult.Succeed(
            "마지막으로 설치한 블록을 되돌렸습니다."
        );
    }


    private ItemUseResult CreateFailureResult(
        UndoTargetState state)
    {
        switch (state)
        {
            case UndoTargetState.NoTarget:

                return ItemUseResult.Fail(
                    ItemUseFailureReason.NoUndoTarget,
                    "되돌릴 수 있는 블록이 없습니다."
                );


            case UndoTargetState.AlreadyUsed:

                return ItemUseResult.Fail(
                    ItemUseFailureReason.AlreadyUndone,
                    "새 블록을 설치한 후 다시 사용할 수 있습니다."
                );


            case UndoTargetState.BoardChanged:

                return ItemUseResult.Fail(
                    ItemUseFailureReason.BoardChanged,
                    "게임판이 변경되어 마지막 블록을 되돌릴 수 없습니다."
                );


            case UndoTargetState.TargetDestroyed:

                return ItemUseResult.Fail(
                    ItemUseFailureReason.NoUndoTarget,
                    "마지막 블록이 이미 제거되어 되돌릴 수 없습니다."
                );


            default:

                return ItemUseResult.Fail(
                    ItemUseFailureReason.Unknown,
                    "되돌리기 아이템을 사용할 수 없습니다."
                );
        }
    }
}