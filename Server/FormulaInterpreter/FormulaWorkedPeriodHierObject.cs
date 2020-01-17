using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.WCF;
using Proryv.Servers.Calculation.DBAccess.Common.Ext;
using Proryv.Servers.Calculation.DBAccess.Interface;

namespace Proryv.Servers.Calculation.FormulaInterpreter
{
    class FormulaWorkedPeriodHierObject : IWorkedPeriodHierObject
    {
        public FormulaWorkedPeriodHierObject(int tiId, DateTime dtStart, DateTime dtEnd, enumTimeDiscreteType discreteType, string timeZoneId, IMyListConverters MyListConverters, IDateTimeExtensions dateTimeExtensions,IGetNotWorkedPeriodService _iGetNotWorkedPeriodService)
        {
            TiId = tiId;
            DiscreteType = discreteType;
            //Здесь запрашиваем и считаем количество отработанных часов
            var workedPeriods = _iGetNotWorkedPeriodService.GetNotWorkedPeriods(new List<int> { tiId }, dateTimeExtensions.ClientToServer(dtStart,timeZoneId), dateTimeExtensions.ClientToServer(dtEnd,timeZoneId));

            HoursByHalfhourNumber = new Dictionary<DateTime, double>();

            //Считаем отработанные часы
            if (discreteType == enumTimeDiscreteType.DBInterval)
            {
                HoursByHalfhourNumber.Add(dtStart, CalculateNumberWorkedHours(dtStart, dtEnd, workedPeriods, timeZoneId,MyListConverters));
            }
            else
            {
                var dts = MyListConverters.GetDateTimeListForPeriod(dtStart, dtEnd, discreteType, timeZoneId.GeTimeZoneInfoById());
                for (var i = 0; i < dts.Count; i++)
                {
                    var dt = dts[i];
                    var dte = i < dts.Count - 1 ? dts[i + 1].AddMinutes(-30) : dtEnd;
                    HoursByHalfhourNumber.Add(dt, CalculateNumberWorkedHours(dt, dte, workedPeriods, timeZoneId, MyListConverters));
                }
            }
        }
        public int TiId { get; private set; }
        public enumTimeDiscreteType DiscreteType { get; private set; }
        public Dictionary<DateTime, double> HoursByHalfhourNumber { get; private set; }

        private double CalculateNumberWorkedHours(DateTime dtStart, DateTime dtEnd, List<IPeriodID> workedPeriods, string timeZoneId, IMyListConverters MyListConverters)
        {
            var totalHours = (double) MyListConverters.GetNumbersValuesInPeriod(enumTimeDiscreteType.DBHours, dtStart, dtEnd, timeZoneId);
            if (workedPeriods == null || workedPeriods.Count == 0) return totalHours;

            //Пока с точностью до 30 минут!!!
            foreach (var period in workedPeriods.Where(w => w.StartDateTime <= dtEnd && (w.FinishDateTime ?? new DateTime(2100, 1, 1)) >= dtStart))
            {
                var dts = period.StartDateTime < dtStart ? dtStart : period.StartDateTime;
                var dte = !period.FinishDateTime.HasValue || period.FinishDateTime.Value > dtEnd ? dtEnd : period.FinishDateTime.Value;

                totalHours = totalHours - (dte.AddMinutes(30).ServerToUtc() - dts.ServerToUtc()).TotalMinutes / 60.0;
            }

            return totalHours;
        }
    }
}
