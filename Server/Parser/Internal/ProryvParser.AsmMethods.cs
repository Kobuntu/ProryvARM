using System;
using System.Collections;
using System.Collections.Generic;
using Proryv.Servers.Calculation.Parser.Internal.Functions;

namespace Proryv.Servers.Calculation.Parser.Internal
{
    public partial class ProryvParser
    {
        #region Evaluate the value of the method.
        private object call_method(object name, ArrayList argsList)
        {
            int category;
            object baseValue = argsList[0];

            #region Global method ToString
            switch ((StiMethodType)name)
            {
                case StiMethodType.ToString:
                    category = get_category(argsList[0]);
                    if (category == 1)
                    {
                        return Convert.ToString(argsList[0]);
                    }
                    else if (category == 2 || category == 3)
                    {
                        decimal resDecimal = Convert.ToDecimal(argsList[0]);
                        if (argsList.Count == 1) return resDecimal.ToString();
                        else return resDecimal.ToString(Convert.ToString(argsList[1]));
                    }
                    else if (category == 4 || category == 6)
                    {
                        ulong resUlong = Convert.ToUInt64(argsList[0]);
                        if (argsList.Count == 1) return resUlong.ToString();
                        else return resUlong.ToString(Convert.ToString(argsList[1]));
                    }
                    else if (category == 5 || category == 7)
                    {
                        long resLong = Convert.ToInt64(argsList[0]);
                        if (argsList.Count == 1) return resLong.ToString();
                        else return resLong.ToString(Convert.ToString(argsList[1]));
                    }
                    else if (category == 8)
                    {
                        DateTime resDate = Convert.ToDateTime(argsList[0]);
                        if (argsList.Count == 1) return resDate.ToString();
                        else return resDate.ToString(Convert.ToString(argsList[1]));
                    }
                    else if (category == 8)
                    {
                        return Convert.ToBoolean(argsList[0]).ToString();
                    }
                    else if (baseValue == null)
                        return string.Empty;
                    else
                        return argsList[0].ToString();
            }
            #endregion

            #region type String
            if (baseValue is string)
            {
                switch ((StiMethodType)name)
                {
                    case StiMethodType.Substring:
                        category = get_category(argsList[0]);
                        if (category != 1) ThrowError(0);
                        if (argsList.Count == 3)
                        {
                            return ((string)argsList[0]).Substring((int)argsList[1], (int)argsList[2]);
                        }
                        else if (argsList.Count == 2)
                        {
                            return ((string)argsList[0]).Substring((int)argsList[1]);
                        }
                        else ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Substring", (argsList.Count - 1).ToString());
                        break;

                    case StiMethodType.ToLower:
                        category = get_category(argsList[0]);
                        if (category != 1) ThrowError(0);
                        if (argsList.Count == 1)
                        {
                            return ((string)argsList[0]).ToLowerInvariant();
                        }
                        else ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "ToLower", (argsList.Count - 1).ToString());
                        break;

                    case StiMethodType.ToUpper:
                        category = get_category(argsList[0]);
                        if (category != 1) ThrowError(0);
                        if (argsList.Count == 1)
                        {
                            return ((string)argsList[0]).ToUpperInvariant();
                        }
                        else ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "ToUpper", (argsList.Count - 1).ToString());
                        break;

                    case StiMethodType.IndexOf:
                        category = get_category(argsList[0]);
                        if (category != 1) ThrowError(ParserErrorCode.ItemDoesNotContainDefinition, GetTypeName(argsList[0]), "IndexOf");
                        if (argsList.Count == 2)
                        {
                            category = get_category(argsList[1]);
                            if (category != 1)
                            {
                                ThrowError(ParserErrorCode.MethodHasInvalidArgument, "IndexOf", "1", GetTypeName(argsList[0]), "string");
                            }
                            return ((string)argsList[0]).IndexOf((string)argsList[1]);
                        }
                        else ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "IndexOf", (argsList.Count - 1).ToString());
                        break;

                    case StiMethodType.StartsWith:
                        category = get_category(argsList[0]);
                        if (category != 1) ThrowError(ParserErrorCode.ItemDoesNotContainDefinition, GetTypeName(argsList[0]), "StartsWith");
                        if (argsList.Count == 2)
                        {
                            category = get_category(argsList[1]);
                            if (category != 1)
                            {
                                ThrowError(ParserErrorCode.MethodHasInvalidArgument, "StartsWith", "1", GetTypeName(argsList[0]), "string");
                            }
                            return ((string)argsList[0]).StartsWith((string)argsList[1]);
                        }
                        else ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "StartsWith", (argsList.Count - 1).ToString());
                        break;

                    case StiMethodType.EndsWith:
                        category = get_category(argsList[0]);
                        if (category != 1) ThrowError(ParserErrorCode.ItemDoesNotContainDefinition, GetTypeName(argsList[0]), "EndsWith");
                        if (argsList.Count == 2)
                        {
                            category = get_category(argsList[1]);
                            if (category != 1)
                            {
                                ThrowError(ParserErrorCode.MethodHasInvalidArgument, "EndsWith", "1", GetTypeName(argsList[0]), "string");
                            }
                            return ((string)argsList[0]).EndsWith((string)argsList[1]);
                        }
                        else ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "EndsWith", (argsList.Count - 1).ToString());
                        break;
                }

            }
            #endregion

            #region type List
            if (baseValue is List<bool> ||
                baseValue is List<char> ||
                baseValue is List<DateTime> ||
                baseValue is List<TimeSpan> ||
                baseValue is List<decimal> ||
                baseValue is List<float> ||
                baseValue is List<double> ||
                baseValue is List<byte> ||
                baseValue is List<short> ||
                baseValue is List<int> ||
                baseValue is List<long> ||
                baseValue is List<Guid> ||
                baseValue is List<string>)
            {
                switch ((StiMethodType)name)
                {
                    case StiMethodType.Contains:
                        //category = get_category(argsList[0]);
                        //if (category != 1) ThrowError(ParserErrorCode.ItemDoesNotContainDefinition, GetTypeName(argsList[0]), "Contains");
                        if (argsList.Count == 2)
                        {
                            if (baseValue is List<bool>) return ((List<bool>)argsList[0]).Contains(Convert.ToBoolean(argsList[1]));
                            if (baseValue is List<char>) return ((List<char>)argsList[0]).Contains(Convert.ToChar(argsList[1]));
                            if (baseValue is List<DateTime>) return ((List<DateTime>)argsList[0]).Contains(Convert.ToDateTime(argsList[1]));
                            if (baseValue is List<TimeSpan>) return ((List<TimeSpan>)argsList[0]).Contains((TimeSpan)argsList[1]);
                            if (baseValue is List<decimal>) return ((List<decimal>)argsList[0]).Contains(Convert.ToDecimal(argsList[1]));
                            if (baseValue is List<float>) return ((List<float>)argsList[0]).Contains(Convert.ToSingle(argsList[1]));
                            if (baseValue is List<double>) return ((List<double>)argsList[0]).Contains(Convert.ToDouble(argsList[1]));
                            if (baseValue is List<byte>) return ((List<byte>)argsList[0]).Contains(Convert.ToByte(argsList[1]));
                            if (baseValue is List<short>) return ((List<short>)argsList[0]).Contains(Convert.ToInt16(argsList[1]));
                            if (baseValue is List<int>) return ((List<int>)argsList[0]).Contains(Convert.ToInt32(argsList[1]));
                            if (baseValue is List<long>) return ((List<long>)argsList[0]).Contains(Convert.ToInt64(argsList[1]));
                            if (baseValue is List<Guid>) return ((List<Guid>)argsList[0]).Contains((Guid)argsList[1]);
                            if (baseValue is List<string>) return ((List<string>)argsList[0]).Contains(Convert.ToString(argsList[1]));
                        }
                        else ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Contains", (argsList.Count - 1).ToString());
                        break;

                    case StiMethodType.ToQueryString:
                        if (argsList.Count == 1)
                        {
                            if (baseValue is List<bool>) return ((BoolList)argsList[0]).ToQueryString();
                            if (baseValue is List<char>) return ((CharList)argsList[0]).ToQueryString();
                            if (baseValue is List<DateTime>) return ((DateTimeList)argsList[0]).ToQueryString();
                            if (baseValue is List<TimeSpan>) return ((TimeSpanList)argsList[0]).ToQueryString();
                            if (baseValue is List<decimal>) return ((DecimalList)argsList[0]).ToQueryString();
                            if (baseValue is List<float>) return ((FloatList)argsList[0]).ToQueryString();
                            if (baseValue is List<double>) return ((DoubleList)argsList[0]).ToQueryString();
                            if (baseValue is List<byte>) return ((ByteList)argsList[0]).ToQueryString();
                            if (baseValue is List<short>) return ((ShortList)argsList[0]).ToQueryString();
                            if (baseValue is List<int>) return ((IntList)argsList[0]).ToQueryString();
                            if (baseValue is List<long>) return ((LongList)argsList[0]).ToQueryString();
                            if (baseValue is List<Guid>) return ((GuidList)argsList[0]).ToQueryString();
                            if (baseValue is List<string>) return ((StringList)argsList[0]).ToQueryString();
                        }
                        else if (argsList.Count == 2)
                        {
                            if (baseValue is List<bool>) return ((BoolList)argsList[0]).ToQueryString(Convert.ToString(argsList[1]));
                            if (baseValue is List<char>) return ((CharList)argsList[0]).ToQueryString(Convert.ToString(argsList[1]));
                            if (baseValue is List<DateTime>) return ((DateTimeList)argsList[0]).ToQueryString(Convert.ToString(argsList[1]));
                            if (baseValue is List<TimeSpan>) return ((TimeSpanList)argsList[0]).ToQueryString(Convert.ToString(argsList[1]));
                            if (baseValue is List<decimal>) return ((DecimalList)argsList[0]).ToQueryString(Convert.ToString(argsList[1]));
                            if (baseValue is List<float>) return ((FloatList)argsList[0]).ToQueryString(Convert.ToString(argsList[1]));
                            if (baseValue is List<double>) return ((DoubleList)argsList[0]).ToQueryString(Convert.ToString(argsList[1]));
                            if (baseValue is List<byte>) return ((ByteList)argsList[0]).ToQueryString(Convert.ToString(argsList[1]));
                            if (baseValue is List<short>) return ((ShortList)argsList[0]).ToQueryString(Convert.ToString(argsList[1]));
                            if (baseValue is List<int>) return ((IntList)argsList[0]).ToQueryString(Convert.ToString(argsList[1]));
                            if (baseValue is List<long>) return ((LongList)argsList[0]).ToQueryString(Convert.ToString(argsList[1]));
                            if (baseValue is List<Guid>) return ((GuidList)argsList[0]).ToQueryString(Convert.ToString(argsList[1]));
                            if (baseValue is List<string>) return ((StringList)argsList[0]).ToQueryString(Convert.ToString(argsList[1]));
                        }
                        else if (argsList.Count == 3)
                        {
                            if (baseValue is List<DateTime>) return ((DateTimeList)argsList[0]).ToQueryString(Convert.ToString(argsList[1]), Convert.ToString(argsList[2]));
                            if (baseValue is List<TimeSpan>) return ((TimeSpanList)argsList[0]).ToQueryString(Convert.ToString(argsList[1]), Convert.ToString(argsList[2]));
                        }
                        else ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "ToQueryString", (argsList.Count - 1).ToString());
                        break;
                }
            }
            #endregion

            #region type DateTime
            if (baseValue is DateTime)
            {
                switch ((StiMethodType)name)
                {
                    case StiMethodType.AddDays:
                        category = get_category(argsList[1]);
                        if (category < 2 || category > 7) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "AddDays", "1", GetTypeName(argsList[1]), "double");
                        if (argsList.Count == 2) return ((DateTime)argsList[0]).AddDays(Convert.ToDouble(argsList[1]));
                        ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "AddDays", (argsList.Count - 1).ToString());
                        break;

                    case StiMethodType.AddHours:
                        category = get_category(argsList[1]);
                        if (category < 2 || category > 7) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "AddHours", "1", GetTypeName(argsList[1]), "double");
                        if (argsList.Count == 2) return ((DateTime)argsList[0]).AddHours(Convert.ToDouble(argsList[1]));
                        ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "AddHours", (argsList.Count - 1).ToString());
                        break;

                    case StiMethodType.AddMilliseconds:
                        category = get_category(argsList[1]);
                        if (category < 2 || category > 7) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "AddMilliseconds", "1", GetTypeName(argsList[1]), "double");
                        if (argsList.Count == 2) return ((DateTime)argsList[0]).AddMilliseconds(Convert.ToDouble(argsList[1]));
                        ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "AddMilliseconds", (argsList.Count - 1).ToString());
                        break;

                    case StiMethodType.AddMinutes:
                        category = get_category(argsList[1]);
                        if (category < 2 || category > 7) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "AddMinutes", "1", GetTypeName(argsList[1]), "double");
                        if (argsList.Count == 2) return ((DateTime)argsList[0]).AddMinutes(Convert.ToDouble(argsList[1]));
                        ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "AddMinutes", (argsList.Count - 1).ToString());
                        break;

                    case StiMethodType.AddMonths:
                        category = get_category(argsList[1]);
                        if (category < 4 || category > 7) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "AddMonths", "1", GetTypeName(argsList[1]), "int");
                        if (argsList.Count == 2) return ((DateTime)argsList[0]).AddMonths(Convert.ToInt32(argsList[1]));
                        ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "AddMonths", (argsList.Count - 1).ToString());
                        break;

                    case StiMethodType.AddSeconds:
                        category = get_category(argsList[1]);
                        if (category < 2 || category > 7) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "AddSeconds", "1", GetTypeName(argsList[1]), "double");
                        if (argsList.Count == 2) return ((DateTime)argsList[0]).AddSeconds(Convert.ToDouble(argsList[1]));
                        ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "AddSeconds", (argsList.Count - 1).ToString());
                        break;

                    case StiMethodType.AddYears:
                        category = get_category(argsList[1]);
                        if (category < 4 || category > 7) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "AddYears", "1", GetTypeName(argsList[1]), "int");
                        if (argsList.Count == 2) return ((DateTime)argsList[0]).AddYears(Convert.ToInt32(argsList[1]));
                        ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "AddYears", (argsList.Count - 1).ToString());
                        break;
                }
            }
            #endregion

            string message1 = (baseValue == null) ? "null" : GetTypeName(argsList[0]);
            ThrowError(ParserErrorCode.ItemDoesNotContainDefinition, message1, Enum.GetName(typeof(StiMethodType), name));

            return null;
        }
        #endregion    
    }
}
