using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Public.GroupTP;
using Proryv.Servers.Calculation.DBAccess.Common.Data;

namespace Proryv.AskueARM2.Server.WCF.GroupTp
{
    /// <summary>
    /// Параметры для обсчета фактической мощности
    /// </summary>
    [DataContract]
    public class GroupTpWaiterParams
    {
        [DataMember] public EnumDataSourceType? DataSourceType;
        [DataMember] public EnumFactPowerCalculateMode FactPowerCalculateMode;
        [DataMember] public List<ID_TypeHierarchy> IdList;
        [DataMember] public bool IsCalculateFactPowerByTPVoltageLevel;
        [DataMember] public bool IsReadCalculatedValues;
        [DataMember] public bool IsRunWaiter;
        [DataMember] public DateTime StartMonthYear;
        [DataMember] public string TimeZoneId;
    }
}
