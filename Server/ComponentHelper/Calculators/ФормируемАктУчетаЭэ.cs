using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FlexCel.Core;
using FlexCel.XlsAdapter;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances.Data;
using Proryv.AskueARM2.Server.DBAccess.Public.Utils;
using Proryv.AskueARM2.Server.VisualCompHelpers.Data;
using Proryv.Servers.Calculation.Parser.Internal;

namespace Proryv.AskueARM2.Server.VisualCompHelpers
{
    public partial class ExcelReportFreeHierarchyAdapter
    {
        private MemoryStream ФормируемАктУчетаЭэ(XlsFileExBase xls, BalanceFreeHierarchyCalculatedResult balanceCalculatedResult,
            bool isHeaderFormed)
        {
            xls.ActiveSheet = 1;
            xls.SheetName = "Report";
            xls.SheetZoom = 100;
            xls.StartRow = 5;
            xls.StartCol = 1;

            var f = xls.GetCellVisibleFormatDef(3, 6);
            f.Font.Style = TFlxFontStyles.Bold;
            //if (_adapterType == TExportExcelAdapterType.toHTML)
            //{
            //    f.FillPattern.Pattern = TFlxPatternStyle.Solid;
            //    f.FillPattern.FgColor = Color.FromArgb(255, 244, 250, 254);
            //}
            var boldFormat = xls.AddFormat(f);

            List<IFreeHierarchyBalanceSignature> signatures = null;
            if (_signaturesByBalance!=null)
            {
                _signaturesByBalance.TryGetValue(balanceCalculatedResult.BalanceFreeHierarchyUn, out signatures);
            }

            #region Шапка

            if (!isHeaderFormed)
            {
                xls.SetCellValue(1, 3, "Акт учета (оборота) электрической энергии ");
                xls.SetCellValue(2, 3, "Субъект ОРЭ:   " + _branchName);
                xls.SetCellValue(3, 3, "Расчетный период:  " + _dtStart.ToString("dd-MM-yyyy HH:mm") + "-" + _dtEnd.ToString("dd-MM-yyyy HH:mm"));
            }
            else
            {
                //Обрабатываем наши ф-ии
                SpreadsheetFunctionHelper.Evaluate(xls, this, out xls.StartRow, _errors);
            }

            #endregion

            //f.Font.Name = "Arial Cyr";
            //f.Font.Size20 = 180;
            f.Font.Style = TFlxFontStyles.None;
            f.VAlignment = TVFlxAlignment.center;
            f.HAlignment = THFlxAlignment.center;
            f.Borders.Bottom.Color = _borderColor;
            f.Borders.Left.Color = _borderColor;
            f.Borders.Right.Color = _borderColor;
            f.Borders.Top.Color = _borderColor;

            f.Borders.Bottom.Style = TFlxBorderStyle.Dotted;
            f.Borders.Left.Style = TFlxBorderStyle.Dotted;
            f.Borders.Right.Style = TFlxBorderStyle.Dotted;
            f.Borders.Top.Style = TFlxBorderStyle.Dotted;
            f.WrapText = true;
            var centerFormat = xls.AddFormat(f);

            f.WrapText = false;
            f.HAlignment = THFlxAlignment.right;
            var rightFormat = xls.AddFormat(f);

            f.Font.Style = TFlxFontStyles.Bold;
            var rightBoldFormat = xls.AddFormat(f);

            f.Format = "### ### ### ### ##0." + new string('0', _doublePrecisionProfile);
            var rightDoubleBoldFormat = xls.AddFormat(f);

            f.Font.Style = TFlxFontStyles.None;
            var rightDoubleFormat = xls.AddFormat(f);

            f.HAlignment = THFlxAlignment.left;
            f.Font.Style = TFlxFontStyles.None;
            f.WrapText = true;
            f.Format = "";
            var leftFormat = xls.AddFormat(f);

            xls.Row = xls.StartRow;

            var days = new List<int>();
            var hours = 0;

            #region Колонка с датой временем

            var prevDay = 0;
            var prevRow = -1;
            foreach (var dt in _dts)
            {
                //смена дня
                if (dt.Day != prevDay)
                {
                    if (prevRow > 0)
                    {
                        xls.MergeCells(prevRow, xls.StartCol, xls.Row, xls.StartCol);
                        xls.SetCellFormat(prevRow, xls.StartCol, xls.Row, xls.StartCol, centerFormat);
                        xls.SetCellValue(xls.Row, xls.StartCol + 1, "Итого", rightBoldFormat);
                        xls.Row++;
                        days.Add(hours);
                        xls.Row++;
                        hours = 0;
                    }

                    xls.SetCellValue(xls.Row, xls.StartCol, "Дата", centerFormat);
                    xls.MergeCells(xls.Row, xls.StartCol, xls.Row + 3, xls.StartCol);
                    xls.SetCellValue(xls.Row, xls.StartCol + 1, "Время", centerFormat);
                    xls.MergeCells(xls.Row, xls.StartCol + 1, xls.Row + 3, xls.StartCol + 1);

                    prevDay = dt.Day;
                    xls.Row = xls.Row + 4;
                    prevRow = xls.Row;

                    xls.SetCellValue(xls.Row, xls.StartCol, dt.ToString("dd.MM.yyyy"), rightFormat);
                }

                xls.SetCellValue(xls.Row, xls.StartCol + 1, dt.ToString("HH:mm-") + dt.AddMinutes(((int)_discreteType + 1) * 30).ToString("HH:mm"), rightFormat);
                hours++;
                xls.Row++;
            }

            //Последняя запись
            if (prevRow > 0)
            {
                xls.MergeCells(prevRow, xls.StartCol, xls.Row, xls.StartCol);
                xls.SetCellFormat(prevRow, xls.StartCol, xls.Row, xls.StartCol, centerFormat);
                xls.SetCellValue(xls.Row, xls.StartCol + 1, "Итого", rightBoldFormat);
                days.Add(hours);
            }

            //Итого за период
            xls.Row++;
            xls.SetCellValue(xls.Row, xls.StartCol, "ИТОГО за период, " + _unitDigitName, rightBoldFormat);
            xls.MergeCells(xls.Row, xls.StartCol, xls.Row, xls.StartCol + 1);

            #endregion

            xls.Col = xls.StartCol + 2;
            foreach (var itemParams in balanceCalculatedResult.ItemsParamsBySection.Values)
            {
                foreach (var itemParam in itemParams.OrderBy(itm => itm.SortNumber))
                {
                    if (itemParam.HalfHours == null) continue;

                    var archives = MyListConverters.ConvertHalfHoursToOtherList(_discreteType, itemParam.HalfHours, 0, _intervalTimeList);

                    xls.SetColWidth(xls.Col, 4364);
                    xls.Row = xls.StartRow;
                    var totalHours = 0;
                    var totalSum = 0.0;
                    //Перебираем дни
                    foreach (var dayHours in days)
                    {
                        xls.SetCellValue(xls.Row, xls.Col, itemParam.Name + ",\n" + _unitDigitName, centerFormat);
                        xls.MergeCells(xls.Row, xls.Col, xls.Row + 3, xls.Col);
                        xls.Row = xls.Row + 4;
                        var daySum = 0.0;
                        var row1 = xls.Row;
                        //Перебираем часы в этом дне
                        for (var hour = 0; hour < dayHours; hour++)
                        {
                            var aVal = archives.ElementAtOrDefault(totalHours + hour);
                            if (aVal != null)
                            {
                                daySum += aVal.F_VALUE;
                                xls.SetCellValue(xls.Row, xls.Col, aVal.F_VALUE, rightDoubleFormat);
                            }
                            else
                            {
                                xls.SetCellFormat(xls.Row, xls.Col, rightDoubleFormat);
                            }
                            xls.Row++;
                        }

                        //Итого
                        WriteRowRangeFormulaToCell(xls, "-1", xls.Row, xls.Col, rightDoubleFormat, daySum, new FormulaRowsRange { Col = xls.Col, Row1 = row1, Row2 = xls.Row - 1 });
                        AddRowRangeToFormulaSum(xls, "1", new FormulaRowsRange { Col = xls.Col, Row1 = xls.Row, Row2 = xls.Row });
                        totalSum += daySum;
                        xls.Row++;
                        xls.Row++;

                        totalHours += dayHours;
                    }

                    //Итого за период
                    WriteRowRangeFormulaToCell(xls, "1", xls.Row - 1, xls.Col, rightDoubleBoldFormat, totalSum);
                    //_xls.SetCellValue(xls._row - 1, xls._col, totalSum, rightDoubleBoldFormat);

                    xls.Col++;
                }
            }

            #region Подписанты

            xls.StartRow = xls.Row;

            xls.Row++;
            xls.Row++;

            WriteSignatures(xls, signatures, boldFormat, leftFormat);

            //_xls.SetCellFormat(startRow, 1, xls._row, 9, leftFormat);

            #endregion

            xls.SetCellFormat(1, 1, 4, xls.Col - 1, boldFormat);

            return Export(xls);
        }

        private void WriteSignatures(XlsFileExBase xls, List<IFreeHierarchyBalanceSignature> signatures, int boldFormat, int leftFormat)
        {
            if (signatures == null)
            {
                xls.Row = xls.Row + 9;
                xls.SetCellFormat(xls.StartRow, 1, xls.Row, xls.Col, leftFormat);
                return;
            }

            foreach (var signature in signatures.GroupBy(s => s.Группа ?? s.Группа))
            {
                xls.SetCellValue(xls.Row, 1, "Согласовано:", boldFormat);
                xls.SetCellFormat(xls.Row, 1, xls.Row, Math.Max(xls.Col, 5), boldFormat);
                xls.MergeCells(xls.Row, 1, xls.Row, 2);

                xls.Row++;
                xls.SetCellFormat(xls.Row, 1, xls.Row, Math.Max(xls.Col, 5), boldFormat);

                foreach (var s in signature)
                {
                    xls.Row++;
                    xls.SetCellValue(xls.Row, 1, s.Должность, leftFormat);
                    xls.MergeCells(xls.Row, 1, xls.Row, 2);
                    //_xls.SetCellValue(xls._row, 2, "_______________________", leftFormat);
                    xls.SetCellValue(xls.Row, 3, s.ФИО, leftFormat);
                    xls.MergeCells(xls.Row, 3, xls.Row, 5);
                    xls.SetCellFormat(xls.Row, 1, xls.Row, Math.Max(xls.Col, 5), leftFormat);
                    xls.Row++;
                    xls.SetCellFormat(xls.Row, 1, xls.Row, Math.Max(xls.Col, 5), leftFormat);
                }

                xls.Row++;
            }
        }
    }
}
