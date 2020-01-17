using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using FlexCel.Core;
using FlexCel.XlsAdapter;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.DB;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances.Data;
using Proryv.AskueARM2.Server.DBAccess.Public.Utils;
using Proryv.AskueARM2.Server.VisualCompHelpers.Data;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;

namespace Proryv.AskueARM2.Server.VisualCompHelpers
{
    public partial class ExcelReportFreeHierarchyAdapter
    {
        private MemoryStream ФормируемПриложение51(XlsFileExBase xls, BalanceFreeHierarchyCalculatedResult balanceCalculatedResult,
            bool isHeaderFormed)
        {
            xls.ActiveSheet = 1;
            var sheetName = xls.SheetName = "Приложение";
            xls.SetPrintMargins(new TXlsMargins(0.75, 1, 0.75, 1, 0.5, 0.5));
            xls.SheetZoom = 100;

            #region Добавление форматов

            var f = xls.GetCellVisibleFormatDef(1, 4);
            f.Font.Name = "Tahoma";
            f.Font.Family = 2;
            f.HAlignment = THFlxAlignment.center;
            f.WrapText = true;

            if (_doublePrecisionProfile == 0)
            {
                f.Format = "#,##0";
            }
            else
            {
                f.Format = "#,##0." + new string(_need0 ? '0' : '#', _doublePrecisionProfile);
            }

            var centerFormat = xls.AddFormat(f);

            f = xls.GetCellVisibleFormatDef(4, 1);

            if (_doublePrecisionProfile == 0)
            {
                f.Format = "#,##0";
            }
            else
            {
                f.Format = "#,##0." + new string(_need0 ? '0' : '#', _doublePrecisionProfile);
            }

            f.Font.Name = "Tahoma";
            f.Font.Family = 2;
            f.Borders.Left.Style = TFlxBorderStyle.Thin;
            f.Borders.Left.Color = Color.FromArgb(0x00, 0x00, 0x00);
            f.Borders.Right.Style = TFlxBorderStyle.Thin;
            f.Borders.Right.Color = Color.FromArgb(0x00, 0x00, 0x00);
            f.Borders.Top.Style = TFlxBorderStyle.Thin;
            f.Borders.Top.Color = Color.FromArgb(0x00, 0x00, 0x00);
            f.Borders.Bottom.Style = TFlxBorderStyle.Thin;
            f.Borders.Bottom.Color = Color.FromArgb(0x00, 0x00, 0x00);
            f.HAlignment = THFlxAlignment.center;
            f.VAlignment = TVFlxAlignment.center;
            f.WrapText = true;
            var borderedCenterFormat = xls.AddFormat(f);

            f.Font.Style = TFlxFontStyles.Bold;

            var boldBorderedCenterFormat = xls.AddFormat(f);

            var startRow = 18;
            var startCol = 1;

            #endregion

            xls.ProfileFormat = xls.NoDecimalFormat = borderedCenterFormat;
            
            List<Dict_Balance_FreeHierarchy_Section> sections = null;
            if (_balanceCalculated.SectionsByType != null)
            {
                _balanceCalculated.SectionsByType.TryGetValue(balanceCalculatedResult.BalanceFreeHierarchyType, out sections);
            }

            if (!isHeaderFormed)
            {
                #region Шапка

                xls.SetColWidth(1, 9142); //(34.96 + 0.75) * 256
                xls.SetColWidth(2, 4554); //(11.96 + 0.75) * 256
                xls.SetColWidth(3, 6582); //(24.96 + 0.75) * 256
                xls.SetColWidth(4, 6582); //(24.96 + 0.75) * 256

                xls.MergeCells(10, 1, 10, 3);
                xls.MergeCells(12, 1, 12, 3);
                xls.MergeCells(1, 2, 3, 3);
                xls.MergeCells(4, 1, 4, 3);
                xls.MergeCells(6, 1, 6, 3);
                xls.MergeCells(8, 1, 8, 3);

                xls.SetCellFormat(1, 1, 3, 3, centerFormat);
                xls.SetCellValue(1, 2, "Приложение № 63\nк приказу Минэнерго России\nот 23 июля 2012г. №340");
                

                xls.SetCellFormat(4, 1, 4, 3, boldBorderedCenterFormat);

                //_xls.Add

                //_xls.SetCellFromHtml(4, 1, "<b>121212</b> weqeeqewq <i>wwewewewe</i>");
                xls.SetCellValue(4, 1,
                    "Показатели баланса производства и потребления электроэнергии\n" +
                    "и отпуска тепловой энергии по субьектам электроэнергетики\n" +
                    "в границах субьектов Российской Федерации\n" +
                    "за " + DateTime.Now.Date.Year.ToString() + " год.");

                xls.SetRowHeight(4, 1000);

                 xls.SetCellFormat(6, 1, 6, 3, borderedCenterFormat);
                xls.SetCellValue(6, 1, "КОНФИДЕНЦИАЛЬНОСТЬ ГАРАНТИРУЕТСЯ ПОЛУЧАТЕЛЕМ ИНФОРМАЦИИ");
                xls.SetCellFormat(8, 1, 8, 3, borderedCenterFormat);
                xls.SetCellValue(8, 1, "ВОЗМОЖНО ПРЕДСТАВЛЕНИЕ В ЭЛЕКТРОННОМ ВИДЕ");
                xls.SetCellFormat(9, 1, 10, 3, borderedCenterFormat);
                xls.SetCellValue(10, 1, "1. Предоставляется электростанциями генерирующих компаний и других собственников, ФГУП `Концерн " +
                                         "Росэнергоатом`, котельными");
                xls.SetRowHeight(10, 500);
                xls.SetCellFormat(12, 1, 12, 3, borderedCenterFormat);
                xls.SetCellValue(12, 1, balanceCalculatedResult.DocumentName.Replace("№51", "№63") + " филиал " + _branchName);
                xls.SetCellFormat(13, 1, 14, 3, centerFormat);
                xls.SetCellValue(14, 2, string.Format("{0:dd.MM.yyyy}", _dtEnd));
                xls.SetCellValue(14, 3, _unitDigitName + ", " + _unitDigitHeatName);


                #endregion
            }
            else
            {
                //Обрабатываем наши ф-ии
                SpreadsheetFunctionHelper.Evaluate(xls, this, out startRow, _errors);
            }

            #region Заголовок таблицы

            xls.SetCellFormat(startRow, 1, startRow + 2, 3, borderedCenterFormat);
            xls.SetCellValue(startRow, 1, "Наименование показателя");
            xls.MergeCells(startRow, 1, startRow + 1, 1);

            xls.SetCellValue(startRow, 2, "Фактические значения показателя");
            xls.MergeCells(startRow, 2, startRow, 3);
            xls.SetRowHeight(startRow, 400);

            xls.SetCellValue(startRow + 1, 2, "за сутки");
            xls.SetCellValue(startRow + 1, 3, "нарастающим итогом с начала месяца");

            startRow++; startRow++;

            xls.SetCellValue(startRow, 1, "А");
            xls.SetCellValue(startRow, 2, "1");
            xls.SetCellValue(startRow, 3, "2");

            startRow++;

            #endregion

            #region Таблица

            if (_doublePrecisionProfile == 0)
            {
                f.Format = "#,##0";
            }
            else
            {
                f.Format = "#,##0." + new string(_need0 ? '0' : '#', _doublePrecisionProfile);
            }

            var boldBorderedDoubleFormat = xls.AddFormat(f);
            xls.ProfileBoldFormat = boldBorderedDoubleFormat;
            var sectionNumber = 1;
            var row = startRow;
            double potreb = 0.0, potrebDaily = 0.0;
            foreach (var section in sections.OrderBy(s => s.SortNumber))
            {
                xls.SetCellValue(row, startCol, section.SectionName, boldBorderedCenterFormat);
                xls.SetCellValue(row, startCol + 1, row - startRow + 1, boldBorderedCenterFormat);
                var sectionRow = row;
                List<BalanceFreeHierarchyItemParams> itemParams; //Все объекты для данного раздела
                balanceCalculatedResult.ItemsParamsBySection.TryGetValue(section.Section_UN, out itemParams);
                var formulaIdSumm = "summ" + section.SortNumber;
                var formulaIdDailySumm = "dailySumm" + section.SortNumber;

                double sectionSum = 0.0, sectionDailySum = 0.0;
                var itemNumber = 1;
                if (itemParams != null && itemParams.Count > 0)
                {
                    //Перебираем объекты в разделе
                    foreach (var itemParam in itemParams.OrderBy(itm=>itm.SortNumber))
                    {
                        TVALUES_DB latestDay = null;
                        var daysSumm = 0.0;
                        var coeff = itemParam.Coef ?? 1;
                        if (itemParam.HalfHours != null)
                        {
                            var archives = MyListConverters.ConvertHalfHoursToOtherList(_discreteType, itemParam.HalfHours, 0, _intervalTimeList);
                            latestDay = archives.LastOrDefault();
                            //Нарастающее с начала месяца
                            daysSumm = archives.Sum(itm => itm.F_VALUE);
                        }
                        
                        //Объекты в разделе отображаем только для поступления
                        if (Equals(section.MetaString1, "postupilo"))
                        {
                            row++;
                            xls.SetCellValue(row, startCol, sectionNumber + "." + itemNumber + " " + itemParam.Name, borderedCenterFormat);
                            //_xls.SetCellValue(row, startCol + 1, row - startRow + 1, borderedCenterFormat);

                            if (latestDay != null)
                            {
                                //Факт за последние сутки
                                xls.SetCellFloatValue(row, startCol + 1, latestDay.F_VALUE * coeff, false);
                            }
                            else
                            {
                                xls.SetCellFormat(row, startCol + 1, borderedCenterFormat);
                            }

                            xls.SetCellFloatValue(row, startCol + 2, daysSumm * coeff, false);

                            AddRowRangeToFormulaSum(xls, formulaIdSumm, new FormulaRowsRange
                            {
                                Row1 = row,
                                Row2 = row,
                                Col = startCol + 1,
                            });

                            AddRowRangeToFormulaSum(xls, formulaIdDailySumm, new FormulaRowsRange
                            {
                                Row1 = row,
                                Row2 = row,
                                Col = startCol + 2,
                            });

                            itemNumber++;
                        }
                        
                        if (latestDay != null) sectionSum += latestDay.F_VALUE * coeff;
                        sectionDailySum += daysSumm * coeff;
                    }
                }

                if (Equals(section.MetaString1, "postupilo"))
                {
                    potreb += sectionSum;
                    potrebDaily += sectionDailySum;
                }
                else if (Equals(section.MetaString1, "saldo"))
                {
                    potreb -= sectionSum;
                    potrebDaily -= sectionDailySum;
                }

                if (Equals(section.MetaString1, "potreb"))
                {
                    sectionSum = potreb;
                    sectionDailySum = potrebDaily;
                }
               
                if (!string.IsNullOrEmpty(section.MetaString1))
                {
                    if (!Equals(section.MetaString1, "potreb"))
                    {
                        WriteFormulaToCell(xls, formulaIdSumm, sectionRow, startCol + 1, boldBorderedDoubleFormat, sectionSum);
                        WriteFormulaToCell(xls, formulaIdDailySumm, sectionRow, startCol + 2, boldBorderedDoubleFormat, sectionDailySum);
                    }
                    else
                    {
                        xls.SetCellFloatValue(sectionRow, startCol + 1, sectionSum, false);
                        xls.SetCellFloatValue(sectionRow, startCol + 2, sectionDailySum, false);
                    }
                }
                else
                {
                    xls.SetCellFormat(sectionRow, startCol + 1, sectionRow, startCol + 2, boldBorderedDoubleFormat);
                }
                sectionNumber ++;
                row++;
            }


            #endregion

            return Export(xls);
        }
    }
}
