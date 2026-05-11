using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractionController : MonoBehaviour
{
    public float interactDistance = 10f;
    public LayerMask interactableLayer;
    private IInteractable currentTarget;

    void Update()
    {
        // UI를 클릭 중일 때는 레이캐스트 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (interactable != currentTarget)
                {
                    if (currentTarget != null) currentTarget.OnHoverExit();
                    currentTarget = interactable;
                    currentTarget.OnHoverEnter();
                }

                if (Input.GetMouseButtonDown(0))
                {
                    currentTarget.OnClick();
                }
            }
        }
        else
        {
            if (currentTarget != null)
            {
                currentTarget.OnHoverExit();
                currentTarget = null;
            }
        }
    }
}