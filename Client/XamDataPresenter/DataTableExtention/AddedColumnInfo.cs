using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;

namespace Proryv.AskueARM2.Both.VisualCompHelpers.XamDataPresenter.DataTableExtention
{
    public class AddedColumnInfo
    {
        public readonly ID_Hierarchy Id;
        public readonly EnumDataSourceType? DataSourceType;
        public readonly string MeasureUnitUn;
        public readonly List<TVALUES_DB> Values;
        public readonly byte Channel;

        /// <summary>
        /// Значения замещаемых ТИ по индексам в которых должны лежать данные
        /// </summary>
        //public readonly Dictionary<int, TVALUES_DB> ValuesByIndexes;

        /// <summary>
        /// Информация по замещаемых ТИ, с какого по какой индекс замещались
        /// </summary>
        public readonly Dictionary<int, TDateTimeOV> TiReplaced;

        public readonly int ReplacedTiId;

        /// <summary>
        /// Для группировки по замещаемой ТИ
        /// </summary>
        public readonly string GroupByReplacedTiName;

        public AddedColumnInfo(ID_Hierarchy id, EnumDataSourceType? dataSourceType, string measureUnitUn, 
            List<TVALUES_DB> values, byte channel, Dictionary<int, TDateTimeOV> tiReplaced = null, int replacedTiId = -1,
            string groupByReplacedTiName = null)
        {
            Id = id;
            DataSourceType = dataSourceType;
            MeasureUnitUn = measureUnitUn;
            Values = values;
            Channel = channel;
            ReplacedTiId = replacedTiId;

            //ValuesByIndexes = ValuesByIndexes;
            TiReplaced = tiReplaced;
            GroupByReplacedTiName = groupByReplacedTiName;
        }
    }
}
