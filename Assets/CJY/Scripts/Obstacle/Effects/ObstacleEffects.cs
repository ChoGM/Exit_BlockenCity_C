using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleEffects : MonoBehaviour
{
    [Header("������ ��� (ȸ�� ����)")]
    [SerializeField] private float freezeChance = 0.9f;

    [Header("�ܼ� (�Է� ����)")]
    [SerializeField] private float inputDelayTime = 0.5f;

    [Header("��ǳ (��� �и�)")]
    [SerializeField] private float windDuration = 3f;
    [SerializeField] private float windInterval = 0.3f;
    [SerializeField] private Vector2 windWaitIntervalRange = new Vector2(2f, 5f);

    [Header("Ȳ�� (�þ� ����)")]
    [SerializeField] private Vector2 dustSpawnIntervalRange = new Vector2(0.5f, 3f);
    [SerializeField] private Vector2 dustLifetimeRange = new Vector2(1f, 2f);

    [Header("���� (���� �Ͻ� ����)")]
    [SerializeField] private Vector2 lightningIntervalRange = new Vector2(2f, 5f);
    [SerializeField] private float lightningStopTime = 1.0f;

    [Header("�帶 (���� �ӵ� ����)")]
    [SerializeField] private float rainDropSpeedMultiplier = 0.5f; // �⺻ �ӵ��� 50%

    [Header("�Ǳ� (��� �ı� Ȯ��)")]
    [SerializeField] private float BreakBlockChance = 0.5f;

    // 1�� : ȸ�� ���� (������ ���)
    public void TryFreezeBlock(ObstacleGameState state)
    {
        Debug.Log("������ ���(ȸ������) ȿ�� ����");

        TetriminoBlock block = state.LockedBlock; // ��ġ ��� ����
        if (block == null) return;

        if (UnityEngine.Random.value <= freezeChance)
        {
            Debug.Log("ȸ������ ȿ��");
            state.InputBlocker.blockRotation = true;
            state.VisualPlayer?.VisualFreezeBlock(block);
        }
    }


    // 2�� : ��� �и� (��ǳ)
    public void PushBlockRandomly(ObstacleGameState state)
    {
        Debug.Log("��ǳ (��� �и�) ȿ�� ����");
        if (state == null || state.SpawnedBlock == null)  // �� SpawnedBlock ���
        {
            //Debug.LogWarning("[ObstacleEffects] PushBlockRandomly ȣ�� �� state �Ǵ� SpawnedBlock�� null�Դϴ�.");
            return;
        }

        // �ڷ�ƾ �� ���� �����ϸ� WindPushLoop�� �ݺ� ����
        state.StartManagedCoroutine?.Invoke(WindPushLoop(state));
    }

    private IEnumerator WindPushLoop(ObstacleGameState state)
    {
        if (state.SpawnedBlock == null) yield break;  // �� SpawnedBlock ���

        // �ٶ� ���� ����
        Vector3 dir = UnityEngine.Random.value > 0.5f ? Vector3.left : Vector3.right;
        Debug.Log($"[Wind] �ٶ� ����! ����={dir}, ����={windDuration}s");

        float timer = 0f;
        var obj = state.VisualPlayer?.PlayStrongWindEffect();
        if (obj != null)
        {
            state.RegisterObject?.Invoke(obj);
            Destroy(obj, windDuration);
        }

        // �ٶ� ���� �ð� ���� ��� �̵�
        while (timer < windDuration && state.SpawnedBlock != null)  // �� SpawnedBlock ���
        {
            state.TetrisController?.MoveBlockByWind(dir);
            yield return new WaitForSeconds(windInterval);
            timer += windInterval;
        }

        Debug.Log("[Wind] �ٶ� ����");
    }



    // 3�� : �Է� ���� (�ܼ�)
    public void InputDelay(ObstacleGameState state)
    {
        Debug.Log("�ܼ� �Է� ���� ȿ�� ����");

        var blocker = state.InputBlocker;
        if (blocker == null) return;

        blocker.SetInputProcessDelay(inputDelayTime);

        var obj = state.VisualPlayer?.SnowfallEffect();
        if (obj != null) state.RegisterObject?.Invoke(obj);
    }

    // 4~5�� : �þ� ���� (Ȳ��)
    public void ApplyDustStormEffect(ObstacleGameState state)
    {
        Debug.Log("Ȳ�� (�þ� ����) ȿ�� ����");

        var blocker = state.InputBlocker;
        if (blocker == null) return;

        blocker.StartCoroutine(DustStormLoop(state.VisualPlayer));
    }

    private IEnumerator DustStormLoop(EffectVisualPlayer visualPlayer)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(dustSpawnIntervalRange.x, dustSpawnIntervalRange.y));

            var instance = visualPlayer?.DustStormEffect();
            if (instance != null)
                Destroy(instance, Random.Range(dustLifetimeRange.x, dustLifetimeRange.y));
        }
    }

    // 6�� : ���� �Ͻ� ���� (����)
    public void DisableControlTemporary(ObstacleGameState state)
    {
        Debug.Log("���� (���� �Ͻ� ����) ȿ�� ����");

        var blocker = state.InputBlocker;
        if (blocker == null) return;

        state.StartManagedCoroutine?.Invoke(LightningStormLoop(blocker, state.VisualPlayer));
    }

    private IEnumerator LightningStormLoop(InputBlocker blocker, EffectVisualPlayer visualPlayer)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(lightningIntervalRange.x, lightningIntervalRange.y));

            //Debug.Log("(�ڷ�ƾ)���� �Ͻ� ����");
            blocker.isInputBlocked = true;

            var obj = visualPlayer?.PlayLightningEffect();
            if (obj != null)
                Destroy(obj, lightningStopTime); // ���� ���� �ð� �� ����
            

            yield return new WaitForSeconds(lightningStopTime); // �ν��Ͻ� �ʵ� ����

            blocker.isInputBlocked = false;
        }
    }

    // 7�� : �����̽� �Ұ���
    public void DisableSpace(ObstacleGameState state)
    {
        Debug.Log("�帶 (�����̽��Ұ���) ȿ�� ����");

        var blocker = state.InputBlocker;
        if (blocker == null) return;

        blocker.blockHardDrop = true;

        var obj = state.VisualPlayer?.PlayRainEffect();
        if (obj != null) state.RegisterObject?.Invoke(obj);
    }

    // 7�� : ���ϼӵ� ���� (�帶)
    public void SlowDropSpeed(ObstacleGameState state)
    {
        Debug.Log("�帶 (���ϼӵ�����) ȿ�� ����");
        if (state == null) return;

        state.CurrentDropSpeed = rainDropSpeedMultiplier;
    }

    // 8�� : ���� ��� UI ��Ȱ��ȭ (����)
    public void HideNextBlockUI(ObstacleGameState state)
    {
        Debug.Log("���� (����UI ��Ȱ��ȭ) ȿ�� ����");

        var obj = state.VisualPlayer?.PlayOverheatWarning("���� ��� ����!");
        if (obj != null) state.RegisterObject?.Invoke(obj);
    }

    // 10�� : ��� �ı� Ȯ�� (�Ǳ�)
    public void BreakBlockOnPlace(ObstacleGameState state)
    {
        Debug.Log("�Ǳ� (����ı�) ȿ�� ����");

        var block = state.LockedBlock;  // �� ��ġ�� ��� ����
        if (block == null) return;

        var children = block.GetComponentsInChildren<TetriminoBlockChild>();
        if (children.Length == 0) return;

        bool anyDeleted = false; // ĭ�� �ϳ��� �����ƴ��� üũ

        foreach (var child in children)
        {
            if (child != null && UnityEngine.Random.value <= BreakBlockChance) // 50% Ȯ��
            {
                child.DeletBlock();
                anyDeleted = true;
            }
        }

        // ĭ�� �ϳ��� ������ ��쿡�� ����Ʈ ����
        if (anyDeleted)
        {
            state.VisualPlayer.PlayBlockCrumbleEffect(block.transform.position);
        }

        // Ȥ�� ����� �� �μ������� �����⵵ ����
        block.CleanupIfEmpty();
    }

    // 11�� : ���� UI ����� ȿ�� (�����)
    public void ApplySmogOverlay(ObstacleGameState state)
    {
        Debug.Log("����� (�׵θ� �þ߹���) ȿ�� ����");

        var obj = state.VisualPlayer?.PlaySmogEffect();
        if (obj != null) state.RegisterObject?.Invoke(obj);
    }
}
