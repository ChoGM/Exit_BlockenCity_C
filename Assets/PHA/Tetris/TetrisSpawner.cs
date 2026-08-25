using System;
using System.Collections;
using System.Collections.Generic;
using TetrisGame;
using UnityEngine;

public class TetrisSpawner : MonoBehaviour
{
    [SerializeField]
    private TetriminoBlock blockPrefab;

    [SerializeField]
    private TetriminoBlock nextBlock;
    [SerializeField]
    private TetriminoBlock currentBlock;

    [SerializeField]
    Vector3 spawnPosition;

    [SerializeField]
    Vector3 towerSpawnPosition;

    // 추가
    // 블록 스폰 시 호출 이벤트
    public event Action<TetriminoBlock> OnBlockSpawned;
    public event Action<TetriminoBlock> OnBlockSwapped;

    private void Awake()
    {
        nextBlock = Instantiate(blockPrefab, spawnPosition, Quaternion.identity);
    }

    public void SetTowerSpawnPosition(Vector3 pos)
    {
        towerSpawnPosition = pos;
    }

    public void SpawnBlock()
    {
        currentBlock = nextBlock;

        TetriminoBlock newBlock = currentBlock.GetComponent<TetriminoBlock>();

        if (newBlock != null)
        {
            // 기본 스폰 위치
            Vector3 spawnPos = towerSpawnPosition;

            // I 모양이면 한 칸 더 높게 스폰
            if (newBlock.shapeType == BlockShapes.I) 
            {
                spawnPos += Vector3.up;
            }

            newBlock.transform.position = spawnPos;
            newBlock.SetIsSelet(true);
        }

        // 스폰 위치랑 타워 위치 다르니까 현재 블럭 타워 위치로 이동시키기
        nextBlock = Instantiate(blockPrefab, spawnPosition, Quaternion.identity);

        // 추가
        // 스폰 블록 처리 이벤트 호출
        OnBlockSpawned?.Invoke(currentBlock);
    }

    public TetriminoBlock GetTetriminoBlock()
    {
        return currentBlock;
    }

    public bool TrySwapWithNext(Vector3 targetWorldPos)
    {
        if (currentBlock == null || nextBlock == null) return false;

        //추가
        // Bomb 등 특수 Piece는 Next와 교환 불가
        if (currentBlock.IsSpecialPiece)
        {
            return false;
        }

        // next를 현재 위치로 옮길 수 있는지 검사 (타워 경계/충돌 포함)
        Vector3 delta = targetWorldPos - nextBlock.transform.position;
        if (!nextBlock.CanMove(delta)) return false;  // 못 들어오면 스왑 안 함

        // 스왑
        var oldCurrent = currentBlock;

        oldCurrent.SetIsSelet(false);                  // 기존 current 비활성
        nextBlock.transform.position = targetWorldPos; // next를 현재 위치로
        nextBlock.SetIsSelet(true);                    // 조작 대상 지정
        currentBlock = nextBlock;                      // 현재 블럭 교체

        // 기존 current를 벤치(넥스트 자리)로
        oldCurrent.transform.position = spawnPosition;
        oldCurrent.SetIsSelet(false);
        nextBlock = oldCurrent;

        // [추가] 블록 교체(Hold) 이벤트 호출
        OnBlockSwapped?.Invoke(currentBlock);

        return true;
    }

    //추가
    //아이템 시스템에서 사용
    public bool TryReplaceCurrentBlock(
    TetriminoBlock replacementPrefab,
    out TetriminoBlock replacementBlock)
    {
        replacementBlock = null;


        if (currentBlock == null ||
            replacementPrefab == null)
        {
            return false;
        }


        // 현재 Piece 위치 유지
        Vector3 targetPosition =
            currentBlock.transform.position;


        // 기존 Piece
        TetriminoBlock oldCurrent =
            currentBlock;


        oldCurrent.SetIsSelet(false);


        // 특수 Piece 생성
        replacementBlock =
            Instantiate(
                replacementPrefab,
                targetPosition,
                Quaternion.identity
            );


        if (replacementBlock == null)
        {
            oldCurrent.SetIsSelet(true);

            return false;
        }


        // 새로운 current 등록
        currentBlock =
            replacementBlock;


        currentBlock.SetIsSelet(true);


        // 기존 떨어지던 일반 Piece 제거
        oldCurrent.gameObject.SetActive(false);

        Destroy(oldCurrent.gameObject);


        Debug.Log(
            $"[TetrisSpawner] Current Piece 교체 | " +
            $"{oldCurrent.name} -> {currentBlock.name}"
        );


        return true;
    }
}
