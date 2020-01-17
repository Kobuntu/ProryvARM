using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Proryv.AskueARM2.Server.WCF.Loader
{
    /// <summary>
    /// Для подгрузки объектов иерархии
    /// </summary>
    [DataContract]
    public class HierarchyLoaderParams
    {
        [DataMember] public List<int> ParrentList;
        [DataMember] public bool IsCA;
        [DataMember] public bool IsTP;
    }
}
