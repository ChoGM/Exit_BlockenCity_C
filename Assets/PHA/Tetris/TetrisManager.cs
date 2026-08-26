using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TetrisGame;

public class TetrisManager : MonoBehaviour
{
    [Header("Stage Shape Settings")]
    public List<StageShapeSetting> stageShapeSettings = new();

    public static TetrisManager Instance;

    [Header("Setting Presets")]
    public List<StageSettingPreset> settingPresets = new List<StageSettingPreset>();

    [Header("Stage Settings")]
    public List<StageSetting> stageSettings = new List<StageSetting>();

    public StageSettingPreset CurrentSettingPreset
    {
        get;
        private set;
    }

    public StageSetting CurrentStageSetting
    {
        get;
        private set;
    }

    [Header("Tower")]

    public Vector3Int tetrisTowerSize = new Vector3Int(4, 8, 4);

    public float fallInterval;
    public TetrisTower tower;
    public TetrisSpawner spawner;
    public TetrisController controller;

    [Header("Tower Layout")]
    public Transform towerLayout;

    private int[] typeBlockCount = new int[(int)BlockType.None];
    public ScoreUIBinder scoreUIBinder;

    public bool isGameEnded
    {
        get;
        private set;
    } = false;

    public bool isPaused
    {
        get;
        private set;
    } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            ApplyStageSetting();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        isGameEnded = false;
        isPaused = false;

        ApplyStageSetting();

        tower.Initialize();

        Vector3 spawnPos = tower.GetSpawnPosition();

        spawner.SetTowerSpawnPosition(spawnPos);

        SpawnNextBlock();

        for (int i = 0; i < typeBlockCount.Length; i++)
        {
            typeBlockCount[i] = 0;
        }
    }

    private void ApplyStageSetting()
    {
        if (Datamanager.Instance == null)
        {
            Debug.LogError("[TetrisManager] Datamanager.Instance가 없습니다.");
            return;
        }

        int currentStage = Datamanager.Instance.saveData.progress.currentStage;

        Debug.Log($"[TetrisManager] 현재 Stage : {currentStage}");

        StageSetting stageSetting = stageSettings.Find(x => x.stage == currentStage);


        if (stageSetting == null)
        {
            Debug.LogError($"[TetrisManager] " + $"Stage {currentStage}의 설정이 없습니다.");
            return;
        }

        CurrentStageSetting = stageSetting;

        StageSettingPreset preset = settingPresets.Find(x => x.presetID == stageSetting.presetID);


        if (preset == null)
        {
            Debug.LogError( $"[TetrisManager] " + $"Preset ID {stageSetting.presetID}를 찾을 수 없습니다.");
            return;
        }

        CurrentSettingPreset = preset;

        tetrisTowerSize = preset.towerSize;


        Debug.Log($"[TetrisManager] " + $"Stage {currentStage} → " + $"Preset {preset.presetID}");

        Debug.Log($"[TetrisManager] " + $"Tower Size : {preset.towerSize}");

        if (towerLayout != null)
        {
            towerLayout.localPosition = preset.layoutPosition;
            towerLayout.localScale = preset.layoutScale;
        }


        Debug.Log($"[TetrisManager] " + $"Layout Position : " + $"{preset.layoutPosition}");

        Debug.Log($"[TetrisManager] " + $"Layout Scale : " + $"{preset.layoutScale}");

        Debug.Log($"[TetrisManager] " + $"Stage {currentStage} " + $"Preset {preset.presetID} 적용 완료");
    }

    public void SetPause(bool pause)
    {
        isPaused = pause;
    }

    public void GameClear()
    {
        if (isGameEnded)
            return;

        isGameEnded = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StopGame();
        }

        if (controller != null)
        {
            controller.enabled = false;
        }

        Debug.Log("[Tetris] GAME CLEAR");

        if (scoreUIBinder != null)
        {
            scoreUIBinder.ToggleScoreUI(
                true,
                isGameOver: false
            );
        }
    }

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

    public int GetBlockCount(BlockType type)
    {
        return typeBlockCount[(int)type];
    }

    public int[] GetAllBlockCounts()
    {
        return (int[])typeBlockCount.Clone();
    }


    public void SpawnNextBlock()
    {
        if (isGameEnded || isPaused)
            return;

        spawner.SpawnBlock();

        controller.SetCurrentBlock(spawner.GetTetriminoBlock());
    }

    public void CheckTower()
    {
        if (isGameEnded || isPaused)
            return;

        tower.CheckAndDeleteFullLines();
    }

    public void GameOver()
    {
        if (isGameEnded)
            return;

        isGameEnded = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StopGame();
        }

        Debug.Log("[Tetris] GAME OVER");

        if (controller != null)
        {
            controller.enabled = false;
        }

        if (StageManager.Instance != null)
        {
            StageManager.Instance.OverStage();
        }

        if (scoreUIBinder != null)
        {
            scoreUIBinder.ToggleScoreUI(
                true,
                isGameOver: true
            );
        }
    }

    public BlockShapes GetRandomStageShape()
    {
        int currentStage = Datamanager.Instance.saveData.progress.currentStage;

        foreach (var setting in stageShapeSettings)
        {
            if (setting.stage != currentStage)
                continue;

            if (setting.availableShapes == null || setting.availableShapes.Count == 0)
                break;

            int totalWeight = 0;

            foreach (var shape in setting.availableShapes)
            {
                if (shape.weight > 0)
                    totalWeight += shape.weight;
            }

            if (totalWeight <= 0)
            {
                Debug.LogWarning($"Stage {currentStage}의 Shape 가중치가 모두 0입니다.");
                break;
            }

            int randomValue = Random.Range(0, totalWeight);

            foreach (var shape in setting.availableShapes)
            {
                if (shape.weight <= 0)
                    continue;

                randomValue -= shape.weight;

                if (randomValue < 0)
                {
                    return shape.shape;
                }
            }
        }

        // 설정이 없을 경우 기본 7종
        BlockShapes[] defaultShapes =
        {
        BlockShapes.I,
        BlockShapes.O,
        BlockShapes.T,
        BlockShapes.L,
        BlockShapes.J,
        BlockShapes.S,
        BlockShapes.Z
    };

        return defaultShapes[Random.Range(0, defaultShapes.Length)];
    }
    private BlockShapes GetRandomShapeType()
    {
        return TetrisManager.Instance.GetRandomStageShape();
    }
}