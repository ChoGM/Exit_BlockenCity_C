using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TetrisGame;
using Unity.VisualScripting;

public class TetriminoBlock : MonoBehaviour
{
    [SerializeField]
    public BlockType blockType;    //블럭 종류

    // 머티리얼 대신 프리팹 사용
    //private Material blockMaterial; //블럭 머티리얼
    private GameObject blockPrefab;


    // 테트리스 네비게이터
    [SerializeField] private TetrisNavigator navigator;
    [SerializeField] private Material navigatorGhostMaterial;

    [SerializeField]
    private Vector3Int localPosition;   //블럭의 타워상 위치

    [SerializeField]
    private bool isSelect;   //현재 조작 중인 블럭인가

    private bool isLock; // 블럭이 설치되어 고정상태인가


    public BlockShapes shapeType; //블럭 모양

    private Vector3[] shapeData;    //블럭 모양에 따른 벡터

    [SerializeField]
    private TetriminoBlockChild tetriminoBlock;


    [SerializeField]
    private float fallInterval = 0.5f;  //떨어지는 속도
    private float fallTimer = 0f;


    private float inputWhileLandedTimer = 0f;
    private float lockDelay = 1f;   // 1초간 입력 없으면 락
    private bool isLanded = false; // 바닥에 닿았는지 여부

    // 자식 블럭
    private TetriminoBlockChild[] tetriminoBlockChild;

    //추가
    //방해물 시스템에서 사용하는 블록 락 이벤트
    public static event System.Action<TetriminoBlock> OnAnyBlockLocked;

    public bool IsLocked => isLock;


    void Awake()
    {
        isSelect = false;

        //블럭 모양 가져오기
        shapeType = GetRandomShapeType();
        shapeData = BlockData.Shapes[shapeType];


        //블럭 타입 처음은 None 표시
        blockType = BlockType.None;

        //블럭 머티리얼 설정
        blockType = GetRandomBlockType();
        //blockMaterial = BlockMaterialBinder.Materials[blockType];

        if (!BlockPrefabBinder.Prefabs.TryGetValue(blockType, out blockPrefab) || blockPrefab == null)
        {
            Debug.LogError($"[TetriminoBlock] Missing prefab for type {blockType}");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        CheckPrefabs();

        // 블럭 형태 생성 및 머티리얼 적용
        SpawnBlockVisual();

        fallInterval = TetrisManager.Instance.fallInterval;


        if (navigator == null)
        {
            // 내 자식으로 Navigator를 하나 붙인다 (인스펙터에 미리 배치해도 OK)
            var go = new GameObject("TetrisNavigator");
            go.transform.SetParent(this.transform, false);
            navigator = go.AddComponent<TetrisNavigator>();
        }
        navigator.Initialize(this, navigatorGhostMaterial);
        navigator.gameObject.SetActive(isSelect);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSelect) return;

        // [추가] 게임이 일시정지되었거나 이미 종료되었다면 아래 낙하 계산을 하지 않고 멈춤
        if (TetrisManager.Instance != null && (TetrisManager.Instance.isPaused || TetrisManager.Instance.isGameEnded))
        {
            return;
        }

        if (blockType == BlockType.None)
        {
            //None일 경우 잘못된 블럭 처리 요망
        }

        if (isLock)
        {
            // 락된 블럭일 때만 처리
        }

        if (!CanMove(Vector3.down))
        {
            if (!isLanded)
            {
                isLanded = true;
                inputWhileLandedTimer = 0f;
            }

            inputWhileLandedTimer += Time.deltaTime;

            // 입력이 없는 상태에서 일정 시간 지나면 고정
            if (inputWhileLandedTimer >= lockDelay)
            {
                BlockLock();
                isLanded = false;
            }
        }
        else
        {
            // 바닥에서 떨어졌으면 초기화
            isLanded = false;

            // 자동 낙하
            fallTimer += Time.deltaTime;
            if (fallTimer >= fallInterval)
            {
                Move(Vector3.down);
                fallTimer = 0f;
            }
        }
    }

    public void SetIsSelet(bool selet)
    {
        isSelect = selet;

        if (navigator != null)
            navigator.gameObject.SetActive(isSelect);
    }

    public void BlockLock()
    {
        isSelect = false;
        isLock = true;

        int towerHeight = 8; // y 최대 개수
        foreach (var child in tetriminoBlockChild)
        {
            if (child == null) continue;
            Vector3Int pos = WorldToTowerOffset(child.transform.position); // 위치→타워셀 좌표 변환
            if (pos.y >= towerHeight)   // 꼭대기(유효범위 밖)면 게임오버
            {
                TetrisManager.Instance.GameOver();
                return; // 더 진행하지 않음 (타워 등록/다음 스폰 X)
            }
        }

        foreach (var child in tetriminoBlockChild)
        {
            if (child != null)
            {
                // 자식 블럭의 월드 좌표 → 타워 좌표
                Vector3Int towerPos = WorldToTowerOffset(child.transform.position);

                // 타워에 등록
                TetrisManager.Instance.tower.AddBlockToTower(towerPos, blockType);

                // 추가
                // Child에게 좌표 기억시킴
                child.SetGridPosition(towerPos);

                child.BlockLock();
            }
        }

        if (navigator != null)
        {
            Destroy(navigator.gameObject);
            navigator = null;
        }

        //추가
        //블록 락 이벤트 활성화
        OnAnyBlockLocked?.Invoke(this);

        //높이부분 추가
        //int currentHeight = TetrisManager.Instance.tower.GetCurrentHeight();
        //SpecialQuestManager.Instance.OnHeightChanged(currentHeight);

        SpecialQuestManager.Instance.OnBlockDropped();
        TetrisManager.Instance.CheckTower();
        TetrisManager.Instance.SpawnNextBlock();
    }

    public void DeletBlock()
    {
        TetrisManager.Instance.DecreaseTypeBlockCount(blockType); //추가요

        foreach (var child in tetriminoBlockChild)
        {
            child?.DeletBlock();
        }

        Destroy(gameObject);
    }

    // 블럭 모양 랜덤 -> 수정
    private BlockShapes GetRandomShapeType()
    {
        return TetrisManager.Instance.GetRandomStageShape();
    }

    // 블럭 타입 랜덤으로 정해주기
    private BlockType GetRandomBlockType()
    {
        BlockType[] values = (BlockType[])System.Enum.GetValues(typeof(BlockType));

        // None은 제외
        List<BlockType> valid = new(values);
        valid.Remove(BlockType.None);

        return valid[Random.Range(0, valid.Count)];
    }

    // 블럭 모양 설정 && 머티리얼 불러오기
    private void SpawnBlockVisual()
    {
        if (shapeData == null || shapeData.Length == 0 || tetriminoBlock == null)
            return;

        tetriminoBlockChild = new TetriminoBlockChild[shapeData.Length];

        for (int i = 0; i < shapeData.Length; i++)
        {
            Vector3 pos = shapeData[i];

            // 기존 머티리얼 형식
            //// 자식 블록 프리팹 인스턴스화
            //GameObject childObj = Instantiate(tetriminoBlock.gameObject, this.transform);
            //childObj.transform.localPosition = pos;

            //// 컴포넌트 참조 및 설정
            //TetriminoBlockChild child = childObj.GetComponent<TetriminoBlockChild>();
            //if (child != null)
            //{
            //    child.SetBlockType(blockType);
            //    child.SetBlockMaterial(blockMaterial);
            //    tetriminoBlockChild[i] = child;
            //}


            if (blockPrefab == null)
            {
                Debug.LogError("[TetriminoBlock] blockPrefab is null; cannot spawn visuals.");
                continue;
            }

            GameObject childObj = Instantiate(blockPrefab, this.transform);
            childObj.transform.localPosition = pos;

            // TetriminoBlockChild가 있다면 논리(타워 좌표, 락 처리 등) 연결
            var child = childObj.GetComponent<TetriminoBlockChild>();
            if (child != null)
            {
                child.SetBlockType(blockType);

                tetriminoBlockChild[i] = child;
            }
        }
    }

    // 이동
    public void Move(Vector3 direction)
    {
        if (isSelect)
        {
            transform.position += direction;
            Vector3Int delta = WorldToTowerOffset(direction);
            localPosition += delta;

            inputWhileLandedTimer = 0f;
        }
    }

    // 타워 좌표로 변경
    private Vector3Int WorldToTowerOffset(Vector3 worldDir)
    {
        Vector3 localToTower = worldDir - TetrisManager.Instance.tower.transform.position;

        return new Vector3Int(
            Mathf.FloorToInt(localToTower.x + 0.0001f),
            Mathf.FloorToInt(localToTower.y + 0.0001f),
            Mathf.FloorToInt(localToTower.z + 0.0001f)
        );
    }


    public bool CanMove(Vector3 direction)
    {
        foreach (var child in tetriminoBlockChild)
        {
            if (child == null) continue;

            Vector3 targetPos = child.transform.position + direction;
            Vector3Int towerPos = WorldToTowerOffset(targetPos);

            if (!TetrisManager.Instance.tower.IsInsideTower(towerPos)) return false;
            if (TetrisManager.Instance.tower.IsFilled(towerPos)) return false;
        }

        return true;
    }


    //라인 삭제시 블럭 위치 조정
    public void ApplyLineClear(int clearedY)
    {
        if (tetriminoBlockChild == null)
        {
            CleanupIfEmpty();
            return;
        }

        foreach (var child in tetriminoBlockChild)
        {
            if (child == null) continue;

            // WorldToTowerOffset 대신, 락 때 저장해둔 GridPosition을 기준으로 판정
            Vector3Int pos = child.GridPosition;

            if (pos.y == clearedY)
            {
                // 삭제는 child가 towerGrid도 비우는 구조(DeletBlock 안에서 RemoveBlockFromTower)
                child.DeletBlock();  // :contentReference[oaicite:3]{index=3}
            }
            else if (pos.y > clearedY)
            {
                // 1) 비주얼 이동
                child.transform.position += Vector3.down;

                // 2) 논리 좌표도 같이 이동
                child.ShiftDownOneCell();   // 이미 구현돼 있으면 사용 :contentReference[oaicite:4]{index=4}
                                            // 또는 child.SetGridPosition(pos + Vector3Int.down);
            }
        }
    }

    // 자식들이 비었다면 껍데기 삭제
    public void CleanupIfEmpty()
    {
        int alive = 0;
        if (tetriminoBlockChild != null)
        {
            foreach (var c in tetriminoBlockChild)
            {
                if (c != null) alive++;
            }
        }

        if (alive == 0)
            Destroy(gameObject);
    }

    public void SetLocalPosition(Vector3Int position)
    {
        localPosition = position;
    }

    public void RotateX()
    {
        // 회전 시 유효 위치인지 확인
        transform.Rotate(Vector3.right * 90f, Space.World);

        if (!CanMove(Vector3.zero)) // 회전 후 충돌 체크
        {
            // 불가능하면 되돌리기
            transform.Rotate(Vector3.right * -90f, Space.World);
        }

        // 자식 회전 추가해서 머티리얼 안돌아가도록 추가하기
    }

    public void RotateY()
    {
        transform.Rotate(Vector3.up * 90f, Space.World);

        if (!CanMove(Vector3.zero)) // 회전 후 충돌 체크
        {
            transform.Rotate(Vector3.up * -90f, Space.World);
        }
    }

    public void RotateZ()
    {
        transform.Rotate(Vector3.forward * 90f, Space.World);

        if (!CanMove(Vector3.zero)) // 회전 후 충돌 체크
        {
            transform.Rotate(Vector3.forward * -90f, Space.World);
        }
    }

    private void CheckPrefabs()
    {
        // 바인더가 아직 초기화 안됐으면 한 번 더 시도
        if (blockPrefab == null)
        {
            // 딕셔너리가 비어있다면, 씬에서 바인더를 찾아 강제 초기화
            if (BlockPrefabBinder.Prefabs.Count == 0)
            {
                var binder = FindObjectOfType<BlockPrefabBinder>();
                if (binder == null)
                    Debug.LogError("[TetriminoBlock] BlockPrefabBinder not found in scene.");
                // binder가 있으면 Awake()에서 이미 Prefabs를 채우도록 설계됨
            }

            // 다시 시도
            BlockPrefabBinder.Prefabs.TryGetValue(blockType, out blockPrefab);
        }

    }
}
