using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Forecasting.Data
{
    [DataContract]
    public class TForecastFreeHierarchyItem : IHierarchyItem<TForecastFreeHierarchyItem>
    {
        [DataMember]
        public List<TForecastFreeHierarchyItem> Children { get; set; }

        [DataMember]
        public int FreeHierItemId { get; set; }

        [DataMember]
        public string Id { get; set; }

        public TForecastFreeHierarchyItem Parent { get; set; }

        [DataMember]
        public byte TypeHierarchy { get; set; }
    }
}
