using System;
using System.Collections.Generic;
using System.Linq;
using Proryv.Servers.Calculation.DBAccess.Common.Ext;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;

namespace Proryv.AskueARM2.Server.DBAccess.Internal.TClasses
{
    public class TArchiveHierObjectPeriod
    {
        public readonly IHierarchyChannelID Id;
        public readonly  DateTime DtStart;
        public readonly DateTime DtEnd;
        public readonly List<ArchiveHierObjectPeriodValue> Vals;
        public readonly enumTimeDiscreteType DiscreteType;
        
        public TArchiveHierObjectPeriod(IHierarchyChannelID id, DateTime dtStart, DateTime dtEnd, List<Servers.Calculation.DBAccess.Interface.Data.TVALUES_DB> vals, enumTimeDiscreteType discreteType, 
            string timeZoneId,IMyListConverters _myListConverters)
        {
            Id = id;
            DtStart = dtStart;
            DtEnd = dtEnd;
            var indx = 0;
            Vals = new List<ArchiveHierObjectPeriodValue>();
            foreach (var dt in _myListConverters.GetDateTimeListForPeriod(dtStart, dtEnd,
                discreteType, timeZoneId.GeTimeZoneInfoById()))
            {
                Vals.Add(new ArchiveHierObjectPeriodValue
                {
                    Year = dt.Year,
                    Month = dt.Month,
                    Day = dt.Day,
                    Hour = dt.Hour,
                    Val = vals.ElementAtOrDefault(indx++),
                });
            }
            DiscreteType = discreteType;
        }

    }

    public class ArchiveHierObjectPeriodEqualityComparer : IEqualityComparer<TArchiveHierObjectPeriod>
    {
        public bool Equals(TArchiveHierObjectPeriod x, TArchiveHierObjectPeriod y)
        {
            if (y == null) return false;

            return x.Id.Equals(y.Id) && Equals(x.DtStart, y.DtStart) && Equals(x.DtEnd, y.DtEnd);
        }

        public int GetHashCode(TArchiveHierObjectPeriod obj)
        {
            return String.Format("{0}{1}{2}{3}{4}", obj.Id.TypeHierarchy, obj.Id.Channel, obj.Id.ID, obj.DtStart, obj.DtEnd).GetHashCode();
        }
    }

    public class ArchiveHierObjectPeriodValue
    {
        public int Year;
        public int Month;
        public int Day;
        public int Hour;
        public TVALUES_DB Val;
    }
}
