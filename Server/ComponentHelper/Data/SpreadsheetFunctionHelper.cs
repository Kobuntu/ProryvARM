using FlexCel.XlsAdapter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using FlexCel.Core;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.Servers.Calculation.Parser;
using Proryv.Servers.Calculation.Parser.Internal;

namespace Proryv.AskueARM2.Server.VisualCompHelpers.Data
{
    /// <summary>
    /// Вспомогательный класс для обсчета пользовательских ф-ий
    /// </summary>
    public class SpreadsheetFunctionHelper
    {
        private static readonly Regex Regex = new Regex(@"\{([^\}]+)\}");

        /// <summary>
        /// Обсчитываем все что выделено между {}
        /// </summary>
        public static void Evaluate(XlsFile xls, ISpreadsheetProperties properties, out int row, StringBuilder errors)
        {
            row = 1;
            var rowCount = Math.Min(xls.RowCount, 100);
            for (var r = 1; r <= rowCount; r++)
            {
                var isNotExistVoidCol = false;
                for (var c = 1; c <= xls.ColCount; c++)
                {
                    var cVal = xls.GetCellValue(r, c);

                    if (!isNotExistVoidCol && cVal != null) isNotExistVoidCol = true;

                    var str = cVal as string;
                    if (!string.IsNullOrEmpty(str))
                    {
                        var indxStart = str.IndexOf('{');
                        if (indxStart >= 0)
                        {
                            var indxEnd = str.IndexOf('}');
                            if (indxEnd > 0)
                            {
                                try
                                {
                                    var subStr = str.Substring(indxStart, indxEnd - indxStart + 1);
                                    var ev = ProryvParsersFactory.ParseTextValue(subStr, properties);
                                    if (str[0] == '=' && ev != null)
                                    {
                                        //Это формула
                                        xls.SetCellValue(r, c, new TFormula(ev.ToString().Replace(',', '.')));
                                    }
                                    else
                                    {
                                        xls.SetCellValue(r, c, str.Replace(subStr, ev.ToString()));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (errors != null)
                                        errors.Append(" Ошибка в ячейке ").Append(TCellAddress.EncodeColumn(c))
                                            .Append(r).Append(": ").Append(ex.Message).Append(" ")
                                            .Append(
                                                ex.InnerException != null ? ex.InnerException.Message : string.Empty)
                                            .Append("\n");
                                }
                            }
                        }
                    }
                }

                //Есть хоть одна заполненная ячейка, смещаем начальную ячейку
                if (isNotExistVoidCol) row = r;
            }

            row++;
        }

        private static readonly Dictionary<string, PropertyInfo> PropertyInfos = new Dictionary<string, PropertyInfo>();
        private static readonly SpreadsheetFormatProvider FormatProvider = new SpreadsheetFormatProvider();

        private static object _spreadsheetPropertiesLock;
        private static List<string> spreadsheetProperties;
        public static List<string> SpreadsheetProperties
        {
            get
            {
                lock (_spreadsheetPropertiesLock)
                {
                    if (spreadsheetProperties != null) return spreadsheetProperties;

                    spreadsheetProperties = new List<string>();


                    return spreadsheetProperties;
                }
            }
        }
    }

    public class SpreadsheetFormatProvider: IFormatProvider
    {
        public object GetFormat(Type formatType)
        {
            return this;
        }
    }
}
