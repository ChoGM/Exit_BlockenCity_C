using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��� ���ع� ���� �� �� ȿ�� ������ ���
/// </summary>
public class ObstacleManager : MonoBehaviour
{
    [SerializeField] private ObstacleEffects effects;
    [SerializeField] private EffectVisualPlayer visualPlayer;
    [SerializeField] private InputBlocker inputBlocker;
    [SerializeField] private TetrisController tetrisController;
    [SerializeField] private TetrisSpawner tetrisspawner;

    [Header("�׽�Ʈ ���")]
    [SerializeField] private bool useInspectorObstacle = false;
    [SerializeField] private MonthlyObstacleTable.Entry selectedObstacleEntry; // �׽�Ʈ ����

    private List<ObstacleRuntime> obstacles = new();
    [SerializeField] private bool obstacleSystemEnabled = true;

    private List<Coroutine> runningCoroutines = new();
    private List<GameObject> spawnedObjects = new();
    private ObstacleEffects effectsInstance;

    void Start()
    {
        Initialize();
        EvaluateCurrentObstacle();

        // ��� ������ �̺�Ʈ ����
        tetrisspawner.OnBlockSpawned += HandleBlockSpawned;
        // ��� ���� �̺�Ʈ ����
        TetriminoBlock.OnAnyBlockLocked += HandleBlockLocked;
    }

    private void OnDestroy()
    {
        tetrisspawner.OnBlockSpawned -= HandleBlockSpawned;
        TetriminoBlock.OnAnyBlockLocked -= HandleBlockLocked;
    }

    private void Initialize()
    {
        if (effects == null)
        {
            Debug.LogError("ObstacleEffects�� Inspector���� ������� �ʾҽ��ϴ�!");
            return;
        }

        var factory = new ObstacleFactory(effects);
        obstacles = factory.CreateAllObstacles();
        effectsInstance = effects;
    }

    /// <summary>
    /// ���� Ȱ��ȭ�� ���ع� ��Ʈ�� ��������
    /// </summary>
    private MonthlyObstacleTable.Entry GetActiveObstacleEntry()
    {
        if (useInspectorObstacle && selectedObstacleEntry != null)
        {
            //Debug.Log($"[ObstacleManager] �׽�Ʈ ��� ��� �� �� {selectedObstacleEntry.type}");
            return selectedObstacleEntry;
        }

        var entry = GameObstacleSystem.Instance.GetSelectedObstacle();
        //Debug.Log($"[ObstacleManager] �κ� Ȯ�� ���ع� ��� �� {entry?.type.ToString() ?? "����"}");
        return entry;
    }

    private void HandleBlockSpawned(TetriminoBlock block)
    {
        Debug.Log($"[HandleBlockSpawned] ȣ��, block={block}");
        if (!obstacleSystemEnabled || block == null) return;

        var activeObstacle = GetActiveObstacleEntry();
        if (activeObstacle == null) return;

        var state = new ObstacleGameState(
            activeObstacle.type,
            visualPlayer,
            inputBlocker,
            tetrisController
        )
        {
            SpawnedBlock = block,
            StartManagedCoroutine = routine =>
            {
                var co = StartCoroutine(routine);
                runningCoroutines.Add(co);
                return co;
            },
            RegisterObject = o => spawnedObjects.Add(o)
        };

        foreach (var obstacle in obstacles)
        {
            if (obstacle.AreConditionsMet(state))
                obstacle.ExecuteSpawnEffects(state);
        }
    }

    private void HandleBlockLocked(TetriminoBlock block)
    {
        if (!obstacleSystemEnabled) return;

        var activeObstacle = GetActiveObstacleEntry();
        if (activeObstacle == null) return;

        var state = new ObstacleGameState(
            activeObstacle.type,
            visualPlayer,
            inputBlocker,
            tetrisController
        )
        {
            LockedBlock = block,
            StartManagedCoroutine = routine =>
            {
                var co = StartCoroutine(routine);
                runningCoroutines.Add(co);
                return co;
            },
            RegisterObject = o => spawnedObjects.Add(o)
        };

        foreach (var obstacle in obstacles)
        {
            if (obstacle.AreConditionsMet(state))
                obstacle.ExecuteLockEffects(state);
        }
    }

    private void EvaluateCurrentObstacle()
    {
        if (!obstacleSystemEnabled) return;

        var activeObstacle = GetActiveObstacleEntry();
        if (activeObstacle == null) return;

        var state = new ObstacleGameState(
            activeObstacle.type,
            visualPlayer,
            inputBlocker,
            tetrisController
        )
        {
            StartManagedCoroutine = routine =>
            {
                var co = StartCoroutine(routine);
                runningCoroutines.Add(co);
                return co;
            },
            RegisterObject = o => spawnedObjects.Add(o)
        };

        foreach (var obstacle in obstacles)
        {
            if (obstacle.AreConditionsMet(state))
                obstacle.ExecuteEffects(state);
        }

        Debug.Log($"[ObstacleManager] {activeObstacle.type} ���ع� �� ����");
    }

    private void StopAllObstacleEffects()
    {
        foreach (var c in runningCoroutines)
            if (c != null) StopCoroutine(c);
        runningCoroutines.Clear();

        foreach (var obj in spawnedObjects)
            if (obj != null) Destroy(obj);
        spawnedObjects.Clear();

        inputBlocker.ResetAll();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            obstacleSystemEnabled = !obstacleSystemEnabled;
            if (!obstacleSystemEnabled)
            {
                Debug.Log("���ع� ����");
                StopAllObstacleEffects();
            }
            else
            {
                EvaluateCurrentObstacle();
            }
        }
    }
}
