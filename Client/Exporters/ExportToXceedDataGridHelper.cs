using Infragistics.Documents.Excel;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using Proryv.AskueARM2.Both.VisualCompHelpers.Interfaces;
using Proryv.AskueARM2.Both.VisualCompHelpers.TClasses;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.ServiceReference.Service.Dictionary;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;
using Proryv.Servers.Forecast.Client_ServiceReference;
using Proryv.Servers.Forecast.Client_ServiceReference.Common;
using Proryv.Servers.Forecast.Client_ServiceReference.ForecastServiceReference;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Proryv.AskueARM2.Client.Styles.Common
{
    public static class ExportHelper
    {
        private const string _intFormatted = "### ### ### ### ### ### ##0";

        public static string BuildFileName(FrameworkElement grid, out bool isExportCollapsedDetail, out bool excludeGroupBySettings)
        {
            var form = VisualHelper.FindParent<IFormingExportedFileName>(grid);
            string nameFromForm;
            isExportCollapsedDetail = false;

            if (form != null)
            {
                nameFromForm = form.FormingExportedFileName(grid.Name, out isExportCollapsedDetail);
                excludeGroupBySettings = form.ExportExcludeGroupBySettings(grid.Name);
            }
            else
            {
                nameFromForm = string.Empty;
                excludeGroupBySettings = false;
            }

            return nameFromForm;
        }

        public static void SetCellValue(WorksheetCell cell, object value, 
            IMeasure measured = null, object tag = null, string fieldName = null)
        {
            var @switch = new TypeSwitch<object>()
                .Case((int i) =>
                {
                    //cell.CellFormat.Alignment = HorizontalCellAlignment.Right;
                    return i;
                })
                .Case((double d) =>
                {
                    if (tag != null && string.Equals(tag.ToString(), "UseMeasureConverter"))
                    {
                        if (measured != null && measured.UseBindedConverter)
                        {
                            var ud = (double)measured.UsedUnitDigit / (double)measured.SelectedUnitDigit;

                            if (ud != 1) d = Math.Round(d, 5) * ud;
                            else d = Math.Round(d, 5);
                        }
                        else
                        {
                            d = Math.Round(d, 7);
                        }
                    }

                    if ((d % 1) != 0)
                    {
                        cell.CellFormat.FormatString = CommonEx.ExcelformatString;
                    }
                    else
                    {
                        cell.CellFormat.FormatString = _intFormatted;
                    }

                    return d;
                })
                .Case((DateTime dt) =>
                {
                    string formatString;

                    if (tag != null)
                    {
                        var templateName = tag.ToString();
                        switch (templateName)
                        {
                            case ConstantHelper.HalfHourstoRangeTimeTemplateName:
                            case ConstantHelper.HourstoRangeTimeTemplateName:
                                formatString = "HH:mm";
                                var dtTo = dt.TimeOfDay.Add(templateName == ConstantHelper.HourstoRangeTimeTemplateName ? GlobalEnums.Delta60Minute : GlobalEnums.Delta30Minute);
                                formatString += "\"-" + dtTo.Hours.ToString("00") + ":" + dtTo.Minutes.ToString("00") + "\"";
                                if (dt.TimeOfDay == TimeSpan.Zero) formatString = "dd.MM.yyyy " + formatString;
                                break;
                            case ConstantHelper.DateTemplateName:
                                formatString = "dd.MM.yyyy";
                                break;
                            default:
                                formatString = "dd.MM.yyyy HH:mm";
                                break;
                        }
                    }
                    else
                    {
                        formatString = "dd.MM.yyyy HH:mm";
                    }

                    cell.CellFormat.FormatString = formatString;
                    return dt;
                })
                .Case((TPS_isCA ps) =>
                {
                    //
                    if (ps.IsCA) return EnumClientServiceDictionary.ContrPSList[ps.PS_ID];
                    return EnumClientServiceDictionary.DetailPSList[ps.PS_ID].HierarchyObject;
                })
                .Case((TFormulaValidateByOneValues ti) =>
                {
                    //
                    if (ti.TI_Ch_ID.IsCA) return EnumClientServiceDictionary.TICAList[ti.TI_Ch_ID.TI_ID];
                    return EnumClientServiceDictionary.TIHierarchyList[ti.TI_Ch_ID.TI_ID];
                })
                .Case((TGroupTPResult tpResult) =>
                {
                    if (string.IsNullOrEmpty(fieldName)) return null;

                    switch (fieldName)
                    {
                        case "PowerMax":
                            {
                                //Здась надо учесть IsAbsolutely
                                return tpResult.PowerMax;
                            }
                        case "PowerFactUsed":
                            return tpResult.PowerFactUsed != null ? tpResult.PowerFactUsed.F_VALUE : 0;
                        case "PowerMaxReserved":
                            return tpResult.PowerMaxReserved;
                        case "GroupTP_MAX_VALUE.EventDateTime":
                            return tpResult.TotalPowerInfoInSystemOperatorsHours != null &&
                            tpResult.TotalPowerInfoInSystemOperatorsHours.GroupTP_MAX_VALUE != null ?
                            tpResult.TotalPowerInfoInSystemOperatorsHours.GroupTP_MAX_VALUE.EventDateTime as DateTime? : null;
                    }

                    return null;
                })
                .Case((enumTypeHierarchy th) =>
                {
                    string hierarchyName;
                    if (GlobalEnumsDictionary.EnumTypeHierarchyName.TryGetValue(th, out hierarchyName)) return hierarchyName;

                    return th.ToString();
                })
                .Case((bool b) =>
                {
                    if (b)
                    {
                        cell.CellFormat.Alignment = HorizontalCellAlignment.Center;
                        return char.ConvertFromUtf32(0xF0FE);
                    }
                    return string.Empty;
                })
                .Case((EnumForecastObjectFixedType foft) =>
                {
                    string resource;
                    return !ServiceDictionary.ForecastObjectFixedTypeDictionary.TryGetValue(foft, out resource) ? string.Empty : resource;
                })
                .Case((TTariffPeriodID ch) =>
                {
                    var chName = string.Empty;
                    if (!ChannelFactory.ChanelTypeNameFSK.TryGetValue(ch.ChannelType, out chName))
                    {
                        chName = " <?>";
                    }

                    return ChannelDictionaryClass.MakeZoneNamePrefix(ch.TI_ID, ch.ChannelType, ch.StartDateTime, ch.FinishDateTime) + chName;
                })
                .Case((IFValue fv) =>
                {
                    double v;

                    if (fieldName == "Value_Section" || fieldName == "Value_CA") v = fv.F_VALUE;
                    else
                    {
                        ServiceReference.ARM_20_Service.EnumUnitDigit? unitDigit;
                        if (measured != null && measured.UseBindedConverter)
                        {
                            var ud = (double)measured.UsedUnitDigit / (double)measured.SelectedUnitDigit;

                            if (ud != 1) v = Math.Round(fv.F_VALUE, 5) * ud;
                            else v = Math.Round(fv.F_VALUE, 5);

                            unitDigit = measured.SelectedUnitDigit;
                        }
                        else
                        {
                            v = Math.Round(fv.F_VALUE, 7);
                            unitDigit = null;
                        }

                        //Если нужно форматировать, то раскоментировать здесь, но почему то тормоза
                        if ((v % 1) != 0)
                        {
                            cell.CellFormat.FormatString = CommonEx.GetExcelFormatString(unitDigit);
                        }
                        else
                        {
                            cell.CellFormat.FormatString = _intFormatted;
                        }

                        Color color;
                        if (fv.F_FLAG.HasFlag(ServiceReference.ARM_20_Service.VALUES_FLAG_DB.DataNotComplete) ||
                            (fv.F_FLAG.HasFlag(ServiceReference.ARM_20_Service.VALUES_FLAG_DB.NotCorrect)))
                            color = Color.IndianRed;
                        else if (fv.F_FLAG.HasFlag(ServiceReference.ARM_20_Service.VALUES_FLAG_DB.DataNotFull))
                            color = Color.LightGray;
                        else
                            color = Color.LightGreen;

                        //if (cell.Comment == null) cell.Comment = new WorksheetCellComment();
                        //cell.Comment.Text = new FormattedString(TVALUES_DB.FLAG_to_String(valuesDb.F_FLAG, ",\n")); 
                        cell.CellFormat.Alignment = HorizontalCellAlignment.Right;
                        cell.CellFormat.Fill = new CellFillPattern(new WorkbookColorInfo(color), new WorkbookColorInfo(color), FillPatternStyle.Solid);
                    }

                    return v;
                })
                .Case((IForecastValue fVal) =>
                {
                    double v;

                    if (measured != null && measured.UseBindedConverter)
                    {
                        v = fVal.F_VALUE / (double)measured.SelectedUnitDigit;
                    }
                    else
                    {
                        v = fVal.F_VALUE;
                    }

                    cell.CellFormat.FormatString = CommonEx.ExcelformatString;

                    Color color;

                    if (fVal.F_FLAG.HasFlag(Proryv.Servers.Forecast.Client_ServiceReference.ForecastServiceReference.EnumForecastValueValid.DataNotComplete) ||
                        (fVal.F_FLAG.HasFlag(Proryv.Servers.Forecast.Client_ServiceReference.ForecastServiceReference.EnumForecastValueValid.NotCorrect)))
                        color = Color.IndianRed;
                    else if (fVal.F_FLAG.HasFlag(Proryv.Servers.Forecast.Client_ServiceReference.ForecastServiceReference.EnumForecastValueValid.DataNotFull))
                        color = Color.LightGray;
                    else
                        color = Color.LightGreen;

                    //if (cell.Comment == null) cell.Comment = new WorksheetCellComment();
                    //cell.Comment.Text = new FormattedString(TFORECAST_DB.ForecastFlagToString(fVal.F_FLAG, "\n"));

                    cell.CellFormat.Alignment = HorizontalCellAlignment.Right;
                    cell.CellFormat.Fill = new CellFillPattern(new WorkbookColorInfo(color), new WorkbookColorInfo(color), FillPatternStyle.Solid);


                    //if (cell.Comment == null) cell.Comment = new WorksheetCellComment();
                    //cell.Comment.Text = new FormattedString(TFORECAST_DB.ForecastFlagToString(fVal.F_FLAG, "\n"));

                    //cell.CellFormat.Alignment = HorizontalCellAlignment.Right;
                    //cell.CellFormat.Fill = new CellFillPattern(new WorkbookColorInfo(color), new WorkbookColorInfo(color), FillPatternStyle.Solid);

                    if ((v % 1) != 0)
                    {
                        cell.CellFormat.FormatString = CommonEx.ExcelformatString;
                    }
                    else
                    {
                        cell.CellFormat.FormatString = _intFormatted;
                    }

                    return v;
                })
                .Case((RequestStatus rs) =>
                {
                    cell.CellFormat.Alignment = HorizontalCellAlignment.Center;
                    switch (rs)
                    {
                        case RequestStatus.Aborted:
                            return "Отклонен";
                        case RequestStatus.Applied:
                            return "Утвержден";
                        case RequestStatus.Created:
                            return "На рассмотрении";
                    }

                    return string.Empty;
                    //e.FormatSettings.HorizontalAlignment = HorizontalCellAlignment.Center;
                })
                .Case((List<KeyValuePair<int, int>> list) =>
                {
                    var res = string.Empty;
                    foreach (var l in list)
                    {
                        res += (res.Length > 0 ? ", " : "") + EnumClientServiceDictionary.TIHierarchyList[l.Value];
                    }
                    return res;
                })
                .Case((enumChannelType ct) =>
                {
                    return ChannelFactory.ChanelTypeNameFSK[(int)ct];
                })
                .Case((IFreeHierarchyObject fho) =>
                {
                    return fho.Name;
                })
                .Case((ServiceReference.ARM_20_Service.VALUES_FLAG_DB flag) =>
                {
                    return FlagToCell(flag, cell);
                })
                .Case((enumPowerFlag flag) =>
                {
                    return FlagToCell(flag, cell);
                })
                .Case((ulong l) =>
                {
                    var flag = (ServiceReference.ARM_20_Service.VALUES_FLAG_DB)l;

                    //if (!Enum.IsDefined(typeof(VALUES_FLAG_DB), flag)) return l;

                    return FlagToCell(flag, cell);
                })
                .Case((IEnumerable enumerable) =>
                {
                    //var sb = new StringBuilder();
                    //foreach (var v in (IEnumerable)value)
                    //{
                    //    sb.Append(v).Append("\n");
                    //}
                    //return sb.ToString();
                    return string.Empty;
                })
                .Case((ID_TypeHierarchy id) =>
                {
                    string un;
                    if (id.ID > 0) un = id.ID.ToString();
                    else un = id.StringId;

                    var hierObject = HierarchyObjectHelper.ToHierarchyObject(un, id.TypeHierarchy);
                    if (hierObject != null) return hierObject.Name;

                    return string.Empty;
                })
                .Case((ObjectIdCollection ids) =>
                {
                    var result = new StringBuilder();

                    foreach (var id in ids.Source)
                    {
                        var hierObject = HierarchyObjectHelper.ToHierarchyObject(id.ID, id.TypeHierarchy);
                        if (hierObject != null)
                        {
                            result.Append(hierObject.Name).Append(" ; ");
                        }
                    }

                    return result.ToString();
                })
                //.Case((ArchTechValue archTech) =>
                //{
                //    double v = archTech.F_VALUE * 1000;
                //    if (measured != null && measured.UseBindedConverter)
                //    {
                //        v = archTech.F_VALUE / (double)measured.SelectedUnitDigit;
                //    }
                //    else
                //    {
                //        v = archTech.F_VALUE;
                //    }

                //    if ((v % 1) != 0)
                //    {
                //        cell.CellFormat.FormatString = CommonEx.ExcelformatString;
                //    }
                //    else
                //    {
                //        cell.CellFormat.FormatString = _intFormatted;
                //    }

                //    Color color;
                //    if (archTech.F_FLAG.HasFlag(EnumArchTechStatus.DataNotComplete) ||
                //        (archTech.F_FLAG.HasFlag(EnumArchTechStatus.NotCorrect)))
                //        color = Color.IndianRed;
                //    else if (archTech.F_FLAG.HasFlag(EnumArchTechStatus.DataNotFull))
                //        color = Color.LightGray;
                //    else
                //        color = Color.LightGreen;

                //    cell.CellFormat.Alignment = HorizontalCellAlignment.Right;
                //    cell.CellFormat.Fill = new CellFillPattern(new WorkbookColorInfo(color), new WorkbookColorInfo(color), FillPatternStyle.Solid);

                //    return v;
                //})
                .Case((long l) =>
                {
                    return l;
                });

            cell.Value = @switch.Switch(value);
        }

        private static object FlagToCell(ServiceReference.ARM_20_Service.VALUES_FLAG_DB flag, WorksheetCell cell)
        {
            Color color;
            if (flag.HasFlag(ServiceReference.ARM_20_Service.VALUES_FLAG_DB.DataNotComplete) ||
                (flag.HasFlag(ServiceReference.ARM_20_Service.VALUES_FLAG_DB.NotCorrect)))
                color = Color.IndianRed;
            else if (flag.HasFlag(ServiceReference.ARM_20_Service.VALUES_FLAG_DB.DataNotFull))
                color = Color.LightGray;
            else
                color = Color.LightGreen;

            cell.CellFormat.Alignment = HorizontalCellAlignment.Center;
            cell.CellFormat.VerticalAlignment = VerticalCellAlignment.Center;
            cell.CellFormat.Fill = new CellFillPattern(new WorkbookColorInfo(color), new WorkbookColorInfo(color), FillPatternStyle.Solid);

            return ServiceReference.ARM_20_Service.TVALUES_DB.FLAG_to_String(flag, ",\n");
        }

        private static object FlagToCell(enumPowerFlag flag, WorksheetCell cell)
        {
            //Color color;
            //if (flag.HasFlag(VALUES_FLAG_DB.DataNotComplete) ||
            //    (flag.HasFlag(VALUES_FLAG_DB.NotCorrect)))
            //    color = Color.IndianRed;
            //else if (flag.HasFlag(VALUES_FLAG_DB.DataNotFull))
            //    color = Color.LightGray;
            //else
            //    color = Color.LightGreen;

            cell.CellFormat.Alignment = HorizontalCellAlignment.Center;
            cell.CellFormat.VerticalAlignment = VerticalCellAlignment.Center;
            //cell.CellFormat.Fill = new CellFillPattern(new WorkbookColorInfo(color), new WorkbookColorInfo(color), FillPatternStyle.Solid);
            string result;
            if (GlobalTreeDictionary.PowerFlagDictionary.TryGetValue(flag, out result)) return result;

            return string.Empty;
        }
    }
}
