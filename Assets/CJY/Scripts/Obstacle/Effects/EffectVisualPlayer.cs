using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class EffectVisualPlayer : MonoBehaviour
{
    [Header("==== 이펙트 프리팹 설정 ====")]
    [Tooltip("강풍 이펙트 (없을 경우 Resources/Effects/StrongWindEffect에서 로드)")]
    [SerializeField] private GameObject strongWindPrefab;

    [Tooltip("황사 이펙트 (없을 경우 Resources/Effects/DustEffect에서 로드)")]
    [SerializeField] private GameObject dustPrefab;

    [Tooltip("번개 이펙트 (없을 경우 Resources/Effects/LightningEffect에서 로드)")]
    [SerializeField] private GameObject lightningPrefab;

    [Tooltip("비 이펙트 (없을 경우 Resources/Effects/RainEffect에서 로드)")]
    [SerializeField] private GameObject rainPrefab;

    [Tooltip("스모그 이펙트 (없을 경우 Resources/Effects/SmogEffect에서 로드)")]
    [SerializeField] private GameObject smogPrefab;

    [Tooltip("잔설 이펙트 (없을 경우 Resources에서 로드)")]
    [SerializeField] private GameObject snowfallPrefab;

    [Header("==== UI 프리팹 ====")]
    [Tooltip("과열 경고 UI (없을 경우 Resources/Effects/OverheatUI에서 로드)")]
    [SerializeField] private GameObject overheatUIPrefab;

    [Header("==== 블록 비주얼 설정 ====")]
    [SerializeField] private float IceEffect = 0.5f;

    // 기본 리소스 경로 (백업용)
    private const string StrongWindPath = "GraphicResourc/Prefabs/VFX/VFX_WindScr_Burst";
    private const string DustPath = "GraphicResourc/Prefabs/VFX/VFX_DustScr_Burst";
    private const string LightningPath = "GraphicResourc/Prefabs/VFX/VFX_ElectricScr_Burst";
    private const string RainPath = "GraphicResourc/Prefabs/VFX/VFX_RainScr_Loop";
    private const string SmogPath = "GraphicResourc/Prefabs/VFX/VFX_SmogScr_Loop";
    private const string OverheatPath = "GraphicResourc/Prefabs/VFX/VFX_WindScr_Burst";
    private const string SnowfallPath = "GraphicResourc/Prefabs/VFX/VFX_FrozenScr_Loop";

    // 1월 얼음 블록 이미지
    public GameObject VisualFreezeBlock(TetriminoBlock block)
    {
        if (block == null)
        {
            Debug.LogWarning("VisualFreezeBlock 호출 시 block이 null!");
            return null;
        }

        // 모든 자식 VFX 찾기
        var vfxList = block.GetComponentsInChildren<TetrisBlockVFX>();
        if (vfxList.Length == 0)
        {
            Debug.LogWarning($"{block.name}의 자식에 TetrisBlockVFX 없음!");
            return block.gameObject;
        }

        // 각 자식 VFX에 적용 + 디버그
        foreach (var vfx in vfxList)
        {
            vfx.SetTextureSlider(IceEffect);
            Debug.Log($"얼음 효과 적용: {vfx.gameObject.name}, TextureSlider = 0.5");
        }

        return block.gameObject;
    }

    // 2월 강풍 효과 (단발 효과 → 반환 X)
    public GameObject PlayStrongWindEffect()
    {
        var prefab = strongWindPrefab ?? Resources.Load<GameObject>(StrongWindPath);
        //var prefab = Resources.Load<GameObject>(StrongWindPath);

        if (prefab == null) return null;

        return Instantiate(prefab);
    }

    // 3월 잔설 효과
    public GameObject SnowfallEffect()
    {
        var prefab = snowfallPrefab ?? Resources.Load<GameObject>(SnowfallPath);
        if (prefab == null)
        {
            Debug.LogWarning("[SnowfallEffect] 잔설 프리팹을 찾을 수 없습니다.");
            return null;
        }

        return Instantiate(prefab);
    }

    // 4,5월 황사 효과 (반복 → IEnumerator 반환)
    public GameObject DustStormEffect()
    {
        var prefab = dustPrefab ?? Resources.Load<GameObject>(DustPath);
        //var prefab = Resources.Load<GameObject>(DustPath);
        if (prefab == null) return null;

        return Instantiate(prefab);
    }

    // 6월 번개 이펙트 (단발 효과)
    public GameObject PlayLightningEffect()
    {
        var prefab = lightningPrefab ?? Resources.Load<GameObject>(LightningPath);
        //var prefab = Resources.Load<GameObject>(LightningPath);
        if (prefab == null) return null;

        return Instantiate(prefab);
    }

    // 7월 비 내리는 효과
    public GameObject PlayRainEffect()
    {
        var prefab = rainPrefab ?? Resources.Load<GameObject>(RainPath);
        if (prefab == null) return null;

        var instance = Instantiate(prefab);

        //비 프리팹 안 Camera 찾기
        var rainCamera = instance.GetComponentInChildren<Camera>(true);
        if (rainCamera == null)
        {
            Debug.LogWarning("[RainEffect] 프리팹에 Camera가 없습니다.");
            return instance;
        }

        //모든 UI Canvas를 이 Camera로 바인딩
        UICameraBinder.BindAllCanvas(rainCamera);

        return instance;
    }


    // 8월 폭염 효과
    public GameObject PlayOverheatWarning(string message = "건축 기계 과열!")
    {
        var prefab = Resources.Load<GameObject>(OverheatPath);
        if (prefab == null) return null;

        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return null;

        var instance = Instantiate(prefab, canvas.transform);

        var text = instance.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = message;

        var rect = instance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        return instance;
    }

    // 10월 건기 블록 파괴 효과
    public void PlayBlockCrumbleEffect(Vector3 position)
    {
        // 미정
    }

    // 11월 스모그 효과
    public GameObject PlaySmogEffect()
    {
        var prefab = smogPrefab ?? Resources.Load<GameObject>(SmogPath);
        //var prefab = Resources.Load<GameObject>(SmogPath);
        if (prefab == null) return null;

        return Instantiate(prefab);
    }
}
