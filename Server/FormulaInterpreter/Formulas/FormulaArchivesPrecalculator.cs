using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Public.Common;
using Proryv.AskueARM2.Server.WCF;
using Proryv.Servers.Calculation.DBAccess.Common;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;
using Proryv.Servers.Calculation.FormulaInterpreter;
using Proryv.Servers.Calculation.FormulaInterpreter.Formulas;

namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas
{
    //Без многопоточности!!!
    /// <summary>
    /// Прекалькуляция формул для расчета часов работы точки или определять работает ли точка в данной получасовке
    /// </summary>
    public class FormulaArchivesPrecalculator : IFormulaArchivesPrecalculator
    {
        private readonly DateTime _dtServerStart;
        private readonly DateTime _dtServerEnd;

        private readonly DateTime _dtClientStart;
        private readonly DateTime _dtClientEnd;

        private readonly bool _isReadCalculatedValues;
        private readonly string _timeZoneId;

        private readonly EnumDataSourceType? _dataSourceType;
        private readonly IMyListConverters _myListConverters;
        private readonly IGetNotWorkedPeriodService _getNotWorkedPeriodService;
        private readonly IDateTimeExtensions _dateTimeExtensions;

        private readonly bool _isRequestArchives;
        /// <summary>
        /// Ошибки расчетов или запросов в архив
        /// </summary>
        public readonly StringBuilder Errors;

        /// <summary>
        /// Словарь с преварительным обсчетом
        /// </summary>
        private readonly List<TArchiveHierObjectPeriod> _precalculateArchives;

        /// <summary>
        /// Словарь с отработанными часами за запрашиваемый период
        /// </summary>
        private readonly List<IWorkedPeriodHierObject> _precalculateWorkedPeriod;

        public FormulaArchivesPrecalculator(string timeZoneId, bool isReadCalculatedValues, DateTime dtServerStart, DateTime dtServerEnd, EnumDataSourceType? dataSourceType, IMyListConverters myListConverters,
            IGetNotWorkedPeriodService IGetNotWorkedPeriodService, IDateTimeExtensions DateTimeExtensions)
        {

            _timeZoneId = timeZoneId;
            _isReadCalculatedValues = isReadCalculatedValues;
            _dataSourceType = dataSourceType;
            _myListConverters = myListConverters;
            this._getNotWorkedPeriodService = IGetNotWorkedPeriodService;
            _dateTimeExtensions = DateTimeExtensions;
            _isRequestArchives = dtServerStart != default(DateTime);
            if (_isRequestArchives)
            {
                _dtServerStart = RoundToHalfHour(dtServerStart, true);
                _dtServerEnd = RoundToHalfHour(dtServerEnd, true);
                _dtClientStart = _dtServerStart.ServerToClient(_timeZoneId);
                _dtClientEnd = _dtServerEnd.ServerToClient(_timeZoneId);
            }

            Errors = new StringBuilder();

            _precalculateArchives = new List<TArchiveHierObjectPeriod>();
            _precalculateWorkedPeriod = new List<IWorkedPeriodHierObject>();
        }

        /// <summary>
        /// Результат прекалькуляции 
        /// </summary>
        /// <returns></returns>
        public TVALUES_DB GetArchive(IHierarchyChannelID id, int halfHourIndx, enumTimeDiscreteType discreteType, List<Servers.Calculation.DBAccess.Interface.Data.TVALUES_DB> requestedArchives)
        {
            if (!_isRequestArchives) return null;

            List<ArchiveHierObjectPeriodValue> allPeriodArchive = null;
            if (_precalculateArchives.Count > 0)
            {
                var cashedArchives = _precalculateArchives.FirstOrDefault(a => a.Id.Equals(id) && a.DiscreteType == discreteType);
                if (cashedArchives != null)
                {
                    allPeriodArchive = cashedArchives.Vals;
                }
            }

            if (allPeriodArchive == null)
            {

                DateTime dtStart, dtEnd; //Период в рамках которого нужны данные по указанной точке
                switch (discreteType)
                {
                    case enumTimeDiscreteType.DB24Hour: //нужны посуточные данные
                        dtStart = _dtClientStart.Date; //округляем до начала суток клиента
                        dtEnd = new DateTime(_dtClientEnd.Year, _dtClientEnd.Month, _dtClientEnd.Day, 23, 30, 0);
                        break;
                    case enumTimeDiscreteType.DBMonth: //нужны данные по месяцам
                        dtStart = new DateTime(_dtClientStart.Year, _dtClientStart.Month, 1); //округляем до начала месяца клиента
                        dtEnd = new DateTime(_dtClientEnd.Year, _dtClientEnd.Month, 1).AddMonths(1).AddMinutes(-30); //Округляем до окончания месяца
                        break;
                    default:
                        //В остальных случаях границы периода не расширяем
                        dtStart = _dtClientStart;
                        dtEnd = _dtClientEnd;
                        break;
                }


                //смотрим нужно ли запрашивать в архим
                if (requestedArchives != null && dtStart == _dtClientStart && dtEnd == _dtClientEnd)
                {
                    var archives = _myListConverters.ConvertHalfHoursToOtherList(discreteType, requestedArchives, _dtClientStart, _dtClientEnd, _timeZoneId);
                    var objectArchive = new TArchiveHierObjectPeriod(id, dtStart, dtEnd, archives, discreteType, _timeZoneId, _myListConverters);
                    allPeriodArchive = objectArchive.Vals;
                    if (_precalculateArchives != null) _precalculateArchives.Add(objectArchive);
                }
                else
                {
                    IArchivesHierObject archives = new ArchivesHierObjectProvider(new List<IHierarchyChannelID> { id }, true, dtStart, dtEnd, _dataSourceType,
                        discreteType, EnumUnitDigit.None, _isReadCalculatedValues, _timeZoneId);

                    if (archives.Errors.Length > 0)
                    {
                        Errors.Append(archives.Errors);
                    }

                    if (archives.result_Values != null && archives.result_Values.Count > 0)
                    {
                        var values = archives.result_Values.FirstOrDefault().Value;
                        var objectArchive = new TArchiveHierObjectPeriod(id, dtStart, dtEnd, values, discreteType, _timeZoneId, _myListConverters);
                        allPeriodArchive = objectArchive.Vals;
                        if (_precalculateArchives != null) _precalculateArchives.Add(objectArchive);

                    }
                }
            }

            if (allPeriodArchive == null) return null;

            ArchiveHierObjectPeriodValue objectPeriodValue;
            if (discreteType == enumTimeDiscreteType.DBInterval)
            {
                objectPeriodValue = allPeriodArchive.FirstOrDefault();
            }
            else
            {
                var dt = _dtServerStart.ServerToUtc().AddMinutes(30 * halfHourIndx).UtcToClient(_timeZoneId);
                //Теперь выбираем нужный период (текущие сутки, текущий месяц) из всего архива
                switch (discreteType)
                {
                    case enumTimeDiscreteType.DBMonth: //нужны данные по месяцам
                        objectPeriodValue = allPeriodArchive.FirstOrDefault(v => v.Year == dt.Year && v.Month == dt.Month);
                        break;
                    case enumTimeDiscreteType.DB24Hour: //нужны посуточные данные
                        objectPeriodValue = allPeriodArchive.FirstOrDefault(v => v.Year == dt.Year && v.Month == dt.Month && v.Day == dt.Day);
                        break;
                    default: //нужны часовые
                        objectPeriodValue = allPeriodArchive.FirstOrDefault(v => v.Year == dt.Year && v.Month == dt.Month && v.Day == dt.Day && v.Hour == dt.Hour);
                        break;
                }
            }
            if (objectPeriodValue != null) return objectPeriodValue.Val;

            return null;
        }


        /// <summary>
        /// Индекс значения результата
        /// </summary>
        /// <returns></returns>
        public double ИндексЗначения(IHierarchyChannelID id, int halfHourIndx, enumTimeDiscreteType discreteType)
        {
            if (halfHourIndx == 0) return 0;

            if (discreteType == enumTimeDiscreteType.DBHalfHours)
            {
                return halfHourIndx;
            }
            if (discreteType == enumTimeDiscreteType.DBHours)
            {

                if (halfHourIndx % 2 != 0)
                {
                    return 0;
                }
                return halfHourIndx / 2;
            }
            if (discreteType == enumTimeDiscreteType.DB24Hour)
            {
                if (halfHourIndx % 24 != 0)
                {
                    return 0;
                }
                return halfHourIndx / 24;
            }
            return 1;

        }


        /// <summary>
        /// Количество часов в расчетном периоде, которое отработала точка
        /// </summary>
        /// <returns></returns>
        public double Ччи(IHierarchyChannelID id, int halfHourIndx, enumTimeDiscreteType discreteType)
        {
            if (!_isRequestArchives || id == null) return 0;

            int tiId;
            if (!int.TryParse(id.ID, out tiId)) return 0;

            Dictionary<DateTime, double> hoursByHalfhourNumber = null;
            if (_precalculateWorkedPeriod.Count > 0)
            {
                var cashedArchives = _precalculateWorkedPeriod.FirstOrDefault(a => a.TiId == tiId && a.DiscreteType == discreteType);
                if (cashedArchives != null)
                {
                    hoursByHalfhourNumber = cashedArchives.HoursByHalfhourNumber;
                }
            }

            if (hoursByHalfhourNumber == null)
            {
                //Нет в словаре, надо добавить
                DateTime dtStart, dtEnd; //Период в рамках которого нужны данные по указанной точке
                switch (discreteType)
                {
                    case enumTimeDiscreteType.DB24Hour: //нужны посуточные данные
                        dtStart = _dtClientStart.Date; //округляем до начала суток клиента
                        dtEnd = new DateTime(_dtClientEnd.Year, _dtClientEnd.Month, _dtClientEnd.Day, 23, 30, 0);
                        break;
                    case enumTimeDiscreteType.DBMonth: //нужны данные по месяцам
                        dtStart = new DateTime(_dtClientStart.Year, _dtClientStart.Month, 1); //округляем до начала месяца клиента
                        dtEnd = new DateTime(_dtClientEnd.Year, _dtClientEnd.Month, 1).AddMonths(1).AddMinutes(-30); //Округляем до окончания месяца
                        break;
                    default:
                        //В остальных случаях границы периода не расширяем
                        dtStart = _dtClientStart;
                        dtEnd = _dtClientEnd;
                        break;
                }

                var workedPeriodHierObject = new FormulaWorkedPeriodHierObject(tiId, dtStart, dtEnd, discreteType, _timeZoneId, _myListConverters, _dateTimeExtensions, _getNotWorkedPeriodService);
                if (_precalculateWorkedPeriod != null) _precalculateWorkedPeriod.Add(workedPeriodHierObject);
                hoursByHalfhourNumber = workedPeriodHierObject.HoursByHalfhourNumber;
            }

            if (hoursByHalfhourNumber == null) return 0;

            //Считаем количество отработанных часов в нужном периоде дискретизвции
            double hours;
            if (discreteType == enumTimeDiscreteType.DBInterval)
            {
                hoursByHalfhourNumber.TryGetValue(_dtClientStart, out hours);
            }
            else
            {
                var dt = _dtServerStart.ServerToUtc().AddMinutes(30 * halfHourIndx).UtcToClient(_timeZoneId);
                //Теперь выбираем нужный период (текущие сутки, текущий месяц) из всего архива
                switch (discreteType)
                {
                    case enumTimeDiscreteType.DBMonth: //нужны данные по месяцам
                        hours = hoursByHalfhourNumber.FirstOrDefault(v => v.Key.Year == dt.Year && v.Key.Month == dt.Month).Value;
                        break;
                    case enumTimeDiscreteType.DB24Hour: //нужны посуточные данные
                        hours = hoursByHalfhourNumber.FirstOrDefault(v => v.Key.Year == dt.Year && v.Key.Month == dt.Month && v.Key.Day == dt.Day).Value;
                        break;
                    default: //нужны часовые
                        hours = hoursByHalfhourNumber.FirstOrDefault(v => v.Key.Year == dt.Year && v.Key.Month == dt.Month && v.Key.Day == dt.Day && v.Key.Hour == dt.Hour).Value;
                        break;
                }
            }

            return hours;
        }



        /// <summary>
        /// Определяем работала ли точка в расчетном периоде
        /// </summary>
        /// <param name="id">Идентифиуатор объекта</param>
        /// <param name="halfHourIndx">Индекс получасовки</param>
        /// <param name="discreteType">Период дискретизации</param>
        /// <returns></returns>
        public bool ЕслиВключено(IHierarchyChannelID id, int halfHourIndx, enumTimeDiscreteType discreteType)
        {

            return true;
        }



        /// <summary>
        /// Корректировка даты-время по часовому поясу сервера к часовому поясу сервера клиента
        /// </summary>
        /// <returns></returns>
        public static DateTime ServerToClient(DateTime date, string clientTimeZoneId)
        {
            if (string.IsNullOrEmpty(clientTimeZoneId) || date.Year < 1990 || date.Year > 2100
                || string.Equals(clientTimeZoneId, TimeZoneInfo.Local.Id)) return date;

            try
            {
                var clientTimeZoneInfo = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(tz => tz.Id == clientTimeZoneId);
                var serverTimeZoneInfo = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(tz => tz.Id == TimeZoneInfo.Local.Id);
                return DateTime.SpecifyKind(MyTimeZoneInfo.ConvertTime(date, serverTimeZoneInfo, clientTimeZoneInfo), DateTimeKind.Unspecified);
            }
            catch
            {
                return date;
            }
        }



        public static DateTime RoundToHalfHour(DateTime date, bool isLess)
        {
            var min = Math.Abs(date.TimeOfDay.Minutes / 30) % 2;
            if (isLess)
            {
                if (min == 1) min = 30;
            }
            else
            {
                if (date.TimeOfDay.Minutes != 0)
                {
                    if (min == 0) min = 30;
                    else
                    {
                        if (date.TimeOfDay.Minutes != 30)
                        {
                            min = 0;
                            date = date.AddHours(1);
                        }
                        else min = 30;
                    }
                }
            }
            date = new DateTime(date.Year, date.Month, date.Day, date.Hour, min, 0);
            return date;
        }
    }
}
