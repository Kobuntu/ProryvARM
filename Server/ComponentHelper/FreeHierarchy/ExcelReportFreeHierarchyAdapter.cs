using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using FlexCel.Core;
using FlexCel.XlsAdapter;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using System;
using System.Collections;
using System.Linq;
using FlexCel.Render;
using Proryv.AskueARM2.Server.DBAccess.Internal.Balanses;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances.Data;
using Proryv.AskueARM2.Server.VisualCompHelpers.Data;
using BalanceFreeHierarchyCalculatedResult = Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances.Data.BalanceFreeHierarchyCalculatedResult;
using Dict_Balance_FreeHierarchy_Section = Proryv.AskueARM2.Server.DBAccess.Internal.DB.Dict_Balance_FreeHierarchy_Section;
using TExportExcelAdapterType = Proryv.AskueARM2.Server.DBAccess.Internal.TExportExcelAdapterType;
using Proryv.AskueARM2.Server.DBAccess.Internal.DB;
using Proryv.AskueARM2.Server.DBAccess.Public.Common;
using Proryv.AskueARM2.Server.DBAccess.Public.Utils;
using Proryv.AskueARM2.Server.WCF;
using Proryv.Servers.Calculation.Parser.Internal;
using Proryv.Servers.Calculation.DBAccess.Interface.Documents;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances;
using System.Collections.Concurrent;
using Proryv.AskueARM2.Server.VisualCompHelpers.FreeHierarchy;
using Proryv.Servers.Calculation.DBAccess.Common.Ext;

namespace Proryv.AskueARM2.Server.VisualCompHelpers
{
    public partial class ExcelReportFreeHierarchyAdapter : IDisposable, ISpreadsheetProperties
    {
        private StringBuilder _errors;
        public readonly TExportExcelAdapterType AdapterType;
        /// <summary>
        /// Филиал
        /// </summary>
        private readonly string _branchName;
        private readonly DateTime _dtStart;
        private readonly DateTime _dtEnd;
        private readonly List<DateTime> _dts;
        private readonly string _unitDigitName;
        private readonly string _unitDigitIntegralName;
        private readonly string _unitDigitHeatName;
        
        private bool _isFormingFormulasToCell;
        private readonly enumTimeDiscreteType _discreteType;
        private readonly List<int> _intervalTimeList;

        private readonly Color _htmlDocBkColor = Color.FromArgb(255, 236, 242, 244); // цвет фона HTML
        private readonly Color _borderColor = Color.Gray; // цвет рамок
        private const int DblFontZoom = 20;
        private readonly bool _isFormingSeparateList;
        private readonly bool _printLandscape;
        private readonly EnumUnitDigit _unitDigitIntegrals;
        private readonly EnumUnitDigit _unitDigit;

        private readonly byte _doublePrecisionProfile;
        private readonly byte _doublePrecisionIntegral;

        private readonly Color _noDrumsColor = Color.DarkOrange;
        private readonly Color _noDataColor = Color.LightPink;
        private readonly Color _offsetFromMoscowEnbledForDrumsColor = Color.Yellow;

        //Сравнительный анализ энергии учтенной ПУ и суммы получасовок
        private bool _setPercisionAsDisplayed;

        /// <summary>
        /// Дополнять дробную часть нулями до указанного количества знаков
        /// </summary>
        private readonly bool _need0;

        private BalanceFreeHierarchyResults _balanceCalculated;

        private Dictionary<string, List<IFreeHierarchyBalanceSignature>> _signaturesByBalance;
        private ConcurrentDictionary<EnumBalanceFreeHierarchyType, List<Dict_Balance_FreeHierarchy_Section>> _sectionsByType;

        public ExcelReportFreeHierarchyAdapter(BalanceFreeHierarchyResults balanceCalculated,
            Dictionary<string, List<IFreeHierarchyBalanceSignature>> signaturesByBalance,
            TExportExcelAdapterType adapterType, StringBuilder errors, 
            bool isFormingSeparateList, 
            bool isUseThousandKVt, bool printLandscape, string branchName, byte doublePrecisionProfile, 
            byte doublePrecisionIntegral, bool need0, bool isAnalisIntegral, string timeZoneId, bool setPercisionAsDisplayed)
        {
            _branchName = branchName;
            _printLandscape = printLandscape;
            _balanceCalculated = balanceCalculated;
            _signaturesByBalance = signaturesByBalance;
            AdapterType = adapterType;
            _discreteType = balanceCalculated.DiscreteType;
            _errors = errors;
            _dtStart = balanceCalculated.DTStart;
            _dtEnd = balanceCalculated.DTEnd;
            _dts = MyListConverters.GetDateTimeListForPeriod(balanceCalculated.DTStart, balanceCalculated.DTEnd, balanceCalculated.DiscreteType, balanceCalculated.TimeZoneId.GeTimeZoneInfoById());
            _isFormingSeparateList = isFormingSeparateList && (AdapterType == TExportExcelAdapterType.toXLS || AdapterType == TExportExcelAdapterType.toXLSx);
            _unitDigitIntegrals = balanceCalculated.UnitDigitIntegrals;
            _unitDigit = balanceCalculated.UnitDigit;
            _doublePrecisionIntegral = doublePrecisionIntegral;
            _doublePrecisionProfile = doublePrecisionProfile;
            _need0 = need0;
            IsAnalisIntegral = isAnalisIntegral;
            _setPercisionAsDisplayed = setPercisionAsDisplayed;

            _intervalTimeList = MyListConverters.GetIntervalTimeList(_dtStart, _dtEnd, _discreteType, timeZoneId);

            switch (_unitDigit)
            {
                case EnumUnitDigit.Null:
                case EnumUnitDigit.None:
                    _unitDigitName = "";
                    break;
                case EnumUnitDigit.Kilo:
                    _unitDigitName = "к";
                    break;
                case EnumUnitDigit.Mega:
                    _unitDigitName = isUseThousandKVt ? "тыс.к" : "М";
                    break;
            }

            switch (_unitDigitIntegrals)
            {
                case EnumUnitDigit.Null:
                case EnumUnitDigit.None:
                    _unitDigitIntegralName = "";
                    break;
                case EnumUnitDigit.Kilo:
                    _unitDigitIntegralName = "к";
                    break;
                case EnumUnitDigit.Mega:
                    _unitDigitIntegralName = isUseThousandKVt ? "тыс.к" : "М";
                    break;
            }

            _unitDigitHeatName = "Гкал"; //решили что отпуск тепла будет хранится/отображаться как есть (например в ЛЭРС) в Гкал
            _unitDigitName += "Вт*ч";
            _unitDigitIntegralName += "Вт*ч";
        }

        private XlsFileExBase InitBalanceFreeHierarchy(string balanceUn, out bool isHeaderFormed)
        {
            isHeaderFormed = false;
            if (_balanceCalculated == null) return null;

            BalanceFreeHierarchyCalculatedResult balanceCalculatedResult;
            if (!_balanceCalculated.CalculatedValues.TryGetValue(balanceUn, out balanceCalculatedResult)) return null;

            var header = FreeHierarchyFactory.BL_GetBalanceHeader(balanceUn);
            var ms = header == null ? null : header.Item2;

            XlsFileExBase xls;
            var decompressed = CompressUtility.DecompressGZip(ms);
            if (decompressed != null)
            {
                isHeaderFormed = true;
                try
                {
                    xls = new XlsFileExBase(decompressed, true);
                }
                catch (Exception ex)
                {
                    _errors.Append(ex.Message);
                    xls = new XlsFileExBase(1, TExcelFileFormat.v2010, true);
                    xls.OptionsPrecisionAsDisplayed = _setPercisionAsDisplayed;
                }

            }
            else
            {
                xls = new XlsFileExBase(1, TExcelFileFormat.v2013, true);
                xls.NewFile(1, TExcelFileFormat.v2013);
            }

            
            Init(xls);

            if (balanceCalculatedResult != null)
            {
                if (string.IsNullOrEmpty(balanceCalculatedResult.DocumentName))
                {
                    xls.SheetName = "Документ от " + DateTime.Today.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                }
                else
                {
                    xls.SheetName = balanceCalculatedResult.DocumentName;
                }
            }

            xls.OptionsPrecisionAsDisplayed = _setPercisionAsDisplayed;

            return xls;
        }
        
        #region Private

        private void Init(XlsFileExBase xls)
        {
            if (xls.FormulasSum != null) return;

            _isFormingFormulasToCell = AdapterType == TExportExcelAdapterType.toXLS || AdapterType == TExportExcelAdapterType.toXLSx;
            xls.FormulasSum = new Dictionary<string, List<FormulaRowsRange>>();

            xls.SheetOptions = TSheetOptions.ShowGridLines | TSheetOptions.ShowRowAndColumnHeaders | TSheetOptions.ZeroValues | TSheetOptions.AutomaticGridLineColors | TSheetOptions.OutlineSymbols |
                                TSheetOptions.PageBreakView;
            xls.PrintOptions &= ~(TPrintOptions.Orientation | TPrintOptions.NoPls);
            xls.PrintScale = 100;
            xls.PrintFirstPageNumber = 1;
            //_xls.PrintXResolution = 300;
            //_xls.PrintYResolution = 300;
            xls.PrintPaperSize = TPaperSize.A4;
            xls.PrintNumberOfHorizontalPages = 1;
            xls.PrintNumberOfVerticalPages = 20;
            xls.SetPrintMargins(new TXlsMargins(0.1, 0.1, 0.1, 0.1, 0.1, 0.1));
            xls.SheetZoom = 100;
            xls.PrintToFit = true;
            xls.PrintLandscape = _printLandscape;
            xls.PrintXResolution = 300;
            xls.PrintYResolution = 300;

            xls.Need0 = _need0;
            xls.SetPercisionAsDisplayed = _setPercisionAsDisplayed;
            xls.DoublePrecisionIntegral = _doublePrecisionIntegral;
            xls.DoublePrecisionProfile = _doublePrecisionProfile;

            var defaultFormat = xls.GetDefaultFormat;
            var defaultFormatId = xls.DefaultFormatId;

            defaultFormat.VAlignment = TVFlxAlignment.center;
            defaultFormat.Font.Scheme = TFontScheme.None;
            defaultFormat.Font.Name = "Segoe UI";
            defaultFormat.Font.Size20 = DblFontZoom * 9;
            defaultFormat.Font.Family = 0;
            defaultFormat.Font.CharSet = 204;
            defaultFormat.Font.Scheme = TFontScheme.None;
            xls.SetFormat(defaultFormatId, defaultFormat);

            var headersAndFooters = new THeaderAndFooter
            {
                AlignMargins = false,
                ScaleWithDoc = true,
                DiffFirstPage = false,
                DiffEvenPages = false,
                FirstHeader = "",
                FirstFooter = "",
                EvenHeader = "",
                EvenFooter = ""
            };
            xls.SetPageHeaderAndFooter(headersAndFooters);

            
        }

        public MemoryStream BuildBalanceFreeHier(BalanceFreeHierarchyCalculatedResult balanceCalculatedResult)
        {
            if (balanceCalculatedResult == null) return null;

            bool isHeaderFormed;
            var xls = InitBalanceFreeHierarchy(balanceCalculatedResult.BalanceFreeHierarchyUn, out isHeaderFormed);

            //Формируем балансы в зависимости от выбранного типа
            switch (balanceCalculatedResult.DocumentType)
            {
                case EnumBalanceFreeHierarchyType.БалансНаПодстанции:
                case EnumBalanceFreeHierarchyType.БалансЭлектростанции:
                    return ФормируемПодстанцийЭлектростанций(xls, balanceCalculatedResult, isHeaderFormed);
                    break;
                case EnumBalanceFreeHierarchyType.АктУчетаЭэ:
                    return ФормируемАктУчетаЭэ(xls, balanceCalculatedResult, isHeaderFormed);
                    break;
                case EnumBalanceFreeHierarchyType.Приложение51:
                    return ФормируемПриложение51(xls, balanceCalculatedResult, isHeaderFormed);
                    break;
                case EnumBalanceFreeHierarchyType.СводныйИнтегральныйАкт:
                    return ФормируемСводныйИнтегральныйАкт(balanceCalculatedResult);
                    break;
                default:
                    lock (_errors)
                    {
                        _errors.Append(balanceCalculatedResult.DocumentName + " - документ не создан");
                    }

                    break;
            }

            return null;
        }

        internal void AddRowRangeToFormulaSum(XlsFileExBase xls, string id, FormulaRowsRange addedRange)
        {
            if (!_isFormingFormulasToCell) return;

            List<FormulaRowsRange> existsRanges;
            if (!xls.FormulasSum.TryGetValue(id, out existsRanges))
            {
                existsRanges = new List<FormulaRowsRange>();
                xls.FormulasSum.Add(id, existsRanges);
            }

            existsRanges.Add(addedRange);
        }

        /// <summary>
        /// Пишем формулу в excel
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <param name="row">Ячейка</param>
        /// <param name="col">Колонка</param>
        /// <param name="format">Формат</param>
        /// <param name="defaultValue">Значение по умолчанию, если не excel, или не найдена формула</param>
        internal void WriteRowRangeFormulaToCell(XlsFileExBase xls, string id, int row, int col, int format, double defaultValue, FormulaRowsRange rowsRange = null)
        {
            if (_isFormingFormulasToCell)
            {
                var formula = new StringBuilder("=SUM(");

                #region Формирование формулы

                var formingFormulaAction = (Action<string, int, int, int, int?>) ((sheetName, row1, row2, col1, col2) =>
                {
                    var sheet = string.Empty;
                    if (!string.IsNullOrEmpty(sheetName))
                    {
                        sheet = "'" + sheetName + "'!";
                    }

                    if (row1 == row2 && !col2.HasValue)
                    {
                        formula.Append(sheet + TCellAddress.EncodeColumn(col1)).Append(row1)
                            .Append(",");
                    }
                    else
                    {
                        formula.Append(sheet + TCellAddress.EncodeColumn(col1)).Append(row1)
                            .Append(":")
                            .Append(sheet + TCellAddress.EncodeColumn(col2 ?? col1)).Append(row2)
                            .Append(",");
                    }
                });

                #endregion

                #region Если пишем формулу не из словаря

                if (rowsRange != null)
                {
                    formingFormulaAction(rowsRange.SheetName, rowsRange.Row1, rowsRange.Row2, rowsRange.Col, rowsRange.Col2);
                    formula.Replace(",", ")", formula.Length - 1, 1); //Удаляем последний ;
                    xls.SetCellValue(row, col, new TFormula(formula.ToString()), format);
                    return;
                }

                #endregion

                #region Пишем формулу из словаря

                List<FormulaRowsRange> existsRanges;
                if (xls.FormulasSum.TryGetValue(id, out existsRanges) && existsRanges.Count > 0)
                {
                    var prevSheetName = string.Empty;
                    var prevCol = -1;
                    var prevRow1 = -1;
                    var prevRow2 = -1;
                    foreach (var formulasRange in existsRanges.OrderBy(r => r.SheetName)
                        .ThenBy(r => r.Row1).ThenBy(r => r.Row2))
                    {
                        //Обрабатываем предыдущую запись
                        if (Equals(prevSheetName, (formulasRange.SheetName ?? string.Empty))
                            && prevCol == formulasRange.Col && prevRow2 >= formulasRange.Row1 - 1)
                        {
                            //Пересекаются диапазоны, присоединяем диапазон к предыдущему
                            prevRow2 = formulasRange.Row2;
                        }
                        else
                        {
                            //Пишем предыдущую запись
                            if (prevCol > 0)
                                formingFormulaAction(prevSheetName, prevRow1, prevRow2, prevCol, null);

                            //Сохраняем текущую
                            prevSheetName = formulasRange.SheetName ?? string.Empty;
                            prevCol = formulasRange.Col;
                            prevRow1 = formulasRange.Row1;
                            prevRow2 = formulasRange.Row2;
                        }

                    }

                    //Обрабатываем последнюю запись
                    if (prevCol > 0) formingFormulaAction(prevSheetName, prevRow1, prevRow2, prevCol, null);

                    formula.Replace(",", ")", formula.Length - 1, 1); //Удаляем последний ;
                    xls.SetCellValue(row, col, new TFormula(formula.ToString()), format);

                    xls.FormulasSum.Remove(id);
                    return;
                }

                #endregion
            }
            //Нет формулы, или не надо ее формировать
            xls.SetCellValue(row, col, defaultValue, format);
        }

        internal void WriteFormulaToCell(XlsFileExBase xls, string id, int row, int col, int format, double defaultValue)
        {
            if (_isFormingFormulasToCell)
            {
                var formula = new StringBuilder("=");

                #region Пишем формулу из словаря

                List<FormulaRowsRange> existsRanges;
                if (xls.FormulasSum.TryGetValue(id, out existsRanges) && existsRanges.Count > 0)
                {
                    foreach (var formulasRange in existsRanges)
                    {
                        formula.Append(!string.IsNullOrEmpty(formulasRange.Before) ? formulasRange.Before : "+");

                        if (!string.IsNullOrEmpty(formulasRange.SheetName))
                        {
                            formula.Append("'").Append(formulasRange.SheetName).Append("'!");
                        }
                        formula.Append(TCellAddress.EncodeColumn(formulasRange.Col)).Append(formulasRange.Row1);
                    }

                    //Обрабатываем последнюю запись
                    try
                    {
                        xls.SetCellValue(row, col, new TFormula(formula.ToString()), format);
                    }
                    catch (Exception ex)
                    {
                        _errors.Append("Ошибка формирования формулы в '" + TCellAddress.EncodeColumn(col) + row + "': " + ex.Message + "\n" + formula);
                        xls.SetCellValue(row, col, defaultValue, format);
                    }


                    xls.FormulasSum.Remove(id);
                    return;
                }

                #endregion
            }
            //Нет формулы, или не надо ее формировать
            xls.SetCellValue(row, col, defaultValue, format);
        }

        public MemoryStream Export(XlsFileExBase xls)
        {
            xls.ActiveSheet = 1;

            var destStream = new MemoryStream();

            if (AdapterType == TExportExcelAdapterType.toXLS || AdapterType == TExportExcelAdapterType.toXLSx)
            {
                xls.Save(destStream, AdapterType == TExportExcelAdapterType.toXLS ? TFileFormats.Xls : TFileFormats.Xlsx);
            }

            if (AdapterType == TExportExcelAdapterType.toHTML)
            {
                var temps = new MemoryStream();
                var htmlExport = new FlexCelHtmlExport(xls, true);

                var strHtmlColor = ColorTranslator.ToHtml(_htmlDocBkColor);
                var s = new string[1];
                s[0] = "<body bgcolor=" + strHtmlColor + ">";
                htmlExport.ExtraInfo.BodyStart = s;
                using (var sw = new StreamWriter(temps))
                {
                    htmlExport.Export(sw, "", null);
                    sw.Flush();
                    var b = temps.ToArray();
                    destStream.Write(b, 0, b.Length);
                }
            }

            //----- PDF ---
            if (AdapterType == TExportExcelAdapterType.toPDF)
            {
                xls.PrintLandscape = false;
                using (var pdfExport = new FlexCelPdfExport(xls, true))
                {
                    pdfExport.Export(destStream);
                }
            }

            destStream.Position = 0;

            return destStream;
        }

        public void Dispose()
        {
            _errors = null;
            _balanceCalculated = null;
            _integralActPsList = null;
            _signaturesByBalance = null;
            _sectionsByType = null;
            _commentProps = null;
        }

        private enum EnumHeaderType
        {
            None = 0,
            Transformator = 1,
            Reactor = 2,
            Other = 3,
        }

        #endregion

        #region ISpreadsheetParams members

        public DateTime НачальнаяДата
        {
            get { return _dtStart; }
        }

        public DateTime КонечнаяДата
        {
            get { return _dtEnd; }
        }

        public string Филиал
        {
            get { return _branchName; }
        }

        public string НазваниеОтчета
        {
            get { return String.Empty; }
        }

        public string ЕдиницыИзмерения
        {
            get { return _unitDigitName; }
        }

        public Type ProryvFunctionFactory
        {
            get { return typeof(ReportFunctionFactory); }
        }

        public List<IFreeHierarchyBalanceSignature> Подписанты 
        {
            get { return null; }
        }

        #endregion
        
        /// <summary>
        /// Класс со статичными процедурами, которые можно использовать в документе
        /// </summary>
        public class ReportFunctionFactory
        {
            
        }

        internal void InitFormulas()
        {
            
        }
    }
}
