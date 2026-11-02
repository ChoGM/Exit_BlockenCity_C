using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum SnowInputMode
{
    QueueDelay,   // 기존 큐 방식
    DelayedLock   // 지연 후 실행 + 입력 잠금
}

public class InputBlocker : MonoBehaviour
{
    public bool blockRotation = false;
    public bool blockHardDrop = false;

    public bool isInputBlocked = false;

    // 델리게이트 참조 저장
    private Action leftArrowHandler;
    private Action rightArrowHandler;
    private Action upArrowHandler;
    private Action downArrowHandler;
    private Action keySHandler;
    private Action spaceHandler;
    private Action keyAHandler;
    private Action keyFHandler;
    private Action keyDHandler;
    private Action keyCHandler;

    public event Action OnLeftArrow;
    public event Action OnRightArrow;
    public event Action OnUpArrow;
    public event Action OnDownArrow;
    public event Action OnKeyS;
    public event Action OnSpace;
    public event Action OnKeyA;
    public event Action OnKeyF;
    public event Action OnKeyD;
    public event Action OnKeyC;

    // 입력 동작 큐 및 상태
    private Queue<Action> inputQueue = new();
    private bool isProcessingInput = false;

    // 딜레이 시간
    public float inputProcessDelay = 0.0f;

    // 현재 잔설 입력 모드
    private SnowInputMode snowInputMode =
        SnowInputMode.QueueDelay;

    // DelayedLock 처리 중인지
    private bool isDelayedInputLocked = false;

    // 각 입력 처리 코루틴
    private Coroutine queueCoroutine;
    private Coroutine delayedLockCoroutine;

    void Start()
    {
        HookInput();
    }

    public void SetInputProcessDelay(float delay)
    {
        inputProcessDelay = delay;
    }

    public void ResetAll()
    {
        isInputBlocked = false;
        blockHardDrop = false;

        inputQueue.Clear();

        StopAllCoroutines();

        isProcessingInput = false;
        isDelayedInputLocked = false;

        queueCoroutine = null;
        delayedLockCoroutine = null;

        // 기본 모드로 복구
        snowInputMode = SnowInputMode.QueueDelay;

        inputProcessDelay = 0f;
    }

    public void HookInput()
    {
        // 큐에 넣기만 함, 바로 실행 X
        leftArrowHandler = () =>
        {
            if (!isInputBlocked)
                EnqueueInput(() => OnLeftArrow?.Invoke());
        };
        rightArrowHandler = () =>
        {
            if (!isInputBlocked)
                EnqueueInput(() => OnRightArrow?.Invoke());
        };
        upArrowHandler = () =>
        {
            if (!isInputBlocked)
                EnqueueInput(() => OnUpArrow?.Invoke());
        };
        downArrowHandler = () =>
        {
            if (!isInputBlocked)
                EnqueueInput(() => OnDownArrow?.Invoke());
        };

        keySHandler = () =>
        {
            if (!isInputBlocked && !blockRotation)
                EnqueueInput(() => OnKeyS?.Invoke());
        };

        spaceHandler = () =>
        {
            if (!isInputBlocked && !blockHardDrop)
                EnqueueInput(() => OnSpace?.Invoke());
        };

        keyAHandler = () =>
        {
            if (!isInputBlocked && !blockRotation)
                EnqueueInput(() => OnKeyA?.Invoke());
        };
        keyFHandler = () =>
        {
            if (!isInputBlocked)
                EnqueueInput(() => OnKeyF?.Invoke());
        };
        keyDHandler = () =>
        {
            if (!isInputBlocked && !blockRotation)
                EnqueueInput(() => OnKeyD?.Invoke());
        };

        keyCHandler = () =>
        {
            EnqueueInput(() => OnKeyC?.Invoke());
        };

        // 기존 InputManager 이벤트 연결
        InputManager.Instance.OnLeftArrow += leftArrowHandler;
        InputManager.Instance.OnRightArrow += rightArrowHandler;
        InputManager.Instance.OnUpArrow += upArrowHandler;
        InputManager.Instance.OnDownArrow += downArrowHandler;
        
        InputManager.Instance.OnSpace += spaceHandler;

        InputManager.Instance.OnKeyS += keySHandler;
        InputManager.Instance.OnKeyA += keyAHandler;
        InputManager.Instance.OnKeyD+= keyDHandler;
        InputManager.Instance.OnKeyF += keyFHandler;
        InputManager.Instance.OnKeyC += keyCHandler;
    }

    public void UnhookInput()
    {
        InputManager.Instance.OnLeftArrow -= leftArrowHandler;
        InputManager.Instance.OnRightArrow -= rightArrowHandler;
        InputManager.Instance.OnUpArrow -= upArrowHandler;
        InputManager.Instance.OnDownArrow -= downArrowHandler;
        
        InputManager.Instance.OnSpace -= spaceHandler;
        InputManager.Instance.OnKeyS -= keySHandler;
        InputManager.Instance.OnKeyA -= keyAHandler;
        InputManager.Instance.OnKeyF -= keyFHandler;
        InputManager.Instance.OnKeyD -= keyDHandler;
        InputManager.Instance.OnKeyC -= keyCHandler;
    }

    private void EnqueueInput(Action inputAction)
    {
        // 신규 방식: 입력 지연 + 잠금
        if (snowInputMode == SnowInputMode.DelayedLock)
        {
            // 이미 처리 중이라면 추가 입력 무시
            if (isDelayedInputLocked)
                return;

            // 첫 입력을 받는 순간 잠금
            isDelayedInputLocked = true;

            // 일정 시간 뒤 해당 입력 실행
            delayedLockCoroutine = StartCoroutine(
                ProcessDelayedInput(inputAction)
            );

            return;
        }

        // 기존 방식: QueueDelay
        inputQueue.Enqueue(inputAction);

        if (!isProcessingInput)
        {
            queueCoroutine = StartCoroutine(
                ProcessInputQueue()
            );
        }
    }

    private IEnumerator ProcessInputQueue()
    {
        isProcessingInput = true;

        while (inputQueue.Count > 0)
        {
            var inputAction = inputQueue.Dequeue();

            inputAction?.Invoke();

            yield return new WaitForSeconds(
                inputProcessDelay
            );
        }

        isProcessingInput = false;

        // 추가
        queueCoroutine = null;
    }

    public void SetSnowInputMode(
    SnowInputMode mode,
    float delay)
    {
        // 이전 입력 처리 중지
        if (queueCoroutine != null)
        {
            StopCoroutine(queueCoroutine);
            queueCoroutine = null;
        }

        if (delayedLockCoroutine != null)
        {
            StopCoroutine(delayedLockCoroutine);
            delayedLockCoroutine = null;
        }

        // 기존 대기 입력 제거
        inputQueue.Clear();

        // 처리 상태 초기화
        isProcessingInput = false;
        isDelayedInputLocked = false;

        // 새로운 모드 설정
        snowInputMode = mode;
        inputProcessDelay = Mathf.Max(0f, delay);

        Debug.Log(
            $"[SnowInput] Mode: {snowInputMode}, " +
            $"Delay: {inputProcessDelay}"
        );
    }

    private IEnumerator ProcessDelayedInput(
    Action inputAction)
    {
        // 지정한 시간만큼 입력 실행 대기
        yield return new WaitForSeconds(
            inputProcessDelay
        );

        try
        {
            // 대기 종료 시점에 입력 차단 상태 확인
            if (!isInputBlocked)
            {
                inputAction?.Invoke();
            }
        }
        finally
        {
            // 실행 성공 여부와 관계없이 잠금 해제
            isDelayedInputLocked = false;
            delayedLockCoroutine = null;
        }
    }
}
