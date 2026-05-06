using UnityEngine;

public class EncyclopediaTestController : MonoBehaviour
{
    [Header("연결할 도감 UI 컨트롤러")]
    public EncyclopediaUIController encyclopediaUI;

    [Header("테스트 대상 캐릭터 ID")]
    // 여기 값을 바꾸면 특정 캐릭터를 직접 해금할 수 있음
    // 예: "li_sihwa", "hongryeon_boss"
    public string testCharacterId;

    [Header("테스트 인덱스")]
    // 0~3 사이 값
    // 0 = 첫 번째 스토리, 1 = 두 번째 스토리 ...
    public int testStoryIndex = 0;

    // 0~2 사이 값
    // 0 = 첫 번째 관계, 1 = 두 번째 관계 ...
    public int testRelationIndex = 0;

    [Header("현재 선택된 캐릭터 기준으로 테스트할지")]
    // true면 UI에서 현재 선택 중인 캐릭터를 대상으로 해금
    // false면 위의 testCharacterId 값을 대상으로 해금
    public bool useCurrentSelectedCharacter = true;

    private string GetTargetCharacterId()
    {
        if (useCurrentSelectedCharacter && encyclopediaUI != null)
        {
            return encyclopediaUI.GetCurrentCharacterId();
        }

        return testCharacterId;
    }

    public void TestUnlockCharacter()
    {
        string characterId = GetTargetCharacterId();

        if (string.IsNullOrEmpty(characterId))
        {
            Debug.LogWarning("해금할 캐릭터 ID가 비어 있습니다.");
            return;
        }

        EncyclopediaSaveManager.Instance.UnlockCharacter(characterId);

        RefreshUI();

        Debug.Log($"테스트 캐릭터 해금 완료: {characterId}");
    }

    public void TestUnlockStory()
    {
        string characterId = GetTargetCharacterId();

        if (string.IsNullOrEmpty(characterId))
        {
            Debug.LogWarning("스토리를 해금할 캐릭터 ID가 비어 있습니다.");
            return;
        }

        EncyclopediaSaveManager.Instance.UnlockStory(characterId, testStoryIndex);

        RefreshUI();

        Debug.Log($"테스트 스토리 해금 완료: {characterId} / Story Index: {testStoryIndex}");
    }

    public void TestUnlockRelation()
    {
        string characterId = GetTargetCharacterId();

        if (string.IsNullOrEmpty(characterId))
        {
            Debug.LogWarning("관계를 해금할 캐릭터 ID가 비어 있습니다.");
            return;
        }

        EncyclopediaSaveManager.Instance.UnlockRelation(characterId, testRelationIndex);

        RefreshUI();

        Debug.Log($"테스트 관계 해금 완료: {characterId} / Relation Index: {testRelationIndex}");
    }

    public void TestUnlockAllCurrentCharacterData()
    {
        string characterId = GetTargetCharacterId();

        if (string.IsNullOrEmpty(characterId))
        {
            Debug.LogWarning("전체 해금할 캐릭터 ID가 비어 있습니다.");
            return;
        }

        EncyclopediaSaveManager.Instance.UnlockCharacter(characterId);

        for (int i = 0; i < 4; i++)
        {
            EncyclopediaSaveManager.Instance.UnlockStory(characterId, i);
        }

        for (int i = 0; i < 3; i++)
        {
            EncyclopediaSaveManager.Instance.UnlockRelation(characterId, i);
        }

        RefreshUI();

        Debug.Log($"현재 캐릭터 전체 도감 해금 완료: {characterId}");
    }

    public void TestResetAllData()
    {
        EncyclopediaSaveManager.Instance.ResetAllData();

        RefreshUI();

        Debug.Log("도감 테스트 데이터 전체 초기화 완료");
    }

    private void RefreshUI()
    {
        if (encyclopediaUI != null)
        {
            encyclopediaUI.RefreshCurrentUI();
        }
    }
}