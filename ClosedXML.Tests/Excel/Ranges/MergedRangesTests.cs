using ClosedXML.Excel;
using NUnit.Framework;
using System.Linq;

namespace ClosedXML.Tests
{
    [TestFixture]
    public class MergedRangesTests
    {
        [Test]
        public void LastCellFromMerge()
        {
            var wb = new XLWorkbook();
            IXLWorksheet ws = wb.Worksheets.Add("Sheet");
            ws.Range("B2:D4").Merge();

            string first = ws.FirstCellUsed(XLCellsUsedOptions.All).Address.ToStringRelative();
            string last = ws.LastCellUsed(XLCellsUsedOptions.All).Address.ToStringRelative();

            Assert.AreEqual("B2", first);
            Assert.AreEqual("D4", last);
        }

        [TestCase("A1:A2", "A1:A2")]
        [TestCase("A2:B2", "A2:B2")]
        [TestCase("A3:C3", "A3:E3")]
        [TestCase("B4:B6", "B4:B6")]
        [TestCase("C7:D7", "E7:F7")]
        public void MergedRangesShiftedOnColumnInsert(string originalRange, string expectedRange)
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("MRShift");
                var range = ws.Range(originalRange).Merge();

                ws.Column(2).InsertColumnsAfter(2);

                var mr = ws.MergedRanges.ToArray();
                Assert.AreEqual(1, mr.Length);
                Assert.AreSame(range, mr.Single());
                Assert.AreEqual(expectedRange, range.RangeAddress.ToString());
            }
        }

        [TestCase("A1:B1", "A1:B1")]
        [TestCase("B1:B2", "B1:B2")]
        [TestCase("C1:C3", "C1:C5")]
        [TestCase("D2:F2", "D2:F2")]
        [TestCase("G4:G5", "G6:G7")]
        public void MergedRangesShiftedOnRowInsert(string originalRange, string expectedRange)
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("MRShift");
                var range = ws.Range(originalRange).Merge();

                ws.Row(2).InsertRowsBelow(2);

                var mr = ws.MergedRanges.ToArray();
                Assert.AreEqual(1, mr.Length);
                Assert.AreSame(range, mr.Single());
                Assert.AreEqual(expectedRange, range.RangeAddress.ToString());
            }
        }

        [TestCase("A1:A2", true, "A1:A2")]
        [TestCase("A2:B2", true, "A2:A2")]
        [TestCase("A3:C3", true, "A3:B3")]
        [TestCase("B4:B6", false, "")]
        [TestCase("C7:D7", true, "B7:C7")]
        public void MergedRangesShiftedOnColumnDelete(string originalRange, bool expectedExist, string expectedRange)
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("MRShift");
                var range = ws.Range(originalRange).Merge();

                ws.Column(2).Delete();

                var mr = ws.MergedRanges.ToArray();
                if (expectedExist)
                {
                    Assert.AreEqual(1, mr.Length);
                    Assert.AreSame(range, mr.Single());
                    Assert.AreEqual(expectedRange, range.RangeAddress.ToString());
                }
                else
                {
                    Assert.AreEqual(0, mr.Length);
                    Assert.IsFalse(range.RangeAddress.IsValid);
                }
            }
        }

        [TestCase("A1:B1", true, "A1:B1")]
        [TestCase("B1:B2", true, "B1:B1")]
        [TestCase("C1:C3", true, "C1:C2")]
        [TestCase("D2:F2", false, "")]
        [TestCase("G4:G5", true, "G3:G4")]
        public void MergedRangesShiftedOnRowDelete(string originalRange, bool expectedExist, string expectedRange)
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("MRShift");
                var range = ws.Range(originalRange).Merge();

                ws.Row(2).Delete();

                var mr = ws.MergedRanges.ToArray();
                if (expectedExist)
                {
                    Assert.AreEqual(1, mr.Length);
                    Assert.AreSame(range, mr.Single());
                    Assert.AreEqual(expectedRange, range.RangeAddress.ToString());
                }
                else
                {
                    Assert.AreEqual(0, mr.Length);
                    Assert.IsFalse(range.RangeAddress.IsValid);
                }
            }
        }

        [Test]
        public void ShiftRangeRightBreaksMerges()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("MRShift");
                ws.Range("B2:C3").Merge();
                ws.Range("B4:C5").Merge();
                ws.Range("F2:G3").Merge(); // to be broken
                ws.Range("F4:G5").Merge(); // to be broken
                ws.Range("H1:I2").Merge();
                ws.Range("H5:I6").Merge();

                ws.Range("D3:E4").InsertColumnsAfter(2);

                var mr = ws.MergedRanges.ToArray();
                Assert.AreEqual(4, mr.Length);
                Assert.AreEqual("H1:I2", mr[0].RangeAddress.ToString());
                Assert.AreEqual("B2:C3", mr[1].RangeAddress.ToString());
                Assert.AreEqual("B4:C5", mr[2].RangeAddress.ToString());
                Assert.AreEqual("H5:I6", mr[3].RangeAddress.ToString());
            }
        }

        [Test]
        public void ShiftRangeLeftBreaksMerges()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("MRShift");
                ws.Range("B2:C3").Merge();
                ws.Range("B4:C5").Merge();
                ws.Range("F2:G3").Merge(); // to be broken
                ws.Range("F4:G5").Merge(); // to be broken
                ws.Range("H1:I2").Merge();
                ws.Range("H5:I6").Merge();

                ws.Range("D3:E4").Delete(XLShiftDeletedCells.ShiftCellsLeft);

                var mr = ws.MergedRanges.ToArray();
                Assert.AreEqual(4, mr.Length);
                Assert.AreEqual("H1:I2", mr[0].RangeAddress.ToString());
                Assert.AreEqual("B2:C3", mr[1].RangeAddress.ToString());
                Assert.AreEqual("B4:C5", mr[2].RangeAddress.ToString());
                Assert.AreEqual("H5:I6", mr[3].RangeAddress.ToString());
            }
        }

        [Test]
        public void RangeShiftDownBreaksMerges()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("MRShift");
                ws.Range("B2:C3").Merge();
                ws.Range("D2:E3").Merge();
                ws.Range("B6:C7").Merge(); // to be broken
                ws.Range("D6:E7").Merge(); // to be broken
                ws.Range("A8:B9").Merge();
                ws.Range("E8:F9").Merge();

                ws.Range("C4:D5").InsertRowsBelow(2);

                var mr = ws.MergedRanges.ToArray();
                Assert.AreEqual(4, mr.Length);
                Assert.AreEqual("B2:C3", mr[0].RangeAddress.ToString());
                Assert.AreEqual("D2:E3", mr[1].RangeAddress.ToString());
                Assert.AreEqual("A8:B9", mr[2].RangeAddress.ToString());
                Assert.AreEqual("E8:F9", mr[3].RangeAddress.ToString());
            }
        }

        [Test]
        public void RangeShiftUpBreaksMerges()
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("MRShift");
                ws.Range("B2:C3").Merge();
                ws.Range("D2:E3").Merge();
                ws.Range("B6:C7").Merge(); // to be broken
                ws.Range("D6:E7").Merge(); // to be broken
                ws.Range("A8:B9").Merge();
                ws.Range("E8:F9").Merge();

                ws.Range("C4:D5").Delete(XLShiftDeletedCells.ShiftCellsUp);

                var mr = ws.MergedRanges.ToArray();
                Assert.AreEqual(4, mr.Length);
                Assert.AreEqual("B2:C3", mr[0].RangeAddress.ToString());
                Assert.AreEqual("D2:E3", mr[1].RangeAddress.ToString());
                Assert.AreEqual("A8:B9", mr[2].RangeAddress.ToString());
                Assert.AreEqual("E8:F9", mr[3].RangeAddress.ToString());
            }
        }

        [Test]
        public void MergedCellsAcquireFirstCellStyle()
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet1");
                ws.Cell("A1").Style.Fill.BackgroundColor = XLColor.Red;
                ws.Cell("A2").Style.Fill.BackgroundColor = XLColor.Yellow;
                ws.Cell("A3").Style.Fill.BackgroundColor = XLColor.Green;
                ws.Range("A1:A3").Merge();

                Assert.AreEqual(XLColor.Red, ws.Cell("A1").Style.Fill.BackgroundColor);
                Assert.AreEqual(XLColor.Red, ws.Cell("A2").Style.Fill.BackgroundColor);
                Assert.AreEqual(XLColor.Red, ws.Cell("A3").Style.Fill.BackgroundColor);
            }
        }

        [Test]
        public void MergedCellsLooseData()
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet1");
                ws.Range("A1:A3").SetValue(100);
                ws.Range("A1:A3").Merge();

                Assert.AreEqual(100, ws.Cell("A1").Value);
                Assert.AreEqual(Blank.Value, ws.Cell("A2").Value);
                Assert.AreEqual(Blank.Value, ws.Cell("A3").Value);
            }
        }

        [Test]
        public void MergedCellsLooseConditionalFormats()
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet1");
                ws.Cell("A1").AddConditionalFormat().WhenContains("1").Fill.BackgroundColor = XLColor.Red;
                ws.Cell("A2").AddConditionalFormat().WhenContains("2").Fill.BackgroundColor = XLColor.Yellow;

                ws.Range("A1:A2").Merge();

                Assert.AreEqual(1, ws.ConditionalFormats.Count());
                Assert.AreEqual("A1:A1", ws.ConditionalFormats.Single().Ranges.Single().RangeAddress.ToString());
            }
        }

        [Test]
        public void MergedCellsLooseDataValidation()
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet1");
                ws.Cell("A1").CreateDataValidation().WholeNumber.Between(1, 2);
                ws.Cell("A2").CreateDataValidation().Date.GreaterThan(new System.DateTime(2018, 1, 1));

                ws.Range("A1:A2").Merge();

                Assert.IsTrue(ws.Cell("A1").HasDataValidation);
                Assert.AreEqual("1", ws.Cell("A1").GetDataValidation().MinValue);
                Assert.AreEqual("2", ws.Cell("A1").GetDataValidation().MaxValue);
                Assert.IsFalse(ws.Cell("A2").HasDataValidation);
            }
        }

        [Test]
        public void UnmergedCellsPreserveStyle()
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Sheet1");
                var range = ws.Range("B2:D4");
                range.Style.Fill.SetBackgroundColor(XLColor.Yellow);
                range.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thick)
                    .Border.SetOutsideBorderColor(XLColor.DarkBlue)
                    .Border.SetInsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetInsideBorderColor(XLColor.Pink);
                range.Cells().ForEach(c => c.Value = c.Address.ToString());

                var firstCell = ws.Cell("B2");
                firstCell.Style.Fill.SetBackgroundColor(XLColor.Red)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Font.SetBold();

                range.Merge();
                range.Unmerge();

                Assert.IsTrue(range.Cells().All(c => c.Style.Fill.BackgroundColor == XLColor.Red));
                Assert.IsTrue(range.Cells().Where(c => c != firstCell).All(c => c.Value.Equals(Blank.Value)));
                Assert.AreEqual("B2", firstCell.Value);

                Assert.AreEqual(XLBorderStyleValues.Thick, ws.Cell("B2").Style.Border.TopBorder);
                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("B2").Style.Border.RightBorder);
                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("B2").Style.Border.BottomBorder);
                Assert.AreEqual(XLBorderStyleValues.Thick, ws.Cell("B2").Style.Border.LeftBorder);

                Assert.AreEqual(XLBorderStyleValues.Thick, ws.Cell("C2").Style.Border.TopBorder);
                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("C2").Style.Border.RightBorder);
                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("C2").Style.Border.BottomBorder);
                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("C2").Style.Border.LeftBorder);

                Assert.AreEqual(XLBorderStyleValues.Thick, ws.Cell("D2").Style.Border.TopBorder);
                Assert.AreEqual(XLBorderStyleValues.Thick, ws.Cell("D2").Style.Border.RightBorder);
                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("D2").Style.Border.BottomBorder);
                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("D2").Style.Border.LeftBorder);

                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("B3").Style.Border.TopBorder);
                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("B3").Style.Border.RightBorder);
                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("B3").Style.Border.BottomBorder);
                Assert.AreEqual(XLBorderStyleValues.Thick, ws.Cell("B3").Style.Border.LeftBorder);

                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("C3").Style.Border.TopBorder);
                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("C3").Style.Border.RightBorder);
                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("C3").Style.Border.BottomBorder);
                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("C3").Style.Border.LeftBorder);

                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("D3").Style.Border.TopBorder);
                Assert.AreEqual(XLBorderStyleValues.Thick, ws.Cell("D3").Style.Border.RightBorder);
                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("D3").Style.Border.BottomBorder);
                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("D3").Style.Border.LeftBorder);

                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("B4").Style.Border.TopBorder);
                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("B4").Style.Border.RightBorder);
                Assert.AreEqual(XLBorderStyleValues.Thick, ws.Cell("B4").Style.Border.BottomBorder);
                Assert.AreEqual(XLBorderStyleValues.Thick, ws.Cell("B4").Style.Border.LeftBorder);

                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("C4").Style.Border.TopBorder);
                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("C4").Style.Border.RightBorder);
                Assert.AreEqual(XLBorderStyleValues.Thick, ws.Cell("C4").Style.Border.BottomBorder);
                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("C4").Style.Border.LeftBorder);

                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("D4").Style.Border.TopBorder);
                Assert.AreEqual(XLBorderStyleValues.Thick, ws.Cell("D4").Style.Border.RightBorder);
                Assert.AreEqual(XLBorderStyleValues.Thick, ws.Cell("D4").Style.Border.BottomBorder);
                Assert.AreEqual(XLBorderStyleValues.None, ws.Cell("D4").Style.Border.LeftBorder);
            }
        }

        [Test]
        public void MergedRangesCellValuesShouldNotBeSet()
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.AddWorksheet();
                ws.Range("A2:A4").Merge();
                ws.Cell("A2").Value = 1;
                ws.Cell("A3").Value = 1;
                ws.Cell("A4").Value = 1;
                ws.Cell("B1").FormulaA1 = "SUM(A:A)";
                Assert.AreEqual(1, ws.Cell("B1").Value);
            }

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.AddWorksheet();
                ws.Range("A2:A4").Merge().SetValue(1);
                ws.Cell("B1").FormulaA1 = "SUM(A:A)";
                Assert.AreEqual(1, ws.Cell("B1").Value);
            }
        }

        [Test]
        public void MergedRangesCellFormulasShouldNotBeSet()
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.AddWorksheet();
                ws.Range("A2:A4").Merge();
                ws.Cell("A2").FormulaA1 = "=1";
                ws.Cell("A3").FormulaA1 = "=1";
                ws.Cell("A4").FormulaA1 = "=1";
                ws.Cell("B1").FormulaA1 = "SUM(A:A)";
                Assert.AreEqual(1, ws.Cell("B1").Value);
            }

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.AddWorksheet();
                ws.Range("A2:A4").Merge();
                ws.Cell("A2").SetFormulaA1("=1");
                ws.Cell("A3").SetFormulaA1("=1");
                ws.Cell("A4").SetFormulaA1("=1");
                ws.Cell("B1").SetFormulaA1("SUM(A:A)");
                Assert.AreEqual(1, ws.Cell("B1").Value);
            }

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.AddWorksheet();
                ws.Range("A2:A4").Merge();
                ws.Cell("A2").FormulaR1C1 = "=1";
                ws.Cell("A3").FormulaR1C1 = "=1";
                ws.Cell("A4").FormulaR1C1 = "=1";
                ws.Cell("B1").FormulaR1C1 = "SUM(A:A)";
                Assert.AreEqual(1, ws.Cell("B1").Value);
            }

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.AddWorksheet();
                ws.Range("A2:A4").Merge();
                ws.Cell("A2").SetFormulaR1C1("=1");
                ws.Cell("A3").SetFormulaR1C1("=1");
                ws.Cell("A4").SetFormulaR1C1("=1");
                ws.Cell("B1").SetFormulaR1C1("SUM(A:A)");
                Assert.AreEqual(1, ws.Cell("B1").Value);
            }
        }

        [Test]
        public void MergeSingleCellRangeDoesNothing()
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet();
            var range = ws.Range(1, 1, 1, 1);

            range.Merge();

            Assert.IsFalse(range.IsMerged());
            Assert.AreEqual(0, ws.MergedRanges.Count);
        }
    }
}
