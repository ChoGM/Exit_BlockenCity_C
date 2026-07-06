using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample : MonoBehaviour
{
    [Header("UI")]
    public ScoreUIBinder scoreUIBinder;

    void Start()
    {
        Datamanager.Instance.LoadGameData();

        StageManager.Instance.StartStage();

        scoreUIBinder?.Refresh();
    }

    private void OnApplicationQuit()
    {
        Datamanager.Instance.SaveGameData();
    }

    public void NextStage()
    {
        var progress = Datamanager.Instance.saveData.progress;

        if (progress.currentStage < 12)
        {
            StageManager.Instance.ClearStage();

            progress.currentStage++;

            Datamanager.Instance.SaveGameData();

            StageManager.Instance.StartStage();

            scoreUIBinder?.Refresh();
        }
    }
}