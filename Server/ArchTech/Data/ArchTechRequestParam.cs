using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.Servers.Calculation.DBAccess.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Calculation.ArchTech.Data
{
    [DataContract]
    public class ArchTechRequestParam
    {
        [DataMember]
        public ID_TypeHierarchy ID;

        [DataMember]
        public byte ChannelType;
    }
}
