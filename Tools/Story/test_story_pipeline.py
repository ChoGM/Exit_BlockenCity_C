"""Run with python -m unittest test_story_pipeline from Tools/Story."""
import contextlib
import csv
import io
import tempfile
import unittest
from pathlib import Path
from openpyxl import Workbook, load_workbook
from StoryExcelBuilder import build, save_book, preserve_cell_design
from StoryExcelExporter import export, read_workbook
from story_schema import FIELDS, empty_data

def fixture(count=3):
    d=empty_data()
    d["Stories"]=[dict(StoryId="S",StoryType="Normal",StoryTitle="샘플",StartNodeId="n0",Priority=0,DefaultBackgroundId="Town")]
    d["StoryNodes"]=[dict(StoryId="S",NodeOrder=i+1,NodeId=f"n{i}",NodeType="Narration" if i+1<count else "End",
                          Text='=인용, "대사"\n줄바꿈' if i==0 else "",NextNodeId=f"n{i+1}" if i+1<count else "",
                          BackgroundId="Alley" if i==1 else "",BackgroundTransition="Fade" if i==1 else "Instant",
                          BackgroundTransitionDuration=0.5,**{"Branch/Scene":"Common","Memo":"유지"}) for i in range(count)]
    d["Characters"]=[dict(CharacterId="C",PortraitId="P")]
    return d

class StoryPipelineTests(unittest.TestCase):
    def test_choice_text_lookup_without_cached_excel_values(self):
        d = fixture()
        d['StoryChoices'] = [dict(StoryId='S', NodeId='n0', TargetNodeId='n1', ChoiceIndex=0)]
        self.make(d)
        w = load_workbook(self.path)
        s = next(s for s in w if 'StoryChoices_1' in s.tables)
        formula = '=IFERROR(INDEX(StoryNodes_1[Text],MATCH(StoryChoices_1[[#This Row],[TargetNodeId]],StoryNodes_1[NodeId],0)),"")'
        s['B3'] = formula
        s['B4'] = formula  # unused formatted row must not create a choice
        nodes = next(s for s in w if 'StoryNodes_1' in s.tables)
        nodes['C4'] = 'Updated dialogue'
        w.save(self.path)
        w.close()
        original = self.path.read_bytes()
        data = read_workbook(self.path)
        self.assertEqual(1, len(data['StoryChoices']))
        self.assertEqual('Updated dialogue', data['StoryChoices'][0]['ChoiceText'])
        self.assertEqual(original, self.path.read_bytes())
        w = load_workbook(self.path)
        s = next(s for s in w if 'StoryChoices_1' in s.tables)
        s['B3'] = '=1+1'
        w.save(self.path)
        w.close()
        with self.assertRaises(ValueError):
            read_workbook(self.path)

    def test_excel_formula_storage_and_template_design(self):
        from openpyxl.styles import Font, PatternFill
        from zipfile import ZipFile
        self.make(fixture())
        template = load_workbook(self.path)
        sheet = template["S_대사"]
        sheet["C2"].font = Font(name="맑은 고딕", bold=True, color="FFFFFF")
        sheet["C2"].fill = PatternFill("solid", fgColor="34495E")
        sheet.column_dimensions["C"].width = 72
        sheet.row_dimensions[3].height = 32.1
        template.save(self.path)
        template.close()
        original = self.path.read_bytes()
        book = build(fixture())
        preserve_cell_design(book, self.path)
        save_book(book, self.path, True)
        book.close()
        self.assertEqual(original, next(self.path.parent.glob("*.bak.xlsx")).read_bytes())
        result = load_workbook(self.path)
        sheet = result["S_대사"]
        self.assertEqual("맑은 고딕", sheet["C2"].font.name)
        self.assertEqual("0034495E", sheet["C2"].fill.fgColor.rgb)
        self.assertEqual(72, sheet.column_dimensions["C"].width)
        self.assertEqual(32.1, sheet.row_dimensions[3].height)
        result.close()
        with ZipFile(self.path) as archive:
            tables = [archive.read(n).decode() for n in archive.namelist() if n.startswith("xl/tables/")]
        self.assertTrue(any("[#This Row]" in t for t in tables))
        self.assertFalse(any("[@" in t for t in tables))

    def setUp(self):
        self.tmp=tempfile.TemporaryDirectory()
        self.addCleanup(self.tmp.cleanup)
        self.path=Path(self.tmp.name)/"story.xlsx"
        self.out=Path(self.tmp.name)/"csv"

    def make(self,d):
        book=build(d)
        try:save_book(book,self.path,True)
        finally:book.close()

    def run_export(self,**kwargs):
        with contextlib.redirect_stdout(io.StringIO()):
            return export(self.path,self.out,**kwargs)

    def test_long_sheet_and_insert_after_table(self):
        self.make(fixture(240))
        w=load_workbook(self.path)
        s=w["S_대사"]
        headers={c.value:c.column for c in s[2]}
        # Excel users sometimes paste directly below the declared table.
        s.cell(242,headers["NodeType"],"Narration")
        s.cell(242,headers["NextNodeId"],"new")
        s.cell(243,headers["NodeId"],"new")
        s.cell(243,headers["NodeType"],"End")
        w.save(self.path)
        w.close()
        d=self.run_export()
        self.assertEqual(241,len(d["StoryNodes"]))
        self.assertEqual("0",d["Stories"][0]["Priority"])
        self.assertEqual("Town",d["Stories"][0]["DefaultBackgroundId"])
        self.assertEqual(("Alley","Fade","0.5"),tuple(d["StoryNodes"][1][k] for k in ("BackgroundId","BackgroundTransition","BackgroundTransitionDuration")))
        with (self.out/"StoryNodes.csv").open(encoding="utf-8-sig",newline="") as f: first=next(csv.DictReader(f))
        self.assertEqual('=인용, "대사"\n줄바꿈',first["Text"])
        self.assertEqual("TRUE",first["UseTypingEffect"])
        self.assertEqual("TRUE",first["KeepPortrait"])

    def test_valid_choice_roundtrip_with_condition_and_effect(self):
        d=fixture()
        d["StoryNodes"][0]["NodeType"]="Choice"
        d["StoryChoices"]=[dict(StoryId="S",NodeId="n0",ChoiceIndex=i,ChoiceText=label,
                                TargetNodeId=target,ResultKey="decision",ResultValue=target)
                           for i,(label,target) in enumerate([("조사한다","n1"),("지나간다","n2")])]
        d["StoryConditions"]=[dict(StoryId="S",ConditionIndex=0,ConditionType="ChoiceEquals",Key="met",Value="yes")]
        d["StoryEffects"]=[dict(StoryId="S",NodeId="n1",EffectIndex=0,EffectType="ScreenDarken",Target="EntireScreen",Duration=0.2)]
        self.make(d)
        out=self.run_export()
        self.assertEqual([("0","조사한다","n1"),("1","지나간다","n2")],
                         [(r["ChoiceIndex"],r["ChoiceText"],r["TargetNodeId"]) for r in out["StoryChoices"]])
        self.assertEqual("met",out["StoryConditions"][0]["Key"])
        self.assertEqual("ScreenDarken",out["StoryEffects"][0]["EffectType"])

    def test_errors_leave_all_csvs_untouched(self):
        self.make(fixture())
        self.run_export()
        before={p.name:p.read_bytes() for p in self.out.glob("*.csv")}
        d=fixture()
        d["StoryNodes"][0]["NodeType"]="Choice"
        d["StoryChoices"]=[dict(StoryId="S",NodeId="n0",ChoiceIndex=0,ChoiceText="확인",TargetNodeId="typo",UseCondition="오타")]
        self.make(d)
        with self.assertRaises(ValueError) as why:self.run_export()
        for text in ("S_선택지:3","TargetNodeId","UseCondition"):self.assertIn(text,str(why.exception))
        self.assertEqual(before,{p.name:p.read_bytes() for p in self.out.glob("*.csv")})

    def test_auto_next_is_opt_in_and_requires_same_branch(self):
        d=fixture()
        d["StoryNodes"][0]["NextNodeId"]=""
        self.make(d)
        with self.assertRaisesRegex(ValueError,"NextNodeId"):self.run_export(check=True)
        self.assertEqual("n1",self.run_export(check=True,auto_next=True)["StoryNodes"][0]["NextNodeId"])
        d["StoryNodes"][1]["Branch/Scene"]="Other"
        self.make(d)
        with self.assertRaisesRegex(ValueError,"분기 경계"):self.run_export(check=True,auto_next=True)
        self.assertFalse(self.out.exists())

    def test_shifted_legacy_headers_and_incomplete_rows_preserved(self):
        d=fixture()
        w=Workbook()
        s=w.active
        s.title="old"
        for row,kind in [(4,"Stories"),(14,"StoryNodes"),(310,"StoryChoices"),(380,"StoryConditions"),(425,"StoryEffects")]:
            headers=[f for f in FIELDS[kind] if f!="StoryId" or kind=="Stories"]
            for col,f in enumerate(headers,1):
                s.cell(row,col,{"NodeOrder":"Order"}.get(f,f))
            for i,r in enumerate(d[kind],row+1):
                for j,f in enumerate(headers,1):
                    if r.get(f) is not None:
                        c=s.cell(i,j,r[f])
                        if isinstance(r[f],str):c.data_type="s"
        s.cell(260,4,"ID 없이 작성한 대사")
        w.save(self.path)
        w.close()
        rows=read_workbook(self.path)["StoryNodes"]
        self.assertEqual(4,len(rows))
        self.assertEqual("ID 없이 작성한 대사",rows[-1]["Text"])
        with self.assertRaisesRegex(ValueError,"old:260"):self.run_export(check=True)

if __name__=="__main__":unittest.main()

# 기능: 엑셀·CSV 변환 도구가 정상 작동하는지 테스트
# 터미널 (프로젝트 루트):
# python -m unittest discover -s Tools/Story -p test_story_pipeline.py -v
