using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObstacleSystem : MonoBehaviour
{
    public static GameObstacleSystem Instance { get; private set; }

    [SerializeField] private List<MonthlyObstacleTable> monthlyTables;

    private MonthlyObstacleTable.Entry selectedObstacle;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 전환 후에도 유지
    }

    // 로비에서 호출 → 이번 달 방해물 확정
    public void SelectObstacleForMonth(int month)
    {
        MonthlyObstacleTable table = monthlyTables.Find(t => t.month == month);
        if (table == null || table.obstacles.Count == 0)
        {
            //Debug.LogWarning($"[GameObstacleSystem] {month}월 테이블이 비어있음!");
            // 빈 테이블이면 자동으로 None 세팅
            selectedObstacle = new MonthlyObstacleTable.Entry { type = ObstacleType.None, weight = 1 };
            return;
        }

        selectedObstacle = GetWeightedRandom(table.obstacles);
        Debug.Log($"[GameObstacleSystem] {month}월 확정 방해물: {selectedObstacle.type}");
    }


    // 인게임에서 불러올 때
    public MonthlyObstacleTable.Entry GetSelectedObstacle()
    {
        return selectedObstacle;
    }

    // 가중치 랜덤 선택
    private MonthlyObstacleTable.Entry GetWeightedRandom(List<MonthlyObstacleTable.Entry> entries)
    {
        int total = 0;
        foreach (var e in entries) total += e.weight;

        int rand = Random.Range(0, total);
        int cumulative = 0;

        foreach (var e in entries)
        {
            cumulative += e.weight;
            if (rand < cumulative)
                return e;
        }
        return entries[entries.Count - 1];
    }
}
