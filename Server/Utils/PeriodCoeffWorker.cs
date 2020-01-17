using System;
using System.Collections.Generic;
using System.Linq;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.Servers.Calculation.DBAccess.Interface;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Utils.Data
{
    public class PeriodCoeffWorker<T> where T: struct
    {
        private IPeriodBase<T> _currentCoeff;
        private bool _isCurrentDayCoeffFound;
        private readonly bool _isTotalPeriodCoeffFound;

        private readonly IEnumerable<IPeriodBase<T>> _coeffs;

        private readonly DateTime _dtStart;
        private readonly DateTime _dtEnd;

        private DateTime? _baseDate;

        public PeriodCoeffWorker(IEnumerable<IPeriodBase<T>> coeffs, DateTime dtStart, DateTime dtEnd, IPeriodBase<T> defaultValue)
        {
            _coeffs = coeffs;
            _dtEnd = dtEnd;
            _dtStart = dtStart;

            if (_coeffs == null)
            {
                _currentCoeff = defaultValue;

                _isTotalPeriodCoeffFound = true;
                return;
            }

            //Попытка найти коэфф на весь период
            _currentCoeff = _coeffs.FirstOrDefault(t =>
                t.PeriodValue != null && _dtStart >= t.StartDateTime &&
                (!t.FinishDateTime.HasValue || _dtEnd <= t.FinishDateTime));

            _isTotalPeriodCoeffFound = _currentCoeff != null && _currentCoeff.PeriodValue.HasValue;
        }

        public void GoNextPeriodIfNotTotal(DateTime periodStart, DateTime periodEnd)
        {
            if (_isTotalPeriodCoeffFound && _currentCoeff!=null) return; //Найден коэфф. на весь период, нет смысла снова искать 

            //Пытаемся найти коэфф на указанный период
            _isCurrentDayCoeffFound = _currentCoeff != null && periodStart >= _currentCoeff.StartDateTime && (!_currentCoeff.FinishDateTime.HasValue || periodEnd <= _currentCoeff.FinishDateTime);

            if (!_isCurrentDayCoeffFound && _coeffs != null)
            {
                _isCurrentDayCoeffFound = (_currentCoeff = _coeffs.FirstOrDefault(t =>
                                             t.PeriodValue != null && periodStart >= t.StartDateTime &&
                                             (!t.FinishDateTime.HasValue || periodEnd <= t.FinishDateTime))) != null;

                _baseDate = periodStart.Date;
            }
        }

        public bool IsCurrentDayCoeffFound
        {
            get { return _isCurrentDayCoeffFound; }
        }

        public bool TryGetCurrentPeriodOrFindForHalfhour(int hhIndx, out IPeriodBase<T> currentCoeff, int discreteType = 30)
        {
            if ((_isTotalPeriodCoeffFound || _isCurrentDayCoeffFound) && _currentCoeff != null)
            {
                currentCoeff = _currentCoeff;
                return true;
            }

            if (_coeffs != null && _baseDate.HasValue)
            {
                var currHhDateTime = _baseDate.Value.AddMinutes(hhIndx * discreteType);

                _currentCoeff = currentCoeff = _coeffs
                    .LastOrDefault(t =>
                        t.PeriodValue != null && currHhDateTime >= t.StartDateTime &&
                        currHhDateTime <= t.FinishDateTime);

                return currentCoeff != null;
            }

            currentCoeff = null;
            return false;
        }

        public bool TryGetCurrentPeriodOrFindForTotalIndexHh(int totalHalfhourIndex, out IPeriodBase<T> currentCoeff, int discreteType = 30)
        {
            if ((_isTotalPeriodCoeffFound || _isCurrentDayCoeffFound) && _currentCoeff != null)
            {
                currentCoeff = _currentCoeff;
                return true;
            }

            if (_coeffs != null)
            {
                var currHhDateTime = _dtStart.AddMinutes(totalHalfhourIndex * discreteType);

                _currentCoeff = currentCoeff = _coeffs
                    .LastOrDefault(t =>
                        t.PeriodValue != null && currHhDateTime >= t.StartDateTime &&
                        currHhDateTime <= t.FinishDateTime);

                _isCurrentDayCoeffFound = currentCoeff != null && (!currentCoeff.FinishDateTime.HasValue || currentCoeff.FinishDateTime.Value >= _dtEnd);

                return currentCoeff != null;
            }

            currentCoeff = null;
            return false;
        }

        public bool TryGetForDateTime(DateTime dt, out IPeriodBase<T> currentCoeff)
        {
            if (_isTotalPeriodCoeffFound && _currentCoeff != null)
            {
                currentCoeff = _currentCoeff;
                return true;
            }

            if (_coeffs != null)
            {
                currentCoeff = _coeffs
                    .LastOrDefault(t =>
                        t.PeriodValue != null && dt >= t.StartDateTime && dt <= t.FinishDateTime);

                return currentCoeff != null;
            }

            currentCoeff = null;
            return false;
        }

        public T? GetCurrentCoeff()
        {
            if (_currentCoeff == null) return null;
            return _currentCoeff.PeriodValue;
        }
    }
}
