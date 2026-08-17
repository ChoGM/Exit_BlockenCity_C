using UnityEngine;

[System.Serializable]
public class StageSettingPreset
{
    [Header("Preset ID")]
    public int presetID;

    [Header("Tower Size")]
    public Vector3Int towerSize = new Vector3Int(4, 8, 4);

    [Header("Tower Layout")]
    public Vector3 layoutPosition = Vector3.zero;

    public Vector3 layoutScale = Vector3.one;

    [Header("Camera Points")]
    public Transform front;
    public Transform right;
    public Transform back;
    public Transform left;

    [Header("Camera Orthographic Size")]
    public float orthographicSize = 6f;

    public Transform[] GetCameraPoints()
    {
        return new Transform[]
        {
            front,
            right,
            back,
            left
        };
    }
}