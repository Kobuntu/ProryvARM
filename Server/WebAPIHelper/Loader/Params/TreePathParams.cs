using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.Servers.Calculation.DBAccess.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Proryv.AskueARM2.Server.WCF.Loader.Params
{
    [DataContract]
    public class TreePathParams
    {
        [DataMember]
        public List<ID_TypeHierarchy> Ids;
        [DataMember]
        public string UserId;
        [DataMember]
        public int? TreeID;
    }
}
