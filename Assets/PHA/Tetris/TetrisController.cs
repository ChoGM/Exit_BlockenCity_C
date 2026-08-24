using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputDir
{
    Left,
    Right,
    Forward,
    Back
}

public class TetrisController : MonoBehaviour
{
    // 현재 조작중인 블럭
    public TetriminoBlock currentBlock;

    private InputManager input;

    // 추가 방해물 관련
    // 입력 제어 클래스
    public InputBlocker inputBlocker;

    // 추가 카메라 관련
    // 현재 카메라 논리 방향
    private CameraDir currentCameraDir = CameraDir.Front;

    // 입력 처리 활성화
    private void OnEnable()
    {
        input = InputManager.Instance ?? FindObjectOfType<InputManager>();
        if (input == null)
        {
            Debug.LogError("[TetrisController] InputManager가 씬에 없습니다.");
            enabled = false;
            return;
        }

        inputBlocker.OnLeftArrow += MoveLeft;
        inputBlocker.OnRightArrow += MoveRight;
        inputBlocker.OnUpArrow += MoveForward;
        inputBlocker.OnDownArrow += MoveBack;

        inputBlocker.OnKeyF += SoftDrop;
        inputBlocker.OnSpace += HardDrop;

        inputBlocker.OnKeyA += XRotate;
        inputBlocker.OnKeyS += YRotate;
        inputBlocker.OnKeyD += ZRotate;

        inputBlocker.OnKeyC += BlockChange;
    }


    // 입력 처리 비활성화
    private void OnDisable()
    {
        inputBlocker.UnhookInput();
    }

    //추가
    //바람에 따라 밀리는 블록
    public void MoveBlockByWind(Vector3 dir)
    {
        if (currentBlock == null) return;
        if (currentBlock.CanMove(dir))
            currentBlock.Move(dir);
    }

    private void MoveLeft() => Move(InputDir.Left);
    private void MoveRight() => Move(InputDir.Right);
    private void MoveForward() => Move(InputDir.Back);
    private void MoveBack() => Move(InputDir.Forward);

    private void SoftDrop()
    {
        if (IsGameplayPaused())
            return;


        if (currentBlock == null)
            return;


        Vector3 dir =
            Vector3.down;


        if (currentBlock.CanMove(dir))
        {
            currentBlock.Move(dir);
        }
    }

    private void HardDrop()
    {
        if (IsGameplayPaused())
            return;


        if (currentBlock == null)
            return;


        if (TetrisManager.Instance == null)
            return;


        int guard =
            TetrisManager.Instance
                .tetrisTowerSize.y;


        while (guard-- > 0 &&
               currentBlock.CanMove(
                   Vector3.down))
        {
            currentBlock.Move(
                Vector3.down
            );
        }


        currentBlock.BlockLock();
    }

    private void XRotate()
    {
        if (IsGameplayPaused())
            return;

        //추가
        if (currentBlock == null)
            return;

        // Bomb 같은 특수 블록은 회전 금지
        if (currentBlock.IsSpecialPiece)
            return;

        currentBlock.RotateX();
    }

    private void YRotate()
    {
        if (IsGameplayPaused())
            return;

        //추가
        if (currentBlock == null)
            return;

        if (currentBlock.IsSpecialPiece)
            return;

        currentBlock.RotateY();
    }


    private void ZRotate()
    {
        if (IsGameplayPaused())
            return;

        //추가
        if (currentBlock == null)
            return;

        if (currentBlock.IsSpecialPiece)
            return;

        currentBlock.RotateZ();
    }

    private void BlockChange()
    {
        if (IsGameplayPaused())
            return;

        //추가
        if (currentBlock == null)
            return;


        // Bomb 상태에서는 교체 불가
        if (currentBlock.IsSpecialPiece)
            return;


        var spawner =
            FindObjectOfType<TetrisSpawner>();


        if (spawner == null)
            return;


        Vector3 target =
            currentBlock.transform.position;


        if (inputBlocker != null)
        {
            inputBlocker.blockRotation = false;
        }


        bool ok =
            spawner.TrySwapWithNext(
                target
            );


        if (ok)
        {
            currentBlock =
                spawner.GetTetriminoBlock();
        }
    }

    private Vector3 GetWorldDir(InputDir input)
    {
        switch (currentCameraDir)
        {
            case CameraDir.Front:
                return input switch
                {
                    InputDir.Left => Vector3.left,
                    InputDir.Right => Vector3.right,
                    InputDir.Forward => Vector3.forward,
                    InputDir.Back => Vector3.back,
                    _ => Vector3.zero
                };

            case CameraDir.Left:
                return input switch
                {
                    InputDir.Left => Vector3.forward,
                    InputDir.Right => Vector3.back,
                    InputDir.Forward => Vector3.right,
                    InputDir.Back => Vector3.left,
                    _ => Vector3.zero
                };

            case CameraDir.Back:
                return input switch
                {
                    InputDir.Left => Vector3.right,
                    InputDir.Right => Vector3.left,
                    InputDir.Forward => Vector3.back,
                    InputDir.Back => Vector3.forward,
                    _ => Vector3.zero
                };

            case CameraDir.Right:
                return input switch
                {
                    InputDir.Left => Vector3.back,
                    InputDir.Right => Vector3.forward,
                    InputDir.Forward => Vector3.left,
                    InputDir.Back => Vector3.right,
                    _ => Vector3.zero
                };
        }

        return Vector3.zero;
    }


    public void SetCurrentBlock(TetriminoBlock block)
    {
        currentBlock = block;
    }

    // 카메라 관련 추가
    public void SetCameraDir(CameraDir dir)
    {
        currentCameraDir = dir;
    }

    private void Move(InputDir input)
    {
        if (IsGameplayPaused())
            return;


        if (currentBlock == null)
            return;


        Vector3 dir =
            GetWorldDir(input);


        if (currentBlock.CanMove(dir))
        {
            currentBlock.Move(dir);
        }
    }

    private bool IsGameplayPaused()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.IsGamePaused() ||
                GameManager.Instance.isGameEnded)
            {
                return true;
            }
        }


        if (TetrisManager.Instance != null)
        {
            if (TetrisManager.Instance.isPaused ||
                TetrisManager.Instance.isGameEnded)
            {
                return true;
            }
        }


        return false;
    }
}
