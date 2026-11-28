using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public GameObject scoreWindowObject;


    [Header("Scene Names")]
    public string storySceneName = "Story";
    public string lobbySceneName = "Lobby";


    private bool isGameOverState;


    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.scoreManager = this;
        }
    }


    // =====================================================
    // Score UI Open / Close
    // =====================================================

    public void ToggleScoreUI(
        bool isActive,
        bool isGameOver = false)
    {
        isGameOverState = isGameOver;


        if (scoreWindowObject != null)
        {
            scoreWindowObject.SetActive(
                isActive);
        }


        if (!isActive)
        {
            return;
        }


        if (clearWindowObject != null)
        {
            clearWindowObject.SetActive(
                !isGameOver);
        }


        if (gameOverWindowObject != null)
        {
            gameOverWindowObject.SetActive(
                isGameOver);
        }


        Refresh(isGameOver);
    }


    // =====================================================
    // Next Button
    // =====================================================

    public void OnNextButtonClick()
    {
        if (isGameOverState)
        {
            SceneManager.LoadScene(
                lobbySceneName);
        }
        else
        {
            SceneManager.LoadScene(
                storySceneName);
        }
    }


    // =====================================================
    // Refresh
    // =====================================================

    public void Refresh(bool isGameOver)
    {
        int currentStage =
            GameDataManager.Instance.GetCurrentStage();

        string playerName =
            GameDataManager.Instance.GetPlayerName();

        int currentMoney =
            GameDataManager.Instance.GetMoney();


        // -------------------------------------------------
        // Player / Date
        // -------------------------------------------------

        dateText.text =
            $"{currentStage:00}.00.";

        playerNameText.text =
            playerName;

        playerNameText2.text =
            playerName;


        // -------------------------------------------------
        // Money
        // -------------------------------------------------

        if (isGameOver)
        {
            // 게임오버 시 이번 판 보상 없음
            salaryText.text = "0";

            salaryText.color =
                new Color32(
                    214,
                    47,
                    45,
                    255);

            totalMoneyText.text =
                currentMoney.ToString();
        }
        else
        {
            int baseSalary =
                StageManager.Instance.GetBaseSalary(
                    currentStage);

            int earnedMoney =
                StageManager.Instance.stageData.earnedMoney;

            int totalReward =
                baseSalary + earnedMoney;


            salaryText.text =
                $"{baseSalary} + {earnedMoney}";


            // ClearStage와 동일하게
            // 보상이 0 이하라면 재화 감소는 발생하지 않음
            int rewardToApply =
                Mathf.Max(
                    0,
                    totalReward);


            long previewMoney =
                (long)currentMoney +
                rewardToApply;


            previewMoney =
                System.Math.Clamp(
                    previewMoney,
                    GameDataManager.MIN_MONEY,
                    GameDataManager.MAX_MONEY);


            totalMoneyText.text =
                previewMoney.ToString();
        }


        // -------------------------------------------------
        // Relationship
        // -------------------------------------------------

        RefreshRelationship(
            danwolSlider,
            danwolStateText,
            RelationshipType.Danwol,
            isGameOver);

        RefreshRelationship(
            yaseoSlider,
            yaseoStateText,
            RelationshipType.Yaseo,
            isGameOver);

        RefreshRelationship(
            macheonSlider,
            macheonStateText,
            RelationshipType.Macheon,
            isGameOver);

        RefreshRelationship(
            hongryeonSlider,
            hongryeonStateText,
            RelationshipType.Hongryeon,
            isGameOver);

        RefreshRelationship(
            jeonsangyeonSlider,
            jeonsangyeonStateText,
            RelationshipType.JeonSangYeon,
            isGameOver);
    }


    // =====================================================
    // Relationship UI
    // =====================================================

    private void RefreshRelationship(
        Slider slider,
        TMP_Text stateText,
        RelationshipType type,
        bool isGameOver)
    {
        float savedValue =
            GameDataManager.Instance.GetRelationship(
                type);


        float stageDelta = 0f;


        // 게임오버에서는 이번 판의 변화량을 표시하지 않음
        if (!isGameOver)
        {
            stageDelta =
                StageManager.Instance.GetRelationshipDelta(
                    type);
        }


        float previewValue =
            savedValue + stageDelta;


        previewValue =
            Mathf.Clamp(
                previewValue,
                GameDataManager.MIN_RELATIONSHIP,
                GameDataManager.MAX_RELATIONSHIP);


        slider.minValue =
            GameDataManager.MIN_RELATIONSHIP;

        slider.maxValue =
            GameDataManager.MAX_RELATIONSHIP;

        slider.value =
            previewValue;


        stateText.text =
            GameDataManager.Instance.GetRelationshipState(previewValue);
    }

}