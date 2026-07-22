using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(HighlightEffect))]
public class LobbyInteractable : MonoBehaviour, IInteractable
{
    public string objectName;

    [Header("클릭 시 실행할 이벤트")]
    public UnityEvent onClickEvent;

    private HighlightEffect highlightEffect;

    private void Start()
    {
        highlightEffect = GetComponent<HighlightEffect>();
    }

    public void OnHoverEnter()
    {
        if (highlightEffect != null)
            highlightEffect.EnableHighlight();
    }

    public void OnHoverExit()
    {
        if (highlightEffect != null)
            highlightEffect.DisableHighlight();
    }

    public void OnClick()
    {
        onClickEvent?.Invoke();
    }
}