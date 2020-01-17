using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.Servers.Calculation.DBAccess.Common.Data;

namespace Proryv.AskueARM2.Server.WCF.Loader
{
    /// <summary>
    /// Параметры для запроса получасовок
    /// </summary>
    [DataContract]
    public class Archive30TiParams
    {
        [DataMember] public bool IsCoeff;
        [DataMember] public bool IsCaEnabled;
        [DataMember] public enumOVMode OvMode;
        [DataMember] public bool IsAutoRead;
        [DataMember] public bool IsTiInFormulaEnabled;
        [DataMember] public bool IsReadCalculatedValues;
        [DataMember] public bool UseLossesCoefficient;
        [DataMember] public bool IsJournalEnabled;
        [DataMember] public bool IsEmulateHalfHours;
        [DataMember] public List<TFormulaParam> Formulas;
        //Запрос ТИ
        [DataMember] public List<TI_ChanelType> TiIds;
        //Запрос интегралов
        [DataMember] public List<TI_ChanelType> Integrals;
        [DataMember] public List<ID_TypeHierarchy> Tps;
        [DataMember] public HashSet<int> Sections;
        [DataMember] public HashSet<string> FormulaConstantIds;
        [DataMember] public HashSet<string> BalanceFreeHierarchyIds;
        [DataMember] public List<TPUParam> Tis;
        [DataMember] public int Innerlev;
        [DataMember] public enumTypeInformation IsPower;
        [DataMember] public EnumUnitDigit EnumUnit;
        [DataMember] public enumTimeDiscreteType DiscreteType;
        [DataMember] public string TimeZoneId;
        [DataMember] public DateTime DTStart;
        [DataMember] public DateTime DTEnd;
        [DataMember] public bool IsSumTiForChart; //считаем сумму поканально по ТИ на графиках
        [DataMember] public bool RoundData;
        [DataMember] public bool UseRoundedTi;
        /// <summary>
        /// Эмулировать профиль на основе минуток
        /// </summary>
        [DataMember] public bool isEmulateProfileFromMinutes;
        [DataMember] public List<EnumDataSourceType> DataSourceTypes;

        /// <summary>
        /// Детализировать информацию по ОВ
        /// </summary>
        [DataMember] public bool IsDetailOv;

        [DataMember]
        public int? Phase;

        [DataMember]
        public double? LowPowerConsumption;

        [DataMember]
        public double? HiPowerConsumption;

        [DataMember]
        public bool IsOffsetFromMoscowEnbledForDrums;
    }
}
