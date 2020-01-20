using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Proryv.AskueARM2.Server.DBAccess.Internal.DB;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Archives;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances.Data;
using Proryv.AskueARM2.Server.DBAccess.Public.Utils;
using Proryv.AskueARM2.Server.WCF;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Drums;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Formulas;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Section;
using System.Threading;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances
{
    /// <summary>
    /// Результат по списку универсальных балансов
    /// </summary>
    [DataContract]
    public class BalanceFreeHierarchyResults
    {
        /// <summary>
        /// Идентификаторы балансов
        /// </summary>
        [DataMember] public readonly List<string> BalanceFreeHierarchyUNs;
        /// <summary>
        /// Время начала выборки
        /// </summary>
        [DataMember] public readonly DateTime DTStart;
        /// <summary>
        /// Время конца выборки
        /// </summary>
        [DataMember] public readonly DateTime DTEnd;
        /// <summary>
        /// Идентификатор часового пояса MS
        /// </summary>
        [DataMember]
        public readonly string TimeZoneId;
        /// <summary>
        /// Разрядность профиля
        /// </summary>
        [DataMember] public readonly EnumUnitDigit UnitDigit;
        /// <summary>
        /// Разрядность интегралов
        /// </summary>
        [DataMember] public readonly EnumUnitDigit UnitDigitIntegrals;

        /// <summary>
        /// Формат документа
        /// </summary>
        [DataMember] public readonly TExportExcelAdapterType AdapterType;

        /// <summary>
        /// Период дискретизации
        /// </summary>
        [DataMember] public readonly enumTimeDiscreteType DiscreteType;

        /// <summary>
        /// Ошибки
        /// </summary>
        [DataMember] public readonly StringBuilder Errors;

        /// <summary>
        /// Результаты расчетов балансов
        /// </summary>
        [DataMember] public readonly ConcurrentDictionary<string, BalanceFreeHierarchyCalculatedResult> CalculatedValues;

        /// <summary>
        /// Используемые разделы по типам
        /// </summary>
        public ConcurrentDictionary<EnumBalanceFreeHierarchyType, List<Dict_Balance_FreeHierarchy_Section>> SectionsByType;

        public Dictionary<string, string> SubsectionNames;

        private readonly CancellationToken? _cancellationToken;

        #region Private
        private readonly DateTime _dtServerStart;
        private readonly DateTime _dtServerEnd;
        private readonly int _numbersHalfHours;
        private readonly bool _isGenerateDoc;
        
        #endregion

        public BalanceFreeHierarchyResults(List<string> balanceFreeHierarchyUNs, DateTime dTStart, DateTime dTEnd,
            string timeZoneId, TExportExcelAdapterType adapterType, bool isGenerateDoc, enumTimeDiscreteType discreteType, EnumUnitDigit unitDigit, 
            EnumUnitDigit unitDigitIntegrals, CancellationToken? cancellationToken = null)
        {
            if (unitDigit == EnumUnitDigit.Null) unitDigit = EnumUnitDigit.None;

            BalanceFreeHierarchyUNs = balanceFreeHierarchyUNs.Distinct().ToList();
            DTStart = dTStart.RoundToHalfHour(true);
            DTEnd = dTEnd.RoundToHalfHour(true);
            TimeZoneId = timeZoneId;
            UnitDigit = unitDigit;
            UnitDigitIntegrals = unitDigitIntegrals;
            AdapterType = adapterType;
            DiscreteType = discreteType;
            _cancellationToken = cancellationToken;
            Errors = new StringBuilder();
            CalculatedValues = new ConcurrentDictionary<string, BalanceFreeHierarchyCalculatedResult>();

            if (BalanceFreeHierarchyUNs == null || BalanceFreeHierarchyUNs.Count == 0)
            {
                Errors.Append("Неверный идентификатор(ы) баланса!");
                return;
            }

            if ((DTEnd < DTStart))
            {
                Errors.Append("Дата окончания должна быть больше или равна даты начала!");
                return;
            }

            _dtServerStart = DTStart.ClientToServer(TimeZoneId);
            _dtServerEnd = DTEnd.ClientToServer(TimeZoneId);
            _isGenerateDoc = isGenerateDoc;

            
            var tiForRequestAdditionalInfo = new ConcurrentDictionary<TI_ChanelType, ConcurrentStack<BalanceFreeHierarchyItemParams>>(new TI_ChanelComparer());

            using (var calculator = new BalanceFreeHierarchyCalculator(BalanceFreeHierarchyUNs, isGenerateDoc, Errors, DiscreteType, DTStart, DTEnd, TimeZoneId, tiForRequestAdditionalInfo, 
                UnitDigit, UnitDigitIntegrals))
            {
                SectionsByType = calculator.SectionsByType;

                var po = new ParallelOptions();
                if (cancellationToken.HasValue)
                {
                    po.CancellationToken = cancellationToken.Value;
                }

                po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
                po.CancellationToken.ThrowIfCancellationRequested();

                //Строим результаты по документам
                Parallel.ForEach(BalanceFreeHierarchyUNs, po, (balanceFreeHierarchyUn, loopState) =>
                {
                    try
                    {
                        if (po.CancellationToken.IsCancellationRequested) loopState.Break();

                        var calculatedResult = calculator.Calculate(balanceFreeHierarchyUn);
                        if (calculatedResult == null) return;

                        CalculatedValues.TryAdd(balanceFreeHierarchyUn, calculatedResult);
                    }
                    catch (Exception ex)
                    {
                        lock (Errors)
                        {
                            Errors.Append(ex.Message);
                        }
                    }
                });

                SubsectionNames = calculator.SubsectionNames;
            }

            if (_cancellationToken.HasValue && _cancellationToken.Value.IsCancellationRequested) return;

            #region Собираем доп. информацию


            if (!tiForRequestAdditionalInfo.IsEmpty)
            {
                SortedList<TI_ChanelType, List<TTransformators_Information>> transormatorsInformation = null;
                ConcurrentDictionary<TI_ChanelType, List<ArchCalc_Replace_ActUndercount>> replaceActUndercountList = null;

                Parallel.Invoke(() =>
                    {
                        //Информация о смене трансформаторов
                        transormatorsInformation = SectionIntegralActsResultsList.GetTransformationInformationList(DTStart, DTEnd, tiForRequestAdditionalInfo.Keys, UnitDigit, 
                            enumOVMode.NormalMode, true,
                            TimeZoneId);
                    },
                    () =>
                    {
                        ConcurrentDictionary<TI_ChanelType, List<ArchCalc_Replace_ActUndercount>> resultOvs;
                        //Информация об акте недоучета
                        replaceActUndercountList = SectionIntegralActsResultsList.GetReplaceActUndercount(_dtServerStart, _dtServerEnd, tiForRequestAdditionalInfo.Keys, Errors, out resultOvs);
                    }
                );

                foreach (var pair in tiForRequestAdditionalInfo)
                {
                    if (transormatorsInformation != null)
                    {
                        List<TTransformators_Information> informations;
                        if (transormatorsInformation.TryGetValue(pair.Key, out informations))
                        {
                            foreach (var itemParam in pair.Value)
                            {
                                itemParam.Transformatos_Information = informations;
                            }
                        }
                    }
                    
                    if (replaceActUndercountList != null && replaceActUndercountList.Count > 0)
                    {
                        List<ArchCalc_Replace_ActUndercount> replaceInformation;
                        if (replaceActUndercountList.TryGetValue(pair.Key, out replaceInformation) && replaceInformation!=null)
                        {
                            foreach (var itemParam in pair.Value)
                            {
                                itemParam.ReplaceActUndercountList = replaceInformation;
                            }
                        }
                    }
                }
            }

            #endregion
        }
    }
}
