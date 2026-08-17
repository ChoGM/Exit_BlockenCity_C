using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // [추가] 씬 관리를 위해 필요

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float startGameTime = 150f;   // Inspector에서 초 단위 설정 (0인지 확인 필요!)
    public float gameTime;

    public bool isGameEnded { get; private set; } = false;
    public bool isPaused { get; private set; } = false;

    [HideInInspector] public ScoreUIBinder scoreManager;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    // [추가] 오브젝트가 활성화될 때 씬 로드 이벤트 구독
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // [추가] 오브젝트가 비활성화될 때 이벤트 구독 해제 (메모리 누수 방지)
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // [추가] 씬이 이동하여 로드가 완료될 때마다 자동 호출되는 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetGame(); // 씬이 바뀔 때마다 시간, isGameEnded, isPaused를 완벽히 리셋
        Debug.Log($"[{scene.name}] 씬 로드 완료: 게임 상태 및 시간 리셋 완료");
    }

    void Start()
    {
        ResetGame();
    }

    void Update()
    {
        // 일시정지 상태이거나 게임이 종료되었으면 타이머 멈춤
        if (isGameEnded || isPaused) return;

        if (gameTime > 0f)
        {
            gameTime -= Time.deltaTime;

            if (gameTime <= 0f)
            {
                gameTime = 0f;
                TimeOver();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;

        if (TetrisManager.Instance != null)
        {
            TetrisManager.Instance.SetPause(true);
        }

        Debug.Log("[GameManager] 게임 일시정지");
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (TetrisManager.Instance != null)
        {
            TetrisManager.Instance.SetPause(false);
        }

        Debug.Log("[GameManager] 게임 재개");
    }

    public bool IsGamePaused()
    {
        return isPaused;
    }
    public void StopGame()
    {
        isGameEnded = true;
    }

    public void TimeOver()
    {
        if (isGameEnded) return;
        isGameEnded = true;

        Debug.Log("일차 종료!");

        if (TetrisManager.Instance != null)
        {
            TetrisManager.Instance.GameClear();
        }

        if (scoreManager != null)
        {
            scoreManager.ToggleScoreUI(true, isGameOver: false);
        }
    }

    public void ResetGame()
    {
        gameTime = startGameTime;
        isGameEnded = false;
        isPaused = false;
    }
}