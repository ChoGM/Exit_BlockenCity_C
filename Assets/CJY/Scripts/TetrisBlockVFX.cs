using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class TetrisBlockVFX : MonoBehaviour
{
    [Header("ShaderGraph: TextureSlider �� (0~1)")]
    [Range(0f, 1f)]
    [SerializeField] private float textureSlider = 0.0f;

    private MeshRenderer meshRenderer;
    private Material materialInstance;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        // ��Ÿ�ӿ����� �ν��Ͻ� ���͸��� ��� �� �ΰ��ӿ��� �ٷ� �ݿ�
        materialInstance = meshRenderer.material;
        ApplyAll();
    }

    private void Update()
    {
        // �÷��� �߿��� �� ���� �� �ݿ�
        ApplyAll();
    }

    // �����Ϳ��� Inspector �� �ٲ� �� �ٷ� �ݿ�
    private void OnValidate()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
#if UNITY_EDITOR
            // Edit Mode������ sharedMaterial ��� �� �޸� ���� ����
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

    // �ܺο��� �� �����
    public void SetTextureSlider(float value)
    {
        textureSlider = Mathf.Clamp01(value);

        if (materialInstance != null && materialInstance.HasProperty("_TextureSlider"))
        {
            materialInstance.SetFloat("_TextureSlider", textureSlider);
            Debug.Log($"{gameObject.name}: ApplyTextureSlider ȣ��, _TextureSlider = {textureSlider}");
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this); //Inspector �� ����
#endif
    }
}
