using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObstacleEffects : MonoBehaviour
{
    [Header("얼어붙은 블록 (회전 금지)")]
    [SerializeField] private float freezeChance = 0.9f;

    [Header("잔설 (입력 지연)")]
    [SerializeField] private float inputDelayTime = 0.5f;

    [Header("강풍 (블록 밀림)")]
    [SerializeField] private float windDuration = 3f;
    [SerializeField] private float windInterval = 0.3f;
    [SerializeField] private Vector2 windWaitIntervalRange = new Vector2(2f, 5f);

    [Header("황사 (시야 차단)")]
    [SerializeField] private Vector2 dustSpawnIntervalRange = new Vector2(0.5f, 3f);
    //[SerializeField] private Vector2 dustLifetimeRange = new Vector2(1f, 2f);

    [Header("낙뢰 (조작 일시 정지)")]
    [SerializeField] private Vector2 lightningIntervalRange = new Vector2(2f, 5f);
    [SerializeField] private float lightningStopTime = 1.0f;

    [Header("장마 (낙하 속도 저하)")]
    [SerializeField] private float rainDropSpeedMultiplier = 0.5f; // 기본 속도의 50%

    [Header("건기 (블록 파괴 확률)")]
    [SerializeField] private float BreakBlockChance = 0.5f;

    //번개 중복 방지
    private Coroutine lightningCoroutine;

    //황사 중복 방지
    private Coroutine dustStormCoroutine;

    // 월 1회 생성형 이펙트 캐싱
    private GameObject snowfallInstance;
    private GameObject rainInstance;
    private GameObject smogInstance;

    // 강풍 중복 방지 + 상태 관리
    private Coroutine randomWindCoroutine;


    // 1월 : 회전 금지 (얼어붙은 블록)
    public void TryFreezeBlock(ObstacleGameState state)
    {
        Debug.Log("얼어붙은 블록(회전금지) 효과 실행");

        TetriminoBlock block = state.SpawnedBlock; //흐음..
        if (block == null) return;

        state.InputBlocker.blockRotation = false;

        if (UnityEngine.Random.value <= freezeChance)
        {
            Debug.Log("회전금지 효과");
            state.InputBlocker.blockRotation = true;
            state.VisualPlayer?.VisualFreezeBlock(block);
        }
    }


    // 2월 : 블록 밀림 (강풍)
    public void PushBlockRandomly(ObstacleGameState state)
    {
        // 이미 강풍이 돌고 있으면 시작하지 않음
        if (randomWindCoroutine != null)
            return;

        if (state == null || state.SpawnedBlock == null)
            return;

        randomWindCoroutine = state.StartManagedCoroutine?.Invoke(
            WindPushLoop(state)
        );
    }


    private IEnumerator WindPushLoop(ObstacleGameState state)
    {
        while (state.SpawnedBlock != null)
        {
            yield return new WaitForSeconds(
                Random.Range(windWaitIntervalRange.x, windWaitIntervalRange.y)
            );

            if (state.SpawnedBlock == null)
                break;

            Vector3 dir = Random.value > 0.5f ? Vector3.left : Vector3.right;

            float timer = 0f;

            var obj = state.VisualPlayer?.PlayStrongWindEffect();
            if (obj != null)
                state.RegisterObject?.Invoke(obj);

            while (timer < windDuration && state.SpawnedBlock != null)
            {
                state.TetrisController?.MoveBlockByWind(dir);
                yield return new WaitForSeconds(windInterval);
                timer += windInterval;
            }
        }

        //여기서 반드시 초기화
        randomWindCoroutine = null;
    }

    // 3월 : 입력 지연 (잔설)
    public void InputDelay(ObstacleGameState state)
    {
        Debug.Log("잔설 입력 지연 효과 실행");

        var blocker = state.InputBlocker;
        if (blocker == null) return;

        blocker.SetInputProcessDelay(inputDelayTime);

        //var obj = state.VisualPlayer?.SnowfallEffect();
        //if (obj != null) state.RegisterObject?.Invoke(obj);
        if (snowfallInstance == null)
        {
            snowfallInstance = state.VisualPlayer?.SnowfallEffect();
            if (snowfallInstance != null)
                state.RegisterObject?.Invoke(snowfallInstance);
        }

    }

    // 4~5월 : 시야 차단 (황사)
    public void ApplyDustStormEffect(ObstacleGameState state)
    {
        Debug.Log("황사 (시야 차단) 효과 실행");

        // 이미 황사 코루틴이 돌고 있으면 중복 실행 금지
        if (dustStormCoroutine != null)
            return;

        var blocker = state.InputBlocker;
        if (blocker == null) return;

        dustStormCoroutine = blocker.StartCoroutine(
            DustStormLoop(state.VisualPlayer)
        );
    }

    private IEnumerator DustStormLoop(EffectVisualPlayer visualPlayer)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(dustSpawnIntervalRange.x, dustSpawnIntervalRange.y));

            var instance = visualPlayer?.DustStormEffect();
            //if (instance != null)
            //    Destroy(instance, Random.Range(dustLifetimeRange.x, dustLifetimeRange.y));
        }
    }

    // 6월 : 조작 일시 정지 (낙뢰)
    public void DisableControlTemporary(ObstacleGameState state)
    {
        Debug.Log("낙뢰 (조작 일시 정시) 효과 실행");

        // 이미 낙뢰 코루틴이 돌고 있으면 또 시작하지 않음
        if (lightningCoroutine != null)
            return;

        var blocker = state.InputBlocker;
        if (blocker == null) return;

        lightningCoroutine = state.StartManagedCoroutine?.Invoke(
            LightningStormLoop(blocker, state.VisualPlayer)
        );
    }

    private IEnumerator LightningStormLoop(InputBlocker blocker, EffectVisualPlayer visualPlayer)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(lightningIntervalRange.x, lightningIntervalRange.y));

            //Debug.Log("(코루틴)조작 일시 정지");
            blocker.isInputBlocked = true;

            var obj = visualPlayer?.PlayLightningEffect();
            if (obj != null)
            {
                //Destroy(obj, lightningStopTime); // 번개 지속 시간 후 삭제
            }

            yield return new WaitForSeconds(lightningStopTime); // 인스턴스 필드 접근

            blocker.isInputBlocked = false;
        }
    }

    // 7월 : 스페이스 불가능
    public void DisableSpace(ObstacleGameState state)
    {
        Debug.Log("장마 (스페이스불가능) 효과 실행");

        var blocker = state.InputBlocker;
        if (blocker == null) return;

        blocker.blockHardDrop = true;

        //var obj = state.VisualPlayer?.PlayRainEffect();
        //if (obj != null) state.RegisterObject?.Invoke(obj);
        if (rainInstance == null)
        {
            rainInstance = state.VisualPlayer?.PlayRainEffect();
            if (rainInstance != null)
                state.RegisterObject?.Invoke(rainInstance);
        }


    }

    // 7월 : 낙하속도 저하 (장마)
    public void SlowDropSpeed(ObstacleGameState state)
    {
        Debug.Log("장마 (낙하속도저하) 효과 실행");
        if (state == null) return;

        TetrisManager.Instance.fallInterval *= rainDropSpeedMultiplier;
    }

    // 8월 : 다음 블록 UI 비활성화 (폭염)
    public void HideNextBlockUI(ObstacleGameState state)
    {
        Debug.Log("폭염 (다음UI 비활성화) 효과 실행");

        var obj = state.VisualPlayer?.PlayOverheatWarning("건축 기계 과열!");
        if (obj != null) state.RegisterObject?.Invoke(obj);
    }

    // 10월 : 블록 파괴 확률 (건기)
    //public void BreakBlockOnPlace(ObstacleGameState state)
    //{
    //    Debug.Log("건기 (y축 압축 낙하)");

    //    var block = state.LockedBlock;
    //    if (block == null) return;

    //    var children = block.GetComponentsInChildren<TetriminoBlockChild>();
    //    if (children.Length == 0) return;

    //    var tower = TetrisManager.Instance.tower;
    //    if (tower == null) return;

    //    List<TetriminoBlockChild> deleted = new();
    //    List<TetriminoBlockChild> survivors = new();

    //    // 1) 삭제할 블록 선택
    //    foreach (var c in children)
    //    {
    //        if (c != null && UnityEngine.Random.value <= BreakBlockChance)
    //            deleted.Add(c);
    //        else
    //            survivors.Add(c);
    //    }

    //    if (deleted.Count == 0) return;

    //    // 2) 컬럼(x,z)별 삭제된 y좌표 수집
    //    Dictionary<Vector2Int, List<float>> deletedMap = new();

    //    foreach (var d in deleted)
    //    {
    //        Vector3 wp = d.transform.position;
    //        Vector2Int key = new Vector2Int(
    //            Mathf.RoundToInt(wp.x * 10f),
    //            Mathf.RoundToInt(wp.z * 10f)
    //        );
    //        if (!deletedMap.ContainsKey(key))
    //            deletedMap[key] = new List<float>();
    //        deletedMap[key].Add(wp.y);
    //    }

    //    // 3) 컬럼별로 압축
    //    foreach (var kv in deletedMap)
    //    {
    //        Vector2Int key = kv.Key;
    //        var deletedYs = kv.Value;

    //        // 해당 컬럼에서 살아남은 블록 찾기
    //        var column = survivors.Where(c =>
    //        {
    //            Vector3 wp = c.transform.position;
    //            Vector2Int k = new Vector2Int(
    //                Mathf.RoundToInt(wp.x * 10f),
    //                Mathf.RoundToInt(wp.z * 10f)
    //            );
    //            return k == key;
    //        }).OrderBy(c => c.transform.position.y).ToList();

    //        foreach (var c in column)
    //        {
    //            Vector3 wp = c.transform.position;

    //            // 삭제된 y중 c보다 아래에 있는 칸 수
    //            int fallCount = deletedYs.Count(dy => dy < wp.y);

    //            if (fallCount <= 0) continue;

    //            float targetY = wp.y - fallCount;
    //            Vector3 target = new Vector3(wp.x, targetY, wp.z);

    //            // 타워에서 기존 위치 제거
    //            tower.RemoveBlockFromTower(c.GridPosition);

    //            Debug.DrawLine(wp, target, Color.green, 2f);
    //            Debug.Log($"[압축] {c.name} {wp.y:F1} -> {targetY:F1} (낙하 {fallCount}칸)");

    //            state.StartManagedCoroutine?.Invoke(SmoothDropAndSync(c, target, tower));
    //        }
    //    }

    //    // 4) 실제 삭제 수행
    //    foreach (var d in deleted)
    //    {
    //        tower.RemoveBlockFromTower(d.GridPosition);
    //        d.DeletBlock();
    //    }

    //    // 5) 이펙트
    //    state.VisualPlayer.PlayBlockCrumbleEffect(block.transform.position);
    //    block.CleanupIfEmpty();
    //}


    // 시각적 낙하 + 타워정보 갱신

    // 10월 : 블록 파괴 확률 (건기)
    public void BreakBlockOnPlace(ObstacleGameState state)
    {
        Debug.Log("건기 (y축 압축 낙하)");

        var block = state.LockedBlock;
        if (block == null) return;

        var children = block.GetComponentsInChildren<TetriminoBlockChild>();
        if (children.Length == 0) return;

        var tower = TetrisManager.Instance.tower;
        if (tower == null) return;

        List<TetriminoBlockChild> deleted = new();
        List<TetriminoBlockChild> survivors = new();

        // 1) 삭제할 블록 선택
        foreach (var c in children)
        {
            if (c != null && UnityEngine.Random.value <= BreakBlockChance)
                deleted.Add(c);
            else
                survivors.Add(c);
        }

        if (deleted.Count == 0) return;

        // 2) 컬럼(x,z)별 삭제된 y좌표 수집
        Dictionary<Vector2Int, List<float>> deletedMap = new();

        foreach (var d in deleted)
        {
            Vector3 wp = d.transform.position;
            Vector2Int key = new Vector2Int(
                Mathf.RoundToInt(wp.x * 10f),
                Mathf.RoundToInt(wp.z * 10f)
            );
            if (!deletedMap.ContainsKey(key))
                deletedMap[key] = new List<float>();
            deletedMap[key].Add(wp.y);
        }

        // 3) 컬럼별로 압축
        foreach (var kv in deletedMap)
        {
            Vector2Int key = kv.Key;
            var deletedYs = kv.Value;

            // 해당 컬럼에서 살아남은 블록 찾기
            var column = survivors.Where(c =>
            {
                Vector3 wp = c.transform.position;
                Vector2Int k = new Vector2Int(
                    Mathf.RoundToInt(wp.x * 10f),
                    Mathf.RoundToInt(wp.z * 10f)
                );
                return k == key;
            }).OrderBy(c => c.transform.position.y).ToList();

            foreach (var c in column)
            {
                Vector3 wp = c.transform.position;

                // 삭제된 y중 c보다 아래에 있는 칸 수
                int fallCount = deletedYs.Count(dy => dy < wp.y);

                if (fallCount <= 0) continue;

                float targetY = wp.y - fallCount;
                Vector3 target = new Vector3(wp.x, targetY, wp.z);

                // 타워에서 기존 위치 제거
                tower.RemoveBlockFromTower(c.GridPosition);

                Debug.DrawLine(wp, target, Color.green, 2f);
                Debug.Log($"[압축] {c.name} {wp.y:F1} -> {targetY:F1} (낙하 {fallCount}칸)");

                state.StartManagedCoroutine?.Invoke(SmoothDropAndSync(c, target, tower));
            }
        }

        // 4) 실제 삭제 및 이펙트 재생
        foreach (var d in deleted)
        {
            // (A) 블록 각 위치마다 파괴 이펙트 생성
            var worldVfx = state.VisualPlayer?.PlayBlockCrumbleEffect(d.transform.position);
            if (worldVfx != null)
                state.RegisterObject?.Invoke(worldVfx);

            tower.RemoveBlockFromTower(d.GridPosition);
            d.DeletBlock();
        }

        // (B) 화면 전체 파괴 이펙트 1회 재생
        var screenVfx = state.VisualPlayer?.PlayDrySeasonScreenEffect();
        if (screenVfx != null)
            state.RegisterObject?.Invoke(screenVfx);

        block.CleanupIfEmpty();
    }

    private IEnumerator SmoothDropAndSync(TetriminoBlockChild child, Vector3 targetWorldPos, TetrisTower tower)
    {
        if (child == null) yield break;

        Vector3 start = child.transform.position;
        float duration = 0.25f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float tLerp = Mathf.Clamp01(elapsed / duration);
            child.transform.position = Vector3.Lerp(start, targetWorldPos, tLerp);
            yield return null;
        }

        child.transform.position = targetWorldPos;

        Vector3 towerOrigin = tower.transform.position;
        Vector3Int newGrid = new Vector3Int(
            Mathf.FloorToInt(targetWorldPos.x - towerOrigin.x + 0.01f),
            Mathf.FloorToInt(targetWorldPos.y - towerOrigin.y + 0.01f),
            Mathf.FloorToInt(targetWorldPos.z - towerOrigin.z + 0.01f)
        );

        child.SetGridPosition(newGrid);
        tower.AddBlockToTower(newGrid, child.BlockType); // 여기 퀘스트받아오는거때문에 수정했는데 문제생기면 수정
    }


    // 11월 : 정보 UI 스모그 효과 (스모그)
    public void ApplySmogOverlay(ObstacleGameState state)
    {
        Debug.Log("스모그 (테두리 시야방해) 효과 실행");

        if (smogInstance == null)
        {
            smogInstance = state.VisualPlayer?.PlaySmogEffect();
            if (smogInstance != null)
                state.RegisterObject?.Invoke(smogInstance);
        }

    }
}
