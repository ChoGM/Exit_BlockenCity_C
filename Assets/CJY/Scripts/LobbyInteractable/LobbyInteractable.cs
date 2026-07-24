using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(HighlightEffect))]
public class LobbyInteractable : MonoBehaviour, IInteractable
{
    public string objectName;

    [Header("UI 설정")]
    [Tooltip("마우스 호버 시 표시될 화살표 UI 오브젝트입니다.")]
    public GameObject arrowUI;

    [Header("클릭 시 실행할 이벤트")]
    public UnityEvent onClickEvent;

    private HighlightEffect highlightEffect;

    private void Start()
    {
        highlightEffect = GetComponent<HighlightEffect>();

        // 시작 시 화살표 UI 숨김
        if (arrowUI != null)
        {
            arrowUI.SetActive(false);
        }
    }

    public void OnHoverEnter()
    {
        // 1. 아웃라인 하이라이트 켜기
        //if (highlightEffect != null)
        //    highlightEffect.EnableHighlight();

        // 2. 화살표 UI 켜기
        if (arrowUI != null)
            arrowUI.SetActive(true);
    }

    public void OnHoverExit()
    {
        // 1. 아웃라인 하이라이트 끄기
        //if (highlightEffect != null)
        //    highlightEffect.DisableHighlight();

        // 2. 화살표 UI 끄기
        if (arrowUI != null)
            arrowUI.SetActive(false);
    }

    public void OnClick()
    {
        onClickEvent?.Invoke();
    }
}