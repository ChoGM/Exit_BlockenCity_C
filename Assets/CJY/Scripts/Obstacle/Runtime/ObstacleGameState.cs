using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���ع� ���� �� �� ȿ�� ������ ���� �ּ����� ���� ���� ����
/// </summary>
public class ObstacleGameState
{
    public ObstacleType SelectedObstacle { get; private set; }
    public EffectVisualPlayer VisualPlayer { get; private set; }
    public InputBlocker InputBlocker { get; private set; }
    public TetrisController TetrisController { get; private set; }

    // ��� �и�
    public TetriminoBlock SpawnedBlock { get; set; }  // ���� ���� ���
    public TetriminoBlock LockedBlock { get; set; }   // ��ġ ���� ���

    public float CurrentDropSpeed { get; set; }

    public System.Action<Coroutine> RegisterCoroutine;
    public System.Action<GameObject> RegisterObject;
    public Func<IEnumerator, Coroutine> StartManagedCoroutine;

    public ObstacleGameState(ObstacleType selectedObstacle,
                             EffectVisualPlayer visualPlayer,
                             InputBlocker inputBlocker,
                             TetrisController tetrisController)
    {
        SelectedObstacle = selectedObstacle;
        VisualPlayer = visualPlayer;
        InputBlocker = inputBlocker;
        TetrisController = tetrisController;
        CurrentDropSpeed = 1f;
    }

    public void UpdateSelectedObstacle(ObstacleType type)
    {
        SelectedObstacle = type;
    }

    public bool CheckCondition(ObstacleType required)
    {
        return SelectedObstacle == required;
    }
}
