"""Build an editable story workbook from a CSV folder or migrate an old workbook."""
import argparse
import os
import re
import tempfile
from collections import defaultdict
from pathlib import Path
from shutil import copy2
from datetime import datetime
from copy import copy
from openpyxl import Workbook, load_workbook
from openpyxl.styles import Alignment, Font
from openpyxl.utils import get_column_letter
from openpyxl.worksheet.datavalidation import DataValidation
from openpyxl.worksheet.table import Table, TableStyleInfo, TableColumn, TableFormula
from openpyxl.worksheet.filters import AutoFilter
from story_schema import DATA_DIR, FIELDS, ENUMS, BOOL, read_csv_folder

def table_sheet(w, title, table_name, kind, rows, story_id=None, parent_table=None, has_backgrounds=False):
    ws = w.create_sheet(title)
    scoped = kind in {"StoryNodes", "StoryChoices"}
    hidden_fields = {"StoryId","NodeOrder","ChoiceIndex"} if scoped else set()
    columns = [c for c in FIELDS[kind] if c not in hidden_fields]
    formula_column = {"StoryNodes":"ChoiceParent","StoryChoices":"TargetPreview"}.get(kind)
    if formula_column:
        columns.append(formula_column)
    ws.append(["StoryId" if scoped else "종류", story_id if scoped else kind])
    ws.append(columns)
    rows = list(rows)
    if not rows: rows = [{}]
    table = Table(displayName=table_name, ref=f"A2:{get_column_letter(len(columns))}{2+len(rows)}")
    table.tableStyleInfo = TableStyleInfo(name="TableStyleMedium2", showRowStripes=True, showColumnStripes=False)
    formula = None
    if kind == "StoryNodes":
        formula = 'IF([[#This Row],[NodeType]]="Choice",[[#This Row],[NodeId]],"")'
    if kind == "StoryChoices":
        formula = f'IFERROR(INDEX({parent_table}[Text],MATCH([[#This Row],[TargetNodeId]],{parent_table}[NodeId],0)),"")'
    for j, field in enumerate(columns, 1):
        col = TableColumn(id=j,name=field)
        if field == formula_column:
            col.calculatedColumnFormula = TableFormula(attr_text=formula)
        table.tableColumns.append(col)
    for i, r in enumerate(rows,3):
        for j,field in enumerate(columns,1):
            value = r.get(field)
            if value in (None,""): continue
            cell = ws.cell(i,j,value)
            if isinstance(value,str): cell.data_type = "s"  # preserve literal "=" at start of dialogue
            if field in {"Text","ChoiceText","Memo"}:
                cell.alignment=Alignment(wrap_text=True,vertical="top")
        if formula_column:
            ws.cell(i,len(columns),"="+formula)
    table.autoFilter = AutoFilter(ref=table.ref)
    ws.add_table(table)
    for j,field in enumerate(columns,1):
        letter = get_column_letter(j)
        ws.column_dimensions[letter].width = 58 if field in {"Text","ChoiceText","TargetPreview"} else max(16,len(field)+3)
        if field == "ChoiceParent": ws.column_dimensions[letter].hidden = True
        options = ENUMS.get(field)
        source = f'"{",".join(options.split())}"' if options else None
        if field in BOOL: source='"TRUE,FALSE"'
        if has_backgrounds and field in {"BackgroundId","DefaultBackgroundId"}:
            source = 'INDIRECT("StoryBackgrounds[BackgroundId]")'
        if kind == "StoryNodes" and field in {"CharacterId","PortraitId"}:
            source = f'INDIRECT("StoryCharacters[{field}]")'
        if kind == "StoryChoices" and field in {"NodeId","TargetNodeId"}:
            target_col = "ChoiceParent" if field=="NodeId" else "NodeId"
            source = f'INDIRECT("{parent_table}[{target_col}]")'
        if source:
            validation=DataValidation(type="list",formula1=source,allow_blank=True,showErrorMessage=True)
            validation.errorTitle="입력값 확인"
            validation.error="목록에서 등록된 값을 선택하세요."
            ws.add_data_validation(validation)
            validation.add(f"{letter}3:{letter}1048576")
    ws.freeze_panes="B3"
    return ws

def safe_title(sid,suffix,used):
    stem=re.sub(r"[\\/*?:\[\]']","_",sid)
    idx=0
    while True:
        end = suffix if not idx else f"_{idx}{suffix}"
        value = stem[:31-len(end)] + end
        if value.lower() not in used:
            used.add(value.lower())
            return value
        idx+=1

def build(data):
    w = Workbook()
    instructions = w.active
    instructions.title="사용법"
    for line in (
        "스토리 로그 편집",
        "Stories에서 스토리를 확인하고 같은 StoryId의 대사·선택지 시트를 편집하세요.",
        "새 줄은 표 안에서 행을 삽입하거나 마지막 행 바로 아래에 붙여 넣으세요. 위에서 아래로 재생 순서 번호가 매겨집니다.",
        "대사 순서는 연결과 별개입니다. NextNodeId에 대상을 지정하거나, 같은 Branch/Scene 안에서는 @next로 다음 줄에 연결하세요. --auto-next를 사용하면 같은 분기의 빈 NextNodeId도 연결합니다. 분기 끝·합류 대상은 직접 지정하세요.",
        "선택지의 NodeId는 버튼을 보여줄 Choice 로그입니다. TargetNodeId는 클릭 후 이동할 로그입니다. 버튼 순서는 위에서 아래입니다.",
        "배경은 변경하는 줄에서 BackgroundId를 지정하세요. 공란은 현재 배경을 유지합니다. Fade 시간은 초 단위입니다.",
        "새 스토리를 추가하려면 생성기에 --input 기존.xlsx --output 수정본.xlsx --add-story 새ID 옵션을 전달하세요. 새 시트와 선택지 연결이 함께 만들어집니다.",
        "원고를 옮길 때 대사 본문을 Text에 붙여 넣고, 분기마다 고유한 NodeId와 이동 대상을 지정하세요.",
        "StoryExcelExporter.py --check 로 검증한 후 CSV를 출력하세요.",
    ):
        instructions.append([line])
    instructions["A1"].font=Font(bold=True,size=15)
    instructions.column_dimensions["A"].width=115
    table_sheet(w,"Stories","StoriesAll","Stories",data["Stories"],has_backgrounds=bool(data["Backgrounds"]))
    used={s.lower() for s in w.sheetnames}|{"characters","conditions","effects","backgrounds"}
    by_story=defaultdict(list)
    for kind in ("StoryNodes","StoryChoices"):
        for row in data[kind]:
            by_story[kind,row["StoryId"]].append(row)
    def order(r,col):
        try:return int(r.get(col,""))
        except (TypeError,ValueError):return 2147483647
    for idx,s in enumerate(data["Stories"],1):
        sid=s["StoryId"]
        node_table=f"StoryNodes_{idx}"
        rows=sorted(by_story["StoryNodes",sid],key=lambda r:order(r,"NodeOrder"))
        table_sheet(w,safe_title(sid,"_대사",used),node_table,"StoryNodes",rows,sid,has_backgrounds=bool(data["Backgrounds"]))
        rows=sorted(by_story["StoryChoices",sid],key=lambda r:(r.get("NodeId",""),order(r,"ChoiceIndex")))
        table_sheet(w,safe_title(sid,"_선택지",used),f"StoryChoices_{idx}","StoryChoices",rows,sid,node_table)
    for kind,title,name in (("StoryConditions","Conditions","StoryConditionsAll"),("StoryEffects","Effects","StoryEffectsAll"),("Characters","Characters","StoryCharacters"),("Backgrounds","Backgrounds","StoryBackgrounds")):
        table_sheet(w,title,name,kind,data[kind])
    return w


def preserve_cell_design(book, template_path):
    """Reuse the author's cell formatting, keeping the expandable table layout."""
    if not template_path or not template_path.is_file():
        return
    template = load_workbook(template_path)
    def style(target, source):
        for attr in ("font", "fill", "border", "alignment", "protection"):
            setattr(target, attr, copy(getattr(source, attr)))
        target.number_format = source.number_format
    try:
        sections = {}
        markers = {"StartNodeId": "Stories", "NodeType": "StoryNodes",
                   "ChoiceIndex": "StoryChoices", "ChoiceText": "StoryChoices",
                   "ConditionType": "StoryConditions", "EffectType": "StoryEffects",
                   "CharacterName": "Characters", "BackgroundId": "Backgrounds"}
        for sheet in template:
            if sheet.title in {"README", "Enums", "StoryList", "STORY_TEMPLATE", "사용법"}:
                continue
            for row in sheet.iter_rows():
                values = {c.value for c in row if isinstance(c.value, str)}
                kind = next((kind for marker, kind in markers.items() if marker in values), None)
                if kind and kind not in sections:
                    sections[kind] = (sheet, row[0].row)
        from StoryExcelExporter import TABLE_TYPES
        for sheet in book:
            for table in sheet.tables.values():
                kind = next((kind for prefix, kind in TABLE_TYPES.items() if table.name.startswith(prefix)), None)
                source = sections.get(kind) or sections.get("StoryNodes")
                if not source:
                    continue
                old, header = source
                columns = {str(c.value): c.column for c in old[header] if c.value}
                if "ChoiceNodeId" in columns:
                    columns["NodeId"] = columns["ChoiceNodeId"]
                table.tableStyleInfo = TableStyleInfo(showRowStripes=False, showColumnStripes=False)
                sheet.sheet_view.showGridLines = old.sheet_view.showGridLines
                sheet.sheet_view.zoomScale = old.sheet_view.zoomScale
                sheet.row_dimensions[2].height = old.row_dimensions[header].height or 30
                for cell in sheet[1]:
                    style(cell, old["A1"])
                sheet.row_dimensions[1].height = old.row_dimensions[1].height or 32
                for column in table.tableColumns:
                    j = column.id
                    source_col = columns.get(column.name, 1)
                    letter = get_column_letter(j)
                    if column.name in columns:
                        dimension = next((d for d in old.column_dimensions.values()
                                          if d.min <= source_col <= d.max), None)
                        sheet.column_dimensions[letter].width = dimension.width if dimension else old.sheet_format.defaultColWidth or 13
                    if column.name in {"Text", "ChoiceText", "TargetPreview"}:
                        sheet.column_dimensions[letter].width = max(68, sheet.column_dimensions[letter].width)
                    style(sheet.cell(2, j), old.cell(header, source_col))
                    for r in range(3, sheet.max_row + 1):
                        style(sheet.cell(r, j), old.cell(header + 1, source_col))
                        if column.name in {"Text", "ChoiceText", "TargetPreview", "Memo"}:
                            sheet.cell(r, j).alignment = Alignment(wrap_text=True, vertical="top")
                for r in range(3, sheet.max_row + 1):
                    sheet.row_dimensions[r].height = old.row_dimensions[header + 1].height or 32.1
    finally:
        template.close()

def save_book(w, path, overwrite):
    if path.exists() and not overwrite: raise ValueError(f"이미 존재합니다: {path}. 다른 --output 경로나 --overwrite를 지정하세요.")
    path.parent.mkdir(parents=True,exist_ok=True)
    fd, name=tempfile.mkstemp(dir=path.parent,suffix=".xlsx")
    os.close(fd)
    temp=Path(name)
    try:
        w.save(temp)
        if path.exists():
            backup=path.with_name(path.stem+"."+datetime.now().strftime("%Y%m%d-%H%M%S-%f")+".bak.xlsx")
            copy2(path,backup)
            print(f"백업: {backup}")
        os.replace(temp,path)
    finally:
        temp.unlink(missing_ok=True)

def main():
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument("--input",type=Path,default=None,help="CSV 폴더 또는 기존 .xlsx (기본: 현재 StoryData.xlsx, 없으면 CSV)")
    p.add_argument("--output",type=Path,default=DATA_DIR/"Excel/StoryData.xlsx")
    p.add_argument("--overwrite",action="store_true")
    p.add_argument("--template",type=Path,help="셀 디자인을 가져올 기존 엑셀 (기본: 출력 파일 또는 입력 엑셀)")
    p.add_argument("--add-story",metavar="STORY_ID",help="새 스토리의 대사·선택지 시트와 시작 로그를 추가합니다.")
    a=p.parse_args()
    if a.input is None:
        a.input = a.output if a.output.is_file() else DATA_DIR/"CSV"
    if a.input.is_dir():
        data=read_csv_folder(a.input)
    else:
        from StoryExcelExporter import read_workbook
        data=read_workbook(a.input)
    if a.add_story:
        sid=a.add_story.strip()
        if not sid or any(s.get("StoryId")==sid for s in data["Stories"]):
            raise ValueError("--add-story에 비어 있지 않은 새 ID를 지정하세요.")
        data["Stories"].append(dict(StoryId=sid,StoryTitle=sid,StoryType="Normal",StartNodeId="end"))
        data["StoryNodes"].append(dict(StoryId=sid,NodeId="end",NodeOrder=1,NodeType="End"))
    if not data["Stories"]:raise ValueError("스토리 없음")
    ids=[r["StoryId"] for r in data["Stories"]]
    if any(not sid for sid in ids) or len(set(ids))!=len(ids):
        raise ValueError("비어 있거나 중복된 StoryId")
    for kind in ("StoryNodes","StoryChoices"):
        for row in data[kind]:
            if row["StoryId"] not in ids:
                raise ValueError(f"{row['_where']}: 등록되지 않은 StoryId")
    book=build(data)
    template = a.template or (a.output if a.output.is_file() else a.input if a.input.is_file() else None)
    try:
        preserve_cell_design(book, template)
        save_book(book,a.output,True)
    finally:book.close()
    print(f"작성용 엑셀: {a.output}")

if __name__=="__main__":
    try:main()
    except (ValueError,OSError) as e:raise SystemExit(str(e))

# 기능: 기존 엑셀을 새 양식으로 갱신합니다. 원본은 날짜별 백업하고 StoryData.xlsx 이름을 유지합니다.
# 터미널 (Tools/Story 폴더): python .\StoryExcelBuilder.py
# CSV에서 다시 만들 때: python .\StoryExcelBuilder.py --input ..\..\Assets\PHA\Story\Data\CSV
