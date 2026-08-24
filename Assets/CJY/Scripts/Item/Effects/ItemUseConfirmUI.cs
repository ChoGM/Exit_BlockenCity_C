using UnityEngine;
using UnityEngine.UI;

public class ItemUseConfirmUI : MonoBehaviour
{
    public static ItemUseConfirmUI Instance
    {
        get;
        private set;
    }


    [Header("Panel")]
    [SerializeField]
    private GameObject confirmPanel;


    [Header("Item UI")]
    [SerializeField]
    private Image itemIcon;

    [SerializeField]
    private Text itemNameText;

    [SerializeField]
    private Text descriptionText;

    [SerializeField]
    private Text questionText;


    [Header("Buttons")]
    [SerializeField]
    private Button yesButton;

    [SerializeField]
    private Button noButton;


    private ItemData pendingItem;

    private bool isConfirmOpen = false;

    public bool IsConfirmOpen
        => isConfirmOpen;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        if (yesButton != null)
        {
            yesButton.onClick.AddListener(
                ConfirmUse
            );
        }


        if (noButton != null)
        {
            noButton.onClick.AddListener(
                CancelUse
            );
        }


        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }


        if (yesButton != null)
        {
            yesButton.onClick.RemoveListener(
                ConfirmUse
            );
        }


        if (noButton != null)
        {
            noButton.onClick.RemoveListener(
                CancelUse
            );
        }
    }


    /// <summary>
    /// 버튼 / 단축키 모두 여기로 들어온다.
    /// </summary>
    public void RequestUseItem(
        ItemData itemData)
    {
        if (itemData == null)
            return;


        // 이미 확인창이 떠 있으면
        // 다른 아이템 요청은 무시
        if (isConfirmOpen)
            return;


        if (ItemManager.Instance == null)
        {
            Debug.LogError(
                "[ItemConfirmUI] ItemManager가 없습니다."
            );

            return;
        }


        // 게임 종료 상태
        if (GameManager.Instance != null &&
            GameManager.Instance.isGameEnded)
        {
            return;
        }


        // 다른 시스템 때문에 이미 Pause 중이라면
        // 아이템 확인창을 새로 열지 않는다.
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGamePaused())
        {
            return;
        }


        int count =
            ItemManager.Instance.GetItemCount(
                itemData.ItemId
            );


        // 아이템 없음
        if (count <= 0)
        {
            Debug.Log(
                $"[ItemConfirmUI] " +
                $"{itemData.ItemName}을(를) 보유하고 있지 않습니다."
            );

            return;
        }


        pendingItem = itemData;

        RefreshPanel(itemData);


        isConfirmOpen = true;


        if (confirmPanel != null)
        {
            confirmPanel.SetActive(true);
        }


        // ----------------------------
        // 여기서 게임 Pause
        // ----------------------------

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }


        Debug.Log(
            $"[ItemConfirmUI] " +
            $"{itemData.ItemName} 사용 확인창 열림"
        );
    }


    private void RefreshPanel(
        ItemData itemData)
    {
        if (itemIcon != null)
        {
            itemIcon.sprite =
                itemData.Icon;
        }


        if (itemNameText != null)
        {
            itemNameText.text =
                itemData.ItemName;
        }


        if (descriptionText != null)
        {
            descriptionText.text =
                itemData.Description;
        }


        if (questionText != null)
        {
            questionText.text =
                $"정말 [{itemData.ItemName}]을(를) " +
                $"사용하시겠습니까?";
        }
    }


    /// <summary>
    /// 네
    /// </summary>
    public void ConfirmUse()
    {
        if (!isConfirmOpen)
            return;


        if (pendingItem == null)
        {
            ClosePanelAndResume();
            return;
        }


        if (ItemManager.Instance == null)
        {
            ClosePanelAndResume();
            return;
        }


        GameItemId itemId =
            pendingItem.ItemId;


        // IMPORTANT:
        // 아직 Pause를 풀지 않은 상태에서
        // 실제 아이템 효과 실행
        ItemUseResult result =
            ItemManager.Instance.TryUseItem(
                itemId
            );


        if (result.Success)
        {
            Debug.Log(
                $"[ItemConfirmUI] 사용 성공 | " +
                $"{pendingItem.ItemName} | " +
                $"{result.Message}"
            );
        }
        else
        {
            Debug.LogWarning(
                $"[ItemConfirmUI] 사용 실패 | " +
                $"{pendingItem.ItemName} | " +
                $"Reason: {result.FailureReason} | " +
                $"{result.Message}"
            );
        }


        ClosePanelAndResume();
    }


    /// <summary>
    /// 아니오
    /// </summary>
    public void CancelUse()
    {
        if (!isConfirmOpen)
            return;


        Debug.Log(
            "[ItemConfirmUI] 아이템 사용 취소"
        );


        // ItemManager를 호출하지 않으므로
        // 아이템 수량은 그대로 유지
        ClosePanelAndResume();
    }


    private void ClosePanelAndResume()
    {
        pendingItem = null;

        isConfirmOpen = false;


        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }


        // ----------------------------
        // 여기서 다시 게임 재개
        // ----------------------------

        if (GameManager.Instance != null &&
            !GameManager.Instance.isGameEnded)
        {
            GameManager.Instance.ResumeGame();
        }
    }
}