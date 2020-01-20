using System;
using System.Globalization;

namespace Proryv.Servers.Calculation.Parser.Internal.Functions
{
    public class ProryvFunctionsStrings
    {
        #region Base functions
        public static int Length(object str)
        {
            if (str == null || str == DBNull.Value) return 0;
            return str.ToString().Length;
        }

        public static string Trim(object str)
        {
            if (str == null || str == DBNull.Value) return string.Empty;
            return str.ToString().Trim();
        }

        public static string TrimStart(object str)
        {
            if (str == null || str == DBNull.Value) return string.Empty;
            return str.ToString().TrimStart();
        }

        public static string TrimEnd(object str)
        {
            if (str == null || str == DBNull.Value) return string.Empty;
            return str.ToString().TrimEnd();
        }

        public static string ToLowerCase(object str)
        {
            if (str == null || str == DBNull.Value) return string.Empty;
            return str.ToString().ToLower(System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string ToUpperCase(object str)
        {
            if (str == null || str == DBNull.Value) return string.Empty;
            return str.ToString().ToUpper(System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string ToProperCase(object str)
        {
            if (str == null || str == DBNull.Value) return string.Empty;

            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str.ToString().ToLowerInvariant());
        }

        public static string Substring(object str, int startIndex, int length)
        {
            if (str == null || str == DBNull.Value) return string.Empty;
            string st = str.ToString();
            if (startIndex + length >= st.Length) return st.Substring(startIndex);
            return st.Substring(startIndex, length);
        }

        public static string Left(object str, int length)
        {
            if (str == null || str == DBNull.Value) return string.Empty;
            string st = str.ToString();
            if (length >= st.Length) return st;
            return st.Substring(0, length);
        }

        public static string Right(object str, int length)
        {
            if (str == null || str == DBNull.Value) return string.Empty;
            string st = str.ToString();
            if (length >= st.Length) return st;
            return st.Substring(st.Length - length, length);
        }

        public static string Mid(object str, int startIndex, int length)
        {
            return Substring(str, startIndex, length);
        }

        public static string Roman(int value)
        {
            return Func.Convert.ToRoman(value);
        }

        public static string ABC(int value)
        {
            return Func.Convert.ToABC(value);
        }

        public static string Arabic(int value)
        {
            return Func.Convert.ToArabic(value, false);
        }
        public static string Arabic(string value)
        {
            return Func.Convert.ToArabic(value, false);
        }
        public static string Persian(int value)
        {
            return Func.Convert.ToArabic(value, true);
        }
        public static string Persian(string value)
        {
            return Func.Convert.ToArabic(value, true);
        }

        public static bool IsNumeric(object str)
        {
            if (str == null || str == DBNull.Value) return false;

            double result;
            return Double.TryParse(
                str.ToString(),
                System.Globalization.NumberStyles.Any,
                System.Globalization.NumberFormatInfo.InvariantInfo, out result);
        }

        public static string Replace(object str, object oldValue, object newValue)
        {
            if (str == null || str == DBNull.Value) return string.Empty;
            if (oldValue == null || oldValue == DBNull.Value) return str.ToString();
            if (newValue == null || newValue == DBNull.Value) return str.ToString();

            return str.ToString().Replace(oldValue.ToString(), newValue.ToString());
        }

        public static string Remove(object str, int startIndex, int count)
        {
            if (str == null || str == DBNull.Value) return string.Empty;

            return str.ToString().Remove(startIndex, count);
        }

        public static string Remove(object str, int startIndex)
        {
            if (str == null || str == DBNull.Value) return string.Empty;

            return str.ToString().Substring(0, startIndex);
        }

        public static string Insert(object str, int startIndex, object value)
        {
            if (str == null || str == DBNull.Value) return string.Empty;
            if (value == null || value == DBNull.Value) return str.ToString();

            return str.ToString().Insert(startIndex, value.ToString());
        }
        #endregion

        #region TryParse
        public static bool TryParseDecimal(string value)
        {
            decimal result;
            return decimal.TryParse(value, out result);
        }

        public static bool TryParseDouble(string value)
        {
            double result;
            return double.TryParse(value, out result);
        }

        public static bool TryParseLong(string value)
        {
            long result;
            return long.TryParse(value, out result);
        }
        #endregion

        #region ToWords
        public static string ToWords(long value)
        {
            return Func.En.NumToStr(value);
        }

        public static string ToWords(decimal value)
        {
            return Func.En.NumToStr(value);
        }

        public static string ToWords(double value)
        {
            return Func.En.NumToStr(value);
        }

        public static string ToWords(long value, bool upperCase)
        {
            return Func.En.NumToStr(value, upperCase);
        }

        public static string ToWords(decimal value, bool upperCase)
        {
            return Func.En.NumToStr(value, upperCase);
        }

        public static string ToWords(double value, bool upperCase)
        {
            return Func.En.NumToStr(value, upperCase);
        }

        public static string ToWords(long? value)
        {
            if (!value.HasValue) return string.Empty;
            return Func.En.NumToStr(value.Value);
        }

        public static string ToWords(decimal? value)
        {
            if (!value.HasValue) return string.Empty;
            return Func.En.NumToStr(value.Value);
        }

        public static string ToWords(double? value)
        {
            if (!value.HasValue) return string.Empty;
            return Func.En.NumToStr(value.Value);
        }

        public static string ToWords(long? value, bool? upperCase)
        {
            if (!value.HasValue) return string.Empty;
            if (!upperCase.HasValue) upperCase = true;
            return Func.En.NumToStr(value.Value, upperCase.Value);
        }

        public static string ToWords(decimal? value, bool? upperCase)
        {
            if (!value.HasValue) return string.Empty;
            if (!upperCase.HasValue) upperCase = true;
            return Func.En.NumToStr(value.Value, upperCase.Value);
        }

        public static string ToWords(double? value, bool? upperCase)
        {
            if (!value.HasValue) return string.Empty;
            if (!upperCase.HasValue) upperCase = true;
            return Func.En.NumToStr(value.Value, upperCase.Value);
        }
        #endregion

        #region DateToStr
        public static string DateToStr(DateTime value)
        {
            return Func.En.DateToStr(value);
        }

        public static string DateToStr(DateTime value, bool upperCase)
        {
            return Func.En.DateToStr(value, upperCase);
        }

        public static string DateToStr(DateTime? value)
        {
            if (!value.HasValue) return string.Empty;
            return Func.En.DateToStr(value.Value);
        }

        public static string DateToStr(DateTime? value, bool upperCase)
        {
            if (!value.HasValue) return string.Empty;
            return Func.En.DateToStr(value.Value, upperCase);
        }
        #endregion

        #region ToCurrencyWords
        public static string ToCurrencyWords(long value)
        {
            return Func.En.CurrToStr(value);
        }

        public static string ToCurrencyWords(decimal value)
        {
            return Func.En.CurrToStr(value);
        }

        public static string ToCurrencyWords(double value)
        {
            return Func.En.CurrToStr(value);
        }

        public static string ToCurrencyWords(long value, bool upperCase)
        {
            return Func.En.CurrToStr(value, upperCase);
        }

        public static string ToCurrencyWords(decimal value, bool upperCase)
        {
            return Func.En.CurrToStr(value, upperCase);
        }

        public static string ToCurrencyWords(double value, bool upperCase)
        {
            return Func.En.CurrToStr(value, upperCase);
        }

        public static string ToCurrencyWords(long value, bool upperCase, bool showCents)
        {
            return Func.En.CurrToStr(value, upperCase, showCents);
        }

        public static string ToCurrencyWords(decimal value, bool upperCase, bool showCents)
        {
            return Func.En.CurrToStr(value, upperCase, showCents);
        }

        public static string ToCurrencyWords(double value, bool upperCase, bool showCents)
        {
            return Func.En.CurrToStr(value, upperCase, showCents);
        }

        public static string ToCurrencyWords(double value, bool upperCase, bool showCents, string dollars, string cents)
        {
            return Func.En.CurrToStr(value, upperCase, showCents, dollars, cents);
        }

        public static string ToCurrencyWords(long value, bool upperCase, bool showCents, string dollars, string cents)
        {
            return Func.En.CurrToStr(value, upperCase, showCents, dollars, cents);
        }

        public static string ToCurrencyWords(decimal value, bool upperCase, bool showCents, string dollars, string cents)
        {
            return Func.En.CurrToStr(value, upperCase, showCents, dollars, cents);
        }

        public static string ToCurrencyWords(long? value)
        {
            if (!value.HasValue) return string.Empty;
            return Func.En.CurrToStr(value.Value);
        }

        public static string ToCurrencyWords(decimal? value)
        {
            if (!value.HasValue) return string.Empty;
            return Func.En.CurrToStr(value.Value);
        }

        public static string ToCurrencyWords(double? value)
        {
            if (!value.HasValue) return string.Empty;
            return Func.En.CurrToStr(value.Value);
        }

        public static string ToCurrencyWords(long? value, bool? upperCase)
        {
            if (!value.HasValue) return string.Empty;
            if (!upperCase.HasValue) upperCase = true;
            return Func.En.CurrToStr(value.Value, upperCase.Value);
        }

        public static string ToCurrencyWords(decimal? value, bool? upperCase)
        {
            if (!value.HasValue) return string.Empty;
            if (!upperCase.HasValue) upperCase = true;
            return Func.En.CurrToStr(value.Value, upperCase.Value);
        }

        public static string ToCurrencyWords(double? value, bool? upperCase)
        {
            if (!value.HasValue) return string.Empty;
            if (!upperCase.HasValue) upperCase = true;
            return Func.En.CurrToStr(value.Value, upperCase.Value);
        }

        public static string ToCurrencyWords(long? value, bool? upperCase, bool? showCents)
        {
            if (!value.HasValue) return string.Empty;
            if (!upperCase.HasValue) upperCase = true;
            if (!showCents.HasValue) showCents = true;
            return Func.En.CurrToStr(value.Value, upperCase.Value, showCents.Value);
        }

        public static string ToCurrencyWords(decimal? value, bool? upperCase, bool? showCents)
        {
            if (!value.HasValue) return string.Empty;
            if (!upperCase.HasValue) upperCase = true;
            if (!showCents.HasValue) showCents = true;
            return Func.En.CurrToStr(value.Value, upperCase.Value, showCents.Value);
        }

        public static string ToCurrencyWords(double? value, bool? upperCase, bool? showCents)
        {
            if (!value.HasValue) return string.Empty;
            if (!upperCase.HasValue) upperCase = true;
            if (!showCents.HasValue) showCents = true;
            return Func.En.CurrToStr(value.Value, upperCase.Value, showCents.Value);
        }

        public static string ToCurrencyWords(double? value, bool? upperCase, bool? showCents, string dollars, string cents)
        {
            if (!value.HasValue) return string.Empty;
            if (!upperCase.HasValue) upperCase = true;
            if (!showCents.HasValue) showCents = true;
            return Func.En.CurrToStr(value.Value, upperCase.Value, showCents.Value, dollars, cents);
        }

        public static string ToCurrencyWords(long? value, bool? upperCase, bool? showCents, string dollars, string cents)
        {
            if (!value.HasValue) return string.Empty;
            if (!upperCase.HasValue) upperCase = true;
            if (!showCents.HasValue) showCents = true;
            return Func.En.CurrToStr(value.Value, upperCase.Value, showCents.Value, dollars, cents);
        }

        public static string ToCurrencyWords(decimal? value, bool? upperCase, bool? showCents, string dollars, string cents)
        {
            if (!value.HasValue) return string.Empty;
            if (!upperCase.HasValue) upperCase = true;
            if (!showCents.HasValue) showCents = true;
            return Func.En.CurrToStr(value.Value, upperCase.Value, showCents.Value, dollars, cents);
        }
        #endregion

        #region ToWordsRu
        public static string ToWordsRu(long value)
        {
            return Func.Ru.NumToStr(value);
        }

        public static string ToWordsRu(decimal value)
        {
            return Func.Ru.NumToStr(value);
        }

        public static string ToWordsRu(double value)
        {
            return Func.Ru.NumToStr(value);
        }

        public static string ToWordsRu(long value, bool upperCase)
        {
            return Func.Ru.NumToStr(value, upperCase);
        }

        public static string ToWordsRu(decimal value, bool upperCase)
        {
            return Func.Ru.NumToStr(value, upperCase);
        }

        public static string ToWordsRu(double value, bool upperCase)
        {
            return Func.Ru.NumToStr(value, upperCase);
        }
        #endregion

        #region DateToStrRu
        public static string DateToStrRu(DateTime value)
        {
            return Func.Ru.DateToStr(value);
        }

        public static string DateToStrRu(DateTime value, bool upperCase)
        {
            return Func.Ru.DateToStr(value, upperCase);
        }
        #endregion

        #region ToCurrencyWordsRu
        public static string ToCurrencyWordsRu(long value)
        {
            return Func.Ru.CurrToStr((decimal)value);
        }

        public static string ToCurrencyWordsRu(decimal value)
        {
            return Func.Ru.CurrToStr(value);
        }

        public static string ToCurrencyWordsRu(double value)
        {
            return Func.Ru.CurrToStr(value);
        }

        public static string ToCurrencyWordsRu(long value, bool upperCase)
        {
            return Func.Ru.CurrToStr(value, upperCase);
        }

        public static string ToCurrencyWordsRu(decimal value, bool upperCase)
        {
            return Func.Ru.CurrToStr(value, upperCase);
        }

        public static string ToCurrencyWordsRu(double value, bool upperCase)
        {
            return Func.Ru.CurrToStr(value, upperCase);
        }

        public static string ToCurrencyWordsRu(decimal value, bool showCents, string currency)
        {
            return Func.Ru.CurrToStr(value, currency, showCents);
        }

        public static string ToCurrencyWordsRu(double value, bool showCents, string currency)
        {
            return Func.Ru.CurrToStr(value, currency, showCents);
        }

        public static string ToCurrencyWordsRu(long value, bool showCents, string currency)
        {
            return Func.Ru.CurrToStr(value, currency, showCents);
        }

        public static string ToCurrencyWordsRu(decimal value, string currency, bool upperCase)
        {
            return Func.Ru.CurrToStr(value, upperCase, currency);
        }

        public static string ToCurrencyWordsRu(double value, string currency, bool upperCase)
        {
            return Func.Ru.CurrToStr(value, upperCase, currency);
        }

        public static string ToCurrencyWordsRu(long value, string currency, bool upperCase)
        {
            return Func.Ru.CurrToStr(value, upperCase, currency);
        }
        #endregion

        #region ToCurrencyWordsThai
        public static string ToCurrencyWordsThai(long value)
        {
            return SP_STRtNumToMny((decimal)value);
        }

        public static string ToCurrencyWordsThai(double value)
        {
            return SP_STRtNumToMny((decimal)value);
        }

        public static string ToCurrencyWordsThai(decimal value)
        {
            return SP_STRtNumToMny(value);
        }

        private static string SP_STRtNumToMny(decimal value)
        {
            string tDecimal = string.Empty;
            string tInteger = string.Empty;
            string[] atSep = null;
            string tBaht = string.Empty;
            string tStang = string.Empty;
            string tResult = string.Empty;
            string ptMoneyNum = string.Empty;

            ptMoneyNum = Math.Round(value, 2).ToString();
            atSep = ptMoneyNum.Split('.');
            if (atSep.GetUpperBound(0) > 1)
            {
                return "";
            }

            tInteger = atSep[0];
            if (atSep.GetUpperBound(0) > 0)
            {
                tDecimal = atSep[1];
                if (tDecimal.Length > 2)
                {

                    return "";
                }
            }

            if (!string.IsNullOrEmpty(tInteger))
            {
                tBaht = SP_XCGtNumToMny(tInteger);
            }
            else
            {
                tBaht = tC_0;
            }
            if (!string.IsNullOrEmpty(tDecimal))
            {
                switch (tDecimal.Length)
                {
                    case 1:
                        tDecimal = tDecimal + "0";
                        break;
                    case 2:
                        break;
                    default:
                        tDecimal = tDecimal.Remove(2);
                        break;
                }
                tStang = SP_XCGtNumToMny(tDecimal);
            }
            else
            {
                tStang = tC_0;
            }

            if ((tBaht == tC_0) && (tStang == tC_0))
            {
                tResult = tBaht + tC_Baht + tC_Complete;

            }
            else if ((tBaht == tC_0) && (tStang != tC_0))
            {
                tResult = tStang + tC_Satang;
            }
            else if ((tBaht != tC_0) && (tStang == tC_0))
            {
                tResult = tBaht + tC_Baht + tC_Complete;
            }
            else if ((tBaht != tC_0) && (tStang != tC_0))
            {
                tResult = tBaht + tC_Baht + tStang + tC_Satang;
            }
            return tResult;
        }

        private static string SP_XCGtNumToMny(string ptMoneyNum)
        {
            string tInteger = ptMoneyNum;
            //string[] atSep;
            if (string.IsNullOrEmpty(ptMoneyNum))
            {
                return "";
            }
            while ((tInteger[0] == '0') && (tInteger.Length > 1))
            {
                tInteger = tInteger.Remove(1, 1);
            }
            tInteger = ReverseString(tInteger);
            int nI = 0;
            string tSum = "";
            char[] tZZZ = tInteger.ToCharArray();
            for (nI = tZZZ.GetLowerBound(0); nI <= tZZZ.GetUpperBound(0); nI++)
            {
                string tUnit = "";
                string tValue = "";
                int nMod = 0;
                nMod = nI % 6;
                switch (tZZZ[nI])
                {
                    case '0':
                        tValue = "";
                        break;
                    case '1':
                        switch (nMod)
                        {
                            case 0:
                                if ((tZZZ.GetUpperBound(0) % 6 == 0) && (nI == tZZZ.GetUpperBound(0)))
                                {
                                    tValue = tC_1;
                                }
                                else
                                {
                                    tValue = tC_01;
                                }
                                break;
                            case 1:
                                tValue = tC_10;
                                break;
                            default:
                                tValue = tC_1;
                                break;
                        }
                        break;
                    case '2':
                        switch (nMod)
                        {
                            case 1:
                                tValue = tC_20;
                                break;
                            default:
                                tValue = tC_2;
                                break;
                        }
                        break;
                    case '3':
                        tValue = tC_3;
                        break;
                    case '4':
                        tValue = tC_4;
                        break;
                    case '5':
                        tValue = tC_5;
                        break;
                    case '6':
                        tValue = tC_6;
                        break;
                    case '7':
                        tValue = tC_7;
                        break;
                    case '8':
                        tValue = tC_8;
                        break;
                    case '9':
                        tValue = tC_9;
                        break;
                }
                if (!string.IsNullOrEmpty(tValue) || (nI % 6 == 0))
                {
                    switch (nMod)
                    {
                        case 0:
                            if (nI / 6 <= 0)
                            {
                                tUnit = "";
                            }
                            else
                            {
                                tUnit = tC_1000000;
                            }
                            break;
                        case 1:
                            if (tValue == tC_10)
                            {
                                tUnit = "";
                            }
                            else
                            {
                                tUnit = tC_10;
                            }
                            break;
                        case 2:
                            tUnit = tC_100;
                            break;
                        case 3:
                            tUnit = tC_1000;
                            break;
                        case 4:
                            tUnit = tC_10000;
                            break;
                        case 5:
                            tUnit = tC_100000;
                            break;
                    }
                }
                tSum = tValue + tUnit + tSum;
            }
            if (string.IsNullOrEmpty(tSum))
            {
                tSum = tC_0;
            }
            return tSum;
        }

        private static string ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        private const string tC_0 = "ศูนย์";
        private const string tC_1 = "หนึ่ง";
        private const string tC_2 = "สอง";
        private const string tC_3 = "สาม";
        private const string tC_4 = "สี่";
        private const string tC_5 = "ห้า";
        private const string tC_6 = "หก";
        private const string tC_7 = "เจ็ด";
        private const string tC_8 = "แปด";
        private const string tC_9 = "เก้า";
        private const string tC_01 = "เอ็ด";
        private const string tC_10 = "สิบ";
        private const string tC_20 = "ยี่";
        private const string tC_100 = "ร้อย";
        private const string tC_1000 = "พัน";
        private const string tC_10000 = "หมื่น";
        private const string tC_100000 = "แสน";
        private const string tC_1000000 = "ล้าน";
        private const string tC_Baht = "บาท";
        private const string tC_Satang = "สตางค์";
        private const string tC_Complete = "ถ้วน";
        #endregion

        #region ToWordsUa
        public static string ToWordsUa(long value)
        {
            return Func.Ua.NumToStr(value);
        }

        public static string ToWordsUa(decimal value)
        {
            return Func.Ua.NumToStr(value);
        }

        public static string ToWordsUa(double value)
        {
            return Func.Ua.NumToStr(value);
        }

        public static string ToWordsUa(long value, bool upperCase)
        {
            return Func.Ua.NumToStr(value, upperCase);
        }

        public static string ToWordsUa(decimal value, bool upperCase)
        {
            return Func.Ua.NumToStr(value, upperCase);
        }

        public static string ToWordsUa(double value, bool upperCase)
        {
            return Func.Ua.NumToStr(value, upperCase);
        }
        #endregion

        #region DateToStrUa
        public static string DateToStrUa(DateTime value)
        {
            return Func.Ua.DateToStr(value);
        }

        public static string DateToStrUa(DateTime value, bool upperCase)
        {
            return Func.Ua.DateToStr(value, upperCase);
        }
        #endregion

        #region ToCurrencyWordsUa
        public static string ToCurrencyWordsUa(long value)
        {
            return Func.Ua.CurrToStr((decimal)value);
        }

        public static string ToCurrencyWordsUa(decimal value)
        {
            return Func.Ua.CurrToStr(value);
        }

        public static string ToCurrencyWordsUa(double value)
        {
            return Func.Ua.CurrToStr(value);
        }

        public static string ToCurrencyWordsUa(long value, bool upperCase)
        {
            return Func.Ua.CurrToStr(value, upperCase);
        }

        public static string ToCurrencyWordsUa(decimal value, bool upperCase)
        {
            return Func.Ua.CurrToStr(value, upperCase);
        }

        public static string ToCurrencyWordsUa(double value, bool upperCase)
        {
            return Func.Ua.CurrToStr(value, upperCase);
        }

        public static string ToCurrencyWordsUa(decimal value, bool upperCase, string currency)
        {
            return Func.Ua.CurrToStr(value, currency, upperCase);
        }

        public static string ToCurrencyWordsUa(double value, bool upperCase, string currency)
        {
            return Func.Ua.CurrToStr(value, currency, upperCase);
        }

        public static string ToCurrencyWordsUa(long value, bool upperCase, string currency)
        {
            return Func.Ua.CurrToStr(value, currency, upperCase);
        }
        #endregion

        #region ToWordsPt
        public static string ToWordsPt(long value, bool upperCase)
        {
            return Func.Pt.NumToStr(value, upperCase);
        }
        #endregion

        #region ToCurrencyWordsPt
        public static string ToCurrencyWordsPt(decimal value, bool upperCase, bool showCents)
        {
            return Func.Pt.CurrToStr(value, upperCase, showCents);
        }
        #endregion

        #region ToCurrencyWordsPtBr
        public static string ToCurrencyWordsPtBr(decimal value)
        {
            return Func.PtBr.NumToStr(value);
        }
        #endregion

        #region DateToStrPt
        public static string DateToStrPt(DateTime value)
        {
            return Func.Pt.DateToStr(value);
        }
        #endregion

        #region DateToStrPtBr
        public static string DateToStrPtBr(DateTime value)
        {
            return Func.Pt.DateToStr(value).ToLowerInvariant();
        }
        #endregion

        #region ToCurrencyWordsFr
        public static string ToCurrencyWordsFr(decimal number, string currencyISO, int decimals)
        {
            return Func.Fr.ConvertToWord(number, currencyISO, decimals);
        }
        #endregion

        #region ToCurrencyWordsEs
        public static string ToCurrencyWordsEs(decimal number, string currencyISO, int decimals)
        {
            return Func.Es.ConvertToWord(number, currencyISO, decimals);
        }
        #endregion

        #region ToWordsEs
        public static string ToWordsEs(long value, bool upperCase)
        {
            return Func.Es.NumToStr(value, upperCase);
        }

        public static string ToWordsEs(long value, bool upperCase, bool female)
        {
            return Func.Es.NumToStr(value, upperCase, female);
        }
        #endregion

        #region ToCurrencyWordsNl
        public static string ToCurrencyWordsNl(decimal number, string currencyISO, int decimals)
        {
            return Func.Nl.ConvertToWord(number, currencyISO, decimals);
        }
        #endregion

        #region ToCurrencyWordsEnGb
        public static string ToCurrencyWordsEnGb(decimal number, string currencyISO, int decimals)
        {
            return Func.EnGb.ConvertToWord(number, currencyISO, decimals);
        }
        #endregion

        #region ToWordsPl
        public static string ToWordsPl(long value, bool upperCase)
        {
            return Func.Pl.NumToStr(value, upperCase);
        }
        #endregion

        #region DateToStrPl
        public static string DateToStrPl(DateTime value, bool upperCase)
        {
            return Func.Pl.DateToStr(value, upperCase);
        }
        #endregion

        #region ToCurrencyWordsPl
        public static string ToCurrencyWordsPl(decimal value, string currencyISO, bool showCents, bool upperCase)
        {
            return Func.Pl.CurrToStr(value, currencyISO, showCents, upperCase);
        }
        #endregion

        #region ToWordsEnIn
        public static string ToWordsEnIn(long value, bool blankIfZero)
        {
            return Func.EnIn.NumberToStr(value, blankIfZero);
        }
        #endregion

        #region ToCurrencyWordsEnIn
        public static string ToCurrencyWordsEnIn(string currencyBasicUnit, string currencyFractionalUnit, decimal value, int decimalPlaces, bool blankIfZero = false)
        {
            return Func.EnIn.CurrencyToStr(currencyBasicUnit, currencyFractionalUnit, value, decimalPlaces, blankIfZero);
        }
        #endregion

        #region ToWordsFa
        public static string ToWordsFa(long value)
        {
            return Func.Fa.ConvertToWord(value);
        }
        #endregion

        #region ToOrdinal
        public static string ToOrdinal(long value)
        {
            string s = value.ToString();

            // Negative and zero have no ordinal representation
            if (value < 1)
            {
                return s;
            }

            value %= 100;
            if ((value >= 11) && (value <= 13))
            {
                return s + "th";
            }

            switch (value % 10)
            {
                case 1: return s + "st";
                case 2: return s + "nd";
                case 3: return s + "rd";
                default: return s + "th";
            }
        }
        #endregion

    }
}
