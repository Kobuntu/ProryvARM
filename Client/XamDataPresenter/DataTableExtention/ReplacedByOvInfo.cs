using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;

namespace Proryv.AskueARM2.Both.VisualCompHelpers.XamDataPresenter.DataTableExtention
{
    public class ReplacedByOvInfo
    {
        public readonly ID_Hierarchy Id;
        public readonly EnumDataSourceType? DataSourceType;
        public readonly string MeasureUnitUn;
        public readonly byte Channel;

        public Dictionary<int, TVALUES_DB> ValuesByIndexes;

        public ReplacedByOvInfo(ID_Hierarchy id, EnumDataSourceType? dataSourceType, string measureUnitUn, byte channel, Dictionary<int, TVALUES_DB> valuesByIndexes)
        {
            Id = id;
            DataSourceType = dataSourceType;
            MeasureUnitUn = measureUnitUn;
            Channel = channel;
            ValuesByIndexes = valuesByIndexes;
        }
    }
}
