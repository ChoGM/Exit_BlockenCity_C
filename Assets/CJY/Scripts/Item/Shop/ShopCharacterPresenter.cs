using UnityEngine;
using TMPro;

public class ShopCharacterPresenter : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField]
    private TMP_Text dialogueText;


    [Header("Speech Bubble")]
    [SerializeField]
    private AudioVisualizer speechBubbleVisualizer;


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
    // 상점 입장
    // =====================================================

    [Header("Shop Enter")]
    [SerializeField]
    private bool playGreetingOnEnable = false;


    private void OnEnable()
    {
        if (playGreetingOnEnable)
        {
            PlayShopGreeting();
        }
    }


    // =====================================================
    // 상점 입장 인사
    // =====================================================

    public void PlayShopGreeting()
    {
        SetDialogue(
            shopGreetingMessage
        );

        PlaySpeechAnimation();
    }


    // =====================================================
    // 일반 아이템 구매 성공
    // =====================================================

    public void PlayItemPurchaseSuccess(
        ItemData itemData)
    {
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
    }


    // =====================================================
    // 일반 아이템 구매 실패
    // =====================================================

    public void PlayItemPurchaseFailed(
        ItemData itemData)
    {
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
    }


    // =====================================================
    // 브로커 접선 시작
    // =====================================================

    public void PlayBrokerGreeting()
    {
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
        SetDialogue(
            brokerPurchaseSuccessMessage
        );

        PlaySpeechAnimation();
    }


    // =====================================================
    // 세력 우호도 구매 실패
    // =====================================================

    public void PlayFavorPurchaseFailed()
    {
        SetDialogue(
            brokerPurchaseFailMessage
        );

        PlaySpeechAnimation();
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
                "[ShopCharacterPresenter] Dialogue Text가 연결되지 않았습니다."
            );

            return;
        }


        dialogueText.text =
            message;
    }


    // =====================================================
    // 말풍선 애니메이션 실행
    // =====================================================

    private void PlaySpeechAnimation()
    {
        if (speechBubbleVisualizer == null)
        {
            Debug.LogWarning(
                "[ShopCharacterPresenter] AudioVisualizer가 연결되지 않았습니다."
            );

            return;
        }


        speechBubbleVisualizer.AutoStop();
    }
}