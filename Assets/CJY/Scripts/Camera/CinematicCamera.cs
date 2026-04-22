using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraShot
{
    public Transform movePoint; // 위치와 회전 사용
}

public class CinematicCamera : MonoBehaviour
{
    public CameraShot[] shots;
    public Transform lookTarget; // 전역 타겟 (하나만)

    public float moveSpeed = 3f;
    public float rotateSpeed = 5f;

    private CameraShot currentShot;
    private bool isMoving = false;

    // 추가된 변수: 이동 시작 시점의 총 거리를 기억하기 위함
    private float initialDistance;

    void Start()
    {
        if (shots.Length == 0) return;

        currentShot = shots[0];
        ApplyImmediate();

        if (shots.Length > 1)
            PlayShot(1);
    }

    void LateUpdate()
    {
        if (currentShot == null) return;

        Move();
        Rotate();
    }

    public void PlayShot(int index)
    {
        if (index < 0 || index >= shots.Length) return;

        currentShot = shots[index];
        isMoving = true;

        // 이동 시작할 때 현재 위치와 목표 위치 사이의 총 거리를 저장
        if (currentShot.movePoint != null)
        {
            initialDistance = Vector3.Distance(transform.position, currentShot.movePoint.position);
        }
    }

    void Move()
    {
        if (!isMoving || currentShot.movePoint == null) return;

        transform.position = Vector3.Lerp(
            transform.position,
            currentShot.movePoint.position,
            Time.deltaTime * moveSpeed
        );

        float dist = Vector3.Distance(transform.position, currentShot.movePoint.position);

        if (dist < 0.01f)
        {
            isMoving = false;
        }
    }

    void Rotate()
    {
        if (currentShot.movePoint == null) return;

        Quaternion targetRot;

        // 타겟이 있다면, 이동 진행도(남은 거리)에 따라 회전값을 자연스럽게 섞어줌.
        if (lookTarget != null)
        {
            // 1. 타겟을 바라보는 회전값
            Vector3 dir = lookTarget.position - transform.position;
            Quaternion lookAtRot = dir.sqrMagnitude > 0.001f ? Quaternion.LookRotation(dir) : transform.rotation;

            // 2. 최종 도착 지점의 회전값
            Quaternion finalRot = currentShot.movePoint.rotation;

            // 3. 남은 거리에 따른 비율(0~1) 계산
            float currentDist = Vector3.Distance(transform.position, currentShot.movePoint.position);
            float progress = initialDistance > 0.01f ? (currentDist / initialDistance) : 0f;

            // 4. 거리가 멀면(progress=1) 타겟을 보고, 가까워지면(progress=0) 최종 회전값으로 자연스럽게 전환
            targetRot = Quaternion.Slerp(finalRot, lookAtRot, progress);
        }
        else
        {
            // 타겟이 없을 때는 그냥 목표 위치의 회전을 사용
            targetRot = currentShot.movePoint.rotation;
        }

        // 최종 계산된 방향으로 부드럽게 카메라 돌리기
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * rotateSpeed
        );
    }

    void ApplyImmediate()
    {
        if (currentShot.movePoint != null)
        {
            transform.position = currentShot.movePoint.position;
            transform.rotation = currentShot.movePoint.rotation;
        }
    }
}