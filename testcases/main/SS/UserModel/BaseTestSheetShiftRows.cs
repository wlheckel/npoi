/* ====================================================================
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for Additional information regarding copyright ownership.
   The ASF licenses this file to You under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
==================================================================== */

namespace TestCases.SS.UserModel
{
    using System;
    using NPOI.SS;
    using NPOI.SS.Util;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestCases.SS;
    using System.Collections;
    using NPOI.SS.UserModel;

    /**
     * Tests row Shifting capabilities.
     *
     * @author Shawn Laubach (slaubach at apache dot com)
     * @author Toshiaki Kamoshida (kamoshida.toshiaki at future dot co dot jp)
     */
    public abstract class BaseTestSheetShiftRows
    {

        private ITestDataProvider _testDataProvider;

        protected BaseTestSheetShiftRows(ITestDataProvider testDataProvider)
        {
            _testDataProvider = testDataProvider;
        }

        /**
         * Tests the ShiftRows function.  Does three different Shifts.
         * After each Shift, Writes the workbook to file and Reads back to
         * Check.  This ensures that if some Changes code that breaks
         * writing or what not, they realize it.
         *
         * @param sampleName the sample file to test against
         */
        
        public void BaseTestShiftRows()
        {
            // Read Initial file in
            String sampleName = "SimpleMultiCell." + _testDataProvider.StandardFileNameExtension;
            Workbook wb = _testDataProvider.OpenSampleWorkbook(sampleName);
            Sheet s = wb.GetSheetAt(0);

            // Shift the second row down 1 and write to temp file
            s.ShiftRows(1, 1, 1);

            wb = _testDataProvider.WriteOutAndReadBack(wb);

            // Read from temp file and check the number of cells in each
            // row (in original file each row was unique)
            s = wb.GetSheetAt(0);

            Assert.AreEqual(s.GetRow(0).PhysicalNumberOfCells, 1);
            ConfirmEmptyRow(s, 1);
            Assert.AreEqual(s.GetRow(2).PhysicalNumberOfCells, 2);
            Assert.AreEqual(s.GetRow(3).PhysicalNumberOfCells, 4);
            Assert.AreEqual(s.GetRow(4).PhysicalNumberOfCells, 5);

            // Shift rows 1-3 down 3 in the current one.  This tests when
            // 1 row is blank.  Write to a another temp file
            s.ShiftRows(0, 2, 3);
            wb = _testDataProvider.WriteOutAndReadBack(wb);

            // Read and ensure things are where they should be
            s = wb.GetSheetAt(0);
            ConfirmEmptyRow(s, 0);
            ConfirmEmptyRow(s, 1);
            ConfirmEmptyRow(s, 2);
            Assert.AreEqual(s.GetRow(3).PhysicalNumberOfCells, 1);
            ConfirmEmptyRow(s, 4);
            Assert.AreEqual(s.GetRow(5).PhysicalNumberOfCells, 2);

            // Read the first file again
            wb = _testDataProvider.OpenSampleWorkbook(sampleName);
            s = wb.GetSheetAt(0);

            // Shift rows 3 and 4 up and write to temp file
            s.ShiftRows(2, 3, -2);
            wb = _testDataProvider.WriteOutAndReadBack(wb);
            s = wb.GetSheetAt(0);
            Assert.AreEqual(s.GetRow(0).PhysicalNumberOfCells, 3);
            Assert.AreEqual(s.GetRow(1).PhysicalNumberOfCells, 4);
            ConfirmEmptyRow(s, 2);
            ConfirmEmptyRow(s, 3);
            Assert.AreEqual(s.GetRow(4).PhysicalNumberOfCells, 5);
        }
        private static void ConfirmEmptyRow(Sheet s, int rowIx)
        {
            Row row = s.GetRow(rowIx);
            Assert.IsTrue(row == null || row.PhysicalNumberOfCells == 0);
        }

        /**
         * Tests when rows are null.
         */
        [TestMethod]
        public void BaseTestShiftRow()
        {
            Workbook b = _testDataProvider.CreateWorkbook();
            Sheet s = b.CreateSheet();
            s.CreateRow(0).CreateCell(0).SetCellValue("TEST1");
            s.CreateRow(3).CreateCell(0).SetCellValue("TEST2");
            s.ShiftRows(0, 4, 1);
        }

        /**
         * Tests when Shifting the first row.
         */
        [TestMethod]
        public void BaseTestActiveCell()
        {
            Workbook b = _testDataProvider.CreateWorkbook();
            Sheet s = b.CreateSheet();

            s.CreateRow(0).CreateCell(0).SetCellValue("TEST1");
            s.CreateRow(3).CreateCell(0).SetCellValue("TEST2");
            s.ShiftRows(0, 4, 1);
        }

        /**
         * When Shifting rows, the page breaks should go with it
         *
         */
        [TestMethod]
        public void BaseTestShiftRowBreaks()
        {
            Workbook b = _testDataProvider.CreateWorkbook();
            Sheet s = b.CreateSheet();
            Row row = s.CreateRow(4);
            row.CreateCell(0).SetCellValue("test");
            s.SetRowBreak(4);

            s.ShiftRows(4, 4, 2);
            Assert.IsTrue(s.IsRowBroken(6), "Row number 6 should have a pagebreak");
        }

        [TestMethod]
        public void BaseTestShiftWithComments()
        {
            Workbook wb = _testDataProvider.OpenSampleWorkbook("comments." + _testDataProvider.StandardFileNameExtension);

            Sheet sheet = wb.GetSheet("Sheet1");
            Assert.AreEqual(3, sheet.LastRowNum);

            // Verify comments are in the position expected
            Assert.IsNotNull(sheet.GetCellComment(0, 0));
            Assert.IsNull(sheet.GetCellComment(1, 0));
            Assert.IsNotNull(sheet.GetCellComment(2, 0));
            Assert.IsNotNull(sheet.GetCellComment(3, 0));

            String comment1 = sheet.GetCellComment(0, 0).String.String;
            Assert.AreEqual(comment1, "comment top row1 (index0)\n");
            String comment3 = sheet.GetCellComment(2, 0).String.String;
            Assert.AreEqual(comment3, "comment top row3 (index2)\n");
            String comment4 = sheet.GetCellComment(3, 0).String.String;
            Assert.AreEqual(comment4, "comment top row4 (index3)\n");

            // Shifting all but first line down to test comments Shifting
            sheet.ShiftRows(1, sheet.LastRowNum, 1, true, true);

            // Test that comments were Shifted as expected
            Assert.AreEqual(4, sheet.LastRowNum);
            Assert.IsNotNull(sheet.GetCellComment(0, 0));
            Assert.IsNull(sheet.GetCellComment(1, 0));
            Assert.IsNull(sheet.GetCellComment(2, 0));
            Assert.IsNotNull(sheet.GetCellComment(3, 0));
            Assert.IsNotNull(sheet.GetCellComment(4, 0));

            String comment1_Shifted = sheet.GetCellComment(0, 0).String.String;
            Assert.AreEqual(comment1, comment1_Shifted);
            String comment3_Shifted = sheet.GetCellComment(3, 0).String.String;
            Assert.AreEqual(comment3, comment3_Shifted);
            String comment4_Shifted = sheet.GetCellComment(4, 0).String.String;
            Assert.AreEqual(comment4, comment4_Shifted);

            // Write out and read back in again
            // Ensure that the Changes were persisted
            wb = _testDataProvider.WriteOutAndReadBack(wb);
            sheet = wb.GetSheet("Sheet1");
            Assert.AreEqual(4, sheet.LastRowNum);

            // Verify comments are in the position expected after the shift
            Assert.IsNotNull(sheet.GetCellComment(0, 0));
            Assert.IsNull(sheet.GetCellComment(1, 0));
            Assert.IsNull(sheet.GetCellComment(2, 0));
            Assert.IsNotNull(sheet.GetCellComment(3, 0));
            Assert.IsNotNull(sheet.GetCellComment(4, 0));

            comment1_Shifted = sheet.GetCellComment(0, 0).String.String;
            Assert.AreEqual(comment1, comment1_Shifted);
            comment3_Shifted = sheet.GetCellComment(3, 0).String.String;
            Assert.AreEqual(comment3, comment3_Shifted);
            comment4_Shifted = sheet.GetCellComment(4, 0).String.String;
            Assert.AreEqual(comment4, comment4_Shifted);
        }
        [TestMethod]
        public void BaseTestShiftWithNames()
        {
            Workbook wb = _testDataProvider.CreateWorkbook();
            Sheet sheet1 = wb.CreateSheet("Sheet1");
            wb.CreateSheet("Sheet2");
            Row row = sheet1.CreateRow(0);
            row.CreateCell(0).SetCellValue(1.1);
            row.CreateCell(1).SetCellValue(2.2);

            Name name1 = wb.CreateName();
            name1.NameName = ("name1");
            name1.RefersToFormula = ("Sheet1!$A$1+Sheet1!$B$1");

            Name name2 = wb.CreateName();
            name2.NameName = ("name2");
            name2.RefersToFormula = ("Sheet1!$A$1");

            //refers to A1 but on Sheet2. Should stay unaffected.
            Name name3 = wb.CreateName();
            name3.NameName = ("name3");
            name3.RefersToFormula = ("Sheet2!$A$1");

            //The scope of this one is Sheet2. Should stay unaffected.
            Name name4 = wb.CreateName();
            name4.NameName = ("name4");
            name4.RefersToFormula = ("A1");
            name4.SheetIndex = (1);

            sheet1.ShiftRows(0, 1, 2);  //shift down the top row on Sheet1.
            name1 = wb.GetNameAt(0);
            Assert.AreEqual("Sheet1!$A$3+Sheet1!$B$3", name1.RefersToFormula);

            name2 = wb.GetNameAt(1);
            Assert.AreEqual("Sheet1!$A$3", name2.RefersToFormula);

            //name3 and name4 refer to Sheet2 and should not be affected
            name3 = wb.GetNameAt(2);
            Assert.AreEqual("Sheet2!$A$1", name3.RefersToFormula);

            name4 = wb.GetNameAt(3);
            Assert.AreEqual("A1", name4.RefersToFormula);
        }
        [TestMethod]
        public void BaseTestShiftWithMergedRegions()
        {
            Workbook wb = _testDataProvider.CreateWorkbook();
            Sheet sheet = wb.CreateSheet();
            Row row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue(1.1);
            row.CreateCell(1).SetCellValue(2.2);
            CellRangeAddress region = new CellRangeAddress(0, 0, 0, 2);
            Assert.AreEqual("A1:C1", region.FormatAsString());

            sheet.AddMergedRegion(region);

            sheet.ShiftRows(0, 1, 2);
            region = sheet.GetMergedRegion(0);
            Assert.AreEqual("A3:C3", region.FormatAsString());
        }

        /**
         * See bug #34023
         *
         * @param sampleName the sample file to test against
         */
        [TestMethod]
        public void BaseTestShiftWithFormulas()
        {
            Workbook wb = _testDataProvider.OpenSampleWorkbook("ForShifting." + _testDataProvider.StandardFileNameExtension);

            Sheet sheet = wb.GetSheet("Sheet1");
            Assert.AreEqual(20, sheet.LastRowNum);

            ConfirmRow(sheet, 0, 1, 171, 1, "ROW(D1)", "100+B1", "COUNT(D1:E1)");
            ConfirmRow(sheet, 1, 2, 172, 1, "ROW(D2)", "100+B2", "COUNT(D2:E2)");
            ConfirmRow(sheet, 2, 3, 173, 1, "ROW(D3)", "100+B3", "COUNT(D3:E3)");

            ConfirmCell(sheet, 6, 1, 271, "200+B1");
            ConfirmCell(sheet, 7, 1, 272, "200+B2");
            ConfirmCell(sheet, 8, 1, 273, "200+B3");

            ConfirmCell(sheet, 14, 0, 0.0, "A12"); // the cell referred to by this formula will be Replaced

            // -----------
            // Row index 1 -> 11 (row "2" -> row "12")
            sheet.ShiftRows(1, 1, 10);

            // Now check what sheet looks like after move

            // no Changes on row "1"
            ConfirmRow(sheet, 0, 1, 171, 1, "ROW(D1)", "100+B1", "COUNT(D1:E1)");

            // row "2" is now empty
            ConfirmEmptyRow(sheet, 1);

            // Row "2" moved to row "12", and the formula has been updated.
            // note however that the cached formula result (2) has not been updated. (POI differs from Excel here)
            ConfirmRow(sheet, 11, 2, 172, 1, "ROW(D12)", "100+B12", "COUNT(D12:E12)");

            // no Changes on row "3"
            ConfirmRow(sheet, 2, 3, 173, 1, "ROW(D3)", "100+B3", "COUNT(D3:E3)");


            ConfirmCell(sheet, 14, 0, 0.0, "#REF!");


            // Formulas on rows that weren't Shifted:
            ConfirmCell(sheet, 6, 1, 271, "200+B1");
            ConfirmCell(sheet, 7, 1, 272, "200+B12"); // this one moved
            ConfirmCell(sheet, 8, 1, 273, "200+B3");

            // check formulas on other sheets
            Sheet sheet2 = wb.GetSheet("Sheet2");
            ConfirmCell(sheet2, 0, 0, 371, "300+Sheet1!B1");
            ConfirmCell(sheet2, 1, 0, 372, "300+Sheet1!B12");
            ConfirmCell(sheet2, 2, 0, 373, "300+Sheet1!B3");

            ConfirmCell(sheet2, 11, 0, 300, "300+Sheet1!#REF!");


            // Note - named ranges formulas have not been updated
        }

        private static void ConfirmRow(Sheet sheet, int rowIx, double valA, double valB, double valC,
                    String formulaA, String formulaB, String formulaC)
        {
            ConfirmCell(sheet, rowIx, 4, valA, formulaA);
            ConfirmCell(sheet, rowIx, 5, valB, formulaB);
            ConfirmCell(sheet, rowIx, 6, valC, formulaC);
        }

        private static void ConfirmCell(Sheet sheet, int rowIx, int colIx,
                double expectedValue, String expectedFormula)
        {
            Cell cell = sheet.GetRow(rowIx).GetCell(colIx);
            Assert.AreEqual(expectedValue, cell.NumericCellValue, 0.0);
            Assert.AreEqual(expectedFormula, cell.CellFormula);
        }
    }
}




