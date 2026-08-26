using UnityEngine;

/// <summary>
/// 파티클 시스템이 없는 화면 이펙트/UI 이펙트 등의 자동 삭제용 컴포넌트
/// </summary>
public class TimedAutoDestroy : MonoBehaviour
{
    [Tooltip("이펙트가 유지된 후 삭제될 시간(초)")]
    [SerializeField] private float destroyDelay = 1.5f;

    private void Start()
    {
        Destroy(gameObject, destroyDelay);
    }
}