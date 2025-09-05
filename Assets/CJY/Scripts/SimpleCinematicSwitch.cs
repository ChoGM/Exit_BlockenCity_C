using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCinematicCamera : MonoBehaviour
{
    public Transform pointA;     // ��ġ A
    public Transform pointB;     // ��ġ B
    public Transform lookTarget; // ȸ���ؼ� �ٶ� ��ǥ

    public float moveSpeed = 2f;     // ��ġ �̵� �ӵ�
    public float rotateSpeed = 2f;   // ȸ�� �ӵ�

    private bool atPointA = true;

    void Update()
    {
        // ������ ��ġ ��ȯ
        if (Input.GetKeyDown(KeyCode.P))
        {
            atPointA = !atPointA;
        }

        // ��ǥ ��ġ ���
        Transform targetPoint = atPointA ? pointA : pointB;

        // ��ġ �̵� (�ε巴��)
        transform.position = Vector3.Lerp(
            transform.position,
            targetPoint.position,
            Time.deltaTime * moveSpeed
        );

        // ȸ��: ��ǥ ������Ʈ�� �ٶ󺸵��� ����
        if (lookTarget != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookTarget.position - transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotateSpeed
            );
        }
    }
}
