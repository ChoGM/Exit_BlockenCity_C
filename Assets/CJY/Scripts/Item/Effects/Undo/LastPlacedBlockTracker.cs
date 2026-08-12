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


    public TetriminoBlock LastPlacedBlock => lastPlacedBlock;

    public UndoTargetState CurrentState => currentState;

    public int BoardVersion => boardVersion;

    public int LastPlacementSequence => lastPlacementSequence;


    private void OnEnable()
    {
        TetriminoBlock.OnAnyBlockLocked += HandleBlockLocked;
    }


    private void OnDisable()
    {
        TetriminoBlock.OnAnyBlockLocked -= HandleBlockLocked;
    }


    /// <summary>
    /// 일반 TetriminoBlock이 정상적으로 Lock되었을 때 호출된다.
    /// 새로운 블록이 설치되면 Undo 대상도 새 블록으로 갱신된다.
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
    /// 현재 마지막 Piece를 Undo할 수 있는지 검사한다.
    /// 실제 블록 삭제는 하지 않는다.
    /// </summary>
    public bool CanUndo()
    {
        // 기록 자체가 없음
        if (currentState == UndoTargetState.NoTarget)
            return false;

        // 이미 Undo를 사용함
        if (currentState == UndoTargetState.AlreadyUsed)
            return false;

        // 마지막 Piece 이후 Board 구조가 바뀜
        if (currentState == UndoTargetState.BoardChanged)
            return false;

        // Unity Object가 이미 Destroy됨
        if (lastPlacedBlock == null)
        {
            currentState = UndoTargetState.TargetDestroyed;
            return false;
        }

        // 기록 당시 Board와 현재 Board가 다름
        if (lastPieceBoardVersion != boardVersion)
        {
            currentState = UndoTargetState.BoardChanged;
            return false;
        }

        return currentState == UndoTargetState.Available;
    }


    /// <summary>
    /// Undo Effect가 실제 삭제할 Piece를 요청할 때 사용한다.
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
    /// 폭탄, 최하단 삭제, 자동 라인 삭제, 재해 등으로
    /// 고정된 Board 구조 자체가 바뀌었음을 알린다.
    ///
    /// 다음 단계부터 해당 시스템들이 이 메서드를 호출하게 된다.
    /// </summary>
    public void NotifyBoardStructureChanged()
    {
        boardVersion++;

        if (currentState == UndoTargetState.Available)
        {
            currentState = UndoTargetState.BoardChanged;

            Debug.Log(
                $"[UndoTracker] Board 변경으로 기존 Undo 대상 무효화 | " +
                $"BoardVersion: {boardVersion}"
            );
        }
    }


    /// <summary>
    /// Undo가 실제로 성공했을 때 호출한다.
    /// 같은 대상에 Undo를 연속 사용할 수 없게 한다.
    /// </summary>
    public void NotifyUndoSucceeded()
    {
        boardVersion++;

        currentState = UndoTargetState.AlreadyUsed;
        lastPlacedBlock = null;

        Debug.Log(
            $"[UndoTracker] Undo 사용 완료 | " +
            $"Sequence: {lastPlacementSequence} | " +
            $"BoardVersion: {boardVersion}"
        );
    }
}