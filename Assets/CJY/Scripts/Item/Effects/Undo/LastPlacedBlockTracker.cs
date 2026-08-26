using UnityEngine;

public enum UndoTargetState
{
    NoTarget,
    Available,
    AlreadyUsed,
    BoardChanged,
    TargetDestroyed
}

public class LastPlacedBlockTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private TetrisTower tower;


    [Header("Runtime Debug")]
    [SerializeField]
    private TetriminoBlock lastPlacedBlock;

    [SerializeField]
    private UndoTargetState currentState = UndoTargetState.NoTarget;

    [SerializeField]
    private int placementSequence = 0;

    [SerializeField]
    private int lastPlacementSequence = -1;

    [SerializeField]
    private int boardVersion = 0;

    [SerializeField]
    private int lastPieceBoardVersion = -1;


    private bool towerEventSubscribed = false;


    public TetriminoBlock LastPlacedBlock => lastPlacedBlock;

    public UndoTargetState CurrentState => currentState;

    public int BoardVersion => boardVersion;

    public int LastPlacementSequence => lastPlacementSequence;


    private void OnEnable()
    {
        // 일반 Piece Lock 추적
        TetriminoBlock.OnAnyBlockLocked += HandleBlockLocked;

        TrySubscribeTowerEvent();
    }


    private void Start()
    {
        // OnEnable 시점에 TetrisManager가 준비되지 않았을 경우 대비
        TrySubscribeTowerEvent();
    }


    private void OnDisable()
    {
        TetriminoBlock.OnAnyBlockLocked -= HandleBlockLocked;

        UnsubscribeTowerEvent();
    }


    /// <summary>
    /// TetrisTower 이벤트 연결.
    /// Inspector에서 직접 Tower를 넣어도 되고,
    /// 비어 있으면 자동으로 찾는다.
    /// </summary>
    private void TrySubscribeTowerEvent()
    {
        if (towerEventSubscribed)
            return;


        if (tower == null &&
            TetrisManager.Instance != null)
        {
            tower = TetrisManager.Instance.tower;
        }


        if (tower == null)
        {
            tower = FindObjectOfType<TetrisTower>();
        }


        if (tower == null)
        {
            Debug.LogWarning(
                "[UndoTracker] TetrisTower를 찾지 못했습니다."
            );

            return;
        }


        tower.OnTowerStructureChanged += HandleTowerStructureChanged;

        towerEventSubscribed = true;
    }


    private void UnsubscribeTowerEvent()
    {
        if (!towerEventSubscribed)
            return;

        if (tower != null)
        {
            tower.OnTowerStructureChanged -= HandleTowerStructureChanged;
        }

        towerEventSubscribed = false;
    }


    /// <summary>
    /// 일반 TetriminoBlock이 Lock되었을 때
    /// 새로운 Undo 대상으로 기록.
    /// </summary>
    private void HandleBlockLocked(TetriminoBlock block)
    {
        if (block == null)
            return;

        placementSequence++;
        boardVersion++;

        lastPlacedBlock = block;

        lastPlacementSequence = placementSequence;
        lastPieceBoardVersion = boardVersion;

        currentState = UndoTargetState.Available;

        Debug.Log(
            $"[UndoTracker] 새 Undo 대상 기록 | " +
            $"Piece: {block.name} | " +
            $"Sequence: {lastPlacementSequence} | " +
            $"BoardVersion: {boardVersion}"
        );
    }


    /// <summary>
    /// 자동 라인 삭제 등으로 Tower 자체가 변경됨.
    /// </summary>
    private void HandleTowerStructureChanged()
    {
        NotifyBoardStructureChanged();
    }


    /// <summary>
    /// 현재 마지막 Piece를 Undo할 수 있는지 검사.
    /// </summary>
    public bool CanUndo()
    {
        if (currentState == UndoTargetState.NoTarget)
            return false;


        if (currentState == UndoTargetState.AlreadyUsed)
            return false;


        if (currentState == UndoTargetState.BoardChanged)
            return false;


        if (lastPlacedBlock == null)
        {
            currentState = UndoTargetState.TargetDestroyed;

            return false;
        }


        if (lastPieceBoardVersion != boardVersion)
        {
            currentState = UndoTargetState.BoardChanged;

            return false;
        }


        return currentState == UndoTargetState.Available;
    }


    /// <summary>
    /// Undo Effect에서 실제 삭제 대상을 요청.
    /// </summary>
    public bool TryGetUndoTarget(
        out TetriminoBlock target,
        out UndoTargetState state)
    {
        if (!CanUndo())
        {
            target = null;
            state = currentState;

            return false;
        }


        target = lastPlacedBlock;
        state = currentState;

        return true;
    }


    /// <summary>
    /// 폭탄, 최하단 삭제, 자동 라인 삭제,
    /// 재해 등으로 Board 구조가 변경됐을 때 호출.
    /// </summary>
    public void NotifyBoardStructureChanged()
    {
        boardVersion++;


        if (currentState == UndoTargetState.Available)
        {
            currentState = UndoTargetState.BoardChanged;

            Debug.Log(
                $"[UndoTracker] Board 변경으로 Undo 대상 무효화 | " +
                $"BoardVersion: {boardVersion}"
            );
        }
    }


    /// <summary>
    /// Undo가 실제로 성공한 뒤 호출.
    /// 같은 상태에서 연속 Undo를 막는다.
    /// </summary>
    public void NotifyUndoSucceeded()
    {
        boardVersion++;

        currentState = UndoTargetState.AlreadyUsed;

        lastPlacedBlock = null;


        Debug.Log(
            $"[UndoTracker] Undo 성공 | " +
            $"Sequence: {lastPlacementSequence} | " +
            $"BoardVersion: {boardVersion}"
        );
    }
}