using System;
using System.Collections;
using System.Collections.Generic;
using Proryv.Servers.Calculation.Parser.Internal.Functions;

namespace Proryv.Servers.Calculation.Parser.Internal
{
    public partial class ProryvParser
    {
        #region Evaluate the value of the function.
        private object call_func(object name, ArrayList argsList)
        {
            int category;
            int category2;
            ProryvFunctionType functionType = (ProryvFunctionType)name;
            int overload = CheckParserMethodInfo(functionType, argsList);
            switch (functionType)
            {
                #region Math
                case ProryvFunctionType.Abs:
                    category = get_category(argsList[0]);
                    if (category <= 1 || category >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Abs", "1", GetTypeName(argsList[0]), "double");
                    }
                    else if (category == 2) return Math.Abs(Convert.ToDecimal(argsList[0]));
                    else if (category == 3) return Math.Abs(Convert.ToDouble(argsList[0]));
                    return Math.Abs(Convert.ToInt32(argsList[0]));

                case ProryvFunctionType.Acos:
                    category = get_category(argsList[0]);
                    if (category <= 1 || category >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Acos", "1", GetTypeName(argsList[0]), "double");
                    }
                    return Math.Acos(Convert.ToDouble(argsList[0]));

                case ProryvFunctionType.Asin:
                    category = get_category(argsList[0]);
                    if (category <= 1 || category >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Asin", "1", GetTypeName(argsList[0]), "double");
                    }
                    return Math.Asin(Convert.ToDouble(argsList[0]));

                case ProryvFunctionType.Atan:
                    category = get_category(argsList[0]);
                    if (category <= 1 || category >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Atan", "1", GetTypeName(argsList[0]), "double");
                    }
                    return Math.Atan(Convert.ToDouble(argsList[0]));

                case ProryvFunctionType.Ceiling:
                    category = get_category(argsList[0]);
                    if (category <= 1 || category >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Ceiling", "1", GetTypeName(argsList[0]), "double");
                    }
                    else if (category == 2) return Math.Ceiling(Convert.ToDecimal(argsList[0]));
                    return Math.Ceiling(Convert.ToDouble(argsList[0]));

                case ProryvFunctionType.Cos:
                    category = get_category(argsList[0]);
                    if (category <= 1 || category >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Cos", "1", GetTypeName(argsList[0]), "double");
                    }
                    return Math.Cos(Convert.ToDouble(argsList[0]));

                case ProryvFunctionType.Div:
                    category = get_category(argsList[0]);
                    category2 = get_category(argsList[1]);
                    if (category <= 1 || category >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Div", "1", GetTypeName(argsList[0]), "double");
                    }
                    else if (category2 <= 1 || category2 >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Div", "2", GetTypeName(argsList[1]), "double");
                    }
                    else if (argsList.Count == 3)
                    {
                        if (category == 2)
                        {
                            if (Convert.ToDecimal(argsList[1]) == 0) return Convert.ToDecimal(argsList[2]);
                            return Convert.ToDecimal(argsList[0]) / Convert.ToDecimal(argsList[1]);
                        }
                        else
                        {
                            if (Convert.ToDouble(argsList[1]) == 0) return Convert.ToDouble(argsList[2]);
                            return Convert.ToDouble(argsList[0]) / Convert.ToDouble(argsList[1]);
                        }
                    }
                    else if (argsList.Count == 2)
                    {
                        if (category == 2) return Convert.ToDecimal(argsList[0]) / Convert.ToDecimal(argsList[1]);
                        else return Convert.ToDouble(argsList[0]) / Convert.ToDouble(argsList[1]);
                    }
                    ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Div", argsList.Count.ToString());
                    break;

                case ProryvFunctionType.Exp:
                    category = get_category(argsList[0]);
                    if (category <= 1 || category >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Exp", "1", GetTypeName(argsList[0]), "double");
                    }
                    return Math.Exp(Convert.ToDouble(argsList[0]));

                case ProryvFunctionType.Floor:
                    category = get_category(argsList[0]);
                    if (category <= 1 || category >= 4)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Floor", "1", GetTypeName(argsList[0]), "double");
                    }
                    if (category == 2) return Math.Floor(Convert.ToDecimal(argsList[0]));
                    return Math.Floor(Convert.ToDouble(argsList[0]));

                case ProryvFunctionType.Log:
                    category = get_category(argsList[0]);
                    if (category <= 1 || category >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Log", "1", GetTypeName(argsList[0]), "double");
                    }
                    return Math.Log(Convert.ToDouble(argsList[0]));

                case ProryvFunctionType.Maximum:
                    if (argsList.Count != 2) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Maximum", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    category2 = get_category(argsList[1]);
                    if (category <= 1 || category >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Maximum", "1", GetTypeName(argsList[0]), "double");
                    }
                    else if (category2 <= 1 || category2 >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Maximum", "2", GetTypeName(argsList[1]), "double");
                    }
                    else if (category == 2) return Math.Max(Convert.ToDecimal(argsList[0]), Convert.ToDecimal(argsList[1]));
                    else if (category == 3) return Math.Max(Convert.ToDouble(argsList[0]), Convert.ToDouble(argsList[1]));
                    return Math.Max(Convert.ToInt64(argsList[0]), Convert.ToInt64(argsList[1]));

                case ProryvFunctionType.Minimum:
                    if (argsList.Count != 2) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Minimum", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    category2 = get_category(argsList[1]);
                    if (category <= 1 || category >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Minimum", "1", GetTypeName(argsList[0]), "double");
                    }
                    else if (category2 <= 1 || category2 >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Minimum", "2", GetTypeName(argsList[1]), "double");
                    }
                    else if (category == 2) return Math.Min(Convert.ToDecimal(argsList[0]), Convert.ToDecimal(argsList[1]));
                    else if (category == 3) return Math.Min(Convert.ToDouble(argsList[0]), Convert.ToDouble(argsList[1]));
                    return Math.Min(Convert.ToInt64(argsList[0]), Convert.ToInt64(argsList[1]));

                case ProryvFunctionType.Round:
                    category = get_category(argsList[0]);
                    if (category <= 1 || category >= 4)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Round", "1", GetTypeName(argsList[0]), "double");
                    }
                    else if (argsList.Count == 1)
                    {
                        if (category == 2) return Math.Round(Convert.ToDecimal(argsList[0]), MidpointRounding.AwayFromZero);
                        else return Math.Round(Convert.ToDouble(argsList[0]), MidpointRounding.AwayFromZero);
                    }
                    else if (argsList.Count == 2)
                    {
                        category2 = get_category(argsList[1]);
                        if (category2 < 4 || category2 > 7)
                        {
                            ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Round", "2", GetTypeName(argsList[1]), "int");
                        }
                        if (category == 2) return Math.Round(Convert.ToDecimal(argsList[0]), Convert.ToInt32(argsList[1]), MidpointRounding.AwayFromZero);
                        else return Math.Round(Convert.ToDouble(argsList[0]), Convert.ToInt32(argsList[1]), MidpointRounding.AwayFromZero);
                    }
                    ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Round", argsList.Count.ToString());
                    break;

                case ProryvFunctionType.Sign:
                    category = get_category(argsList[0]);
                    if (category <= 1 || category >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Sign", "1", GetTypeName(argsList[0]), "double");
                    }
                    else if (category == 2) return Math.Sign(Convert.ToDecimal(argsList[0]));
                    else if (category == 3) return Math.Sign(Convert.ToDouble(argsList[0]));
                    return Math.Sign(Convert.ToInt32(argsList[0]));

                case ProryvFunctionType.Sin:
                    category = get_category(argsList[0]);
                    if (category <= 1 || category >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Sin", "1", GetTypeName(argsList[0]), "double");
                    }
                    return Math.Sin(Convert.ToDouble(argsList[0]));

                case ProryvFunctionType.Sqrt:
                    category = get_category(argsList[0]);
                    if (category <= 1 || category >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Sqrt", "1", GetTypeName(argsList[0]), "double");
                    }
                    return Math.Sqrt(Convert.ToDouble(argsList[0]));

                case ProryvFunctionType.Power:
                    if (argsList.Count != 2) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Power", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    category2 = get_category(argsList[1]);
                    if (category <= 2 || category >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Power", "1", GetTypeName(argsList[0]), "double");
                    }
                    else if (category2 <= 2 || category2 >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Power", "2", GetTypeName(argsList[1]), "double");
                    }
                    else if (category == 3) return Math.Pow(Convert.ToDouble(argsList[0]), Convert.ToDouble(argsList[1]));
                    return Math.Pow(Convert.ToInt64(argsList[0]), Convert.ToInt64(argsList[1]));

                case ProryvFunctionType.Tan:
                    category = get_category(argsList[0]);
                    if (category <= 1 || category >= 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Tan", "1", GetTypeName(argsList[0]), "double");
                    }
                    return Math.Tan(Convert.ToDouble(argsList[0]));

                case ProryvFunctionType.Truncate:
                    category = get_category(argsList[0]);
                    if (category <= 1 || category >= 4)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Truncate", "1", GetTypeName(argsList[0]), "double");
                    }
                    else if (category == 2) return Math.Truncate(Convert.ToDecimal(argsList[0]));
                    return Math.Truncate(Convert.ToDouble(argsList[0]));
                #endregion

                #region Date
                case ProryvFunctionType.DateDiff:
                    if (argsList.Count != 2) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "DateDiff", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    category2 = get_category(argsList[1]);
                    if (category != 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "DateDiff", "1", GetTypeName(argsList[0]), "DateTime");
                    }
                    else if (category2 != 8)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "DateDiff", "2", GetTypeName(argsList[1]), "DateTime");
                    }
                    else return Convert.ToDateTime(argsList[0]).Subtract(Convert.ToDateTime(argsList[1]));
                    break;

                case ProryvFunctionType.DateSerial:
                    if (argsList.Count != 3) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "DateSerial", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category < 4 || category > 7) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "DateSerial", "1", GetTypeName(argsList[0]), "int");
                    category = get_category(argsList[1]);
                    if (category < 4 || category > 7) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "DateSerial", "2", GetTypeName(argsList[0]), "int");
                    category = get_category(argsList[2]);
                    if (category < 4 || category > 7) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "DateSerial", "3", GetTypeName(argsList[0]), "int");
                    return new DateTime(Convert.ToInt32(argsList[0]), Convert.ToInt32(argsList[1]), Convert.ToInt32(argsList[2]));

                case ProryvFunctionType.Day:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Day", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 8) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Day", "1", GetTypeName(argsList[0]), "DateTime");
                    return Convert.ToDateTime(argsList[0]).Day;

                case ProryvFunctionType.DayOfWeek:
                    switch (overload)
                    {
                        case 1: return ProryvFunctionsDate.DayOfWeek(Convert.ToDateTime(argsList[0]));
                        case 3: return ProryvFunctionsDate.DayOfWeek(Convert.ToDateTime(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 5: return ProryvFunctionsDate.DayOfWeek(Convert.ToDateTime(argsList[0]), Convert.ToString(argsList[1]));
                        case 7: return ProryvFunctionsDate.DayOfWeek(Convert.ToDateTime(argsList[0]), Convert.ToString(argsList[1]), Convert.ToBoolean(argsList[2]));
                        case 2: return ProryvFunctionsDate.DayOfWeek((DateTime?)(argsList[0]));
                        case 4: return ProryvFunctionsDate.DayOfWeek((DateTime?)(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 6: return ProryvFunctionsDate.DayOfWeek((DateTime?)(argsList[0]), Convert.ToString(argsList[1]));
                        case 8: return ProryvFunctionsDate.DayOfWeek((DateTime?)(argsList[0]), Convert.ToString(argsList[1]), Convert.ToBoolean(argsList[2]));
                    }
                    break;

                case ProryvFunctionType.MonthName:
                    switch (overload)
                    {
                        case 1: return ProryvFunctionsDate.MonthName(Convert.ToDateTime(argsList[0]));
                        case 3: return ProryvFunctionsDate.MonthName(Convert.ToDateTime(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 5: return ProryvFunctionsDate.MonthName(Convert.ToDateTime(argsList[0]), Convert.ToString(argsList[1]));
                        case 7: return ProryvFunctionsDate.MonthName(Convert.ToDateTime(argsList[0]), Convert.ToString(argsList[1]), Convert.ToBoolean(argsList[2]));
                        case 2: return ProryvFunctionsDate.MonthName((DateTime?)(argsList[0]));
                        case 4: return ProryvFunctionsDate.MonthName((DateTime?)(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 6: return ProryvFunctionsDate.MonthName((DateTime?)(argsList[0]), Convert.ToString(argsList[1]));
                        case 8: return ProryvFunctionsDate.MonthName((DateTime?)(argsList[0]), Convert.ToString(argsList[1]), Convert.ToBoolean(argsList[2]));
                    }
                    break;

                case ProryvFunctionType.DayOfYear:
                    category = get_category(argsList[0]);
                    if (category != 8) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "DayOfYear", "1", GetTypeName(argsList[0]), "DateTime");
                    if (argsList.Count == 1) return Convert.ToDateTime(argsList[0]).DayOfYear.ToString();
                    ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "DayOfYear", argsList.Count.ToString());
                    break;

                case ProryvFunctionType.DaysInMonth:
                    switch (overload)
                    {
                        case 1: return (long)DateTime.DaysInMonth(Convert.ToDateTime(argsList[0]).Year, Convert.ToDateTime(argsList[0]).Month);
                        case 3: return (long)DateTime.DaysInMonth(Convert.ToInt32(argsList[0]), Convert.ToInt32(argsList[1]));
                        case 2: return (long)DateTime.DaysInMonth(((DateTime?)argsList[0]).Value.Year, Convert.ToDateTime(argsList[0]).Month);
                    }
                    break;

                case ProryvFunctionType.DaysInYear:
                    switch (overload)
                    {
                        case 1: return (long)(DateTime.IsLeapYear(Convert.ToDateTime(argsList[0]).Year) ? 366 : 365);
                        case 3: return (long)(DateTime.IsLeapYear(Convert.ToInt32(argsList[0])) ? 366 : 365);
                        case 2: return (long)(DateTime.IsLeapYear(((DateTime?)argsList[0]).Value.Year) ? 366 : 365);
                    }
                    break;

                case ProryvFunctionType.Hour:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Hour", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 8) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Hour", "1", GetTypeName(argsList[0]), "DateTime");
                    return Convert.ToDateTime(argsList[0]).Hour;

                case ProryvFunctionType.Minute:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Minute", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 8) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Minute", "1", GetTypeName(argsList[0]), "DateTime");
                    return Convert.ToDateTime(argsList[0]).Minute;

                case ProryvFunctionType.Month:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Month", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 8) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Month", "1", GetTypeName(argsList[0]), "DateTime");
                    return Convert.ToDateTime(argsList[0]).Month;

                case ProryvFunctionType.Second:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Second", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 8) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Second", "1", GetTypeName(argsList[0]), "DateTime");
                    return Convert.ToDateTime(argsList[0]).Second;

                case ProryvFunctionType.TimeSerial:
                    if (argsList.Count != 3) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "TimeSerial", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category < 4 || category > 7) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "TimeSerial", "1", GetTypeName(argsList[0]), "int");
                    category = get_category(argsList[1]);
                    if (category < 4 || category > 7) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "TimeSerial", "2", GetTypeName(argsList[0]), "int");
                    category = get_category(argsList[2]);
                    if (category < 4 || category > 7) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "TimeSerial", "3", GetTypeName(argsList[0]), "int");
                    return new TimeSpan(Convert.ToInt32(argsList[0]), Convert.ToInt32(argsList[1]), Convert.ToInt32(argsList[2]));

                case ProryvFunctionType.Year:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Year", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 8) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Year", "1", GetTypeName(argsList[0]), "DateTime");
                    return Convert.ToDateTime(argsList[0]).Year;
                #endregion

                #region Strings
                case ProryvFunctionType.Insert:
                    if (argsList.Count != 3) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Insert", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 1)
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Insert", "1", GetTypeName(argsList[0]), "string");
                    category = get_category(argsList[1]);
                    if (category < 4 || category > 7)
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Insert", "2", GetTypeName(argsList[0]), "int");
                    category = get_category(argsList[2]);
                    if (category != 1)
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Insert", "3", GetTypeName(argsList[0]), "string");
                    return Convert.ToString(argsList[0]).Insert(Convert.ToInt32(argsList[1]), Convert.ToString(argsList[2]));

                case ProryvFunctionType.Length:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Length", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 1) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Length", "1", GetTypeName(argsList[0]), "string");
                    return Convert.ToString(argsList[0]).Length;

                case ProryvFunctionType.Remove:
                    if (argsList.Count != 3) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Remove", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 1)
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Remove", "1", GetTypeName(argsList[0]), "string");
                    category = get_category(argsList[1]);
                    if (category < 4 || category > 7)
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Remove", "2", GetTypeName(argsList[0]), "int");
                    category = get_category(argsList[2]);
                    if (category < 4 || category > 7)
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Remove", "3", GetTypeName(argsList[0]), "int");
                    return Convert.ToString(argsList[0]).Remove(Convert.ToInt32(argsList[1]), Convert.ToInt32(argsList[2]));

                case ProryvFunctionType.Replace:
                    if (argsList.Count != 3) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Replace", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 1)
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Replace", "1", GetTypeName(argsList[0]), "string");
                    category = get_category(argsList[1]);
                    if (category != 1)
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Replace", "2", GetTypeName(argsList[0]), "string");
                    category = get_category(argsList[2]);
                    if (category != 1)
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Replace", "3", GetTypeName(argsList[0]), "string");
                    return Convert.ToString(argsList[0]).Replace(Convert.ToString(argsList[1]), Convert.ToString(argsList[2]));

                case ProryvFunctionType.Roman:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Roman", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category >= 4 && category <= 7) return ProryvFunctionsStrings.Roman(Convert.ToInt32(argsList[0]));
                    ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Roman", "1", GetTypeName(argsList[0]), "int");
                    break;

                case ProryvFunctionType.Substring:
                    if (argsList.Count != 3) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Substring", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 1)
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Substring", "1", GetTypeName(argsList[0]), "string");
                    category = get_category(argsList[1]);
                    if (category < 4 || category > 7)
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Substring", "2", GetTypeName(argsList[0]), "int");
                    category = get_category(argsList[2]);
                    if (category < 4 || category > 7)
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Substring", "3", GetTypeName(argsList[0]), "int");
                    return Convert.ToString(argsList[0]).Substring(Convert.ToInt32(argsList[1]), Convert.ToInt32(argsList[2]));

                case ProryvFunctionType.ToLowerCase:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "ToLowerCase", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 1) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "ToLowerCase", "1", GetTypeName(argsList[0]), "string");
                    return Convert.ToString(argsList[0]).ToLowerInvariant();

                case ProryvFunctionType.ToProperCase:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "ToProperCase", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 1) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "ToProperCase", "1", GetTypeName(argsList[0]), "string");
                    return System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(Convert.ToString(argsList[0]).ToLowerInvariant());

                case ProryvFunctionType.ToUpperCase:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "ToUpperCase", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 1) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "ToUpperCase", "1", GetTypeName(argsList[0]), "string");
                    return Convert.ToString(argsList[0]).ToUpperInvariant();

                case ProryvFunctionType.Trim:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Trim", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 1) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Trim", "1", GetTypeName(argsList[0]), "string");
                    return Convert.ToString(argsList[0]).Trim();

                case ProryvFunctionType.TryParseDecimal:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "TryParseDecimal", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 1) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "TryParseDecimal", "1", GetTypeName(argsList[0]), "string");
                    decimal tempTryParseDecimal = 0;
                    return decimal.TryParse(Convert.ToString(argsList[0]), out tempTryParseDecimal);

                case ProryvFunctionType.TryParseDouble:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "TryParseDouble", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 1) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "TryParseDouble", "1", GetTypeName(argsList[0]), "string");
                    double tempTryParseDouble = 0;
                    return double.TryParse(Convert.ToString(argsList[0]), out tempTryParseDouble);

                case ProryvFunctionType.TryParseLong:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "TryParseLong", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 1) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "TryParseLong", "1", GetTypeName(argsList[0]), "string");
                    long tempTryParseLong = 0;
                    return long.TryParse(Convert.ToString(argsList[0]), out tempTryParseLong);

                case ProryvFunctionType.Arabic:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Arabic", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category == 1) return ProryvFunctionsStrings.Arabic(Convert.ToString(argsList[0]));
                    if (category >= 4 && category <= 7) return ProryvFunctionsStrings.Arabic(Convert.ToInt32(argsList[0]));
                    ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Arabic", "1", GetTypeName(argsList[0]), "string");
                    break;

                case ProryvFunctionType.Persian:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Persian", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category == 1) return ProryvFunctionsStrings.Persian(Convert.ToString(argsList[0]));
                    if (category >= 4 && category <= 7) return ProryvFunctionsStrings.Persian(Convert.ToInt32(argsList[0]));
                    ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Persian", "1", GetTypeName(argsList[0]), "string");
                    break;

                case ProryvFunctionType.ToOrdinal:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "ToOrdinal", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category >= 4 && category <= 7) return ProryvFunctionsStrings.ToOrdinal(Convert.ToInt32(argsList[0]));
                    ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "ToOrdinal", "1", GetTypeName(argsList[0]), "int");
                    break;

                case ProryvFunctionType.Left:
                    if (overload == 1) return ProryvFunctionsStrings.Left(Convert.ToString(argsList[0]), Convert.ToInt32(argsList[1]));
                    break;
                case ProryvFunctionType.Right:
                    if (overload == 1) return ProryvFunctionsStrings.Right(Convert.ToString(argsList[0]), Convert.ToInt32(argsList[1]));
                    break;
                case ProryvFunctionType.Mid:
                    if (overload == 1) return ProryvFunctionsStrings.Mid(Convert.ToString(argsList[0]), Convert.ToInt32(argsList[1]), Convert.ToInt32(argsList[2]));
                    break;

                case ProryvFunctionType.ToWords:
                    switch (overload)
                    {
                        case 1: return ProryvFunctionsStrings.ToWords(Convert.ToInt64(argsList[0]));
                        case 2: return ProryvFunctionsStrings.ToWords(Convert.ToDecimal(argsList[0]));
                        case 3: return ProryvFunctionsStrings.ToWords(Convert.ToDouble(argsList[0]));
                        case 4: return ProryvFunctionsStrings.ToWords(Convert.ToInt64(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 5: return ProryvFunctionsStrings.ToWords(Convert.ToDecimal(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 6: return ProryvFunctionsStrings.ToWords(Convert.ToDouble(argsList[0]), Convert.ToBoolean(argsList[1]));
                    }
                    break;
                case ProryvFunctionType.ToWordsEs:
                    switch (overload)
                    {
                        case 1: return ProryvFunctionsStrings.ToWordsEs(Convert.ToInt64(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 2: return ProryvFunctionsStrings.ToWordsEs(Convert.ToInt64(argsList[0]), Convert.ToBoolean(argsList[1]), Convert.ToBoolean(argsList[2]));
                    }
                    break;
                case ProryvFunctionType.ToWordsEnIn: return ProryvFunctionsStrings.ToWordsEnIn(Convert.ToInt64(argsList[0]), Convert.ToBoolean(argsList[1]));
                case ProryvFunctionType.ToWordsFa: return ProryvFunctionsStrings.ToWordsFa(Convert.ToInt64(argsList[0]));
                case ProryvFunctionType.ToWordsPl: return ProryvFunctionsStrings.ToWordsPl(Convert.ToInt64(argsList[0]), Convert.ToBoolean(argsList[1]));
                case ProryvFunctionType.ToWordsPt: return ProryvFunctionsStrings.ToWordsPt(Convert.ToInt64(argsList[0]), Convert.ToBoolean(argsList[1]));
                case ProryvFunctionType.ToWordsRu:
                    switch (overload)
                    {
                        case 1: return ProryvFunctionsStrings.ToWordsRu(Convert.ToInt64(argsList[0]));
                        case 2: return ProryvFunctionsStrings.ToWordsRu(Convert.ToDecimal(argsList[0]));
                        case 3: return ProryvFunctionsStrings.ToWordsRu(Convert.ToDouble(argsList[0]));
                        case 4: return ProryvFunctionsStrings.ToWordsRu(Convert.ToInt64(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 5: return ProryvFunctionsStrings.ToWordsRu(Convert.ToDecimal(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 6: return ProryvFunctionsStrings.ToWordsRu(Convert.ToDouble(argsList[0]), Convert.ToBoolean(argsList[1]));
                    }
                    break;
                case ProryvFunctionType.ToWordsUa:
                    switch (overload)
                    {
                        case 1: return ProryvFunctionsStrings.ToWordsUa(Convert.ToInt64(argsList[0]));
                        case 2: return ProryvFunctionsStrings.ToWordsUa(Convert.ToDecimal(argsList[0]));
                        case 3: return ProryvFunctionsStrings.ToWordsUa(Convert.ToDouble(argsList[0]));
                        case 4: return ProryvFunctionsStrings.ToWordsUa(Convert.ToInt64(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 5: return ProryvFunctionsStrings.ToWordsUa(Convert.ToDecimal(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 6: return ProryvFunctionsStrings.ToWordsUa(Convert.ToDouble(argsList[0]), Convert.ToBoolean(argsList[1]));
                    }
                    break;

                case ProryvFunctionType.ToCurrencyWords:
                    switch (overload)
                    {
                        case 1: return ProryvFunctionsStrings.ToCurrencyWords(Convert.ToInt64(argsList[0]));
                        case 2: return ProryvFunctionsStrings.ToCurrencyWords(Convert.ToDouble(argsList[0]));
                        case 3: return ProryvFunctionsStrings.ToCurrencyWords(Convert.ToDecimal(argsList[0]));
                        case 4: return ProryvFunctionsStrings.ToCurrencyWords(Convert.ToInt64(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 5: return ProryvFunctionsStrings.ToCurrencyWords(Convert.ToDouble(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 6: return ProryvFunctionsStrings.ToCurrencyWords(Convert.ToDecimal(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 7: return ProryvFunctionsStrings.ToCurrencyWords(Convert.ToInt64(argsList[0]), Convert.ToBoolean(argsList[1]), Convert.ToBoolean(argsList[2]));
                        case 8: return ProryvFunctionsStrings.ToCurrencyWords(Convert.ToDouble(argsList[0]), Convert.ToBoolean(argsList[1]), Convert.ToBoolean(argsList[2]));
                        case 9: return ProryvFunctionsStrings.ToCurrencyWords(Convert.ToDecimal(argsList[0]), Convert.ToBoolean(argsList[1]), Convert.ToBoolean(argsList[2]));
                        case 10: return ProryvFunctionsStrings.ToCurrencyWords(Convert.ToDouble(argsList[0]), Convert.ToBoolean(argsList[1]), Convert.ToBoolean(argsList[2]), Convert.ToString(argsList[3]), Convert.ToString(argsList[4]));
                    }
                    break;
                case ProryvFunctionType.ToCurrencyWordsEnGb: return ProryvFunctionsStrings.ToCurrencyWordsEnGb(Convert.ToDecimal(argsList[0]), Convert.ToString(argsList[1]), Convert.ToInt32(argsList[2]));
                case ProryvFunctionType.ToCurrencyWordsEnIn: return ProryvFunctionsStrings.ToCurrencyWordsEnIn(Convert.ToString(argsList[0]), Convert.ToString(argsList[1]), Convert.ToDecimal(argsList[2]), Convert.ToInt32(argsList[3]), Convert.ToBoolean(argsList[4]));
                case ProryvFunctionType.ToCurrencyWordsEs: return ProryvFunctionsStrings.ToCurrencyWordsEs(Convert.ToDecimal(argsList[0]), Convert.ToString(argsList[1]), Convert.ToInt32(argsList[2]));
                case ProryvFunctionType.ToCurrencyWordsFr: return ProryvFunctionsStrings.ToCurrencyWordsFr(Convert.ToDecimal(argsList[0]), Convert.ToString(argsList[1]), Convert.ToInt32(argsList[2]));
                case ProryvFunctionType.ToCurrencyWordsNl: return ProryvFunctionsStrings.ToCurrencyWordsNl(Convert.ToDecimal(argsList[0]), Convert.ToString(argsList[1]), Convert.ToInt32(argsList[2]));
                case ProryvFunctionType.ToCurrencyWordsPl: return ProryvFunctionsStrings.ToCurrencyWordsPl(Convert.ToDecimal(argsList[0]), Convert.ToString(argsList[1]), Convert.ToBoolean(argsList[2]), Convert.ToBoolean(argsList[3]));
                case ProryvFunctionType.ToCurrencyWordsPt: return ProryvFunctionsStrings.ToCurrencyWordsPt(Convert.ToDecimal(argsList[0]), Convert.ToBoolean(argsList[1]), Convert.ToBoolean(argsList[2]));
                case ProryvFunctionType.ToCurrencyWordsPtBr: return ProryvFunctionsStrings.ToCurrencyWordsPtBr(Convert.ToDecimal(argsList[0]));
                case ProryvFunctionType.ToCurrencyWordsRu:
                    switch (overload)
                    {
                        case 1: return ProryvFunctionsStrings.ToCurrencyWordsRu(Convert.ToInt64(argsList[0]));
                        case 2: return ProryvFunctionsStrings.ToCurrencyWordsRu(Convert.ToDouble(argsList[0]));
                        case 3: return ProryvFunctionsStrings.ToCurrencyWordsRu(Convert.ToDecimal(argsList[0]));
                        case 4: return ProryvFunctionsStrings.ToCurrencyWordsRu(Convert.ToInt64(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 5: return ProryvFunctionsStrings.ToCurrencyWordsRu(Convert.ToDouble(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 6: return ProryvFunctionsStrings.ToCurrencyWordsRu(Convert.ToDecimal(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 7: return ProryvFunctionsStrings.ToCurrencyWordsRu(Convert.ToInt64(argsList[0]), Convert.ToBoolean(argsList[1]), Convert.ToString(argsList[2]));
                        case 8: return ProryvFunctionsStrings.ToCurrencyWordsRu(Convert.ToDouble(argsList[0]), Convert.ToBoolean(argsList[1]), Convert.ToString(argsList[2]));
                        case 9: return ProryvFunctionsStrings.ToCurrencyWordsRu(Convert.ToDecimal(argsList[0]), Convert.ToBoolean(argsList[1]), Convert.ToString(argsList[2]));
                        case 10: return ProryvFunctionsStrings.ToCurrencyWordsRu(Convert.ToInt64(argsList[0]), Convert.ToString(argsList[1]), Convert.ToBoolean(argsList[2]));
                        case 11: return ProryvFunctionsStrings.ToCurrencyWordsRu(Convert.ToDouble(argsList[0]), Convert.ToString(argsList[1]), Convert.ToBoolean(argsList[2]));
                        case 12: return ProryvFunctionsStrings.ToCurrencyWordsRu(Convert.ToDecimal(argsList[0]), Convert.ToString(argsList[1]), Convert.ToBoolean(argsList[2]));
                    }
                    break;
                case ProryvFunctionType.ToCurrencyWordsThai:
                    switch (overload)
                    {
                        case 1: return ProryvFunctionsStrings.ToCurrencyWordsThai(Convert.ToInt64(argsList[0]));
                        case 2: return ProryvFunctionsStrings.ToCurrencyWordsThai(Convert.ToDouble(argsList[0]));
                        case 3: return ProryvFunctionsStrings.ToCurrencyWordsThai(Convert.ToDecimal(argsList[0]));
                    }
                    break;
                case ProryvFunctionType.ToCurrencyWordsUa:
                    switch (overload)
                    {
                        case 1: return ProryvFunctionsStrings.ToCurrencyWordsUa(Convert.ToInt64(argsList[0]));
                        case 2: return ProryvFunctionsStrings.ToCurrencyWordsUa(Convert.ToDouble(argsList[0]));
                        case 3: return ProryvFunctionsStrings.ToCurrencyWordsUa(Convert.ToDecimal(argsList[0]));
                        case 4: return ProryvFunctionsStrings.ToCurrencyWordsUa(Convert.ToInt64(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 5: return ProryvFunctionsStrings.ToCurrencyWordsUa(Convert.ToDouble(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 6: return ProryvFunctionsStrings.ToCurrencyWordsUa(Convert.ToDecimal(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 7: return ProryvFunctionsStrings.ToCurrencyWordsUa(Convert.ToInt64(argsList[0]), Convert.ToBoolean(argsList[1]), Convert.ToString(argsList[2]));
                        case 8: return ProryvFunctionsStrings.ToCurrencyWordsUa(Convert.ToDouble(argsList[0]), Convert.ToBoolean(argsList[1]), Convert.ToString(argsList[2]));
                        case 9: return ProryvFunctionsStrings.ToCurrencyWordsUa(Convert.ToDecimal(argsList[0]), Convert.ToBoolean(argsList[1]), Convert.ToString(argsList[2]));
                    }
                    break;

                case ProryvFunctionType.DateToStr:
                    switch (overload)
                    {
                        case 1: return ProryvFunctionsStrings.DateToStr(Convert.ToDateTime(argsList[0]));
                        case 3: return ProryvFunctionsStrings.DateToStr(Convert.ToDateTime(argsList[0]), Convert.ToBoolean(argsList[1]));
                        case 2: return ProryvFunctionsStrings.DateToStr((DateTime?)(argsList[0]));
                        case 4: return ProryvFunctionsStrings.DateToStr((DateTime?)(argsList[0]), Convert.ToBoolean(argsList[1]));
                    }
                    break;
                case ProryvFunctionType.DateToStrPl: return ProryvFunctionsStrings.DateToStrPl(Convert.ToDateTime(argsList[0]), Convert.ToBoolean(argsList[1]));
                case ProryvFunctionType.DateToStrRu:
                    switch (overload)
                    {
                        case 1: return ProryvFunctionsStrings.DateToStrRu(Convert.ToDateTime(argsList[0]));
                        case 2: return ProryvFunctionsStrings.DateToStrRu(Convert.ToDateTime(argsList[0]), Convert.ToBoolean(argsList[1]));
                    }
                    break;
                case ProryvFunctionType.DateToStrUa:
                    switch (overload)
                    {
                        case 1: return ProryvFunctionsStrings.DateToStrUa(Convert.ToDateTime(argsList[0]));
                        case 2: return ProryvFunctionsStrings.DateToStrUa(Convert.ToDateTime(argsList[0]), Convert.ToBoolean(argsList[1]));
                    }
                    break;
                case ProryvFunctionType.DateToStrPt: return ProryvFunctionsStrings.DateToStrPt(Convert.ToDateTime(argsList[0]));
                case ProryvFunctionType.DateToStrPtBr: return ProryvFunctionsStrings.DateToStrPtBr(Convert.ToDateTime(argsList[0]));

                case ProryvFunctionType.StringIsNullOrEmpty:
                    if (overload == 1) return string.IsNullOrEmpty(Convert.ToString(argsList[0]));
                    break;
                case ProryvFunctionType.StringIsNullOrWhiteSpace:
                    if (overload == 1) return string.IsNullOrWhiteSpace(Convert.ToString(argsList[0]));
                    break;
                #endregion

                #region Programming
                case ProryvFunctionType.IIF:
                    return Convert.ToBoolean(argsList[0]) ? argsList[1] : argsList[2];

                case ProryvFunctionType.Choose:
                    if (argsList.Count < 2) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Choose", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category < 4 || category > 7) ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Choose", "1", GetTypeName(argsList[0]), "int");
                    int chooseIndex = Convert.ToInt32(argsList[0]);
                    if (chooseIndex > 0 && chooseIndex < argsList.Count) return argsList[chooseIndex];
                    return null;

                case ProryvFunctionType.Switch:
                    if (argsList.Count < 2) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Switch", argsList.Count.ToString());
                    int switchIndex = 0;
                    while (switchIndex + 1 < argsList.Count)
                    {
                        if (Convert.ToBoolean(argsList[switchIndex])) return argsList[switchIndex + 1];
                        switchIndex += 2;
                    }
                    return null;
                #endregion

                #region ToString
                case ProryvFunctionType.ToString:
                    if (argsList[0] == null || argsList[0] == DBNull.Value) return string.Empty;
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
                    else if (category == 9)
                    {
                        return Convert.ToBoolean(argsList[0]).ToString();
                    }
                    else return argsList[0].ToString();
                #endregion

                #region Format
                case ProryvFunctionType.Format:
                    if (argsList.Count != 2) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "Format", argsList.Count.ToString());
                    category = get_category(argsList[0]);
                    if (category != 1)
                    {
                        ThrowError(ParserErrorCode.FunctionHasInvalidArgument, "Format", "1", GetTypeName(argsList[0]), "string");
                    }
                    return string.Format(Convert.ToString(argsList[0]), argsList[1]);
                #endregion

                #region System.Convert
                case ProryvFunctionType.SystemConvertToBoolean:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "System.Convert.ToBoolean", argsList.Count.ToString());
                    return System.Convert.ToBoolean(argsList[0]);

                case ProryvFunctionType.SystemConvertToByte:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "System.Convert.ToByte", argsList.Count.ToString());
                    return System.Convert.ToByte(argsList[0]);

                case ProryvFunctionType.SystemConvertToChar:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "System.Convert.ToChar", argsList.Count.ToString());
                    return System.Convert.ToChar(argsList[0]);

                case ProryvFunctionType.SystemConvertToDateTime:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "System.Convert.ToDateTime", argsList.Count.ToString());
                    return System.Convert.ToDateTime(argsList[0]);

                case ProryvFunctionType.SystemConvertToDecimal:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "System.Convert.ToDecimal", argsList.Count.ToString());
                    return System.Convert.ToDecimal(argsList[0]);

                case ProryvFunctionType.SystemConvertToDouble:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "System.Convert.ToDouble", argsList.Count.ToString());
                    return System.Convert.ToDouble(argsList[0]);

                case ProryvFunctionType.SystemConvertToInt16:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "System.Convert.ToInt16", argsList.Count.ToString());
                    return System.Convert.ToInt16(argsList[0]);

                case ProryvFunctionType.SystemConvertToInt32:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "System.Convert.ToInt32", argsList.Count.ToString());
                    return System.Convert.ToInt32(argsList[0]);

                case ProryvFunctionType.SystemConvertToInt64:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "System.Convert.ToInt64", argsList.Count.ToString());
                    return System.Convert.ToInt64(argsList[0]);

                case ProryvFunctionType.SystemConvertToSByte:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "System.Convert.ToSByte", argsList.Count.ToString());
                    return System.Convert.ToSByte(argsList[0]);

                case ProryvFunctionType.SystemConvertToSingle:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "System.Convert.ToSingle", argsList.Count.ToString());
                    return System.Convert.ToSingle(argsList[0]);

                case ProryvFunctionType.SystemConvertToString:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "System.Convert.ToString", argsList.Count.ToString());
                    return System.Convert.ToString(argsList[0]);

                case ProryvFunctionType.SystemConvertToUInt16:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "System.Convert.ToUInt16", argsList.Count.ToString());
                    return System.Convert.ToUInt16(argsList[0]);

                case ProryvFunctionType.SystemConvertToUInt32:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "System.Convert.ToUInt32", argsList.Count.ToString());
                    return System.Convert.ToUInt32(argsList[0]);

                case ProryvFunctionType.SystemConvertToUInt64:
                    if (argsList.Count != 1) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "System.Convert.ToUInt64", argsList.Count.ToString());
                    return System.Convert.ToUInt64(argsList[0]);
                #endregion

                #region Parse
                case ProryvFunctionType.ParseDateTime:
                    switch (overload)
                    {
                        case 1: return DateTime.Parse(Convert.ToString(argsList[0]));
                    }
                    break;
                case ProryvFunctionType.ParseDecimal:
                    switch (overload)
                    {
                        case 1: return Decimal.Parse(Convert.ToString(argsList[0]));
                    }
                    break;
                case ProryvFunctionType.ParseDouble:
                    switch (overload)
                    {
                        case 1: return Double.Parse(Convert.ToString(argsList[0]));
                    }
                    break;
                case ProryvFunctionType.ParseInt:
                    switch (overload)
                    {
                        case 1: return int.Parse(Convert.ToString(argsList[0]));
                    }
                    break;
                #endregion

                #region EngineHelper
                case ProryvFunctionType.EngineHelperToQueryString:
                    switch (overload)
                    {
                        case 1:
                            if (argsList[0] is IEnumerable)
                            {
                                try
                                {
                                    List<object> list = new List<object>();
                                    IEnumerator enumerator = ((IEnumerable)argsList[0]).GetEnumerator();
                                    while (enumerator.MoveNext())
                                    {
                                        list.Add(enumerator.Current);
                                    }
                                    return Func.EngineHelper.ToQueryString(list, Convert.ToString(argsList[1]), Convert.ToString(argsList[2]));
                                }
                                catch
                                {
                                }
                            }
                            break;
                    }
                    break;
                #endregion

            }

            if ((functionType >= ProryvFunctionType.rCount && functionType <= ProryvFunctionType.rLast) ||
                (functionType >= ProryvFunctionType.riCount && functionType <= ProryvFunctionType.riLast) ||
                (functionType >= ProryvFunctionType.cCount && functionType <= ProryvFunctionType.cLast) ||
                (functionType >= ProryvFunctionType.crCount && functionType <= ProryvFunctionType.crLast) ||
                (functionType >= ProryvFunctionType.ciCount && functionType <= ProryvFunctionType.ciLast) ||
                (functionType >= ProryvFunctionType.criCount && functionType <= ProryvFunctionType.criLast))
            {
                ThrowError(ParserErrorCode.FunctionNotYetImplemented, functionType.ToString());
            }


            if (functionType >= ProryvFunctionType.UserFunction)
            {
                //find function name
                string functionName = null;
                foreach (DictionaryEntry de in UserFunctionsList)
                {
                    if ((ProryvFunctionType)de.Value == functionType)
                    {
                        functionName = (string)de.Key;
                        break;
                    }
                }
                if (functionName != null)
                {
                    //prepare arrays
                    int count = argsList.Count;
                    Type[] types = new Type[count];
                    object[] args = new object[count];
                    for (int index = 0; index < count; index++)
                    {
                        if (argsList[index] == null)
                        {
                            types[index] = typeof(object);
                        }
                        else
                        {
                            types[index] = argsList[index].GetType();
                        }
                        args[index] = argsList[index];
                    }

                    //find function by name and arguments
                    var functions = ProryvFunctions.GetFunctions(false);
                    foreach (ProryvFunction func in functions)
                    {
                        if (func.FunctionName != functionName) continue;
                        if ((func.ArgumentTypes != null ? func.ArgumentTypes.Length : 0) != count) continue;

                        //check arguments type
                        //bool flag2 = true;
                        for (int index = 0; index < count; index++)
                        {
                            if (IsImplicitlyCastableTo(types[index], func.ArgumentTypes[index])) continue;
                            //flag2 = false;
                            break;
                        }
                        //if (flag2)
                        //{
                        //    var method = func.TypeOfFunction.GetMethod(func.FunctionName);
                        //    return method.Invoke(report, args);
                        //}
                    }
                }
            }

            return null;
        }
        #endregion
    }
}
