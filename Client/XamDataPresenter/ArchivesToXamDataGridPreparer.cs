using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Infragistics.Windows.Controls;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Editors;
using Proryv.AskueARM2.Both.VisualCompHelpers.Converters;
using Proryv.AskueARM2.Both.VisualCompHelpers.Interfaces;
using Proryv.AskueARM2.Both.VisualCompHelpers.SummaryCalculators;
using Proryv.AskueARM2.Both.VisualCompHelpers.XamDataPresenter.DataTableExtention;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Proryv.AskueARM2.Client.ServiceReference.Service;

namespace Proryv.AskueARM2.Both.VisualCompHelpers.XamDataPresenter
{
    public static partial class ArchivesToXamDataGrid
    {
        public static void ConfigureGrid(XamDataGrid dataGrid, DataTableEx userTable, enumTimeDiscreteType discreteType, enumTypeInformation typeInformation,
            string format, bool isHalfhours, bool isCumulateDrums = false, bool isDateTimeGroupingGridControlXam = true, bool useBindedConverter = true)
        {
            if (dataGrid == null || userTable == null) return;

            var view = userTable.DefaultView;

            try
            {
                dataGrid.BeginInit();
                dataGrid.FieldSettings.AllowEdit = false;

                view.AllowDelete = view.AllowEdit = view.AllowNew = false;

                //var style = new Style(typeof(XamNumericEditor));
                //style.Setters.Add(new Setter(ValueEditor.FormatProperty, format));

                if (isHalfhours)
                {
                    SetHalfhoursColumnsForXamDataGrid(dataGrid, userTable, discreteType, typeInformation, format, isCumulateDrums, isDateTimeGroupingGridControlXam, useBindedConverter);
                }
                else
                {
                    SetValidateColumnsForXamDataGrid(dataGrid, userTable);
                }

                //view.Table = null;
            }
            finally
            {
                dataGrid.DataSource = view;
                dataGrid.EndInit();
            }

            //dataGrid.Dispatcher.BeginInvoke((System.Action) (() =>
            //{

            //}), DispatcherPriority.Background);
        }

        static void SetHalfhoursColumnsForXamDataGrid(XamDataGrid dataGrid, DataTableEx userTable,
            enumTimeDiscreteType discreteType, enumTypeInformation typeInformation, string format, bool isCumulateDrums
            , bool isDateTimeGroupingGridControlXam, bool useBindedConverter)
        {
            //var nameStyle = dataGrid.FindResource("TIChanelName") as Style;

            ////Style labelStyle = null;

            try
            {
                var labelStyle = dataGrid.FindResource("LabelObjectStyle") as Style;
                dataGrid.FieldSettings.LabelPresenterStyle = labelStyle;
            }
            catch
            {
            }

            //var vTemplate = dataGrid.FindResource("FValueTemplate") as DataTemplate;
            var fValueNoBindMeasureStyle = dataGrid.FindResource("FValueNoBindMeasureStyle") as Style;

            var vFreeHierarchyObjectTemplate = Application.Current.FindResource("FreeHierarchyObjectTemlate") as DataTemplate;
            var fValueStyle = useBindedConverter ?  dataGrid.FindResource("FValueStyle") as Style : fValueNoBindMeasureStyle;

            var widthValue = new FieldLength(105);
            var widthFreeHier = new FieldLength(270);

            var comparer = new IFValueComparer();

            var measure = dataGrid.FindParent<IMeasure>();
            dataGrid.Resources["IMeasure"] = measure;

            //var fConverter = new FValueConverter
            //{
            //    FParameter = new FValueParameter
            //    {
            //        Measure = measure,
            //    }
            //};

            var fieldLayout = dataGrid.FieldLayouts[0];
            var fields = fieldLayout.Fields;

            try
            {
                fields.BeginUpdate();
                //fieldLayout.SortedFields.BeginUpdate();

                var dtCol = userTable.Columns["EventDateTime"];

                var dtField = new TemplateField
                {
                    Name = dtCol.ColumnName,
                    DataType = dtCol.DataType,
                    Label = dtCol.Caption,
                    Width = new FieldLength(95),
                    AllowFixing = AllowFieldFixing.Default,
                    FixedLocation = FixedFieldLocation.FixedToNearEdge,
                    Settings =
                    {
                        LabelPresenterStyle = dataGrid.FindResource("TimeStyle") as Style
                    }
                };

                fields.Add(dtField);

                if (isCumulateDrums)
                {
                    //dtField.Settings.CellValuePresenterStyle =
                    //    XamDataGridHelper.DataTemplateToCellValuePresenterStyle(ConstantHelper.DateTimeTemplateName);
                    dtField.DisplayTemplate = Application.Current.Resources[ConstantHelper.DateTimeTemplateName] as DataTemplate;
                }
                else
                {
                    string dataTemplateToCellValuePresenterStyleName;
                    //-------------------------------
                    switch (discreteType)
                    {
                        case enumTimeDiscreteType.DBHalfHours:
                            dataTemplateToCellValuePresenterStyleName = ConstantHelper.HalfHourstoRangeTimeTemplateName;
                            break;
                        case enumTimeDiscreteType.DBHours:
                            dataTemplateToCellValuePresenterStyleName = ConstantHelper.HourstoRangeTimeTemplateName;
                            break;
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

                    dtField.Tag = dataTemplateToCellValuePresenterStyleName; //Это нужно для форматирования при выгрузке в Excel
                    dtField.DisplayTemplate = Application.Current.Resources[dataTemplateToCellValuePresenterStyleName] as DataTemplate;

                    //dtField.Settings.CellValuePresenterStyle =
                    //    XamDataGridHelper.DataTemplateToCellValuePresenterStyle(
                    //        dataTemplateToCellValuePresenterStyleName);
                }

                dtField.Label = "Время";

                if (discreteType == enumTimeDiscreteType.DB24Hour)
                {
                    dtField.Settings.GroupByMode = FieldGroupByMode.Month;
                    dtField.Settings.GroupByRecordPresenterStyle =
                        Application.Current.FindResource("MonthYearXamDataGridStyle") as Style;
                }
                else
                {
                    dtField.Settings.GroupByMode = FieldGroupByMode.Date;
                }


                SummaryCalculator statType;
                string prefix;
                if (typeInformation == enumTypeInformation.Power)
                {
                    statType = new FValueAvgCalculator(format);
                    prefix = "Сред:";
                }
                else
                {
                    statType = new FValueSumCalculator(format);
                    prefix = "Сум:";
                }

                var stringFormat = prefix + " {0:" + format + "}";

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

                    if (colGroupByObject.Key == null || (colGroupByObject.Key.ChannelType == 0 && colGroupByObject.Key.ColumnType == EnumColumnType.None))
                    {
                        //Группировать не нужно
                        foreach (var colByObject in colGroupByObject)
                        {
                            DetailFieldInfo fieldInfo;
                            userTable.ItemsIndexesDict.TryGetValue(colByObject.Item2.ColumnName, out fieldInfo);

                            SummaryDefinition summaryDefinition;
                            var fld = AddFieldAndSummary(colByObject.Item2, widthValue, comparer, fieldInfo != null && fieldInfo.UseMeasureModule,
                                isCumulateDrums, statType, stringFormat, out summaryDefinition, fValueStyle);

                            fld.Label = fieldInfo == null ? colByObject.Item2.Caption : (object)colByObject.Item1;
                            fields.Add(fld);
                            if (summaryDefinition != null)
                            {
                                fieldLayout.SummaryDefinitions.Add(summaryDefinition);
                            }
                        }
                    }
                    //else if ()
                    //{
                    //    //Группируем по типу
                    //    foreach (var colByObjectByColumnType in colGroupByObject.GroupBy(c => c.Item1.ColumnType))
                    //    {
                    //        foreach (var colByObject in colByObjectByColumnType)
                    //        {
                    //            DetailFieldInfo fieldInfo;
                    //            if (!userTable.ItemsIndexesDict.TryGetValue(colByObject.Item2.ColumnName, out fieldInfo)) continue;

                    //            FValueSummaryDefinition summaryDefinition;
                    //            var fld = AddFieldAndSummary(colByObject.Item2, widthValue, comparer, fieldInfo != null && fieldInfo.UseMeasureModule,
                    //                isCumulateDrums, statType, stringFormat, out summaryDefinition, fValueStyle);

                    //            fld.Label = fieldInfo == null ? colByObject.Item2.Caption : (object)colByObject.Item1;
                    //            fields.Add(fld);
                    //            if (summaryDefinition != null)
                    //            {
                    //                fieldLayout.SummaryDefinitions.Add(summaryDefinition);
                    //            }
                    //        }
                    //    }
                    //}
                    else
                    {
                        var fieldInfo = colGroupByObject.Key;
                        var fieldGroupByObject = new FieldGroup
                        {
                            Label = fieldInfo.Id,
                        };

                        fieldLayout.FieldItems.Add(fieldGroupByObject);

                        //Группируем по ед.измерения
                        foreach (var colGroupByMeasureCategory in colGroupByObject.GroupBy(c => c.Item1.MeasureUnitUn.SubstringQuantityUn()))
                        {
                            var colGroupByMeasureCategoryList = colGroupByMeasureCategory.ToList();

                            if (!string.IsNullOrEmpty(colGroupByMeasureCategory.Key) && colGroupByMeasureCategoryList.Count > 1)
                            {
                                //Есть категории ед. измерения и их несколько
                                var fieldGroupByMeasureCategory = new FieldGroup
                                {
                                    Label = new LabelMeasureQuantity
                                    {
                                        MeasureQuantityType_UN = colGroupByMeasureCategory.Key,
                                    },
                                };

                                fieldGroupByObject.Children.Add(fieldGroupByMeasureCategory);

                                foreach (var colByMeasure in colGroupByMeasureCategoryList)
                                {
                                    SummaryDefinition summaryDefinition;
                                    var fld = AddFieldAndSummary(colByMeasure.Item2, widthValue, comparer, fieldInfo.UseMeasureModule,
                                        isCumulateDrums, statType, stringFormat, out summaryDefinition, fValueStyle);

                                    fld.Label = colByMeasure.Item1;
                                    fieldGroupByMeasureCategory.Children.Add(fld);
                                    if (summaryDefinition != null)
                                    {
                                        fieldLayout.SummaryDefinitions.Add(summaryDefinition);
                                    }
                                }
                            }
                            else
                            {
                                var fielgGroupByReplacedTiDict = new Dictionary<string, FieldGroup>();

                                //Нет категорий, или она одна
                                foreach (var colByMeasure in colGroupByMeasureCategoryList)
                                {

                                    if (colByMeasure.Item2.ColumnName.StartsWith("ovInfo"))
                                    {
                                        var hoc = new HierObjectsConverterComparer();

                                        //var fld = AddField(colByMeasure.Item2, widthFreeHier, hoc, fieldInfo.UseMeasureModule,
                                        //    vFreeHierarchyObjectTemplate,
                                        //    vFreeHierarchyObjectTemplate, new HierObjectsConverter(), null);

                                        var fld = new TemplateField
                                        {
                                            Name = colByMeasure.Item2.ColumnName,
                                            DataType = colByMeasure.Item2.DataType,
                                            Width = widthFreeHier,
                                            AllowFixing = AllowFieldFixing.No,
                                            DisplayTemplate = vFreeHierarchyObjectTemplate,
                                            Settings =
                                            {
                                                AutoSizeOptions = FieldAutoSizeOptions.None,
                                                Width = widthFreeHier,
                                                SummaryUIType = SummaryUIType.MultiSelect,
                                                FilterComparer = hoc,
                                                FilterOperandUIType = FilterOperandUIType.Combo,
                                                AllowRecordFiltering = true,
                                            },
                                            HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                            VerticalContentAlignment = VerticalAlignment.Stretch,
                                        };

                                        fld.Label = "Замещаемые ТИ";
                                        fld.Settings.FilterOperandUIType = FilterOperandUIType.DropDownList;
                                        fld.Settings.FilterLabelIconDropDownType = FilterLabelIconDropDownType.MultiSelectExcelStyle;
                                        fld.Settings.GroupByComparer = hoc;

                                        //fld.ValueToTextConverter = new HierObjectsConverter();
                                        fld.Settings.FilterComparer = new HierObjectsConverterComparer();
                                        fieldGroupByObject.Children.Add(fld);
                                    }
                                    else
                                    {
                                        FieldGroup fielgGroupByReplacedTi = null;

                                        //Группируем по замещаемой ТИ
                                        if (colByMeasure.Item2.ColumnName.StartsWith("ovValue") &&
                                            !fielgGroupByReplacedTiDict.TryGetValue(
                                                colByMeasure.Item1.GroupByReplacedTiName, out fielgGroupByReplacedTi))
                                        {
                                            fielgGroupByReplacedTi = new FieldGroup
                                            {
                                                Label = colByMeasure.Item1.ReplacedId,
                                            };

                                            fielgGroupByReplacedTiDict[colByMeasure.Item1.GroupByReplacedTiName] =
                                                fielgGroupByReplacedTi;
                                        }

                                        SummaryDefinition summaryDefinition;
                                        Field fld;

                                        if (!string.IsNullOrEmpty(colByMeasure.Item1.MeasureUnitUn) && colByMeasure.Item1.MeasureUnitUn.IndexOf("RatioUnit") > -1)
                                        {
                                            //Это %, нужно усреднять
                                            fld = AddFieldAndSummary(colByMeasure.Item2, widthValue, comparer, colByMeasure.Item1.UseMeasureModule,
                                                isCumulateDrums, new FValueAvgCalculator(format), "Сред: {0:" + format + "}", out summaryDefinition, fValueNoBindMeasureStyle);
                                        }
                                        else
                                        {
                                            fld = AddFieldAndSummary(colByMeasure.Item2, widthValue, comparer, useBindedConverter && colByMeasure.Item1.UseMeasureModule,
                                                isCumulateDrums, statType, stringFormat, out summaryDefinition, fValueStyle);
                                        }

                                        fld.Label = colByMeasure.Item1;

                                        if (fielgGroupByReplacedTi != null)
                                        {
                                            fielgGroupByReplacedTi.Children.Add(fld);
                                        }
                                        else
                                        {
                                            fieldGroupByObject.Children.Add(fld);
                                        }

                                        if (summaryDefinition != null)
                                        {
                                            fieldLayout.SummaryDefinitions.Add(summaryDefinition);
                                        }

                                        if (colByMeasure.Item1.IsOv)
                                        {
                                            //Если это ОВ, добавляем суммарную информацию о неразнесенных значениях
                                            var unWritedOvs = new FValueSummaryDefinition
                                            {
                                                SourceFieldName = colByMeasure.Item2.ColumnName,
                                                Calculator = new FValueSumUnreplacedCalculator(format),
                                                StringFormat = stringFormat,
                                                UseMeasureModule = fieldInfo.UseMeasureModule,
                                            };

                                            fieldLayout.SummaryDefinitions.Add(unWritedOvs);
                                        }
                                    }
                                }

                                if (fielgGroupByReplacedTiDict.Count > 0)
                                {
                                    fieldGroupByObject.Children.AddRange(fielgGroupByReplacedTiDict.Values);
                                }
                            }
                        }
                    }
                }

                if (isDateTimeGroupingGridControlXam)
                {
                    //dtField.Settings.AllowSummaries = false;
                    dtField.Settings.AllowGroupBy = true;


                    fieldLayout.SortedFields.Add(new FieldSortDescription("EventDateTime", ListSortDirection.Ascending,
                        true));
                }
                else if (discreteType != enumTimeDiscreteType.DBMonth && discreteType != enumTimeDiscreteType.DB24Hour)
                {
                    dtField.DisplayTemplate = Application.Current.Resources[ConstantHelper.DateTimeTemplateName] as DataTemplate;
                }

                //var consumptionScheduleField = fields.FirstOrDefault(ff => ff.Name == "ConsumptionSchedule");
                //if (consumptionScheduleField != null)
                //{
                //    consumptionScheduleField.Label = "Тип. потр.";
                //}

                //var consumptionSchedulePercentField = fields.FirstOrDefault(ff => ff.Name == "ConsumptionSchedulePercent");
                //if (consumptionSchedulePercentField != null)
                //{
                //    consumptionSchedulePercentField.Label = "Тип. потр. %";
                //}
            }
            finally
            {
                // fieldLayout.SortedFields.EndUpdate();
                fields.EndUpdate();
            }

            //XamDataGridHelper.ConfigureGrid(dataGrid, true, false, true, true, true);
            dataGrid.FieldLayoutSettings.FixedFieldUIType = FixedFieldUIType.Splitter;

            dataGrid.FieldSettings.SummaryDisplayArea = SummaryDisplayAreas.BottomFixed;

            if (userTable.Columns.Count < 20)
            {
                //из-за того что тормоза ограничиваем
                //dataGrid.FieldSettings.SummaryDisplayArea = dataGrid.FieldSettings.SummaryDisplayArea | SummaryDisplayAreas.InGroupByRecords;
            }

            if (userTable.Columns.Count < 1000)
            {
                dataGrid.RecordsInViewChanged += ExpandFirstRecord;
            }
        }

        private static Field AddFieldAndSummary(DataColumn col, FieldLength? width, IFValueComparer comparer, bool useMeasureModule,
            bool isCumulateDrums, SummaryCalculator statType, string stringFormat,
            out SummaryDefinition summaryDefinition, Style fValueStyle)
        {
            var fld = AddFValueField(col.ColumnName, col.DataType, width, fValueStyle, _comparer);

            if (!isCumulateDrums)
            {
                summaryDefinition = new FValueSummaryDefinition
                {
                    SourceFieldName = fld.Name,
                    Calculator = statType,
                    StringFormat = stringFormat,
                    UseMeasureModule = useMeasureModule,
                };

                //fieldLayout.SummaryDefinitions.Add(sd);
            }
            else
            {
                summaryDefinition = null;
            }

            return fld;
        }

        private static IFValueComparer _comparer = new IFValueComparer();

        private static Field AddFValueField(string name, Type dataType, FieldLength? width, Style fValueStyle,
            IComparer comparer = null)
        {
            var fld = new Field
            {
                Name = name,
                DataType = dataType,
                Width = width,
                AllowFixing = AllowFieldFixing.No,
                Settings =
                {
                    SortComparer = comparer,
                    GroupByComparer = comparer,
                    AutoSizeOptions = FieldAutoSizeOptions.None,
                    Width = width,
                    SummaryUIType = SummaryUIType.MultiSelect,
                    AllowEdit = false,
                    EditorStyle = null,
                    FilterComparer = comparer,
                    FilterOperandUIType = FilterOperandUIType.Combo,
                    AllowRecordFiltering = true,
                    CellValuePresenterStyle = fValueStyle,
                },
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Center,
            };

            return fld;
        }

        public static void ExpandFirstRecord(object sender, EventArgs e)
        {
            var dp = sender as DataPresenterBase;
            if (dp == null) return;

            dp.RecordsInViewChanged -= ExpandFirstRecord;

            dp.Dispatcher.BeginInvoke((System.Action)(() =>
            {
                try
                {
                    var row = dp.Records.FirstOrDefault();
                    if (row != null)
                    {
                        row.IsExpanded = true;
                    }
                }
                catch { }
            }), DispatcherPriority.Send);
        }

        static void SetValidateColumnsForXamDataGrid(XamDataGrid dataGrid, DataTableEx userTable)
        {
            var hierObjectStyle = dataGrid.FindResource("HierObjectStyle") as Style;
            var hTemplate = dataGrid.FindResource("HierarchyObjectTemplate") as DataTemplate;
            var chTemplate = dataGrid.FindResource("ChannelTemplate") as DataTemplate;
            var fFlagStyle = dataGrid.FindResource("FFlagStyle") as Style;

            dataGrid.FieldSettings.LabelPresenterStyle = dataGrid.FindResource("ValidLabelStyle") as Style;
            // dataGrid.BeginInit();

            //var leftHorizontalStyle = dataGrid.FindResource("FastHorizontalLeftCellValuePresenterStyle") as Style;

            var fieldLayout = dataGrid.FieldLayouts[0];
            var fields = fieldLayout.Fields;

            //var field = fieldLayout.Fields["NameTI"] as TemplateField;
            try
            {
                fields.BeginUpdate();

                #region Вспомогательные поля

                var comparer = new IFreeHierarchyObjectComparer();

                var tiFld = new Field
                {
                    Name = "NameTI",
                    DataType = typeof(IFreeHierarchyObject),
                    Label = "Объект",
                    Width = new FieldLength(200),
                    FixedLocation = FixedFieldLocation.FixedToNearEdge,
                    //FixedLocation = FixedFieldLocation.FixedToNearEdge,
                    Settings =
                        {
                            AllowGroupBy = true,
                            GroupByComparer = comparer,
                            SortComparer = comparer,
                            FilterComparer = comparer,
                            CellVisibilityWhenGrouped = Visibility.Collapsed,
                            FilterOperandUIType = FilterOperandUIType.DropDownList,
                            AllowFixing = AllowFieldFixing.Near,
                            AllowEdit = false,
                            EditorStyle = null,
                            AllowRecordFiltering = true,
                            CellValuePresenterStyle = hierObjectStyle,
                        },
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Center,
                };

                //if (leftHorizontalStyle != null)
                //{
                //    //Это 
                //    tiFld.Settings.CellValuePresenterStyle = leftHorizontalStyle;
                //}

                fields.Add(tiFld);

                fields.Add(new TemplateField
                {
                    Name = "Channel",
                    Label = "К",
                    DataType = typeof(object),
                    DisplayTemplate = chTemplate,
                    Width = new FieldLength(30),
                    FixedLocation = FixedFieldLocation.FixedToNearEdge,
                    Settings = { FilterOperandUIType = FilterOperandUIType.None,
                        AllowFixing = AllowFieldFixing.Near,
                        AllowEdit = false,
                    EditorStyle = null,
                    },
                });

                //fieldLayout.Fields.Add(new TemplateField
                //{
                //    Name = "DataSource",
                //    Label = "Источник данных",
                //    Width = new FieldLength(130),
                //});

                var fld = new Field
                {
                    Name = "Parent",
                    DataType = typeof(IFreeHierarchyObject),
                    Label = "Родитель",
                    Width = new FieldLength(160),
                    AllowFixing = AllowFieldFixing.Near,
                    //FixedLocation = FixedFieldLocation.FixedToNearEdge,
                    Settings =
                        {
                            //GroupByMode = FieldGroupByMode.Value,
                            AllowGroupBy = true,
                            GroupByComparer = comparer,
                            SortComparer = comparer,
                            FilterComparer = comparer,
                            CellVisibilityWhenGrouped = Visibility.Collapsed,
                            FilterOperandUIType = FilterOperandUIType.DropDownList,
                            AllowFixing = AllowFieldFixing.Near,
                            AllowEdit = false,
                            EditorStyle = null,
                            AllowRecordFiltering = true,
                            CellValuePresenterStyle = hierObjectStyle,
                        },
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Center,
                };

                fields.Add(fld);
                var fsd = new FieldSortDescription("Parent", ListSortDirection.Ascending, true);
                fieldLayout.SortedFields.Add(fsd);

                #endregion

                // var flagStyle = XamDataGridHelper.DataTemplateSelectorToCellEditorStyle(flagTemplateSelector, null);

                var width = new FieldLength(32);
                var height = new FieldLength(20);

                foreach (DataColumn col in userTable.Columns)
                {
                    //var column = fieldLayout.Fields.FirstOrDefault(c => string.Equals(c.Name, "Valid" + i)) as TemplateField;
                    //if (column == null) return;
                    var isValid = col.ColumnName.IndexOf("Valid") >= 0;
                    if (isValid || col.ColumnName.IndexOf("TotalFlag") >= 0)
                    {
                        fields.Add(new Field
                        {
                            Name = col.ColumnName,
                            DataType = col.DataType,
                            ToolTip = col.Caption,
                            Label = col.Caption,
                            Visibility = isValid ? Visibility.Visible : Visibility.Collapsed,
                            Width = width,
                            Height = height,
                            AllowFixing = AllowFieldFixing.No,
                            AllowResize = false,
                            Settings =
                        {
                            SummaryUIType = SummaryUIType.MultiSelect,
                            FilterOperandUIType = FilterOperandUIType.Combo,
                            FilterOperatorDropDownItems = ComparisonOperatorFlags.None,
                            CellValuePresenterStyle = fFlagStyle,
                            AllowEdit = false,
                            AllowRecordFiltering = true,
                            //CellValuePresenterStyle = fFlagStyle,
                        },

                        });
                    }
                }
            }
            finally
            {
                fieldLayout.Fields.EndUpdate();
            }

            //XamDataGridHelper.ConfigureGrid(dataGrid, false, true, false, true, false);

            // раскрываем первую группу
            dataGrid.RecordsInViewChanged += ExpandFirstRecord;
        }

        public static bool RegisterSpecialFilterOperands()
        {
            //SpecialFilterOperands.Register(FValueSpecialFilterOperand.Even);
            //SpecialFilterOperands.Register(EvenOrOddOperand.Odd);

            return true;
        }

        public static void RemoveRecordsInViewChanged(DataPresenterBase dp)
        {
            dp.RecordsInViewChanged -= ExpandFirstRecord;
        }

        public static void GridControlXamOnGrouping(GroupingEventArgs e)
        {
            var fieldLayout = e.FieldLayout;
            var dtField = fieldLayout.Fields.FirstOrDefault(f => f.Name == "EventDateTime");
            if (dtField == null) return;

            if (e.Groups.Any(g => g.FieldName == "EventDateTime"))
            {
                var dataTemplateToCellValuePresenterStyleName = dtField.Tag as string;
                if (!string.IsNullOrEmpty(dataTemplateToCellValuePresenterStyleName))
                {
                    try
                    {
                        dtField.Settings.CellValuePresenterStyle =
                            XamDataGridHelper.DataTemplateToCellValuePresenterStyle(
                                dataTemplateToCellValuePresenterStyleName);
                    }
                    catch (Exception ex)
                    {
                        //Manager.UI.ShowMessage("Ошибка темплейта для группировки по времени");
                    }
                }
            }
            else
            {
                var dataTemplateToCellValuePresenterStyleName = dtField.Tag as string;
                if (!string.IsNullOrEmpty(dataTemplateToCellValuePresenterStyleName))
                {
                    try
                    {
                        dtField.Settings.CellValuePresenterStyle =
                            XamDataGridHelper.DataTemplateToCellValuePresenterStyle(
                                "full" + dataTemplateToCellValuePresenterStyleName);
                    }
                    catch (Exception ex)
                    {
                        //Manager.UI.ShowMessage("Ошибка темплейта для группировки по времени");
                    }
                }

                //dtField.Settings.CellValuePresenterStyle =
                //XamDataGridHelper.DataTemplateToCellValuePresenterStyle(ConstantHelper.DateTimeTemplateName);
            }
        }
    }
}
