using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class TetrisBlockVFX : MonoBehaviour
{
    [Header("Metallic Map ����Ʈ")]
    [SerializeField] private Texture2D[] metallicMaps;

    [Header("������ Metallic Map �ε��� (-1 = ����)")]
    [SerializeField] private int selectedMapIndex = -1;

    [Header("Metallic �� (0=��ݼ�, 1=���� �ݼ�)")]
    [Range(0f, 1f)]
    [SerializeField] private float metallicValue = 0.0f;

    [Header("Smoothness �� (0=��ĥ��, 1=�Ų�����)")]
    [Range(0f, 1f)]
    [SerializeField] private float smoothnessValue = 0.5f;

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
        ApplyMetallicMap();
        ApplyMetallicValue();
        ApplySmoothnessValue();
    }

    private void ApplyMetallicMap()
    {
        if (materialInstance == null) return;

        Texture map = null;
        if (selectedMapIndex >= 0 && selectedMapIndex < metallicMaps.Length)
            map = metallicMaps[selectedMapIndex];

        // URP Lit Shader���� ����ϴ� ������Ƽ
        if (materialInstance.HasProperty("_MetallicSpecGlossMap"))
            materialInstance.SetTexture("_MetallicSpecGlossMap", map);

        if (map != null)
            materialInstance.EnableKeyword("_METALLICSPECGLOSSMAP");
        else
            materialInstance.DisableKeyword("_METALLICSPECGLOSSMAP");
    }

    private void ApplyMetallicValue()
    {
        if (materialInstance != null && materialInstance.HasProperty("_Metallic"))
            materialInstance.SetFloat("_Metallic", Mathf.Clamp01(metallicValue));
    }

    private void ApplySmoothnessValue()
    {
        if (materialInstance != null && materialInstance.HasProperty("_Smoothness"))
            materialInstance.SetFloat("_Smoothness", Mathf.Clamp01(smoothnessValue));
    }

    // �ܺο��� �� �����
    public void SetMetallicMap(int index) => selectedMapIndex = index;
    public void SetMetallicValue(float value) => metallicValue = Mathf.Clamp01(value);
    public void SetSmoothness(float value) => smoothnessValue = Mathf.Clamp01(value);
}
