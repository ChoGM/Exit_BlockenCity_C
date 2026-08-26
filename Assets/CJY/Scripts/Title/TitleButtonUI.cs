using UnityEngine;
using UnityEngine.EventSystems;

public class TitleButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject arrow;

    private void Awake()
    {
        if (arrow != null)
            arrow.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (arrow != null)
            arrow.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (arrow != null)
            arrow.SetActive(false);
    }
}