using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//스페셜퀘스트로 활용중
public class DialogUI : MonoBehaviour
{
    [Header("Background")]
    public Image backgroundImage;
    public List<BackgroundEntry> backgroundList;

    [Header("Character")]
    public Image characterImage;
    public List<CharacterImageEntry> characterImageList;

    [Header("Button Colors By Background")]
    public List<ButtonColorEntry> buttonColorList;

    [Header("UI Components")]
    public TMP_Text ocp;
    public TMP_Text nameText;
    public TMP_Text dialogText;
    public GameObject dialogPanel;

    public Button acceptButton;
    public Button declineButton;

    private Image acceptButtonImage;
    private Image declineButtonImage;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;

    private DialogLine currentLine;
    private bool isTyping = false;
    private string currentDialog = "";
    private Coroutine typingCoroutine;
    private bool isWaitingForChoice = false;

    [Header("Quest Data")]
    public List<SpecialQuestData> allQuestData; // Branch 번호 기반 인덱스 접근

    public static DialogUI Instance;
    public bool IsDialogActive => dialogPanel.activeSelf;
    public bool IsDialogRunning { get; private set; }
    private void Awake()
    {
        Instance = this;

        acceptButtonImage = acceptButton.GetComponent<Image>();
        declineButtonImage = declineButton.GetComponent<Image>();
    }

    void Start()
    {
        acceptButton.onClick.AddListener(OnAcceptClicked);
        declineButton.onClick.AddListener(OnDeclineClicked);

        HideAllButtons();
        dialogPanel.SetActive(false);

        InputManager.Instance.OnKeyV += HandleKeyV;
        InputManager.Instance.OnKey1 += HandleKey1;
        InputManager.Instance.OnKey2 += HandleKey2;
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnKeyV -= HandleKeyV;
            InputManager.Instance.OnKey1 -= HandleKey1;
            InputManager.Instance.OnKey2 -= HandleKey2;
        }
    }

    private void HandleKeyV()
    {
        if (!dialogPanel.activeSelf) return;

        if (isTyping)
        {
            FinishTyping(); // 타이핑 중이면 전체 출력
            return;
        }

        if (isWaitingForChoice)
        {
            return; // 선택지 상태에서는 스킵만 허용
        }
        ShowNextDialog();   // 선택지가 없고, 타이핑도 끝난 상태면 다음 대사 진행
    }


    private void HandleKey1()
    {
        if (!dialogPanel.activeSelf) return;
        if (isWaitingForChoice) OnAcceptClicked();
    }

    private void HandleKey2()
    {
        if (!dialogPanel.activeSelf) return;
        if (isWaitingForChoice) OnDeclineClicked();
    }

    public void StartDialog(int branch)
    {
        Debug.Log($"[DialogUI] StartDialog 호출됨 / branch = {branch}");
        Debug.Log($"DialogManager.Instance = {DialogManager.Instance}");

        IsDialogRunning = true;

        DialogManager.Instance.ClearQueue();
        DialogManager.Instance.LoadDialogByBranch(branch);
        isWaitingForChoice = false;
        //HideAllButtons();
        dialogPanel.SetActive(true);
        ShowNextDialog();
    }

    public void ShowNextDialog()
    {
        //HideAllButtons();

        if (!DialogManager.Instance.HasMoreDialog())
        {
            IsDialogRunning = false;

            nameText.text = "";
            dialogText.text = "대화가 종료되었습니다.";
            dialogPanel.SetActive(false);
            return;
        }
        currentLine = DialogManager.Instance.GetNextDialog();

        // ⭐ null 방어
        if (currentLine == null)
        {
            Debug.LogError("currentLine NULL 발생");
            IsDialogRunning = false;
            dialogPanel.SetActive(false);
            return;
        }

        // ===== name & ocp =====
        nameText.text = currentLine.name;

        if (!string.IsNullOrEmpty(currentLine.ocp))
        {
            ocp.gameObject.SetActive(true);
            ocp.text = currentLine.ocp;
        }
        else
        {
            ocp.gameObject.SetActive(false);
        }

        // ===== background =====
        UpdateBackground(currentLine.backGround);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeDialog(currentLine.dialog));

        isWaitingForChoice = currentLine.questionType == 1;
        UpdateCharacterImage(currentLine.characterImageID);
    }
    private void UpdateBackground(int bgID)
    {
        BackgroundEntry bgEntry = backgroundList.Find(b => b.id == bgID);

        if (bgEntry != null && bgEntry.image != null)
        {
            backgroundImage.sprite = bgEntry.image;
            backgroundImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Background ID {bgID}에 해당하는 이미지가 없습니다.");
        }

        ButtonColorEntry colorEntry = buttonColorList.Find(c => c.backgroundID == bgID);

        if (colorEntry != null)
        {
            if (acceptButtonImage != null)
                acceptButtonImage.color = colorEntry.acceptButtonColor;

            if (declineButtonImage != null)
                declineButtonImage.color = colorEntry.declineButtonColor;
        }
        else
        {
            Debug.Log($"Background ID {bgID}에 대한 버튼 색 설정이 없습니다.");
        }
    }

    private void UpdateCharacterImage(int charID)
    {
        CharacterImageEntry entry = characterImageList.Find(c => c.id == charID);

        if (entry != null && entry.image != null)
        {
            characterImage.sprite = entry.image;
            characterImage.gameObject.SetActive(true);
        }
        else
        {
            characterImage.gameObject.SetActive(false);
            Debug.LogWarning($"캐릭터 이미지 없음 ID: {charID}");
        }
    }
    private IEnumerator TypeDialog(string dialog)
    {
        isTyping = true;
        currentDialog = dialog;
        dialogText.text = "";

        foreach (char c in dialog.ToCharArray())
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        if (currentLine.questionType == 1)
        {
            ShowChoiceButtons();
        }
    }

    private void FinishTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogText.text = currentDialog;
        isTyping = false;

        if (currentLine.questionType == 1)
        {
            ShowChoiceButtons();
        }
    }

    private void ShowChoiceButtons()
    {
        acceptButton.gameObject.SetActive(true);
        declineButton.gameObject.SetActive(true);
    }

    private void HideAllButtons()
    {
        acceptButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);
    }

    private void OnAcceptClicked()
    {
        Debug.Log("수락 선택됨");
        isWaitingForChoice = false;
        HideAllButtons();

        if (currentLine.acceptBranch != -1)
        {
            SpecialQuestData matchedQuest = allQuestData.Find(q => q.branchID == currentLine.acceptBranch);
           
            if (matchedQuest != null)
            {
                SpecialQuestManager.Instance.AddQuest(matchedQuest);
            }
            Debug.Log("수락 선택됨2");
            DialogManager.Instance.ClearQueue();
            DialogManager.Instance.LoadDialogByBranch(currentLine.acceptBranch);
        }

        ShowNextDialog();
    }

    private void OnDeclineClicked()
    {
        Debug.Log("거절 선택됨");
        isWaitingForChoice = false;
        HideAllButtons();

        if (currentLine.declineBranch != -1)
        {
            DialogManager.Instance.ClearQueue();
            DialogManager.Instance.LoadDialogByBranch(currentLine.declineBranch);
            ShowNextDialog();
        }
        else
        {
            DialogManager.Instance.ClearQueue();
            nameText.text = "";
            dialogText.text = "대화를 거절했습니다.";
            dialogPanel.SetActive(false);
        }
    }
}
