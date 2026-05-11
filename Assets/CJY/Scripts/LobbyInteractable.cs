using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 이 스크립트를 넣으면 HighlightEffect도 자동으로 같이 붙게 해주는 편의성 코드입니다.
[RequireComponent(typeof(HighlightEffect))]
public class LobbyInteractable : MonoBehaviour, IInteractable
{
    [Header("연결할 UI 설정")]
    public string objectName; // 오브젝트 이름 (선택 사항)
    public GameObject targetCanvas; // 이 오브젝트를 클릭하면 뜰 캔버스

    private HighlightEffect highlightEffect;

    void Start()
    {
        // 시작할 때 같은 게임오브젝트에 있는 HighlightEffect를 찾아 연결합니다.
        highlightEffect = GetComponent<HighlightEffect>();
    }

    public void OnHoverEnter()
    {
        // 아웃라인 활성화
        if (highlightEffect != null)
        {
            highlightEffect.EnableHighlight();
        }
        Debug.Log($"{objectName} 아웃라인 ON");
    }

    public void OnHoverExit()
    {
        // 아웃라인 비활성화
        if (highlightEffect != null)
        {
            highlightEffect.DisableHighlight();
        }
        Debug.Log($"{objectName} 아웃라인 OFF");
    }

    public void OnClick()
    {
        if (targetCanvas != null)
        {
            // UI Manager에게 이 캔버스를 켜달라고 요청
            UIManager.Instance.ShowCanvas(targetCanvas);
        }
    }
}