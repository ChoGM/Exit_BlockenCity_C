using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class TetrisBlockVFX : MonoBehaviour
{
    [Header("ShaderGraph: TextureSlider 값 (0~1)")]
    [Range(0f, 1f)]
    [SerializeField] private float textureSlider = 0.0f;

    private MeshRenderer meshRenderer;
    private Material materialInstance;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        // 런타임에서는 인스턴스 머터리얼 사용 → 인게임에서 바로 반영
        materialInstance = meshRenderer.material;
        ApplyAll();
    }

    private void Update()
    {
        // 플레이 중에도 값 변경 시 반영
        ApplyAll();
    }

    // 에디터에서 Inspector 값 바꿀 때 바로 반영
    private void OnValidate()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
#if UNITY_EDITOR
            // Edit Mode에서는 sharedMaterial 사용 → 메모리 누수 방지
            materialInstance = meshRenderer.sharedMaterial;
#else
            materialInstance = meshRenderer.material;
#endif
            ApplyAll();
        }
    }

    private void ApplyAll()
    {
        ApplyTextureSlider();
    }

    private void ApplyTextureSlider()
    {
        if (materialInstance != null && materialInstance.HasProperty("_TextureSlider"))
            materialInstance.SetFloat("_TextureSlider", Mathf.Clamp01(textureSlider));
    }

    // 외부에서 값 변경용
    public void SetTextureSlider(float value)
    {
        textureSlider = Mathf.Clamp01(value);

        if (materialInstance != null && materialInstance.HasProperty("_TextureSlider"))
        {
            materialInstance.SetFloat("_TextureSlider", textureSlider);
            Debug.Log($"{gameObject.name}: ApplyTextureSlider 호출, _TextureSlider = {textureSlider}");
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this); //Inspector 값 갱신
#endif
    }
}
