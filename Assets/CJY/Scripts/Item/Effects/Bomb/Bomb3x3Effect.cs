using UnityEngine;

public class Bomb3x3Effect : ItemEffect
{
    [Header("References")]
    [SerializeField]
    private TetrisSpawner spawner;

    [SerializeField]
    private TetrisController controller;


    [Header("Bomb")]
    [SerializeField]
    private TetriminoBlock bombPiecePrefab;


    public override GameItemId ItemId
        => GameItemId.Bomb3x3;


    private void Awake()
    {
        FindReferences();
    }


    private void FindReferences()
    {
        if (TetrisManager.Instance != null)
        {
            if (spawner == null)
            {
                spawner = TetrisManager.Instance.spawner;
            }

            if (controller == null)
            {
                controller = TetrisManager.Instance.controller;
            }
        }


        if (spawner == null)
        {
            spawner = FindObjectOfType<TetrisSpawner>();
        }


        if (controller == null)
        {
            controller = FindObjectOfType<TetrisController>();
        }
    }


    public override ItemUseResult TryUse()
    {
        FindReferences();


        if (TetrisManager.Instance == null ||
            spawner == null ||
            controller == null)
        {
            return ItemUseResult.Fail(
                ItemUseFailureReason.InvalidState,
                "테트리스 시스템이 준비되어 있지 않습니다."
            );
        }


        if (TetrisManager.Instance.isGameEnded)
        {
            return ItemUseResult.Fail(
                ItemUseFailureReason.InvalidState,
                "게임이 종료된 상태에서는 사용할 수 없습니다."
            );
        }


        if (bombPiecePrefab == null)
        {
            return ItemUseResult.Fail(
                ItemUseFailureReason.InvalidState,
                "폭탄 블록 프리팹이 연결되어 있지 않습니다."
            );
        }


        TetriminoBlock currentBlock =
            spawner.GetTetriminoBlock();


        if (currentBlock == null ||
            currentBlock.IsLocked)
        {
            return ItemUseResult.Fail(
                ItemUseFailureReason.NoActiveBlock,
                "현재 조작 중인 블록이 없습니다."
            );
        }


        // 이미 특수 Piece라면 중복 사용 방지
        if (currentBlock.IsSpecialPiece)
        {
            return ItemUseResult.Fail(
                ItemUseFailureReason.InvalidState,
                "이미 특수 블록을 조작 중입니다."
            );
        }


        bool replaced =
            spawner.TryReplaceCurrentBlock(
                bombPiecePrefab,
                out TetriminoBlock bombBlock
            );


        if (!replaced || bombBlock == null)
        {
            return ItemUseResult.Fail(
                ItemUseFailureReason.InvalidState,
                "현재 블록을 폭탄 블록으로 교체하지 못했습니다."
            );
        }


        // Controller가 조작하는 대상도 Bomb으로 교체
        controller.SetCurrentBlock(bombBlock);


        return ItemUseResult.Succeed(
            "현재 블록을 3x3x3 폭탄으로 교체했습니다."
        );
    }
}