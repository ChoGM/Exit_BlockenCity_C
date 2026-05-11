using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightEffect : MonoBehaviour
{
    [Header("Outline Layer Settings")]
    [Tooltip("아웃라인 효과를 적용할 유니티 레이어 이름입니다.")]
    public string outlineLayerName = "Outline";

    private int originalLayer;
    private int outlineLayer;

    void Awake()
    {
        // 1. 시작할 때 원래 가지고 있던 레이어를 기억해둡니다. (예: "Interactable" 등)
        originalLayer = gameObject.layer;

        // 2. 아웃라인용 레이어의 인덱스를 찾아둡니다.
        outlineLayer = LayerMask.NameToLayer(outlineLayerName);

        if (outlineLayer == -1)
        {
            Debug.LogError($"[HighlightEffect] '{outlineLayerName}' 레이어가 존재하지 않습니다. 유니티 설정(Tags and Layers)에서 레이어를 먼저 추가해주세요.");
        }
    }

    public void EnableHighlight()
    {
        if (outlineLayer != -1)
        {
            // 나 자신과 모든 자식 오브젝트의 레이어를 Outline으로 변경
            SetLayerRecursively(gameObject, outlineLayer);
        }
    }

    public void DisableHighlight()
    {
        if (outlineLayer != -1)
        {
            // 나 자신과 모든 자식 오브젝트의 레이어를 원래 레이어로 복구
            SetLayerRecursively(gameObject, originalLayer);
        }
    }

    // 자식 오브젝트들까지 모두 레이어를 바꿔주기 위한 함수
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}