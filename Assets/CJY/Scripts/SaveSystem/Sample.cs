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

    // 테스트용 키 입력 처리를 위한 Update 함수 추가
    void Update()
    {
        // 0번 키: 스코어 UI 리프레쉬
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            scoreUIBinder?.Refresh();
            Debug.Log("테스트: UI가 리프레쉬되었습니다.");
        }

        // 9번 키: 이번 스테이지 획득 머니 증가 (StageManager 호출)
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            // 예시로 한 번 누를 때마다 100원씩 증가하도록 설정했습니다.
            StageManager.Instance.AddMoney(100);

            // 값이 바뀐 것을 화면에 바로 보여주기 위해 리프레쉬도 함께 호출합니다.
            scoreUIBinder?.Refresh();
            Debug.Log("테스트: 스테이지 머니 +100 증가");
        }

        // 8번 키: 단월국 호감도 증가 (StageManager 호출)
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            // 예시로 한 번 누를 때마다 호감도가 10f씩 증가하도록 설정했습니다.
            StageManager.Instance.AddDanwol(10f);

            // 값이 바뀐 것을 화면에 바로 보여주기 위해 리프레쉬도 함께 호출합니다.
            scoreUIBinder?.Refresh();
            Debug.Log("테스트: 단월국 호감도 +10 증가");
        }
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