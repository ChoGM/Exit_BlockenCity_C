using System.Collections.Generic;
using UnityEngine;

public class BombBlockBehaviour : SpecialBlockLockHandler
{
    [Header("References")]
    [SerializeField]
    private TetrisTower tower;

    [SerializeField]
    private LastPlacedBlockTracker undoTracker;


    private bool exploded = false;


    private void Awake()
    {
        FindReferences();
    }


    private void FindReferences()
    {
        if (TetrisManager.Instance != null &&
            tower == null)
        {
            tower = TetrisManager.Instance.tower;
        }


        if (tower == null)
        {
            tower = FindObjectOfType<TetrisTower>();
        }


        if (undoTracker == null)
        {
            undoTracker =
                FindObjectOfType<LastPlacedBlockTracker>();
        }
    }


    public override void HandleSpecialLock(
        TetriminoBlock owner)
    {
        if (exploded)
            return;

        exploded = true;

        FindReferences();


        if (owner == null ||
            TetrisManager.Instance == null ||
            tower == null)
        {
            Debug.LogError(
                "[Bomb] 폭탄 시스템 참조를 찾을 수 없습니다."
            );

            return;
        }


        // ---------------------------------------------
        // 폭탄의 실제 1x1 Cell 위치를 중심점으로 사용
        // ---------------------------------------------

        TetriminoBlockChild bombCell =
            owner.GetComponentInChildren<TetriminoBlockChild>();


        Vector3 bombWorldPosition =
            bombCell != null
                ? bombCell.transform.position
                : owner.transform.position;


        Vector3Int center =
            owner.WorldToTowerPosition(
                bombWorldPosition
            );


        Debug.Log(
            $"[Bomb] 폭발 중심: {center}"
        );


        // ---------------------------------------------
        // 3 x 3 x 3 범위 생성
        // 타워 밖 좌표는 넣지 않는다.
        // ---------------------------------------------

        HashSet<Vector3Int> explosionArea =
            new HashSet<Vector3Int>();


        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Vector3Int targetPosition =
                        center +
                        new Vector3Int(x, y, z);


                    if (!tower.IsInsideTower(
                            targetPosition))
                    {
                        continue;
                    }


                    explosionArea.Add(
                        targetPosition
                    );
                }
            }
        }


        // ---------------------------------------------
        // 실제 설치된 블록 중
        // 폭발 범위 안에 있는 Cell 검색
        // ---------------------------------------------

        TetriminoBlockChild[] allChildren =
            FindObjectsOfType<TetriminoBlockChild>();


        List<TetriminoBlockChild> targets =
            new List<TetriminoBlockChild>();


        HashSet<TetriminoBlock> affectedParents =
            new HashSet<TetriminoBlock>();


        foreach (TetriminoBlockChild child in allChildren)
        {
            if (child == null ||
                child.PendingDestroy)
            {
                continue;
            }


            TetriminoBlock parent =
                child.GetComponentInParent<TetriminoBlock>();


            if (parent == null)
                continue;


            // 자기 자신(폭탄)은 삭제 대상에서 제외.
            // 폭탄은 마지막에 Piece 전체를 제거한다.
            if (parent == owner)
                continue;


            // 일반 설치 블록만 대상
            if (!parent.IsLocked)
                continue;


            if (parent.IsSpecialPiece)
                continue;


            Vector3Int position =
                child.GridPosition;


            if (!tower.IsInsideTower(position))
                continue;


            if (!tower.IsFilled(position))
                continue;


            if (!explosionArea.Contains(position))
                continue;


            targets.Add(child);
            affectedParents.Add(parent);
        }


        // ---------------------------------------------
        // 실제 블록 삭제
        // ---------------------------------------------

        int destroyedCount = 0;


        foreach (TetriminoBlockChild target in targets)
        {
            if (target == null ||
                target.PendingDestroy)
            {
                continue;
            }


            target.DeletBlock();

            destroyedCount++;
        }


        // ---------------------------------------------
        // 자식이 전부 사라진 Tetrimino 부모 정리
        // ---------------------------------------------

        foreach (TetriminoBlock parent
                 in affectedParents)
        {
            if (parent == null)
                continue;

            parent.CleanupIfEmpty();
        }


        // ---------------------------------------------
        // 폭탄은 Board 구조를 바꾸는 행동이므로
        // 이전 Undo 대상 무효화
        // ---------------------------------------------

        if (undoTracker != null)
        {
            undoTracker.NotifyBoardStructureChanged();
        }


        Debug.Log(
            $"[Bomb] 3x3x3 폭발 완료 | " +
            $"삭제 블록 수: {destroyedCount}"
        );


        // ---------------------------------------------
        // 폭탄 자체는 Tower에 등록하지 않는다.
        // ---------------------------------------------

        owner.gameObject.SetActive(false);

        Destroy(owner.gameObject);


        // ---------------------------------------------
        // 다음 일반 블록 진행
        // ---------------------------------------------

        TetrisManager.Instance.SpawnNextBlock();
    }
}