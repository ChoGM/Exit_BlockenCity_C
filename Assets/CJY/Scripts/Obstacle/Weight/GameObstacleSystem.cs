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
        DontDestroyOnLoad(gameObject); // �� ��ȯ �Ŀ��� ����
    }

    // �κ񿡼� ȣ�� �� �̹� �� ���ع� Ȯ��
    public void SelectObstacleForMonth(int month)
    {
        MonthlyObstacleTable table = monthlyTables.Find(t => t.month == month);
        if (table == null || table.obstacles.Count == 0)
        {
            Debug.LogWarning($"[GameObstacleSystem] {month}�� ���̺��� �������!");
            // �� ���̺��̸� �ڵ����� None ����
            selectedObstacle = new MonthlyObstacleTable.Entry { type = ObstacleType.None, weight = 1 };
            return;
        }

        selectedObstacle = GetWeightedRandom(table.obstacles);
        Debug.Log($"[GameObstacleSystem] {month}�� Ȯ�� ���ع�: {selectedObstacle.type}");
    }


    // �ΰ��ӿ��� �ҷ��� ��
    public MonthlyObstacleTable.Entry GetSelectedObstacle()
    {
        return selectedObstacle;
    }

    // ����ġ ���� ����
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
