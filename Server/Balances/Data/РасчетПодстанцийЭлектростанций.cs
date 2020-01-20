using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.DB;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Internal.Utils;
using Proryv.AskueARM2.Server.DBAccess.Public.Utils;
using Proryv.AskueARM2.Server.WCF;
using Proryv.Servers.Calculation.DBAccess.Common;
using Proryv.Servers.Calculation.DBAccess.Enums.Internals;
using Proryv.Servers.Calculation.DBAccess.Interface;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances.Data
{
    public partial class BalanceFreeHierarchyCalculator
    {
        private void РасчетПодстанцийЭлектростанций(BalanceFreeHierarchyCalculatedResult balanceResult)
        {
            var calculatedByDiscretePeriods = balanceResult.CalculatedByDiscretePeriods;

            #region Обсчитываем трансформаторы

            foreach (var transformator in balanceResult.Transformators)
            {
                transformator.CalculateLosses(_archiveHalfhours.ResultCommonForTransformatorsAndReactors,
                    _numbersHalfHours, null, _errors, _dtStart, _dtEnd, _unitDigit);
                transformator.CalculateLossesByDiscretePeriod(enumTimeDiscreteType.DBHalfHours, _dtStart, _dtEnd,
                    _intervalTimeList);
            }

            #endregion

            #region Обсчитываем реакторы

            foreach (var reactor in balanceResult.Reactors)
            {
                reactor.CalculateLosses(_archiveHalfhours.ResultCommonForTransformatorsAndReactors, 0.5, null);
            }

            #endregion

            var ovInfos = new List<IOV_Values>(); //Информация по ОВ которую нужно дополнить дополнительно
            var ovIdsForIntegral = new List<TI_ChanelType>(); //Идентификаторы ОВ для дозапросов и дорасчетов интегралов по ним

            foreach (var paramPair in balanceResult.ItemsParamsBySection)
            {
                foreach (var itemParam in paramPair.Value)
                {
                    _archiveHalfhours.PopulateArchivesByHierarchyType(itemParam, _tiForRequestAdditionalInfo,
                        _discreteType, //В данном документе работаем только с получасовками
                        true, balanceResult.Transformators, balanceResult.Reactors, _numbersHalfHours);

                    if (itemParam.OV_Values_List != null && itemParam.OV_Values_List.Count > 0)
                    {
                        ovInfos.AddRange(itemParam.OV_Values_List);
                    }
                }
            }

            //GetOvInfos(ovInfos);

            List<Dict_Balance_FreeHierarchy_Section> sections;
            SectionsByType.TryGetValue(balanceResult.BalanceFreeHierarchyType, out sections);
            if (sections == null) return;

            var dopustNebalnsUn = sections.First(s => Equals(s.MetaString1, "dopust_nebalns"))
                .BalanceFreeHierarchySection_UN;

            var hlv = balanceResult.HighLimitValue;
            var llv = balanceResult.LowerLimitValue;
            var hlp = balanceResult.HighLimit;
            var llp = balanceResult.LowerLimit;

            var balanceResultInfo = balanceResult.ResultInfo;

            //Для обсчетов итогов в заданном периоде дискретизации
            var discretePeriodStepOfReadHalfHours = 0;
            var discretePeriodNumbersHalfHoursInOurPeriod = _intervalTimeList.FirstOrDefault();

            var ourPeriodCounter = 0;

            var sectionValues = sections.ToDictionary(k => k.BalanceFreeHierarchySection_UN, v => 0.0);

            var discretePeriodOtpusk = 0.0;
            var discretePeriodPostupilo = 0.0;
            var discretePeriodFlag = VALUES_FLAG_DB.None;
            var totalFlag = VALUES_FLAG_DB.None;
            var totalBalanceStatus = EnumBalanceStatus.None;
            var totalOtpusk = 0.0;
            var totalPostupilo = 0.0;

            var factUnBalancePercentAverage = 0.0;
            double? factUnBalancePercentMax = null;
            double? factUnBalancePercentMin = null;
            var factUnBalancePercentMaxIndx = 0.0;
            var factUnBalancePercentMinIndx = 0.0;

            for (var archiveIndex = 0; archiveIndex < _numbersHalfHours; archiveIndex++)
            {
                //Обсчитываем всё в рамках одной получасовки
                var o = 0.0;
                var p = 0.0;
                var flag = VALUES_FLAG_DB.None;

                #region Считаем сумму по подгруппам участвующим в балансе

                foreach (var paramPair in balanceResult.ItemsParamsBySection)
                {
                    var sectionUn = paramPair.Key;
                    var halfhourSum = 0.0;
                    var integralSum = 0.0;

                    foreach (var itemParam in paramPair.Value)
                    {
                        if (!itemParam.IsInput && !itemParam.IsOutput || itemParam.HalfHours == null)
                            continue; //Объект не участвует в балансе

                        if (archiveIndex == 0)
                        {
                            integralSum = _archiveHalfhours.GetIntegral(itemParam);
                        }

                        var aVal = itemParam.HalfHours.ElementAtOrDefault(archiveIndex);

                        if (aVal == null || !itemParam.Coef.HasValue || itemParam.Coef.Value == 0)
                        {
                            continue;
                        }

                        var v = aVal.F_VALUE * itemParam.Coef.Value;
                        halfhourSum += v; //Сумма по подгруппе

                        if (itemParam.IsInput) p += v;
                        else if (itemParam.IsOutput) o += v;

                        flag = flag.CompareAndReturnMostBadStatus(aVal.F_FLAG);

                        itemParam.F_FLAG = itemParam.F_FLAG.CompareAndReturnMostBadStatus(aVal.F_FLAG);

                    }

                    sectionValues[sectionUn] += halfhourSum;
                }

                #endregion

                var halfHourBalanceStatus = EnumBalanceStatus.None;
                double halfHourFactUnbalancePercent;
                double halfHourFactUnbalanceValues;
                double? halfHourResolvedUnbalancePercent;

                #region Смотрим превышение

                CalculateBalanceInfo(balanceResult.ItemsParamsBySection.Values, p, o, archiveIndex, 1,
                    out halfHourFactUnbalancePercent, out halfHourFactUnbalanceValues,
                    out halfHourResolvedUnbalancePercent);

                if (halfHourResolvedUnbalancePercent.HasValue)
                {
                    sectionValues[dopustNebalnsUn] += halfHourResolvedUnbalancePercent.Value;
                }

                if (hlv.HasValue && halfHourFactUnbalanceValues > hlv.Value)
                {
                    halfHourBalanceStatus |= EnumBalanceStatus.ExcessHiValueLimit;
                }
                else if (llv.HasValue && halfHourFactUnbalanceValues < llv.Value)
                {
                    halfHourBalanceStatus |= EnumBalanceStatus.ExcessLoValueLimit;
                }

                if (hlp.HasValue && halfHourFactUnbalancePercent > hlp.Value)
                {
                    halfHourBalanceStatus |= EnumBalanceStatus.ExcessHiPercentLimit;
                }
                else if (llp.HasValue && halfHourFactUnbalancePercent < llp.Value)
                {
                    halfHourBalanceStatus |= EnumBalanceStatus.ExcessLoPercentLimit;
                }

                if (Math.Abs(halfHourFactUnbalancePercent) - halfHourResolvedUnbalancePercent > 0)
                {
                    halfHourBalanceStatus |= EnumBalanceStatus.Unbalance;
                    //    balanceResult.FactUnbalanceInfo.UnbalanceDates.Add(timeList.ElementAtOrDefault(archiveIndex));
                }

                #endregion

                //Информация для графика
                balanceResult.BalanceHalfhours.Add(new TBalanceHalfhour
                {
                    BalanceStatus = halfHourBalanceStatus,
                    FactUnBalancePercent = halfHourFactUnbalancePercent,
                    FactUnBalanceValue = halfHourFactUnbalanceValues,
                    ResolvedUnBalancePercent = halfHourResolvedUnbalancePercent.GetValueOrDefault(),
                    Flag = flag,
                });

                #region Накапливаем состояния для заданного периода дискретизации

                discretePeriodPostupilo += p;
                discretePeriodOtpusk += o;
                discretePeriodFlag = discretePeriodFlag.CompareAndReturnMostBadStatus(flag);
                totalBalanceStatus |= halfHourBalanceStatus;

                #endregion

                #region Итоговые состояния

                factUnBalancePercentAverage = (halfHourFactUnbalancePercent + factUnBalancePercentAverage * archiveIndex) / (archiveIndex + 1);

                if (!factUnBalancePercentMax.HasValue || factUnBalancePercentMax < halfHourFactUnbalancePercent)
                {
                    factUnBalancePercentMax = halfHourFactUnbalancePercent;
                    factUnBalancePercentMaxIndx = archiveIndex;
                }

                if (!factUnBalancePercentMin.HasValue || factUnBalancePercentMin > halfHourFactUnbalancePercent)
                {
                    factUnBalancePercentMin = halfHourFactUnbalancePercent;
                    factUnBalancePercentMinIndx = archiveIndex;
                }

                #endregion

                #region Накапливаем результат в нужном нам периоде дискретизации

                if (discretePeriodStepOfReadHalfHours == discretePeriodNumbersHalfHoursInOurPeriod) //Закрываем нужный нам период дискретизвции
                {
                    double discretePeriodFactUnbalancePercent;
                    double discretePeriodFactUnbalanceValues;
                    double? discretePeriodResolvedUnbalancePercent;

                    if (_discreteType == enumTimeDiscreteType.DBHalfHours)
                    {
                        //Нужны получасовки, ничего дорасчитывать не нужно
                        discretePeriodFactUnbalancePercent = halfHourFactUnbalancePercent;
                        discretePeriodFactUnbalanceValues = halfHourFactUnbalanceValues;
                        discretePeriodResolvedUnbalancePercent = halfHourResolvedUnbalancePercent.GetValueOrDefault();
                    }
                    else
                    {
                        //Подитоги нужно дорасчитать только в нужном периоде дискретизации
                        CalculateBalanceInfo(balanceResult.ItemsParamsBySection.Values, discretePeriodPostupilo,
                            discretePeriodOtpusk, archiveIndex, discretePeriodNumbersHalfHoursInOurPeriod + 1,
                            out discretePeriodFactUnbalancePercent, out discretePeriodFactUnbalanceValues,
                            out discretePeriodResolvedUnbalancePercent);
                    }

                    //Поитог по периоду дискретизации, для Excel и таблицы
                    calculatedByDiscretePeriods.Add(new BalanceFreeHierarchyCalculatedDiscretePeriod
                    {
                        SectionValues = sectionValues,

                        //Фактический небаланс ((I-(II+III)-IV-V - Потери автортансформаторов)/I)*100%
                        FactUnbalancePercent = discretePeriodFactUnbalancePercent,

                        //Небаланс, кВт*ч
                        FactUnbalanceValue = discretePeriodFactUnbalanceValues,

                        //Допустимый небаланс (VII), %
                        ResolvedUnBalanceValues = discretePeriodResolvedUnbalancePercent.GetValueOrDefault(),

                        //Отпуск и расход + трансформаторы и реакторы
                        OutValues = discretePeriodOtpusk,

                        //Поступило на шины, всего (I)
                        InBusValues = discretePeriodPostupilo,

                        F_FLAG = discretePeriodFlag,
                    });

                    //Накапливаем для общего итога
                    totalOtpusk += discretePeriodOtpusk;
                    totalPostupilo += discretePeriodPostupilo;
                    totalFlag = discretePeriodFlag.CompareAndReturnMostBadStatus(discretePeriodFlag);

                    if (++ourPeriodCounter < _intervalTimeList.Count)
                    {
                        //Сбрасываем накопленные состояния для накопления в последующем периоде (час, сутки или месяц)
                        discretePeriodNumbersHalfHoursInOurPeriod = _intervalTimeList[ourPeriodCounter];
                        discretePeriodFlag = VALUES_FLAG_DB.None;
                        discretePeriodStepOfReadHalfHours = 0;
                        discretePeriodOtpusk = 0.0;
                        discretePeriodPostupilo = 0.0;
                        sectionValues = sections.ToDictionary(k => k.BalanceFreeHierarchySection_UN, v => 0.0);
                        //totalBalanceStatus = EnumBalanceStatus.None;
                    }
                    else
                    {
                        break;
                    }

                }
                else
                {
                    discretePeriodStepOfReadHalfHours++; //Дальше считаем получасовки до нужного нам количества
                }

                #endregion

            } //Перебираем получасовки

            #region Общий итог 

            balanceResultInfo.FactUnBalancePercentAverage = factUnBalancePercentAverage;
            balanceResultInfo.FactUnBalancePercentMax = factUnBalancePercentMax;
            balanceResultInfo.FactUnBalancePercentMin = factUnBalancePercentMin;

            if (factUnBalancePercentMax.HasValue)
            {
                balanceResultInfo.FactUnBalancePercentMaxDt = _dtServerStart.ServerToUtc()
                    .AddMinutes(factUnBalancePercentMaxIndx * 30).UtcToClient(_timeZoneId);
            }

            if (factUnBalancePercentMin.HasValue)
            {
                balanceResultInfo.FactUnBalancePercentMinDt = _dtServerStart.ServerToUtc()
                    .AddMinutes(factUnBalancePercentMinIndx * 30).UtcToClient(_timeZoneId);
            }
            
            double totalFactUnbalancePercent;
            double totalFactUnbalanceValue;
            double? totalResolvedUnbalancePercent;

            if (_discreteType != enumTimeDiscreteType.DBInterval || calculatedByDiscretePeriods.Count == 0)
            {
                CalculateBalanceInfo(balanceResult.ItemsParamsBySection.Values, totalPostupilo, totalOtpusk,
                    _numbersHalfHours - 1, _numbersHalfHours,
                    out totalFactUnbalancePercent, out totalFactUnbalanceValue, out totalResolvedUnbalancePercent);
            }
            else
            {
                //Для интервального общий итог не нужно пересчитывать, уже посчитали
                var cv = calculatedByDiscretePeriods.First();
                totalFactUnbalancePercent = cv.FactUnbalancePercent;
                totalFactUnbalanceValue = cv.FactUnbalanceValue;
                totalResolvedUnbalancePercent = cv.ResolvedUnBalanceValues;
            }

            balanceResultInfo.FactUnBalancePercent = totalFactUnbalancePercent;
            balanceResultInfo.FactUnBalanceValue = totalFactUnbalanceValue;
            balanceResultInfo.ResolvedUnBalancePercent = totalResolvedUnbalancePercent;
            balanceResultInfo.BalanceStatus = totalBalanceStatus & (~EnumBalanceStatus.Unbalance); //Статус небаланса берем не из получасовок
            balanceResultInfo.TotalTiFlag = totalFlag;


            #endregion

            if (Math.Abs(balanceResultInfo.FactUnBalancePercent.GetValueOrDefault()) -
                balanceResultInfo.ResolvedUnBalancePercent > 0)
            {
                balanceResultInfo.BalanceStatus |= EnumBalanceStatus.Unbalance;
            }
        }

        /// <summary>
        /// Обсчитываем факт. небаланс и допустимый небаланс всех объектов по указанным индексам получасовок
        /// </summary>
        /// <param name="balanceItems">Объекты в балансе</param>
        /// <param name="postupilo">Поступление, относительно которого обсчитываем</param>
        /// <param name="otpusk">Отдача, относительно которой обсчитываем</param>
        /// <param name="archiveIndex">Индекс последней получасовки которую берем для обсчета</param>
        /// <param name="numbersHalfHoursInOurPeriod">Количество получасовок которые нужно учесть от последней получасовки</param>
        /// <param name="factUnbalancePercent">Результат обсчета фак. небаланс в %</param>
        /// <param name="factUnbalanceValues">Результат обсчета фак. небаланс в абс. величинах (Вт, кВт, МВт и т.д.)</param>
        /// <param name="resolvedUnbalancePercent">Допустимый небаланс в %</param>
        private void CalculateBalanceInfo(IEnumerable<List<BalanceFreeHierarchyItemParams>> balanceItems, double postupilo, double otpusk, int archiveIndex, int numbersHalfHoursInOurPeriod, 
            out double factUnbalancePercent, out double factUnbalanceValues, out double? resolvedUnbalancePercent)
        {
            //фактический небаланс в абсолютных цифрах
            var totalFactUnbalanceValue = postupilo - otpusk;
            if (postupilo > 0)
            {
                //фактический небаланс в %
                factUnbalancePercent = totalFactUnbalanceValue / postupilo * 100;
            }
            else if (otpusk > 0)
            {
                //фактический небаланс в %
                factUnbalancePercent = -100;
            }
            else
            {
                //фактический небаланс в %
                factUnbalancePercent = 0;
            }

            factUnbalanceValues = Math.Abs(totalFactUnbalanceValue);

            double? totalResolvedUnbalance = null;
            var numbersBack = archiveIndex - numbersHalfHoursInOurPeriod;

            //Это обсчет всех получасовок, нужно сохранить процентную долю и небаланс в самой ТИ
            var isSaveResolvedUnbalanceInItem = (archiveIndex == _numbersHalfHours - 1) &&
                                                    (numbersHalfHoursInOurPeriod == _numbersHalfHours);

            //Обсчитываем допустимый небаланс по итоговым цифрам (?возможно усреднять по получасовкам?)
            foreach (var itemParams in balanceItems)
            {
                foreach (var itemParam in itemParams)
                {
                    if ((!itemParam.IsInput && !itemParam.IsOutput) ||
                        itemParam.HalfHours == null) continue; //Объект не участвует в балансе или отутствуют получасовки

                    double itemPartPercent; //Доля эл эн данного объекта в балансе в %
                    double resolvedUnbalanceTi;

                    if ((itemParam.IsInput && postupilo > 0) || (!itemParam.IsInput && otpusk > 0))
                    {
                        var coeff = itemParam.Coef ?? 1;
                        var sum = 0.0;
                        for (int indx = archiveIndex; indx > numbersBack; indx--)
                        {
                            var hVal = itemParam.HalfHours.ElementAtOrDefault(indx);
                            if (hVal != null)
                            {
                                sum += hVal.F_VALUE;
                            }
                        }

                        if (itemParam.IsInput)
                        {
                            //Процентная доля данной ТИ в общем значении по поступлению
                            itemPartPercent = ((sum * coeff) / postupilo);
                        }
                        else
                        {
                            //Процентная доля данной ТИ в общем значении по отдаче
                            itemPartPercent = (sum * coeff) / otpusk;
                        }

                        resolvedUnbalanceTi = itemPartPercent * itemParam.MeasuringComplexError * itemPartPercent * itemParam.MeasuringComplexError;
                    }
                    else
                    {
                        //Не можем посчитать
                        itemPartPercent = 0.0;
                        resolvedUnbalanceTi = 0.0;
                    }

                    if (!totalResolvedUnbalance.HasValue) totalResolvedUnbalance = 0.0;

                    if (isSaveResolvedUnbalanceInItem)
                    {
                        itemParam.PartPercent = itemPartPercent;
                        itemParam.UnBalanceTi = resolvedUnbalanceTi;
                    }

                    //Общий небаланс
                    totalResolvedUnbalance += resolvedUnbalanceTi;
                }
            }

            if (totalResolvedUnbalance.HasValue)
            {
                resolvedUnbalancePercent = Math.Sqrt(Math.Abs(totalResolvedUnbalance.Value));
            }
            else
            {
                resolvedUnbalancePercent = null;
            }
        }
    }
}
