using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpecialQuestSpawner : MonoBehaviour
{
    [Header("Timer Slider")]
    public Slider timeSlider;

    [Header("첫 번째 퀘스트")]
    public float firstMin = 0.05f;
    public float firstMax = 0.01f;

    [Header("두 번째 퀘스트")]
    public float secondMin = 0.2f;
    public float secondMax = 0.25f;

    private float firstSpawnRatio;
    private float secondSpawnRatio;

    private bool firstSpawned = false;
    private bool secondSpawned = false;

    private bool cycleStarted = false;

    private float danWolWeight = 0.9f;
    private float hongNyeonWeight = 0.95f;
    private float yaSeoWeight = 1f;
    private float jeonSangWeight = 1.05f;
    private float maCheonWeight = 1.1f;

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

        // 새로운 사이클 시작
        if (!cycleStarted && ratio > 0.01f)
        {
            cycleStarted = true;

            firstSpawned = false;
            secondSpawned = false;

            SetNextSpawnRatio();

            Debug.Log($"첫 번째 스폰 : {firstSpawnRatio}");
            Debug.Log($"두 번째 스폰 : {secondSpawnRatio}");
        }

        // 사이클 종료
        if (ratio > 0.99f)
        {
            cycleStarted = false;
        }

        // 첫 번째 퀘스트
        if (cycleStarted && !firstSpawned && ratio >= firstSpawnRatio)
        {
            SpawnSpecialQuest(0);
            firstSpawned = true;
        }

        // 두 번째 퀘스트
        if (cycleStarted && !secondSpawned && ratio >= secondSpawnRatio)
        {
            SpawnSpecialQuest(1);
            secondSpawned = true;
        }
    }

    void SetNextSpawnRatio()
    {
        firstSpawnRatio = Random.Range(firstMin, firstMax);

        do
        {
            secondSpawnRatio = Random.Range(secondMin, secondMax);
        }
        while (Mathf.Abs(secondSpawnRatio - firstSpawnRatio) < 0.1f);
    }

    void SpawnSpecialQuest(int rank)
    {
        int month = Datamanager.Instance.saveData.progress.currentStage;

        int a = Datamanager.Instance.saveData.friendlinessData.DanWol;
        int b = Datamanager.Instance.saveData.friendlinessData.HongNyeonGwi;
        int c = Datamanager.Instance.saveData.friendlinessData.YaSeo;
        int d = Datamanager.Instance.saveData.friendlinessData.JeonSangYeon;
        int e = Datamanager.Instance.saveData.friendlinessData.MaCheonGyo;

        int generatedID = month * 100000 + a * 10000 + b * 1000 + c * 100 + d * 10 + e;

        //Debug.Log($"생성된 QuestID → {generatedID}");

        List<int> bestIDs = FindBestBranchIDs(generatedID);

        if (rank < bestIDs.Count)
        {
            Debug.Log($"{rank + 1}번째 퀘스트 → {bestIDs[rank]}");
            DialogUI.Instance.StartDialog(bestIDs[rank]);
        }
        else
        {
            Debug.LogWarning($"{rank + 1}번째 퀘스트가 존재하지 않습니다.");
        }
    }

    List<int> FindBestBranchIDs(int targetID)
    {
        List<int> allBranch = DialogManager.Instance.GetSpawnableBranchIDs();
        List<(int id, float score)> candidates = new();

        int tMonth = targetID / 100000;
        int tA = (targetID / 10000) % 10;
        int tB = (targetID / 1000) % 10;
        int tC = (targetID / 100) % 10;
        int tD = (targetID / 10) % 10;
        int tE = targetID % 10;

        foreach (int id in allBranch)
        {
            if (id / 100000 != tMonth)
                continue;

            int iA = (id / 10000) % 10;
            int iB = (id / 1000) % 10;
            int iC = (id / 100) % 10;
            int iD = (id / 10) % 10;
            int iE = id % 10;

            float score =
                Mathf.Abs(tA - iA) * danWolWeight +
                Mathf.Abs(tB - iB) * hongNyeonWeight +
                Mathf.Abs(tC - iC) * yaSeoWeight +
                Mathf.Abs(tD - iD) * jeonSangWeight +
                Mathf.Abs(tE - iE) * maCheonWeight;

            candidates.Add((id, score));
        }

        candidates.Sort((x, y) =>
        {
            int compare = x.score.CompareTo(y.score);

            if (compare == 0)
                compare = x.id.CompareTo(y.id);

            return compare;
        });

        List<int> result = new();

        if (candidates.Count > 0)
            result.Add(candidates[0].id);

        if (candidates.Count > 1)
            result.Add(candidates[1].id);

        return result;
    }
}