using Infragistics.Windows.DataPresenter;
using Proryv.AskueARM2.Both.VisualCompHelpers.TClasses;
using Proryv.AskueARM2.Both.VisualCompHelpers.XamDataPresenter.DataTableExtention;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.ServiceReference.Service.Extensions;
using Proryv.AskueARM2.Server.DBAccess.Public.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;

namespace Proryv.AskueARM2.Both.VisualCompHelpers.XamDataPresenter
{
    public static partial class ArchivesToXamDataGrid
    {
        /// <summary>
        /// Строим суточные интегралы
        /// </summary>
        /// <param name="archiveIntegrals">Интегралы</param>
        /// <param name="getNameInterface"></param>
        /// <param name="dts">Даты/время</param>
        /// <returns></returns>
        public static DataTableEx ExecuteDailyIntegrals(ArchTariffIntegralsTIs archiveIntegrals, enumTimeDiscreteType discreteType)
        {
            if (archiveIntegrals == null || archiveIntegrals.IntegralsValue30orHour == null) return null;

            if (discreteType < enumTimeDiscreteType.DB24Hour)
            {
                //Допускается только 3 периода дискретизации
                discreteType = enumTimeDiscreteType.DB24Hour; 
            }

            var unitDigitCoeff = (double)archiveIntegrals.UnitDigit;
            var dtStart = archiveIntegrals.DTStart;
            var dtEnd = archiveIntegrals.DTEnd; //Округляем до суток

            var dts = Extention.GetDateTimeListForPeriod(dtStart, dtEnd,
                discreteType, archiveIntegrals.TimeZoneId.GeTimeZoneInfoById());

#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif
            var userTable = new DataTableEx()
            {
                FieldForSearch = "NameTI",
            };

            userTable.BeginLoadData();
            try
            {
                #region Создаем основные колонки

                userTable.Columns.Add("Parent", typeof(IFreeHierarchyObject));
                userTable.Columns.Add("NameTI", typeof(IFreeHierarchyObject));
                userTable.Columns.Add("Coeff", typeof(double));
                userTable.Columns.Add("CoeffLosses", typeof(double));
                userTable.Columns.Add("Channel", typeof(object));
                userTable.Columns.Add("DataSource", typeof(string));

                #endregion

                #region Посуточные колонки

                var sufixFormat = "dd.MM.yyyy";
                var prefixs = new List<string>
                {
                    "_OnStart",
                    "_OnEnd", 
                    "_Delta", 
                }; //Строим 3 колонки по каждой дате

                foreach(var d in dts)
                {
                    var startCaption = d.ToString(sufixFormat);
                    var endCaption = "";
                    string groupLabel;
                    switch(discreteType)
                    {
                        case enumTimeDiscreteType.DB24Hour:
                            groupLabel = "За " + startCaption;
                            endCaption = d.AddDays(1).ToString(sufixFormat);
                            break;
                        case enumTimeDiscreteType.DBMonth:
                            groupLabel = "За " + d.ToString("MMMM yyyy");
                            endCaption = d.AddMonths(1).ToString(sufixFormat);
                            break;
                        default:
                            groupLabel = "За период c " + dtStart.ToString("dd.MM.yyyy HH:mm") + " по " + dtEnd.ToString("dd.MM.yyyy HH:mm");
                            endCaption = dtEnd.Date.AddDays(1).ToString(sufixFormat);
                            break;
                    }

                    foreach (var prefix in prefixs)
                    {
                        var col = new DataColumnEx(prefix + startCaption, typeof(IConvertible));
                        switch(prefix)
                        {
                            case "_OnStart":
                                col.Caption = startCaption;
                                break;
                            case "_OnEnd":
                                col.Caption = endCaption;
                                break;
                            default:
                                col.Caption = "Расход";
                                col.IsFValue = true;
                                break;
                        }
                        
                        col.GroupName = groupLabel;


                        userTable.Columns.Add(col);
                    }
                }

                #endregion

                #region Наполняем содержимым

                var rd = TimeSpan.FromDays(1);

                foreach (var tiVal in archiveIntegrals.IntegralsValue30orHour)
                {
                    if (tiVal.Val_List == null) continue;

                    var row = userTable.NewRow() as DataRowEx;

                    

                    TPSHierarchy ps;
                    if (EnumClientServiceDictionary.DetailPSList.TryGetValue(tiVal.PS_ID, out ps))
                    {
                        row["Parent"] = ps;
                    }

                    TInfo_TI ti;
                    if (EnumClientServiceDictionary.TIHierarchyList.TryGetValue(tiVal.TI_Ch_ID.TI_ID, out ti))
                    {
                        row["NameTI"] = ti;
                    }

                    row["Coeff"] = tiVal.CoeffTransformation;

                    row["Channel"] = new TTariffPeriodID
                    {
                        ChannelType = tiVal.TI_Ch_ID.ChannelType,
                        IsOV = false,
                        TI_ID = tiVal.TI_Ch_ID.TI_ID,
                        StartDateTime = dtStart,
                        FinishDateTime = dtEnd,
                    };

                    row["DataSource"] = EnumClientServiceDictionary
                                .DataSourceTypeList
                                .FirstOrDefault(v =>
                                    EqualityComparer<EnumDataSourceType?>.Default.Equals(tiVal.TI_Ch_ID.DataSourceType,
                                        v.Key))
                                .Value;

                    DateTime? startRangeDt = null, endRangeDt = null; //Обрабатываемый диапазон
                    var rangeDiff = 0.0; //накопленный расход за диапазон
                    var ranfeFlag = VALUES_FLAG_DB.None; //Состояние интегралов за диапазон
                    var isFirst = true;

                    //Перебираем интегралы
                    foreach (var iVal in tiVal.Val_List.OrderBy(d=>d.EventDateTime))
                    {
                        if (isFirst)
                        {
                            row["CoeffLosses"] = iVal.CoeffLosses ?? 1;

                            isFirst = false;
                        }

                        var roundedDt = iVal.EventDateTime.Round(rd);

                        //Это новая запись или переход на следующую дату/время
                        if (!startRangeDt.HasValue || roundedDt >= endRangeDt)
                        {
                            if (iVal.IsIntegralType)
                            {
                                //Эта дата осталась от предыдущего диапазона, надо закрыть диапазона
                                if (startRangeDt.HasValue)
                                {
                                    row["_OnEnd" + startRangeDt.Value.ToString(sufixFormat)] = iVal.F_VALUE / unitDigitCoeff;
                                    row["_Delta" + startRangeDt.Value.ToString(sufixFormat)] = new TVALUES_DB
                                    {
                                        F_VALUE = rangeDiff,
                                        F_FLAG = ranfeFlag,
                                    };
                                }

                                if (roundedDt > dtEnd) break;

                                //Теперь новый диапазон

                                switch (discreteType)
                                {
                                    case enumTimeDiscreteType.DB24Hour:
                                        //Первая дата/время нового диапазона
                                        startRangeDt = new DateTime(roundedDt.Year, roundedDt.Month, roundedDt.Day);
                                        //Последняя дата/время нового диапазона
                                        endRangeDt = startRangeDt.Value.AddDays(1);
                                        break;
                                    case enumTimeDiscreteType.DBMonth:
                                        //Первая дата/время нового диапазона
                                        startRangeDt = new DateTime(roundedDt.Year, roundedDt.Month, 1);
                                        //Последняя дата/время нового диапазона
                                        endRangeDt = startRangeDt.Value.AddMonths(1);
                                        break;
                                    default:
                                        //Первая дата/время нового диапазона
                                        startRangeDt = dtStart.Date;
                                        //Последняя дата/время нового диапазона
                                        endRangeDt = dtEnd.Date.AddDays(1);
                                        break;
                                }

                                row["_OnStart" + startRangeDt.Value.ToString(sufixFormat)] = iVal.F_VALUE / unitDigitCoeff;

                                rangeDiff = 0;
                                ranfeFlag = VALUES_FLAG_DB.None;
                            }
                        }

                        rangeDiff += iVal.F_VALUE_DIFF / unitDigitCoeff;
                        ranfeFlag = ranfeFlag.CompareAndReturnMostBadStatus(iVal.F_FLAG);
                    }

                    userTable.Rows.Add(row);

                }

                #endregion
            }
            finally
            {
                userTable.EndLoadData();
                userTable.AcceptChanges();
            }
#if DEBUG
            sw.Stop();
            Console.WriteLine("ExecuteDailyIntegralsExpand - > {0} млс", sw.ElapsedMilliseconds);
#endif
            return userTable;
        }

        public static void ConfigureGridDailyIntegrals(XamDataGrid dataGrid, DataTableEx userTable, bool useLossesCoefficient)
        {
            if (dataGrid == null || userTable == null) return;

            try
            {
                var labelStyle = dataGrid.FindResource("LabelObjectStyle") as Style;
                dataGrid.FieldSettings.LabelPresenterStyle = labelStyle;
            }
            catch
            {
            }

            var view = userTable.DefaultView;

#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            try
            {
                var hierObjectStyle = dataGrid.FindResource("HierObjectStyle") as Style;
                var chTemplate = dataGrid.FindResource("ChannelTemplate") as DataTemplate;
                var fValueStyle = dataGrid.FindResource("FValueStyle") as Style;
                var fValueNoFlagStyle = dataGrid.FindResource("FValueNoFlagStyle") as Style;
                var widthValue = new FieldLength(105);

                dataGrid.BeginInit();
                var fieldLayout = dataGrid.FieldLayouts[0];
                var fields = fieldLayout.Fields;

                try
                {
                    fields.BeginUpdate();

                    #region Создаем основные колонки

                    var comparer = new IFreeHierarchyObjectComparer();

                    fields.Add(new Field
                    {
                        Name = "Parent",
                        DataType = typeof(IFreeHierarchyObject),
                        Label = "Объект",
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
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        VerticalContentAlignment = VerticalAlignment.Center,
                    });

                    fields.Add(new Field
                    {
                        Name = "NameTI",
                        DataType = typeof(IFreeHierarchyObject),
                        Label = "ТИ",
                        Width = new FieldLength(200),
                        AllowFixing = AllowFieldFixing.Near,
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
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        VerticalContentAlignment = VerticalAlignment.Center,
                    });

                    fields.Add(new TemplateField
                    {
                        Name = "Channel",
                        Label = "К",
                        DataType = typeof(object),
                        DisplayTemplate = chTemplate,
                        Width = new FieldLength(40),
                        AllowFixing = AllowFieldFixing.Near,
                        //FixedLocation = FixedFieldLocation.FixedToNearEdge,
                        Settings =
                        {
                            AllowGroupBy = true,
                            FilterOperandUIType = FilterOperandUIType.DropDownList,
                            AllowFixing = AllowFieldFixing.Near,
                            AllowEdit = false,
                            EditorStyle = null,
                            AllowRecordFiltering = true,
                        },
                    });

                    fieldLayout.Fields.Add(new Field
                    {
                        Name = "DataSource",
                        Label = "Источник данных",
                        Width = new FieldLength(100),
                        AllowFixing = AllowFieldFixing.Near,
                        Settings =
                        {
                            AllowGroupBy = true,
                            FilterOperandUIType = FilterOperandUIType.DropDownList,
                            AllowFixing = AllowFieldFixing.Near,
                            AllowEdit = false,
                            EditorStyle = null,
                            AllowRecordFiltering = true,
                        },
                    });

                    fields.Add(new Field
                    {
                        Name = "Coeff",
                        DataType = typeof(double),
                        Width = widthValue,
                        Label= "Коэфф. тр.",
                        Format= "#0",
                        AllowFixing = AllowFieldFixing.Near,
                        Settings =
                        {
                            AllowGroupBy = true,
                            AutoSizeOptions = FieldAutoSizeOptions.None,
                            Width = widthValue,
                            SummaryUIType = SummaryUIType.MultiSelect,
                            AllowEdit = false,
                            EditorStyle = null,
                            FilterOperandUIType = FilterOperandUIType.Combo,
                            AllowRecordFiltering = true,
                        },
                        HorizontalContentAlignment = HorizontalAlignment.Right,
                        VerticalContentAlignment = VerticalAlignment.Center,
                    });

                    if (useLossesCoefficient)
                    {
                        fields.Add(new Field
                        {
                            Name = "CoeffLosses",
                            DataType = typeof(double),
                            Width = widthValue,
                            Label = "Коэфф. потерь",
                            Format = "#0.###",
                            AllowFixing = AllowFieldFixing.Near,
                            Settings =
                        {
                            AllowGroupBy = true,
                            AutoSizeOptions = FieldAutoSizeOptions.None,
                            Width = widthValue,
                            SummaryUIType = SummaryUIType.MultiSelect,
                            AllowEdit = false,
                            EditorStyle = null,
                            FilterOperandUIType = FilterOperandUIType.Combo,
                            AllowRecordFiltering = true,
                        },
                            HorizontalContentAlignment = HorizontalAlignment.Right,
                            VerticalContentAlignment = VerticalAlignment.Center,
                        });
                    }

                    #endregion

                    #region Посуточные колонки
                    var counter = 0;
                    FieldGroup fieldGroup = null;

                    foreach (var col in userTable.Columns.OfType<DataColumnEx>())
                    {
                        if (col == null) continue;

                        if (counter == 0)
                        {
                            fieldGroup = new FieldGroup
                            {
                                Label = col.GroupName,
                            };

                            fieldLayout.FieldItems.Add(fieldGroup);
                        }

                        var fld = AddFValueField(col.ColumnName, col.DataType, widthValue, col.IsFValue ? fValueStyle : fValueNoFlagStyle, null);
                        fld.Label = col.Caption;

                        //if (!col.IsFValue)
                        //{
                        //    fld.Format = "##0.######";
                        //}

                        fieldGroup.Children.Add(fld);

                        if (++counter == 3) counter = 0;
                    }

                    #endregion

                }
                finally
                {
                    fieldLayout.Fields.EndUpdate();
                }

                dataGrid.RecordsInViewChanged += ExpandFirstRecord;
            }
            finally
            {
                dataGrid.DataSource = view;
                dataGrid.EndInit();
            }

#if DEBUG
            sw.Stop();
            Console.WriteLine("ConfigureGridDailyIntegrals - > {0} млс", sw.ElapsedMilliseconds);
#endif
        }
    }
}
