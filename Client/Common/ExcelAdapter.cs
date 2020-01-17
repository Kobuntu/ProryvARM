using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;

namespace Proryv.AskueARM2.Both.VisualCompHelpers
{
    public static class ExcelAdapter
    {
        public static void ConvertClipboardValuesFromVt(bool fromVt, EnumUnitDigit selectedUnitDigits,
            string cultureName, string subformatString, Action<string> onError)
        {
            var result = new StringBuilder();
            try
            {
                var cl = Clipboard.GetText();
                var ci = new CultureInfo(cultureName);
                var rows = cl.Split(new[] { "\r\n" }, StringSplitOptions.None);
                foreach (var row in rows.Take(rows.Length - 1))
                {
                    if (row != null)
                    {
                        var cols = row.Split(new[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                        if (cols.Length > 0)
                        {
                            foreach (var col in cols)
                            {
                                var indScope = col.IndexOf('(');
                                string text;

                                if (indScope > 0) text = col.Substring(0, indScope);
                                else text = col;

                                double v;
                                if (double.TryParse(text, NumberStyles.Any, ci, out v) || double.TryParse(text, out v))
                                {
                                    if (fromVt) v = v / (double)selectedUnitDigits; //Преобразуем из Вт
                                    else v = v * (double)selectedUnitDigits; //Преобразуем в Вт

                                    result.Append(v.ToString(subformatString, ci).Trim()).Append("\t");
                                }
                                else
                                {
                                    result.Append(col).Append("\t");
                                }
                            }

                            result.Remove(result.Length - 1, 1);
                        }

                        result.Append(" \r\n");
                    }
                }

                // result.Remove(result.Length - 4, 4);
            }
            catch (Exception ex)
            {
                if (onError!=null) onError(ex.Message);
            }

            try
            {
                //Здесь могут быть проблемы в XP, microsoft не пофиксили до сих пор
                Clipboard.SetDataObject(result.ToString());
            }
            catch
            {
            }
        }
    }
}
