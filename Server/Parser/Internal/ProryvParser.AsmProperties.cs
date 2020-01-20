using System;
using System.Collections;
using Proryv.Servers.Calculation.Parser.Internal.Functions;

namespace Proryv.Servers.Calculation.Parser.Internal
{
    public partial class ProryvParser
    {
        #region Evaluate the value of the property.
        private object call_property(object name, ArrayList argsList)
        {
            object baseValue = argsList[0];
            if (baseValue is DateTime)
            {
                switch ((StiPropertyType)name)
                {
                    case StiPropertyType.Year: return ((DateTime)baseValue).Year;
                    case StiPropertyType.Month: return ((DateTime)baseValue).Month;
                    case StiPropertyType.Day: return ((DateTime)baseValue).Day;
                    case StiPropertyType.Hour: return ((DateTime)baseValue).Hour;
                    case StiPropertyType.Minute: return ((DateTime)baseValue).Minute;
                    case StiPropertyType.Second: return ((DateTime)baseValue).Second;
                }
                ThrowError(ParserErrorCode.ItemDoesNotContainDefinition, GetTypeName(argsList[0]), Enum.GetName(typeof(StiPropertyType), name));
            }

            if (baseValue is TimeSpan)
            {
                switch ((StiPropertyType)name)
                {
                    case StiPropertyType.Days: return ((TimeSpan)baseValue).Days;
                    case StiPropertyType.Hours: return ((TimeSpan)baseValue).Hours;
                    case StiPropertyType.Milliseconds: return ((TimeSpan)baseValue).Milliseconds;
                    case StiPropertyType.Minutes: return ((TimeSpan)baseValue).Minutes;
                    case StiPropertyType.Seconds: return ((TimeSpan)baseValue).Seconds;
                    case StiPropertyType.Ticks: return ((TimeSpan)baseValue).Ticks;
                }
                ThrowError(ParserErrorCode.ItemDoesNotContainDefinition, GetTypeName(argsList[0]), Enum.GetName(typeof(StiPropertyType), name));
            }

            if (baseValue is String)
            {
                switch ((StiPropertyType)name)
                {
                    case StiPropertyType.Length: return Convert.ToString(baseValue).Length;
                }
                ThrowError(ParserErrorCode.ItemDoesNotContainDefinition, GetTypeName(argsList[0]), Enum.GetName(typeof(StiPropertyType), name));
            }

            if (baseValue == null)
            {
                switch ((StiPropertyType)name)
                {
                    case StiPropertyType.Length: return 0;
                }
            }

            //if (baseValue is Range)
            //{
            //    switch ((StiPropertyType)name)
            //    {
            //        case StiPropertyType.From: return (baseValue as Range).FromObject;
            //        case StiPropertyType.To: return (baseValue as Range).ToObject;
            //        case StiPropertyType.FromDate: return (baseValue as DateTimeRange).FromDate;
            //        case StiPropertyType.ToDate: return (baseValue as DateTimeRange).ToDate;
            //        case StiPropertyType.FromTime: return (baseValue as TimeSpanRange).FromTime;
            //        case StiPropertyType.ToTime: return (baseValue as TimeSpanRange).ToTime;
            //    }
            //    ThrowError(ParserErrorCode.ItemDoesNotContainDefinition, GetTypeName(argsList[0]), Enum.GetName(typeof(StiPropertyType), name));
            //}

            if (baseValue is IStiList)
            {
                switch ((StiPropertyType)name)
                {
                    case StiPropertyType.Count: return (baseValue as IStiList).Count;
                }
                ThrowError(ParserErrorCode.ItemDoesNotContainDefinition, GetTypeName(argsList[0]), Enum.GetName(typeof(StiPropertyType), name));
            }

            return null;
        }
        #endregion    
    }
}
