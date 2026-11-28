using System;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    private static GameObject container;
    private static GameDataManager instance;


    // =====================================================
    // Limits
    // =====================================================

    public const int MIN_MONEY = 0;
    public const int MAX_MONEY = 9999999;

    public const float MIN_RELATIONSHIP = -100f;
    public const float MAX_RELATIONSHIP = 100f;

    public const int MIN_STAGE = 1;
    public const int MAX_STAGE = 12;


    // =====================================================
    // Events
    // =====================================================

    public event Action<int> OnMoneyChanged;

    public event Action<RelationshipType, float>
        OnRelationshipChanged;


    // =====================================================
    // Singleton
    // =====================================================

    public static GameDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                container =
                    new GameObject("GameDataManager");

                instance =
                    container.AddComponent<GameDataManager>();

                DontDestroyOnLoad(container);
            }

            return instance;
        }
    }


    private SaveData Data
    {
        get
        {
            return Datamanager.Instance.saveData;
        }
    }


    // =====================================================
    // Money
    // =====================================================

    public int GetMoney()
    {
        int safeValue =
            Mathf.Clamp(
                Data.player.totalMoney,
                MIN_MONEY,
                MAX_MONEY);

        if (safeValue != Data.player.totalMoney)
        {
            Data.player.totalMoney =
                safeValue;
        }

        return safeValue;
    }


    public void SetMoney(long value)
    {
        if (value < MIN_MONEY)
        {
            value = MIN_MONEY;
        }
        else if (value > MAX_MONEY)
        {
            value = MAX_MONEY;
        }


        int newValue =
            (int)value;

        int oldValue =
            Data.player.totalMoney;


        Data.player.totalMoney =
            newValue;


        if (oldValue != newValue)
        {
            OnMoneyChanged?.Invoke(
                newValue);
        }
    }


    public void AddMoney(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning(
                "AddMoney에는 1 이상의 값을 사용해주세요.");

            return;
        }


        long result =
            (long)GetMoney() + amount;


        SetMoney(result);
    }


    public bool TrySpendMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning(
                "TrySpendMoney에는 0 이상의 값을 사용해주세요.");

            return false;
        }


        int currentMoney =
            GetMoney();


        if (currentMoney < amount)
        {
            return false;
        }


        SetMoney(
            (long)currentMoney - amount);


        return true;
    }


    // =====================================================
    // Relationship
    // =====================================================
    public string GetRelationshipState(float value)
    {
        if (value < 0)
            return "적대";

        if (value <= 30)
            return "무관심";

        if (value <= 60)
            return "중립";

        if (value <= 80)
            return "호의";

        if (value <= 99)
            return "친밀";

        return "동맹";
    }

    public string GetRelationshipState(RelationshipType type)
    {
        return GetRelationshipState(GetRelationship(type));
    }

    public float GetRelationship(
        RelationshipType type)
    {
        float value = 0f;


        switch (type)
        {
            case RelationshipType.Danwol:

                value =
                    Data.relationship.danwol;

                break;


            case RelationshipType.Yaseo:

                value =
                    Data.relationship.yaseo;

                break;


            case RelationshipType.Macheon:

                value =
                    Data.relationship.macheon;

                break;


            case RelationshipType.Hongryeon:

                value =
                    Data.relationship.hongryeon;

                break;


            case RelationshipType.JeonSangYeon:

                value =
                    Data.relationship.JeonSangYeon;

                break;
        }


        float safeValue =
            Mathf.Clamp(
                value,
                MIN_RELATIONSHIP,
                MAX_RELATIONSHIP);


        if (!Mathf.Approximately(
                value,
                safeValue))
        {
            SetRelationship(
                type,
                safeValue);
        }


        return safeValue;
    }


    public void SetRelationship(
        RelationshipType type,
        float value)
    {
        float safeValue =
            Mathf.Clamp(
                value,
                MIN_RELATIONSHIP,
                MAX_RELATIONSHIP);


        float oldValue =
            GetRawRelationship(type);


        switch (type)
        {
            case RelationshipType.Danwol:

                Data.relationship.danwol =
                    safeValue;

                break;


            case RelationshipType.Yaseo:

                Data.relationship.yaseo =
                    safeValue;

                break;


            case RelationshipType.Macheon:

                Data.relationship.macheon =
                    safeValue;

                break;


            case RelationshipType.Hongryeon:

                Data.relationship.hongryeon =
                    safeValue;

                break;


            case RelationshipType.JeonSangYeon:

                Data.relationship.JeonSangYeon =
                    safeValue;

                break;
        }


        if (!Mathf.Approximately(
                oldValue,
                safeValue))
        {
            OnRelationshipChanged?.Invoke(
                type,
                safeValue);
        }
    }


    private float GetRawRelationship(
        RelationshipType type)
    {
        switch (type)
        {
            case RelationshipType.Danwol:
                return Data.relationship.danwol;

            case RelationshipType.Yaseo:
                return Data.relationship.yaseo;

            case RelationshipType.Macheon:
                return Data.relationship.macheon;

            case RelationshipType.Hongryeon:
                return Data.relationship.hongryeon;

            case RelationshipType.JeonSangYeon:
                return Data.relationship.JeonSangYeon;

            default:
                return 0f;
        }
    }


    public void AddRelationship(
        RelationshipType type,
        float amount)
    {
        float currentValue =
            GetRelationship(type);


        SetRelationship(
            type,
            currentValue + amount);
    }


    // =====================================================
    // Item
    // =====================================================

    public int GetItemCount(
        GameItemId itemId)
    {
        if (Data.items == null)
        {
            Data.items =
                new ItemSaveData();
        }


        return Mathf.Max(
            0,
            Data.items.GetCount(itemId));
    }


    public void SetItemCount(
        GameItemId itemId,
        int count)
    {
        if (Data.items == null)
        {
            Data.items =
                new ItemSaveData();
        }


        Data.items.SetCount(
            itemId,
            Mathf.Max(0, count));
    }


    public void AddItem(
        GameItemId itemId,
        int amount = 1)
    {
        if (amount <= 0)
        {
            return;
        }


        int currentCount =
            GetItemCount(itemId);


        long result =
            (long)currentCount + amount;


        if (result > int.MaxValue)
        {
            result = int.MaxValue;
        }


        SetItemCount(
            itemId,
            (int)result);
    }


    public bool TryConsumeItem(
        GameItemId itemId,
        int amount = 1)
    {
        if (amount <= 0)
        {
            return true;
        }


        int currentCount =
            GetItemCount(itemId);


        if (currentCount < amount)
        {
            return false;
        }


        SetItemCount(
            itemId,
            currentCount - amount);


        return true;
    }


    // =====================================================
    // Player Name
    // =====================================================

    public string GetPlayerName()
    {
        return Data.player.playerName;
    }


    public void SetPlayerName(
        string playerName)
    {
        Data.player.playerName =
            playerName;
    }


    // =====================================================
    // Stage
    // =====================================================

    public int GetCurrentStage()
    {
        int safeStage =
            Mathf.Clamp(
                Data.progress.currentStage,
                MIN_STAGE,
                MAX_STAGE);


        if (safeStage !=
            Data.progress.currentStage)
        {
            Data.progress.currentStage =
                safeStage;
        }


        return safeStage;
    }


    public void SetCurrentStage(
        int stage)
    {
        Data.progress.currentStage =
            Mathf.Clamp(
                stage,
                MIN_STAGE,
                MAX_STAGE);
    }


    public bool AdvanceStage()
    {
        int currentStage =
            GetCurrentStage();


        if (currentStage >= MAX_STAGE)
        {
            return false;
        }


        SetCurrentStage(
            currentStage + 1);


        return true;
    }


    // =====================================================
    // Save
    // =====================================================

    public void SaveGameData()
    {
        Datamanager.Instance
            .SaveGameData();
    }

    public void NormalizeLoadedData()
    {
        // =========================================
        // SaveData 자체
        // =========================================

        if (Datamanager.Instance.saveData == null)
        {
            Datamanager.Instance.saveData =
                new SaveData();
        }


        SaveData data =
            Datamanager.Instance.saveData;


        // =========================================
        // 필요한 하위 데이터 null 보정
        // =========================================

        if (data.player == null)
        {
            data.player =
                new PlayerData();
        }


        if (data.relationship == null)
        {
            data.relationship =
                new RelationshipData();
        }


        if (data.progress == null)
        {
            data.progress =
                new ProgressData();
        }


        if (data.items == null)
        {
            data.items =
                new ItemSaveData();
        }


        // =========================================
        // 재화 범위 보정
        // =========================================

        SetMoney(
            data.player.totalMoney
        );


        // =========================================
        // 우호도 범위 보정
        // =========================================

        SetRelationship(
            RelationshipType.Danwol,
            data.relationship.danwol
        );


        SetRelationship(
            RelationshipType.Yaseo,
            data.relationship.yaseo
        );


        SetRelationship(
            RelationshipType.Macheon,
            data.relationship.macheon
        );


        SetRelationship(
            RelationshipType.Hongryeon,
            data.relationship.hongryeon
        );


        SetRelationship(
            RelationshipType.JeonSangYeon,
            data.relationship.JeonSangYeon
        );


        // =========================================
        // Stage 범위 보정
        // =========================================

        SetCurrentStage(
            data.progress.currentStage
        );
    }
}