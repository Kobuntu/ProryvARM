using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Calculation.ArchTech.Data
{
    [DataContract]
    public class ArchTechRequestParams
    {
        [DataMember]
        public List<ArchTechRequestParam> ArchTechObjectIds;
        [DataMember]
        public bool UseLossesCoefficient;
        [DataMember]
        public bool UseCoeffTransformation;
        [DataMember]
        public string TimeZoneId;
        [DataMember]
        public EnumUnitDigit UnitDigit;
        [DataMember]
        public DateTime DtStart;
        [DataMember]
        public DateTime DtEnd;
        [DataMember]
        public EnumTechProfilePeriod? TechProfilePeriod;
    }
}
