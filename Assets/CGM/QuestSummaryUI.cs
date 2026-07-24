using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestSummaryUI : MonoBehaviour
{
    public TMP_Text questName;
    public Slider progressSlider;

    public void SetData(string name, float value, float maxValue)
    {
        questName.text = name;

        progressSlider.maxValue = Mathf.Max(1, maxValue);
        progressSlider.value = value;
    }
}