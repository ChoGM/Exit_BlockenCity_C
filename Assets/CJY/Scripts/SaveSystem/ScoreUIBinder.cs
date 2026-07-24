using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 이동을 위해 추가
using TMPro;

public class ScoreUIBinder : MonoBehaviour
{
    [Header("Player")]
    public TMP_Text playerNameText;
    public TMP_Text playerNameText2;

    [Header("Date")]
    public TMP_Text dateText;

    [Header("Money")]
    public TMP_Text totalMoneyText;
    public TMP_Text salaryText;

    [Header("Relationship - Danwol")]
    public Slider danwolSlider;
    public TMP_Text danwolStateText;

    [Header("Relationship - Yaseo")]
    public Slider yaseoSlider;
    public TMP_Text yaseoStateText;

    [Header("Relationship - Macheon")]
    public Slider macheonSlider;
    public TMP_Text macheonStateText;

    [Header("Relationship - Hongryeon")]
    public Slider hongryeonSlider;
    public TMP_Text hongryeonStateText;

    [Header("Relationship - JeonSangYeon")]
    public Slider jeonsangyeonSlider;
    public TMP_Text jeonsangyeonStateText;

    [Header("UI Window Object")]
    public GameObject clearWindowObject;
    public GameObject gameOverWindowObject;
    public GameObject scoreWindowObject;   // 최상위 부모 (전체 UI를 감싸는 캔버스나 패널)

    [Header("Scene Names")]
    public string storySceneName = "StoryScene"; // 게임 클리어 시 이동할 씬 이름
    public string lobbySceneName = "Lobby";      // 게임 오버 시 이동할 씬 이름

    // 현재 결과 창의 상태를 저장할 변수
    private bool isGameOverState;

    private void Awake()
    {
        // 씬이 시작될 때 현재 살아있는 GameManager.Instance에 자신을 등록
        if (GameManager.Instance != null)
        {
            GameManager.Instance.scoreManager = this;
        }
    }

    // 기존 함수에 bool 매개변수 추가 및 상태 저장
    public void ToggleScoreUI(bool isActive, bool isGameOver = false)
    {
        // 1. 현재 상태 저장
        isGameOverState = isGameOver;

        if (scoreWindowObject != null)
        {
            scoreWindowObject.SetActive(isActive);
        }

        if (isActive)
        {
            if (clearWindowObject != null)
            {
                clearWindowObject.SetActive(!isGameOver); // 게임오버가 아닐 때(클리어)만 켬
            }

            if (gameOverWindowObject != null)
            {
                gameOverWindowObject.SetActive(isGameOver);   // 게임오버일 때만 켬
            }

            // 3. 데이터를 최신 상태에 맞춰 갱신합니다.
            Refresh(isGameOver);
        }
    }

    // ===== 추가된 부분: 다음/확인 버튼에서 호출할 함수 =====
    public void OnNextButtonClick()
    {
        if (isGameOverState)
        {
            // 게임 오버 상태 -> 로비 씬으로 이동
            SceneManager.LoadScene(lobbySceneName);
        }
        else
        {
            // 게임 클리어 상태 -> 스토리 씬으로 이동
            SceneManager.LoadScene(storySceneName);
        }
    }

    // Refresh 함수도 상태를 받도록 수정
    public void Refresh(bool isGameOver)
    {
        var saveData = Datamanager.Instance.saveData;
        var stageData = StageManager.Instance.stageData;

        // 날짜 및 플레이어 이름 표시 (공통)
        dateText.text = $"{saveData.progress.currentStage:00}.00.";
        playerNameText.text = saveData.player.playerName;
        playerNameText2.text = saveData.player.playerName;

        // ===== 게임 오버 상태일 때 =====
        if (isGameOver)
        {
            // 월급 0원 및 빨간색 처리
            salaryText.text = "0";
            salaryText.color = new Color32(214, 47, 45, 255);

            // 총 자산은 이번 판에 번 돈 없이 기존 자산만 표시
            totalMoneyText.text = saveData.player.totalMoney.ToString();
        }
        // ===== 게임 클리어(성공) 상태일 때 =====
        else
        {
            int baseSalary = GetBaseSalary(saveData.progress.currentStage);
            int totalSalary = baseSalary + stageData.earnedMoney;

            // 월급 및 총 자산 표시
            salaryText.text = $"{baseSalary} + {stageData.earnedMoney}";

            int totalMoney = saveData.player.totalMoney + totalSalary;
            totalMoneyText.text = totalMoney.ToString();
        }

        // ===== 세력 =====
        RefreshRelationship(
            danwolSlider,
            danwolStateText,
            saveData.relationship.danwol + stageData.danwolDelta);

        RefreshRelationship(
            yaseoSlider,
            yaseoStateText,
            saveData.relationship.yaseo + stageData.yaseoDelta);

        RefreshRelationship(
            macheonSlider,
            macheonStateText,
            saveData.relationship.macheon + stageData.macheonDelta);

        RefreshRelationship(
            hongryeonSlider,
            hongryeonStateText,
            saveData.relationship.hongryeon + stageData.hongryeonDelta);

        RefreshRelationship(
            jeonsangyeonSlider,
            jeonsangyeonStateText,
            saveData.relationship.JeonSangYeon + stageData.JeonSangYeonDelta);
    }

    private void RefreshRelationship(Slider slider, TMP_Text stateText, float value)
    {
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = Mathf.Clamp(value, 0, 100);

        stateText.text = GetRelationshipState(value);
    }

    private string GetRelationshipState(float value)
    {
        if (value < 0)
            return "적대";

        if (value <= 30)
            return "무관심";

        if (value <= 60)
            return "중립";

        if (value <= 80)
            return "호의";

        if (value <= 99)
            return "친밀";

        return "동맹";
    }

    private int GetBaseSalary(int currentStage)
    {
        return 10000 + ((currentStage - 1) / 3) * 5000;
    }
}