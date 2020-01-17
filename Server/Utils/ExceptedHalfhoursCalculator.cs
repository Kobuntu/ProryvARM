using Proryv.AskueARM2.Server.DBAccess.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.WCF;
using Proryv.AskueARM2.Server.DBAccess.Public.ActUndercount;
using Proryv.Servers.Calculation.DBAccess.Common;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Utils.Data
{
    /// <summary>
    /// Калькулятор для затыкания дыр отсутствующих в базе значений
    /// </summary>
    public class ExceptedHalfhoursCalculator
    {
        private readonly DateTime _dTStartServer;
        private readonly string _timeZoneId;
        private readonly Dictionary<int, DBDataSourceToTiTp> _dataSourceTiTpByMonthNumber;
        private readonly Dictionary<int, EnumDataSourceType> _dataSourceIdToTypeDict;
        private readonly Dictionary<int, List<DBPriorityList>> _totalPriorityByMonthNumber;
        private readonly bool _isExistsNotWorkedPeriod;
        private readonly List<Tuple<int, int?>> _notWorkedRange;
        private readonly List<int> _intervalTimeList;
        private readonly double _unitDigit;

        private bool _isActUndercountExists;
        private List<TActUndercountPeriod> _actUndercountValues;

        private readonly bool _calculateCoeffs;
        private readonly PeriodCoeffWorker<double> _coeffTranformationWorker;
        private readonly PeriodCoeffWorker<double> _coeffLossesWorker;
        private readonly PeriodCoeffWorker<bool> _coeffTransformationDisabledWorker;
        private readonly bool _useLossesCoeff;
        private readonly bool _isCoeffTransformationDisabledByDefault;

        public ExceptedHalfhoursCalculator(
            DateTime dTStartServer,
            string timeZoneId, 
            Dictionary<int, DBDataSourceToTiTp> dataSourceTiTpByMonthNumber,
            Dictionary<int, EnumDataSourceType> dataSourceIdToTypeDict,
            Dictionary<int, List<DBPriorityList>> totalPriorityByMonthNumber,
            bool isExistsNotWorkedPeriod, 
            List<Tuple<int, int?>> notWorkedRange, 
            List<int> intervalTimeList,
            EnumUnitDigit unitDigit
            , PeriodCoeffWorker<double> coeffTranformationWorker = null
            , PeriodCoeffWorker<double> coeffLossesWorker = null
            , PeriodCoeffWorker<bool> coeffTransformationDisabledWorker = null
            , bool useLossesCoeff = false
            , bool isCoeffTransformationDisabledByDefault = false)
        {
            _dTStartServer = dTStartServer;
            _timeZoneId = timeZoneId;
            _dataSourceTiTpByMonthNumber = dataSourceTiTpByMonthNumber;
            _dataSourceIdToTypeDict = dataSourceIdToTypeDict;
            _totalPriorityByMonthNumber = totalPriorityByMonthNumber;
            _isExistsNotWorkedPeriod = isExistsNotWorkedPeriod;
            _notWorkedRange = notWorkedRange;
            _unitDigit = (double) unitDigit;
            _intervalTimeList = intervalTimeList;

            _coeffTranformationWorker = coeffTranformationWorker;
            _coeffLossesWorker = coeffLossesWorker;
            _coeffTransformationDisabledWorker = coeffTransformationDisabledWorker;
            _useLossesCoeff = useLossesCoeff;
            _isCoeffTransformationDisabledByDefault = isCoeffTransformationDisabledByDefault;

            _calculateCoeffs = _coeffTranformationWorker != null && _coeffLossesWorker != null && _coeffTransformationDisabledWorker != null;
        }

        public void SetActUndercount(bool isActUndercountExists,
            List<TActUndercountPeriod> actUndercountValues)
        {
            _isActUndercountExists = isActUndercountExists;
            _actUndercountValues = actUndercountValues;
        }


        /// <summary>
        /// Проверяем, есть ли пропуск м/у предыдущим чтением данных, если есть наполняем данными с признакм отсутствия
        /// </summary>
        public void VerifyExcetedValuesAndAdd(
            TValidateDataSourceCalculator dataSourceCalculator,
            DateTime baseDateClient,
            int indxFirstValue,
            int firstNumber
            )
        {
            var dateFromCompare = (dataSourceCalculator.LastReadedEventDate ?? _dTStartServer).ServerToClient(_timeZoneId);
            var halfHoursNumbers =
                (baseDateClient.AddMinutes((indxFirstValue - firstNumber) * 30) - dateFromCompare).TotalMinutes / 30;

            if (!(halfHoursNumbers > 0)) return;

            //Обнаружен пропуск с предыдущим чтением, необходимо расчитать пропущенные значения
            int prevDay = -1;
            int i = 0;
            ushort currMonthNumber = 0;
            for (var hhIndex = 0; hhIndex < halfHoursNumbers; hhIndex++)
            {
                var dtClient = dateFromCompare.AddMinutes(hhIndex * 30);

                if (dtClient.Day != prevDay)
                {
                    currMonthNumber = (ushort)(dtClient.Year * 12 + dtClient.Month);
                    prevDay = dtClient.Day;

                    DBDataSourceToTiTp dataSourceToTiTp = null;
                    //Здесь надо выставить приоритет на текущий месяц
                    var isCurrentMonthExistsManualSet = _dataSourceTiTpByMonthNumber != null && _dataSourceTiTpByMonthNumber.TryGetValue(currMonthNumber, out dataSourceToTiTp) &&
                                                        dataSourceToTiTp != null;
                    //Определяем приоритетность данного источника
                    var dataSourceIdToType = _dataSourceIdToTypeDict.FirstOrDefault(ds => ds.Value == dataSourceCalculator.DataSourceType);
                    ArchiveValuesFactory.SetPriorityDataSource(isCurrentMonthExistsManualSet, dataSourceCalculator.ValidateDataSource,
                        _totalPriorityByMonthNumber, currMonthNumber, dataSourceIdToType.Key, dataSourceToTiTp);

                }

                dataSourceCalculator.IncHalfHourNumber();
                
                VALUES_FLAG_DB flag;
                var isNotWorkedHalfHour = _isExistsNotWorkedPeriod && _notWorkedRange.Any(r => (dataSourceCalculator.HalfHourIndex >= r.Item1) 
                                                                                               && (!r.Item2.HasValue || dataSourceCalculator.HalfHourIndex <= r.Item2));
                if (isNotWorkedHalfHour)
                {
                    flag = VALUES_FLAG_DB.IsNotWorkedPeriod;
                }
                else
                {
                    flag = VALUES_FLAG_DB.DataNotFull;

                    #region Акт недоучета, обработка ручного ввода КА

                    //Прибавляем по акту недоучета
                    if (_isActUndercountExists)
                    {
                        EnumActMode actMode;
                        var actSumm = _actUndercountValues.CalculateUndercountValues(dataSourceCalculator.HalfHourIndex - 1, out actMode);
                        dataSourceCalculator.CalculateEnergyValue(actSumm, VALUES_FLAG_DB.None, false);
                        if (actSumm.HasValue)
                        {
                            if (actMode == EnumActMode.Замещение)
                            {
                                flag &= ~(VALUES_FLAG_DB.DataNotFull | VALUES_FLAG_DB.NotCorrect | VALUES_FLAG_DB.DataNotComplete);
                            }
                            flag |= VALUES_FLAG_DB.ActUndercountExists;
                        }
                    }

                    #endregion
                }

                //Накапливаем пустые получасовки
                if (!dataSourceCalculator.AccamulateCalculatedValues(flag, currMonthNumber, isNotWorkedHalfHour, null, 0)) break;
                //dtServer = dtServer.AddMinutes(30);
                i++;
            }
        }

        /// <summary>
        /// Наполнение списка архива отсутствующими значений
        /// </summary>
        /// <param name="discreteType">Период дискретизации</param>
        /// <param name="archivesList">Архивы, которые наполняем</param>
        public void FillVoidHalfHours(List<TVALUES_DB> archivesList, enumTimeDiscreteType discreteType, 
            VALUES_FLAG_DB mask, ref VALUES_FLAG_DB totalPrimaryFlag)
        {
            var totalFlag = totalPrimaryFlag;
            var totalHalfhourIndex = 0;

            foreach (var numbersHalfHoursInOurPeriod in _intervalTimeList)
            {
                var periodFlag = VALUES_FLAG_DB.None;
                var periodValue = 0.0;

                for (var hhIndex = 0; hhIndex <= numbersHalfHoursInOurPeriod; hhIndex++)
                {
                    VALUES_FLAG_DB flag;
                    double value;

                    var isNotWorkedHalfHour = _isExistsNotWorkedPeriod && _notWorkedRange.Any(r =>
                                                  (hhIndex >= r.Item1) && (!r.Item2.HasValue || hhIndex <= r.Item2));
                    if (isNotWorkedHalfHour)
                    {
                        flag = VALUES_FLAG_DB.IsNotWorkedPeriod;
                        value = 0;
                    }
                    else
                    {
                        flag = VALUES_FLAG_DB.DataNotFull | mask;

                        #region Акт недоучета, обработка ручного ввода КА

                        //Прибавляем по акту недоучета
                        if (_isActUndercountExists)
                        {
                            IPeriodBase<double> currentCoeff = null;
                            IPeriodBase<double> currentLossesCoeff = null;

                            if (_calculateCoeffs)
                            {
                                #region Коэфф. трансформации

                                IPeriodBase<bool> isDisabledPeriod;
                                var haveDisabledPeriod =
                                    _coeffTransformationDisabledWorker.TryGetCurrentPeriodOrFindForTotalIndexHh(totalHalfhourIndex,
                                        out isDisabledPeriod);

                                if ((!haveDisabledPeriod && !_isCoeffTransformationDisabledByDefault)
                                    || (haveDisabledPeriod && !isDisabledPeriod.PeriodValue.Value)
                                ) //Если нет заблокированного периода, или период разрешен
                                {
                                    //Не нашли один коэфф для данного часа или нет общего коэфф, значит ищем по каждой получасовке
                                    _coeffTranformationWorker.TryGetCurrentPeriodOrFindForTotalIndexHh(totalHalfhourIndex,
                                            out currentCoeff);
                                }

                                #endregion

                                #region Коэфф. потерь

                                if (_useLossesCoeff)
                                {
                                    _coeffLossesWorker.TryGetCurrentPeriodOrFindForHalfhour(totalHalfhourIndex,
                                            out currentLossesCoeff);
                                }

                                #endregion
                            }


                            EnumActMode actMode;
                            var av = _actUndercountValues.CalculateUndercountValues(totalHalfhourIndex, out actMode, currentCoeff, currentLossesCoeff);
                            if (av.HasValue)
                            {
                                value = Math.Round(av.Value, 8) / _unitDigit;

                                if (actMode == EnumActMode.Замещение)
                                {
                                    flag &= ~(VALUES_FLAG_DB.DataNotFull | VALUES_FLAG_DB.NotCorrect |
                                              VALUES_FLAG_DB.DataNotComplete);
                                }

                                flag |= VALUES_FLAG_DB.ActUndercountExists;

                                
                            }
                            else
                            {
                                value = 0;
                            }
                        }
                        else
                        {
                            value = 0;
                        }

                        #endregion
                    }

                    periodValue += value;
                    periodFlag |= flag;
                    totalHalfhourIndex++;
                }

                totalFlag |= periodFlag;
                archivesList.Add(new Servers.Calculation.DBAccess.Interface.Data.TVALUES_DB(periodFlag, periodValue));
            }

            totalPrimaryFlag = totalFlag;
        }
    }
}
