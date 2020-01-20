using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Proryv.Servers.Calculation.Parser.Internal.Functions;

namespace Proryv.Servers.Calculation.Parser.Internal
{
    public partial class ProryvParser
    {
        #region Objects

        private ISpreadsheetProperties _proryvSpreadsheetObject;

        /// <summary>
        /// Передаем объект вокруг которого и крутятся все парсеры
        /// </summary>
        /// <param name="proryvSpreadsheetObject"></param>
        public void SetProryvSpreadsheetObject(ISpreadsheetProperties proryvSpreadsheetObject)
        {
            _proryvSpreadsheetObject = proryvSpreadsheetObject;

            if (_proryvSpreadsheetObject == null || _proryvSpreadsheetObject.ProryvFunctionFactory == null) return;

            #region Свойства объекта

            lock (_proryvSpreadsheetPropertiesLock)
            {
                if (_proryvSpreadsheetProperties == null)
                {
                    _proryvSpreadsheetProperties = new Dictionary<string, PropertyInfo>();
                    _proryvFreeHierarchyBalanceSignature = new Dictionary<string, PropertyInfo>();
                    foreach (var property in typeof (ISpreadsheetProperties).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var name = property.Name.ToLowerInvariant();
                        _proryvSpreadsheetProperties.Add(name, property);
                    }

                    foreach (var property in typeof(IFreeHierarchyBalanceSignature).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var name = property.Name.ToLowerInvariant();
                        _proryvFreeHierarchyBalanceSignature.Add(name, property);
                    }
                }
            }

            #endregion

            #region Статичные процедуры

            if (_proryvSpreadsheetObject != null)
            {
                var classRef = _proryvSpreadsheetObject.ProryvFunctionFactory.GetType();
                foreach (var methodInfo in classRef.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                {
                    var returnType = methodInfo.ReturnType;
                    var parameterInfos = methodInfo.GetParameters();

                    var parTypes = new Type[parameterInfos.Length];
                    var parNames = new string[parameterInfos.Length];
                    var parDescs = new string[parameterInfos.Length];

                    if (parameterInfos.Length > 0)
                    {
                        for (var i = 0; i < parameterInfos.Length; i++)
                        {
                            var pi = parameterInfos[i];
                            parNames[i] = pi.Name;
                            parTypes[i] = pi.ParameterType;
                            parDescs[i] = pi.Name;
                        }
                    }

                    ProryvFunctions.AddFunction("Расчетные", null, methodInfo.Name, methodInfo.Name, classRef, returnType,
                        methodInfo.Name, parTypes, parNames, parDescs);
                }
            }

            #endregion
        }
        
        #endregion
        #region Properties
        private static ValuesDict<TypeCode> _typesList = null;
        private static ValuesDict<TypeCode> TypesList
        {
            get
            {
                if (_typesList == null)
                {
                    _typesList = new ValuesDict<TypeCode>();
                    _typesList["bool"] = TypeCode.Boolean;
                    _typesList["Boolean"] = TypeCode.Boolean;
                    _typesList["byte"] = TypeCode.Byte;
                    _typesList["Byte"] = TypeCode.Byte;
                    _typesList["sbyte"] = TypeCode.SByte;
                    _typesList["Sbyte"] = TypeCode.SByte;
                    _typesList["char"] = TypeCode.Char;
                    _typesList["Char"] = TypeCode.Char;
                    _typesList["decimal"] = TypeCode.Decimal;
                    _typesList["Decimal"] = TypeCode.Decimal;
                    _typesList["double"] = TypeCode.Double;
                    _typesList["Double"] = TypeCode.Double;
                    _typesList["float"] = TypeCode.Single;
                    _typesList["Single"] = TypeCode.Single;
                    _typesList["int"] = TypeCode.Int32;
                    _typesList["uint"] = TypeCode.UInt32;
                    _typesList["long"] = TypeCode.Int64;
                    _typesList["ulong"] = TypeCode.UInt64;
                    _typesList["short"] = TypeCode.Int16;
                    _typesList["Int16"] = TypeCode.Int16;
                    _typesList["Int32"] = TypeCode.Int32;
                    _typesList["Int64"] = TypeCode.Int64;
                    _typesList["ushort"] = TypeCode.UInt16;
                    _typesList["UInt16"] = TypeCode.UInt16;
                    _typesList["UInt32"] = TypeCode.UInt32;
                    _typesList["UInt64"] = TypeCode.UInt64;
                    _typesList["object"] = TypeCode.Object;
                    _typesList["string"] = TypeCode.String;
                    _typesList["String"] = TypeCode.String;
                    _typesList["DateTime"] = TypeCode.DateTime;
                    _typesList["DateTime"] = TypeCode.DateTime;
                }
                return _typesList;
            }
        }

        private static ValuesDict<ProryvSystemVariableType> _systemVariablesList = null;
        private static ValuesDict<ProryvSystemVariableType> SystemVariablesList
        {
            get
            {
                if (_systemVariablesList == null)
                {
                    _systemVariablesList = new ValuesDict<ProryvSystemVariableType>();
                    _systemVariablesList["Time"] = ProryvSystemVariableType.Time;
                    _systemVariablesList["Today"] = ProryvSystemVariableType.Today;
                    _systemVariablesList["Date"] = ProryvSystemVariableType.Today;
                    _systemVariablesList["value"] = ProryvSystemVariableType.ConditionValue;
                    _systemVariablesList["tag"] = ProryvSystemVariableType.ConditionTag;
                    _systemVariablesList["sender"] = ProryvSystemVariableType.Sender;
                    _systemVariablesList["DateTime"] = ProryvSystemVariableType.NameSpace;
                    _systemVariablesList["DateTime.Now"] = ProryvSystemVariableType.DateTimeNow;
                    _systemVariablesList["DateTime.Today"] = ProryvSystemVariableType.DateTimeToday;
                }
                return _systemVariablesList;
            }
        }

        private static ValuesDict<StiPropertyType> _propertiesList = null;
        private static ValuesDict<StiPropertyType> PropertiesList
        {
            get
            {
                if (_propertiesList == null)
                {
                    _propertiesList = new ValuesDict<StiPropertyType>();
                    _propertiesList["YEAR"] = StiPropertyType.Year;
                    _propertiesList["MONTH"] = StiPropertyType.Month;
                    _propertiesList["DAY"] = StiPropertyType.Day;
                    _propertiesList["HOUR"] = StiPropertyType.Hour;
                    _propertiesList["MINUTE"] = StiPropertyType.Minute;
                    _propertiesList["SECOND"] = StiPropertyType.Second;
                    _propertiesList["LENGTH"] = StiPropertyType.Length;
                    _propertiesList["FROM"] = StiPropertyType.From;
                    _propertiesList["TO"] = StiPropertyType.To;
                    _propertiesList["FROMDATE"] = StiPropertyType.FromDate;
                    _propertiesList["TODATE"] = StiPropertyType.ToDate;
                    _propertiesList["FROMTIME"] = StiPropertyType.FromTime;
                    _propertiesList["TOTIME"] = StiPropertyType.ToTime;
                    _propertiesList["SELECTEDLINE"] = StiPropertyType.SelectedLine;
                    _propertiesList["NAME"] = StiPropertyType.Name;
                    _propertiesList["TAGVALUE"] = StiPropertyType.TagValue;

                    _propertiesList["DAYS"] = StiPropertyType.Days;
                    _propertiesList["HOURS"] = StiPropertyType.Hours;
                    _propertiesList["MILLISECONDS"] = StiPropertyType.Milliseconds;
                    _propertiesList["MINUTES"] = StiPropertyType.Minutes;
                    _propertiesList["SECONDS"] = StiPropertyType.Seconds;
                    _propertiesList["TICKS"] = StiPropertyType.Ticks;

                    _propertiesList["COUNT"] = StiPropertyType.Count;
                }
                return _propertiesList;
            }
        }

        //Proryv
        private static HashSet<string> proryvPropertiesList;
        private static HashSet<string> ProryvPropertiesList
        {
            get
            {
                if (proryvPropertiesList != null) return proryvPropertiesList;

                proryvPropertiesList = new HashSet<string>
                {
                    "Значение",
                    "Время",
                    "Статус",
                    "Достоверность",
                    "СтатусСтрока",
                };

                return proryvPropertiesList;
            }
        }

        //Proryv
        private static HashSet<string> proryvFunctionsList;
        private static HashSet<string> ProryvFunctionsList
        {
            get
            {
                if (proryvFunctionsList != null) return proryvFunctionsList;

                var funcs = ProryvFunctions.GetFunctions(false);
                if (funcs != null && funcs.Length > 0)
                {
                    proryvFunctionsList = new HashSet<string>(funcs
                        .Where(f => Equals(f.TypeOfFunction.Name, "ProryvReportFunctions"))
                        .Select(f => f.FunctionName));
                }
                else
                {
                    proryvFunctionsList = new HashSet<string>();
                }

                return proryvFunctionsList;
            }
        }

        private static readonly object _proryvSpreadsheetPropertiesLock = new object();
        private static Dictionary<string, PropertyInfo> _proryvSpreadsheetProperties;
        private static Dictionary<string, PropertyInfo> _proryvFreeHierarchyBalanceSignature;


        private static ValuesDict<ProryvFunctionType> _functionsList = null;
        private static ValuesDict<ProryvFunctionType> FunctionsList
        {
            get
            {
                if (_functionsList == null)
                {
                    _functionsList = new ValuesDict<ProryvFunctionType>();
                    
                    _functionsList["ABS"] = ProryvFunctionType.Abs;
                    _functionsList["ACOS"] = ProryvFunctionType.Acos;
                    _functionsList["ASIN"] = ProryvFunctionType.Asin;
                    _functionsList["ATAN"] = ProryvFunctionType.Atan;
                    _functionsList["CEILING"] = ProryvFunctionType.Ceiling;
                    _functionsList["COS"] = ProryvFunctionType.Cos;
                    _functionsList["DIV"] = ProryvFunctionType.Div;
                    _functionsList["EXP"] = ProryvFunctionType.Exp;
                    _functionsList["FLOOR"] = ProryvFunctionType.Floor;
                    _functionsList["LOG"] = ProryvFunctionType.Log;
                    _functionsList["MAXIMUM"] = ProryvFunctionType.Maximum;
                    _functionsList["MINIMUM"] = ProryvFunctionType.Minimum;
                    _functionsList["ROUND"] = ProryvFunctionType.Round;
                    _functionsList["SIGN"] = ProryvFunctionType.Sign;
                    _functionsList["SIN"] = ProryvFunctionType.Sin;
                    _functionsList["SQRT"] = ProryvFunctionType.Sqrt;
                    _functionsList["TAN"] = ProryvFunctionType.Tan;
                    _functionsList["TRUNCATE"] = ProryvFunctionType.Truncate;
                    _functionsList["Power"] = ProryvFunctionType.Power;

                    _functionsList["DateDiff"] = ProryvFunctionType.DateDiff;
                    _functionsList["DateSerial"] = ProryvFunctionType.DateSerial;
                    _functionsList["Day"] = ProryvFunctionType.Day;
                    _functionsList["DayOfWeek"] = ProryvFunctionType.DayOfWeek;
                    _functionsList["DayOfYear"] = ProryvFunctionType.DayOfYear;
                    _functionsList["DaysInMonth"] = ProryvFunctionType.DaysInMonth;
                    _functionsList["DaysInYear"] = ProryvFunctionType.DaysInYear;
                    _functionsList["Hour"] = ProryvFunctionType.Hour;
                    _functionsList["Minute"] = ProryvFunctionType.Minute;
                    _functionsList["Month"] = ProryvFunctionType.Month;
                    _functionsList["Second"] = ProryvFunctionType.Second;
                    _functionsList["TimeSerial"] = ProryvFunctionType.TimeSerial;
                    _functionsList["Year"] = ProryvFunctionType.Year;
                    _functionsList["MonthName"] = ProryvFunctionType.MonthName;

                    _functionsList["DateToStr"] = ProryvFunctionType.DateToStr;
                    _functionsList["DateToStrPl"] = ProryvFunctionType.DateToStrPl;
                    _functionsList["DateToStrRu"] = ProryvFunctionType.DateToStrRu;
                    _functionsList["DateToStrUa"] = ProryvFunctionType.DateToStrUa;
                    _functionsList["DateToStrPt"] = ProryvFunctionType.DateToStrPt;
                    _functionsList["DateToStrPtBr"] = ProryvFunctionType.DateToStrPtBr;
                    _functionsList["Insert"] = ProryvFunctionType.Insert;
                    _functionsList["Length"] = ProryvFunctionType.Length;
                    _functionsList["Remove"] = ProryvFunctionType.Remove;
                    _functionsList["Replace"] = ProryvFunctionType.Replace;
                    _functionsList["Roman"] = ProryvFunctionType.Roman;
                    _functionsList["Substring"] = ProryvFunctionType.Substring;
                    _functionsList["ToCurrencyWords"] = ProryvFunctionType.ToCurrencyWords;
                    _functionsList["ToCurrencyWordsEnGb"] = ProryvFunctionType.ToCurrencyWordsEnGb;
                    _functionsList["ToCurrencyWordsEnIn"] = ProryvFunctionType.ToCurrencyWordsEnIn;
                    _functionsList["ToCurrencyWordsEs"] = ProryvFunctionType.ToCurrencyWordsEs;
                    _functionsList["ToCurrencyWordsFr"] = ProryvFunctionType.ToCurrencyWordsFr;
                    _functionsList["ToCurrencyWordsNl"] = ProryvFunctionType.ToCurrencyWordsNl;
                    _functionsList["ToCurrencyWordsPl"] = ProryvFunctionType.ToCurrencyWordsPl;
                    _functionsList["ToCurrencyWordsPt"] = ProryvFunctionType.ToCurrencyWordsPt;
                    _functionsList["ToCurrencyWordsPtBr"] = ProryvFunctionType.ToCurrencyWordsPtBr;
                    _functionsList["ToCurrencyWordsRu"] = ProryvFunctionType.ToCurrencyWordsRu;
                    _functionsList["ToCurrencyWordsThai"] = ProryvFunctionType.ToCurrencyWordsThai;
                    _functionsList["ToCurrencyWordsUa"] = ProryvFunctionType.ToCurrencyWordsUa;
                    _functionsList["ToLowerCase"] = ProryvFunctionType.ToLowerCase;
                    _functionsList["ToProperCase"] = ProryvFunctionType.ToProperCase;
                    _functionsList["ToUpperCase"] = ProryvFunctionType.ToUpperCase;
                    _functionsList["ToWords"] = ProryvFunctionType.ToWords;
                    _functionsList["ToWordsEs"] = ProryvFunctionType.ToWordsEs;
                    _functionsList["ToWordsEnIn"] = ProryvFunctionType.ToWordsEnIn;
                    _functionsList["ToWordsFa"] = ProryvFunctionType.ToWordsFa;
                    _functionsList["ToWordsPl"] = ProryvFunctionType.ToWordsPl;
                    _functionsList["ToWordsPt"] = ProryvFunctionType.ToWordsPt;
                    _functionsList["ToWordsRu"] = ProryvFunctionType.ToWordsRu;
                    _functionsList["ToWordsUa"] = ProryvFunctionType.ToWordsUa;
                    _functionsList["Trim"] = ProryvFunctionType.Trim;
                    _functionsList["TryParseDecimal"] = ProryvFunctionType.TryParseDecimal;
                    _functionsList["TryParseDouble"] = ProryvFunctionType.TryParseDouble;
                    _functionsList["TryParseLong"] = ProryvFunctionType.TryParseLong;
                    _functionsList["Arabic"] = ProryvFunctionType.Arabic;
                    _functionsList["Persian"] = ProryvFunctionType.Persian;
                    _functionsList["ToOrdinal"] = ProryvFunctionType.ToOrdinal;
                    _functionsList["Left"] = ProryvFunctionType.Left;
                    _functionsList["Mid"] = ProryvFunctionType.Mid;
                    _functionsList["Right"] = ProryvFunctionType.Right;

                    _functionsList["IsNull"] = ProryvFunctionType.IsNull;
                    _functionsList["Next"] = ProryvFunctionType.Next;
                    _functionsList["NextIsNull"] = ProryvFunctionType.NextIsNull;
                    _functionsList["Previous"] = ProryvFunctionType.Previous;
                    _functionsList["PreviousIsNull"] = ProryvFunctionType.PreviousIsNull;

                    _functionsList["IIF"] = ProryvFunctionType.IIF;
                    _functionsList["Choose"] = ProryvFunctionType.Choose;
                    _functionsList["Switch"] = ProryvFunctionType.Switch;

                    _functionsList["ToString"] = ProryvFunctionType.ToString;
                    _functionsList["Format"] = ProryvFunctionType.Format;

                    _functionsList["System"] = ProryvFunctionType.NameSpace;
                    _functionsList["System.Convert"] = ProryvFunctionType.NameSpace;
                    _functionsList["System.Convert.ToBoolean"] = ProryvFunctionType.SystemConvertToBoolean;
                    _functionsList["System.Convert.ToByte"] = ProryvFunctionType.SystemConvertToByte;
                    _functionsList["System.Convert.ToChar"] = ProryvFunctionType.SystemConvertToChar;
                    _functionsList["System.Convert.ToDateTime"] = ProryvFunctionType.SystemConvertToDateTime;
                    _functionsList["System.Convert.ToDecimal"] = ProryvFunctionType.SystemConvertToDecimal;
                    _functionsList["System.Convert.ToDouble"] = ProryvFunctionType.SystemConvertToDouble;
                    _functionsList["System.Convert.ToInt16"] = ProryvFunctionType.SystemConvertToInt16;
                    _functionsList["System.Convert.ToInt32"] = ProryvFunctionType.SystemConvertToInt32;
                    _functionsList["System.Convert.ToInt64"] = ProryvFunctionType.SystemConvertToInt64;
                    _functionsList["System.Convert.ToSByte"] = ProryvFunctionType.SystemConvertToSByte;
                    _functionsList["System.Convert.ToSingle"] = ProryvFunctionType.SystemConvertToSingle;
                    _functionsList["System.Convert.ToString"] = ProryvFunctionType.SystemConvertToString;
                    _functionsList["System.Convert.ToUInt16"] = ProryvFunctionType.SystemConvertToUInt16;
                    _functionsList["System.Convert.ToUInt32"] = ProryvFunctionType.SystemConvertToUInt32;
                    _functionsList["System.Convert.ToUInt64"] = ProryvFunctionType.SystemConvertToUInt64;
                    _functionsList["Convert"] = ProryvFunctionType.NameSpace;
                    _functionsList["Convert.ToBoolean"] = ProryvFunctionType.SystemConvertToBoolean;
                    _functionsList["Convert.ToByte"] = ProryvFunctionType.SystemConvertToByte;
                    _functionsList["Convert.ToChar"] = ProryvFunctionType.SystemConvertToChar;
                    _functionsList["Convert.ToDateTime"] = ProryvFunctionType.SystemConvertToDateTime;
                    _functionsList["Convert.ToDecimal"] = ProryvFunctionType.SystemConvertToDecimal;
                    _functionsList["Convert.ToDouble"] = ProryvFunctionType.SystemConvertToDouble;
                    _functionsList["Convert.ToInt16"] = ProryvFunctionType.SystemConvertToInt16;
                    _functionsList["Convert.ToInt32"] = ProryvFunctionType.SystemConvertToInt32;
                    _functionsList["Convert.ToInt64"] = ProryvFunctionType.SystemConvertToInt64;
                    _functionsList["Convert.ToSByte"] = ProryvFunctionType.SystemConvertToSByte;
                    _functionsList["Convert.ToSingle"] = ProryvFunctionType.SystemConvertToSingle;
                    _functionsList["Convert.ToString"] = ProryvFunctionType.SystemConvertToString;
                    _functionsList["Convert.ToUInt16"] = ProryvFunctionType.SystemConvertToUInt16;
                    _functionsList["Convert.ToUInt32"] = ProryvFunctionType.SystemConvertToUInt32;
                    _functionsList["Convert.ToUInt64"] = ProryvFunctionType.SystemConvertToUInt64;

                    _functionsList["Math"] = ProryvFunctionType.NameSpace;
                    _functionsList["Math.Round"] = ProryvFunctionType.MathRound;

                    _functionsList["AddAnchor"] = ProryvFunctionType.AddAnchor;
                    _functionsList["GetAnchorPageNumber"] = ProryvFunctionType.GetAnchorPageNumber;
                    _functionsList["GetAnchorPageNumberThrough"] = ProryvFunctionType.GetAnchorPageNumberThrough;

                    _functionsList["ConvertRtf"] = ProryvFunctionType.ConvertRtf;

                    _functionsList["int.Parse"] = ProryvFunctionType.ParseInt;
                    _functionsList["double.Parse"] = ProryvFunctionType.ParseDouble;
                    _functionsList["Double.Parse"] = ProryvFunctionType.ParseDouble;
                    _functionsList["decimal.Parse"] = ProryvFunctionType.ParseDecimal;
                    _functionsList["Decimal.Parse"] = ProryvFunctionType.ParseDecimal;
                    _functionsList["DateTime.Parse"] = ProryvFunctionType.ParseDateTime;

                    _functionsList["string.IsNullOrEmpty"] = ProryvFunctionType.StringIsNullOrEmpty;
                    _functionsList["String.IsNullOrEmpty"] = ProryvFunctionType.StringIsNullOrEmpty;
                    _functionsList["string.IsNullOrWhiteSpace"] = ProryvFunctionType.StringIsNullOrWhiteSpace;
                    _functionsList["String.IsNullOrWhiteSpace"] = ProryvFunctionType.StringIsNullOrWhiteSpace;
                    _functionsList["string.Format"] = ProryvFunctionType.Format;
                    _functionsList["String.Format"] = ProryvFunctionType.Format;

                    _functionsList["Func"] = ProryvFunctionType.NameSpace;
                    _functionsList["Func.EngineHelper"] = ProryvFunctionType.NameSpace;
                    _functionsList["Func.EngineHelper.JoinColumnContent"] = ProryvFunctionType.EngineHelperJoinColumnContent;
                    _functionsList["Func.EngineHelper.ToQueryString"] = ProryvFunctionType.EngineHelperToQueryString;

                }
                return _functionsList;
            }
        }



        private static ValuesDict<StiMethodType> methodsList = null;
        private static ValuesDict<StiMethodType> MethodsList
        {
            get
            {
                if (methodsList == null)
                {
                    methodsList = new ValuesDict<StiMethodType>();
                    //methodsList[""] = StiFunctionType.FunctionNameSpace;
                    methodsList["Substring"] = StiMethodType.Substring;
                    methodsList["ToString"] = StiMethodType.ToString;
                    methodsList["ToLower"] = StiMethodType.ToLower;
                    methodsList["ToUpper"] = StiMethodType.ToUpper;
                    methodsList["IndexOf"] = StiMethodType.IndexOf;
                    methodsList["StartsWith"] = StiMethodType.StartsWith;
                    methodsList["EndsWith"] = StiMethodType.EndsWith;

                    methodsList["Parse"] = StiMethodType.Parse;
                    methodsList["Contains"] = StiMethodType.Contains;
                    methodsList["GetData"] = StiMethodType.GetData;
                    methodsList["ToQueryString"] = StiMethodType.ToQueryString;

                    methodsList["AddYears"] = StiMethodType.AddYears;
                    methodsList["AddMonths"] = StiMethodType.AddMonths;
                    methodsList["AddDays"] = StiMethodType.AddDays;
                    methodsList["AddHours"] = StiMethodType.AddHours;
                    methodsList["AddMinutes"] = StiMethodType.AddMinutes;
                    methodsList["AddSeconds"] = StiMethodType.AddSeconds;
                    methodsList["AddMilliseconds"] = StiMethodType.AddMilliseconds;
                }
                return methodsList;
            }
        }

        // список параметров функций, которые не надо вычислять сразу, 
        // а оставлять в виде кода для последующего вычисления
        private static Hashtable parametersList = null;
        private static Hashtable ParametersList
        {
            get
            {
                if (parametersList == null)
                {
                    parametersList = new Hashtable();

                    #region Fill parameters list

                    #region Aggregate functions - Report
                    parametersList[ProryvFunctionType.CountDistinct] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.Avg] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.AvgD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.AvgDate] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.AvgI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.AvgTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.Max] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.MaxD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.MaxDate] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.MaxI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.MaxStr] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.MaxTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.Median] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.MedianD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.MedianI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.Min] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.MinD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.MinDate] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.MinI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.MinStr] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.MinTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.Mode] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.ModeD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.ModeI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.Sum] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.SumD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.SumDistinct] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.SumI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.SumTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.First] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.Last] = ProryvParameterNumber.Param2;

                    parametersList[ProryvFunctionType.rCountDistinct] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rAvg] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rAvgD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rAvgDate] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rAvgI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rAvgTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rMax] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rMaxD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rMaxDate] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rMaxI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rMaxStr] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rMaxTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rMedian] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rMedianD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rMedianI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rMin] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rMinD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rMinDate] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rMinI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rMinStr] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rMinTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rMode] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rModeD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rModeI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rSum] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rSumD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rSumDistinct] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.rSumI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rSumTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rFirst] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.rLast] = ProryvParameterNumber.Param2;

                    parametersList[ProryvFunctionType.iCount] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.iCountDistinct] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iAvg] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iAvgD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iAvgDate] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iAvgI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iAvgTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iMax] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iMaxD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iMaxDate] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iMaxI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iMaxStr] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iMaxTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iMedian] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iMedianD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iMedianI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iMin] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iMinD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iMinDate] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iMinI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iMinStr] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iMinTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iMode] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iModeD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iModeI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iSum] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iSumD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iSumDistinct] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3 | ProryvParameterNumber.Param4;
                    parametersList[ProryvFunctionType.iSumI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iSumTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iFirst] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.iLast] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;

                    parametersList[ProryvFunctionType.riCount] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.riCountDistinct] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riAvg] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riAvgD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riAvgDate] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riAvgI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riAvgTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riMax] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riMaxD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riMaxDate] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riMaxI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riMaxStr] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riMaxTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riMedian] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riMedianD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riMedianI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riMin] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riMinD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riMinDate] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riMinI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riMinStr] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riMinTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riMode] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riModeD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riModeI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riSum] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riSumD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riSumDistinct] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3 | ProryvParameterNumber.Param4;
                    parametersList[ProryvFunctionType.riSumI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riSumTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riFirst] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.riLast] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    #endregion

                    #region Aggregate functions - Column
                    parametersList[ProryvFunctionType.cCountDistinct] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cAvg] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cAvgD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cAvgDate] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cAvgI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cAvgTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cMax] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cMaxD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cMaxDate] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cMaxI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cMaxStr] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cMaxTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cMedian] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cMedianD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cMedianI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cMin] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cMinD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cMinDate] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cMinI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cMinStr] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cMinTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cMode] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cModeD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cModeI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cSum] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cSumD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cSumDistinct] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.cSumI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cSumTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cFirst] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.cLast] = ProryvParameterNumber.Param2;

                    parametersList[ProryvFunctionType.crCountDistinct] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crAvg] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crAvgD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crAvgDate] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crAvgI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crAvgTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crMax] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crMaxD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crMaxDate] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crMaxI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crMaxStr] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crMaxTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crMedian] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crMedianD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crMedianI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crMin] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crMinD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crMinDate] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crMinI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crMinStr] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crMinTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crMode] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crModeD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crModeI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crSum] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crSumD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crSumDistinct] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.crSumI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crSumTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crFirst] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.crLast] = ProryvParameterNumber.Param2;

                    parametersList[ProryvFunctionType.ciCount] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.ciCountDistinct] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciAvg] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciAvgD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciAvgDate] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciAvgI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciAvgTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciMax] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciMaxD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciMaxDate] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciMaxI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciMaxStr] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciMaxTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciMedian] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciMedianD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciMedianI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciMin] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciMinD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciMinDate] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciMinI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciMinStr] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciMinTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciMode] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciModeD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciModeI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciSum] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciSumD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciSumDistinct] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3 | ProryvParameterNumber.Param4;
                    parametersList[ProryvFunctionType.ciSumI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciSumTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciFirst] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.ciLast] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;

                    parametersList[ProryvFunctionType.criCount] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.criCountDistinct] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criAvg] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criAvgD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criAvgDate] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criAvgI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criAvgTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criMax] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criMaxD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criMaxDate] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criMaxI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criMaxStr] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criMaxTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criMedian] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criMedianD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criMedianI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criMin] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criMinD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criMinDate] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criMinI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criMinStr] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criMinTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criMode] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criModeD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criModeI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criSum] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criSumD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criSumDistinct] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3 | ProryvParameterNumber.Param4;
                    parametersList[ProryvFunctionType.criSumI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criSumTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criFirst] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.criLast] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    #endregion

                    #region Aggregate functions - Page
                    parametersList[ProryvFunctionType.pCountDistinct] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pAvg] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pAvgD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pAvgDate] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pAvgI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pAvgTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pMax] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pMaxD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pMaxDate] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pMaxI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pMaxStr] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pMaxTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pMedian] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pMedianD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pMedianI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pMin] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pMinD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pMinDate] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pMinI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pMinStr] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pMinTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pMode] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pModeD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pModeI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pSum] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pSumD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pSumDistinct] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.pSumI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pSumTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pFirst] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.pLast] = ProryvParameterNumber.Param2;

                    parametersList[ProryvFunctionType.prCountDistinct] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prAvg] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prAvgD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prAvgDate] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prAvgI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prAvgTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prMax] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prMaxD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prMaxDate] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prMaxI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prMaxStr] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prMaxTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prMedian] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prMedianD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prMedianI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prMin] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prMinD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prMinDate] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prMinI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prMinStr] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prMinTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prMode] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prModeD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prModeI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prSum] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prSumD] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prSumDistinct] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.prSumI] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prSumTime] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prFirst] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.prLast] = ProryvParameterNumber.Param2;

                    parametersList[ProryvFunctionType.piCount] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.piCountDistinct] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piAvg] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piAvgD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piAvgDate] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piAvgI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piAvgTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piMax] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piMaxD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piMaxDate] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piMaxI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piMaxStr] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piMaxTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piMedian] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piMedianD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piMedianI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piMin] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piMinD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piMinDate] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piMinI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piMinStr] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piMinTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piMode] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piModeD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piModeI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piSum] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piSumD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piSumDistinct] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3 | ProryvParameterNumber.Param4;
                    parametersList[ProryvFunctionType.piSumI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piSumTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piFirst] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.piLast] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;

                    parametersList[ProryvFunctionType.priCount] = ProryvParameterNumber.Param2;
                    parametersList[ProryvFunctionType.priCountDistinct] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priAvg] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priAvgD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priAvgDate] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priAvgI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priAvgTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priMax] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priMaxD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priMaxDate] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priMaxI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priMaxStr] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priMaxTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priMedian] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priMedianD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priMedianI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priMin] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priMinD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priMinDate] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priMinI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priMinStr] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priMinTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priMode] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priModeD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priModeI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priSum] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priSumD] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priSumDistinct] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3 | ProryvParameterNumber.Param4;
                    parametersList[ProryvFunctionType.priSumI] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priSumTime] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priFirst] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    parametersList[ProryvFunctionType.priLast] = ProryvParameterNumber.Param2 | ProryvParameterNumber.Param3;
                    #endregion

                    parametersList[ProryvFunctionType.Rank] = ProryvParameterNumber.Param2;
                    #endregion
                }
                return parametersList;
            }
        }


        private static object lockMethodsHash = new object();
        private static Hashtable methodsHash = null;
        private static Hashtable MethodsHash
        {
            get
            {
                if (methodsHash == null)
                {
                    lock (lockMethodsHash)
                    {
                        StiParserMethodInfo[] methods = new StiParserMethodInfo[]
                        {
                            #region Date
                            new StiParserMethodInfo(ProryvFunctionType.DateDiff, 1, new Type[] {typeof(DateTime), typeof(DateTime)}, typeof(TimeSpan)),
                            new StiParserMethodInfo(ProryvFunctionType.DateDiff, 2, new Type[] {typeof(DateTime?), typeof(DateTime?)}, typeof(TimeSpan?)),

                            new StiParserMethodInfo(ProryvFunctionType.DateSerial, 1, new Type[] {typeof(long), typeof(long), typeof(long)}, typeof(DateTime)),
                            new StiParserMethodInfo(ProryvFunctionType.TimeSerial, 1, new Type[] {typeof(long), typeof(long), typeof(long)}, typeof(TimeSpan)),

                            new StiParserMethodInfo(ProryvFunctionType.Year, 1, new Type[] {typeof(DateTime)}, typeof(Int64)),
                            new StiParserMethodInfo(ProryvFunctionType.Year, 2, new Type[] {typeof(DateTime?)}, typeof(Int64)),
                            new StiParserMethodInfo(ProryvFunctionType.Month, 1, new Type[] {typeof(DateTime)}, typeof(Int64)),
                            new StiParserMethodInfo(ProryvFunctionType.Month, 2, new Type[] {typeof(DateTime?)}, typeof(Int64)),
                            new StiParserMethodInfo(ProryvFunctionType.Day, 1, new Type[] {typeof(DateTime)}, typeof(Int64)),
                            new StiParserMethodInfo(ProryvFunctionType.Day, 2, new Type[] {typeof(DateTime?)}, typeof(Int64)),
                            new StiParserMethodInfo(ProryvFunctionType.Hour, 1, new Type[] {typeof(DateTime)}, typeof(Int64)),
                            new StiParserMethodInfo(ProryvFunctionType.Hour, 2, new Type[] {typeof(DateTime?)}, typeof(Int64)),
                            new StiParserMethodInfo(ProryvFunctionType.Minute, 1, new Type[] {typeof(DateTime)}, typeof(Int64)),
                            new StiParserMethodInfo(ProryvFunctionType.Minute, 2, new Type[] {typeof(DateTime?)}, typeof(Int64)),
                            new StiParserMethodInfo(ProryvFunctionType.Second, 1, new Type[] {typeof(DateTime)}, typeof(Int64)),
                            new StiParserMethodInfo(ProryvFunctionType.Second, 2, new Type[] {typeof(DateTime?)}, typeof(Int64)),

                            new StiParserMethodInfo(ProryvFunctionType.DayOfWeek, 1, new Type[] {typeof(DateTime)}),
                            new StiParserMethodInfo(ProryvFunctionType.DayOfWeek, 2, new Type[] {typeof(DateTime?)}),
                            new StiParserMethodInfo(ProryvFunctionType.DayOfWeek, 3, new Type[] {typeof(DateTime), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.DayOfWeek, 4, new Type[] {typeof(DateTime?), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.DayOfWeek, 5, new Type[] {typeof(DateTime), typeof(string)}),
                            new StiParserMethodInfo(ProryvFunctionType.DayOfWeek, 6, new Type[] {typeof(DateTime?), typeof(string)}),
                            new StiParserMethodInfo(ProryvFunctionType.DayOfWeek, 7, new Type[] {typeof(DateTime), typeof(string), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.DayOfWeek, 8, new Type[] {typeof(DateTime?), typeof(string), typeof(bool)}),

                            new StiParserMethodInfo(ProryvFunctionType.DayOfYear, 1, new Type[] {typeof(DateTime)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.DayOfYear, 2, new Type[] {typeof(DateTime?)}, typeof(long)),

                            new StiParserMethodInfo(ProryvFunctionType.DaysInMonth, 1, new Type[] {typeof(DateTime)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.DaysInMonth, 2, new Type[] {typeof(DateTime?)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.DaysInMonth, 3, new Type[] {typeof(long), typeof(long)}, typeof(long)),

                            new StiParserMethodInfo(ProryvFunctionType.DaysInYear, 1, new Type[] {typeof(DateTime)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.DaysInYear, 2, new Type[] {typeof(DateTime?)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.DaysInYear, 3, new Type[] {typeof(long)}, typeof(long)),

                            new StiParserMethodInfo(ProryvFunctionType.MonthName, 1, new Type[] {typeof(DateTime)}),
                            new StiParserMethodInfo(ProryvFunctionType.MonthName, 2, new Type[] {typeof(DateTime?)}),
                            new StiParserMethodInfo(ProryvFunctionType.MonthName, 3, new Type[] {typeof(DateTime), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.MonthName, 4, new Type[] {typeof(DateTime?), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.MonthName, 5, new Type[] {typeof(DateTime), typeof(string)}),
                            new StiParserMethodInfo(ProryvFunctionType.MonthName, 6, new Type[] {typeof(DateTime?), typeof(string)}),
                            new StiParserMethodInfo(ProryvFunctionType.MonthName, 7, new Type[] {typeof(DateTime), typeof(string), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.MonthName, 8, new Type[] {typeof(DateTime?), typeof(string), typeof(bool)}),
                            #endregion
                            
                            #region Math
                            new StiParserMethodInfo(ProryvFunctionType.Abs, 1, new Type[] {typeof(long)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.Abs, 2, new Type[] {typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.Abs, 3, new Type[] {typeof(decimal)}, typeof(decimal)),

                            new StiParserMethodInfo(ProryvFunctionType.Acos, 1, new Type[] {typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.Asin, 1, new Type[] {typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.Atan, 1, new Type[] {typeof(double)}, typeof(double)),

                            new StiParserMethodInfo(ProryvFunctionType.Cos, 1, new Type[] {typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.Sin, 1, new Type[] {typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.Tan, 1, new Type[] {typeof(double)}, typeof(double)),

                            new StiParserMethodInfo(ProryvFunctionType.Ceiling, 1, new Type[] {typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.Ceiling, 2, new Type[] {typeof(decimal)}, typeof(decimal)),

                            new StiParserMethodInfo(ProryvFunctionType.Div, 1, new Type[] {typeof(long), typeof(long)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.Div, 2, new Type[] {typeof(long), typeof(long), typeof(long)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.Div, 3, new Type[] {typeof(double), typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.Div, 4, new Type[] {typeof(double), typeof(double), typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.Div, 5, new Type[] {typeof(decimal), typeof(decimal)}, typeof(decimal)),
                            new StiParserMethodInfo(ProryvFunctionType.Div, 6, new Type[] {typeof(decimal), typeof(decimal), typeof(decimal)}, typeof(decimal)),
                            new StiParserMethodInfo(ProryvFunctionType.Div, 7, new Type[] {typeof(long?), typeof(long?)}, typeof(long?)),
                            new StiParserMethodInfo(ProryvFunctionType.Div, 8, new Type[] {typeof(long?), typeof(long?), typeof(long?)}, typeof(long?)),
                            new StiParserMethodInfo(ProryvFunctionType.Div, 9, new Type[] {typeof(double?), typeof(double?)}, typeof(double?)),
                            new StiParserMethodInfo(ProryvFunctionType.Div, 10, new Type[] {typeof(double?), typeof(double?), typeof(double?)}, typeof(double?)),
                            new StiParserMethodInfo(ProryvFunctionType.Div, 11, new Type[] {typeof(decimal?), typeof(decimal?)}, typeof(decimal?)),
                            new StiParserMethodInfo(ProryvFunctionType.Div, 12, new Type[] {typeof(decimal?), typeof(decimal?), typeof(decimal?)}, typeof(decimal?)),

                            new StiParserMethodInfo(ProryvFunctionType.Exp, 1, new Type[] {typeof(long)}, typeof(double)),

                            new StiParserMethodInfo(ProryvFunctionType.Floor, 1, new Type[] {typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.Floor, 2, new Type[] {typeof(decimal)}, typeof(decimal)),

                            new StiParserMethodInfo(ProryvFunctionType.Log, 1, new Type[] {typeof(double)}, typeof(double)),

                            new StiParserMethodInfo(ProryvFunctionType.Maximum, 1, new Type[] {typeof(long), typeof(long)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.Maximum, 2, new Type[] {typeof(double), typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.Maximum, 3, new Type[] {typeof(decimal), typeof(decimal)}, typeof(decimal)),
                            new StiParserMethodInfo(ProryvFunctionType.Maximum, 4, new Type[] {typeof(long?), typeof(long?)}, typeof(long?)),
                            new StiParserMethodInfo(ProryvFunctionType.Maximum, 5, new Type[] {typeof(double?), typeof(double?)}, typeof(double?)),
                            new StiParserMethodInfo(ProryvFunctionType.Maximum, 6, new Type[] {typeof(decimal?), typeof(decimal?)}, typeof(decimal?)),

                            new StiParserMethodInfo(ProryvFunctionType.Minimum, 1, new Type[] {typeof(long), typeof(long)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.Minimum, 2, new Type[] {typeof(double), typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.Minimum, 3, new Type[] {typeof(decimal), typeof(decimal)}, typeof(decimal)),
                            new StiParserMethodInfo(ProryvFunctionType.Minimum, 4, new Type[] {typeof(long?), typeof(long?)}, typeof(long?)),
                            new StiParserMethodInfo(ProryvFunctionType.Minimum, 5, new Type[] {typeof(double?), typeof(double?)}, typeof(double?)),
                            new StiParserMethodInfo(ProryvFunctionType.Minimum, 6, new Type[] {typeof(decimal?), typeof(decimal?)}, typeof(decimal?)),

                            new StiParserMethodInfo(ProryvFunctionType.Round, 1, new Type[] {typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.Round, 2, new Type[] {typeof(double), typeof(int)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.Round, 3, new Type[] {typeof(decimal)}, typeof(decimal)),
                            new StiParserMethodInfo(ProryvFunctionType.Round, 4, new Type[] {typeof(decimal), typeof(int)}, typeof(decimal)),

                            new StiParserMethodInfo(ProryvFunctionType.Sign, 1, new Type[] {typeof(long)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.Sign, 2, new Type[] {typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.Sign, 3, new Type[] {typeof(decimal)}, typeof(decimal)),

                            new StiParserMethodInfo(ProryvFunctionType.Truncate, 1, new Type[] {typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.Truncate, 2, new Type[] {typeof(decimal)}, typeof(decimal)),
                            #endregion

                            #region Print state
                            new StiParserMethodInfo(ProryvFunctionType.IsNull, 1, new Type[] {typeof(object), typeof(string)}, typeof(bool)),
                            new StiParserMethodInfo(ProryvFunctionType.Next, 1, new Type[] {typeof(object), typeof(string)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.NextIsNull, 1, new Type[] {typeof(object), typeof(string)}, typeof(bool)),
                            new StiParserMethodInfo(ProryvFunctionType.Previous, 1, new Type[] {typeof(object), typeof(string)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.PreviousIsNull, 1, new Type[] {typeof(object), typeof(string)}, typeof(bool)),
                            #endregion

                            #region Programming Shortcut
                            new StiParserMethodInfo(ProryvFunctionType.IIF, 1, new Type[] {typeof(bool), typeof(object), typeof(object)}, typeof(object)),

                            new StiParserMethodInfo(ProryvFunctionType.Choose, 1, new Type[] {typeof(int), typeof(object)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.Choose, 2, new Type[] {typeof(int), typeof(object), typeof(object)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.Choose, 3, new Type[] {typeof(int), typeof(object), typeof(object), typeof(object)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.Choose, 4, new Type[] {typeof(int), typeof(object), typeof(object), typeof(object), typeof(object)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.Choose, 5, new Type[] {typeof(int), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.Choose, 6, new Type[] {typeof(int), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.Choose, 7, new Type[] {typeof(int), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.Choose, 8, new Type[] {typeof(int), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.Choose, 9, new Type[] {typeof(int), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.Choose, 10, new Type[] {typeof(int), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object), typeof(object)}, typeof(object)),

                            new StiParserMethodInfo(ProryvFunctionType.Switch, 1, new Type[] {typeof(bool), typeof(object)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.Switch, 2, new Type[] {typeof(bool), typeof(object), typeof(bool), typeof(object)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.Switch, 3, new Type[] {typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.Switch, 4, new Type[] {typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.Switch, 5, new Type[] {typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.Switch, 6, new Type[] {typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.Switch, 7, new Type[] {typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.Switch, 8, new Type[] {typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.Switch, 9, new Type[] {typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object)}, typeof(object)),
                            new StiParserMethodInfo(ProryvFunctionType.Switch, 10, new Type[] {typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object), typeof(bool), typeof(object)}, typeof(object)),
                            #endregion

                            #region Strings
                            new StiParserMethodInfo(ProryvFunctionType.Arabic, 1, new Type[] {typeof(int)}),
                            new StiParserMethodInfo(ProryvFunctionType.Arabic, 2, new Type[] {typeof(string)}),
                            new StiParserMethodInfo(ProryvFunctionType.Persian, 1, new Type[] {typeof(int)}),
                            new StiParserMethodInfo(ProryvFunctionType.Persian, 2, new Type[] {typeof(string)}),

                            new StiParserMethodInfo(ProryvFunctionType.DateToStr, 1, new Type[] {typeof(DateTime)}),
                            new StiParserMethodInfo(ProryvFunctionType.DateToStr, 2, new Type[] {typeof(DateTime?)}),
                            new StiParserMethodInfo(ProryvFunctionType.DateToStr, 3, new Type[] {typeof(DateTime), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.DateToStr, 4, new Type[] {typeof(DateTime?), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.DateToStrPl, 1, new Type[] {typeof(DateTime), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.DateToStrRu, 1, new Type[] {typeof(DateTime)}),
                            new StiParserMethodInfo(ProryvFunctionType.DateToStrRu, 2, new Type[] {typeof(DateTime), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.DateToStrUa, 1, new Type[] {typeof(DateTime)}),
                            new StiParserMethodInfo(ProryvFunctionType.DateToStrUa, 2, new Type[] {typeof(DateTime), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.DateToStrPt, 1, new Type[] {typeof(DateTime)}),
                            new StiParserMethodInfo(ProryvFunctionType.DateToStrPtBr, 1, new Type[] {typeof(DateTime)}),

                            new StiParserMethodInfo(ProryvFunctionType.Insert, 1, new Type[] {typeof(object), typeof(int), typeof(object)}),   //string by specification, but object by code
                            new StiParserMethodInfo(ProryvFunctionType.Left, 1, new Type[] {typeof(object), typeof(int)}),
                            new StiParserMethodInfo(ProryvFunctionType.Right, 1, new Type[] {typeof(object), typeof(int)}),
                            new StiParserMethodInfo(ProryvFunctionType.Mid, 1, new Type[] {typeof(object), typeof(int), typeof(int)}),
                            new StiParserMethodInfo(ProryvFunctionType.Length, 1, new Type[] {typeof(object)}, typeof(int)),
                            new StiParserMethodInfo(ProryvFunctionType.Remove, 1, new Type[] {typeof(object), typeof(int)}),
                            new StiParserMethodInfo(ProryvFunctionType.Remove, 2, new Type[] {typeof(object), typeof(int), typeof(int)}),
                            new StiParserMethodInfo(ProryvFunctionType.Replace, 1, new Type[] {typeof(object), typeof(object), typeof(object)}),
                            new StiParserMethodInfo(ProryvFunctionType.Substring, 1, new Type[] {typeof(object), typeof(int), typeof(int)}),
                            new StiParserMethodInfo(ProryvFunctionType.Trim, 1, new Type[] {typeof(object)}),
                            //new StiParserMethodInfo(StiFunctionType.TrimStart, 1, new Type[] {typeof(object)}),   //exist in code
                            //new StiParserMethodInfo(StiFunctionType.TrimEnd, 1, new Type[] {typeof(object)}),     //exist in code

                            new StiParserMethodInfo(ProryvFunctionType.Roman, 1, new Type[] {typeof(int)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToOrdinal, 1, new Type[] {typeof(long)}),
                            //new StiParserMethodInfo(StiFunctionType.Abc, 1, new Type[] {typeof(int)}),     //exist in code

                            new StiParserMethodInfo(ProryvFunctionType.ToLowerCase, 1, new Type[] {typeof(object)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToUpperCase, 1, new Type[] {typeof(object)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToProperCase, 1, new Type[] {typeof(object)}),

                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWords, 1, new Type[] {typeof(long)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWords, 2, new Type[] {typeof(double)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWords, 3, new Type[] {typeof(decimal)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWords, 4, new Type[] {typeof(long), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWords, 5, new Type[] {typeof(double), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWords, 6, new Type[] {typeof(decimal), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWords, 7, new Type[] {typeof(long), typeof(bool), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWords, 8, new Type[] {typeof(double), typeof(bool), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWords, 9, new Type[] {typeof(decimal), typeof(bool), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWords, 10, new Type[] {typeof(double), typeof(bool), typeof(bool), typeof(string), typeof(string)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsEnGb, 1, new Type[] {typeof(decimal), typeof(string), typeof(int)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsEnIn, 1, new Type[] {typeof(string), typeof(string), typeof(decimal), typeof(int), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsEs, 1, new Type[] {typeof(decimal), typeof(string), typeof(int)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsFr, 1, new Type[] {typeof(decimal), typeof(string), typeof(int)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsNl, 1, new Type[] {typeof(decimal), typeof(string), typeof(int)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsPl, 1, new Type[] {typeof(decimal), typeof(string), typeof(bool), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsPt, 1, new Type[] {typeof(decimal), typeof(bool), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsPtBr, 1, new Type[] {typeof(decimal)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsRu, 1, new Type[] {typeof(long)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsRu, 2, new Type[] {typeof(double)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsRu, 3, new Type[] {typeof(decimal)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsRu, 4, new Type[] {typeof(long), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsRu, 5, new Type[] {typeof(double), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsRu, 6, new Type[] {typeof(decimal), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsRu, 7, new Type[] {typeof(long), typeof(bool), typeof(string)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsRu, 8, new Type[] {typeof(double), typeof(bool), typeof(string)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsRu, 9, new Type[] {typeof(decimal), typeof(bool), typeof(string)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsRu, 10, new Type[] {typeof(long), typeof(string), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsRu, 11, new Type[] {typeof(double), typeof(string), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsRu, 12, new Type[] {typeof(decimal), typeof(string), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsThai, 1, new Type[] {typeof(long)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsThai, 2, new Type[] {typeof(double)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsThai, 3, new Type[] {typeof(decimal)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsUa, 1, new Type[] {typeof(long)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsUa, 2, new Type[] {typeof(double)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsUa, 3, new Type[] {typeof(decimal)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsUa, 4, new Type[] {typeof(long), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsUa, 5, new Type[] {typeof(double), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsUa, 6, new Type[] {typeof(decimal), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsUa, 7, new Type[] {typeof(long), typeof(bool), typeof(string)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsUa, 8, new Type[] {typeof(double), typeof(bool), typeof(string)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToCurrencyWordsUa, 9, new Type[] {typeof(decimal), typeof(bool), typeof(string)}),

                            new StiParserMethodInfo(ProryvFunctionType.ToWords, 1, new Type[] {typeof(long)}),     //проверить соответствие перегрузок
                            new StiParserMethodInfo(ProryvFunctionType.ToWords, 3, new Type[] {typeof(double)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWords, 2, new Type[] {typeof(decimal)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWords, 4, new Type[] {typeof(long), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWords, 6, new Type[] {typeof(double), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWords, 5, new Type[] {typeof(decimal), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWordsEs, 1, new Type[] {typeof(long), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWordsEs, 2, new Type[] {typeof(long), typeof(bool), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWordsEnIn, 1, new Type[] {typeof(long), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWordsFa, 1, new Type[] {typeof(long)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWordsPl, 1, new Type[] {typeof(long), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWordsPt, 1, new Type[] {typeof(long), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWordsRu, 1, new Type[] {typeof(long)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWordsRu, 3, new Type[] {typeof(double)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWordsRu, 2, new Type[] {typeof(decimal)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWordsRu, 4, new Type[] {typeof(long), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWordsRu, 6, new Type[] {typeof(double), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWordsRu, 5, new Type[] {typeof(decimal), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWordsUa, 1, new Type[] {typeof(long)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWordsUa, 3, new Type[] {typeof(double)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWordsUa, 2, new Type[] {typeof(decimal)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWordsUa, 4, new Type[] {typeof(long), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWordsUa, 6, new Type[] {typeof(double), typeof(bool)}),
                            new StiParserMethodInfo(ProryvFunctionType.ToWordsUa, 5, new Type[] {typeof(decimal), typeof(bool)}),

                            new StiParserMethodInfo(ProryvFunctionType.TryParseDecimal, 1, new Type[] {typeof(string)}, typeof(bool)),
                            new StiParserMethodInfo(ProryvFunctionType.TryParseDouble, 1, new Type[] {typeof(string)}, typeof(bool)),
                            new StiParserMethodInfo(ProryvFunctionType.TryParseLong, 1, new Type[] {typeof(string)}, typeof(bool)),
                            #endregion

                            #region Aggregate functions

                            //new StiParserMethodInfo(StiFunctionType.A, 1, new Type[] {typeof(string)}, typeof(bool)),



                            new StiParserMethodInfo(ProryvFunctionType.Rank, 1, new Type[] {typeof(object), typeof(List<StiAsmCommand>)}),
                            
                            #endregion


                            new StiParserMethodInfo(ProryvFunctionType.MathRound, 1, new Type[] {typeof(double), typeof(MidpointRounding)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.MathRound, 2, new Type[] {typeof(double), typeof(int)}, typeof(double)),

                            new StiParserMethodInfo(ProryvFunctionType.GetAnchorPageNumber, 1, new Type[] {typeof(object)}, typeof(int)),
                            new StiParserMethodInfo(ProryvFunctionType.GetAnchorPageNumberThrough, 1, new Type[] {typeof(object)}, typeof(int)),

                            new StiParserMethodInfo(ProryvFunctionType.ParseDateTime, 1, new Type[] {typeof(string)}, typeof(DateTime)),
                            new StiParserMethodInfo(ProryvFunctionType.ParseDecimal, 1, new Type[] {typeof(string)}, typeof(decimal)),
                            new StiParserMethodInfo(ProryvFunctionType.ParseDouble, 1, new Type[] {typeof(string)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.ParseInt, 1, new Type[] {typeof(string)}, typeof(int)),

                            new StiParserMethodInfo(ProryvFunctionType.StringIsNullOrEmpty, 1, new Type[] {typeof(string)}, typeof(bool)),
                            new StiParserMethodInfo(ProryvFunctionType.StringIsNullOrWhiteSpace, 1, new Type[] {typeof(string)}, typeof(bool)),

                            new StiParserMethodInfo(ProryvFunctionType.EngineHelperToQueryString, 1, new Type[] {typeof(IList), typeof(string), typeof(string)}),

                            #region Operators
                            new StiParserMethodInfo(ProryvFunctionType.op_Add, 1, new Type[] {typeof(int), typeof(int)}, typeof(int)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Add, 2, new Type[] {typeof(uint), typeof(uint)}, typeof(uint)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Add, 3, new Type[] {typeof(long), typeof(long)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Add, 4, new Type[] {typeof(ulong), typeof(ulong)}, typeof(ulong)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Add, 5, new Type[] {typeof(float), typeof(float)}, typeof(float)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Add, 6, new Type[] {typeof(double), typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Add, 7, new Type[] {typeof(decimal), typeof(decimal)}, typeof(decimal)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Add, 8, new Type[] {typeof(string), typeof(string)}, typeof(string)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Add, 9, new Type[] {typeof(string), typeof(object)}, typeof(string)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Add, 10, new Type[] {typeof(object), typeof(string)}, typeof(string)),

                            new StiParserMethodInfo(ProryvFunctionType.op_Sub, 1, new Type[] {typeof(int), typeof(int)}, typeof(int)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Sub, 2, new Type[] {typeof(uint), typeof(uint)}, typeof(uint)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Sub, 3, new Type[] {typeof(long), typeof(long)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Sub, 4, new Type[] {typeof(ulong), typeof(ulong)}, typeof(ulong)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Sub, 5, new Type[] {typeof(float), typeof(float)}, typeof(float)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Sub, 6, new Type[] {typeof(double), typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Sub, 7, new Type[] {typeof(decimal), typeof(decimal)}, typeof(decimal)),

                            new StiParserMethodInfo(ProryvFunctionType.op_Mult, 1, new Type[] {typeof(int), typeof(int)}, typeof(int)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Mult, 2, new Type[] {typeof(uint), typeof(uint)}, typeof(uint)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Mult, 3, new Type[] {typeof(long), typeof(long)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Mult, 4, new Type[] {typeof(ulong), typeof(ulong)}, typeof(ulong)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Mult, 5, new Type[] {typeof(float), typeof(float)}, typeof(float)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Mult, 6, new Type[] {typeof(double), typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Mult, 7, new Type[] {typeof(decimal), typeof(decimal)}, typeof(decimal)),

                            new StiParserMethodInfo(ProryvFunctionType.op_Div, 1, new Type[] {typeof(int), typeof(int)}, typeof(int)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Div, 2, new Type[] {typeof(uint), typeof(uint)}, typeof(uint)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Div, 3, new Type[] {typeof(long), typeof(long)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Div, 4, new Type[] {typeof(ulong), typeof(ulong)}, typeof(ulong)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Div, 5, new Type[] {typeof(float), typeof(float)}, typeof(float)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Div, 6, new Type[] {typeof(double), typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Div, 7, new Type[] {typeof(decimal), typeof(decimal)}, typeof(decimal)),

                            new StiParserMethodInfo(ProryvFunctionType.op_Mod, 1, new Type[] {typeof(int), typeof(int)}, typeof(int)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Mod, 2, new Type[] {typeof(uint), typeof(uint)}, typeof(uint)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Mod, 3, new Type[] {typeof(long), typeof(long)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Mod, 4, new Type[] {typeof(ulong), typeof(ulong)}, typeof(ulong)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Mod, 5, new Type[] {typeof(float), typeof(float)}, typeof(float)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Mod, 6, new Type[] {typeof(double), typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Mod, 7, new Type[] {typeof(decimal), typeof(decimal)}, typeof(decimal)),

                            new StiParserMethodInfo(ProryvFunctionType.op_Shl, 1, new Type[] {typeof(int), typeof(int)}, typeof(int)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Shl, 2, new Type[] {typeof(uint), typeof(int)}, typeof(uint)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Shl, 3, new Type[] {typeof(long), typeof(int)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Shl, 4, new Type[] {typeof(ulong), typeof(int)}, typeof(ulong)),

                            new StiParserMethodInfo(ProryvFunctionType.op_Shr, 1, new Type[] {typeof(int), typeof(int)}, typeof(int)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Shr, 2, new Type[] {typeof(uint), typeof(int)}, typeof(uint)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Shr, 3, new Type[] {typeof(long), typeof(int)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Shr, 4, new Type[] {typeof(ulong), typeof(int)}, typeof(ulong)),

                            new StiParserMethodInfo(ProryvFunctionType.op_And, 1, new Type[] {typeof(int), typeof(int)}, typeof(int)),
                            new StiParserMethodInfo(ProryvFunctionType.op_And, 2, new Type[] {typeof(uint), typeof(uint)}, typeof(uint)),
                            new StiParserMethodInfo(ProryvFunctionType.op_And, 3, new Type[] {typeof(long), typeof(long)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.op_And, 4, new Type[] {typeof(ulong), typeof(ulong)}, typeof(ulong)),
                            new StiParserMethodInfo(ProryvFunctionType.op_And, 5, new Type[] {typeof(bool), typeof(bool)}, typeof(bool)),

                            new StiParserMethodInfo(ProryvFunctionType.op_Or, 1, new Type[] {typeof(int), typeof(int)}, typeof(int)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Or, 2, new Type[] {typeof(uint), typeof(uint)}, typeof(uint)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Or, 3, new Type[] {typeof(long), typeof(long)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Or, 4, new Type[] {typeof(ulong), typeof(ulong)}, typeof(ulong)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Or, 5, new Type[] {typeof(bool), typeof(bool)}, typeof(bool)),

                            new StiParserMethodInfo(ProryvFunctionType.op_Xor, 1, new Type[] {typeof(int), typeof(int)}, typeof(int)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Xor, 2, new Type[] {typeof(uint), typeof(uint)}, typeof(uint)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Xor, 3, new Type[] {typeof(long), typeof(long)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Xor, 4, new Type[] {typeof(ulong), typeof(ulong)}, typeof(ulong)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Xor, 5, new Type[] {typeof(bool), typeof(bool)}, typeof(bool)),

                            new StiParserMethodInfo(ProryvFunctionType.op_And2, 1, new Type[] {typeof(bool), typeof(bool)}, typeof(bool)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Or2, 1, new Type[] {typeof(bool), typeof(bool)}, typeof(bool)),

                            new StiParserMethodInfo(ProryvFunctionType.op_Neg, 1, new Type[] {typeof(int)}, typeof(int)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Neg, 2, new Type[] {typeof(long)}, typeof(long)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Neg, 3, new Type[] {typeof(float)}, typeof(float)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Neg, 4, new Type[] {typeof(double)}, typeof(double)),
                            new StiParserMethodInfo(ProryvFunctionType.op_Neg, 5, new Type[] {typeof(decimal)}, typeof(decimal)),

                            new StiParserMethodInfo(ProryvFunctionType.op_Not, 1, new Type[] {typeof(bool)}, typeof(bool)),
                            #endregion
                        };

                        methodsHash = new Hashtable();
                        foreach (StiParserMethodInfo methodInfo in methods)
                        {
                            List<StiParserMethodInfo> list = (List<StiParserMethodInfo>)methodsHash[methodInfo.Name];
                            if (list == null)
                            {
                                list = new List<StiParserMethodInfo>();
                                methodsHash[methodInfo.Name] = list;
                            }
                            list.Add(methodInfo);
                        }
                    }
                }
                return methodsHash;
            }
        }


        private static Hashtable constantsList = null;
        private static Hashtable ConstantsList
        {
            get
            {
                if (constantsList == null)
                {
                    constantsList = new Hashtable();
                    constantsList["true"] = true;
                    constantsList["True"] = true;
                    constantsList["false"] = false;
                    constantsList["False"] = false;
                    constantsList["null"] = null;
                    constantsList["DBNull"] = namespaceObj;
                    constantsList["DBNull.Value"] = DBNull.Value;

                    constantsList["MidpointRounding"] = namespaceObj;
                    constantsList["MidpointRounding.ToEven"] = MidpointRounding.ToEven;
                    constantsList["MidpointRounding.AwayFromZero"] = MidpointRounding.AwayFromZero;

                    constantsList["StiRankOrder"] = namespaceObj;
                    
                    //constantsList[""] = ;
                }
                return constantsList;
            }
        }


        private static object namespaceObj = new object();
        //private static Hashtable namespacesList = null;
        //private static Hashtable NamespacesList
        //{
        //    get
        //    {
        //        if (namespacesList == null)
        //        {
        //            namespacesList = new Hashtable();
        //            namespacesList["MidpointRounding"] = namespaceObj;
        //            namespacesList["System"] = namespaceObj;
        //            namespacesList["System.Convert"] = namespaceObj;
        //            namespacesList["Math"] = namespaceObj;
        //            namespacesList["Totals"] = namespaceObj;
        //        }
        //        return constantsList;
        //    }
        //}


        private object lockUserFunctionsList = new object();
        private Hashtable userFunctionsList = null;
        private Hashtable UserFunctionsList
        {
            get
            {
                if (userFunctionsList == null)
                {
                    lock (lockUserFunctionsList)
                    {
                        userFunctionsList = new Hashtable();
                        var tempUserFunctionsList = new Hashtable();
                        var functions = ProryvFunctions.GetFunctions(false);
                        foreach (var func in functions)
                        {
                            var list = tempUserFunctionsList[func.FunctionName] as List<ProryvFunction>;
                            if (list == null)
                            {
                                list = new List<ProryvFunction>();
                                tempUserFunctionsList[func.FunctionName] = list;
                                userFunctionsList[func.FunctionName] = (int)ProryvFunctionType.UserFunction + userFunctionsList.Count;
                            }
                            list.Add(func);
                        }
                    }
                }
                return userFunctionsList;
            }
        }
        #endregion
    }
}
