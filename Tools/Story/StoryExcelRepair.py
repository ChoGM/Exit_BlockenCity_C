"""Restore story table ranges and helper formulas without rebuilding user data."""
import argparse
from copy import copy
from datetime import datetime
import hashlib
import os
from pathlib import Path
import shutil
import tempfile

from openpyxl import load_workbook
from openpyxl.utils.cell import range_boundaries, get_column_letter
from openpyxl.workbook.properties import CalcProperties

DEFAULT = Path(__file__).resolve().parents[2] / 'Assets/PHA/Story/Data/Excel/StoryData.xlsx'


def repair(book, min_row=0):
    changes = []
    for sheet in book:
        for table in sheet.tables.values():
            if not table.name.startswith(('StoryNodes_', 'StoryChoices_')):
                continue
            left, top, right, bottom = range_boundaries(table.ref)
            headers = {sheet.cell(top, col).value: col for col in range(left, right + 1)}
            helper = 'ChoiceParent' if table.name.startswith('StoryNodes_') else 'TargetPreview'
            helper_col = headers.get(helper)
            last = max([bottom, min_row] + [row for row in range(top + 1, sheet.max_row + 1)
                       if any(sheet.cell(row, col).value not in (None, '')
                              for col in range(left, right + 1) if col != helper_col)])
            old_ref = table.ref
            for row in range(top + 1, last + 1):
                for col in range(left, right + 1):
                    cell = sheet.cell(row, col)
                    if not cell.has_style:
                        cell._style = copy(sheet.cell(top + 1, col)._style)
                if sheet.row_dimensions[row].height is None:
                    sheet.row_dimensions[row].height = sheet.row_dimensions[top + 1].height
            table.ref = f'{get_column_letter(left)}{top}:{get_column_letter(right)}{last}'
            if table.autoFilter is not None:
                table.autoFilter.ref = table.ref
            if helper_col:
                column = next((c for c in table.tableColumns if c.name == helper), None)
                formula = column.calculatedColumnFormula.attr_text if column and column.calculatedColumnFormula else None
                if formula:
                    for row in range(top + 1, last + 1):
                        cell = sheet.cell(row, helper_col)
                        if cell.value is None or cell.data_type == 'f':
                            cell.value = '=' + formula.lstrip('=')
                            if not cell.has_style:
                                cell._style = copy(sheet.cell(top + 1, helper_col)._style)
            if old_ref != table.ref:
                changes.append((sheet.title, old_ref, table.ref))
    book.calculation = CalcProperties(calcId=0, fullCalcOnLoad=True, forceFullCalc=True)
    return changes


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--input', type=Path, default=DEFAULT)
    parser.add_argument('--output', type=Path, help='Write a preview copy instead of replacing the input.')
    parser.add_argument('--min-row', type=int, default=0, help='Extend all dialogue/choice tables to at least this worksheet row.')
    args = parser.parse_args()
    source = args.input.resolve()
    target = args.output.resolve() if args.output else source
    if source == target and source.with_name('~$' + source.name).exists():
        raise SystemExit('먼저 엑셀에서 저장하고 파일을 닫아 주세요.')
    original = source.read_bytes()
    digest = hashlib.sha256(original).digest()
    book = load_workbook(source)
    if not 0 <= args.min_row <= 1048576:
        parser.error('--min-row must be between 0 and 1048576')
    changes = repair(book, args.min_row)
    target.parent.mkdir(parents=True, exist_ok=True)
    fd, name = tempfile.mkstemp(suffix='.xlsx', dir=target.parent)
    os.close(fd)
    try:
        book.save(name)
        if hashlib.sha256(source.read_bytes()).digest() != digest:
            raise RuntimeError('작업 중 원본이 변경되어 중단했습니다. 다시 실행해 주세요.')
        if target.exists():
            backup = target.with_name(target.stem + '.' + datetime.now().strftime('%Y%m%d-%H%M%S-%f') + '.bak.xlsx')
            shutil.copy2(target, backup)
            print('Backup:', backup)
        os.replace(name, target)
    finally:
        book.close()
        Path(name).unlink(missing_ok=True)
    for title, before, after in changes:
        print(title, before, '->', after)
    print('Saved:', target)


if __name__ == '__main__':
    main()

# 기능: 작성한 엑셀의 표 범위와 선택지 ID 연결을 복구합니다. 기존 파일은 백업합니다.
# 터미널 (Tools/Story 폴더, 엑셀 저장 후 닫기): python .\StoryExcelRepair.py
# 모든 달의 대사·선택지를 최소 200행까지 확장: python .\StoryExcelRepair.py --min-row 200
