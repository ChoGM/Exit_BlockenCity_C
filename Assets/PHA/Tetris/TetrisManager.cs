using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TetrisGame;
using System.Xml.Serialization;

public class TetrisManager : MonoBehaviour
{
    public static TetrisManager Instance;

    public Vector3Int tetrisTowerSize = new Vector3Int(4, 8, 4);

    public float fallInterval;

    public TetrisTower tower;

    public TetrisSpawner spawner;

    public TetrisController controller;

    private int[] typeBlockCount = new int[(int)BlockType.None];

    public ScoreUIBinder scoreUIBinder;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

    }

    // Start is called before the first frame update
    void Start()
    {
        Vector3 spawnPos = tower.GetSpawnPosition();
        spawner.SetTowerSpawnPosition(spawnPos);
        SpawnNextBlock();

        for (int i = 0; i < typeBlockCount.Length; i++)
        { typeBlockCount[i] = 0; }

    }


    //타입별 블럭 갯수 카운트
    public void IncreaseTypeBlockCount(BlockType type)
    {
        typeBlockCount[(int)type]++;
    }

    public void DecreaseTypeBlockCount(BlockType type)
    {
        typeBlockCount[(int)type]--;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.UpdateQuestProgress(type);
        }

    }

    // 특정 블럭 타입의 갯수 반환
    public int GetBlockCount(BlockType type)
    {
        return typeBlockCount[(int)type];
    }

    // 전체 카운트 배열을 복사해서 반환
    public int[] GetAllBlockCounts()
    {
        return (int[])typeBlockCount.Clone();
    }



    public void SpawnNextBlock()
    {
        spawner.SpawnBlock();
        controller.SetCurrentBlock(spawner.GetTetriminoBlock());
    }

    public void CheckTower()
    {
        tower.CheckAndDeleteFullLines();       //타워 검사
    }

    public void GameOver()
    {
        Debug.Log("[Tetris] GAME OVER");
        // TODO: 게임오버 UI, 입력 차단, 재시작 처리 등

        StageManager.Instance.OverStage();

        scoreUIBinder.ToggleScoreUI(true, true);

    }

}
