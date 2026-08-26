using System;
using System.Collections;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    [Serializable]
    public class VisualizerPattern
    {
        public float[] values;
    }

    [Header("References")]
    [SerializeField] private Transform[] bars;

    [Header("Animation Settings")]
    [SerializeField] private float playTime = 3f;
    [SerializeField] private float animationInterval = 0.2f;
    [SerializeField] private float transitionDuration = 0.15f;

    [Header("Height Settings")]
    [SerializeField] private float amplification = 0.1f;
    [SerializeField] private float baseHeight = 0.05f;

    [Header("Random Weight")]
    [SerializeField] private float minRandomWeight = 0.9f;
    [SerializeField] private float maxRandomWeight = 1.1f;

    [Header("Patterns")]
    [SerializeField] private VisualizerPattern[] patterns;

    private Coroutine visualizerCoroutine;
    private Coroutine resetCoroutine;
    private Coroutine autoStopCoroutine;

    private float[] startHeights;
    private float[] targetHeights;

    private int currentPatternIndex;

    public void Play()
    {
        if (visualizerCoroutine != null)
            return;

        if (bars == null || bars.Length == 0)
            return;

        if (patterns == null || patterns.Length == 0)
            return;

        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        InitializeArrays();

        visualizerCoroutine = StartCoroutine(VisualizerRoutine());
    }

    public void Stop()
    {
        if (visualizerCoroutine != null)
        {
            StopCoroutine(visualizerCoroutine);
            visualizerCoroutine = null;
        }

        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }

        resetCoroutine = StartCoroutine(ResetBarsRoutine());
    }

    public void AutoStop()
    {
        if (autoStopCoroutine != null)
        {
            StopCoroutine(autoStopCoroutine);
        }

        Play();

        autoStopCoroutine = StartCoroutine(AutoStopRoutine());
    }

    private void InitializeArrays()
    {
        if (startHeights == null ||
            startHeights.Length != bars.Length)
        {
            startHeights = new float[bars.Length];
            targetHeights = new float[bars.Length];
        }
    }

    private IEnumerator VisualizerRoutine()
    {
        while (true)
        {
            VisualizerPattern pattern = patterns[currentPatternIndex];

            if (pattern.values != null && pattern.values.Length > 0)
            {
                SetTargetHeights(pattern);
                yield return StartCoroutine(TransitionBarsRoutine());
            }

            currentPatternIndex = (currentPatternIndex + 1) % patterns.Length;
            yield return new WaitForSeconds(animationInterval);
        }
    }

    private void SetTargetHeights(VisualizerPattern pattern)
    {
        for (int i = 0; i < bars.Length; i++)
        {
            if (bars[i] == null)
                continue;

            startHeights[i] =  bars[i].localScale.y;

            int valueIndex = i % pattern.values.Length;

            float randomWeight = UnityEngine.Random.Range(minRandomWeight, maxRandomWeight);

            targetHeights[i] = pattern.values[valueIndex] * randomWeight * amplification + baseHeight;
        }
    }

    private IEnumerator TransitionBarsRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;

            float t =
                transitionDuration > 0f
                    ? Mathf.Clamp01(elapsedTime / transitionDuration)
                    : 1f;

            for (int i = 0; i < bars.Length; i++)
            {
                if (bars[i] == null)
                    continue;

                Vector3 scale = bars[i].localScale;

                scale.y = Mathf.Lerp(startHeights[i], targetHeights[i], t);

                bars[i].localScale = scale;
            }

            yield return null;
        }

        ApplyTargetHeights();
    }

    private IEnumerator AutoStopRoutine()
    {
        yield return new WaitForSeconds(playTime);
        Stop();

        autoStopCoroutine = null;
    }

    private void ApplyTargetHeights()
    {
        for (int i = 0; i < bars.Length; i++)
        {
            if (bars[i] == null)
                continue;

            Vector3 scale = bars[i].localScale;

            scale.y = targetHeights[i];

            bars[i].localScale = scale;
        }
    }

    private IEnumerator ResetBarsRoutine()
    {
        InitializeArrays();

        for (int i = 0; i < bars.Length; i++)
        {
            if (bars[i] == null)
                continue;

            startHeights[i] = bars[i].localScale.y;
        }

        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = transitionDuration > 0f
                    ? Mathf.Clamp01(elapsedTime / transitionDuration)
                    : 1f;

            for (int i = 0; i < bars.Length; i++)
            {
                if (bars[i] == null)
                    continue;

                Vector3 scale = bars[i].localScale;

                scale.y = Mathf.Lerp(startHeights[i], baseHeight, t);

                bars[i].localScale = scale;
            }

            yield return null;
        }

        ResetBarsImmediately();
        resetCoroutine = null;
    }

    private void ResetBarsImmediately()
    {
        if (bars == null)
            return;

        for (int i = 0; i < bars.Length; i++)
        {
            if (bars[i] == null)
                continue;

            Vector3 scale = bars[i].localScale;
            scale.y = baseHeight;
            bars[i].localScale = scale;
        }
    }

    private void OnDisable()
    {
        if (visualizerCoroutine != null)
        {
            StopCoroutine(visualizerCoroutine);
            visualizerCoroutine = null;
        }

        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        ResetBarsImmediately();
    }
}