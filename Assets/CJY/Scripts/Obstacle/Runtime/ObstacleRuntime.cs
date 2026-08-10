using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 실행 가능한 방해물 객체 (조건 + 효과)
/// </summary>
/// 
public enum ObstacleTriggerType
{
    OnStart,   // 월 시작 / 방해물 확정 시 1회
    OnSpawn,   // 블록 스폰 시
    OnLock    // 블록 고정 시
}

public class ObstacleRuntime
{
    public List<IObstacleCondition> Conditions { get; private set; }

    // trigger별 effect 분리
    private Dictionary<ObstacleTriggerType, List<Action<ObstacleGameState>>> effectsByTrigger;

    public ObstacleRuntime(
        List<IObstacleCondition> conditions,
        Dictionary<ObstacleTriggerType, List<Action<ObstacleGameState>>> effects)
    {
        Conditions = conditions;
        effectsByTrigger = effects;
    }

    public void Execute(ObstacleTriggerType trigger, ObstacleGameState state)
    {
        if (!AreConditionsMet(state)) return;
        if (!effectsByTrigger.TryGetValue(trigger, out var effects)) return;

        // [추가] OnStart 시점에 싱글톤에서 현재 방해물 타입을 가져와 사운드 재생
        if (trigger == ObstacleTriggerType.OnStart)
        {
            var selected = GameObstacleSystem.Instance?.GetSelectedObstacle();
            if (selected != null)
            {
                ObstacleSoundManager.Instance?.PlayObstacleSound(selected.type);

                Debug.Log("방해물 소리 재생 시도: " + selected.type);
            }
        }

        foreach (var effect in effects)
            effect(state);
    }

    private bool AreConditionsMet(ObstacleGameState state)
    {
        foreach (var cond in Conditions)
            if (!cond.Evaluate(state))
                return false;
        return true;
    }
}


