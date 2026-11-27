# 선택지 조건과 다회차 이력

`StoryChoiceHistory.cs`는 선택 결과의 키/값 쌍을 누적 저장합니다. 같은 키에 다른 값을 선택해도 과거 값이 지워지지 않습니다. `PlayerPrefs`의 `Story.ChoiceHistory.v1`에 회차 세이브와 별도로 저장하므로, 현재 `SceneChange.NewGame()` / `Datamanager.DeleteGameData()`로 새 게임을 시작해도 유지됩니다. 기기·로컬 사용자 기준이며 클라우드 동기화는 포함하지 않습니다. PlayerPrefs 전체를 삭제하면 이력도 삭제됩니다.

컴포넌트를 씬에 추가할 필요는 없습니다. `StoryRunner`가 조건 조회와 선택 결과 기록을 호출합니다. 일반 키의 기존 회차 저장 동작은 유지합니다.

| 용도 | RequiredKey | RequiredValue |
| --- | --- | --- |
| 현재 홍련귀 우호도 > 0 | `relationship.positive:Hongryeon` | `True` |
| 현재 야서 우호도 > 0 | `relationship.positive:Yaseo` | `True` |
| 홍련귀 질문을 선택한 적 있음 | `history:AskedHongryeon` | `True` |
| 야서 질문을 선택한 적 있음 | `history:AskedYaseo` | `True` |
| 둘 중 하나라도 선택한 적 있음 | `history.any:AskedHongryeon\|AskedYaseo` | `True` |

`UseCondition=TRUE`, `HideWhenLocked=FALSE`로 두면 잠긴 선택지를 표시하되 누를 수 없게 합니다. 우호도 조건은 이력이 있어도 현재 우호도가 0 이하면 잠깁니다. `relationship.positive:`는 키의 의미 자체가 > 0 조건이므로 RequiredValue로 임계값을 바꾸지 않습니다.

새로운 영구 기록을 추가하려면 해당 선택지에 `ResultKey=history:새로운키`, `ResultValue=원하는값`을 지정합니다. 이후 다른 선택지에서 같은 RequiredKey/RequiredValue로 조회합니다. 이력은 조건을 확인하는 순간이 아니라 실제 선택했을 때만 기록됩니다. 세력별 기록은 별도 키여서 서로 덮어쓰지 않습니다.

이번 설정: 3월 홍련귀/야서 질문이 각각 `history:AskedHongryeon=True`, `history:AskedYaseo=True`를 남깁니다. 5월 충돌 보고는 둘 중 하나, 7월 세력별 질문은 해당 세력 이력으로 해금합니다.

엑셀을 닫고 Tools/Story 터미널에서 `python .\ConfigureChoiceHistory.py`를 실행하면 최신 엑셀의 해당 다섯 선택지만 설정하며 이전 파일을 백업합니다. 행 번호 대신 부모 NodeId/TargetNodeId로 대상을 찾습니다. 이후 기존 엑셀 → CSV → Unity `Story/CSV/Import All StoryData` 순서로 반영합니다. CSV 검증 오류가 있다면 먼저 해결해야 합니다.
