using System.Collections;
using UnityEngine;
using TMPro;

public class ShopCharacterPresenter : MonoBehaviour
{
    // =====================================================
    // UI
    // =====================================================

    [Header("Dialogue UI")]
    [SerializeField]
    private TMP_Text dialogueText;


    [Header("Speech Bubble")]
    [SerializeField]
    private AudioVisualizer speechBubbleVisualizer;


    // =====================================================
    // 캐릭터
    // =====================================================

    [Header("Characters")]

    [SerializeField]
    private GameObject shopkeeperCharacter;

    [SerializeField]
    private GameObject brokerCharacter;


    // =====================================================
    // 상점 캐릭터 대사
    // =====================================================

    [Header("Shopkeeper Dialogue")]

    [TextArea(2, 4)]
    [SerializeField]
    private string shopGreetingMessage =
        "어서 오세요. 필요한 물건이 있으신가요?";


    [TextArea(2, 4)]
    [SerializeField]
    private string shopPurchaseSuccessMessage =
        "{item} 주문이 완료됐습니다. 감사합니다.";


    [TextArea(2, 4)]
    [SerializeField]
    private string shopPurchaseFailMessage =
        "잔액이 조금 부족한 것 같네요.";


    [TextArea(2, 4)]
    [SerializeField]
    private string shopRepurchaseMessage =
        "또 필요한 물건이 있으신가요?";


    // =====================================================
    // 브로커 대사
    // =====================================================

    [Header("Broker Dialogue")]

    [TextArea(2, 4)]
    [SerializeField]
    private string brokerGreetingMessage =
        "어느 쪽에 이야기를 넣어드릴까요?";


    [TextArea(2, 4)]
    [SerializeField]
    private string brokerPurchaseSuccessMessage =
        "좋습니다. 그쪽에 잘 전달해 두죠.";


    [TextArea(2, 4)]
    [SerializeField]
    private string brokerPurchaseFailMessage =
        "이 정도로는 거래가 조금 어렵겠군요.";


    // =====================================================
    // 타이밍
    // =====================================================

    [Header("Dialogue Timing")]

    [Tooltip("구매 결과 대사 후 기본 질문으로 돌아가기까지의 시간")]
    [Min(0f)]
    [SerializeField]
    private float returnMessageDelay = 2.5f;


    // =====================================================
    // 상점 진입
    // =====================================================

    [Header("Shop Enter")]

    [SerializeField]
    private bool playGreetingOnEnable = false;


    private Coroutine returnMessageCoroutine;


    private void OnEnable()
    {
        if (playGreetingOnEnable)
        {
            PlayShopGreeting();
        }
    }


    private void OnDisable()
    {
        StopReturnMessageCoroutine();
    }


    // =====================================================
    // 상점 입장
    // =====================================================

    public void PlayShopGreeting()
    {
        StopReturnMessageCoroutine();

        ShowShopkeeper();


        SetDialogue(
            shopGreetingMessage
        );


        PlaySpeechAnimation();
    }


    // =====================================================
    // 상점 기본 / 재구매 질문
    // =====================================================

    public void PlayShopRepurchasePrompt()
    {
        StopReturnMessageCoroutine();

        ShowShopkeeper();


        SetDialogue(
            shopRepurchaseMessage
        );


        PlaySpeechAnimation();
    }


    // =====================================================
    // 일반 아이템 구매 성공
    // =====================================================

    public void PlayItemPurchaseSuccess(
        ItemData itemData)
    {
        StopReturnMessageCoroutine();

        ShowShopkeeper();


        string message =
            shopPurchaseSuccessMessage;


        if (itemData != null)
        {
            message =
                message.Replace(
                    "{item}",
                    itemData.ItemName
                );
        }


        SetDialogue(message);

        PlaySpeechAnimation();


        StartShopReturnMessage();
    }


    // =====================================================
    // 일반 아이템 구매 실패
    // =====================================================

    public void PlayItemPurchaseFailed(
        ItemData itemData)
    {
        StopReturnMessageCoroutine();

        ShowShopkeeper();


        string message =
            shopPurchaseFailMessage;


        if (itemData != null)
        {
            message =
                message.Replace(
                    "{item}",
                    itemData.ItemName
                );
        }


        SetDialogue(message);

        PlaySpeechAnimation();


        StartShopReturnMessage();
    }


    // =====================================================
    // 브로커 접선
    // =====================================================

    public void PlayBrokerGreeting()
    {
        StopReturnMessageCoroutine();

        ShowBroker();


        SetDialogue(
            brokerGreetingMessage
        );


        PlaySpeechAnimation();
    }


    // =====================================================
    // 세력 우호도 구매 성공
    // =====================================================

    public void PlayFavorPurchaseSuccess()
    {
        StopReturnMessageCoroutine();

        ShowBroker();


        SetDialogue(
            brokerPurchaseSuccessMessage
        );


        PlaySpeechAnimation();


        // 결과 대사 후
        // 다시 "어느 쪽에 이야기를 넣어드릴까요?"
        StartBrokerReturnMessage();
    }


    // =====================================================
    // 세력 우호도 구매 실패
    // =====================================================

    public void PlayFavorPurchaseFailed()
    {
        StopReturnMessageCoroutine();

        ShowBroker();


        SetDialogue(
            brokerPurchaseFailMessage
        );


        PlaySpeechAnimation();


        // 실패 후에도 다시 세력 선택 질문
        StartBrokerReturnMessage();
    }


    // =====================================================
    // 상점 기본 질문으로 자동 복귀
    // =====================================================

    private void StartShopReturnMessage()
    {
        StopReturnMessageCoroutine();


        returnMessageCoroutine =
            StartCoroutine(
                ReturnToShopMessageRoutine()
            );
    }


    private IEnumerator ReturnToShopMessageRoutine()
    {
        yield return new WaitForSeconds(
            returnMessageDelay
        );


        returnMessageCoroutine = null;


        ShowShopkeeper();


        SetDialogue(
            shopRepurchaseMessage
        );


        PlaySpeechAnimation();
    }


    // =====================================================
    // 브로커 질문으로 자동 복귀
    // =====================================================

    private void StartBrokerReturnMessage()
    {
        StopReturnMessageCoroutine();


        returnMessageCoroutine =
            StartCoroutine(
                ReturnToBrokerMessageRoutine()
            );
    }


    private IEnumerator ReturnToBrokerMessageRoutine()
    {
        yield return new WaitForSeconds(
            returnMessageDelay
        );


        returnMessageCoroutine = null;


        ShowBroker();


        SetDialogue(
            brokerGreetingMessage
        );


        PlaySpeechAnimation();
    }


    // =====================================================
    // 예약된 대사 중지
    // =====================================================

    private void StopReturnMessageCoroutine()
    {
        if (returnMessageCoroutine == null)
            return;


        StopCoroutine(
            returnMessageCoroutine
        );


        returnMessageCoroutine = null;
    }


    // =====================================================
    // 상점 캐릭터 표시
    // =====================================================

    private void ShowShopkeeper()
    {
        if (shopkeeperCharacter != null)
        {
            shopkeeperCharacter.SetActive(true);
        }


        if (brokerCharacter != null)
        {
            brokerCharacter.SetActive(false);
        }
    }


    // =====================================================
    // 브로커 표시
    // =====================================================

    private void ShowBroker()
    {
        if (shopkeeperCharacter != null)
        {
            shopkeeperCharacter.SetActive(false);
        }


        if (brokerCharacter != null)
        {
            brokerCharacter.SetActive(true);
        }
    }


    // =====================================================
    // 대사 변경
    // =====================================================

    private void SetDialogue(
        string message)
    {
        if (dialogueText == null)
        {
            Debug.LogWarning(
                "[ShopCharacterPresenter] " +
                "Dialogue Text가 연결되지 않았습니다."
            );

            return;
        }


        dialogueText.text =
            message;
    }


    // =====================================================
    // 말풍선 애니메이션
    // =====================================================

    private void PlaySpeechAnimation()
    {
        if (speechBubbleVisualizer == null)
        {
            Debug.LogWarning(
                "[ShopCharacterPresenter] " +
                "AudioVisualizer가 연결되지 않았습니다."
            );

            return;
        }


        speechBubbleVisualizer.AutoStop();
    }
}