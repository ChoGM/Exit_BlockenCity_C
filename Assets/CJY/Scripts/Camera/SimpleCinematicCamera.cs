using UnityEngine;
public enum CameraDir
{
    Front = 0,
    Right = 1,
    Back = 2,
    Left = 3
}

public class SimpleCinematicCamera : MonoBehaviour
{
    [Header("Tetris Controller")]
    public TetrisController tetrisController;

    [Header("Camera")]
    public Camera targetCamera;

    [Header("Camera Movement")]
    public float moveSpeed = 2f;

    public float rotateSpeed = 2f;

    public CameraDir currentDir = CameraDir.Front;

    private Transform[] currentCameraPoints;

    private void Start()
    {
        ApplyCameraSetting();

        ApplyImmediate();

        NotifyDirChanged();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused())
            return;

        HandleInput();

        MoveCamera();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            RotateLeft();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            RotateRight();
        }
    }

    private void RotateLeft()
    {
        currentDir = (CameraDir) (((int)currentDir + 3) % 4);

        Debug.Log($"카메라 왼쪽 회전 : {currentDir}");

        if (tetrisController != null)
        {
            tetrisController.SetCameraDir(currentDir);
        }
    }

    private void RotateRight()
    {
        currentDir = (CameraDir) (((int)currentDir + 1) % 4);

        Debug.Log($"카메라 오른쪽 회전 : {currentDir}");

        if (tetrisController != null)
        {
            tetrisController.SetCameraDir(currentDir);
        }
    }

    private void ApplyCameraSetting()
    {
        if (TetrisManager.Instance == null)
        {
            Debug.LogError("[Camera] " +"TetrisManager.Instance가 없습니다.");
            return;
        }

        StageSettingPreset preset = TetrisManager.Instance.CurrentSettingPreset;

        if (preset == null)
        {
            Debug.LogError("[Camera] " + "현재 StageSettingPreset이 없습니다.");
            return;
        }

        currentCameraPoints = preset.GetCameraPoints();

        if (currentCameraPoints == null || currentCameraPoints.Length < 4)
        {
            Debug.LogError("[Camera] " + "Camera Point 설정이 올바르지 않습니다.");
            return;
        }

        if (targetCamera != null)
        {
            targetCamera.orthographicSize = preset.orthographicSize;
        }

        Debug.Log(
            $"[Camera] " +
            $"Preset {preset.presetID} 적용\n" +
            $"Orthographic Size : " +
            $"{preset.orthographicSize}"
        );
    }

    private void MoveCamera()
    {
        if (currentCameraPoints == null)
        {
            return;
        }

        if (currentCameraPoints.Length < 4)
        {
            return;
        }

        Transform target = currentCameraPoints[(int)currentDir];

        if (target == null)
        {
            Debug.LogWarning(
                $"[Camera] " +
                $"Camera Point가 비어있습니다. " +
                $"Direction : {currentDir}"
            );
            return;
        }

        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * moveSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, Time.deltaTime * rotateSpeed);
    }

    private void ApplyImmediate()
    {
        if (currentCameraPoints == null)
        {
            return;
        }

        if (currentCameraPoints.Length < 4)
        {
            return;
        }

        Transform target =currentCameraPoints[(int)currentDir];

        if (target == null)
        {
            return;
        }

        transform.position = target.position;
        transform.rotation = target.rotation;
    }

    private void NotifyDirChanged()
    {
        if (tetrisController != null)
        {
            tetrisController.SetCameraDir(currentDir);
        }
    }
}