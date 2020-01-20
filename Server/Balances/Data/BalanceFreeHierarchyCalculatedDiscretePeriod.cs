using System.Collections.Generic;
using System.Runtime.Serialization;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.Servers.Calculation.DBAccess.Enums.Internals;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances.Data
{
    [DataContract]
    public class BalanceFreeHierarchyCalculatedDiscretePeriod
    {
        /// <summary>
        /// Допустимый небаланс
        /// </summary>
        [DataMember]
        public double ResolvedUnBalanceValues;

        /// <summary>
        /// Фактический небаланс,%
        /// </summary>
        [DataMember]
        public double FactUnbalancePercent;

        /// <summary>
        /// Фактический небаланс,кВт (по модулю)
        /// </summary>
        [DataMember]
        public double FactUnbalanceValue;

        /// <summary>
        /// Результаты по разделам
        /// </summary>
        [DataMember]
        public Dictionary<string, double> SectionValues;

        /// <summary>
        /// Достоверность
        /// </summary>
        [DataMember]
        public VALUES_FLAG_DB F_FLAG;

        /// <summary>
        /// Отпуск и расход + трансформаторы и реакторы всего (Вт)
        /// </summary>
        [DataMember]
        public double OutValues;

        /// <summary>
        /// Поступило на шины всего (Вт)
        /// </summary>
        [DataMember]
        public double InBusValues;
    }

    [DataContract]
    public class BalanceFreeHierarchySectionValues
    {
        [DataMember]
        public double IntegralSumm;
        [DataMember]
        public double HalfHourSumm;
    }
}
