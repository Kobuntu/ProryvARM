using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.DB;
using Proryv.AskueARM2.Server.DBAccess.Internal.DB.Forecast.Data;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Archives;
using Proryv.AskueARM2.Server.DBAccess.Public.Forecasting.Data;
using Proryv.AskueARM2.Server.DBAccess.Public.UA.Data;
using Proryv.AskueARM2.Server.DBAccess.Public.Utils;
using Proryv.AskueARM2.Server.WCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess.Internal.Utils;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.DBAccess.Common;
using Proryv.Servers.Calculation.DBAccess.Common.Data;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Forecasting
{
    public static class ForecastObjectFactory
    {
        /// <summary>
        /// Берем первую попавшуюся модель прогнозирования для объекта
        /// </summary>
        /// <param name="forecastObjectUn"></param>
        /// <returns></returns>
        public static List<TForecastCalculateParams> GetForecastObjectParams(HashSet<string> forecastObjectUns, DateTime dtStart, DateTime dtEnd,
            int? forecastModelUserSelected, string timeZoneId, StringBuilder errors, EnumUnitDigit unitDigit, enumTimeDiscreteType forecastDiscreteType, bool isReadCalculatedValues)
        {
            var tis = new List<TI_ChanelType>();
            var formulas = new List<TFormulaParam>();
            var tps = new List<TP_ChanelType>();
            var nodes = new List<long>();

            var result = new Dictionary<string, TForecastCalculateParams>();

            var periodsPriorityByForecastObject = new Dictionary<string, Dictionary<string, Queue<PeriodHierarchy>>>();

            if (forecastObjectUns == null) return result.Values.ToList();

            #region Читаем параметры объектов прогнозирования из базы

            var forecastObjectUnsString = string.Join(",", forecastObjectUns);

            try
            {
                using (var db = new LightDBDataContext(Settings.DbConnectionString)
                {
                    ObjectTrackingEnabled = false, //Указываем что это у нас ReadOnly 
                    DeferredLoadingEnabled = false,
                    CommandTimeout = 180,
                })
                {
                    var multiResult = db.usp2_ForecastObjectParams(forecastObjectUnsString, dtStart.ClientToServer(timeZoneId), dtEnd.ClientToServer(timeZoneId));

                    //Читаем доступные модели, которым будем считать
                    foreach (var modelsGroupByObject in multiResult.GetResult<usp2_ForecastObjectParamsResult1>().ToList().GroupBy(g => g.ForecastObject_UN))
                    {
                        result[modelsGroupByObject.Key] = new TForecastCalculateParams(modelsGroupByObject.Key, forecastModelUserSelected,
                            new HashSet<int>(modelsGroupByObject.Where(m => m.ForecastCalculateModel_ID.HasValue).OrderBy(m => m.Priority ?? 1000).Select(m => (int)(m.ForecastCalculateModel_ID ?? 0))));
                    }

                    #region Читаем ТИ, ТП формулы и т.д. для обсчета

                    foreach (var inputParamByObject in multiResult.GetResult<usp2_ForecastObjectParamsResult2>().ToList().GroupBy(g => g.ForecastObject_UN))
                    {
                        TForecastCalculateParams calculateParams;
                        if (!result.TryGetValue(inputParamByObject.Key, out calculateParams) || calculateParams == null) continue;

                        calculateParams.DtStart = dtStart;
                        calculateParams.DtEnd = dtEnd;
                        calculateParams.DiscreteType = forecastDiscreteType;

                        var periodsByInputParam = new Dictionary<string, Queue<PeriodHierarchy>>();
                        periodsPriorityByForecastObject[inputParamByObject.Key] = periodsByInputParam;

                        foreach (var inputParam in inputParamByObject.GroupBy(g => g.ForecastInputParam_UN))
                        {
                            var periods = new Queue<PeriodHierarchy>();
                            periodsByInputParam[inputParam.Key] = periods;

                            var archiveByInputParam = new ForecastByInputParamArchives(inputParam.Key);
                            calculateParams.ArchivesByInputParam.Add(archiveByInputParam);
                            var isFirst = true;
                            foreach (var physicalParam in inputParam)
                            {
                                bool isFullPeriod;
                                var period = BuildHalfHoursPeriods(physicalParam, dtStart, dtEnd, timeZoneId, out isFullPeriod);

                                if (isFirst)
                                {
                                    archiveByInputParam.MeasureQuantityType_UN = physicalParam.MeasureQuantityType_UN;
                                    isFirst = false;
                                }

                                //Формула
                                if (!string.IsNullOrEmpty(physicalParam.Formula_UN))
                                {
                                    var formulaParam = new TFormulaParam
                                    {
                                        FormulaID = physicalParam.Formula_UN,
                                        FormulasTable = enumFormulasTable.Info_Formula_Description,
                                        IsFormulaHasCorrectDescription = true,
                                    };
                                    formulas.Add(formulaParam);
                                    //archiveByInputParam.Formulas.Add(formulaParam);
                                    periods.Enqueue(new PeriodHierarchy
                                    {
                                        Id = new ID_Hierarchy_Channel(enumTypeHierarchy.Formula, physicalParam.Formula_UN, 0),
                                        IsFullPeriod = isFullPeriod,
                                        IndxStart = period.Item1,
                                        IndxFinish = period.Item2,
                                    });

                                    if (isFullPeriod) continue; //Объект действует на весь период, остальные объекты пропускаем
                                }

                                //OPC пока не используется, т.к. не понятно как приводить к получасовкам
                                if (physicalParam.UANode_ID.HasValue)
                                {
                                    //archiveByInputParam.Nodes.Add(physicalParam.UANode_ID.Value);
                                    nodes.Add(physicalParam.UANode_ID.Value);

                                    periods.Enqueue(new PeriodHierarchy
                                    {
                                        Id = new ID_Hierarchy_Channel(enumTypeHierarchy.UANode, physicalParam.UANode_ID.Value.ToString(), 0),
                                        IsFullPeriod = isFullPeriod,
                                        IndxStart = period.Item1,
                                        IndxFinish = period.Item2,
                                    });

                                    if (isFullPeriod) continue; //Объект действует на весь период, остальные объекты пропускаем
                                }

                                //Для остальных нужен канал, если не указан пропускаем
                                //if (!inputParam.ChannelType.HasValue) continue;

                                //ТИ
                                if (physicalParam.TI_ID.HasValue)
                                {
                                    if (!physicalParam.ChannelType.HasValue)
                                    {
                                        errors.Append("Для ТИ {").Append(physicalParam.TI_ID).Append("} нужно указать канал");
                                        continue;
                                    }

                                    var tiId = new TI_ChanelType
                                    {
                                        ChannelType = physicalParam.ChannelType.Value,
                                        TI_ID = physicalParam.TI_ID.Value,
                                        MsTimeZone = timeZoneId, //Это возможно не нужно
                                    };
                                    //archiveByInputParam.TiIds.Add(tiId);
                                    tis.Add(tiId);
                                    periods.Enqueue(new PeriodHierarchy
                                    {
                                        Id = new ID_Hierarchy_Channel(enumTypeHierarchy.Info_TI, physicalParam.TI_ID.Value.ToString(), physicalParam.ChannelType.Value),
                                        IsFullPeriod = isFullPeriod,
                                        IndxStart = period.Item1,
                                        IndxFinish = period.Item2,
                                    });

                                    if (isFullPeriod) continue; //Объект действует на весь период, остальные объекты пропускаем
                                }

                                //ТП
                                if (physicalParam.TP_ID.HasValue)
                                {
                                    if (!physicalParam.ChannelType.HasValue)
                                    {
                                        errors.Append("Для ТП {").Append(physicalParam.TP_ID).Append("} нужно указать канал");
                                        continue;
                                    }

                                    var tpId = new TP_ChanelType
                                    {
                                        ChannelType = physicalParam.ChannelType.Value,
                                        TP_ID = physicalParam.TP_ID.Value,
                                    };
                                    //archiveByInputParam.TpIds.Add(tpId);
                                    tps.Add(tpId);
                                    periods.Enqueue(new PeriodHierarchy
                                    {
                                        Id = new ID_Hierarchy_Channel(enumTypeHierarchy.Info_TP, physicalParam.TP_ID.Value.ToString(), physicalParam.ChannelType.Value),
                                        IsFullPeriod = isFullPeriod,
                                        IndxStart = period.Item1,
                                        IndxFinish = period.Item2,
                                    });
                                }
                            }
                        }
                    }

                    #endregion
                }

            }
            catch (Exception ex)
            {
                errors.Append(ex.Message);
            }

            #endregion

            #region Читаем архивы

            //Пока запрашиваем только получасовки
            var resultCommonTi = new ArchivesValues_List2(tis, dtStart, dtEnd, 
                true, false, enumTimeDiscreteType.DBHalfHours, enumTypeInformation.Energy, false,
               unitDigit, enumOVMode.NormalMode, timeZoneId,  isReadCalculatedValues: isReadCalculatedValues);

            if (resultCommonTi.Errors != null)
            {
                errors.Append(resultCommonTi.Errors);
            }

            //Запрос значений всех ТП
            var resultCommonTp = new ArchivesTPValues(tps, dtStart, dtEnd, enumBusRelation.PPI_OurSide, enumTimeDiscreteType.DBHalfHours, null, enumTypeInformation.Energy, unitDigit,
                timeZoneId, false, isSupressTpNotFound: true, isReadCalculatedValues: isReadCalculatedValues);
            if (resultCommonTp.Errors != null)
            {
                errors.Append(resultCommonTp.Errors);
            }

            //Все формулы
            var resultCommonFormula = new FormulasResult(formulas, dtStart, dtEnd, enumTimeDiscreteType.DBHalfHours, null, 0, enumTypeInformation.Energy, unitDigit, timeZoneId, //Для описанных здесь формул ед. изм. не учитываем
                true, false, false, false, isReadCalculatedValues: isReadCalculatedValues, ovMode: enumOVMode.NormalMode); //Для формулы сумму с ОВ считаем всегда!!!
            if (resultCommonFormula.Errors != null)
            {
                errors.Append(resultCommonFormula.Errors);
            }

            //Чтение архивов OPC
            UADataReader resultCommonUAData = null;
            try
            {
                resultCommonUAData = new UADataReader(nodes, dtStart, dtEnd, false, false, false, timeZoneId);
            }
            catch
            { }

            #endregion

            #region Подготовка архивов

            var numberHalfHoursForForecasting = MyListConverters.GetNumbersValuesInPeriod(enumTimeDiscreteType.DBHalfHours, dtStart, dtEnd, timeZoneId);

            result.Values.AsParallel().ForAll(calculateParam => //AsParallel().ForAll()
            {
                //new TVALUES_DB[numberHalfHoursForForecasting].Select(v => new TVALUES_DB(VALUES_FLAG_DB.IsNull, 0)).ToList();

                Dictionary<string, Queue<PeriodHierarchy>> periodsByInputParam;
                if (!periodsPriorityByForecastObject.TryGetValue(calculateParam.ForecastObject_UN, out periodsByInputParam) || periodsByInputParam.Count == 0) return; //Нет параметров для обсчета

                foreach (var paramArchives in calculateParam.ArchivesByInputParam)
                {
                    Queue<PeriodHierarchy> periods;
                    if (!periodsByInputParam.TryGetValue(paramArchives.ForecastInputParamUn, out periods) || periods.Count == 0) continue; //Нет параметров для обсчета

                    //var period = periods.Dequeue(); //Первый объект в интервале
                    PeriodHierarchy period = null;
                    var halfHours = new List<TVALUES_DB>();
                    for (var hh = 0; hh < numberHalfHoursForForecasting; hh++)
                    {
                        if (!PeriodInHalfHour(ref period, periods, hh, resultCommonTi, resultCommonFormula,
                                resultCommonTp, resultCommonUAData) || period.Archives == null)
                        {
                            halfHours.Add(new Servers.Calculation.DBAccess.Interface.Data.TVALUES_DB(VALUES_FLAG_DB.IsNull, 0)); //Получасовка не входит в наш диапазон, или нет архива
                            continue;
                        }

                        var aVal = period.Archives.ElementAtOrDefault(hh);
                        if (aVal == null)
                        {
                            aVal = new Servers.Calculation.DBAccess.Interface.Data.TVALUES_DB(VALUES_FLAG_DB.IsNull, 0); //Непонятная ситуация, помечаем как отсутствующие
                        }

                        var cVal = halfHours.ElementAtOrDefault(hh);
                        if (cVal == null)
                        {
                            cVal = new Servers.Calculation.DBAccess.Interface.Data.TVALUES_DB(aVal.F_FLAG, aVal.F_VALUE);
                            halfHours.Add(cVal);
                        }
                        else
                        {
                            cVal.F_VALUE = aVal.F_VALUE;
                            cVal.F_FLAG = aVal.F_FLAG;
                        }
                    }


                    paramArchives.Archives.AddRange(MyListConverters.ConvertHalfHoursToOtherList(forecastDiscreteType, halfHours,
                        dtStart, dtEnd, timeZoneId).Select(v => new Servers.Calculation.DBAccess.Interface.Data.TVALUES_DB(v.F_FLAG, v.F_VALUE)));
                }
            });

            #endregion

            return result.Values.ToList();
        }

        /// <summary>
        /// Проверяем входит ли период действия текущего объекта в указанную получасоку получасовка 
        /// Если не входит, то выбираем следующий объект, который входит
        /// </summary>
        /// <param name="period"></param>
        /// <param name="periods"></param>
        /// <param name="hh"></param>
        /// <param name="resultCommonTi"></param>
        /// <returns></returns>
        private static bool PeriodInHalfHour(ref PeriodHierarchy period, Queue<PeriodHierarchy> periods, int hh,
            ArchivesValues_List2 resultCommonTi, FormulasResult resultCommonFormula, ArchivesTPValues resultCommonTp, UADataReader resultCommonUAData)
        {
            if (period == null)
            {
                if (periods.Count == 0) return false;
                period = periods.Dequeue();
            }

            bool inHalfHour;
            if (period!=null && !period.IsFullPeriod )
            {
                if (period.IndxStart.HasValue && period.IndxStart > hh)
                {
                    //Этоn период еще не начался
                    return false;
                }

                if (period.IndxFinish.HasValue && period.IndxFinish < hh)
                {
                    //У этого периода кончилось действие
                    period = null;
                    inHalfHour = PeriodInHalfHour(ref period, periods, hh, resultCommonTi, resultCommonFormula,
                        resultCommonTp, resultCommonUAData);
                }
                else
                {
                    inHalfHour = true;
                }
            }
            else
            {
                inHalfHour = true;
            }

            if (period == null) return false;

            #region Наполняем архив

            if (period.Archives == null)
            {
                var id = period.Id;
                switch (id.TypeHierarchy)
                {
                    case enumTypeHierarchy.Info_TI:
                        int tiId;
                        List<TArchivesValue> tiValues;
                        if (int.TryParse(id.ID, out tiId) && resultCommonTi.ArchiveValues.TryGetValue(tiId, out tiValues))
                        {
                            var archiveTi = tiValues
                                .FirstOrDefault(v => v.TI_Ch_ID.TI_ID == tiId && v.TI_Ch_ID.ChannelType == id.Channel);
                            if (archiveTi != null)
                            {
                                period.Archives = archiveTi.Val_List;
                            }
                        }
                        break;
                    case enumTypeHierarchy.Formula:
                        var archiveFormula = resultCommonFormula.Result_Values
                            .FirstOrDefault(v => string.Equals(v.Formula_UN, id.ID));
                        if (archiveFormula != null && archiveFormula.Result_Values != null && archiveFormula.Result_Values.Count > 0)
                        {
                            var vals = archiveFormula.Result_Values[0].Val_List;
                            if (vals != null)
                            {
                                period.Archives = vals.OfType<TVALUES_DB>().ToList();
                            }
                        }
                        break;
                    case enumTypeHierarchy.Info_TP:
                        int tpId;
                        if (int.TryParse(id.ID, out tpId))
                        {
                            var archiveTp = resultCommonTp.ArchivesValue30orHour
                                .FirstOrDefault(v => v.TpIdChannel.TP_ID == tpId && v.TpIdChannel.ChannelType == id.Channel);
                            if (archiveTp != null)
                            {
                                //archiveTp.Val_List.TryGetValue(archiveTp.IsMoneyOurSide, out period.Archives);
                                period.Archives = archiveTp.GetValues();
                            }
                        }
                        break;
                    case enumTypeHierarchy.Node:

                        var archiveNode = resultCommonUAData.UADatas
                            .FirstOrDefault(v => string.Equals(v.UANode_ID, id.ID));
                        if (archiveNode != null)
                        {
                            //TODO тут непонятно
                            //archiveTp.Val_List.TryGetValue(archiveTp.IsMoneyOurSide, out period.Archives);
                            //period.Archives = archiveNode.GetValues();
                        }
                        break;
                }
            }

            #endregion

            return inHalfHour;
        }

        public static List<TForecastFreeHierarchyItem> GetForecastObjectsByFreeHierarchyItems(string userId, int freeHierTreeId,
            List<int> freeHierItemsIds, out HashSet<string> forecastObjectUns)
        {
            var idsByHierarchyTypes = new Dictionary<EnumFreeHierarchyItemType, ISet<string>>();

            //Строим деоево и возвращаем только родительские объекты
            var result = DBFreeHierarchyTree.BuildTree<TForecastFreeHierarchyItem>(userId, freeHierTreeId, freeHierItemsIds,
                    EnumObjectRightType.SeeDbObjects,
                    idsByHierarchyTypes,
                    new HashSet<EnumFreeHierarchyItemType> { EnumFreeHierarchyItemType.ForecastObject })
                .Where(fo => fo.Parent == null).ToList();

            ISet<string> localForecastObjectUns;
            if (!idsByHierarchyTypes.TryGetValue(EnumFreeHierarchyItemType.ForecastObject, out localForecastObjectUns) ||
                localForecastObjectUns == null)
            {
                localForecastObjectUns = new HashSet<string>();
            }

            forecastObjectUns = localForecastObjectUns as HashSet<string>;

            return result;
        }

        #region Privates

        /// <summary>
        /// Расчитываем индексы действия параметра в расчетном периоде
        /// </summary>
        /// <returns></returns>
        private static Tuple<int?, int?> BuildHalfHoursPeriods(usp2_ForecastObjectParamsResult2 inputParam, DateTime dtStart, DateTime dtEnd, string timeZoneId,
            out bool isFullPeriod)
        {
            var fsdt = dtStart.ClientToUtc(timeZoneId);

            int? indxStart;
            if (inputParam.StartDateTime.HasValue)
            {
                var sdt = inputParam.StartDateTime.Value.ServerToUtc();
                if (sdt > fsdt)
                {
                    indxStart = (int)(sdt - fsdt).TotalMinutes / 30;
                    isFullPeriod = false;
                }
                else
                {
                    isFullPeriod = true;
                    indxStart = 0;
                }
            }
            else
            {
                indxStart = null;
                isFullPeriod = true;
            }

            int? indxEnd;
            if (inputParam.FinishDateTime.HasValue)
            {
                var fdt = inputParam.FinishDateTime.Value.ServerToUtc();
                var fedt = dtEnd.ClientToUtc(timeZoneId);

                var isEndIndxLess = fdt < fedt;

                if (isEndIndxLess && isFullPeriod) isFullPeriod = false;

                indxEnd = (int)((isEndIndxLess ? fdt : fedt) - fsdt).TotalMinutes / 30;
                if (indxStart.HasValue)
                {
                    indxEnd = indxEnd - indxStart;
                }
            }
            else
            {
                indxEnd = null;
            }

            return new Tuple<int?, int?>(indxStart, indxEnd);
        }

        #endregion

        internal class PeriodHierarchy
        {
            public ID_Hierarchy_Channel Id;
            public int? IndxStart;
            public int? IndxFinish;
            public bool IsFullPeriod;

            public List<TVALUES_DB> Archives;
        }
    }
}
