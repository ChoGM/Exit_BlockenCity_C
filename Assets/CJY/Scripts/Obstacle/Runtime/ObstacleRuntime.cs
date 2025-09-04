using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���� ������ ���ع� ��ü (���� + ȿ��)
/// </summary>
public class ObstacleRuntime
{
    public List<IObstacleCondition> Conditions { get; private set; }
    public List<System.Action<ObstacleGameState>> Effects { get; private set; }

    public ObstacleRuntime(List<IObstacleCondition> conditions, List<System.Action<ObstacleGameState>> effects)
    {
        Conditions = conditions;
        Effects = effects;
    }

    public bool AreConditionsMet(ObstacleGameState state)
    {
        foreach (var cond in Conditions)
        {
            if (!cond.Evaluate(state))
                return false;
        }
        return true;
    }

    public void ExecuteEffects(ObstacleGameState state)
    {
        foreach (var effect in Effects)
            effect(state);
    }

    public void ExecuteSpawnEffects(ObstacleGameState state)
    {
        if (AreConditionsMet(state))
            ExecuteEffects(state); // Spawn �̺�Ʈ �� ����
    }

    public void ExecuteLockEffects(ObstacleGameState state)
    {
        if (AreConditionsMet(state))
            ExecuteEffects(state); // Lock �̺�Ʈ �� ����
    }
}

