using Proryv.AskueARM2.Server.DBAccess.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Proryv.AskueARM2.Server.WCF.Loader.Params
{
    [DataContract]
    public class FreeHierBalanceParams
    {
        [DataMember] public List<string> BalanceFreeHierarchyUNs;
        [DataMember] public DateTime DTStart;
        [DataMember] public DateTime DTEnd;
        [DataMember] public string TimeZoneId;
        [DataMember] public TExportExcelAdapterType AdapterType;
        [DataMember] public bool IsGenerateDoc;
        [DataMember] public enumTimeDiscreteType DiscreteType;
        [DataMember] public EnumUnitDigit UnitDigit;
        [DataMember] public bool IsFormingSeparateList;
        [DataMember] public EnumUnitDigit UnitDigitIntegrals;
        [DataMember] public bool IsUseThousandKVt;
        [DataMember] public bool PrintLandscape;
        [DataMember] public byte DoublePrecisionProfile;
        [DataMember] public byte DoublePrecisionIntegral;
        [DataMember] public bool Need0;
        [DataMember] public bool IsAnalisIntegral;
        [DataMember] public bool SetPercisionAsDisplayed;
    }
}
