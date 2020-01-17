using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlexCel.Core;
using FlexCel.XlsAdapter;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Formulas;
using Proryv.AskueARM2.Server.VisualCompHelpers.Data;
using Proryv.AskueARM2.Server.VisualCompHelpers.Section;
using Proryv.Servers.Calculation.DBAccess.Interface;

namespace Proryv.AskueARM2.Server.VisualCompHelpers.Calculators
{
    /// <summary>
    /// Калькулятор для формирования формул подитогов и итогов
    /// </summary>
    public class ExcelAnaliser : IDisposable
    {
        private XlsFileExBase _xls;

        private readonly int _colHalfHour;
        private readonly int _colDrum;
        private readonly int _colAnalis;
        private readonly string  _halfhourToIntegralUnitCoeff;
        private readonly bool _isAnalisIntegral;
        private readonly int _halfhourCount;

        private readonly Dictionary<string, List<FormulaRowsRange>> _formulas;
        private readonly Dictionary<string, string> _footers;
        private bool _isFormingFormulasToCell;

        private StringBuilder _errors;

        public ExcelAnaliser(XlsFileExBase xls, int colDrum, int colHalfHour, 
            int colAnalis, double halfhourToIntegralUnitCoeff, 
            bool isAnalisIntegral, TExportExcelAdapterType adapterType, 
            StringBuilder errors, int halfhourCount)
        {
            _colHalfHour = colHalfHour;
            _colDrum = colDrum;
            _xls = xls;
            _colAnalis = colAnalis;
            _isAnalisIntegral = isAnalisIntegral;
            _errors = errors;
            _halfhourToIntegralUnitCoeff = halfhourToIntegralUnitCoeff != 1 ? "/" + halfhourToIntegralUnitCoeff : "";
            _halfhourCount = halfhourCount;

            _isFormingFormulasToCell = adapterType == TExportExcelAdapterType.toXLS || adapterType == TExportExcelAdapterType.toXLSx;
            _formulas = new Dictionary<string, List<FormulaRowsRange>>();
            _footers = new Dictionary<string, string>();
        }

        
        /// <summary>
        /// Добавляем диапазон в формулу
        /// </summary>
        /// <param name="formulaKey">Ключ формулы</param>
        /// <param name="row">Строка</param>
        /// <param name="addHalfhour">Добавляем колонки получасовок</param>
        /// <param name="addDrum">Добавляем колонки барабанов</param>
        /// <param name="isMinus">Минусовать</param>
        /// <param name="footerName">Название подитога (если нужно), в который добавляем данные</param>
        public void AddToFormula(string formulaKey, int row, bool addHalfhour = true, bool addDrum = true, 
            bool isMinus = false)
        {
            //Добавляем барабаны для подитога
            if (!string.IsNullOrEmpty(formulaKey))
            {
                if (addDrum)
                {
                    AddRowRangeToFormulaSum(formulaKey + "_drum", new FormulaRowsRange
                    {
                        Col = _colDrum,
                        Row1 = row,
                        Row2 = row,
                        IsMinus = isMinus,
                    }); //Для итого по перетоку
                }

                //Добавляем получасовки для подитога
                if (addHalfhour)
                {
                    for (var i = 0; i < _halfhourCount; i++)
                    {
                        AddRowRangeToFormulaSum(formulaKey + "_halfhour" + i, new FormulaRowsRange
                        {
                            Col = _colHalfHour + i,
                            Row1 = row,
                            Row2 = row,
                            IsMinus = isMinus,
                        });
                    }
                }
            }

            if (_isAnalisIntegral && addHalfhour && addDrum)
            {
                //Делаем сравнительный анализ барабанов и получасовок

                var formulaHalfhour = TCellAddress.EncodeColumn(_colHalfHour) + row;

                var formulaStr = formulaHalfhour + " - " +
                                 TCellAddress.EncodeColumn(_colDrum) + row 
                                 + _halfhourToIntegralUnitCoeff;

                _xls.SetCellValue(row, _colAnalis,
                    new TFormula("=" + formulaStr), _xls.ProfileFormat);

                _xls.SetCellValue(row, _colAnalis + 1,
                    new TFormula("=(" + formulaStr + ")/" + formulaHalfhour), _xls.ProfileFormat);
            }
        }

        /// <summary>
        /// Добавляем диапазон в формулу
        /// </summary>
        /// <param name="footerKey">Ключ формулы</param>
        /// <param name="row">Строка</param>
        /// <param name="addHalfhour">Добавляем колонки получасовок</param>
        /// <param name="addDrum">Добавляем колонки барабанов</param>
        /// <param name="isMinus">Минусовать</param>
        /// <param name="footerName">Название подитога (если нужно), в который добавляем данные</param>
        public void AddToFooter(string footerKey, string footerName, byte channel, int row, bool addHalfhour = true, bool addDrum = true)
        {
            //Добавляем барабаны для подитога
            if (string.IsNullOrEmpty(footerKey)) return;

            _footers[footerKey] = footerName;

            AddToFormula(footerKey + "_" + channel, row, addHalfhour, addDrum);
        }

        public void WriteFormula(string formulaKey, int row)
        {
            WriteRowRangeFormulaToCell(formulaKey + "_drum", row, _colDrum, _xls.IntegralBoldFormat, 0);

            for (var i = 0; i < _halfhourCount; i++)
            {
                WriteRowRangeFormulaToCell(formulaKey + "_halfhour" + i, row, _colHalfHour + i, _xls.ProfileBoldFormat, 0);
            }
        }

        public bool WriteFooters(ref int row)
        {
            if (_footers.Count == 0) return false;

            foreach (var footerResultPair in _footers)
            {
                var footerKey = footerResultPair.Key;
                var footerName = footerResultPair.Value;

                _xls.SetCellValue(row, _xls.StartCol + 1, "Итого по " + footerName, _xls.BoldLeftFormatThin);
                _xls.MergeCells(row, _xls.StartCol + 1, row, _xls.StartCol + 9);

                for (byte chanel = 1; chanel <= 2; chanel++)
                {
                    row++;
                    _xls.SetCellValue(row, _xls.StartCol + 1, chanel == 1 ? "Прием" : "Отдача");
                    _xls.MergeCells(row, _xls.StartCol + 1, row, _xls.StartCol + 8);

                    WriteFormula(footerKey + "_" + chanel, row);
                }

                row++;
            }

            return true;
        }

        public string GenerateFooterName(EnumIntegralActFooterType footerType, TI_Integral_ValuesForHalfHours2 tpVals)
        {
            switch (footerType)
            {
                case EnumIntegralActFooterType.FskMsk:
                    return (tpVals.IsMSK ? "ПС 220кВ и ниже" : "ПС 330кВ и выше");

                case EnumIntegralActFooterType.FactVoltage:
                    return string.Format("{0:###0.##}, кВ", tpVals.Voltage);

                case EnumIntegralActFooterType.TariffLevelVoltage:
                    if (tpVals.TpVoltageLevel.HasValue)
                    {
                        var d = tpVals.TpVoltageLevel.Value.GetEnumDescription();
                        if (d != null) return d.FullDescription;
                    }
                    break;
            }

            return string.Empty;
        }

        public string GenerateFooterKey(EnumIntegralActFooterType footerType, TI_Integral_ValuesForHalfHours2 tpVals)
        {
            switch (footerType)
            {
                case EnumIntegralActFooterType.FskMsk:
                    return (tpVals.IsMSK ? "220" : "330");
                case EnumIntegralActFooterType.FactVoltage:
                    return tpVals.Voltage.ToString();

                case EnumIntegralActFooterType.TariffLevelVoltage:
                    if (tpVals.TpVoltageLevel.HasValue)
                    {
                        return tpVals.TpVoltageLevel.Value.ToString();
                    }
                    break;
            }

            return string.Empty;
        }

        #region Работа с формулами

        private void AddRowRangeToFormulaSum(string id, FormulaRowsRange addedRange)
        {
            if (!_isFormingFormulasToCell) return;

            List<FormulaRowsRange> existsRanges;
            if (!_formulas.TryGetValue(id, out existsRanges))
            {
                existsRanges = new List<FormulaRowsRange>();
                _formulas.Add(id, existsRanges);
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
        internal void WriteRowRangeFormulaToCell(string id, int row, int col, int format, double defaultValue, FormulaRowsRange rowsRange = null)
        {
            if (_isFormingFormulasToCell)
            {
                var formula = new StringBuilder("=SUM(");

                #region Формирование формулы

                var formingFormulaAction = (Action<string, int, int, int, int?, bool>)((sheetName, row1, row2, col1, col2, isMinus) =>
                {
                    var sheet = isMinus ? "-" : string.Empty;
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
                    formingFormulaAction(rowsRange.SheetName, rowsRange.Row1, rowsRange.Row2, rowsRange.Col, rowsRange.Col2, rowsRange.IsMinus);
                    formula.Replace(",", ")", formula.Length - 1, 1); //Удаляем последний ;
                    _xls.SetCellValue(row, col, new TFormula(formula.ToString()), format);
                    return;
                }

                #endregion

                #region Пишем формулу из словаря

                List<FormulaRowsRange> existsRanges;
                if (_formulas.TryGetValue(id, out existsRanges) && existsRanges.Count > 0)
                {
                    var prevSheetName = string.Empty;
                    var prevCol = -1;
                    var prevRow1 = -1;
                    var prevRow2 = -1;
                    bool isMinus = false;
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
                                formingFormulaAction(prevSheetName, prevRow1, prevRow2, prevCol, null, isMinus);

                            //Сохраняем текущую
                            prevSheetName = formulasRange.SheetName ?? string.Empty;
                            prevCol = formulasRange.Col;
                            prevRow1 = formulasRange.Row1;
                            prevRow2 = formulasRange.Row2;
                        }

                        isMinus = formulasRange.IsMinus;
                    }

                    //Обрабатываем последнюю запись
                    if (prevCol > 0) formingFormulaAction(prevSheetName, prevRow1, prevRow2, prevCol, null, isMinus);

                    formula.Replace(",", ")", formula.Length - 1, 1); //Удаляем последний 
                    if (col <= 16384)
                    {
                        _xls.SetCellValue(row, col, new TFormula(formula.ToString()), format);
                    }

                    _formulas.Remove(id);
                    return;
                }

                #endregion
            }
            //Нет формулы, или не надо ее формировать
            _xls.SetCellValue(row, col, defaultValue, format);
        }

        internal void WriteFormulaToCell(XlsFile xls, string id, int row, int col, int format, double defaultValue)
        {
            if (_isFormingFormulasToCell)
            {
                var formula = new StringBuilder("=");

                #region Пишем формулу из словаря

                List<FormulaRowsRange> existsRanges;
                if (_formulas.TryGetValue(id, out existsRanges) && existsRanges.Count > 0)
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
                        if (_errors != null)
                        {
                            _errors.Append("Ошибка формирования формулы в '" + TCellAddress.EncodeColumn(col) + row + "': " + ex.Message + "\n" + formula);
                        }
                        xls.SetCellValue(row, col, defaultValue, format);
                    }


                    _formulas.Remove(id);
                    return;
                }

                #endregion
            }
            //Нет формулы, или не надо ее формировать
            xls.SetCellValue(row, col, defaultValue, format);
        }

        #endregion

        #region IDispose

        public void Dispose()
        {
            _xls = null;
            _errors = null;
        }

        #endregion
    }
}
