using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Internal.Utils;
using Proryv.AskueARM2.Server.DBAccess.Public.ActUndercount;
using Proryv.AskueARM2.Server.DBAccess.Public.ConsumptionSchedule;
using Proryv.AskueARM2.Server.DBAccess.Public.Utils.Data;
using Proryv.AskueARM2.Server.WCF;
using Proryv.Servers.Calculation.DBAccess.Common;
using Proryv.Servers.Calculation.DBAccess.Common.Ext;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Utils
{
    public static class ArchiveValuesFactory
    {
        //---------------------------------------------------
        /// <summary>
        /// Читаем получасоки с датаридера из таблицы ArchXXX_30_Virtual_X
        /// </summary>
        /// <returns></returns>
        public static void GetDatareaderValues(
            IHalfhoursCalculatedPu calculatedPu,
            SqlDataReader dr,
            TDRParams param,
            List<TDateTimeOV> dateTimeOv,
            bool isConsumptionScheduleAnalyse,
            ConsumptionScheduleTypesByTI consumptionSchedule,
            EnumDataSourceType? neededDataSourceType,
            StringBuilder errors,
            Dictionary<int, DBDataSourceToTiTp> dataSourceTiTpByMonthNumber, 
            Dictionary<int, List<DBPriorityList>> totalPriorityByMonthNumber,
            Dictionary<int, EnumDataSourceType> dataSourceIdToTypeDict,
            List<Tuple<int, int?>> notWorkedRange = null, 
            IEnumerable<ArchBit_30_Values_WS> wsByTiChArchives = null,
            List<PeriodIndexesTpInSection> rangeTiIndexesInSectionList = null,
            List<TLossesCoefficient> lossesCoefficients = null,
            Dictionary<DateTime, TPreviousDayDispatchDateTimeHistory> previousDayDispatchDateTimeHistory = null,
            bool calculateMinAndMax = false, 
            DateTime? ovStart = null, 
            DateTime? ovEnd = null, 
            List<int> ovIntervalTimes = null)
        {
            var isReadCalculatedValues = calculatedPu.IsReadCalculatedValues;
            var isCoeffEnabled = calculatedPu.IsCoeffEnabled;
            var isCoeffTransformationDisabledByDefault = calculatedPu.IsCoeffTransformationDisabled;
            var isClosedPeriod = calculatedPu.IsClosedPeriod;
            var isOv = calculatedPu.IsOV;
            var isCa = calculatedPu.IsCA;
            var tiType = calculatedPu.TIType;
            var flagByOtherDataSourceList = calculatedPu.FlagByOtherDataSource;
            var powerValue = calculatedPu.PowerValue;
            var dTStartServer = ovStart ?? param.DtServerStart;
            var dTEndServer = ovEnd ?? param.DtServerEnd;
            var discreteType = param.DiscreteType;
            var typeInformation = param.IsPower;

            var unitDigit = param.UnitDigit;
            var ovMode = param.OvMode;
            var isOnlyIfOvReplacedOtherTi = ovMode.CheckFlag(enumOVMode.OnlyIfOvReplacedOtherTi);
            var intervalTimeList = ovIntervalTimes ?? param.IntervalTimeList;
            var monthYearList = param.MonthYearList;
            var timeZoneId = param.ClientTimeZoneId;

            var isExistsNotWorkedPeriod = notWorkedRange != null && notWorkedRange.Count > 0;

            #region Помошники, помогающие быстро найти параметр в нужной получасовке

            var coeffTranformationWorker = new PeriodCoeffWorker<double>(calculatedPu.CoeffTransformations,
                dTStartServer, dTEndServer,
                new TPeriodCoeff
                {
                    PeriodValue = 1,
                    StartDateTime = dTStartServer,
                    FinishDateTime = dTEndServer,
                });

            var useLossesCoeff = lossesCoefficients != null && lossesCoefficients.Count > 0;
            var coeffLossesWorker = new PeriodCoeffWorker<double>(lossesCoefficients, dTStartServer,
                dTEndServer, new TPeriodCoeff
                {
                    PeriodValue = 1,
                    StartDateTime = dTStartServer,
                    FinishDateTime = dTEndServer,
                });

            var coeffTransformationDisabledWorker = new PeriodCoeffWorker<bool>(
                calculatedPu.CoeffTransformationDisabledPeriods, dTStartServer, dTEndServer,
                new TPeriodStatus
                {
                    PeriodValue = calculatedPu.IsCoeffTransformationDisabled,
                    StartDateTime = dTStartServer,
                    FinishDateTime = dTEndServer,
                });

            #endregion

            var exceptedHalfhoursCalculator = new ExceptedHalfhoursCalculator(dTStartServer, timeZoneId,
                dataSourceTiTpByMonthNumber, dataSourceIdToTypeDict, totalPriorityByMonthNumber,
                isExistsNotWorkedPeriod, notWorkedRange, intervalTimeList, unitDigit
                , coeffTranformationWorker, coeffLossesWorker, coeffTransformationDisabledWorker, useLossesCoeff, isCoeffTransformationDisabledByDefault);

            var result = new List<TVALUES_DB>();
            try
            {
                var isReturnPreviousDispatchDateTime = previousDayDispatchDateTimeHistory != null;
                var isPower = typeInformation == enumTypeInformation.Power;
                //-------------Индексы в базе-----------------
                var dtStartClient = dTStartServer.ServerToClient(timeZoneId);
                bool isActUndercountExists = false;

                #region Если расчтеный профиль и не КА то надо запросить данные по акту недоучета

                var actUndercountValues = new List<TActUndercountPeriod>();

                isReadCalculatedValues = isReadCalculatedValues & !isCa;

                //Данные по акту недоучета
                if (isReadCalculatedValues && param.UseActUndercount)
                {
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            actUndercountValues.Add(new TActUndercountPeriod(
                                dr.GetDouble(
                                    0), //Индекс 1 получасовки (не целое, десятичная часть учитывает отклонение от получаса)
                                dr.GetDouble(1), //Индекс последней получасовки (тоже не целое)
                                dr.GetDouble(2), (EnumActMode) dr.GetByte(3),
                                dr.GetGuid(4),
                                dr[5] as bool?,
                                dr[6] as bool?
                                )); //Значение каждой получасовки, считаем исходя из общего количества минут
                        }
                    }

                    isActUndercountExists = actUndercountValues.Count > 0;

                    dr.NextResult();

                    if (dr.HasRows)
                    {
                        var hds = new Dictionary<Guid, Dictionary<int, double>>();
                        Dictionary<int, double> hhs = null;
                        Guid? prevActUndercountUn = null;

                        //Читаем получасовки
                        while (dr.Read())
                        {
                            var actUndercountUn = dr.GetGuid(0);

                            if (!Nullable<Guid>.Equals(actUndercountUn, prevActUndercountUn))
                            {
                                hhs = new Dictionary<int, double>();
                                hds[actUndercountUn] = hhs;
                            }

                            hhs[dr.GetInt32(1)] = dr.GetDouble(2);

                            prevActUndercountUn = actUndercountUn;
                        }

                        hhs = null;

                        foreach (var actUndercountPeriod in actUndercountValues)
                        {
                            Dictionary<int, double> hh;
                            if (hds.TryGetValue(actUndercountPeriod.ActUndercount_UN, out hh))
                            {
                                actUndercountPeriod.Halfhours = hh;
                            }
                        }
                    }

                    dr.NextResult();
                }

                #endregion

                if (isOv)
                {
                    calculatedPu.ActUndercountValues = actUndercountValues;
                }

                isReadCalculatedValues =
                    isReadCalculatedValues &
                    tiType != enumTIType.ArchCalc_30_Month; //Для малых ТИ нет расчетного профиля

                exceptedHalfhoursCalculator.SetActUndercount(isActUndercountExists, actUndercountValues);

                
                var dataSourceCalculatorsDict = new Dictionary<EnumDataSourceType, TValidateDataSourceCalculator>();
                var allDatasource = DBStatic.AllDataSource;

                var prevMonthNumber = dTStartServer.Year * 12 + dTStartServer.Month;

                var numberFirstColumn = 2;
                var numberFirstCalculatedColumn = 52;

                var deltaCalculatedVsMainColumns = numberFirstCalculatedColumn - numberFirstColumn;
                //--С какого по какой перебираем
                var firstIndex = isReadCalculatedValues ? numberFirstCalculatedColumn : numberFirstColumn;
                var firstNeededValue = (int)(dTStartServer.TimeOfDay.TotalMinutes / 30) + firstIndex;
                var indxFirstChannelValue = firstNeededValue;

                var indxLastChannelValue = 47 + firstIndex;
                var stepOfReadHalfHours = 0;
                
                var isHh = discreteType == enumTimeDiscreteType.DBHalfHours;

                var isMakePowerInformation = powerValue != null && isPower;
                calculatedPu.IsExistsMainHalfHoursValues = false;
                
                #region Читаем получасовки

                if (dr.HasRows)
                {
                    //--------------------------------------------------------
                    var currFlag = VALUES_FLAG_DB.None;

                    //Точка в режиме обходного выключателя и замещает на данной получасовке точку из общего списка точек

                    var dtIndx = dr.GetOrdinal("EventDate");
                    var coeffIndx = dr.GetOrdinal("Coeff");
                    var validIndx = dr.GetOrdinal("ValidStatus");
                    var dataSourceIndx = dr.GetOrdinal("DataSource_ID"); //Идентификатор источника
                    var contrReplaceStatusIndx = 0;

                    int dispatchIndx;
                    int dtPrevDispIndx;
                    try
                    {
                        dispatchIndx = dr.GetOrdinal("DispatchDateTime");
                        dtPrevDispIndx = isCa ? dtIndx : dr.GetOrdinal("PreviousDispatchDateTime");
                    }
                    catch (Exception)
                    {
                        dispatchIndx = dtIndx;
                        dtPrevDispIndx = dtIndx;
                    }

                    //EnumDataSourceType? lastDataSourceType = null; //Полученный источник

                    var manualEnterStatusIndx = dr.GetOrdinal("ManualEnterStatus");
                    var manualValidStatusIndx = dr.GetOrdinal("ManualValidStatus");

                    if (!isCa)
                    {
                        contrReplaceStatusIndx = dr.GetOrdinal("ContrReplaceStatus"); //Достоверность основного профиля
                    }
                    //else if (isCa)
                    //{
                    //    manualEnterStatusIndx = dr.GetOrdinal("ManualEnterStatus");
                    //}

                    var scsIndx = dr.GetOrdinal("StartChannelStatus");
                    var fcsIndx = dr.GetOrdinal("FinishChannelStatus");

                    prevMonthNumber = -1;

                    //-------------Читаем получасовки из базы--------------------------
                    while (dr.Read())
                    {
                        var baseDate = (DateTime) dr[dtIndx]; //Дата текущей записи

                        //Маска, признак достоверности или не достоверности данных
                        var validStatus = DBValues.DBInt64(dr, validIndx);
                        //TODO пока не обрабатываем ручной статус

                        //Маска, признак ручного изменения статуса достоверности (признавали достоверными, или не достоверными)
                        var manualStatus = DBValues.DBInt64(dr, manualEnterStatusIndx);

                        var lastDispatchedDateTime = dr[dispatchIndx] as DateTime?;
                        var baseDateClient = baseDate.ServerToClient(timeZoneId);
                        if (baseDateClient < dtStartClient) baseDateClient = dtStartClient;
                        var monthYear = (ushort) (baseDateClient.Year * 12 + baseDateClient.Month);
                        var currMonthNumber = (ushort) (baseDateClient.Year * 12 + baseDateClient.Month);

                        //Определяем тип источника
                        EnumDataSourceType currentDataSourceType; //Полученный источник
                        if (dataSourceIdToTypeDict == null ||
                            !dataSourceIdToTypeDict.TryGetValue((int) dr[dataSourceIndx], out currentDataSourceType))
                        {
                            continue; //Непонятный источник, лучше пропустить
                        }

                        if (baseDate > dTEndServer.Date)
                        {
                            break; //Вышли за наш диапазон    
                        }

                        #region Словарь калькуляторов по источникам данных

                        TValidateDataSourceCalculator dataSourceCalculator; //Калькулятор для данного источника данных
                        if (!dataSourceCalculatorsDict.TryGetValue(currentDataSourceType, out dataSourceCalculator))
                        {
                            dataSourceCalculator = new TValidateDataSourceCalculator(currentDataSourceType,
                                stepOfReadHalfHours, intervalTimeList,
                                unitDigit,
                                consumptionSchedule, discreteType,
                                indxFirstChannelValue, isPower, true,
                                isClosedPeriod, monthYearList, rangeTiIndexesInSectionList, calculateMinAndMax, dateTimeOv);

                            if (dataSourceIdToTypeDict != null)
                            {
                                DBDataSourceToTiTp dataSourceToTiTp = null;
                                var isCurrentMonthExistsManualSet =
                                    dataSourceTiTpByMonthNumber != null &&
                                    dataSourceTiTpByMonthNumber.TryGetValue(prevMonthNumber, out dataSourceToTiTp) &&
                                    dataSourceToTiTp != null;

                                //Определяем приоритетность данного источника
                                var dataSourceIdToType =
                                    dataSourceIdToTypeDict.FirstOrDefault(ds => ds.Value == currentDataSourceType);
                                SetPriorityDataSource(isCurrentMonthExistsManualSet,
                                    dataSourceCalculator.ValidateDataSource,
                                    totalPriorityByMonthNumber,
                                    currMonthNumber, dataSourceIdToType.Key, dataSourceToTiTp);
                            }

                            dataSourceCalculatorsDict[currentDataSourceType] = dataSourceCalculator;
                        }

                        #endregion

                        dataSourceCalculator.CompareAndGetLastDispatchDateTime(lastDispatchedDateTime, monthYear);

                        TPreviousDayDispatchDateTimeHistory previousDayDispatchDateTime = null;
                        if (isReturnPreviousDispatchDateTime)
                        {
                            previousDayDispatchDateTime = new TPreviousDayDispatchDateTimeHistory
                            {
                                EventDateTime = baseDate,
                                PreviousDayDispatchDateTime = dr[dtPrevDispIndx] as DateTime?,
                                DispatchDateTime = lastDispatchedDateTime,
                            };
                        }

                        //if (lastDataSourceType.HasValue && lastDataSourceType.Value == currentDataSourceType && dataSourceCalculator.LastReadedEventDate.HasValue &&
                        //    baseDate == dataSourceCalculator.LastReadedEventDate.Value) //Основной источник, совпали даты
                        //{
                        //    //Повторение даты одного и того же источника. Такого быть не должно на всякий пропускаем записи
                        //    continue;
                        //}

                        //Здесь надо выставить приоритет на текущий месяц
                        if (monthYear != prevMonthNumber) //Смена месяца, или самое начало
                        {
                            DBDataSourceToTiTp dataSourceToTiTp = null;
                            var isCurrentMonthExistsManualSet =
                                dataSourceTiTpByMonthNumber != null &&
                                dataSourceTiTpByMonthNumber.TryGetValue(monthYear, out dataSourceToTiTp) &&
                                dataSourceToTiTp != null;
                            //Определяем приоритетность данного источника
                            var dataSourceIdToType =
                                dataSourceIdToTypeDict.FirstOrDefault(ds =>
                                    ds.Value == dataSourceCalculator.DataSourceType);
                            SetPriorityDataSource(isCurrentMonthExistsManualSet,
                                dataSourceCalculator.ValidateDataSource,
                                totalPriorityByMonthNumber,
                                monthYear, dataSourceIdToType.Key, dataSourceToTiTp);
                        }

                        #region Вычисляем индексы получасовок

                        var startChannelStatus = dr[scsIndx] as DateTime?; //Дата/время начала действия текущего канала
                        var finishChannelStatus =
                            dr[fcsIndx] as DateTime?; //Дата/время окончания действия текущего канала
                        var nextDate = baseDate.AddDays(1);

                        //Индекс первой получасовки, зависит от начального времени запроса и времени действия канала
                        if (startChannelStatus.HasValue && startChannelStatus.Value > baseDate)
                        {
                            //Есть время действия канала
                            indxFirstChannelValue = (int) (startChannelStatus.Value.TimeOfDay.TotalMinutes / 30) + firstIndex;
                            
                        }
                        else
                        {
                            //Просто проверяем что это не первые сутки
                            if (baseDate > dTStartServer.Date)
                            {
                                firstNeededValue = firstIndex;
                                indxFirstChannelValue = firstIndex;
                            }
                        }

                        if (finishChannelStatus.HasValue && finishChannelStatus.Value < nextDate)
                        {
                            indxLastChannelValue = (int) (finishChannelStatus.Value.TimeOfDay.TotalMinutes / 30) + firstIndex;
                        }
                        else
                        {
                            //Это следующая по порядку дата в БД 
                            if (baseDate == dTEndServer.Date)
                            {
                                indxLastChannelValue = (int) (dTEndServer.TimeOfDay.TotalMinutes / 30) + firstIndex;
                            }
                            else
                            {
                                indxLastChannelValue = 47 + firstIndex;
                            }
                        }

                        #endregion

                        #region Проверка на пропуск записей м/у предыдущим прочитанным значением по данному источнику

                        {
                            exceptedHalfhoursCalculator.VerifyExcetedValuesAndAdd(dataSourceCalculator, baseDateClient,
                                indxFirstChannelValue, firstNeededValue);
                        }

                        #endregion

                        if (isCa)
                        {
                            calculatedPu.CoeffTransformation = DBValues.DBDouble(dr, coeffIndx);
                        }
                        else
                        {
                            //Определяемся с коэффициентом трансформации
                            coeffTranformationWorker.GoNextPeriodIfNotTotal(baseDate, nextDate);
                            coeffTransformationDisabledWorker.GoNextPeriodIfNotTotal(baseDate, nextDate);
                            coeffLossesWorker.GoNextPeriodIfNotTotal(baseDate, nextDate);
                        }

                        var wsMovedInd = 0;
                        for (int i = indxFirstChannelValue; i <= indxLastChannelValue; i++)
                        {
                            var globaHhIndex = dataSourceCalculator.HalfHourIndex;

                            //Есть канал измерения
                            var isOvModeEnabled = false;
                            //---Проверка для обходного выключателя (берем время когда он замещал)
                            if (isOv)
                            {
                                //В этот момент замещали какую то точку
                                isOvModeEnabled = dataSourceCalculator.IsExistsCurrentOvInterval();
                            }

                            dataSourceCalculator.IncHalfHourNumber();

                            //if (ChannelIsAbsent) continue;
                                                        
                            var hhIndx = i - firstIndex;
                            var isNotWorkedHalfHour = isExistsNotWorkedPeriod && notWorkedRange.Any(r =>
                                                          dataSourceCalculator.IsHalfHourNumberBetweenIndexes(r.Item1,
                                                              r.Item2 + 1));

                            #region Обработка отмены зимнего времени

                            double? cal, val;
                            bool isWs;
                            if (Proryv.Servers.Calculation.DBAccess.Common.Ext.Extention.WsDateTime.HasValue &&
                                baseDate == Proryv.Servers.Calculation.DBAccess.Common.Ext.Extention.WsDateTime.Value
                                    .Date && wsMovedInd < 2 && hhIndx == 4)
                            {
                                //Здесь чтение таблиц c дополнительным часом отмены зимнего времени
                                var wsArchive = wsByTiChArchives != null
                                    ? wsByTiChArchives.FirstOrDefault(ws =>
                                        ws.DataSourceType == dataSourceCalculator.DataSourceType)
                                    : null;
                                if (wsArchive == null)
                                {
                                    //Нет записи по данной ТИ в переходной таблице
                                    cal = null;
                                    val = null;
                                }
                                else
                                {
                                    val = wsArchive.Vals[wsMovedInd + 2];
                                    cal = isReadCalculatedValues ? wsArchive.Cals[wsMovedInd + 2] : val;
                                }

                                isWs = true;
                                wsMovedInd++;
                                i--;
                            }
                            else
                            {
                                isWs = false;
                                cal = dr[i] as double?;
                                val = isReadCalculatedValues ? dr[i - deltaCalculatedVsMainColumns] as double? : null;
                            }

                            #endregion

                            double? currValue;
                            bool haveCalcValue; //Признак наличия ручного ввода, есть что-то в поле CAL

                            bool isExistsMainHalfHoursValues = calculatedPu.IsExistsMainHalfHoursValues;
                            GetAndValidateValue(isExistsNotWorkedPeriod, isNotWorkedHalfHour, isReadCalculatedValues,
                                ref isExistsMainHalfHoursValues,
                                cal, val, out currValue, out currFlag, out haveCalcValue);

                            if (haveCalcValue && calculatedPu.ManualEnteredHalfHourIndexes!=null)
                            {
                                //Помечаем индексы с ручным вводом (в дальнейшем эти получасовки не дорасчитываем по формулам, т.к. ручной ввод более приоритетный)
                                calculatedPu.ManualEnteredHalfHourIndexes.Add(globaHhIndex);
                            }

                            calculatedPu.IsExistsMainHalfHoursValues = isExistsMainHalfHoursValues;

                            //Признак ручного изменения статуса (достоверности или получасовок)
                            //Используем только при обработке ручного изменения достоверности
                            var haveManualStatus = (manualStatus >> hhIndx & 1) == 1;

                            if (isWs) currFlag |= VALUES_FLAG_DB.IsWsHour;

                            IPeriodBase<double> currentCoeff = null;
                            IPeriodBase<double> currentLossesCoeff = null;

                            //Определяем достоверность и значение
                            if (!currValue.HasValue) //Если значения нету
                            {
                                currValue = 0;
                                currFlag |= VALUES_FLAG_DB.DataNotFull;
                            }
                            else
                            {
                                if (tiType == enumTIType.ArchCalc_30_Month)
                                {
                                    currFlag |= VALUES_FLAG_DB.IsSmallTi; //это малая ТИ
                                }
                                else
                                {
                                    currFlag |= currentDataSourceType.ToValidateBitField(); //обрабатываем тип источника
                                }

                                if (currValue < 0)
                                {
                                    currValue = 0;
                                    if (isReadCalculatedValues && !haveCalcValue && haveManualStatus)
                                    {
                                        //Проверяем признак ручного ввода достоверности данных
                                        var manualValidStatus = dr[manualValidStatusIndx] as long?;
                                        if (!manualValidStatus.HasValue || (manualValidStatus.Value >> hhIndx & 1) == 1)
                                        {
                                            currFlag |= VALUES_FLAG_DB.DataNotComplete;
                                        }
                                        else
                                        {
                                            currFlag |= VALUES_FLAG_DB.ManualCorrect;
                                        }
                                    }
                                    else
                                    {
                                        currFlag |= VALUES_FLAG_DB.DataNotComplete;
                                    }
                                }
                                else
                                {
                                    if (!isCa)
                                    {
                                        #region Коэфф. трансформации

                                        IPeriodBase<bool> isDisabledPeriod;
                                        var haveDisabledPeriod =
                                            coeffTransformationDisabledWorker.TryGetCurrentPeriodOrFindForHalfhour(
                                                hhIndx,
                                                out isDisabledPeriod);

                                        if ((!haveDisabledPeriod && !isCoeffTransformationDisabledByDefault)
                                            || (haveDisabledPeriod && !isDisabledPeriod.PeriodValue.Value)
                                        ) //Если нет заблокированного периода, или период разрешен
                                        {
                                            //Не нашли один коэфф для данного часа или нет общего коэфф, значит ищем по каждой получасовке
                                            if (coeffTranformationWorker.TryGetCurrentPeriodOrFindForHalfhour(hhIndx,
                                                    out currentCoeff) && currentCoeff.PeriodValue.HasValue)
                                            {
                                                currValue = Math.Round(currValue.Value * currentCoeff.PeriodValue.Value, 8);
                                            }
                                        }

                                        #endregion

                                        #region Коэфф. потерь

                                        if (useLossesCoeff)
                                        {
                                            
                                            if (coeffLossesWorker.TryGetCurrentPeriodOrFindForHalfhour(hhIndx,
                                                    out currentLossesCoeff)
                                                && currentLossesCoeff.PeriodValue.HasValue)
                                            {
                                                currValue = currValue.Value * currentLossesCoeff.PeriodValue.Value;
                                            }
                                        }

                                        #endregion
                                    }

                                    var haveValidStatus = (validStatus >> hhIndx & 1) == 0;
                                    var isCurrhHManualSetValid = false;

                                    //Проверяем признак ручного изменения статуса достоверности
                                    if (isReadCalculatedValues && (haveManualStatus || haveCalcValue))
                                    {
                                        var manualValidStatus = dr[manualValidStatusIndx] as long?;
                                        if (manualValidStatus.HasValue)
                                        {
                                            isCurrhHManualSetValid = true;

                                            //Было ручное изменение статуса достоверности
                                            haveValidStatus = (manualValidStatus.Value >> hhIndx & 1) == 0;

                                            if (haveValidStatus)
                                            {
                                                currFlag |= VALUES_FLAG_DB.ManualCorrect;
                                            }
                                            else
                                            {
                                                currFlag |= VALUES_FLAG_DB.ManualNotCorrect;
                                            }
                                        }
                                    }

                                    if (!haveValidStatus)
                                    {
                                        if (!isCurrhHManualSetValid) currFlag |= VALUES_FLAG_DB.NotCorrect;
                                    }
                                    else if (!calculatedPu.IsExistsMainHalfHoursValues && !isReadCalculatedValues &&
                                             !isNotWorkedHalfHour)
                                    {
                                        calculatedPu.IsExistsMainHalfHoursValues = true;
                                    }
                                }
                            }

                            #region Акт недоучета, обработка ручного ввода КА

                            if (!isCa && (!neededDataSourceType.HasValue || neededDataSourceType.Value != 0))
                            {
                                //Прибавляем по акту недоучета
                                if (isActUndercountExists)
                                {
                                    EnumActMode actMode;
                                    var actSumm = actUndercountValues.CalculateUndercountValues(globaHhIndex, out actMode, currentCoeff, currentLossesCoeff);
                                    //actUndercountValues.FirstOrDefault(v => dataSourceCalculator.IsHalfHourNumberBetweenIndexes(v.StartIndex, v.FinishIndex + 1));
                                    if (actSumm.HasValue)
                                    {
                                        if (actMode == EnumActMode.Замещение)
                                        {
                                            currValue = Math.Round(actSumm.Value, 8); 
                                            currFlag &= ~(VALUES_FLAG_DB.DataNotFull | VALUES_FLAG_DB.NotCorrect |
                                                          VALUES_FLAG_DB.DataNotComplete);
                                        }
                                        else
                                        {
                                            currValue = currValue.Value + actSumm.Value;
                                            if (currValue < 0) currValue = 0.0;
                                        }

                                        currFlag |= VALUES_FLAG_DB.ActUndercountExists;
                                    }
                                }

                                if (!dr.IsDBNull(contrReplaceStatusIndx) &&
                                    ((long) dr[contrReplaceStatusIndx] >> hhIndx & 1) == 1)
                                {
                                    currFlag = currFlag | VALUES_FLAG_DB.ЗамещеныКонтрагентом;
                                }
                            }
                            else if (isCa)
                            {
                                if (haveManualStatus) currFlag = currFlag | VALUES_FLAG_DB.РучнойВвод;
                            }

                            #endregion

                            #region Обсчитываем основной источник

                            if (!isOv //Если это не ОВ
                                || (!ovMode.CheckFlag(enumOVMode.IsOVIntervalWithoutTI_Disabled) &&
                                    !ovMode.CheckFlag(enumOVMode.IsOVIntervalWithTI_Disabled))
                                //Если берем все значения по ОВ
                                || (isOvModeEnabled && !ovMode.CheckFlag(enumOVMode.IsOVIntervalWithTI_Disabled))
                                //Если ОВ замещает на данной получасовке и разрешено при этом брать значение
                                || (!isOvModeEnabled && !ovMode.CheckFlag(enumOVMode.IsOVIntervalWithoutTI_Disabled))
                                //Если ОВ не замещает на данной получасовке и разрешено при этом брать значение
                            )
                            {
                                if (isMakePowerInformation)
                                {
                                    //Считаем, но пишем флаг состояния 
                                    //if ((!currFlag.HasFlag(VALUES_FLAG_DB.DataNotFull) && !currFlag.HasFlag(VALUES_FLAG_DB.DataNotComplete)) || (currValue.HasValue && currValue.Value > 0))
                                    //{
                                    dataSourceCalculator.CalculatePowerValue(baseDate, firstIndex, i,
                                        currValue.GetValueOrDefault(), isHh, timeZoneId, currFlag);
                                    //}
                                }
                                else
                                {
                                    dataSourceCalculator.CalculateEnergyValue(currValue, currFlag, isOnlyIfOvReplacedOtherTi);
                                }
                            }

                            byte day = 0;
                            if (calculateMinAndMax)
                            {
                                day = (byte) baseDateClient.AddMinutes(hhIndx * 30).Day;
                            }

                            //Накапливаем получасовки в итоговом списке
                            if (!dataSourceCalculator.AccamulateCalculatedValues(VALUES_FLAG_DB.None,
                                monthYear,
                                isNotWorkedHalfHour, previousDayDispatchDateTime, day)) break;

                            #endregion
                        }

                        dataSourceCalculator.LastReadedEventDate =
                            baseDate.AddMinutes((indxLastChannelValue - firstIndex + 1) * 30);
                        prevMonthNumber = currMonthNumber;
                    }
                }

                #endregion

                calculatedPu.CoeffTransformation = coeffTranformationWorker.GetCurrentCoeff() ?? 1;

                #region Проверка на пропуск записей м/у предыдущими прочитанными значениями
                {
                    foreach (var calculator in dataSourceCalculatorsDict.Values)
                    {

                        exceptedHalfhoursCalculator.VerifyExcetedValuesAndAdd(calculator,
                            dTEndServer.ServerToClient(timeZoneId).AddMinutes(30),
                            0, 0);
                    }
                }

                #endregion

                if (dataSourceCalculatorsDict.Count > 0)
                {
                    int step = 0;

                    //Перебор расчетных периодов, выявляем наиболее приоритетный по описанию и по достоверности данных
                    foreach (var mY in monthYearList)
                    {
                        //Перепроверяем все возможные источники на наличие прочитанных данных
                        TValidateDataSourceCalculator primaryDataSource = null;
                        //Источник с максимальным приоритетом
                        TValidateDataSourceCalculator maxPriorityDataSource = null;
                        //Первый найденный источник, где есть хоть какие то данные
                        TValidateDataSourceCalculator partialyExistsDataSource = null;
                        bool isMaxPriorityDataSourceEmpty = false;

                        #region Анализ всех источников и выбор приоритетного по достоверности и по приоритетности

                        foreach (var calculator in dataSourceCalculatorsDict.Values
                            .OrderByDescending(d => d.ValidateDataSource[mY].IsPrimary)
                            .ThenByDescending(d => d.ValidateDataSource[mY].Priority))
                        {
                            var validateDataSource = calculator.ValidateDataSource[mY];

                            if (maxPriorityDataSource == null)
                            {
                                maxPriorityDataSource = calculator;
                                isMaxPriorityDataSourceEmpty = !validateDataSource.IsExistsValidHalfHour;
                            }

                            if (primaryDataSource == null) //Приоритетный источник еще не выбран, необходимо выбрать
                            {
                                if (neededDataSourceType.HasValue)
                                {
                                    //Есть источник, который явно затребован
                                    if (calculator.DataSourceType == neededDataSourceType)
                                    {
                                        //Затребованный источник, только по нему и считаем
                                        primaryDataSource = calculator;
                                    }
                                }
                                else if (primaryDataSource == null &&
                                         (((validateDataSource.Flag & VALUES_FLAG_DB.AllAlarmStatuses) ==
                                           VALUES_FLAG_DB.None &&
                                           !validateDataSource.IsAllNotWorkPeriod.GetValueOrDefault()) ||
                                          validateDataSource.IsManualySetPrimary))
                                {
                                    //Здесь выбор наиболее приоритетного источника, сначала смотрим на приоритет, затем на достоверность
                                    primaryDataSource = calculator;
                                    validateDataSource.IsPrimary = true;
                                }
                            }

                            if (partialyExistsDataSource == null &&
                                (validateDataSource.IsExistsValidHalfHour ||
                                 validateDataSource.Flag.HasFlag(VALUES_FLAG_DB.HasValue)))
                            {
                                partialyExistsDataSource = calculator; //частично есть данные
                            }

                            var latestDispatchDateTime = calculatedPu.LatestDispatchDateTime;
                            //Накапливание информации для анализа других источников
                            flagByOtherDataSourceList.AccamulateValidateFlag(validateDataSource,
                                ref latestDispatchDateTime);
                            calculatedPu.LatestDispatchDateTime = latestDispatchDateTime;
                        }

                        if (primaryDataSource == null && !neededDataSourceType.HasValue) //Не нашли приоритетный источник в данном месяце, берем с максимальным приоритетом 
                        {
                            if (!isMaxPriorityDataSourceEmpty || partialyExistsDataSource == null)
                            {
                                primaryDataSource = maxPriorityDataSource;
                            }
                            else
                            {
                                //Если источник с максимальным приоритетом пустой, выбираем ближайший по приоритету источник с данными
                                primaryDataSource = partialyExistsDataSource;
                            }

                        }

                        #endregion

                        //Пишем в итог
                        if (primaryDataSource != null)
                        {
                            var vS = primaryDataSource.ValidateDataSource[mY];
                            calculatedPu.TotalFlag = vS.Flag;
                            result.AddRange(vS.ValuesList);

                            primaryDataSource.FlagMaxAndMin(mY);

                            if (isReturnPreviousDispatchDateTime)
                            {
                                foreach (var previousDayDispatchDateTime in vS.PreviousDayDispatchDateTimeHistory)
                                {
                                    previousDayDispatchDateTimeHistory[previousDayDispatchDateTime.Key] =
                                        previousDayDispatchDateTime.Value;
                                }
                            }

                            calculatedPu.DataSourceType = primaryDataSource.DataSourceType;

                            if (isMakePowerInformation)
                            {
                                powerValue.SelectMaxAndMinPower(primaryDataSource.powerValue, step, mY);
                            }
                        }

                        step++;
                    }
                }

                //Очищаем
                foreach (var calculator in dataSourceCalculatorsDict.Values)
                {
                    calculator.Dispose();
                }

            }
            catch (Exception ex)
            {
                lock (errors)
                {
                    errors.AppendException(ex);
                }
            }

            #region Контроль потребления по типовому графику

            //Всегда уверены в том, что считаем с самого начала суток и четко до последней получасовки
            //Контролируем всегда только часовки или получасовки
            if (isConsumptionScheduleAnalyse && consumptionSchedule != null && (discreteType == enumTimeDiscreteType.DBHalfHours || discreteType == enumTimeDiscreteType.DBHours))
            {
                var numberValuesInDay = discreteType == enumTimeDiscreteType.DBHalfHours ? 48 : 24;
                int numberHh = 0;
                double daySumm = 0;
                int stepOfConsumptionSchedule = 0;
                var isScheduleAnalysComplete = false;
                var daysValues = new Queue<TVALUES_DB>();
                foreach (var tvaluesDB in result)
                {
                    daysValues.Enqueue(tvaluesDB);

                    daySumm += tvaluesDB.F_VALUE;

                    if (++numberHh == numberValuesInDay)
                    {
                        //Есть сумма по нашему графику
                        double specificWeightConsumptionSchedule;
                        if (consumptionSchedule.dayWeights.TryGetValue(stepOfConsumptionSchedule, out specificWeightConsumptionSchedule) && specificWeightConsumptionSchedule != 0)
                        {
                            //Пересчитываем типовой график согласно потреблению точки за сутки
                            foreach (var csVal in consumptionSchedule.ConsumptionScheduleValues.Where(c => c.TotalDay == stepOfConsumptionSchedule))
                            {
                                if (csVal.F_VALUE.HasValue)
                                {
                                    csVal.F_VALUE = csVal.F_VALUE * daySumm / specificWeightConsumptionSchedule;
                                    csVal.MAX_VALUE = csVal.MAX_VALUE * daySumm / specificWeightConsumptionSchedule;
                                    csVal.MIN_VALUE = csVal.MIN_VALUE * daySumm / specificWeightConsumptionSchedule;
                                }

                                //Проверяем коридор
                                var val = daysValues.Dequeue();
                                if (val != null && csVal.F_VALUE.HasValue)
                                {
                                    if (val.F_VALUE > csVal.MAX_VALUE || val.F_VALUE < csVal.MIN_VALUE)
                                    {
                                        val.F_FLAG = val.F_FLAG | VALUES_FLAG_DB.ConsumptionScheduleOverflow;
                                    }
                                }
                            }

                            if (!isScheduleAnalysComplete)
                            {
                                isScheduleAnalysComplete = daySumm != 0;
                            }
                        }
                        else
                        {
                            daysValues.Clear();
                        }

                        numberHh = 0;
                        daySumm = 0;
                        stepOfConsumptionSchedule++;
                    }

                }

                if (isConsumptionScheduleAnalyse && consumptionSchedule != null)
                {
                    consumptionSchedule.IsAnalysComplete = isScheduleAnalysComplete;
                }

            }

            #endregion

            if (result.Count > 0)
            {
                calculatedPu.Val_List = result;
                return;
            }

            //Не набрали данные по какой либо причине, надо наполнить пустотой
            var mask = neededDataSourceType.ToValidateBitField();
            //if (isReadCalculatedValues)
            //{
            //    mask |= VALUES_FLAG_DB.РучнойВвод;
            //}

            var totalFlag = calculatedPu.TotalFlag;
            exceptedHalfhoursCalculator.FillVoidHalfHours(result, discreteType, mask, ref totalFlag);
            calculatedPu.TotalFlag = totalFlag;

            mask |= VALUES_FLAG_DB.DataNotFull;
            if (flagByOtherDataSourceList != null)
            {
                flagByOtherDataSourceList.AddRange(DBStatic.AllDataSource
                    .Select(dataSourceType => new TValidateFlagDataSource(dataSourceType)
                    {
                        Flag = mask,
                    }));
            }

            calculatedPu.Val_List = result;
        }

        public static void SetPriorityDataSource(bool isCurrentMonthExistsManualSet, Dictionary<ushort, TValidateFlagDataSource> validateDataSourceDict,
            Dictionary<int, List<DBPriorityList>> priorityByMonthNumberDict, ushort currMonthNumber, int dataSource_id, DBDataSourceToTiTp dataSourceToTiTp)
        {
            TValidateFlagDataSource validateDataSource;
            if (!validateDataSourceDict.TryGetValue(currMonthNumber, out validateDataSource)) return;

            //Этот источник задан приоритетным вручную
            if (isCurrentMonthExistsManualSet)
            {
                validateDataSource.IsPrimary = validateDataSource.IsManualySetPrimary = dataSourceToTiTp.DataSource_ID == dataSource_id;
            }
            else
            {
                List<DBPriorityList> currMonthPriorityList;
                //В этом месяце, для этой связки ТИ - ТП не выставлен источник вручную, необходимо приоритетный источник определить по общей таблице
                if (priorityByMonthNumberDict != null && priorityByMonthNumberDict.TryGetValue(currMonthNumber, out currMonthPriorityList) && currMonthPriorityList != null &&
                    currMonthPriorityList.Count > 0)
                {
                    var currSource = currMonthPriorityList.First(p => p.DataSource_ID == dataSource_id);
                    if (currSource != null)
                    {
                        validateDataSource.Priority = (byte)currSource.Priority;
                    }
                }
                //else
                //{
                //    //Не наполнена таблица общих приоритетов, на всякий помечаем этот источник приоритетным, т.к. он первый в базе
                //    validateDataSource.IsPrimary = false;
                //}
            }

            //validateDataSourceDict[currMonthNumber] = validateDataSource;
        }

        private static EnumDataSourceType? FindFirstPriorityDataSource(int currMonthNumber, Dictionary<int, DBDataSourceToTiTp> dataSourceTiTpByMonthNumber,
            Dictionary<int, List<DBPriorityList>> priorityByMonthNumberDict,
            Dictionary<int, EnumDataSourceType> dataSourceIdToTypeDict)
        {
            EnumDataSourceType? dataSourceType = null;

            DBDataSourceToTiTp dataSourceToTiTp = null;
            int? dataSource_id = null;

            //Этот источник задан приоритетным вручную
            if (dataSourceTiTpByMonthNumber != null && dataSourceTiTpByMonthNumber.TryGetValue(currMonthNumber, out dataSourceToTiTp) && dataSourceToTiTp != null)
            {
                dataSource_id = dataSourceToTiTp.DataSource_ID;
            }
            else
            {
                List<DBPriorityList> currMonthPriorityList;
                //В этом месяце, для этой связки ТИ - ТП не выставлен источник вручную, необходимо приоритетный истоник определить по общей таблице
                if (priorityByMonthNumberDict != null && priorityByMonthNumberDict.TryGetValue(currMonthNumber, out currMonthPriorityList) && currMonthPriorityList != null &&
                    currMonthPriorityList.Count > 0)
                {
                    var maxMonthPriority = currMonthPriorityList.First(); //Первый истоник имеет наивысший приоритет 
                    dataSource_id = maxMonthPriority.DataSource_ID;
                }
            }

            if (dataSource_id.HasValue)
            {
                EnumDataSourceType ds;
                if (dataSourceIdToTypeDict.TryGetValue(dataSource_id.Value, out ds))
                {
                    dataSourceType = ds;
                }
            }

            return dataSourceType;
        }

        private static void GetAndValidateValue(bool isExistsNotWorkedPeriod, bool isNotWorkedHalfHour, bool isReadCalculatedValues, ref bool isExistsMainHalfHoursValues,
            double? cal, double? val, out double? currValue, out VALUES_FLAG_DB currFlag, out bool haveCalcValue)
        {
            haveCalcValue = false;

            //Определяемся работала ТИ в данной получасовке
            if (isExistsNotWorkedPeriod && isNotWorkedHalfHour)
            {
                //Это диапазон когда ТИ была отключена
                currValue = 0;
                currFlag = VALUES_FLAG_DB.IsNotWorkedPeriod;
                return;
            }

            currValue = cal;
            if (cal.HasValue || val.HasValue)
            {
                currFlag = VALUES_FLAG_DB.HasValue;
            }
            else
            {
                currFlag = VALUES_FLAG_DB.None;
            }

            if (!isReadCalculatedValues) return;

            var mainValue = val;
            if (!currValue.HasValue)
            {
                //Если ручной ввод и данные отсутствуют, пытаемся читаем основные данные (для автосбора)
                currValue = mainValue;
            }
            else
            {
                if (currValue.Value < 0)
                {
                    currValue = mainValue;
                    currFlag |= VALUES_FLAG_DB.CalculatedValuesCancel;
                }
                else
                {
                    currFlag |= VALUES_FLAG_DB.РучнойВвод;
                    haveCalcValue = true;
                }
            }

            //Проверка наличия основного профиля
            if (!mainValue.HasValue)
            {
                if (haveCalcValue) currFlag |= VALUES_FLAG_DB.MainProfileDataNotFull;
            }
            else if (!isExistsMainHalfHoursValues && mainValue.Value >= 0)
            {
                isExistsMainHalfHoursValues = true;
            }
        }
    }
}
