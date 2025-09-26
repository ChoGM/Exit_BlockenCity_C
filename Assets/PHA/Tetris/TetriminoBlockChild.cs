using System.Collections;
using System.Collections.Generic;
using TetrisGame;
using Unity.VisualScripting;
using UnityEngine;

public class TetriminoBlockChild : MonoBehaviour
{
    [SerializeField]
    private BlockType blockType;    //�� ����

    [SerializeField] private Transform visualRoot; // �޽ð� �޸� ������Ʈ(������ ����)
    private Quaternion initialWorldRotation;

    void Awake()
    {
        blockType = BlockType.None;

        if (visualRoot == null) visualRoot = transform; // ������ �ڱ� �ڽ�
        initialWorldRotation = visualRoot.rotation;     // ���� �� ���� ȸ�� ����
    }

    void LateUpdate()
    {
        // ���� �� ȸ������ ���� (��� ������ �ؽ�ó �� ������)
        visualRoot.rotation = initialWorldRotation;
    }

    //�߰�
    //Child�� ������ �� �ڱ� ��ǥ�� �˾ƾ� �ϹǷ� �ʵ�� ���� �޼��� �߰�
    private Vector3Int gridPosition;
    public Vector3Int GridPosition => gridPosition;
    public void SetGridPosition(Vector3Int pos)
    {
        gridPosition = pos;
    }

    // �� Ÿ�� ����
    public void SetBlockType(BlockType blockType)
    {
        this.blockType = blockType;
    }

    // �� ��Ƽ���� ����
    public void SetBlockMaterial(Material material)
    {
        Renderer rend = this.GetComponent<Renderer>();
        if (rend != null && material != null)
            rend.material = material;
    }

    public void BlockLock()
    {
        //Ÿ�� �� ī��Ʈ
        TetrisManager.Instance.IncreaseTypeBlockCount(blockType);


        //Ÿ�Կ� ���� �����̴��� ����
    }

    public void DeletBlock()
    {
        // ��ȣ���� �����̴��� ����

        TetrisManager.Instance.DecreaseTypeBlockCount(blockType);

        // �߰�
        // Ÿ�������� ����
        var tower = TetrisManager.Instance.tower;
        if (tower != null)
        {
            tower.RemoveBlockFromTower(GridPosition);
        }

        Destroy(gameObject);
    }
}
