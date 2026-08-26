using UnityEngine;

public abstract class SpecialBlockLockHandler : MonoBehaviour
{
    /// <summary>
    /// 일반 TetriminoBlock.BlockLock() 대신
    /// 특수 Piece가 Lock될 때 실행할 동작.
    /// </summary>
    public abstract void HandleSpecialLock(TetriminoBlock owner);
}