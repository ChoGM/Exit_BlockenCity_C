using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleFactory
{
    private readonly Dictionary<string, System.Action<ObstacleGameState>> _effectTable;

    // ObstacleEffects �ν��Ͻ��� ���Թ޾Ƽ� effectTable�� ����
    public ObstacleFactory(ObstacleEffects effectsInstance)
    {
        _effectTable = ObstacleEffectMap.Create(effectsInstance);
    }

    public List<ObstacleRuntime> CreateAllObstacles()
    {
        var obstacles = new List<ObstacleRuntime>();

        // ������ ��� - ȸ�� ����
        obstacles.Add(new ObstacleRuntime(
            new List<IObstacleCondition> { new Condition_ObstacleType(ObstacleType.FrozenBlock) },
            new List<System.Action<ObstacleGameState>> { _effectTable["DisableRotation"] }
        ));

        // ��ǳ - ��� �и�
        obstacles.Add(new ObstacleRuntime(
            new List<IObstacleCondition> { new Condition_ObstacleType(ObstacleType.StrongWind) },
            new List<System.Action<ObstacleGameState>> { _effectTable["PushBlockRandomly"] }
        ));

        // �ܼ� - �Է� ����
        obstacles.Add(new ObstacleRuntime(
            new List<IObstacleCondition> { new Condition_ObstacleType(ObstacleType.SnowDelay) },
            new List<System.Action<ObstacleGameState>> { _effectTable["InputDelay"] }
        ));

        // Ȳ�� - �þ� ����
        obstacles.Add(new ObstacleRuntime(
            new List<IObstacleCondition> { new Condition_ObstacleType(ObstacleType.Sandstorm) },
            new List<System.Action<ObstacleGameState>> { _effectTable["ApplyDustStormEffect"] }
        ));
        

        // ���� - ���� �Ͻ� ����
        obstacles.Add(new ObstacleRuntime(
            new List<IObstacleCondition> { new Condition_ObstacleType(ObstacleType.Thunder) },
            new List<System.Action<ObstacleGameState>> { _effectTable["DisableControlTemporary"] }
        ));

        // �帶 - �����̽� ���� + ���� �ӵ� ����
        obstacles.Add(new ObstacleRuntime(
            new List<IObstacleCondition> { new Condition_ObstacleType(ObstacleType.Rainy) },
            new List<System.Action<ObstacleGameState>> {
                _effectTable["DisableSpace"],
                _effectTable["SlowDropSpeed"]
            }
        ));

        // ���� - ���� ��� UI �����
        obstacles.Add(new ObstacleRuntime(
            new List<IObstacleCondition> { new Condition_ObstacleType(ObstacleType.Heatwave) },
            new List<System.Action<ObstacleGameState>> { _effectTable["HideNextBlockUI"] }
        ));

        // �Ǳ� - ��� �ı� Ȯ��
        obstacles.Add(new ObstacleRuntime(
            new List<IObstacleCondition> { new Condition_ObstacleType(ObstacleType.Drought) },
            new List<System.Action<ObstacleGameState>> { _effectTable["BreakBlockOnPlace"] }
        ));

        // ����� - UI ����� ��������
        obstacles.Add(new ObstacleRuntime(
            new List<IObstacleCondition> { new Condition_ObstacleType(ObstacleType.Smog) },
            new List<System.Action<ObstacleGameState>> { _effectTable["ApplySmogOverlay"] }
        ));

        return obstacles;
    }
}
