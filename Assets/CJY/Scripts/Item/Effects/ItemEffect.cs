using UnityEngine;


public abstract class ItemEffect : MonoBehaviour
{
    /// <summary>
    /// 이 Effect가 담당하는 아이템.
    /// </summary>
    public abstract GameItemId ItemId { get; }


    /// <summary>
    /// 실제 아이템 효과 실행.
    ///
    /// 성공하면 Success,
    /// 사용할 수 없는 상황이면 Fail 반환.
    ///
    /// 여기에서는 인벤토리를 차감하지 않는다.
    /// </summary>
    public abstract ItemUseResult TryUse();
}