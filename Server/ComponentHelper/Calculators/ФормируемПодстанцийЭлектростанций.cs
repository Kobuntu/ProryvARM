using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using FlexCel.Core;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances.Data;
using Proryv.AskueARM2.Server.DBAccess.Public.Utils;
using Proryv.AskueARM2.Server.VisualCompHelpers.Calculators;
using Proryv.AskueARM2.Server.VisualCompHelpers.Data;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.DBAccess.Enums;
using FlexCel.XlsAdapter;
using Proryv.Servers.Calculation.Parser.Internal;
using Proryv.Servers.Calculation.DBAccess.Interface.Documents;
using Proryv.AskueARM2.Server.DBAccess.Internal.DB;
using System.IO;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;

namespace Proryv.AskueARM2.Server.VisualCompHelpers
{
    public partial class ExcelReportFreeHierarchyAdapter
    {
        private MemoryStream ФормируемПодстанцийЭлектростанций(XlsFileExBase xls, BalanceFreeHierarchyCalculatedResult balanceCalculatedResult, 
            bool isHeaderFormed)
        {
            xls.ActiveSheet = 1;
            xls.SheetName = "Title";
            var startRow = 17;
            var startCol = 1;

            //Баланс строится сразу за весь интервал времени (не по периодам дискретизации)
            var isIntervalBalance = _discreteType == enumTimeDiscreteType.DBInterval;

            const string formulaIdPostupilo_1 = "Postupilo";
            const string formulaIdRashod_2 = "Rashod";
            const string formulaIdOtpusk_3 = "Otpusk";
            const string formulaIdOtpuskShin_5 = "OtpuskShin";
            
            var commentProps = new TCommentProperties
            {
                Anchor = new TClientAnchor(TFlxAnchorType.DontMoveAndDontResize, 1, 30, 7, 502, 9, 240, 9, 900),
                ShapeFill = new TShapeFill(true, new TSolidFill(Colors.White))
            };

            var integralCoeff = 1/(double) _unitDigitIntegrals;
            var hhCoeff = 1 / (double)_unitDigit;

            var fmt = xls.GetCellVisibleFormatDef(3, 6);
            fmt.Font.Size20 = DblFontZoom*9;
            fmt.Font.Style = TFlxFontStyles.Bold;
            fmt.VAlignment = TVFlxAlignment.top;
            var ff = xls.AddFormat(fmt);

            List<IFreeHierarchyBalanceSignature> signatures = null;
            if (_signaturesByBalance != null)
            {
                _signaturesByBalance.TryGetValue(balanceCalculatedResult.BalanceFreeHierarchyUn, out signatures);
            }

            List<Dict_Balance_FreeHierarchy_Section> sections = null;
            if (_balanceCalculated.SectionsByType!=null)
            {
                _balanceCalculated.SectionsByType.TryGetValue(balanceCalculatedResult.BalanceFreeHierarchyType, out sections);
            }

            #region Шапка

            if (!isHeaderFormed)
            {
                xls.DefaultColWidth = 2364;
                xls.SetColWidth(3, 4064);
                xls.SetColWidth(4, 3064);
                xls.SetColWidth(5, 4364);
                xls.SetColWidth(7, 4364);
                xls.SetColWidth(8, 4064);
                xls.SetColWidth(9, 6364);

                xls.SetCellFormat(1, 1, startRow, 9, ff);
                xls.SetCellValue(3, 6, "АКТ ");
                xls.SetCellValue(4, 3,
                    "О СОCТАВЛЕНИИ БАЛАНСА ЭЛЕКТРОЭНЕРГИИ НА " + balanceCalculatedResult.DocumentName +
                    (string.IsNullOrEmpty(_branchName) ? string.Empty : " ФИЛИАЛЕ " + _branchName));

                xls.SetCellValue(5, 3, "Комиссия в составе:");
                if (signatures != null)
                {
                    int r = 6;
                    foreach (var signatureGrouped in signatures.GroupBy(s => s.Группа))
                    {
                        xls.SetCellValue(r, 3, signatureGrouped.Key + ":");
                        foreach (var signature in signatureGrouped)
                        {
                            xls.SetCellValue(r, 4, signature.Должность + " " + signature.ФИО);
                            xls.MergeCells(r, 4, r, 9);
                            r++;
                        }
                        r++;
                    }
                }
                else
                {
                    xls.SetCellValue(6, 3, "Председатель:");
                    xls.SetCellValue(6, 5, "главный инженер ");
                    xls.SetCellValue(7, 3, "Члены: ");
                    xls.SetCellValue(7, 5, "начальник ПТО ");
                    xls.SetCellValue(8, 5, "начальник ЭЦ       ");
                    xls.SetCellValue(10, 4, "представитель " + _branchName + ":");
                    xls.SetCellValue(11, 4, "ведущий -инженер инспектор  ");
                }

                xls.SetCellValue(13, 3, "   Настоящий акт составлен в том, что за период с ");
                fmt.Format = "DD.MM.YYYY HH:MM";
                //var fd = _xls.AddFormat(fmt);

                xls.SetCellValue(13, 5, string.Format("{0:dd.MM.yyyy HH:mm}  по  {1:dd.MM.yyyy HH:mm}", _dtStart, _dtEnd.AddMinutes(30)));

                xls.SetCellValue(14, 3, "выработка электроэнергии, потребление на собственные  и хозяйственные нужды электростанции,");
                xls.SetCellValue(15, 3, "отпуск электроэнергии потребителям, в сети электросетевых и генерирующих компаний следующие:");
            }
            else
            {
                //Обрабатываем наши ф-ии
                SpreadsheetFunctionHelper.Evaluate(xls, this, out startRow, _errors);
            }

            #endregion

            #region Настройка

            var f = xls.GetCellVisibleFormatDef(1,1);
            f.Font.Color = Color.Black;
            //if (_adapterType == TExportExcelAdapterType.toHTML)
            //{
            //    f.FillPattern.Pattern = TFlxPatternStyle.Solid;
            //    f.FillPattern.FgColor = Color.FromArgb(255, 244, 250, 254);
            //}
            //f.Font.Name = "Arial Cyr";
            //f.Font.Size20 = 180;
            f.Font.Style = TFlxFontStyles.Bold;
            f.VAlignment = TVFlxAlignment.center;
            f.HAlignment = THFlxAlignment.center;
            f.Borders.Bottom.Color = _borderColor;
            f.Borders.Left.Color = _borderColor;
            f.Borders.Right.Color = _borderColor;
            f.Borders.Top.Color = _borderColor;

            f.Borders.Bottom.Style = TFlxBorderStyle.Thin;
            f.Borders.Left.Style = TFlxBorderStyle.Thin;
            f.Borders.Right.Style = TFlxBorderStyle.Thin;
            f.Borders.Top.Style = TFlxBorderStyle.Thin;
            f.WrapText = true;
            var centerBoldThinFormat = xls.AddFormat(f);

            f.Borders.Bottom.Style = TFlxBorderStyle.Thin;
            f.Borders.Left.Style = TFlxBorderStyle.Thin;
            f.Borders.Right.Style = TFlxBorderStyle.Thin;
            f.Borders.Top.Style = TFlxBorderStyle.Thin;
            var centerBoldFormat = xls.AddFormat(f);

            f.HAlignment = THFlxAlignment.left;
            var leftBoldFormat = xls.AddFormat(f);

            f.HAlignment = THFlxAlignment.right;

            f.Format = "### ### ### ### ##0.000";
            var rightDoubleBoldFormatDefault = xls.AddFormat(f);

            string profileFormat;
            if (_doublePrecisionProfile == 0)
            {
                profileFormat = f.Format = "#,##0";
            }
            else
            {
                profileFormat = f.Format = "### ### ### ### ##0." + new string(_need0 ? '0' : '#', _doublePrecisionProfile);
            }
            //f.Font.Style = TFlxFontStyles.None;
            var profileBoldFormat = xls.AddFormat(f);

            f.Format = "#,##0";
            var noDecimalBoldFormat = xls.AddFormat(f);

            f.HAlignment = THFlxAlignment.right;
            f.Font.Style = TFlxFontStyles.None;

            f.Format = profileFormat;
            xls.ProfileFormat = xls.AddFormat(f);

            f.Format = "#,##0";
            xls.NoDecimalFormat = xls.AddFormat(f);

            string _integralFormatting;

            if (_doublePrecisionIntegral == 0)
            {
                _integralFormatting = f.Format = "#,##0";
            }
            else
            {
                _integralFormatting = f.Format = "#,##0." + new string(_need0 ? '0' : '#', _doublePrecisionIntegral);
            }

            xls.IntegralFormat = xls.AddFormat(f);

            f.Font.Style = TFlxFontStyles.Bold;
            xls.IntegralBoldFormat = xls.AddFormat(f);

            f.Font.Style = TFlxFontStyles.None;

            f.HAlignment = THFlxAlignment.left;
            f.Font.Style = TFlxFontStyles.None;
            f.WrapText = true;
            f.Format = "";
            var leftFormat = xls.AddFormat(f);

            f.HAlignment = THFlxAlignment.right;
            var rightFormat = xls.AddFormat(f);

            f.HAlignment = THFlxAlignment.center;
            var centerFormat = xls.AddFormat(f);

            f.HAlignment = THFlxAlignment.right;
            f.Font.Style = TFlxFontStyles.None;
            f.FillPattern.Pattern = TFlxPatternStyle.Solid;
            f.FillPattern.FgColor = _noDataColor;
            f.Format = profileFormat;
            var rightDoubleFormatNoData = xls.AddFormat(f);

            #endregion

            int row = startRow;

            #region Header

            //Отрисовка заголовка для обычных объектов
            var headerAction = (Action<bool>) ((isShowParamsTitle) =>
            {
                xls.PrintToFit = true;
                xls.PrintLandscape = _printLandscape;
                xls.SheetZoom = 100;
                xls.DefaultColWidth = 3964;
                xls.SetColWidth(1, 1100); //№\nп/п
                xls.SetColWidth(2, 4200); //Номера\nсчетчиков\nустановленных\nЭнергосбытом
                xls.SetColWidth(3, 8700); //Наименование\nобъекта учета
                xls.SetColWidth(4, 4364); //Показания\nсчетчиков окончание
                xls.SetColWidth(5, 4364); //Показания\nсчетчиков начало
                xls.SetColWidth(6, 4364); //Разность\nпоказан.\nсчетчиков\nза период
                xls.SetColWidth(7, 4364); //Коэффи-\nциент.\nсчетчиков
                xls.SetColWidth(8, 4564); //Количество\nэнергии,\nучтенной\nПУ
                xls.SetColWidth(9, 4564); //Количество\nэнергии,\nпо сумме\nполучасовок
                xls.SetColWidth(10, 10000); //Примечание

                if (isShowParamsTitle)
                {
                    xls.SetCellFormat(row, startCol, row + 4, startCol + 9, centerBoldThinFormat);
                    xls.SetCellValue(row, startCol, "№\nп/п", centerBoldThinFormat);
                    xls.SetCellValue(row, startCol + 1, "Номера\nсчетчиков\nустановленных\nЭнергосбытом", centerBoldThinFormat);
                    xls.SetCellValue(row, startCol + 2, "Наименование\nобъекта учета", centerBoldThinFormat);
                    
                }


                if (isIntervalBalance)
                {
                    xls.MergeCells(row, startCol, row + 5, startCol);
                    xls.MergeCells(row, startCol + 1, row + 5, startCol + 1);
                    xls.MergeCells(row, startCol + 2, row + 5, startCol + 2);

                    xls.SetCellValue(row, startCol + 3, "Показания\nсчетчиков", centerBoldThinFormat);
                    xls.MergeCells(row, startCol + 3, row + 2, startCol + 4);

                    xls.SetCellValue(row + 3, startCol + 3, "На 00:00\nчасов\n" + _dtEnd.AddMinutes(30).ToString("dd.MM.yyyy"), centerBoldThinFormat);
                    xls.MergeCells(row + 3, startCol + 3, row + 5, startCol + 3);

                    xls.SetCellValue(row + 3, startCol + 4, "На 00:00\nчасов\n" + _dtStart.ToString("dd.MM.yyyy"), centerBoldThinFormat);
                    xls.MergeCells(row + 3, startCol + 4, row + 5, startCol + 4);

                    xls.SetCellValue(row, startCol + 5, "Разность\nпоказан.\nсчетчиков\nза период", centerBoldThinFormat);
                    xls.MergeCells(row, startCol + 5, row + 5, startCol + 5);

                    xls.SetCellValue(row, startCol + 6, "Коэффи-\nциент.\nсчетчиков", centerBoldThinFormat);
                    xls.MergeCells(row, startCol + 6, row + 5, startCol + 6);

                    xls.SetCellValue(row, startCol + 7, "Количество\nэнергии,\nучтенной\nПУ\n" + _unitDigitIntegralName, centerBoldThinFormat);
                    xls.MergeCells(row, startCol + 7, row + 5, startCol + 7);

                    xls.SetCellValue(row, startCol + 8, "Количество\nэнергии,\nпо сумме\nполучасовок,\n" + _unitDigitName, centerBoldThinFormat);
                    xls.MergeCells(row, startCol + 8, row + 5, startCol + 8);

                    xls.SetCellValue(row, startCol + 9, "Примечание", centerBoldThinFormat);
                    xls.MergeCells(row, startCol + 9, row + 5, startCol + 9);

                    if (IsAnalisIntegral)
                    {
                        xls.SetCellValue(row, startCol + 10, "Сводный анализ\nразница показаний и профиля\n" + _unitDigitName, centerBoldThinFormat);
                        xls.MergeCells(row, startCol + 10, row + 5, startCol + 10);
                        xls.SetColWidth(startCol + 10, 4200);
                        xls.SetCellValue(row + 6, startCol + 10, "11", centerBoldThinFormat);

                        xls.SetCellValue(row, startCol + 11, "Сводный анализ\nразница показаний и профиля\n %", centerBoldThinFormat);
                        xls.MergeCells(row, startCol + 11, row + 5, startCol + 11);
                        xls.SetColWidth(startCol + 11, 4200);
                        xls.SetCellValue(row + 6, startCol + 11, "12", centerBoldThinFormat);

                        xls.SetCellFormat(row + 5, startCol, row + 5, startCol + 11, centerBoldThinFormat);
                    }
                    else
                    {
                        xls.SetCellFormat(row + 5, startCol, row + 5, startCol + 9, centerBoldThinFormat);
                    }

                    row = row + 6;

                    xls.SetCellValue(row, startCol, "1", centerBoldThinFormat);
                    xls.SetCellValue(row, startCol + 1, "2", centerBoldThinFormat);
                    xls.SetCellValue(row, startCol + 2, "3", centerBoldThinFormat);
                    xls.SetCellValue(row, startCol + 3, "4", centerBoldThinFormat);
                    xls.SetCellValue(row, startCol + 4, "5", centerBoldThinFormat);
                    xls.SetCellValue(row, startCol + 5, "6", centerBoldThinFormat);
                    xls.SetCellValue(row, startCol + 6, "7", centerBoldThinFormat);
                    xls.SetCellValue(row, startCol + 7, "8", centerBoldThinFormat);
                    xls.SetCellValue(row, startCol + 8, "9", centerBoldThinFormat);
                    xls.SetCellValue(row, startCol + 9, "10", centerBoldThinFormat);
                }
                else
                {
                    xls.MergeCells(row, startCol, row + 3, startCol);
                    xls.MergeCells(row, startCol + 1, row + 3, startCol + 1);
                    xls.MergeCells(row, startCol + 2, row + 3, startCol + 2);

                    xls.SetCellValue(row + 4, startCol, "1", centerBoldThinFormat);
                    xls.SetCellValue(row + 4, startCol + 1, "2", centerBoldThinFormat);
                    xls.SetCellValue(row + 4, startCol + 2, "3", centerBoldThinFormat);

                    var col = 3;
                    foreach (var dt in _dts)
                    {
                        var c = startCol + col;

                        if (c >= 16385) break; //лимит xlsx

                        var colTitle = "Кол.Энергии\n" + _unitDigitName + "\n" + dt.ToString("dd.MM.yyyy");
                        if (_discreteType == enumTimeDiscreteType.DBHalfHours || _discreteType == enumTimeDiscreteType.DBHours)
                        {
                            colTitle += "\n" + dt.ToString("HH:mm") + "-" + dt.AddMinutes(((int) _discreteType + 1)*30).ToString("HH:mm");
                        }

                        xls.SetCellValue(row, c, colTitle, centerBoldThinFormat);
                        xls.SetCellFormat(row, c, row + 4, c, centerBoldThinFormat);
                        xls.MergeCells(row, c, row + 3, c);
                        xls.SetCellValue(row + 4, c, col + 1, centerBoldThinFormat);
                        xls.SetColWidth(c, 4364);

                        col++;
                    }

                    row = row + 4;
                }

                row++;
            });

            //Отрисовка заголовка для трансформаторов
            var headerTransformatorAction = (Action) (() =>
            {
                xls.SetCellFormat(row, startCol, row, startCol + 8, leftFormat);
                xls.SetCellValue(row, startCol, "№\nп/п");
                xls.SetCellValue(row, startCol + 1, "Трансформатор");
                xls.MergeCells(row, startCol + 1, row, startCol + 2);
                xls.SetCellValue(row, startCol + 3, "Потери ХХ" + "\n" + "(паспортные" + "\n" + "данные)," + "\n" + "МВт");
                xls.SetCellValue(row, startCol + 4, "Количество" + "\n" + "отработанных" + "\n" + "часов" + "\n" + "в отчетном периоде");
                xls.SetCellValue(row, startCol + 5, "ΔWтх" + "\n" + "(потери х. х.)" + "\n" + _unitDigitName);
                xls.SetCellValue(row, startCol + 6, "ΔWтн" + "\n" + "(нагрузочные" + "\n" + "потери)" + "\n" + _unitDigitName);
                xls.SetCellValue(row, startCol + 7, "ΔWтх+ΔWтн\n," + _unitDigitName);

                row++;
            });

            //Отрисовка заголовка для реакторов
            var headerReactorAction = (Action) (() =>
            {
                xls.SetCellValue(row, startCol, "№\nп/п", centerBoldFormat);
                xls.SetCellValue(row, startCol + 1, "Тип оборудования", leftFormat);
                xls.SetCellValue(row, startCol + 2, "Каталожные данные\nSном.МВА\n(Qном,МВАр)", leftFormat);
                xls.SetCellValue(row, startCol + 3, "Δ Ртыс. кВт/МВАр в год", leftFormat);
                xls.SetCellValue(row, startCol + 4, "ΔP хх тыс.\n кВт(паспотные данные)", leftFormat);
                xls.SetCellValue(row, startCol + 5, "Количество\nотработанных\nчасов\nв отчетном\nпериоде", leftFormat);
                xls.SetCellValue(row, startCol + 6, "ΔW (потери\nпо ПУ)\n" + _unitDigitName, leftFormat);
                xls.SetCellValue(row, startCol + 7, "ΔW (потери\nрасчетные)\n" + _unitDigitName, leftFormat);

                row++;
            });

            #endregion

            var analiser = isIntervalBalance ? new ExcelAnaliser(xls, xls.StartCol + 8, xls.StartCol + 9, xls.StartCol + 11,  
                (double)_unitDigit / (double)_unitDigitIntegrals, IsAnalisIntegral, AdapterType, _errors, _dts.Count) : null;

            #region Таблица

            if (!_isFormingSeparateList)
            {
                headerAction(true);
            }

            #region Отображение заголовка группы и подгруппы

            var showSectionTitle = (Action<string>) (title =>
            {
                xls.MergeCells(row, startCol, row, 10);
                xls.SetCellFormat(row, startCol, row, startCol + 9, centerBoldFormat);
                xls.SetCellValue(row, startCol, title);
                row++;
            });

            #endregion

            var delta = isIntervalBalance ? 8 : 3;

            var subsectionUns = new string[3];
            var subsectionSum = new double[3];

            #region проверяем конец ли это подгруппы, если конец, отображаем итог

            var showSectionResult = (Func<string, int, List<TVALUES_DB>, double?, int, string, bool>) ((un, level, aVals, coef, col, sectionUn) =>
            {
                //Сменился ли подраздел данного уровня
                if (!Equals(un, subsectionUns[level]))
                {
                    //Если сменился закрываем вложенные уровни включая текущий
                    for (var l = 2; l >= level; l--)
                    {
                        var prevUn = subsectionUns[l];
                        if (!string.IsNullOrEmpty(prevUn))
                        {
                            string prevBalanceFreeHierarchySubsectionName;
                            _balanceCalculated.SubsectionNames.TryGetValue(prevUn, out prevBalanceFreeHierarchySubsectionName);

                            if (isIntervalBalance)
                            {
                                //Итог по предыдущему подразделу
                                xls.MergeCells(row, startCol, row, startCol + 6);
                                xls.SetCellFormat(row, startCol, row, startCol + 7, leftBoldFormat);
                                xls.SetCellFormat(row, startCol, row, startCol + 8, leftBoldFormat);
                                xls.SetCellValue(row, startCol, "Итого " + prevBalanceFreeHierarchySubsectionName);
                                WriteRowRangeFormulaToCell(xls, l.ToString(), row, startCol + delta + col, profileBoldFormat, subsectionSum[l]);

                                //Добавляем в вышестоящую формулу 
                                if (_isFormingFormulasToCell)
                                {
                                    if (l > 0)
                                    {
                                        AddRowRangeToFormulaSum(xls, (l - 1).ToString(),
                                            new FormulaRowsRange
                                                {Row1 = row, Row2 = row, Col = startCol + delta + col});
                                    }
                                    else if (!string.IsNullOrEmpty(sectionUn))
                                    {
                                        //Добавляем всего по разделу
                                        AddRowRangeToFormulaSum(xls, sectionUn,
                                            new FormulaRowsRange
                                                {Row1 = row, Row2 = row, Col = startCol + delta + col});
                                    }
                                }
                            }

                            row++;
                        }

                        subsectionSum[l] = 0;
                        subsectionUns[l] = null;
                    }

                    //Заголовок нового подразделы
                    if (!string.IsNullOrEmpty(un))
                    {
                        string balanceFreeHierarchySubsectionName;
                        _balanceCalculated.SubsectionNames.TryGetValue(un, out balanceFreeHierarchySubsectionName);
                        showSectionTitle(balanceFreeHierarchySubsectionName);
                    }
                }

                if (isIntervalBalance && !string.IsNullOrEmpty(un) && aVals != null && aVals.Count > 0 && coef.HasValue)
                {
                    //Описан ли вообще данный подраздел и есть ли и объект участвует в общем балансе
                    subsectionSum[level] += aVals.First().F_VALUE*coef.Value;
                    return true;
                }

                return false;
            });

            #endregion

            var inputs = new List<BalanceFreeHierarchyItemParams>();
            var outputs = new List<BalanceFreeHierarchyItemParams>();

            #region Перебираем основные разделы

            var postupilo = new Dictionary<int, double>(); //Поступило на шины, всего по получасовкам (1+2)
            var rashodNaPs = new Dictionary<int, double>(); //Расход электроэнергии на станции (3+4+5)
            var rashodPotreb = new Dictionary<int, double>(); //Отпуск электроэнергии потребителям 
            var otpuskShin = new Dictionary<int, double>(); //Отпуск электроэнергии с шин электростанции,

            var previousType = EnumHeaderType.None;
            var sheetNumber = 2;
            var summVoltageCalc = new SortedList<double, double>(); //Отпуск по напряжению

            var isFormingComment = _discreteType == enumTimeDiscreteType.DBInterval || (_dtEnd - _dtStart).TotalDays < 32;

            foreach (
                var section in
                    sections.Where(
                        s =>
                            !string.IsNullOrEmpty(s.MetaString1) &&
                            (s.MetaString1.IndexOf("postupilo", 0, StringComparison.Ordinal) > -1 || s.MetaString1.IndexOf("rashod", 0, StringComparison.Ordinal) > -1))
                        .OrderBy(s => s.SortNumber))
            {
                List<BalanceFreeHierarchyItemParams> itemParams; //Все объекты для данного раздела
                balanceCalculatedResult.ItemsParamsBySection.TryGetValue(section.Section_UN, out itemParams);

                if (itemParams == null) continue;

                string sheetName = null;
                if (_isFormingSeparateList)
                {
                    row = 1;

                    if (string.IsNullOrEmpty(section.SectionName))
                    {
                        sheetName = "АКТ-" + (sheetNumber - 1);
                    }
                    else
                    {
                        sheetName = "АКТ-" + section.SectionName.Substring(0, section.SectionName.IndexOf(".", StringComparison.Ordinal));
                    }

                    xls.AddSheet();
                    xls.ActiveSheet = sheetNumber++;
                    xls.SheetZoom = 100;
                    xls.PrintScale = 100;
                    xls.SheetName = sheetName;
                }

                showSectionTitle(section.SectionName);

                var pp = 1;
                var isPoteri = string.Equals(section.MetaString1, "rashod_poteri") || Equals(section.MetaString1, "rashod_transf"); //Эта группа потери

                if (!isPoteri && _isFormingSeparateList)
                {
                    headerAction(true);
                }

                foreach (var itemParam in itemParams.OrderBy(itm => itm.SortNumber))
                {
                    if (!itemParam.IsInput && !itemParam.IsOutput) continue; //Объект не участвует в балансе    

                    //var aVal = itemParam.Archives != null ? itemParam.Archives.FirstOrDefault() : null;
                    var col = 0;

                    var archives = MyListConverters.ConvertHalfHoursToOtherList(_discreteType, itemParam.HalfHours, 0, _intervalTimeList);

                    var level = -1;
                    if (showSectionResult(itemParam.BalanceFreeHierarchySubsectionUn, 0, archives,
                        itemParam.Coef, col, itemParam.BalanceFreeHierarchySectionUn))
                    {
                        level++; //проверка смены 1 уровня
                    }

                    if (showSectionResult(itemParam.BalanceFreeHierarchySubsectionUn2, 1, archives, itemParam.Coef, col, null)) level++; //проверка смены 2 уровня
                    if (showSectionResult(itemParam.BalanceFreeHierarchySubsectionUn3, 2, archives, itemParam.Coef, col, null)) level++; //проверка смены 3 уровня

                    #region Для потерь выбираем заголок таблицы потерь

                    if (isPoteri)
                    {
                        EnumHeaderType headerType;
                        switch (itemParam.TypeHierarchy)
                        {
                            case enumTypeHierarchy.PTransformator:
                                headerType = EnumHeaderType.Transformator;
                                break;
                            case enumTypeHierarchy.Reactor:
                                headerType = EnumHeaderType.Reactor;
                                break;
                            default:
                                headerType = EnumHeaderType.Other;
                                break;
                        }

                        if (headerType != previousType)
                        {
                            switch (headerType)
                            {
                                case EnumHeaderType.None:
                                case EnumHeaderType.Other:
                                    headerAction(true);
                                    break;
                                case EnumHeaderType.Transformator:
                                    headerTransformatorAction();
                                    break;
                                case EnumHeaderType.Reactor:
                                    headerReactorAction();
                                    break;
                            }

                            previousType = headerType;
                        }
                    }

                    #endregion

                    xls.SetCellValue(row, startCol, pp++, leftFormat);

                    TPTransformatorsResult transformator = null;

                    //Выводим название и параметры
                    if (isPoteri && itemParam.TypeHierarchy == enumTypeHierarchy.PTransformator)
                    {
                        //Трансформаторы
                        xls.SetCellValue(row, startCol + 1, itemParam.Name, leftFormat);
                        xls.MergeCells(row, startCol + 1, row, startCol + 2);
                        transformator = balanceCalculatedResult.Transformators.FirstOrDefault(tr => tr.PTransformatorProperty.PTransformator_ID == itemParam.Id);
                    }
                    else if (isPoteri && itemParam.TypeHierarchy == enumTypeHierarchy.Reactor)
                    {
                        //Реакторы
                    }
                    else if (itemParam.TypeHierarchy == enumTypeHierarchy.Info_TI || itemParam.TypeHierarchy == enumTypeHierarchy.Info_Integral)
                    {
                        //Обычные объекты
                        xls.SetCellValue(row, startCol + 1, itemParam.MeterSerialNumber, centerFormat);
                        xls.SetCellValue(row, startCol + 2, itemParam.Name, leftFormat);
                    }
                    else
                    {
                        bool isExists;
                        var description = itemParam.TypeHierarchy.GetString(out isExists);
                        if (isExists) xls.SetCellValue(row, startCol + 1, description, centerFormat);
                        xls.SetCellValue(row, startCol + 2, itemParam.Name, leftFormat);
                    }

                    if (archives != null)
                    {
                        if (itemParam.IsInput)
                        {
                            inputs.Add(itemParam);
                        }
                        else
                        {
                            outputs.Add(itemParam);
                        }
                    }

                    #region Трансформаторы

                    if (isPoteri && itemParam.TypeHierarchy == enumTypeHierarchy.PTransformator)
                    {
                        if (transformator != null)
                        {
                            xls.SetCellFloatValue( row, startCol + 3, transformator.PTransformatorProperty.IdlingLosses/1000);
                            // Потери холостого хода (паспортные данные) МВт ч

                            var numbersHours = transformator.NumbersWorkedfHours;
                            xls.SetCellFloatValue( row, startCol + 4, numbersHours); // Количество отработанных часов

                            double losses = 0;
                            var summLosses = losses = transformator.NoLoadLosses * transformator.PTransformatorProperty.SpecificValue.Average() * transformator.NumbersHalfHours;
                            xls.SetCellFloatValue( row, startCol + 5, losses); // Потери холостого хода МВт ч
                            losses = transformator.LoadLosses.Sum();
                            xls.SetCellFloatValue( row, startCol + 6, losses); // Нагрузочные потери МВт ч
                            summLosses += losses;
                            xls.SetCellFloatValue( row, startCol + 7, summLosses); // Сумма нагрузочных потери и потерь холостого хода МВт ч
                        }
                    }

                        #endregion

                    #region Реакторы

                    else if (isPoteri && itemParam.TypeHierarchy == enumTypeHierarchy.Reactor)
                    {
                    }

                    #endregion
                    
                    #region Обычные объекты

                    else
                    {
                        var isPaintNoStartIntegral = false;
                        var isPaintNoFinishIntegral = false;

                        if (isIntervalBalance)
                        {
                            var iVal = Math.Round(itemParam.IntegralsDiffSums * integralCoeff, 7, MidpointRounding.AwayFromZero);

                            //Если общая сумма
                            if (itemParam.IntegralsOnStart != null && itemParam.IntegralsOnEnd != null)
                            {
                                xls.SetCellFloatValue( row, startCol + 3, itemParam.IntegralsOnEnd.F_VALUE * integralCoeff, true);

                                //Отсутствуют интегралы, или не полные
                                if ((itemParam.IntegralsOnEnd.F_FLAG & VALUES_FLAG_DB.AllAlarmStatuses) != VALUES_FLAG_DB.None ||
                                    (_dtEnd - itemParam.IntegralsOnEnd.EventDateTime).TotalMinutes > 1)
                                {
                                    xls.SetCellBkColor( row, startCol + 3, _noDrumsColor);
                                    var c = itemParam.IntegralsOnEnd.EventDateTime.ToString("dd.MM.yyyy HH:mm") + " :\n" + itemParam.IntegralsOnEnd.F_FLAG.FlagToString("\n");
                                    if (isFormingComment && row < MaxCommentRows)
                                    {
                                        xls.SetComment(row, startCol + 3, c);
                                        xls.SetCommentProperties(row, startCol + 3, commentProps);
                                    }
                                    isPaintNoFinishIntegral = true;
                                    //showAnnotateNoDrums = true;
                                }

                                xls.SetCellFloatValue( row, startCol + 4, itemParam.IntegralsOnStart.F_VALUE*integralCoeff, true);
                                //Отсутствуют интегралы, или не полные
                                if ((itemParam.IntegralsOnStart.F_FLAG & VALUES_FLAG_DB.AllAlarmStatuses) != VALUES_FLAG_DB.None ||
                                    (itemParam.IntegralsOnStart.EventDateTime - _dtStart).TotalMinutes > 1)
                                {
                                    xls.SetCellBkColor( row, startCol + 4, _noDrumsColor);
                                    var c = itemParam.IntegralsOnStart.EventDateTime.ToString("dd.MM.yyyy HH:mm") + " :\n" + itemParam.IntegralsOnStart.F_FLAG.FlagToString("\n");
                                    if (isFormingComment && row < MaxCommentRows)
                                    {
                                        xls.SetComment(row, startCol + 4, c);
                                        xls.SetCommentProperties(row, startCol + 4, commentProps);
                                    }
                                    isPaintNoStartIntegral = true;
                                    //showAnnotateNoDrums = true;
                                }

                                //Выводим разницу показаний
                                xls.SetCellFloatValue( row, startCol + 5, iVal, true);
                            }
                            else
                            {
                                xls.SetCellFormat(row, startCol + 3, row, startCol + 5, xls.IntegralFormat);
                            }

                            if (itemParam.TypeHierarchy == enumTypeHierarchy.Info_TI || itemParam.TypeHierarchy == enumTypeHierarchy.Info_Integral)
                            {
                                xls.SetCellFloatValue( row, startCol + 6, itemParam.CoeffTransformation, true, need0:false);

                                //Выводим разницу показаний домноженную на коэфф.
                                xls.SetCellFloatValue( row, startCol + 7, iVal * itemParam.CoeffTransformation, true);
                            }
                        }

                        var isPaintNoData = false;

                        if (archives != null)
                        {
                            foreach (var aVal in archives) //Перебираем получасовки
                            {
                                var c = startCol + delta + col;
                                if (c>=16385) break; //Limit xlsx

                                if (!itemParam.Coef.HasValue || itemParam.Coef.Value == 0)
                                {
                                    //В балансе не участвует, выводим информативно, в формулах не участвует
                                    if (aVal != null)
                                    {
                                        xls.SetCellFloatValue( row, c, aVal.F_VALUE);
                                    }
                                    col++;
                                    continue;
                                }

                                if (_isFormingFormulasToCell && level >= 0)
                                {
                                    //Добавляем в общую формулу только если это конечный уровень и нет уровней ниже
                                    AddRowRangeToFormulaSum(xls, level.ToString(), new FormulaRowsRange {Row1 = row, Row2 = row, Col = c});
                                }

                                #region Вывод инфрмации об объекте

                                double? value = null;

                                if (aVal != null)
                                {
                                    var val = aVal.F_VALUE * (itemParam.Coef ?? 1);
                                    value = val;

                                    //Вычитаем значения ОВ
                                    if (isIntervalBalance && itemParam.OV_Values_List != null &&
                                        itemParam.OV_Values_List.Count > 0)
                                    {
                                        val -= itemParam.OV_Values_List.Where(ov => ov.Val_List != null)
                                            .Sum(ov => ov.Val_List.Sum(v => v.F_VALUE));
                                    }

                                    //Вычитаем значения акта недоучета
                                    if (isIntervalBalance && itemParam.ReplaceActUndercountList != null &&
                                        itemParam.ReplaceActUndercountList.Count > 0)
                                    {
                                        double actVal = 0;

                                        foreach (var to in itemParam.ReplaceActUndercountList)
                                        {
                                            actVal += to.AddedValue;

                                            if (to.IsCoeffTransformationEnabled.HasValue && to.IsCoeffTransformationEnabled.Value)
                                            {
                                                actVal *= itemParam.CoeffTransformation;
                                            }
                                        }

                                        val -=  actVal * hhCoeff;
                                    }

                                    xls.SetCellFloatValue( row, c, val, need0: false); //Отображаем без ОВ, но только если выбрана общая сумма

                                    if ((aVal.F_FLAG & VALUES_FLAG_DB.AllAlarmStatuses) != VALUES_FLAG_DB.None)
                                    {
                                        isPaintNoData = true;

                                        //_xls.SetCellValue(row, c, val, frm);
                                        xls.SetCellFloatValue( row, c, val,
                                            need0: false); //Отображаем без ОВ, но только если выбрана общая сумма

                                        if (isFormingComment && row < MaxCommentRows)
                                        {
                                            xls.SetCellBkColor( row, c, _noDataColor);
                                            xls.SetComment(row, c, aVal.F_FLAG.FlagToString("\n"));
                                        }
                                    }
                                }
                                else
                                {
                                    xls.SetCellFormat(row, c, leftFormat);
                                }

                                #endregion

                                //row++;

                                if (!value.HasValue)
                                {
                                    col++;
                                    continue;
                                }

                                #region Подсчет результата

                                if (itemParam.IsInput)
                                {
                                    double v;
                                    if (postupilo.TryGetValue(col, out v)) postupilo[col] = v + value.Value;
                                    else postupilo[col] = value.Value;
                                }

                                #endregion

                                //Подсчет по уровням напряжения
                                if (Equals(section.MetaString2, "otpusk_shin+") || string.Equals(section.MetaString2, "otpusk_shin-"))
                                {
                                    var koeff = string.Equals(section.MetaString2, "otpusk_shin+") ? 1.0 : -1.0;
                                    if ((itemParam.IsInput || itemParam.IsOutput) && itemParam.Voltage.HasValue)
                                    {
                                        double voltage;
                                        summVoltageCalc.TryGetValue(itemParam.Voltage.Value, out voltage);

                                        summVoltageCalc[itemParam.Voltage.Value] = voltage + (koeff*value.Value);
                                    }
                                }

                                col++;
                            }
                        }

                        if (isIntervalBalance)
                        {
                            #region Формируем комментарий

                            var savedRow = row;
                            var comment = new StringBuilder();

                            #region Участвует или нет в балансе

                            if (!itemParam.Coef.HasValue || itemParam.Coef.Value == 0)
                            {
                                comment.Append("В балансе не участвует\n");
                            }

                            #endregion

                            #region  Информация по смене периодов константы

                            if (itemParam.TypeHierarchy == enumTypeHierarchy.FormulaConstant && itemParam.FormulaConstantArchives != null)
                            {
                                if (itemParam.FormulaConstantArchives.Count > 0)
                                {
                                    foreach (var formulaConstantArchive in itemParam.FormulaConstantArchives)
                                    {
                                        comment.AppendFormat("Значение: {0:### ### ### ##0.###}\n"
                                                             + "c {1:dd.MM.yyyy HH:mm}\n"
                                                             + (formulaConstantArchive.FinishDateTime.HasValue ? "по {2:dd.MM.yyyy HH:mm}" : string.Empty) + "\n",
                                            formulaConstantArchive.Value, formulaConstantArchive.StartDateTime, formulaConstantArchive.FinishDateTime);
                                    }
                                }
                                else if (itemParam.DefaultFormulaConstantValue.HasValue)
                                {
                                    comment.AppendFormat("Значение по умолчанию: {0:### ### ### ##0.###}\n",
                                        itemParam.DefaultFormulaConstantValue.Value);
                                }
                            }

                            #endregion

                            #region Смена ПУ

                            var isMeterChanged = false;

                            if (itemParam.IntegralsOnStart != null && itemParam.IntegralsOnStart != null && itemParam.MetersTO_Information != null && itemParam.MetersTO_Information.Count > 0)
                            {
                                isMeterChanged = true;
                                row--;
                                foreach (TMetersTO_Information to in itemParam.MetersTO_Information)
                                {
                                    row++;

                                    if (string.IsNullOrEmpty(to.MeterSerialNumberBefo))
                                    {
                                        //Это начало действия нового стчетчика
                                        comment.AppendFormat("Установка ПУ №{0}\n"
                                                             + "дата установки {1:dd.MM.yyyy HH:mm}\n"
                                                             + "первое значение после установки {2:0.#######}\n",
                                            to.MeterSerialNumberAfter, to.ExchangeDateTime, to.DataAfter * integralCoeff);

                                    }
                                    else
                                    {
                                        comment.AppendFormat("Замена ПУ №{0} на №{1}\n"
                                                             + "дата замены {2:dd.MM.yyyy HH:mm}\n"
                                                             + "последнее значение перед заменой {3:0.#######}\n"
                                                             + "первое значение после замены {4:0.#######}\n",
                                            to.MeterSerialNumberBefo, to.MeterSerialNumberAfter, to.ExchangeDateTime, to.DataBefo / 1000,
                                            to.DataAfter * integralCoeff);

                                        //Информация до смены
                                        xls.SetCellValue(row, startCol + 1, to.MeterSerialNumberBefo + " ", centerFormat); //старый номер ПУ
                                        xls.SetCellFloatValue( row, startCol + 3, to.DataBefo * integralCoeff, true);
                                        if (isPaintNoFinishIntegral)
                                        {
                                            xls.SetCellBkColor( row, startCol + 3, _noDrumsColor);
                                        }

                                        // показания старого ПУ на начало периода
                                        if (itemParam.IntegralsOnStart != null)
                                        {
                                            xls.SetCellFloatValue( row, startCol + 4, itemParam.IntegralsOnStart.F_VALUE * integralCoeff, true);
                                            if (isPaintNoStartIntegral)
                                            {
                                                xls.SetCellBkColor( row, startCol + 4, _noDrumsColor);
                                            }
                                        }
                                        // показания старого ПУ на начало периода

                                        xls.SetCellFloatValue( row, startCol + 5, to.IntDataBefor * integralCoeff, true);
                                        //расход по показаниям до смены счетчика

                                        xls.SetCellFloatValue( row, startCol + 6, to.CoeffTransformationBefo, true, need0: false);// Коэффициент ПУ до смены

                                        xls.SetCellValue(row, startCol + 7, to.HhDataBefor * integralCoeff, !isPaintNoData ? xls.ProfileFormat : rightDoubleFormatNoData);
                                        xls.SetCellValue(row, startCol + 8, to.HhDataBefor * hhCoeff, !isPaintNoData ? xls.ProfileFormat : rightDoubleFormatNoData);
                                        // Получасовки до смены
                                        if (_isFormingFormulasToCell && level >= 0)
                                        {
                                            //Добавляем в общую формулу только если это конечный уровень и нет уровней ниже
                                            AddRowRangeToFormulaSum(xls, level.ToString(), new FormulaRowsRange { Row1 = row, Row2 = row, Col = startCol + 8 });
                                        }

                                        row++;

                                        if (itemParam.IntegralsOnEnd != null)
                                        {
                                            xls.SetCellFloatValue( row, startCol + 3, itemParam.IntegralsOnEnd.F_VALUE * integralCoeff, true);
                                            if (isPaintNoFinishIntegral)
                                            {
                                                xls.SetCellBkColor( row, startCol + 3, _noDrumsColor);
                                            }
                                        }

                                        xls.SetCellValue(row, startCol, pp - 1, leftFormat);
                                        xls.SetCellValue(row, startCol + 1, to.MeterSerialNumberAfter + " ", centerFormat); //старый номер ПУ
                                        xls.SetCellValue(row, startCol + 2, itemParam.Name, leftFormat);
                                        xls.SetCellFloatValue(row, startCol + 4, to.DataAfter * integralCoeff, true); // показания нового ПУ на конец периода
                                        xls.SetCellFloatValue(row, startCol + 5, to.IntDataAfter * integralCoeff, true); //расход по показаниям после смены
                                        xls.SetCellFloatValue( row, startCol + 6, to.CoeffTransformationAfter, true, need0: false);// Коэффициент ПУ после смены

                                        xls.SetCellValue(row, startCol + 7, to.HhDataAfter * integralCoeff, !isPaintNoData ? xls.ProfileFormat : rightDoubleFormatNoData); // Получасовки после смены
                                        xls.SetCellValue(row, startCol + 8, to.HhDataAfter * hhCoeff, !isPaintNoData ? xls.ProfileFormat : rightDoubleFormatNoData); // Получасовки после смены
                                    }

                                    if (_isFormingFormulasToCell && level >= 0)
                                    {
                                        //Добавляем в общую формулу только если это конечный уровень и нет уровней ниже
                                        AddRowRangeToFormulaSum(xls, level.ToString(), new FormulaRowsRange { Row1 = row, Row2 = row, Col = startCol + 8 });
                                    }
                                }
                            }

                            #endregion

                            #region Смена трансформатора

                            if (itemParam.Transformatos_Information != null && itemParam.Transformatos_Information.Count > 0)
                            {
                                if (row == savedRow) row--;
                                var isFirstTransformatorInfo = true;
                                foreach (var to in itemParam.Transformatos_Information)
                                {
                                    row++;
                                    var strK = (to.TransformatorsChangedType & enumTransformatorsChangedType.ChangedCoefI) ==
                                               enumTransformatorsChangedType.ChangedCoefI
                                        ? "Кт"
                                        : "Кн";
                                    comment.AppendFormat("Замена {0}\n"
                                                         + "{1}\n"
                                                         + "дата замены {2:dd.MM.yyyy HH:mm}\n"
                                                         + "значение {5} перед заменой {3:##}\n"
                                                         + "значение {5} после замены {4:##}\n",
                                        to.TransformatorsChangedString, "трансформатор",
                                        to.ExchangeDateTime, to.CoeffTransformationBefo, to.CoeffTransformationAfter, strK);

                                    //Внимание возможно наткнулись на пересекающийся диапазон!!! 
                                    //Пишем это в комментарий, т.к. это косяк
                                    if (to.ExchangeDateTime < _dtStart || to.ExchangeDateTime > _dtEnd)
                                    {
                                        comment.Append("(Возможно произошло пересечение диапазонов \n времени действия трансформатора!\n)");
                                    }

                                    if (!isMeterChanged)
                                    {
                                        if (isFirstTransformatorInfo)
                                        {
                                            //Информация до смены
                                            xls.SetCellValue(row, startCol + 3, " ", leftFormat);
                                            xls.SetCellValue(row, startCol + 5, " ", leftFormat);

                                            xls.SetCellFloatValue( row, startCol + 6, to.CoeffTransformationBefo, true, need0: false); // Коэффициент ПУ до смены

                                            xls.SetCellValue(row, startCol + 7, to.HhDataBefor / integralCoeff, !isPaintNoData ? xls.ProfileFormat : rightDoubleFormatNoData); // Получасовки до смены
                                            xls.SetCellValue(row, startCol + 8, to.HhDataBefor / hhCoeff, !isPaintNoData ? xls.ProfileFormat : rightDoubleFormatNoData); // Получасовки до смены
                                            if (isPaintNoData)
                                            {
                                                var c = xls.GetComment(row, startCol + 8);
                                                c += "\nЗначение дорассчитано";
                                                xls.SetComment(row, startCol + 8, c);
                                            }

                                            if (_isFormingFormulasToCell && level >= 0)
                                            {
                                                //Добавляем в общую формулу только если это конечный уровень и нет уровней ниже
                                                AddRowRangeToFormulaSum(xls, level.ToString(), new FormulaRowsRange { Row1 = row, Row2 = row, Col = startCol + 8 });
                                            }
                                            row++;
                                            isFirstTransformatorInfo = false;
                                        }
                                        xls.SetCellValue(row, startCol, pp - 1, leftFormat);
                                        xls.SetCellValue(row, startCol + 1, itemParam.MeterSerialNumber + " ", centerFormat); //старый номер ПУ
                                        xls.SetCellValue(row, startCol + 2, itemParam.Name, leftFormat);
                                        if (itemParam.IntegralsOnEnd != null)
                                        {
                                            xls.SetCellFloatValue( row, startCol + 3, itemParam.IntegralsOnEnd.F_VALUE * integralCoeff, true);
                                        }
                                        xls.SetCellFormat(row, startCol + 4, row, startCol + 5, leftFormat);
                                        xls.SetCellFloatValue( row, startCol + 6, to.CoeffTransformationAfter, true, need0: false);// Коэффициент ПУ после смены

                                        xls.SetCellValue(row, startCol + 7, to.HhDataAfter / integralCoeff, !isPaintNoData ? xls.ProfileFormat : rightDoubleFormatNoData); // Получасовки после смены
                                        xls.SetCellValue(row, startCol + 8, to.HhDataAfter / hhCoeff, !isPaintNoData ? xls.ProfileFormat : rightDoubleFormatNoData); // Получасовки после смены
                                        if (isPaintNoData)
                                        {
                                            var c = xls.GetComment(row, startCol + 8);
                                            c += "\nЗначение дорассчитано";
                                            xls.SetComment(row, startCol + 8, c);
                                        }
                                        if (_isFormingFormulasToCell && level >= 0)
                                        {
                                            //Добавляем в общую формулу только если это конечный уровень и нет уровней ниже
                                            AddRowRangeToFormulaSum(xls, level.ToString(), new FormulaRowsRange { Row1 = row, Row2 = row, Col = startCol + 8 });
                                        }
                                    }
                                }
                            }

                            #endregion

                            
                            if (comment.Length > 0)
                            {
                                xls.SetCellValue(savedRow, startCol + 9, comment.ToString(), centerFormat);
                                xls.MergeCells(savedRow, startCol + 9, row, startCol + 9);

                                xls.AutofitRow(savedRow, row, true, true, 1); //SetRowHeight(r, drh + 200);
                            }
                            //else
                            {
                                xls.SetCellFormat(savedRow, startCol + 9, row, startCol + 9, centerFormat);
                            }

                            #region Информация по Акту недоучета


                            if (itemParam.ReplaceActUndercountList != null && itemParam.ReplaceActUndercountList.Count > 0)
                            {
                                var actSumm = 0.0;
                                foreach (var to in itemParam.ReplaceActUndercountList)
                                {
                                    row++;

                                    xls.SetCellValue(row, startCol + 9, string.Format(" с {1:dd.MM.yyyy HH:mm} по {2:dd.MM.yyyy HH:mm}\n" + "{3}",
                                        to.AddedValue / 1000, to.StartDateTime, to.FinishDateTime, to.CommentString), centerFormat);

                                    var actVal = to.AddedValue * hhCoeff;

                                    if (to.IsCoeffTransformationEnabled.HasValue && to.IsCoeffTransformationEnabled.Value)
                                    {
                                        actVal *= itemParam.CoeffTransformation;
                                    }

                                    //TODO умножение на коэфф. потерь
                                    //if (itemParam.IsCoeffLossesEnabled)
                                    //itemParam.CoeffTransformation * itemParam.CoeffLosses;

                                    actSumm += actVal * itemParam.CoeffTransformation;

                                    xls.SetCellValue(row, startCol, pp++, leftFormat);
                                    xls.SetCellFloatValue(row, startCol + 8, actVal);

                                    if (_isFormingFormulasToCell && level >= 0)
                                    {
                                        //Добавляем в общую формулу только если это конечный уровень и нет уровней ниже
                                        AddRowRangeToFormulaSum(xls, level.ToString(), new FormulaRowsRange
                                        {
                                            Row1 = row,
                                            Row2 = row,
                                            Col = startCol + 8
                                        });
                                    }

                                    xls.SetCellValue(row, startCol + 2, "Корректировка по Акту недоучета\n",
                                        leftFormat);

                                    //Примечание
                                    //_xls.SetCellValue(row, startCol + 9, string.Format("Корректировка по Акту недоучета\n"
                                    //                                     + " с {0:dd.MM.yyyy HH:mm} по {1:dd.MM.yyyy HH:mm}\n"
                                    //                                     + "{2}", to.StartDateTime, to.FinishDateTime, to.CommentString), rightFormat);
                                }
                            }

                            #endregion

                            #region ОВ

                            if (itemParam.OV_Values_List != null && itemParam.OV_Values_List.Count > 0)
                            {
                                foreach (var ov in itemParam.OV_Values_List)
                                {
                                    if (ov.Val_List == null) continue;
                                    row++;
                                    xls.SetCellValue(row, startCol, pp++, leftFormat);
                                    xls.SetCellValue(row, startCol + 1, ov.MeterSerialNumber, centerFormat);
                                    xls.SetCellValue(row, startCol + 2, ov.Name + " на присоединении " + itemParam.Name, leftFormat);
                                    //_xls.SetCellFormat(row, startCol + 3, row, startCol + 5, _profileFormat);
                                    xls.SetCellFloatValue( row, startCol + 6, ov.CoeffTransformation, true, need0: false);

                                    var ovSumm = ov.Val_List.Sum(v => v.F_VALUE);
                                    //SetCellFloatValue(row, startCol + 7, ovSumm, true);
                                    xls.SetCellFloatValue( row, startCol + 8, ovSumm);
                                    if (_isFormingFormulasToCell && level >= 0)
                                    {
                                        //Добавляем в общую формулу только если это конечный уровень и нет уровней ниже
                                        AddRowRangeToFormulaSum(xls, level.ToString(), new FormulaRowsRange
                                        {
                                            Row1 = row,
                                            Row2 = row,
                                            Col = startCol + 8
                                        });
                                    }

                                    var ovComment = "Замещал с " + ov.DTServerStart.ToString("dd.MM.yyyy HH:mm") + "\n по "
                                                                         + ov.DTServerEnd.ToString("dd.MM.yyyy HH:mm") + "\n";

                                    if (ov.ActUndercountValues != null && ov.ActUndercountValues.Count > 0)
                                    {
                                        ovComment += "Корректировка по Акту недоучета:";

                                        foreach (var actInfo in ov.ActUndercountValues)
                                        {
                                            if (actInfo.Halfhours == null || actInfo.Halfhours.Count == 0) continue;

                                            var actSum = actInfo.Halfhours.Sum(v => v.Value) / (double)_unitDigit;

                                            //ovSum -= actSum;

                                            ovComment += string.Format("\n" + actInfo.ActMode + ": {0:0.###} " + _unitDigitName + "\n",
                                                actSum);
                                        }
                                    }

                                    //Примечание
                                    xls.SetCellValue(row, startCol + 9, ovComment, centerFormat);

                                    if (ov.Val_ListDrum != null) //Если есть значения барабанов
                                    {
                                        var deltaVal = ov.Val_ListDrum.Diff;
                                        var firstVal = ov.Val_ListDrum.First;
                                        var lastVal = ov.Val_ListDrum.Last;

                                        if (lastVal != null)
                                        {
                                            xls.SetCellFloatValue( row, startCol + 3,
                                                ((lastVal.F_VALUE) / 1000), true,
                                                need0: false); // на конеч

                                            xls.SetCellBkColor( row, startCol + 3, _noDrumsColor);
                                            xls.SetComment(row, startCol + 3,
                                                lastVal.EventDateTime.AddMinutes(30).ToString("dd.MM.yyyy HH:mm") + " :\n" +
                                                lastVal.F_FLAG.FlagToString("\n"));
                                            xls.SetCommentProperties(row, startCol + 3, commentProps);
                                        }

                                        if (firstVal != null)
                                        {
                                            xls.SetCellFloatValue( row, startCol + 4,
                                                ((firstVal.F_VALUE) / 1000), true,
                                                need0:false); // на начало

                                            xls.SetCellBkColor( row, startCol + 4, _noDrumsColor);
                                            xls.SetComment(row, startCol + 4,
                                                firstVal.EventDateTime.ToString("dd.MM.yyyy HH:mm") + " :\n" +
                                                firstVal.F_FLAG.FlagToString("\n"));
                                            xls.SetCommentProperties(row, startCol +4, commentProps);
                                        }

                                        xls.SetCellFloatValue( row, startCol + 5, deltaVal / 1000,
                                            true, need0: false); // разность

                                        xls.SetCellFloatValue( row, startCol + 7, deltaVal / 1000 * ov.CoeffTransformation,
                                            true, need0: false); // разность умноженная на коэфф.
                                    }
                                }
                            }

                            #endregion

                            #endregion

                            //Наполняем колонки с разницей получасовок и интегралов
                            analiser.AddToFormula(string.Empty, row);
                        }
                    }

                    #endregion

                    subsectionUns[0] = itemParam.BalanceFreeHierarchySubsectionUn;
                    subsectionUns[1] = itemParam.BalanceFreeHierarchySubsectionUn2;
                    subsectionUns[2] = itemParam.BalanceFreeHierarchySubsectionUn3;

                    row++;
                }

                for (var i = 0; i < 3; i++)
                {
                    showSectionResult(null, i, null, null, 0, section.Section_UN); //окончательный итог
                }

                {
                    xls.MergeCells(row, startCol, row, startCol + delta - (isIntervalBalance ? 2 : 1));
                    xls.SetCellFormat(row, startCol, row, startCol + delta + 1, leftBoldFormat);
                    xls.SetCellValue(row, startCol, "Всего по разделу");

                    //Формируем формулы
                    var col = 0;
                    foreach (var calculated in balanceCalculatedResult.CalculatedByDiscretePeriods)
                    {
                        if (calculated == null)
                        {
                            col++;
                            continue;
                        } //Ошибка расчетов

                        var c = startCol + delta + col;
                        if (c >= 16385) break; //Limit xlsx

                        //Итог по разделу
                        double sectionSum;
                        calculated.SectionValues.TryGetValue(section.Section_UN, out sectionSum);

                        var halfHourSumm = sectionSum;

                        //if (isIntervalBalance && col == 0)
                        //{
                        //    //Интегралы считаются без учета ОВ, и смены трансформаторов и счетчиков, поэтому пока не выводим
                        //    SetCellFloatValue(row, startCol + 7, sectionSum.IntegralSumm, isBold: true);
                        //}

                        //SetCellFloatValue(row, c, halfHourSumm, isBold: true);
                        WriteFormulaToCell(xls, section.Section_UN, row, c, profileBoldFormat, halfHourSumm);

                        if (Equals(section.MetaString1, "postupilo"))
                        {
                            AddRowRangeToFormulaSum(xls, formulaIdPostupilo_1 + col, new FormulaRowsRange
                            {
                                SheetName = sheetName,
                                Row1 = row,
                                Row2 = row,
                                Col = c,
                            });
                        }
                        else if (Equals(section.MetaString1, "rashod_na_PS"))
                        {
                            double v;
                            if (rashodNaPs.TryGetValue(col, out v)) rashodNaPs[col] = v + halfHourSumm;
                            else rashodNaPs[col] = halfHourSumm;

                            AddRowRangeToFormulaSum(xls, formulaIdRashod_2 + col, new FormulaRowsRange
                            {
                                SheetName = sheetName,
                                Row1 = row,
                                Row2 = row,
                                Col = c,
                            });
                        }
                        else if (Equals(section.MetaString1, "rashod_potreb"))
                        {
                            double v;
                            if (rashodPotreb.TryGetValue(col, out v)) rashodPotreb[col] = v + halfHourSumm;
                            else rashodPotreb[col] = halfHourSumm;

                            AddRowRangeToFormulaSum(xls, formulaIdOtpusk_3 + col, new FormulaRowsRange
                            {
                                SheetName = sheetName,
                                Row1 = row,
                                Row2 = row,
                                Col = c,
                            });

                            //AddRowRangeToFormulaSum(formulaIdOtpuskSeti_6, new FormulaRowsRange
                            //{
                            //    SheetName = sheetName,
                            //    Row1 = row,
                            //    Row2 = row,
                            //    Col = c,
                            //});
                        }

                        if (Equals(section.MetaString2, "otpusk_shin+"))
                        {
                            double v;
                            if (otpuskShin.TryGetValue(col, out v)) otpuskShin[col] = v + halfHourSumm;
                            else otpuskShin[col] = halfHourSumm;

                            AddRowRangeToFormulaSum(xls, formulaIdOtpuskShin_5 + col, new FormulaRowsRange
                            {
                                SheetName = sheetName,
                                Row1 = row,
                                Row2 = row,
                                Col = c,
                            });
                        }
                        else if (Equals(section.MetaString2, "otpusk_shin-"))
                        {
                            double v;
                            if (otpuskShin.TryGetValue(col, out v)) otpuskShin[col] = v - halfHourSumm;
                            else otpuskShin[col] = -halfHourSumm;

                            AddRowRangeToFormulaSum(xls, formulaIdOtpuskShin_5 + col, new FormulaRowsRange
                            {
                                SheetName = sheetName,
                                Row1 = row,
                                Row2 = row,
                                Col = c,
                                Before = "-"
                            });
                        }

                        col ++;
                    }
                }

                row++;

            }

            #endregion

            #region Итоги

            var dopustNebalns = sections.FirstOrDefault(s => string.Equals(s.MetaString1, "dopust_nebalns"));
            if (dopustNebalns != null)
            {
                if (isIntervalBalance)
                {
                    if (_isFormingSeparateList)
                    {
                        row = 1;
                        xls.AddSheet();
                        xls.ActiveSheet = sheetNumber++;
                        xls.SheetName = dopustNebalns.SectionName;
                        xls.PrintToFit = true;
                        xls.PrintLandscape = _printLandscape;
                        xls.SheetZoom = 100;
                        xls.DefaultColWidth = 3964;
                        xls.SetColWidth(3, 4864);
                        xls.SetColWidth(4, 3964);
                        xls.SetColWidth(5, 3964);
                        xls.SetColWidth(7, 3964);
                        xls.SetColWidth(8, 4364);
                        xls.SetColWidth(9, 4364);
                    }
                    showSectionTitle(dopustNebalns.SectionName);

                    f.Format = "### ### ### ### ##0.0" + new string('0', 5);
                    var pf = xls.AddFormat(f);

                    var nebalansAction = (Action<string, List<BalanceFreeHierarchyItemParams>>) ((title, list) =>
                    {
                        xls.SetCellValue(row, startCol, title + " (Э*=прием/∑прием)", centerBoldFormat);
                        xls.MergeCells(row, startCol, row, startCol + 9);
                        row++;
                        xls.SetCellFormat(row, startCol, row, startCol + 9, leftFormat);
                        xls.SetCellValue(row, startCol + 1, "Номер\nсчетчика");
                        xls.SetCellValue(row, startCol + 2, "Наименование объекта учета");
                        xls.SetCellValue(row, startCol + 3, "Доля э-э");
                        xls.SetCellValue(row, startCol + 4, "Погр. изм. компл.");
                        xls.SetCellValue(row, startCol + 5, "Небаланс");
                        row++;

                        var pp = 1;
                        foreach (var itemParam in list)
                        {
                            xls.SetCellFormat(row, startCol, rightFormat);
                            if (itemParam.TypeHierarchy == enumTypeHierarchy.FormulaConstant)
                            {
                                xls.SetCellValue(row, startCol + 1, "Константа", leftFormat);
                            }
                            else if (itemParam.TypeHierarchy == enumTypeHierarchy.Formula)
                            {
                                xls.SetCellValue(row, startCol + 1, "Формула", leftFormat);
                            }
                            else
                            {
                                xls.SetCellValue(row, startCol + 1, itemParam.MeterSerialNumber, centerFormat);
                            }

                            xls.SetCellValue(row, startCol + 2, itemParam.Name, leftFormat);

                            xls.SetCellValue(row, startCol + 3, itemParam.PartPercent, pf);
                            xls.SetCellValue(row, startCol + 4, itemParam.MeasuringComplexError, pf);
                            xls.SetCellValue( row, startCol + 5, itemParam.UnBalanceTi, pf);

                            xls.SetCellFormat(row, startCol + 6, row, startCol + 9, rightFormat);
                            row++;
                        }
                    });

                    nebalansAction("Поступило ", inputs);
                    nebalansAction("Отпущено ", outputs);
                }
                else
                {
                    row++;
                }

                if (isIntervalBalance) delta = 3;

                xls.MergeCells(row, startCol, row, startCol + delta - 1);
                xls.SetCellFormat(row, startCol, row, startCol + delta + 1, leftBoldFormat);
                xls.SetCellValue(row, startCol, "Допустимый небаланс,%");

                if (isIntervalBalance && balanceCalculatedResult.Result.HasValue)
                {
                    xls.SetCellValue(row, startCol + delta + 2, balanceCalculatedResult.Result.Value, rightDoubleBoldFormatDefault);
                }
                else
                {
                    var col = 0;
                    foreach (var calculated in balanceCalculatedResult.CalculatedByDiscretePeriods)
                    {
                        var c = startCol + delta + col;
                        if (c >= 16385) break; //Limit xlsx

                        xls.SetCellValue(row, c, calculated.ResolvedUnBalanceValues, rightDoubleBoldFormatDefault);
                        col++;
                    }
                }

                row++;
                row++;
            }

            var itog = sections.FirstOrDefault(s => string.Equals(s.MetaString1, "itog"));
            if (itog != null)
            {
                if (_isFormingSeparateList)
                {
                    row = 1;
                    xls.AddSheet();
                    xls.ActiveSheet = sheetNumber++;
                    xls.SheetName = itog.SectionName;
                    xls.PrintToFit = true;
                    xls.PrintLandscape = _printLandscape;
                    xls.SheetZoom = 100;
                    xls.DefaultColWidth = 500;
                    xls.SetColWidth(3, 0);
                    xls.SetColWidth(2, 4864);
                    xls.SetColWidth(3, 4864);
                    xls.SetColWidth(4, 3964);
                    xls.SetColWidth(5, 3964);
                    xls.SetColWidth(6, 5264);
                }

                //var dopNeb = _sections.FirstOrDefault(s => string.Equals(s.MetaString1, "dopust_nebalns"));
                //if (dopNeb != null)
                //{
                //    showSectionTitle(dopNeb.BalanceFreeHierarchySectionName + ",%");
                //}

                //_xls.SetCellValue(row, startCol + 1, "Допустимый небаланс, %", leftFormat);
                //_xls.MergeCells(row, startCol + 1, row, startCol + 4);
                //_xls.SetCellValue(row, startCol + 5, caculated.ResolvedUnBalanceValues, rightDoubleFormat);
                //row++; row++;

                showSectionTitle(itog.SectionName);

                var number = 1;
                var sf = new StringBuilder("");
                string str;
                if (balanceCalculatedResult.DocumentType == EnumBalanceFreeHierarchyType.БалансЭлектростанции)
                {
                    str = number.ToString();
                    number++;
                    sf.Append(str).Append("+");
                }
                str = number.ToString();
                number++;
                sf.Append(str);

                var ppNum = 1;
                var inStr = sf.ToString();

                xls.SetCellValue(row, startCol, ppNum + ". Поступило на шины (" + sf + ")", leftFormat);
                xls.MergeCells(row, startCol, row, startCol + 2);

                foreach (var v in postupilo)
                {
                    WriteFormulaToCell(xls, formulaIdPostupilo_1 + v.Key, row, startCol + 3 + v.Key, xls.ProfileFormat, v.Value);
                }

                row++;
                ppNum++;

                str = "(" + number++ + "+" + number + "+" + (number + 1) + ")";
                sf.Append("-").Append(str);
                xls.SetCellValue(row, startCol, ppNum + ". Расход электроэнергии на станции " + str, leftFormat);
                xls.MergeCells(row, startCol, row, startCol + 2);

                foreach (var v in rashodNaPs)
                {
                    WriteFormulaToCell(xls, formulaIdRashod_2 + v.Key, row, startCol + 3 + v.Key, xls.ProfileFormat, v.Value);
                }

                row++;
                ppNum++;

                if (balanceCalculatedResult.DocumentType == EnumBalanceFreeHierarchyType.БалансЭлектростанции)
                {
                    str = "(" + (number + 2) + "+" + (number + 3) + ")";
                }
                else
                {
                    str = "(" + (number + 2) + ")";
                }

                sf.Append("-").Append(str).Append("-").Append(number + 4);
                xls.SetCellValue(row, startCol, ppNum + ". Отпуск электроэнергии " + str, leftFormat);
                xls.MergeCells(row, startCol, row, startCol + 2);

                foreach (var v in rashodPotreb)
                {
                    WriteFormulaToCell(xls, formulaIdOtpusk_3 + v.Key, row, startCol + 3 + v.Key, xls.ProfileFormat, v.Value);
                }

                row++;
                ppNum++;

                if (isIntervalBalance)
                {

                    xls.SetCellValue(row, startCol, ppNum + ". Фактический небаланс (" + sf + ")/(" + inStr + ")*100, %", leftFormat);
                }
                else
                {
                    if (_isFormingSeparateList)
                    {
                        headerAction(false);
                    }

                    xls.SetCellValue(row, startCol, " Фактический небаланс, %", leftFormat);
                }


                xls.MergeCells(row, startCol, row, startCol + 2);
                var col = 0;
                foreach (var calculated in balanceCalculatedResult.CalculatedByDiscretePeriods)
                {
                    var c = startCol + delta + col;
                    if (c >= 16385) break; //Limit xlsx

                    xls.SetCellFloatValue( row, c, calculated.FactUnbalancePercent);
                    col++;
                }

                row++;
                ppNum++;

                if (isIntervalBalance)
                {
                    xls.SetCellValue(row, startCol, "     фактический небаланс (" + sf + "), " + _unitDigitName, leftFormat);
                }
                else
                {
                    xls.SetCellValue(row, startCol, "     фактический небаланс, " + _unitDigitName, leftFormat);
                }
                xls.MergeCells(row, startCol, row, startCol + 2);
                col = 0;
                foreach (var calculated in balanceCalculatedResult.CalculatedByDiscretePeriods)
                {
                    var c = startCol + delta + col;
                    if (c >= 16385) break; //Limit xlsx

                    xls.SetCellFloatValue( row, c, calculated.FactUnbalanceValue);
                    col++;
                }

                row++;
                ppNum++;

                if (balanceCalculatedResult.DocumentType == EnumBalanceFreeHierarchyType.БалансЭлектростанции)
                {
                    xls.SetCellValue(row, startCol, ppNum + ". Отпуск электроэнергии с шин электростанции, всего (1-3)", leftFormat);
                    xls.MergeCells(row, startCol, row, startCol + 2);
                    foreach (var v in otpuskShin)
                    {
                        WriteFormulaToCell(xls, formulaIdOtpuskShin_5 + v.Key, row, startCol + 3 + v.Key, xls.ProfileFormat, v.Value);
                    }

                    row++;
                    ppNum++;
                }

                //_xls.SetCellValue(row, startCol + 1, ppNum + ". Отпуск электроэнергии в сети, всего", leftFormat);
                //_xls.MergeCells(row, startCol + 1, row, startCol + 4);
                ////_xls.SetCellValue(row, startCol + 5, otpuskShin, rightDoubleFormat);
                //WriteFormulaToCell(formulaIdOtpuskSeti_6, row, startCol + 5, rightDoubleFormat, otpuskSeti);
                row++;
            }

            row++;
            xls.SetCellValue(row, startCol, "В том числе по классам напряжения:", leftFormat);
            xls.MergeCells(row, startCol, row, startCol + 2);
            row++;

            foreach (var summVoltagePair in summVoltageCalc)
            {
                xls.SetCellFormat(row, startCol, row, startCol + 2, leftBoldFormat);
                xls.SetCellValue(row, startCol + 2, summVoltagePair.Key + " кВ");
                xls.SetCellFloatValue( row, startCol + 3, summVoltagePair.Value);
                row++;
            }

            #endregion

            #endregion

            if (analiser != null)
            {
                analiser.Dispose();
            }

            #region Подписанты

            startRow = row;

            row++;
            row++;

            if (signatures != null)
            {
                foreach (var signature in signatures.GroupBy(s => s.Группа ?? s.Группа))
                {
                    xls.SetCellValue(row, 1, signature.Key + ":", leftBoldFormat);
                    xls.SetCellFormat(row, 1, row, 9, leftBoldFormat);
                    xls.MergeCells(row, 1, row, 2);

                    foreach (var s in signature)
                    {
                        row++;
                        xls.SetCellValue(row, 3, s.Должность, leftFormat);
                        xls.SetCellValue(row, 4, "_______________________", leftFormat);
                        xls.MergeCells(row, 4, row, 5);
                        xls.SetCellValue(row, 6, s.ФИО, leftFormat);
                        xls.SetCellFormat(row, 1, row, 9, leftFormat);
                        row++;
                        xls.SetCellFormat(row, 1, row, 9, leftFormat);
                    }

                    row++;
                }
            }

            else
            {
                row = row + 9;
                xls.SetCellFormat(startRow, 1, row, 9, leftFormat);
            }

            //_xls.SetCellFormat(startRow, 1, row, 9, leftFormat);

            #endregion

            return Export(xls);
        }
    }
}
