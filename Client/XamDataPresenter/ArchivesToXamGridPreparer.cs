using Infragistics.Controls.Grids;
using Proryv.AskueARM2.Both.VisualCompHelpers.Comparer;
using Proryv.AskueARM2.Both.VisualCompHelpers.Converters;
using Proryv.AskueARM2.Both.VisualCompHelpers.Interfaces;
using Proryv.AskueARM2.Both.VisualCompHelpers.SummaryCalculators;
using Proryv.AskueARM2.Both.VisualCompHelpers.XamDataPresenter.DataTableExtention;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Proryv.AskueARM2.Both.VisualCompHelpers.XamDataPresenter
{
    public static partial class ArchivesToXamGrid
    {
        public static void ConfigureGrid(XamGrid dataGrid, DataTableEx userTable, enumTimeDiscreteType discreteType, enumTypeInformation typeInformation,
            string format, bool isHalfhours, DataTemplateSelector flagTemplateSelector, bool isCumulateDrums = false, bool isDateTimeGroupingGridControlXam = true,
            IValueConverter converter = null)
        {
            if (dataGrid == null || userTable == null) return;

            var view = userTable.DefaultView;
            
            try
            {
                //dataGrid.BeginInit();
                //dataGrid.FieldSettings.AllowEdit = false;

                view.AllowDelete = view.AllowEdit = view.AllowNew = false;

                //var style = new Style(typeof(XamNumericEditor));
                //style.Setters.Add(new Setter(ValueEditor.FormatProperty, format));

                if (isHalfhours)
                {
                    SetHalfhoursColumnsForXamDataGrid(dataGrid, userTable, discreteType, typeInformation, format, isCumulateDrums, isDateTimeGroupingGridControlXam);
                }
                //else
                //{
                //    SetValidateColumnsForXamDataGrid(dataGrid, userTable, flagTemplateSelector, converter);
                //}

                //view.Table = null;
            }
            finally
            {
             
                //dataGrid.EndInit();
            }

            //dataGrid.Dispatcher.BeginInvoke((System.Action) (() =>
            //{
                dataGrid.ItemsSource = view;
            //}), DispatcherPriority.Background);
        }

        static void SetHalfhoursColumnsForXamDataGrid(XamGrid dataGrid, DataTableEx userTable,
            enumTimeDiscreteType discreteType, enumTypeInformation typeInformation, string format, bool isCumulateDrums, bool isDateTimeGroupingGridControlXam)
        {
            //var nameStyle = dataGrid.FindResource("TIChanelName") as Style;

            Style labelStyle = null;

            //try
            //{
            //    labelStyle = dataGrid.FindResource("LabelObjectStyle") as Style;
            //    dataGrid.FieldSettings.LabelPresenterStyle = labelStyle;
            //}
            //catch
            //{
            //}

            var groupLabelTemplate = dataGrid.FindResource("GroupLabelObjectTemplate") as DataTemplate;
            var channelLabelTemplate = dataGrid.FindResource("ChannelLabelObjectTemplate") as DataTemplate;
            var unchannelLabelTemplate = dataGrid.FindResource("LabelUnchannelTemplate") as DataTemplate;

            var vTemplate = dataGrid.FindResource("FValueTemplate") as DataTemplate;
            var vTemplateNoBindMeasure = Application.Current.FindResource("FValueTemplateNoBindMeasureFormTemplate") as DataTemplate;

            var vFreeHierarchyObjectTemplate = Application.Current.FindResource("FreeHierarchyObjectTemlate") as DataTemplate;

            var columnWidth = new ColumnWidth(105, false);

            //var widthValue = new FieldLength(105);
            //var widthFreeHier = new FieldLength(270);

            var comparer = new IFValueComparer();
            var dtConverter = new DateTimeConverter();
            var dtToolTiptemplate = Application.Current.Resources[ConstantHelper.DateTimeTemplateName] as DataTemplate;

            var measure = dataGrid.FindParent<IMeasure>();
            dataGrid.Resources["IMeasure"] = measure;

            //var fieldLayout = dataGrid.ColumnLayouts[0];

            //TemplateField dtField;

            try
            {
                //fieldLayout.Fields.BeginUpdate();
                //fieldLayout.SortedFields.BeginUpdate();

                var dtCol = userTable.Columns["EventDateTime"];

                var dtField = new TextColumn
                {
                    Key = dtCol.ColumnName,
                    IsReadOnly = true,
                    //DataType = dtCol.DataType,
                    HeaderText = dtCol.Caption,
                    //Width = new FieldLength(95),
                    Width = new ColumnWidth(90, false),
                    ValueConverter = dtConverter,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    AllowToolTips = AllowToolTips.Always,
                    ToolTipContentTemplate = dtToolTiptemplate,
                    DataViewType = typeof(DateTime),
                    IsSortable = true,
                    IsSorted = SortDirection.Ascending,
                    GroupByComparer = new ByDayComparer(),
                    //Settings = { LabelPresenterStyle = dataGrid.FindResource("TimeStyle") as Style }
                };

                dataGrid.Columns.Add(dtField);

                if (isCumulateDrums)
                {
                    //dtField.Settings.CellValuePresenterStyle =
                    //    XamDataGridHelper.DataTemplateToCellValuePresenterStyle(ConstantHelper.DateTimeTemplateName);


                    dtConverter.Parametr = ConstantHelper.DateTimeTemplateName;

                    //dtField.ItemTemplate = Application.Current.Resources[ConstantHelper.DateTimeTemplateName] as DataTemplate;
                }
                else
                {
                    string dataTemplateToCellValuePresenterStyleName;
                    //-------------------------------
                    switch (discreteType)
                    {
                        case enumTimeDiscreteType.DB24Hour:
                            dataTemplateToCellValuePresenterStyleName = ConstantHelper.DateTemplateName;
                            break;
                        case enumTimeDiscreteType.DBMonth:
                            dataTemplateToCellValuePresenterStyleName = ConstantHelper.MonthName;
                            break;
                        default:
                            dataTemplateToCellValuePresenterStyleName = ConstantHelper.DateTimeTemplateName;
                            break;
                    }

                    dtConverter.Parametr = dataTemplateToCellValuePresenterStyleName;
                    
                    
                }

                dtField.HeaderText = "Время";

                //dataGrid.GroupBySettings.GroupByColumns.Add(dtField);

                //if (discreteType == enumTimeDiscreteType.DB24Hour)
                //{
                    //dtField.Settings.GroupByMode = FieldGroupByMode.Month;
                    //dtField.Settings.GroupByRecordPresenterStyle =
                        //Application.Current.FindResource("MonthYearXamDataGridStyle") as Style;
                //}
                //else
                //{
                    //dtField.Settings.GroupByMode = FieldGroupByMode.Date;
                //}


                //SummaryCalculator statType;
                //string prefix;
                //if (typeInformation == enumTypeInformation.Power)
                //{
                //    statType = new FValueAvgCalculator(format);
                //    prefix = "Сред:";
                //}
                //else
                //{
                //    statType = new FValueSumCalculator(format);
                //    prefix = "Сум:";
                //}

                //var stringFormat = prefix + " {0:" + format + "}";
                var stringFormat = " {0:" + format + "}";

                var dfc = new DetailFieldInfoEqualityComparer();

                //FieldGroup fieldGroupByObject = null;
                //FieldGroup fieldGroupByMeasureCategory = null;
                //EnumMeasureUnitCategory? previousMeasureCategory = null; //Для определения групповать или нет по категории 

                foreach (var colGroupByObject in userTable.Columns
                    .Cast<DataColumn>()
                    .Where(c => c.ColumnName != "EventDateTime")
                    .Select(c =>
                    {
                        DetailFieldInfo fieldInfo;
                        userTable.TryGetIndexByItemName(c.ColumnName, out fieldInfo);

                        return new Tuple<DetailFieldInfo, DataColumn>(fieldInfo, c);
                    })
                    .GroupBy(c => c.Item1, dfc))
                {
                    var fieldInfo = colGroupByObject.Key;
                    if (fieldInfo == null || fieldInfo.ChannelType == 0)
                    {
                        //Группировать не нужно
                        foreach (var colByObject in colGroupByObject)
                        {
                            FValueSummaryDefinition summaryDefinition;
                            var fld = AddFieldAndSummary(colByObject.Item2, comparer,
                                fieldInfo != null && fieldInfo.UseMeasureModule, vTemplate, vTemplateNoBindMeasure, columnWidth);

                            if (fieldInfo!=null && fieldInfo.Id!=null)
                            {
                                fld.HeaderTemplate = groupLabelTemplate;
                                fld.Label = colByObject.Item1.Id;
                            }
                            else
                            {
                                fld.HeaderText = colByObject.Item2.Caption;
                            }

                            //fld.HeaderText = (fieldInfo == null ? colByObject.Item2.Caption : (object)colByObject.Item1).ToString();
                            dataGrid.Columns.Add(fld);
                            //if (summaryDefinition != null)
                            //{
                            //    fieldLayout.SummaryDefinitions.Add(summaryDefinition);
                            //}
                        }
                    }
                    else
                    {
                        var fieldGroupByObject = new GroupColumn
                        {
                            //Label = fieldInfo.Id,
                            Key = fieldInfo.ColumnName + "___",
                            Label = fieldInfo.Id,
                            //HeaderText = fieldInfo.Id.ToString(),
                            HeaderTemplate = groupLabelTemplate,
                        };

                        dataGrid.Columns.Add(fieldGroupByObject);

                        //Группируем по объектам
                        foreach (var colGroupByMeasureCategory in colGroupByObject.GroupBy(c => c.Item1.MeasureUnitUn.SubstringQuantityUn()))
                        {
                            var colGroupByMeasureCategoryList = colGroupByMeasureCategory.ToList();

                            if (!string.IsNullOrEmpty(colGroupByMeasureCategory.Key) && colGroupByMeasureCategoryList.Count > 1)
                            {
                                //Есть категории ед. измерения и их несколько
                                var fieldGroupByMeasureCategory = new GroupColumn
                                {
                                    Label = new LabelMeasureQuantity
                                    {
                                        MeasureQuantityType_UN = colGroupByMeasureCategory.Key,
                                    },
                                    Key = fieldInfo.Id + "_" + colGroupByMeasureCategory.Key,
                                };

                                fieldGroupByObject.Columns.Add(fieldGroupByMeasureCategory);

                                foreach (var colByMeasure in colGroupByMeasureCategoryList)
                                {
                                    FValueSummaryDefinition summaryDefinition;
                                    var fld = AddFieldAndSummary(colByMeasure.Item2, comparer, fieldInfo.UseMeasureModule,
                                        vTemplate,
                                        vTemplateNoBindMeasure,
                                        columnWidth);
                                    //isCumulateDrums, statType, stringFormat, fieldLayout, out summaryDefinition);

                                    //fld.Label = colByMeasure.Item1;

                                    fld.HeaderTemplate = colByMeasure.Item1.ChannelType == 0 ? unchannelLabelTemplate : channelLabelTemplate;
                                    fld.Label = colByMeasure.Item1;

                                    fieldGroupByMeasureCategory.Columns.Add(fld);
                                    //if (summaryDefinition != null)
                                    //{
                                    //    fieldLayout.SummaryDefinitions.Add(summaryDefinition);
                                    //}
                                }
                            }
                            else
                            {
                                //Нет категорий, или она одна
                                var fielgGroupByReplacedTiDict = new Dictionary<string, GroupColumn>();
                                foreach (var colByMeasure in colGroupByMeasureCategoryList)
                                {
                                    if (colByMeasure.Item2.ColumnName.StartsWith("ovInfo"))
                                    {
                                        //Это информация об ОВ
                                        //var hoc = new HierObjectsConverterComparer();

                                        var fld = AddField(colByMeasure.Item2, fieldInfo.UseMeasureModule,
                                            vFreeHierarchyObjectTemplate,
                                            vFreeHierarchyObjectTemplate, columnWidth);

                                        //widthFreeHier

                                        fld.Label = "Замещаемые ТИ";
                                        //fld.Settings.FilterOperandUIType = FilterOperandUIType.DropDownList;
                                        //fld.Settings.FilterLabelIconDropDownType = FilterLabelIconDropDownType.MultiSelectExcelStyle;
                                        //fld.Settings.GroupByComparer = hoc;

                                        //fld.ValueToTextConverter = new HierObjectsConverter();
                                        //fld.Settings.FilterComparer = new HierObjectsConverterComparer();
                                        fieldGroupByObject.Columns.Add(fld);
                                    }
                                    else
                                    {
                                        GroupColumn fielgGroupByReplacedTi = null;

                                        //Группируем по замещаемой ТИ
                                        if (colByMeasure.Item2.ColumnName.StartsWith("ovValue") &&
                                            !fielgGroupByReplacedTiDict.TryGetValue(
                                                colByMeasure.Item1.GroupByReplacedTiName, out fielgGroupByReplacedTi))
                                        {
                                            fielgGroupByReplacedTi = new GroupColumn
                                            {
                                                Label = colByMeasure.Item1.ReplacedId,
                                                Key = colByMeasure.Item1.ColumnName + "_",
                                                HeaderText = colByMeasure.Item1.ReplacedId.ToString(),
                                            };

                                            fielgGroupByReplacedTiDict[colByMeasure.Item1.GroupByReplacedTiName] =
                                                fielgGroupByReplacedTi;
                                        }

                                        //FValueSummaryDefinition summaryDefinition;
                                        var fld = AddFieldAndSummary(colByMeasure.Item2, comparer, fieldInfo.UseMeasureModule,
                                            vTemplate,
                                            vTemplateNoBindMeasure,
                                            columnWidth);
                                        //isCumulateDrums, statType, stringFormat, fieldLayout, out summaryDefinition);

                                        fld.Label = colByMeasure.Item1;
                                        fld.HeaderTemplate = colByMeasure.Item1.ChannelType == 0 ? unchannelLabelTemplate : channelLabelTemplate;

                                        if (fielgGroupByReplacedTi != null)
                                        {
                                            fielgGroupByReplacedTi.Columns.Add(fld);
                                        }
                                        else
                                        {
                                            fieldGroupByObject.Columns.Add(fld);
                                        }

                                        //if (summaryDefinition != null)
                                        //{
                                        //    fieldLayout.SummaryDefinitions.Add(summaryDefinition);
                                        //}

                                        //if (colByMeasure.Item1.IsOv)
                                        //{
                                        //    //Если это ОВ, добавляем суммарную информацию о неразнесенных значениях
                                        //    var unWritedOvs = new FValueSummaryDefinition
                                        //    {
                                        //        SourceFieldName = colByMeasure.Item2.ColumnName,
                                        //        Calculator = new FValueSumUnreplacedCalculator(format),
                                        //        StringFormat = stringFormat,
                                        //        UseMeasureModule = fieldInfo.UseMeasureModule,
                                        //    };

                                        //    fieldLayout.SummaryDefinitions.Add(unWritedOvs);
                                        //}
                                    }
                                }

                                if (fielgGroupByReplacedTiDict.Count > 0)
                                {
                                    foreach (var c in fielgGroupByReplacedTiDict.Values)
                                    {
                                        fieldGroupByObject.Columns.Add(c);
                                    }
                                }
                            }
                        }
                    }
                }



                //if (isDateTimeGroupingGridControlXam &&
                //    (discreteType == enumTimeDiscreteType.DBHalfHours || discreteType == enumTimeDiscreteType.DBHours ||
                //     discreteType == enumTimeDiscreteType.DB24Hour))
                //{
                //    //dtField.Settings.AllowSummaries = false;
                //    dtField.Settings.AllowGroupBy = true;


                //    fieldLayout.SortedFields.Add(new FieldSortDescription("EventDateTime", ListSortDirection.Ascending,
                //        true));
                //}
                //else
                //{
                //    dtField.DisplayTemplate = Application.Current.Resources[ConstantHelper.DateTimeTemplateName] as DataTemplate;

                //    //dtField.Settings.CellValuePresenterStyle =
                //    //    XamDataGridHelper.DataTemplateToCellValuePresenterStyle(ConstantHelper.DateTimeTemplateName);
                //}

                //var consumptionScheduleField = fieldLayout.Fields.FirstOrDefault(ff => ff.Name == "ConsumptionSchedule");
                //if (consumptionScheduleField != null)
                //{
                //    consumptionScheduleField.Label = "Тип. потр.";
                //}

                //var consumptionSchedulePercentField = fieldLayout.Fields.FirstOrDefault(ff => ff.Name == "ConsumptionSchedulePercent");
                //if (consumptionSchedulePercentField != null)
                //{
                //    consumptionSchedulePercentField.Label = "Тип. потр. %";
                //}
            }
            finally
            {
                // fieldLayout.SortedFields.EndUpdate();
                //fieldLayout.Fields.EndUpdate();
            }

            //dataGrid.FieldLayoutSettings.FixedFieldUIType = FixedFieldUIType.Splitter;
            //dataGrid.FieldSettings.SummaryDisplayArea = SummaryDisplayAreas.BottomFixed;

            if (userTable.Columns.Count < 20)
            {
                //из-за того что тормоза ограничиваем
                //dataGrid.FieldSettings.SummaryDisplayArea = dataGrid.FieldSettings.SummaryDisplayArea | SummaryDisplayAreas.InGroupByRecords;
            }

            //if (userTable.Columns.Count < 1000)
            //{
            //    dataGrid.RecordsInViewChanged += ExpandFirstRecord;
            //}
        }

        private static Column AddFieldAndSummary(DataColumn col, IFValueComparer comparer, bool useMeasureModule,
            DataTemplate vTemplate, DataTemplate vTemplateNoBindMeasure, ColumnWidth columnWidth)
        {
            var fld = AddField(col, useMeasureModule, vTemplate, vTemplateNoBindMeasure, columnWidth);

            return fld;
        }

        private static Column AddField(DataColumn col, bool useMeasureModule,
           DataTemplate vTemplate, DataTemplate vTemplateNoBindMeasure, ColumnWidth columnWidth)
        {
            var fld = new UnboundColumn
            {
                Key = col.ColumnName,
                Width = columnWidth,
                IsReadOnly = true,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Center,
                DataViewType = col.DataType,
                IsGroupable = false,
                IsFilterable = true,
                ItemTemplate = useMeasureModule ? vTemplate : vTemplateNoBindMeasure,
            };

            //Settings =
            //{
            //    //AllowResize = true,
            //    //LabelTextWrapping = TextWrapping.WrapWithOverflow,
            //    //LabelPresenterStyle = nameStyle,
            //    AutoSizeOptions = FieldAutoSizeOptions.None,
            //    Width = width,
            //    //AllowSummaries = false,
            //    SummaryUIType = SummaryUIType.MultiSelect,
            //    //SummaryDisplayArea = SummaryDisplayAreas.TopFixed,
            //    FilterComparer = comparer,
            //    FilterOperandUIType = FilterOperandUIType.Combo,
            //    //EditAsType = col.DataType,
            //    AllowRecordFiltering = true,
            //},
            //ValueToTextConverter = valueConverter,

            return fld;
        }
    }
}
