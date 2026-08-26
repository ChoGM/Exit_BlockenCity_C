using System;
using System.Collections.Generic;
using UnityEngine;
using TetrisGame;

[Serializable]
public class ShapeWeight
{
    public BlockShapes shape;

    public int weight = 1;
}

[Serializable]
public class StageShapeSetting
{
    [Header("스테이지 번호")]
    public int stage;

    [Header("등장 블럭 및 가중치")]
    public List<ShapeWeight> availableShapes = new();
}