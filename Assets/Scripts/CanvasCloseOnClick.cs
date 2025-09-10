using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasCloseOnClick : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        Debug.Log("�ִϸ����͸� �����Խ��ϴ�.");
    }

    private void OnEnable()
    {
        if (animator != null)
        {
            animator.Rebind();
            Debug.Log("Ʈ���Ŵ� �⺻ �����Դϴ�.");
        }
    }

    public void PlayCloseAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("IsClose");
            Debug.Log("IsClose�� Ʈ�����մϴ�.");
        }
    }

    public void DeactivateSelf()
    {
        gameObject.SetActive(false);
        Debug.Log("Active�� false�� �Ǿ����ϴ�.");
    }
}
