"""Configure the March/May/July choice conditions in the latest workbook."""
import argparse
from pathlib import Path
from openpyxl import load_workbook
from StoryExcelBuilder import save_book

ROOT = Path(__file__).resolve().parents[2]
BOOK = ROOT / 'Assets/PHA/Story/Data/Excel/StoryData.xlsx'
RULES = [
    ('Normal_03', 'Choice_Question', 'A_000', 'relationship.positive:Hongryeon', 'history:AskedHongryeon'),
    ('Normal_03', 'Choice_Question', 'B_000', 'relationship.positive:Yaseo', 'history:AskedYaseo'),
    ('Normal_05', 'Choice_Report', 'C_001', 'history.any:AskedHongryeon|AskedYaseo', None),
    ('Normal_07', 'Choice_03', 'A_003', 'history:AskedHongryeon', None),
    ('Normal_07', 'Choice_03', 'B_003', 'history:AskedYaseo', None),
]


def configure(book):
    changes = []
    for sid, node, target, required, result in RULES:
        sheet = book[sid + '_선택지']
        columns = {c.value: c.column for c in sheet[2]}
        matches = [row for row in range(3, sheet.max_row + 1)
                   if sheet.cell(row, columns['NodeId']).value == node
                   and sheet.cell(row, columns['TargetNodeId']).value == target]
        if len(matches) != 1:
            raise ValueError(f'{sid}: {node}/{target} must identify exactly one choice.')
        row = matches[0]
        values = dict(UseCondition='TRUE', RequiredKey=required, RequiredValue='True', HideWhenLocked='FALSE')
        if result:
            values.update(ResultKey=result, ResultValue='True')
        for field, value in values.items():
            sheet.cell(row, columns[field], value)
        changes.append(f'{sheet.title}:{row}: {required}')
    return changes


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, help='Preview copy; omit to update StoryData.xlsx with a backup.')
    args = parser.parse_args()
    output = args.output or BOOK
    if output.resolve() == BOOK.resolve() and BOOK.with_name('~$' + BOOK.name).exists():
        raise SystemExit('엑셀을 저장하고 닫은 뒤 다시 실행해 주세요.')
    original = BOOK.read_bytes()
    book = load_workbook(BOOK)
    changes = configure(book)
    if BOOK.read_bytes() != original:
        raise RuntimeError('원본이 변경되었습니다. 다시 실행해 주세요.')
    save_book(book, output, True)
    book.close()
    print('\n'.join(changes))


if __name__ == '__main__':
    main()

# 기능: 엑셀의 3·5·7월 선택지에 우호도 조건과 회차 간 선택 이력 조건을 설정합니다.
# 터미널 (Tools/Story 폴더, 엑셀 저장 후 닫기): python .\ConfigureChoiceHistory.py
