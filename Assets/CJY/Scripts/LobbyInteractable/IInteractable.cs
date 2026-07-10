using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public interface IInteractable
{
    void OnHoverEnter(); // 마우스가 올라갔을 때 (아웃라인 ON)
    void OnHoverExit();  // 마우스가 벗어났을 때 (아웃라인 OFF)
    void OnClick();      // 마우스로 클릭했을 때 (UI 호출)
}
