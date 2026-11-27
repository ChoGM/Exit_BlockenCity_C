using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class EffectVisualPlayer : MonoBehaviour
{
    [Header("==== VFX 생성 부모 ====")]
    [Tooltip("Hierarchy의 VFX 오브젝트를 연결하세요. 지정하지 않으면 기존처럼 루트에 생성됩니다.")]
    [SerializeField] private Transform vfxRoot;

    // VFX를 생성할 때부터 부모를 지정하며, 기존 월드 좌표를 유지합니다.
    private GameObject InstantiateVFX(GameObject prefab)
    {
        return vfxRoot != null
            ? Instantiate(prefab, vfxRoot, true)
            : Instantiate(prefab);
    }

    // 건기 블록 파괴 효과 등 특정 월드 좌표에서 생성되는 VFX용.
    private GameObject InstantiateVFX(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return vfxRoot != null
            ? Instantiate(prefab, position, rotation, vfxRoot)
            : Instantiate(prefab, position, rotation);
    }

    [Header("==== 이펙트 프리팹 설정 ====")]

    [SerializeField]
    private GameObject strongWindPrefab;

    [SerializeField]
    private GameObject dustPrefab;

    [SerializeField]
    private GameObject lightningPrefab;

    [SerializeField]
    private GameObject rainPrefab;

    [SerializeField]
    private GameObject smogPrefab;

    [SerializeField]
    private GameObject snowfallPrefab;


    [Header("==== 방해물 효과음 타입 ====")]

    [Tooltip("1월 얼어붙은 블록 효과음 타입")]
    [SerializeField]
    private ObstacleType freezeSoundType;

    [Tooltip("2월 강풍 효과음 타입")]
    [SerializeField]
    private ObstacleType strongWindSoundType;

    [Tooltip("3월 잔설 효과음 타입")]
    [SerializeField]
    private ObstacleType snowfallSoundType;

    [Tooltip("4~5월 황사 효과음 타입")]
    [SerializeField]
    private ObstacleType dustSoundType;

    [Tooltip("6월 번개 효과음 타입")]
    [SerializeField]
    private ObstacleType lightningSoundType;

    [Tooltip("7월 장마 효과음 타입")]
    [SerializeField]
    private ObstacleType rainSoundType;

    [Tooltip("8월 폭염 효과음 타입")]
    [SerializeField]
    private ObstacleType overheatSoundType;

    private bool overheatSoundPlayed = false;

    [Tooltip("10월 건기 블록 파괴 효과음 타입")]
    [SerializeField]
    private ObstacleType drySeasonSoundType;

    [Tooltip("11월 스모그 효과음 타입")]
    [SerializeField]
    private ObstacleType smogSoundType;


    [Header("==== 폭염 UI 프리팹 ====")]

    [SerializeField]
    private GameObject overheatUIPrefab;

    [SerializeField]
    private GameObject overheatEffectPrefab;


    [Header("==== 블록 비주얼 설정 ====")]

    [SerializeField]
    private float IceEffect = 0.5f;


    // =============================================
    // Resource Paths
    // =============================================

    private const string StrongWindPath =
        "GraphicResourc/Prefabs/VFX/VFX_WindScr_Burst";

    private const string DustPath =
        "GraphicResourc/Prefabs/VFX/VFX_DustScr_Burst";

    private const string LightningPath =
        "GraphicResourc/Prefabs/VFX/VFX_ElectricScr_Burst";

    private const string RainPath =
        "GraphicResourc/Prefabs/VFX/VFX_RainScr_Loop";

    private const string SmogPath =
        "GraphicResourc/Prefabs/VFX/VFX_SmogScr_Loop";

    private const string SnowfallPath =
        "GraphicResourc/Prefabs/VFX/VFX_FrozenScr_Loop";


    [Header("==== 건기 이펙트 설정 ====")]

    [SerializeField]
    private GameObject drySeasonBlockBreakPrefab;

    [SerializeField]
    private GameObject drySeasonScreenPrefab;


    // =============================================
    // 1월 : 얼어붙은 블록
    // 얼음 블록이 실제 적용될 때마다 효과음
    // =============================================
    public GameObject VisualFreezeBlock(
        TetriminoBlock block)
    {
        if (block == null)
        {
            Debug.LogWarning(
                "VisualFreezeBlock 호출 시 block이 null!"
            );

            return null;
        }


        var vfxList =
            block.GetComponentsInChildren<TetrisBlockVFX>();


        if (vfxList.Length == 0)
        {
            Debug.LogWarning(
                $"{block.name}의 자식에 TetrisBlockVFX 없음!"
            );

            return block.gameObject;
        }


        foreach (var vfx in vfxList)
        {
            vfx.SetTextureSlider(IceEffect);
        }


        // 얼음 비주얼이 실제 적용된 경우에만 효과음
        ObstacleSoundManager.Instance?.
            PlayObstacleSound(freezeSoundType);


        return block.gameObject;
    }


    // =============================================
    // 2월 : 강풍
    // 바람 VFX가 실행될 때마다 효과음
    // =============================================
    public GameObject PlayStrongWindEffect()
    {
        var prefab =
            strongWindPrefab ??
            Resources.Load<GameObject>(StrongWindPath);


        if (prefab == null)
            return null;


        GameObject instance =
            InstantiateVFX(prefab);


        ObstacleSoundManager.Instance?.
            PlayObstacleSound(strongWindSoundType);


        return instance;
    }


    // =============================================
    // 3월 : 잔설
    //
    // ObstacleEffects에서 snowfallInstance == null일 때만
    // 이 함수가 호출되므로 효과음도 최초 1회만 발생
    // =============================================
    public GameObject SnowfallEffect()
    {
        var prefab =
            snowfallPrefab ??
            Resources.Load<GameObject>(SnowfallPath);


        if (prefab == null)
        {
            Debug.LogWarning(
                "[SnowfallEffect] 잔설 프리팹을 찾을 수 없습니다."
            );

            return null;
        }


        GameObject instance =
            InstantiateVFX(prefab);


        ObstacleSoundManager.Instance?.
            PlayObstacleSound(snowfallSoundType);


        return instance;
    }


    // =============================================
    // 4~5월 : 황사
    // 황사 VFX가 나타날 때마다 효과음
    // =============================================
    public GameObject DustStormEffect()
    {
        var prefab =
            dustPrefab ??
            Resources.Load<GameObject>(DustPath);


        if (prefab == null)
            return null;


        GameObject instance =
            InstantiateVFX(prefab);


        ObstacleSoundManager.Instance?.
            PlayObstacleSound(dustSoundType);


        return instance;
    }


    // =============================================
    // 6월 : 번개
    // 번개 VFX가 실행될 때마다 효과음
    // =============================================
    public GameObject PlayLightningEffect()
    {
        var prefab =
            lightningPrefab ??
            Resources.Load<GameObject>(LightningPath);


        if (prefab == null)
            return null;


        GameObject instance =
            InstantiateVFX(prefab);


        ObstacleSoundManager.Instance?.
            PlayObstacleSound(lightningSoundType);

        // 카메라 쉐이킹
        CameraShakeManager.Instance?.
            Play(CameraShakeType.Explosion);

        return instance;
    }


    // =============================================
    // 7월 : 장마
    //
    // Rain Effect가 존재하는 동안
    // 비 효과음 계속 반복
    // =============================================
    public GameObject PlayRainEffect()
    {
        var prefab =
            rainPrefab ??
            Resources.Load<GameObject>(RainPath);


        if (prefab == null)
            return null;


        GameObject instance =
            InstantiateVFX(prefab);


        // 비 VFX 오브젝트에 Loop AudioSource 추가
        //
        // instance가 Destroy되면
        // AudioSource도 같이 없어지므로
        // 비 효과음도 자동 종료된다.
        ObstacleSoundManager.Instance?.
            PlayLoopSoundOnObject(
                rainSoundType,
                instance
            );


        // 비 프리팹 안 Camera 찾기
        var rainCamera =
            instance.GetComponentInChildren<Camera>(true);


        if (rainCamera == null)
        {
            Debug.LogWarning(
                "[RainEffect] 프리팹에 Camera가 없습니다."
            );

            return instance;
        }


        // 모든 UI Canvas를 Rain Camera에 바인딩
        UICameraBinder.BindAllCanvas(
            rainCamera
        );


        return instance;
    }


    // =============================================
    // 8월 : 폭염
    // 기존 로직 유지
    // =============================================
    public GameObject PlayOverheatWarning(
    string message = "건축 기계 과열!")
    {
        if (overheatUIPrefab == null)
        {
            Debug.LogWarning(
                "[PlayOverheatWarning] overheatUIPrefab이 연결되어 있지 않습니다!"
            );

            return null;
        }

        var text =
            overheatUIPrefab.GetComponentInChildren<TextMeshProUGUI>();

        if (text != null)
        {
            text.text = message;
        }

        // =============================================
        // 폭염 효과음 최초 1회만 재생
        // =============================================
        if (!overheatSoundPlayed)
        {
            ObstacleSoundManager.Instance?.
                PlayObstacleSound(overheatSoundType);

            if (overheatEffectPrefab != null)
            {
                InstantiateVFX(overheatEffectPrefab);
            }

            overheatSoundPlayed = true;
        }

        overheatUIPrefab.transform.SetAsLastSibling();
        overheatUIPrefab.SetActive(true);

        return overheatUIPrefab;
    }


    // =============================================
    // 10월 : 건기
    //
    // 실제 블록 파괴 VFX가 만들어질 때마다 효과음
    // =============================================
    public GameObject PlayBlockCrumbleEffect(
        Vector3 position)
    {
        if (drySeasonBlockBreakPrefab == null)
        {
            Debug.LogWarning(
                "[PlayBlockCrumbleEffect] 건기 블록 파괴 프리팹이 연결되지 않았습니다."
            );

            return null;
        }


        GameObject instance =
            InstantiateVFX(
                drySeasonBlockBreakPrefab,
                position,
                Quaternion.identity
            );


        ObstacleSoundManager.Instance?.
            PlayObstacleSound(drySeasonSoundType);


        // 카메라 쉐이킹
        CameraShakeManager.Instance?.
            Play(CameraShakeType.Explosion);


        return instance;
    }


    // =============================================
    // 10월 : 건기 화면 전체 이펙트
    //
    // 여기에는 효과음을 넣지 않음.
    // 블록 파괴 VFX에서 이미 재생하기 때문.
    // =============================================
    public GameObject PlayDrySeasonScreenEffect()
    {
        if (drySeasonScreenPrefab == null)
        {
            Debug.LogWarning(
                "[PlayDrySeasonScreenEffect] 건기 화면 이펙트 프리팹이 연결되지 않았습니다."
            );

            return null;
        }


        var instance =
            InstantiateVFX(drySeasonScreenPrefab);


        var binder =
            instance.GetComponent<EffectCameraBinder>();


        if (binder != null)
        {
            binder.BindToMainCamera(instance);
        }


        return instance;
    }


    // =============================================
    // 11월 : 스모그
    //
    // ObstacleEffects에서 smogInstance == null일 때만
    // 호출되므로 최초 1회만 효과음
    // =============================================
    public GameObject PlaySmogEffect()
    {
        var prefab =
            smogPrefab ??
            Resources.Load<GameObject>(SmogPath);


        if (prefab == null)
            return null;


        GameObject instance =
            InstantiateVFX(prefab);


        ObstacleSoundManager.Instance?.
            PlayObstacleSound(smogSoundType);


        return instance;
    }
}