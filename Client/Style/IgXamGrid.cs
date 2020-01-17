using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Infragistics.Documents.Excel;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.DataPresenter.ExcelExporter;
using Infragistics.Windows.Editors;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Both.VisualCompHelpers.TClasses;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Controls;
using Proryv.Servers.Forecast.Client_ServiceReference;
using Proryv.Servers.Forecast.Client_ServiceReference.Common;
using Proryv.Servers.Forecast.Client_ServiceReference.ForecastServiceReference;
using TI_ChanelType = Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.TI_ChanelType;
using TVALUES_DB = Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.TVALUES_DB;
using VALUES_FLAG_DB = Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.VALUES_FLAG_DB;
using Proryv.AskueARM2.Both.VisualCompHelpers.SpecialFilterOperands;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Color = System.Drawing.Color;
using enumTypeHierarchy = Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.enumTypeHierarchy;
using Proryv.AskueARM2.Client.ServiceReference.Service.Dictionary;
using Proryv.AskueARM2.Client.Styles.Exporter;
using Proryv.AskueARM2.Both.VisualCompHelpers.Interfaces;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using Proryv.AskueARM2.Client.Styles.Common;

namespace Proryv.AskueARM2.Client.Styles.Style
{
    public partial class IgXamGrid
    {
        internal void ButtonExportExcel_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var grid = VisualHelper.FindParent<DataPresenterBase>(button);
            if (grid == null) return;

            bool isExportCollapsedDetail;
            bool excludeGroupBySettings;

            _fileName = VisualHelper.prepareFile("xlsx", ExportHelper.BuildFileName(grid, out isExportCollapsedDetail, out excludeGroupBySettings), Manager.UI.ShowMessage);

            if (string.IsNullOrEmpty(_fileName)) return;


            try
            {
                var exportOptions = new ExportOptions
                {
                    ChildRecordCollectionSpacing = ChildRecordCollectionSpacing.None,  // ???
                    ExcludeExpandedState = false,                                           // 
                    ExcludeFieldLayoutSettings = true,                                     //
                    ExcludeFieldSettings = false,                                           // исключить настройки колонок
                    ExcludeGroupBySettings = excludeGroupBySettings,                        // исключить группировку
                    ExcludeRecordFilters = false,                                           // исключить фильтрацию
                    ExcludeRecordVisibility = false,                                        // исключить видимость строк
                    ExcludeSortOrder = false,                                               // исключить сортировку
                    ExcludeSummaries = true,                                               // исключить строки с суммами
                    ExcludeCrossFieldRecordFilters = true,
                    FileLimitBehaviour = FileLimitBehaviour.TruncateData,// при превышении лимита урезать данные

                };

                var exporter = new ProryvDataPresenterExcelExporter
                {
                    Measured = VisualHelper.FindParent<IMeasure>(grid)
                };

                if (exporter.Measured == null)
                {
                    var p = VisualHelper.FindParent<Popup>(grid);
                    if (p != null && p.PlacementTarget != null)
                    {
                        exporter.Measured = VisualHelper.FindParent<IMeasure>(p.PlacementTarget as FrameworkElement);
                    }
                }


                exporter.HeaderLabelExporting += OnLabelExporting;
                exporter.CellExported += OnCellExported;
                //exporter.CellExporting += Exporter_CellExporting;
                exporter.ExportEnded += OnExportEnden;

                //exporter.Export(grid, _fileName, WorkbookFormat.Excel2007, exportOptions);
                exporter.ExportAsync(grid, _fileName, WorkbookFormat.Excel2007, exportOptions);
            }
            catch (Exception ex)
            {
                Manager.UI.ShowMessage("Ошибка при экспорте в Excel! " + ex.Message);
            }
        }

        private void Exporter_CellExporting(object sender, CellExportingEventArgs e)
        {
            if (e.Field.Label == null || string.IsNullOrWhiteSpace(e.Field.Label.ToString()))
            {
                e. Cancel = true;
                e.Field.Visibility = Visibility.Collapsed;
                return;
            }
        }

        void OnCellExported(object sender, CellExportedEventArgs e)
        {
            var value = e.Value;

            if (value == null) return;

            var exporter = sender as ProryvDataPresenterExcelExporter;
            var measured = exporter != null ? exporter.Measured : null;

            if (value is string)
            {
                FormatExportValue(e, measured);
                return;
            }

            WorksheetRow row = null;
            try
            {
                row = e.CurrentWorksheet.Rows[e.CurrentRowIndex];
            }
            catch { }
            if (row == null) return;

            WorksheetCell cell = null;
            try
            {
                cell = row.Cells[e.CurrentColumnIndex];
            }
            catch{}
            if (cell == null) return;

            //var value = e.Value;

            System.Windows.Style cellStyle = e.Field.Settings.EditorStyle;
            if (cellStyle != null)
            {
                var cSetter = cellStyle.Setters.FirstOrDefault(s => s is Setter && (s as Setter).Property.Name.Equals(TextEditorBase.ValueToDisplayTextConverterProperty.Name)) as Setter;

                if (cSetter != null)
                {
                    var c = cSetter.Value as IValueConverter;
                    if (c != null)
                    {
                        cell.Value = c.Convert(value, null, null, null);
                        return;
                    }

                    //e.FormatSettings.FontColor = (fgSetter.Value as SolidColorBrush).Color;
                    //e.FormatSettings.FillPatternForegroundColor = (bgSetter.Value as SolidColorBrush).Color;
                }
            }

            //var measured = e.Record.DataPresenter.FindParent<IMeasure>();

            SetCellValue(value, e, cell, measured);
        }

        void FormatExportValue(CellExportedEventArgs e, IMeasure measured)
        {
            if (e.Record.Cells.Count <= e.Field.Index) return;

            var value = e.Record.Cells[e.Field.Index].Value;

            if (value is string)
            {
                FilteringString(e, value as string);
                return;
            }

            var cell = e.CurrentWorksheet.Rows[e.CurrentRowIndex].Cells[e.CurrentColumnIndex];

            SetCellValue(value, e, cell, measured);
        }

        private void SetCellValue(object value, CellExportedEventArgs e, WorksheetCell cell, IMeasure measured)
        {
            if (e.Field.Name == "Item.ManageRequestStatus" ||
                e.Field.Name == "Item.Request.ManageRequestStatus")
            {
                cell.Value = Expl_User_Journal_ManagePU_Request_List.StatusToString(Convert.ToInt16(value));
                return;
            }

            ExportHelper.SetCellValue(cell, value, measured, e.Field.Tag, e.Field.Name);
        }

        /// <summary>
        /// Отфильтровываем недо символы
        /// </summary>
        /// <param name="e"></param>
        /// <param name="value"></param>
        void FilteringString(CellExportedEventArgs e, string value)
        {
            if (e == null || e.Field == null) return;

            switch (e.Field.Name)
            {
                case "StringData":
                    value = FileAdapter.RemoveBadChar(value, 1024, " ");
                    break;
                default:
                    return;
            }


            var cell = e.CurrentWorksheet.Rows[e.CurrentRowIndex].Cells[e.CurrentColumnIndex];
            cell.Value = value;
        }

        void OnLabelExporting(object sender, HeaderLabelExportingEventArgs e)
        {
            //Proryv check is null
            if (e.Label == null)
            {
                if (e.FieldGroup != null)
                {
                    return;
                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }

            var lts = e.Label.ToString();

            var haveMeasure = e.Label as IHaveMeasureUnit;
            if (haveMeasure != null)
            {
                if (string.IsNullOrEmpty(haveMeasure.MeasureUnitUn))
                {
                    var exporter = sender as ProryvDataPresenterExcelExporter;
                    var measured = exporter != null ? exporter.Measured : null;

                    if (measured != null && measured.MeasureUnitSelectedInfo!=null)
                    {
                        var measureUnitUn = (haveMeasure.ChannelType % 10) > 2
                            ? measured.MeasureUnitSelectedInfo.ReactiveMeasureUnitUn
                            : measured.MeasureUnitSelectedInfo.ActiveMeasureUnitUn;

                        lts += ", " + EnumClientServiceDictionary.GetMeasureUnitAbbreviation(measureUnitUn);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(lts))
            {
                e.Cancel = true;
                return;
            }
            
            e.Label = lts;
        }

        private string _fileName;

        private void OnExportEnden(object sender, ExportEndedEventArgs e)
        {
            var exporter = sender as ProryvDataPresenterExcelExporter;
            if (exporter == null || string.IsNullOrEmpty(_fileName)) return;

            if (e.Canceled)
            {
                if (e.CancelInfo != null && e.CancelInfo.Exception != null)
                {
                    Manager.UI.ShowMessage(e.CancelInfo.Exception.Message);
                }

                return;
            }

            exporter.HeaderLabelExporting -= OnLabelExporting;
            exporter.CellExported -= OnCellExported;
            //exporter.CellExporting -= Exporter_CellExporting;
            exporter.ExportEnded -= OnExportEnden;
            exporter.Dispose();

            Manager.UI.ShowYesNoDialog("Открыть файл \"" + Path.GetFileName(_fileName) + "\" ?",
                delegate ()
                {
                    Proryv.AskueARM2.Client.Visual.Common.CommonEx.OpenSavedFile(_fileName);
                    _fileName = string.Empty;
                });
        }

        #region Работа с фильтром

        private readonly List<Type> _filterResolvedTypes = new List<Type>
        {
            typeof(IFValue), typeof(ulong), typeof(TVALUES_DB)
        };

        private void AddFilterDropDownItems(IList<FilterDropDownItem> filterList)
        {
         
            filterList.Add(CreateFilterDropDownItem(FValueSpecialFilterOperand.FlagValid, "flagGood.png"));
            filterList.Add(CreateFilterDropDownItem(FValueSpecialFilterOperand.FlagDataNotFull, "flagGray.png"));
            filterList.Add(CreateFilterDropDownItem(FValueSpecialFilterOperand.FlagNotCorrect, "flagBad.png"));
        }

        private FilterDropDownItem CreateFilterDropDownItem(SpecialFilterOperandBase filterOperandBase, string imageSource,
            ComparisonOperator comparisonOperator = ComparisonOperator.Equals)
        {
            return new FilterDropDownItem(new ComparisonCondition(comparisonOperator, filterOperandBase), filterOperandBase.Name)
            {
                Image = new BitmapImage(new Uri(GlobalEnumsDictionary.PrefixResource + "" + imageSource,
                UriKind.RelativeOrAbsolute)).GetAsFrozen() as BitmapImage
            };
        }

        #region Альтернатива, когда можно выбирать несколько вариантов одновременно (как в Excel)

        private void AddFilterMenuItems(IList<FieldMenuDataItem> filterList, object fi)
        {
            //чтобы этот режим заработал fld.Settings.FilterOperandUIType = FilterOperandUIType.ExcelStyle;
            var item = new FieldMenuDataItem
            {
                Header = "Достоверные",
                IsCheckable = true,
                Command = new ConditionFilterCommand
                {
                    ShowCustomFilterDialog = false,
                    Condition = new ComparisonCondition(Infragistics.Windows.Controls.ComparisonOperator.Match, FValueSpecialFilterOperand.FlagValid)
                },
                CommandParameter = fi,
                ImageSource = new BitmapImage(new Uri(GlobalEnumsDictionary.PrefixResource + "flagGood.png", UriKind.RelativeOrAbsolute)).GetAsFrozen() as BitmapImage,
                
            };

            filterList.Add(item);
            
            item = new FieldMenuDataItem
            {
                Header = "Неполные",
                IsCheckable = true,
                Command = new ConditionFilterCommand
                {
                    ShowCustomFilterDialog = false,
                    Condition = new ComparisonCondition(Infragistics.Windows.Controls.ComparisonOperator.Match, FValueSpecialFilterOperand.FlagDataNotFull)
                },
                CommandParameter = fi,
                ImageSource = new BitmapImage(new Uri(GlobalEnumsDictionary.PrefixResource + "flagGray.png", UriKind.RelativeOrAbsolute)).GetAsFrozen() as BitmapImage,

            };

            filterList.Add(item);

        }

        #endregion

        #endregion

        private void EventSetterOnHandler(object sender, RecordFilterChangedEventArgs e)
        {
            //работа фильтра
            if (e.RecordFilter.Field.DataType != typeof(IFValue)) return;

            /*
            var grid = sender as DataPresenterBase;
            if (grid == null) return;

            var filter = e.RecordFilter;
            var outRecords = grid.RecordManager.GetFilteredOutDataRecords();

            RecordFilter otherSideFilter = new RecordFilter() { FieldName = filter.FieldName };
            foreach (ComparisonCondition cc in filter.Conditions)
            {
                otherSideFilter.Conditions.Add(cc);
            }

            grid.DefaultFieldLayout.RecordFilters.Add(otherSideFilter);

            var filterIns = new List<IFValue>();
            var filterOuts = new List<IFValue>();

            foreach (DataRecord record in outRecords)
            {
                var sd = record.DataItem as IFValue;
                filterOuts.Add(sd);
            }

            foreach (IFValue sd in filterOuts)
            {
                //vm.FilterOutData.Add(sd);
                //vm.FilterInData.Remove(sd);
            }

            var inRecords = grid.RecordManager.GetFilteredInDataRecords();

            foreach (DataRecord record in inRecords)
            {
                var sd = record.DataItem as IFValue;
                filterIns.Add(sd);
            }

            foreach (var sd in filterIns)
            {
                //vm.FilterOutData.Remove(sd);
                //vm.FilterInData.Add(sd);
            }

            //grid.DefaultFieldLayout.RecordFilters.Clear();
            */

            e.Handled = true;
        }

        private void ExportOnLoaded(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;

            var obj = VisualHelper.FindParent<DataPresenterBase>(fe);
            if (obj != null)
            {
                var p = obj.GetValue(VisualHelper.IsExportToExcelEnabledProperty);
                if (p != null)
                {
                    if ((bool)p)
                    {
                        fe.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        fe.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    fe.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
