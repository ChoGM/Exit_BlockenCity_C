using UnityEngine;

public class BottomLayerRemoveEffect : ItemEffect
{
    [Header("References")]
    [SerializeField]
    private TetrisTower tower;


    public override GameItemId ItemId
        => GameItemId.BottomLayerClear;


    private void Awake()
    {
        FindReferences();
    }


    private void FindReferences()
    {
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


        if (TetrisManager.Instance == null ||
            tower == null)
        {
            return ItemUseResult.Fail(
                ItemUseFailureReason.InvalidState,
                "테트리스 시스템이 준비되어 있지 않습니다."
            );
        }


        // 게임 종료 상태에서는 사용 불가
        if (TetrisManager.Instance.isGameEnded)
        {
            return ItemUseResult.Fail(
                ItemUseFailureReason.InvalidState,
                "게임이 종료된 상태에서는 사용할 수 없습니다."
            );
        }


        // ------------------------------------------------
        // 최하단(Y = 0)에 실제 고정 블록이 존재하는지 확인
        // ------------------------------------------------

        bool hasBottomBlock = false;

        TetriminoBlockChild[] children =
            FindObjectsOfType<TetriminoBlockChild>();


        foreach (TetriminoBlockChild child in children)
        {
            if (child == null ||
                child.PendingDestroy)
            {
                continue;
            }


            TetriminoBlock parent =
                child.GetComponentInParent<TetriminoBlock>();


            // 낙하 중인 현재 Piece는 제외
            if (parent == null ||
                !parent.IsLocked)
            {
                continue;
            }


            if (child.GridPosition.y == 0)
            {
                hasBottomBlock = true;
                break;
            }
        }


        // 맨 밑에 지울 블록이 없으면 아이템 사용 실패
        if (!hasBottomBlock)
        {
            return ItemUseResult.Fail(
                ItemUseFailureReason.InvalidState,
                "맨 밑 층에 제거할 블록이 없습니다."
            );
        }


        // ------------------------------------------------
        // 기존 TetrisTower 기능 재사용
        // Y = 0 삭제 후 모든 상단 블록이 한 칸 내려간다.
        // ------------------------------------------------

        tower.DeleteLine(0);


        return ItemUseResult.Succeed(
            "맨 밑 층을 제거했습니다."
        );
    }
}