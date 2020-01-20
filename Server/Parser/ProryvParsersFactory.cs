using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Proryv.Servers.Calculation.Parser.Internal;

namespace Proryv.Servers.Calculation.Parser
{
    public static class ProryvParsersFactory
    {
        private static volatile bool IsInit;
        private static string _sep;

        public static object ParseTextValue(string inputExpression, ISpreadsheetProperties proryvSpreadsheetObject)
        {
            if (string.IsNullOrEmpty(inputExpression)) return null;
        
            List<ProryvParser.StiAsmCommand> list = null;

            var parser = new ProryvParser();
            parser.SetProryvSpreadsheetObject(proryvSpreadsheetObject);

            Init();

            try
            {
                list = new List<ProryvParser.StiAsmCommand>();
                int counter = 0;
                int pos = 0;
                while (pos < inputExpression.Length)
                {
                    #region Plain text
                    int posBegin = pos;
                    while (pos < inputExpression.Length && inputExpression[pos] != '{') pos++;
                    if (pos != posBegin)
                    {
                        list.Add(new ProryvParser.StiAsmCommand(ProryvParser.StiAsmCommandType.PushValue, inputExpression.Substring(posBegin, pos - posBegin)));
                        counter++;
                        if (counter > 1) list.Add(new ProryvParser.StiAsmCommand(ProryvParser.StiAsmCommandType.Add));
                    }
                    #endregion

                    #region Expression
                    if (pos < inputExpression.Length && inputExpression[pos] == '{')
                    {
                        pos++;
                        posBegin = pos;
                        bool flag = false;
                        while (pos < inputExpression.Length)
                        {
                            if (inputExpression[pos] == '"')
                            {
                                pos++;
                                int pos2 = pos;
                                while (pos2 < inputExpression.Length)
                                {
                                    if (inputExpression[pos2] == '"') break;
                                    if (inputExpression[pos2] == '\\') pos2++;
                                    pos2++;
                                }
                                pos = pos2 + 1;
                                continue;
                            }
                            if (inputExpression[pos] == '}')
                            {
                                string currentExpression = inputExpression.Substring(posBegin, pos - posBegin);
                                if (currentExpression != null && currentExpression.Length > 0)
                                {
                                    parser.expressionPosition = posBegin;
                                    list.AddRange(parser.ParseToAsm(currentExpression));
                                    counter++;
                                    if (counter > 1)
                                    {
                                        list.Add(new ProryvParser.StiAsmCommand(ProryvParser.StiAsmCommandType.Cast, TypeCode.String));
                                        list.Add(new ProryvParser.StiAsmCommand(ProryvParser.StiAsmCommandType.Add));
                                    }
                                }
                                flag = true;
                                pos++;
                                break;
                            }
                            pos++;
                        }
                        if (!flag)
                        {
                            parser.expressionPosition = posBegin;
                            list.Add(new ProryvParser.StiAsmCommand(ProryvParser.StiAsmCommandType.PushValue, inputExpression.Substring(posBegin)));
                            counter++;
                            if (counter > 1) list.Add(new ProryvParser.StiAsmCommand(ProryvParser.StiAsmCommandType.Add));
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                    
                throw ex;
            }

            if (list.Count > 0)
            {
                var v = parser.ExecuteAsm(list);
                return v;
            }
            return null;
        }

        /// <summary>
        /// Делаем расчет того что между {}, расчитанное возвращаем в виде строки
        /// </summary>
        /// <param name="inputExpression"></param>
        /// <param name="proryvSpreadsheetObject"></param>
        /// <returns></returns>
        public static string ParseTextToString(string inputExpression, ISpreadsheetProperties proryvSpreadsheetObject)
        {
            if (inputExpression.IndexOf('{') < 0) return inputExpression;

            var ev = ParseTextValue(inputExpression, proryvSpreadsheetObject);
            return ev != null ? ev.ToString().Replace(_sep, ".") : null;
        }

        private static void Init()
        {
            if (IsInit) return;

            //var cultureName = Thread.CurrentThread.CurrentCulture.Name;
            //var ci = new CultureInfo(cultureName);
            //if (ci.NumberFormat.NumberDecimalSeparator != ",")
            //{
            //    // Forcing use of decimal separator for numerical values
            //    ci.NumberFormat.NumberDecimalSeparator = ",";
            //    Thread.CurrentThread.CurrentCulture = ci;
            //}

            _sep = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            IsInit = true;
        }
    }
}
