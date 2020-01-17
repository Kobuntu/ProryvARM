using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;

namespace Proryv.Servers.Calculation.FormulaInterpreter.Formulas
{
    class ArchivesHierObjectProvider : IArchivesHierObject
    {
        private enumTimeDiscreteType discreteType;
        private DateTime dtEnd;
        private DateTime dtStart;
        private List<IHierarchyChannelID> list;
        private EnumUnitDigit none;
        private bool v;
        private EnumDataSourceType? _dataSourceType;
        private bool _isReadCalculatedValues;
        private string _timeZoneId;

        public ArchivesHierObjectProvider(List<IHierarchyChannelID> list, bool v, DateTime dtStart, DateTime dtEnd, EnumDataSourceType? _dataSourceType, enumTimeDiscreteType discreteType, EnumUnitDigit none, bool _isReadCalculatedValues, string _timeZoneId)
        {
            this.list = list;
            this.v = v;
            this.dtStart = dtStart;
            this.dtEnd = dtEnd;
            this._dataSourceType = _dataSourceType;
            this.discreteType = discreteType;
            this.none = none;
            this._isReadCalculatedValues = _isReadCalculatedValues;
            this._timeZoneId = _timeZoneId;
        }

        public StringBuilder Errors { get; set; }
        public Dictionary<IHierarchyChannelID, List<TVALUES_DB>> result_Values { get; set; }
    }
}
