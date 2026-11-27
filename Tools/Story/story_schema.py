"""Shared CSV contract. Unity reads these column names; locations are diagnostics only."""
from __future__ import annotations
import csv
import math
from collections import defaultdict
from pathlib import Path

DATA_DIR = Path(__file__).resolve().parents[2] / "Assets/PHA/Story/Data"
FIELDS = {
    "Stories": "StoryId StoryType StoryTitle Year Month Day Priority PlayOnce PlayTiming UnlockFactionId StartNodeId DefaultBackgroundId Memo",
    "StoryNodes": "StoryId NodeOrder NodeId NodeType Text CharacterId PortraitId NextNodeId BackgroundId BackgroundTransition BackgroundTransitionDuration DimPortrait KeepPortrait UseTypingEffect AutoAdvance AutoAdvanceDelay Branch/Scene Memo",
    "StoryChoices": "StoryId NodeId ChoiceIndex ChoiceText TargetNodeId ResultKey ResultValue UseCondition RequiredKey RequiredValue HideWhenLocked LockedText Memo",
    "StoryConditions": "StoryId ConditionIndex ConditionType Key Value IntValue Memo",
    "StoryEffects": "StoryId NodeId EffectIndex EffectType Target Duration Strength WaitForCompletion Memo",
    "Characters": "CharacterId CharacterName CharacterNameEng PortraitId PortraitLabel SpriteName",
    "Backgrounds": "BackgroundId DisplayName",
}
FIELDS = {k: v.split() for k, v in FIELDS.items()}
ENUMS = {
    "StoryType": "Normal Force Unlock Branch Event",
    "PlayTiming": "BeforeMonthly Monthly AfterMonthly",
    "NodeType": "CharacterDialogue PlayerDialogue Narration Choice End",
    "ConditionType": "None StoryCompleted StoryNotCompleted FactionIntroduced FactionNotIntroduced RelationshipAtLeast RelationshipAtMost ChoiceEquals",
    "EffectType": "None PortraitFadeIn PortraitFadeOut PortraitShakeSmall PortraitShakeStrong PortraitZoomIn PortraitZoomOut ScreenFlash ScreenDarken TextShake",
    "Target": "Portrait DialoguePanel DialogueText Background EntireScreen",
    "BackgroundTransition": "Instant Fade",
}
BOOL = {"PlayOnce": True, "UseTypingEffect": True, "KeepPortrait": True, "HideWhenLocked": True,
        "DimPortrait": False, "AutoAdvance": False, "WaitForCompletion": False, "UseCondition": False}
INT = {"NodeOrder": None, "ChoiceIndex": None, "EffectIndex": None, "ConditionIndex": None,
       "Priority": 100, "Year": 0, "Month": 0, "Day": 0, "IntValue": 0}
FLOAT = {"AutoAdvanceDelay": 1.5, "Strength": 1, "Duration": 0.3, "BackgroundTransitionDuration": 0.3}
DEFAULT = {"PlayTiming": "Monthly", "BackgroundTransition": "Instant", "RequiredValue": "True", "Target": "Portrait"}
KEYS = {"Stories": ["StoryId"], "StoryNodes": ["StoryId","NodeId"],
        "StoryChoices": ["StoryId","NodeId","ChoiceIndex"], "StoryEffects": ["StoryId","NodeId","EffectIndex"],
        "StoryConditions": ["StoryId","ConditionIndex"], "Characters": ["CharacterId","PortraitId"], "Backgrounds": ["BackgroundId"]}
REQUIRED = {"Stories": "StoryId StartNodeId StoryType", "StoryNodes": "StoryId NodeId NodeType",
            "StoryChoices": "StoryId NodeId TargetNodeId ChoiceText", "StoryConditions": "StoryId ConditionType",
            "StoryEffects": "StoryId NodeId EffectType", "Characters": "CharacterId", "Backgrounds": "BackgroundId"}
UNTRIMMED = {"Text", "ChoiceText", "LockedText", "Memo", "StoryTitle"}

def empty_data():
    return {k: [] for k in FIELDS}

def read_csv_folder(folder):
    data = empty_data()
    for kind, columns in FIELDS.items():
        path = folder / (kind + ".csv")
        if not path.exists():
            if kind not in {"Characters","Backgrounds"}:
                raise ValueError(f"필수 CSV 없음: {path}")
            continue
        with path.open(encoding="utf-8-sig", newline="") as f:
            reader = csv.DictReader(f, strict=True)
            if not set(REQUIRED[kind].split()) <= set(reader.fieldnames or []):
                raise ValueError(f"{path.name}: 필수 열 누락")
            for r in reader:
                if None in r:
                    raise ValueError(f"{path.name}:{reader.line_num}: 헤더보다 열이 많습니다.")
                data[kind].append({**r, "_where": f"{path.name}:{reader.line_num}"})
    return data

def validate(data, auto_next=False):
    """Return diagnostics, and fill defaults/explicit @next links in memory."""
    errors, warnings = [], []
    def err(r, field, msg):
        errors.append(f"{r['_where']} [{field}] {msg}")
    for kind, rows in data.items():
        seen = set()
        for r in rows:
            for field in FIELDS[kind]:
                value = r.get(field)
                s = "" if value is None else str(value)
                if field not in UNTRIMMED:
                    s = s.strip()
                if field in BOOL:
                    if not s:
                        s = str(BOOL[field])
                    val = s.upper()
                    if val in ("TRUE", "1", "YES", "Y"):
                        s = "TRUE"
                    elif val in ("FALSE", "0", "NO", "N"):
                        s = "FALSE"
                    else:
                        err(r, field, f"TRUE 또는 FALSE 필요: {s!r}")
                elif field in INT or field in FLOAT:
                    d = (INT if field in INT else FLOAT)[field]
                    if s or d is not None:
                        try:
                            v = int(s) if s and field in INT else float(s) if s else d
                            if not math.isfinite(v) or (field in FLOAT and v < 0):
                                raise ValueError
                            if field in INT and not -(2**31) <= v < 2**31:
                                raise ValueError
                            if (field in {"Year","NodeOrder","ChoiceIndex","ConditionIndex","EffectIndex"} and v < 0
                                or field == "Month" and not 0 <= v <= 12
                                or field == "Day" and not 0 <= v <= 31):
                                raise ValueError
                            s = str(v)
                        except (ValueError, OverflowError):
                            err(r, field, f"잘못된 숫자: {s!r}")
                    elif d is None:
                        err(r, field, "순서를 입력하세요.")
                elif not s:
                    s = DEFAULT.get(field, "")
                if field in ENUMS and s not in ENUMS[field].split():
                    err(r, field, f"등록되지 않은 값: {s!r}")
                r[field] = s
            for field in REQUIRED[kind].split():
                if not r[field].strip():
                    err(r, field, "빈 값입니다.")
            key = tuple(r[k] for k in KEYS[kind])
            if all(key) and key in seen:
                err(r, "/".join(KEYS[kind]), f"중복: {key}")
            seen.add(key)

    stories = {s["StoryId"]:s for s in data["Stories"]}
    if not stories:
        errors.append("Stories: 스토리가 하나도 없습니다.")
    nodes = defaultdict(dict)
    order = defaultdict(list)
    choices = defaultdict(list)
    edges = defaultdict(set)
    for kind in ("StoryNodes","StoryChoices","StoryConditions","StoryEffects"):
        for r in data[kind]:
            if r["StoryId"] not in stories:
                err(r, "StoryId", f"스토리가 없습니다: {r['StoryId']}")
    for r in data["StoryNodes"]:
        sid, nid = r["StoryId"], r["NodeId"]
        nodes[sid][nid] = r
        order[sid].append(r)
    for r in data["StoryChoices"]:
        sid, nid = r["StoryId"], r["NodeId"]
        choices[sid, nid].append(r)
        if nodes[sid].get(nid, {}).get("NodeType") != "Choice":
            err(r, "NodeId", f"같은 스토리의 Choice 로그를 지정하세요: {nid}")
        if r["TargetNodeId"] not in nodes[sid]:
            err(r, "TargetNodeId", f"이동할 로그가 없습니다: {r['TargetNodeId']}")
        if r["UseCondition"] == "TRUE" and not r["RequiredKey"]:
            err(r, "RequiredKey", "조건부 선택지에는 키가 필요합니다.")
        edges[sid, nid].add(r["TargetNodeId"])
    for r in data["StoryConditions"]:
        if r["ConditionType"] != "None" and not r["Key"]:
            err(r,"Key","스토리 실행 조건에 키가 필요합니다.")
    for r in data["StoryEffects"]:
        if r["NodeId"] not in nodes[r["StoryId"]]:
            err(r, "NodeId", f"효과의 대상 로그가 없습니다: {r['NodeId']}")
    chars = defaultdict(set)
    for r in data["Characters"]:
        chars[r["CharacterId"]].add(r["PortraitId"])
    registered={r["BackgroundId"] for r in data["Backgrounds"]} | {"__HIDE__"}
    if data["Backgrounds"]:
        for table,field in (("Stories","DefaultBackgroundId"),("StoryNodes","BackgroundId")):
            for r in data[table]:
                if r[field] and r[field] not in registered:
                    warnings.append(f"{r['_where']} [{field}] {r[field]}: 엑셀의 배경 목록에 없습니다. Unity 카탈로그를 확인하세요.")
    for sid, s in stories.items():
        if s["StartNodeId"] not in nodes[sid]:
            err(s, "StartNodeId", f"시작 로그가 없습니다: {s['StartNodeId']}")
        rows = sorted(order[sid], key=lambda r: int(r["NodeOrder"]) if r["NodeOrder"].lstrip("-").isdigit() else 0)
        orders = set()
        entry_targets = {r["TargetNodeId"] for (story,_), group in choices.items() if story == sid for r in group}
        for i,r in enumerate(rows):
            nid, typ, nxt = r["NodeId"], r["NodeType"], r["NextNodeId"]
            if r["NodeOrder"] in orders:
                err(r, "NodeOrder", f"스토리 내 순서 중복: {r['NodeOrder']}")
            orders.add(r["NodeOrder"])
            if r["CharacterId"] and chars and r["CharacterId"] not in chars:
                err(r,"CharacterId",f"캐릭터 목록에 없습니다: {r['CharacterId']}")
            elif typ=="CharacterDialogue" and r["PortraitId"] and r["CharacterId"] in chars and r["PortraitId"] not in chars[r["CharacterId"]]:
                warnings.append(f"{r['_where']} [PortraitId] 해당 캐릭터의 표정 목록에 없습니다: {r['PortraitId']}. 유니티의 기존 기본 표정 규칙을 사용합니다.")
            if typ == "Choice":
                group = choices[sid,nid]
                if not 1 <= len(group) <= 3:
                    err(r,"Choice",f"선택지는 1~3개 필요합니다. 현재 {len(group)}개")
                elif all(c["UseCondition"]=="TRUE" for c in group):
                    warnings.append(f"{r['_where']} [{nid}] 모든 선택지가 조건부입니다.")
            if typ in {"Choice","End"}:
                continue
            if nxt == "@next" or (not nxt and auto_next and r.get("_modern") and r["Branch/Scene"]):
                next_row = rows[i+1] if i+1 < len(rows) else {}
                if (not r["Branch/Scene"] or next_row.get("Branch/Scene") != r["Branch/Scene"]
                        or next_row.get("NodeId") in entry_targets):
                    err(r,"NextNodeId","분기 경계 또는 마지막 행입니다. 이동/합류 대상을 지정하세요.")
                    continue
                nxt = r["NextNodeId"] = next_row["NodeId"]
            if nxt not in nodes[sid]:
                err(r, "NextNodeId", f"이동할 로그가 없습니다: {nxt!r}")
            edges[sid,nid].add(nxt)
        reached, stack = set(), [s["StartNodeId"]]
        while stack:
            n = stack.pop()
            if n in reached: continue
            reached.add(n)
            stack.extend(edges[sid,n] - reached)
        missing = sorted(nodes[sid].keys() - reached)
        if missing:
            warnings.append(f"{sid}: 시작점에서 도달하지 못하는 로그 {len(missing)}개: {', '.join(missing[:8])}")
    return errors, warnings

# 기능: 엑셀·CSV 변환에 사용하는 공통 규칙
# 직접 실행 X. Builder와 Exporter가 자동으로 사용
