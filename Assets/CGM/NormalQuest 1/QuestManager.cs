using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TetrisGame;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("모든 퀘스트 데이터")]
    public List<NormalQuest> allQuests;

    [Header("현재 진행중인 퀘스트")]
    public List<NormalQuest> activeQuests = new List<NormalQuest>();

    [Header("UI 관련")]
    public Transform questUIParent;     // UI가 붙을 부모 오브젝트
    public GameObject questUIPrefab;    // 퀘스트 UI 프리팹

    private Dictionary<int, GameObject> questUIMap = new Dictionary<int, GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        GenerateInitialQuests();
    }

    private void GenerateInitialQuests()
    {
        while (activeQuests.Count < 2)
        {
            AddNewQuest();
        }
    }
    public void UpdateQuestProgress(BlockType destroyedType)    //todo: 이거 왜 수정해야하지?????
    {
        List<NormalQuest> completedQuests = new List<NormalQuest>();

        for (int i = 0; i < activeQuests.Count; i++)
        {
            NormalQuest quest = activeQuests[i];
            quest.AddProgress(destroyedType);

            if (quest.IsCompleted)
            {
                completedQuests.Add(quest); // 나중에 완료 처리용으로 따로 저장
                quest.IsCompleted = false;  // 중복 클리어 방지
            }
        }

        // foreach 다 돌고 난 후에 처리
        foreach (var quest in completedQuests)
        {
            CompleteQuest(quest.questID);
        }
    }

    public void AddNewQuest(int excludeQuestID = -1)
    {
        // 현재 진행중인 퀘스트 ID 수집
        var excludedIDs = activeQuests.Select(q => q.questID).ToList();

        // 방금 클리어한 퀘스트도 제외
        if (excludeQuestID != -1)
            excludedIDs.Add(excludeQuestID);

        // 나머지 퀘스트는 다시 뽑을 수 있음 (무한 반복)
        var availableQuests = allQuests.Where(q => !excludedIDs.Contains(q.questID)).ToList();

        if (availableQuests.Count == 0)
        {
            Debug.Log("조건에 맞는 퀘스트가 없어 전체에서 랜덤 선택합니다!");
            availableQuests = allQuests; // 전체 풀에서 랜덤 선택
        }

        NormalQuest newQuest = availableQuests[Random.Range(0, availableQuests.Count)];
        activeQuests.Add(newQuest);

        Debug.Log($"새 퀘스트 추가됨: {newQuest.questName}");

        // UI 생성 및 매핑 등록
        GameObject questUIObj = Instantiate(questUIPrefab, questUIParent);
        NormalQuestUI questUI = questUIObj.GetComponent<NormalQuestUI>();
        questUI.SetQuest(newQuest);

        questUIMap[newQuest.questID] = questUIObj;

        newQuest.currentCount = 0;
        newQuest.IsCompleted = false;

        // 목표 개수를 5~20 랜덤
        newQuest.targetCount = Random.Range(5, 21);
    }

    public void CompleteQuest(int questID)
    {
        var quest = activeQuests.FirstOrDefault(q => q.questID == questID);
        if (quest != null)
        {
            Debug.Log($"퀘스트 완료: {quest.questName}, 보상: {quest.Reward}");

            // 진행중에서 제거
            activeQuests.Remove(quest);

            // UI 제거
            if (questUIMap.ContainsKey(questID))
            {
                Destroy(questUIMap[questID]);
                questUIMap.Remove(questID);
            }

            StageManager.Instance.AddMoney(quest.Reward);
            Debug.Log($"추가 골드 : {quest.Reward}");
            //Debug.Log($"현제 골드 : {DataManager.Instance.data.Gold}");
            // 새 퀘스트 생성 (방금 클리어한 퀘스트 제외)
            StartCoroutine(AddNewQuestNextFrame(questID));
        }
    }
    private IEnumerator AddNewQuestNextFrame(int excludeQuestID)
    {
        yield return null; // 다음 프레임까지 대기
        AddNewQuest(excludeQuestID);
    }
}