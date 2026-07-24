using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpecialQuestSpawner : MonoBehaviour
{
    [Header("Timer Slider")]
    public Slider timeSlider;

    [Header("Status Sliders (A,B,C)")]
    public Slider sliderA;
    public Slider sliderB;
    public Slider sliderC;

    [Header("Spawn Ratio Range (0~1)")]
    [Range(0f, 1f)] public float minSpawnRatio = 0.2f;
    [Range(0f, 1f)] public float maxSpawnRatio = 0.8f;

    private float nextSpawnRatio;
    private bool hasSpawnedThisCycle = false;

    private bool cycleStarted = false;

    void Start()
    {
        SetNextSpawnRatio();
    }

    void Update()
    {
        if (timeSlider == null) return;

        if (DialogUI.Instance != null && DialogUI.Instance.IsDialogRunning)
            return;

        float ratio = 1f - (timeSlider.value / timeSlider.maxValue);

        if (!cycleStarted && ratio > 0.01f)
        {
            cycleStarted = true;
            hasSpawnedThisCycle = false;
            nextSpawnRatio = Random.Range(minSpawnRatio, maxSpawnRatio);

            Debug.Log($"새 스폰 타이밍: {nextSpawnRatio}");
        }

        if (ratio > 0.99f)
        {
            cycleStarted = false;
        }

        if (cycleStarted && !hasSpawnedThisCycle && ratio >= nextSpawnRatio)
        {
            SpawnSpecialQuest();
            hasSpawnedThisCycle = true;
        }
    }

    void SetNextSpawnRatio()
    {
        nextSpawnRatio = Random.Range(minSpawnRatio, maxSpawnRatio);
    }

    int SliderToDigit(Slider slider)
    {
        if (slider == null) return 0;
        float t = Mathf.InverseLerp(slider.minValue, slider.maxValue, slider.value);
        return Mathf.Clamp(Mathf.RoundToInt(t * 9f), 0, 9);
    }

    void SpawnSpecialQuest()
    {
        int month = Datamanager.Instance.saveData.progress.currentStage; ;  // 1~12

        int a = Datamanager.Instance.saveData.friendlinessData.DanWol;
        int b = Datamanager.Instance.saveData.friendlinessData.HongNyeonGwi;
        int c = Datamanager.Instance.saveData.friendlinessData.YaSeo;
        int d = Datamanager.Instance.saveData.friendlinessData.JeonSangYeon;
        int e = Datamanager.Instance.saveData.friendlinessData.MaCheonGyo;

        int generatedID = month * 100000 + a * 10000 + b * 1000 + c * 100 + d * 10 + e;

        Debug.Log($"생성된 QuestID → {generatedID}");

        int finalID = FindBestBranchID(generatedID);

        Debug.Log($"최종 QuestID → {finalID}");

        DialogUI.Instance.StartDialog(finalID);

        SetNextSpawnRatio();
    }

    // ⭐ 자리별 차이 기반 가장 가까운 ID 찾기
    int FindBestBranchID(int targetID)
    {
        List<int> allBranch = DialogManager.Instance.GetSpawnableBranchIDs();

        if (allBranch.Contains(targetID))
            return targetID;

        int bestID = -1;
        int bestScore = int.MaxValue;

        // 목표 ID 분리
        int tMonth = targetID / 100000;
        int tA = (targetID / 10000) % 10;
        int tB = (targetID / 1000) % 10;
        int tC = (targetID / 100) % 10;
        int tD = (targetID / 10) % 10;
        int tE = targetID % 10;

        foreach (int id in allBranch)
        {
            // 월이 다르면 제외
            if (id / 100000 != tMonth)
                continue;

            int iA = (id / 10000) % 10;
            int iB = (id / 1000) % 10;
            int iC = (id / 100) % 10;
            int iD = (id / 10) % 10;
            int iE = id % 10;

            int score =
                Mathf.Abs(tA - iA) +
                Mathf.Abs(tB - iB) +
                Mathf.Abs(tC - iC) +
                Mathf.Abs(tD - iD) +
                Mathf.Abs(tE - iE);

            if (score < bestScore || (score == bestScore && (bestID == -1 || id < bestID)))
            {
                bestScore = score;
                bestID = id;
            }
        }

        return bestID;
    }
}