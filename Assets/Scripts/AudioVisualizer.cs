using System.Collections;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform[] bars;

    [Header("Fake Visualizer Settings")]
    [SerializeField] private float animationInterval = 0.15f;
    [SerializeField] private float amplification = 0.1f;
    [SerializeField] private float baseHeight = 0.05f;

    [Header("Patterns")]
    [SerializeField]
    private float[] pattern =
    {
        1f, 4f, 3f, 2f, 5f, 6f
    };

    private Coroutine visualizerCoroutine;

    public void Play()
    {
        if (visualizerCoroutine != null)
            return;

        visualizerCoroutine = StartCoroutine(VisualizerRoutine());
    }

    public void Stop()
    {
        if (visualizerCoroutine == null)
            return;

        StopCoroutine(visualizerCoroutine);
        visualizerCoroutine = null;

        ResetBars();
    }

    private IEnumerator VisualizerRoutine()
    {
        int patternOffset = 0;

        while (true)
        {
            UpdateBars(patternOffset);

            patternOffset =
                (patternOffset + 1)
                % pattern.Length;

            yield return new WaitForSeconds(animationInterval);
        }
    }

    private void UpdateBars(int patternOffset)
    {
        for (int i = 0; i < bars.Length; i++)
        {
            int patternIndex = (i + patternOffset) % pattern.Length;
            float value = pattern[patternIndex];
            Vector3 scale = bars[i].localScale;
            scale.y = value * amplification + baseHeight;
            bars[i].localScale = scale;
        }
    }

    private void ResetBars()
    {
        for (int i = 0; i < bars.Length; i++)
        {
            Vector3 scale = bars[i].localScale;
            scale.y = baseHeight;
            bars[i].localScale = scale;
        }
    }
}