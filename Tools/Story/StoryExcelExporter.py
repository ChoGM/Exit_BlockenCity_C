"""Validate and export the story workbook. --check only reads the workbook and CSVs."""
from __future__ import annotations
import argparse
import csv
import os
import re
import tempfile
from collections import Counter
from datetime import datetime
from pathlib import Path
from shutil import copy2
from openpyxl import load_workbook
from openpyxl.utils.cell import range_boundaries
from story_schema import DATA_DIR, FIELDS, KEYS, REQUIRED, empty_data, validate

TABLE_TYPES={"StoriesAll":"Stories","StoryCharacters":"Characters","StoryConditionsAll":"StoryConditions",
             "StoryEffectsAll":"StoryEffects","StoryBackgrounds":"Backgrounds","StoryNodes_":"StoryNodes","StoryChoices_":"StoryChoices"}
LEGACY_SKIP={"README","Enums","Characters","StoryList","STORY_TEMPLATE"}
ALIASES={"Order":"NodeOrder","ChoiceNodeId":"NodeId"}
def headers(ws,row):
    return {n:ALIASES.get(str(cell.value),str(cell.value)) for n,cell in enumerate(ws[row],1) if cell.value}
def verify_columns(ws,row,kind,columns,scoped=False):
    required=set(REQUIRED[kind].split())
    if scoped:required.discard("StoryId")
    missing=required-set(columns.values())
    if missing:raise ValueError(f"{ws.title}:{row} 필수 열 누락: {', '.join(sorted(missing))}")
    if len(set(columns.values()))!=len(columns):raise ValueError(f"{ws.title}:{row} 중복 열 제목")

def read_row(ws,row,kind,columns):
    result={}
    for column,field in columns.items():
        if field not in FIELDS[kind]:continue
        cell=ws.cell(row,column)
        if cell.data_type in {"f","e"}:
            raise ValueError(f"{ws.title}!{cell.coordinate} [{field}]: 수식 또는 셀 오류입니다. 계산된 값을 붙여 넣으세요.")
        if cell.value is not None and cell.value!="":result[field]=cell.value
    if not result:return None
    result["_where"]=f"{ws.title}:{row}"
    return result

def read_tables(w,data):
    seen=set()
    for ws in w:
        for t in ws.tables.values():
            kind=next((k for prefix,k in TABLE_TYPES.items() if t.name.startswith(prefix)),None)
            if not kind:continue
            _,head,_,_=range_boundaries(t.ref)
            columns=headers(ws,head)
            scoped=kind in ("StoryNodes","StoryChoices")
            verify_columns(ws,head,kind,columns,scoped)
            expected_columns=set(FIELDS[kind])
            if scoped:
                expected_columns -= {"StoryId","NodeOrder","ChoiceIndex"}
            missing_columns=expected_columns-set(columns.values())
            if missing_columns:
                raise ValueError(f"{ws.title}:{head} 표 열 누락: {', '.join(sorted(missing_columns))}")
            sid=str(ws["B1"].value or "").strip() if scoped else ""
            if scoped and not sid:raise ValueError(f"{ws.title}!B1: StoryId를 입력하세요.")
            if (kind,sid) in seen:raise ValueError(f"{ws.title}: 중복된 {kind} 표, StoryId={sid}")
            seen.add((kind,sid))
            indices=Counter()
            for row in range(head+1,ws.max_row+1):
                record=read_row(ws,row,kind,columns)
                if record is None:continue
                if scoped:
                    record["StoryId"]=sid
                    if kind=="StoryNodes":
                        indices[sid]+=1
                        record["NodeOrder"]=indices[sid]
                        record["_modern"]=True
                    else:
                        key=record.get("NodeId","")
                        record["ChoiceIndex"]=indices[key]
                        indices[key]+=1
                data[kind].append(record)
    expected={(k,"") for k in FIELDS if k not in ("StoryNodes","StoryChoices","Backgrounds")}
    for s in data["Stories"]:
        for k in ("StoryNodes","StoryChoices"):expected.add((k,str(s.get("StoryId","")).strip()))
    missing=expected-seen
    if missing:raise ValueError(f"필수 표가 없습니다: {sorted(missing)}")

def legacy_read(w,data):
    chars=w["Characters"] if "Characters" in w.sheetnames else None
    if chars is not None:
        cols=headers(chars,1)
        verify_columns(chars,1,"Characters",cols)
        for i in range(2,chars.max_row+1):
            r=read_row(chars,i,"Characters",cols)
            if r:data["Characters"].append(r)
    for ws in w:
        if ws.title in LEGACY_SKIP:continue
        sections=[]
        for row in ws.iter_rows():
            keys={c.value for c in row if isinstance(c.value,str)}
            for marker,kind in (("StartNodeId","Stories"),("NodeType","StoryNodes"),("ChoiceIndex","StoryChoices"),
                                ("ConditionIndex","StoryConditions"),("EffectIndex","StoryEffects")):
                if marker in keys and ({"StoryId","NodeId","ChoiceNodeId","ConditionType"} & keys):
                    sections.append((row[0].row,kind))
                    break
        if not sections:
            raise ValueError(f"{ws.title}: 기존 스토리 시트에서 헤더를 찾지 못했습니다.")
        if len(sections)!=5 or len({k for _,k in sections})!=5:
            raise ValueError(f"{ws.title}: 기존 양식의 메타·로그·선택지·조건·효과 헤더를 확인하세요.")
        meta_row=next(r for r,k in sections if k=="Stories")
        scols=headers(ws,meta_row)
        meta=read_row(ws,meta_row+1,"Stories",scols)
        if not meta or not meta.get("StoryId"):raise ValueError(f"{ws.title}:{meta_row+1}: StoryId 없음")
        data["Stories"].append(meta)
        # Legacy section captions are merged cells. Other nonempty rows include drafts missing IDs.
        captions={region.min_row for region in ws.merged_cells.ranges if region.min_row==region.max_row and region.min_col==1}
        for i,(head,kind) in enumerate(sections):
            if kind=="Stories":continue
            cols=headers(ws,head)
            verify_columns(ws,head,kind,cols,True)
            end=sections[i+1][0] if i+1<len(sections) else ws.max_row+1
            for row in range(head+1,end):
                if row in captions:continue
                r=read_row(ws,row,kind,cols)
                if r:
                    r["StoryId"]=meta["StoryId"]
                    data[kind].append(r)
    if not data["Stories"]:raise ValueError("기존 양식에 스토리가 없습니다.")

def resolve_choice_text_formulas(book):
    """Evaluate only our known preview lookup, without Excel's stale formula cache.

    Operates on the in-memory workbook; the authored formulas remain on disk.
    Other formulas remain rejected by read_row.
    """
    tables = {t.name: (s, t) for s in book for t in s.tables.values()}
    for sheet in book:
        for table in sheet.tables.values():
            if not table.name.startswith('StoryChoices_'):
                continue
            head = range_boundaries(table.ref)[1]
            cols = {value: col for col, value in headers(sheet, head).items()}
            if 'ChoiceText' not in cols or 'TargetNodeId' not in cols:
                continue
            for row in range(head + 1, sheet.max_row + 1):
                cell = sheet.cell(row, cols['ChoiceText'])
                if cell.data_type != 'f':
                    continue
                pattern = (r'=IFERROR\(INDEX\((StoryNodes_\d+)\[Text\],MATCH\('
                           + re.escape(table.name)
                           + r'\[\[#This Row\],\[TargetNodeId\]\],\1\[NodeId\],0\)\),""\)')
                match = re.fullmatch(pattern, cell.value)
                if not match or match[1] not in tables:
                    continue
                source, source_table = tables[match[1]]
                if source['B1'].value != sheet['B1'].value:
                    raise ValueError(f'{sheet.title}!{cell.coordinate}: lookup points to another story.')
                source_cols = {value: col for col, value in headers(source, range_boundaries(source_table.ref)[1]).items()}
                target = sheet.cell(row, cols['TargetNodeId']).value
                values = [source.cell(r, source_cols['Text']) for r in range(3, source.max_row + 1)
                          if target not in (None, '') and source.cell(r, source_cols['NodeId']).value == target]
                if len(values) > 1 or any(c.data_type in {'f', 'e'} for c in values):
                    raise ValueError(f'{sheet.title}!{cell.coordinate}: ambiguous or invalid source text.')
                cell.value = values[0].value if values else None
                if isinstance(cell.value, str):
                    cell.data_type = 's'


def read_workbook(path):
    w=load_workbook(path,data_only=False)
    data=empty_data()
    try:
        resolve_choice_text_formulas(w)
        if any(any(t.startswith(tuple(TABLE_TYPES)) for t in s.tables) for s in w):
            read_tables(w,data)
        else:legacy_read(w,data)
        return data
    finally:w.close()

def preview(data,output):
    for kind,rows in data.items():
        if kind=="Backgrounds":continue  # Unity owns this catalog.
        old_path=output/(kind+".csv")
        if not old_path.exists():
            print(f"{kind}: 신규 {len(rows)}행")
            continue
        with old_path.open(encoding="utf-8-sig",newline="") as f:
            old=list(csv.DictReader(f))
        def by_id(records):
            return {tuple(str(r.get(k,"")) for k in KEYS[kind]):r for r in records}
        left,right=by_id(old),by_id(rows)
        common=left.keys() & right.keys()
        changed=sum(any(str(left[key].get(f,"") or "")!=str(right[key].get(f,"") or "")
                        for f in FIELDS[kind]) for key in common)
        print(f"{kind}: {len(old)} → {len(rows)}행  추가 {len(right.keys()-left.keys())} · 수정 {changed} · 삭제 {len(left.keys()-right.keys())}")

def save_csvs(data,output):
    output.mkdir(parents=True,exist_ok=True)
    targets=[(kind,output/(kind+".csv")) for kind in data if kind!="Backgrounds"]
    backup=output.parent/".story-backups"/datetime.now().strftime("%Y%m%d-%H%M%S-%f")
    with tempfile.TemporaryDirectory(dir=output.parent,prefix=".story-export-") as tmp:
        staging=Path(tmp)
        for kind,dest in targets:
            with (staging/dest.name).open("w",encoding="utf-8-sig",newline="") as f:
                writer=csv.DictWriter(f,fieldnames=FIELDS[kind],extrasaction="ignore")
                writer.writeheader()
                writer.writerows(data[kind])
        for _,dest in targets:
            if dest.exists():
                backup.mkdir(parents=True,exist_ok=True)
                copy2(dest,backup/dest.name)
        installed=[]
        try:
            for _,dest in targets:
                os.replace(staging/dest.name,dest)
                installed.append(dest)
        except BaseException:
            for dest in reversed(installed):
                saved=backup/dest.name
                if saved.exists():copy2(saved,dest)
                else:dest.unlink(missing_ok=True)
            raise
    if backup.exists():print(f"이전 CSV 백업: {backup}")

def export(path,output,check=False,auto_next=False):
    data=read_workbook(path)
    errors,warnings=validate(data,auto_next=auto_next)
    for w in warnings:print("[경고]",w)
    if errors:raise ValueError("\n".join(["변환 중단 — 다음 항목을 수정하세요.",*errors]))
    preview(data,output)
    if not check:save_csvs(data,output)
    return data

def main():
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument("--input",type=Path,default=DATA_DIR/"Excel/StoryData.xlsx")
    p.add_argument("--output-dir",type=Path,default=DATA_DIR/"CSV")
    p.add_argument("--check",action="store_true")
    p.add_argument("--auto-next",action="store_true",help="새 양식에서 같은 Branch/Scene의 연속 로그 자동 연결")
    a=p.parse_args()
    export(a.input,a.output_dir,a.check,a.auto_next)

if __name__=="__main__":
    try:main()
    except (ValueError,OSError) as e:raise SystemExit(str(e))

# 편집한 엑셀을 CSV로 변환
# python .\StoryExcelExporter.py
