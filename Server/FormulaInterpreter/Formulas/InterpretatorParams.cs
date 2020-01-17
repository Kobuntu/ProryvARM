using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.Enums;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.Servers.Calculation.FormulaInterpreter.Formulas
{
    public class InterpretatorParams
    {
        public int InnerLevel;
        public DateTime StartDateTime;
        public DateTime EndDateTime;

        public DateTime ServerStartDateTime;
        public DateTime ServerEndDateTime;

        public enumTimeDiscreteType DiscreteType;
        public bool IsOvEnabled;
        public bool IsValuesAllTIEnabled;
        public bool IsResultByIntervalTIValues;
        public StringBuilder Errors;
        public int NumbersValues;
        public List<int> IntervalTimeList;
        public bool IsValidateOnly;
        public bool IsValidateOtherDataSource;
        public int NumbersHalfHours;
        public EnumUnitDigit UnitDigit;
        public EnumDataSourceType? DataSourceType;
        public ConcurrentStack<TI_ChanelType> UsedTisOurSide;
        public enumTypeInformation TypeInformation;
        public bool IsReturnPreviousDispatchDateTime;
        public bool IsReadCalculatedValues;

        public string TimeZoneId;

        public EnumTechProfilePeriod? TechProfilePeriod;

        public bool IsEmulateProfileFromMinutes;
    }
}
