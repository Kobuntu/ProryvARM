using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.DB;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Public.Utils;
using Proryv.AskueARM2.Server.WCF;
using Proryv.Servers.Calculation.DBAccess.Common.Data;
using Proryv.Servers.Calculation.DBAccess.Interface;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances.Data
{
    /// <summary>
    /// Калькулятор для расчета баланса
    /// </summary>
    public partial class BalanceFreeHierarchyCalculator : IDisposable
    {
        private readonly ConcurrentDictionary<string, BalanceFreeHierarchyCalculatedResult> _itemsResultByBalance;

        /// <summary>
        /// Начальное время по часовому поясу сервера
        /// </summary>
        private readonly DateTime _dtServerStart;

        /// <summary>
        /// Конечное время по чассовому поясу сервера
        /// </summary>
        private readonly DateTime _dtServerEnd;

        /// <summary>
        /// Генерировать файл MemoryStream
        /// </summary>
        private readonly bool _isGenerateDoc;

        /// <summary>
        /// Количесто получасовок
        /// </summary>
        private readonly int _numbersHalfHours;

        /// <summary>
        /// Количество архивных значений
        /// </summary>
        private readonly int _numbersByDiscreteType;
        
        /// <summary>
        /// Ошибки
        /// </summary>
        private readonly StringBuilder _errors;
        /// <summary>
        /// Период дискретизации
        /// </summary>
        private readonly enumTimeDiscreteType _discreteType;
        
        private readonly DateTime _dtStart;
        private readonly DateTime _dtEnd;
        private readonly string _timeZoneId;
        private readonly EnumUnitDigit _unitDigit;
        private readonly EnumUnitDigit _unitDigitIntegrals;

        private readonly List<int> _intervalTimeList;

        //Досбор информации по ТИ
        private readonly ConcurrentDictionary<TI_ChanelType, ConcurrentStack<BalanceFreeHierarchyItemParams>> _tiForRequestAdditionalInfo;

        private readonly TArchiveHalfhours _archiveHalfhours;

        public readonly ConcurrentDictionary<EnumBalanceFreeHierarchyType, List<Dict_Balance_FreeHierarchy_Section>> SectionsByType;

        /// <summary>
        /// Названия разделов балансов
        /// </summary>
        public Dictionary<string, string> SubsectionNames;

        public BalanceFreeHierarchyCalculator(List<string> balanceFreeHierarchyUN, bool isGenerateDoc, StringBuilder errors, enumTimeDiscreteType discreteType, DateTime dtStart, DateTime dtEnd, string timeZoneId,
            ConcurrentDictionary<TI_ChanelType, ConcurrentStack<BalanceFreeHierarchyItemParams>> tiForRequestAdditionalInfo,
            EnumUnitDigit unitDigit, EnumUnitDigit unitDigitIntegrals)
        {
            _errors = errors ?? new StringBuilder();
            _discreteType = discreteType;
            _dtStart = dtStart;
            _dtEnd = dtEnd;
            _timeZoneId = timeZoneId;
            
            _tiForRequestAdditionalInfo = tiForRequestAdditionalInfo;
            _unitDigit = unitDigit;
            _unitDigitIntegrals = unitDigitIntegrals;
            _intervalTimeList = MyListConverters.GetIntervalTimeList(_dtStart, _dtEnd, _discreteType, timeZoneId);
            
            _dtServerStart = dtStart.ClientToServer(timeZoneId);
            _dtServerEnd = dtEnd.ClientToServer(timeZoneId);
            _isGenerateDoc = isGenerateDoc;
            _numbersHalfHours = MyListConverters.GetNumbersValuesInPeriod(enumTimeDiscreteType.DBHalfHours, dtStart, dtEnd, timeZoneId);

            var transIds = new ConcurrentDictionary<int, string>();
            var reactorsIds = new ConcurrentStack<int>();
            var tiForTransformatorsAndReactors = new ConcurrentStack<TI_ChanelType>();

            var tis = new ConcurrentStack<TI_ChanelType>();
            var integralTis = new ConcurrentStack<TI_ChanelType>();
            var tps = new ConcurrentStack<TP_ChanelType>();
            var formulas = new ConcurrentStack<TFormulaParam>();
            var sections = new ConcurrentStack<TSectionChannel>();
            var formulaConstantIds = new ConcurrentStack<string>();
            
            try
            {
                _itemsResultByBalance = ReadBalanceParams(balanceFreeHierarchyUN, transIds, reactorsIds, tis, integralTis, tps, formulas,
                    sections, formulaConstantIds);
            }
            catch (AggregateException aex)
            {
                //критическая ошибка
                throw aex.ToException();
            }

            #region Параметры трансформаторов и реакторов

            try
            {
                ReadTransformatorsParams(transIds, tiForTransformatorsAndReactors, _itemsResultByBalance);
            }
            catch (AggregateException aex)
            {
                //не критическая ошибка
                _errors.Append(aex.ToException());
            }

            #endregion

            _archiveHalfhours = new TArchiveHalfhours(isGenerateDoc, dtStart, dtEnd,
                unitDigit, timeZoneId, unitDigitIntegrals, tiForTransformatorsAndReactors,
                tis, integralTis, tps, formulas, sections, formulaConstantIds, _intervalTimeList);

            _numbersByDiscreteType = MyListConverters.GetNumbersValuesInPeriod(_discreteType, _dtStart, _dtEnd, timeZoneId);

            SectionsByType = new ConcurrentDictionary<EnumBalanceFreeHierarchyType, List<Dict_Balance_FreeHierarchy_Section>>(GetFreeHierarchyBalanceSections(_itemsResultByBalance.Values.Select(v => (byte)v.BalanceFreeHierarchyType).Distinct())
                .GroupBy(g => (EnumBalanceFreeHierarchyType)g.BalanceFreeHierarchyType_ID)
                .ToDictionary(k => k.Key, v => v.ToList()));
        }


        public BalanceFreeHierarchyCalculatedResult Calculate(string balanceFreeHierarchyUn)
        {
            BalanceFreeHierarchyCalculatedResult balanceResult;
            if (string.IsNullOrEmpty(balanceFreeHierarchyUn) || !_itemsResultByBalance.TryGetValue(balanceFreeHierarchyUn, out balanceResult) || balanceResult == null)
                return null;

            switch (balanceResult.BalanceFreeHierarchyType)
            {
                case EnumBalanceFreeHierarchyType.БалансНаПодстанции:
                case EnumBalanceFreeHierarchyType.БалансЭлектростанции:
                case EnumBalanceFreeHierarchyType.СводныйИнтегральныйАкт:
                    РасчетПодстанцийЭлектростанций(balanceResult);
                    return balanceResult;
                case EnumBalanceFreeHierarchyType.АктУчетаЭэ:
                    РасчетАктУчетаЭэ(balanceResult);
                    return balanceResult;
                case EnumBalanceFreeHierarchyType.Приложение51:
                    Приложение51(balanceResult);
                    return balanceResult;
                default:
                    _errors.Append(balanceResult.BalanceFreeHierarchyName + " - имеет неподдерживаемый тип расчета");
                    return null;
            }
        }

        public static List<Dict_Balance_FreeHierarchy_Section> GetFreeHierarchyBalanceSections(IEnumerable<byte> balanceFreeHierarchyTypes)
        {
            using (var db = new FSKDataContext(Settings.DbConnectionString)
            {
                ObjectTrackingEnabled = false,
                DeferredLoadingEnabled = false,
                CommandTimeout = 60,
            })
            {
                using (var txn = new TransactionScope(TransactionScopeOption.Required,
                    new TransactionOptions {IsolationLevel = IsolationLevel.ReadUncommitted}))
                {
                    return db.Dict_Balance_FreeHierarchy_Sections
                        .Where(b => balanceFreeHierarchyTypes.Contains(b.BalanceFreeHierarchyType_ID))
                        .ToList();
                }
            }
        }


        /// <summary>
        /// Читаем параметры балансов
        /// </summary>
        /// <param name="transIds"></param>
        /// <param name="reactorsIds"></param>
        /// <param name="tis"></param>
        /// <param name="intgralTis"></param>
        /// <param name="tps"></param>
        /// <param name="formulas"></param>
        /// <param name="sections"></param>
        /// <param name="formulaConstantIds"></param>
        /// <returns></returns>
        private ConcurrentDictionary<string, BalanceFreeHierarchyCalculatedResult> ReadBalanceParams(
            List<string> balanceFreeHierarchyUN,
            ConcurrentDictionary<int, string> transIds, ConcurrentStack<int> reactorsIds,
            ConcurrentStack<TI_ChanelType> tis, ConcurrentStack<TI_ChanelType> intgralTis,
            ConcurrentStack<TP_ChanelType> tps, ConcurrentStack<TFormulaParam> formulas,
            ConcurrentStack<TSectionChannel> sections, ConcurrentStack<string> formulaConstantIds)
        {
            var resultByBalance = new ConcurrentDictionary<string, BalanceFreeHierarchyCalculatedResult>();
            var subsections = new ConcurrentStack<Dict_Balance_FreeHierarchy_Subsection>();
            using (var db = new FSKDataContext(Settings.DbConnectionString)
            {
                ObjectTrackingEnabled = false,
                DeferredLoadingEnabled = false,
                CommandTimeout = 180,
            })
            {
                foreach (var range in Partitioner.Create(0, balanceFreeHierarchyUN.Count, Settings.MaxStringRows)
                    .GetDynamicPartitions())
                {
                    var balanceFreeHierarchyUNs = balanceFreeHierarchyUN.GetRange(range.Item1, range.Item2 - range.Item1);

                    var sprocBalanceParams = db.usp2_Info_GetBalanceFreeHierarchyParams(
                        string.Join(",", balanceFreeHierarchyUNs), _dtServerStart, _dtServerEnd, _isGenerateDoc);

                    var r = new Dictionary<string, BalanceFreeHierarchyCalculatedResult>();
                    foreach (var balanceParam in sprocBalanceParams.GetResult<Info_Balance_FreeHierarchy_List>()
                        .Select(bl => new BalanceFreeHierarchyCalculatedResult(bl)))
                    {
                        r.Add(balanceParam.BalanceFreeHierarchyUn, balanceParam);
                        resultByBalance.TryAdd(balanceParam.BalanceFreeHierarchyUn, balanceParam);
                    }

                    foreach (var bto in sprocBalanceParams.GetResult<BalanceFreeHierarchyToObject>())
                    {
                        BalanceFreeHierarchyCalculatedResult hierarchyCalculatedResult;
                        if (r.TryGetValue(bto.BalanceFreeHierarchy_UN, out hierarchyCalculatedResult))
                        {
                            hierarchyCalculatedResult.ObjectId = bto.ToIdTypeHierarchy();
                        }
                    }

                    if (_isGenerateDoc)
                    {
                        //Заголовки подгрупп балансов
                        var s = sprocBalanceParams.GetResult<Dict_Balance_FreeHierarchy_Subsection>().ToArray();
                        if (s.Length > 0)
                        {
                            subsections.PushRange(s);
                        }
                    }

                    foreach (var bp in sprocBalanceParams.GetResult<usp2_Info_GetBalanceFreeHierarchyParamsResult>())
                    {
                        BalanceFreeHierarchyCalculatedResult balanceResult;
                        if (!r.TryGetValue(bp.BalanceFreeHierarchy_UN, out balanceResult)) continue;

                        var itemParam = new BalanceFreeHierarchyItemParams
                        {
                            ChannelType = bp.ChannelType,
                            BalanceFreeHierarchySectionUn = bp.BalanceFreeHierarchySection_UN,
                            BalanceFreeHierarchySubsectionUn = bp.BalanceFreeHierarchySubsection_UN,
                            BalanceFreeHierarchySubsectionUn2 = bp.BalanceFreeHierarchySubsection2_UN,
                            BalanceFreeHierarchySubsectionUn3 = bp.BalanceFreeHierarchySubsection3_UN,
                            IsOtpuskShin = !string.IsNullOrEmpty(bp.MetaString2)
                                           && bp.MetaString2.IndexOf("otpusk_shin", 0, StringComparison.Ordinal) >= 0,

                            IsInput = !string.IsNullOrEmpty(bp.MetaString1)
                                      && bp.MetaString1.IndexOf("postupilo", 0, StringComparison.Ordinal) >= 0,

                            IsOutput = !string.IsNullOrEmpty(bp.MetaString1)
                                       && bp.MetaString1.IndexOf("rashod", 0, StringComparison.Ordinal) >= 0,
                            Coef = bp.Coef,
                            SortNumber = bp.SortNumber,
                            Name = bp.Name,
                            MeasuringComplexError = bp.MeasuringComplexError,
                            CoeffTransformation = bp.CoeffTransformation,
                            CoeffLosses = bp.CoeffLosses,
                            MeterSerialNumber = bp.MeterSerialNumber,
                            Voltage = bp.Voltage
                        };

                        List<BalanceFreeHierarchyItemParams> items;
                        if (!balanceResult.ItemsParamsBySection.TryGetValue(itemParam.BalanceFreeHierarchySectionUn,
                            out items))
                        {
                            items = new List<BalanceFreeHierarchyItemParams>();
                            balanceResult.ItemsParamsBySection[itemParam.BalanceFreeHierarchySectionUn] = items;
                        }

                        items.Add(itemParam);

                        if (bp.TI_ID.HasValue)
                        {
                            tis.Push(new TI_ChanelType
                            {
                                TI_ID = bp.TI_ID.Value,
                                ChannelType = bp.ChannelType ?? 1,
                                DataSourceType = (EnumDataSourceType?)bp.DataSource_ID,
                                //ClosedPeriod_ID =  Пока по закрытым периодам не формируем
                            });
                            itemParam.Id = bp.TI_ID.Value;
                            itemParam.TypeHierarchy = enumTypeHierarchy.Info_TI;
                        }
                        else if (bp.IntegralTI_ID.HasValue)
                        {
                            intgralTis.Push(new TI_ChanelType
                            {
                                TI_ID = bp.IntegralTI_ID.Value,
                                ChannelType = bp.ChannelType ?? 1,
                                DataSourceType = (EnumDataSourceType?)bp.DataSource_ID,
                                //ClosedPeriod_ID =  Пока по закрытым периодам не формируем
                            });
                            itemParam.Id = bp.IntegralTI_ID.Value;
                            itemParam.TypeHierarchy = enumTypeHierarchy.Info_Integral;
                        }
                        else if (bp.TP_ID.HasValue)
                        {
                            tps.Push(new TP_ChanelType
                            {
                                TP_ID = bp.TP_ID.Value,
                                ChannelType = bp.ChannelType ?? 1,
                                //Пока не работает
                                //DataSourceType = (EnumDataSourceType?)bp.DataSource_ID,
                            });
                            itemParam.Id = bp.TP_ID.Value;
                            itemParam.TypeHierarchy = enumTypeHierarchy.Info_TP;
                        }
                        else if (!string.IsNullOrEmpty(bp.Formula_UN))
                        {
                            formulas.Push(new TFormulaParam(bp.Formula_UN, enumFormulasTable.Info_Formula_Description,
                                null, null, _dtServerStart, null));
                            itemParam.Un = bp.Formula_UN;
                            itemParam.TypeHierarchy = enumTypeHierarchy.Formula;
                        }
                        else if (!string.IsNullOrEmpty(bp.OurFormula_UN))
                        {
                            formulas.Push(new TFormulaParam(bp.OurFormula_UN,
                                enumFormulasTable.Info_TP2_OurSide_Formula_Description, null, null, _dtServerStart,
                                null));
                            itemParam.Un = bp.OurFormula_UN;
                            itemParam.TypeHierarchy = enumTypeHierarchy.Formula_TP_OurSide;
                        }
                        else if (!string.IsNullOrEmpty(bp.ContrFormula_UN))
                        {
                            formulas.Push(new TFormulaParam(bp.ContrFormula_UN,
                                enumFormulasTable.Info_TP2_Contr_Formula_Description, null, null, _dtServerStart,
                                null));
                            itemParam.Un = bp.ContrFormula_UN;
                            itemParam.TypeHierarchy = enumTypeHierarchy.Formula_TP_CA;
                        }
                        else if (bp.PTransformator_ID.HasValue)
                        {
                            transIds.TryAdd(bp.PTransformator_ID.Value, bp.BalanceFreeHierarchy_UN);
                            itemParam.Id = bp.PTransformator_ID.Value;
                            itemParam.TypeHierarchy = enumTypeHierarchy.PTransformator;
                        }
                        else if (bp.PReactor_ID.HasValue)
                        {
                            reactorsIds.Push(bp.PReactor_ID.Value);
                            itemParam.Id = bp.PReactor_ID.Value;
                            itemParam.TypeHierarchy = enumTypeHierarchy.Reactor;
                        }
                        else if (bp.Section_ID.HasValue)
                        {
                            sections.Push(new TSectionChannel
                            {
                                Section_ID = bp.Section_ID.Value,
                                ChannelType = bp.ChannelType.GetValueOrDefault(),
                            });

                            itemParam.Id = bp.Section_ID.Value;
                            itemParam.TypeHierarchy = enumTypeHierarchy.Section;
                        }
                        else if (!string.IsNullOrEmpty(bp.FormulaConstant_UN))
                        {
                            formulaConstantIds.Push(bp.FormulaConstant_UN);
                            itemParam.Un = bp.FormulaConstant_UN;
                            itemParam.TypeHierarchy = enumTypeHierarchy.FormulaConstant;
                        }
                    }
                }
            }

            if (subsections.Count > 0)
            {
                SubsectionNames = subsections
                    .GroupBy(s => s.BalanceFreeHierarchySubsection_UN)
                    .ToDictionary(k => k.Key, v => v.First().BalanceFreeHierarchySubsectionName);
            }

            return resultByBalance;
        }

        /// <summary>
        /// Чтение параметров трансформатора
        /// </summary>
        /// <param name="transIds">Идентификаторы трансформаторов</param>
        /// <param name="tiForTransformatorsAndReactors">Идентифкаторы ТИ для запросов в архив</param>
        /// <param name="itemsResultByBalance">Параметры балансов</param>
        private void ReadTransformatorsParams(ConcurrentDictionary<int, string> transIds,
            ConcurrentStack<TI_ChanelType> tiForTransformatorsAndReactors,
            ConcurrentDictionary<string, BalanceFreeHierarchyCalculatedResult> itemsResultByBalance)
        {
            if (transIds.Count == 0) return;

            var transList = transIds.ToList();
            Parallel.ForEach(Partitioner.Create(0, transIds.Count, Settings.MaxStringRows)
                , range =>
                {
                    var localIds = transList.GetRange(range.Item1, range.Item2 - range.Item1);
                    using (var db = new FSKDataContext(Settings.DbConnectionString)
                    {
                        ObjectTrackingEnabled = false,
                        DeferredLoadingEnabled = false,
                        CommandTimeout = 180,
                    })
                    {
                        var sprocTransformator = db.usp2_Info_GetTransfomatorsPropertyForBalancePS(null, _dtServerStart,
                            _dtServerEnd, string.Join(",", localIds.Select(v => v.Key)));

                        //Берем время работы трансформаторов
                        var transformatorWorkRangeDict = sprocTransformator
                            .GetResult<DBPTransformatorWorkPeriod>()
                            .ToList()
                            .GroupBy(r => r.PTransformator_ID)
                            .ToDictionary(k => k.Key, v => v.ToList());

                        //Считываем характеристики трансформаторов
                        var tps = sprocTransformator.GetResult<DBPTransformatorProperty>()
                            .Select(tProp =>
                            {
                                BalanceFreeHierarchyCalculatedResult balanceResult;
                                string balanceFreeHierarchyUn;
                                if (!transIds.TryGetValue(tProp.PTransformator_ID, out balanceFreeHierarchyUn)
                                    || !itemsResultByBalance.TryGetValue(balanceFreeHierarchyUn, out balanceResult))
                                    return null;

                                var transformator = new TPTransformatorsResult(tProp, transformatorWorkRangeDict,
                                    _dtServerStart, _dtServerEnd, _numbersHalfHours);
                                balanceResult.Transformators.Add(transformator);

                                return transformator;
                            }).ToArray();

                        //заполняем ТИ в трансформаторах
                        foreach (var tiGroupByTransformator in sprocTransformator
                            .GetResult<DBPTransformators_To_TI_List>()
                            .ToList()
                            .GroupBy(t => t.PTransformator_ID))
                        {
                            var tr = tps.FirstOrDefault(t =>
                                t != null && t.PTransformatorProperty.PTransformator_ID == tiGroupByTransformator.Key);
                            if (tr == null) continue;

                            tr.PTransformators_To_TI_List.AddRange(tiGroupByTransformator);
                            for (byte channelType = 1; channelType < 5; channelType++)
                            {
                                var tiArray = tiGroupByTransformator.Select(t => new TI_ChanelType
                                { TI_ID = t.TI_ID, ChannelType = channelType, IsCA = t.IsCA }).ToArray();
                                if (tiArray.Length > 0)
                                    tiForTransformatorsAndReactors.PushRange(tiArray);
                            }
                        }
                    }
                });
        }

        public void Dispose()
        {
            _itemsResultByBalance.Clear();
            //SectionsByType.Clear();
        }
    }
}
