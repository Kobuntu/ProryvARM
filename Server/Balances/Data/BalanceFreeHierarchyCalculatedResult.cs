using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.DB;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.Servers.Calculation.DBAccess.Common.Data;
using Proryv.Servers.Calculation.DBAccess.Interface.Documents;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances.Data
{
    /// <summary>
    /// Результат обсчета по одному балансу
    /// </summary>
    [DataContract]
    public class BalanceFreeHierarchyCalculatedResult : IDocumentArchiveForCalculation
    {
        public BalanceFreeHierarchyCalculatedResult(Info_Balance_FreeHierarchy_List balanceInfo)
        {
            BalanceFreeHierarchyUn = balanceInfo.BalanceFreeHierarchy_UN;
            BalanceFreeHierarchyName = balanceInfo.BalanceFreeHierarchyName;
            BalanceFreeHierarchyType = (EnumBalanceFreeHierarchyType)balanceInfo.BalanceFreeHierarchyType_ID;
            ItemsParamsBySection = new Dictionary<string, List<BalanceFreeHierarchyItemParams>>();
            CalculatedByDiscretePeriods = new List<BalanceFreeHierarchyCalculatedDiscretePeriod>();
            Transformators = new List<TPTransformatorsResult>();
            Reactors = new List<TPReactorsResult>();

            HighLimit = balanceInfo.HighLimit;
            LowerLimit = balanceInfo.LowerLimit;
            HighLimitValue = balanceInfo.HighLimitValue;
            LowerLimitValue = balanceInfo.LowerLimitValue;

            ResultInfo = new TBalanceResultInfo();

            BalanceHalfhours = new List<TBalanceHalfhour>();
        }

        /// <summary>
        /// Идентификатор родительского объекта
        /// </summary>
        [DataMember]
        public ID_TypeHierarchy ObjectId;

        /// <summary>
        /// Идентификатор
        /// </summary>
        [DataMember]
        public readonly string BalanceFreeHierarchyUn;

        /// <summary>
        /// Название баланса
        /// </summary>
        [DataMember]
        public readonly string BalanceFreeHierarchyName;

        public string DocumentName
        {
            get
            {
                return BalanceFreeHierarchyName;
            }
        }

        /// <summary>
        /// Тип баланса
        /// </summary>
        [DataMember]
        public readonly EnumBalanceFreeHierarchyType BalanceFreeHierarchyType;

        public EnumBalanceFreeHierarchyType DocumentType { get
            {
                return BalanceFreeHierarchyType;
            }
        }
                        
        /// <summary>
        /// Результат обсчета по дискретным периодам
        /// </summary>
        [DataMember]
        public List<BalanceFreeHierarchyCalculatedDiscretePeriod> CalculatedByDiscretePeriods { get; private set; }

        /// <summary>
        /// Объекты в балансе по разделам
        /// </summary>
        public Dictionary<string, List<BalanceFreeHierarchyItemParams>> ItemsParamsBySection { get; private set; }
        /// <summary>
        /// Трансформаторы
        /// </summary>
        public List<TPTransformatorsResult> Transformators { get; private set; }
        /// <summary>
        /// Реакторы
        /// </summary>
        public List<TPReactorsResult> Reactors { get; private set; }

        /// <summary>
        /// Сформированный и сжатый документ
        /// </summary>
        [DataMember] public MemoryStream CompressedDoc;


        /// <summary>
        /// Верхняя уставка %
        /// </summary>
        [DataMember]
        public readonly double? HighLimit;

        /// <summary>
        /// Нижняя уставка %
        /// </summary>
        [DataMember]
        public readonly double? LowerLimit;

        /// <summary>
        /// Верхняя уставка (в абсол. ед.)
        /// </summary>
        [DataMember]
        public readonly double? HighLimitValue;

        /// <summary>
        /// Нижняя уставка (в абсол. ед.)
        /// </summary>
        [DataMember]
        public readonly double? LowerLimitValue;

        /// <summary>
        /// Информация о балансе
        /// </summary>
        [DataMember] public readonly TBalanceResultInfo ResultInfo;

        /// <summary>
        /// Результат
        /// </summary>
        public double? Result
        {
            get
            {
                if (ResultInfo == null) return null;

                return ResultInfo.ResolvedUnBalancePercent;
            }
        }

        /// <summary>
        /// Детальная информация по каждой получасовке (для графика)
        /// </summary>
        [DataMember]
        public List<TBalanceHalfhour> BalanceHalfhours;
    }
}
