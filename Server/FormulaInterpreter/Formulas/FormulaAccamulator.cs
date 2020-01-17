using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.Servers.Calculation.DBAccess.Common;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.Servers.Calculation.FormulaInterpreter.Formulas
{
    /// <summary>
    /// Переводим получасовки в запрошенный период дискретизации
    /// </summary>
    internal class FormulaAccamulator : IDisposable
    {
        private List<TVALUES_DB> _result;
        public List<TVALUES_DB> Result
        {
            get
            {
                return _result;
            }
        }
        private List<int> _intervalTimeList;
        private int _stepOfReadHalfHours;
        private int _numbersOurDiscreteInOurPeriod;
        private int _numbersHalfHoursInOurPeriod;
        private bool _isSumm;

        private double _dValue;
        private VALUES_FLAG_DB _flagValues;
        private enumClientFormulaTPType _fFlag;
        private EnumUnitDigit? _unitDigit;

        public FormulaAccamulator(List<int> intervalTimeList, bool isSumm, EnumUnitDigit? unitDigit = null)
        {
            _result = new List<TVALUES_DB>();
            _intervalTimeList = intervalTimeList;
            _isSumm = isSumm;
            _unitDigit = unitDigit;
            if (_intervalTimeList != null)
            {
                _numbersHalfHoursInOurPeriod = _intervalTimeList[0];
            }
        }

        internal void Accamulate(double currValue, VALUES_FLAG_DB currFlag, 
            enumClientFormulaTPType fFlag = enumClientFormulaTPType.None)
        {
            if (_isSumm)
            {
                _flagValues |= currFlag; //Накапливаем состояние
                _dValue += currValue; //Накапливаем сумму
                _fFlag |= fFlag;
            }
            else
            {
                _dValue = (_dValue * _stepOfReadHalfHours + currValue) / (_stepOfReadHalfHours + 1);
            }

            if (_stepOfReadHalfHours == _numbersHalfHoursInOurPeriod) //Если закрыли период дискретизации
            {
                if (_unitDigit.HasValue)
                {
                    _dValue *= (double)_unitDigit.Value;
                }

                var fVal = new TVALUES_DB(_flagValues, _dValue, _fFlag);

                _result.Add(fVal);


                if (_intervalTimeList != null)
                {
                    if (++_numbersOurDiscreteInOurPeriod >= _intervalTimeList.Count) return; //Вышли за наш предел
                    _numbersHalfHoursInOurPeriod = _intervalTimeList[_numbersOurDiscreteInOurPeriod];
                }

                #region -------------Обнуляем значения-----------------------

                _stepOfReadHalfHours = 0;
                _flagValues = VALUES_FLAG_DB.None;
                _fFlag = enumClientFormulaTPType.None;
                _dValue = 0;

                #endregion
            }
            else
            {
                _stepOfReadHalfHours++; //Наращиваем до нужного периода дискретизации
            }
        }

        #region IDisposable

        public void Dispose()
        {
            _result = null;
            _intervalTimeList = null;
        }

        #endregion
    }
}
