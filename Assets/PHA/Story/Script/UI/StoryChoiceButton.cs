using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryChoiceButton : MonoBehaviour
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private TMP_Text choiceText;

    private Action onClicked;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        button.onClick.AddListener(HandleClick);
    }

    public void Initialize(
        string text,
        Action clickAction)
    {
        choiceText.text = text;
        onClicked = clickAction;

        button.interactable = true;
    }

    private void HandleClick()
    {
        if (!button.interactable)
        {
            return;
        }

        button.interactable = false;
        onClicked?.Invoke();
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }
}