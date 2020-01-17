using Infragistics;
using Infragistics.Collections;
using Infragistics.Controls.Grids;
using Infragistics.Documents.Excel;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using Proryv.AskueARM2.Both.VisualCompHelpers.Interfaces;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Styles.Common;
using Proryv.AskueARM2.Client.Styles.Data;
using Proryv.AskueARM2.Client.Visual.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Proryv.AskueARM2.Client.Styles.Style
{
    public partial class IgGrid
    {
        public void ButtonExportExcel_Click(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var xamGrid = VisualHelper.FindParent<XamGrid>(fe);
            if (xamGrid == null || xamGrid.ItemsSource == null) return;

            bool isExportCollapsedDetail;
            bool excludeGroupBySettings;

            var fileName = ExportHelper.BuildFileName(xamGrid, out isExportCollapsedDetail, out excludeGroupBySettings);
            if (string.IsNullOrEmpty(fileName)) return;

            
            //progress.Value = 0;
            //progress.Maximum = requesterHh.PartitionsCount + requsterIntegral.PartitionsCount;

            //if (_worker != null)
            //{
            //    _worker.DoWork -= worker_DoWork;
            //    _worker.RunWorkerCompleted -= worker_RunWorkerCompleted;
            //}

            var worker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };

            fe.Tag = worker;

            var dataView = xamGrid.ItemsSource as DataView;
            if (dataView == null)
            {
                Manager.UI.ShowMessage("Пока работаем только с DataView");
                return;
            }

            var progressBar = SetProgressIndicator(fe, fileName, dataView.Count);

            var columnInfos = new List<XamGridColumnInfo>();
            int currentColumn = 0;
            var measure = VisualHelper.FindParent<IMeasure>(xamGrid);
            CollectColumnInfos(xamGrid.Columns, ref currentColumn, columnInfos, measure);

            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync(new XamExporterResult
            {
                ClickedButton = fe,
                FileName = fileName,
                SheetName = xamGrid.Name,
                ColumnInfos = columnInfos,
                DataView = dataView,
                ProgressBar = progressBar,
                Measure = measure,
            });

            //if (exporter.Measured == null)
            //{
            //    var p = VisualHelper.FindParent<Popup>(grid);
            //    if (p != null && p.PlacementTarget != null)
            //    {
            //        exporter.Measured = VisualHelper.FindParent<IMeasure>(p.PlacementTarget as FrameworkElement);
            //    }
            //}

            fe.IsEnabled = false;
        }

        /// <summary>
        /// Основная процедура экспорта
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            if (worker == null) return;

            var args = e.Argument as XamExporterResult;
            if (args == null) return;

            var sheetName = args.SheetName;
            var columnInfos = args.ColumnInfos;
            var dataView = args.DataView;
            var progressBar = args.ProgressBar;
            var measure = args.Measure;

            args.Workbook = new Workbook();
            args.Workbook.SetCurrentFormat(WorkbookFormat.Excel2007);
            e.Result = args;

            var sheetOne = args.Workbook.Worksheets.Add(sheetName);

            args.DataView = null;
            args.ColumnInfos = null;
            args.SheetName = null;
            args.ProgressBar = null;
            args.Measure = null;

            sheetOne.DisplayOptions.PanesAreFrozen = true;
            sheetOne.DisplayOptions.FrozenPaneSettings.FrozenRows = 1;
            sheetOne.DefaultColumnWidth = 5000;

            SetColumnHeader(sheetOne, columnInfos);

            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            int currentRow = 1;
            int progressPack = 0;
            double progressIndicator = 0;

            foreach (DataRowView rowView in dataView)
            {
                var dataRow = rowView.Row;
                var worksheetRow = sheetOne.Rows[currentRow];

                SetColumnValues(dataRow, worksheetRow, columnInfos, measure);

                currentRow++;
                progressPack++;

                //Обновляем прогресс и проверяем отмену каждые 100 записей
                if (progressPack == 100)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    progressIndicator++;
                    IncProgress(progressIndicator, progressBar);
                    progressPack = 0;
                }
            }
        }

        /// <summary>
        /// Экспорт закончен
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error != null)
                {
                    Manager.UI.ShowMessage(e.Error.Message);
                }
                else
                {
                    try
                    {
                        var exporterResult = e.Result as XamExporterResult;
                        if (exporterResult != null && exporterResult.Workbook != null)
                        {

                            using (var exportStream = new MemoryStream())
                            {
                                exporterResult.Workbook.Save(exportStream);
                                exportStream.Position = 0;

                                FileAdapter.SaveFile(exportStream, exporterResult.FileName, "xlsx", Manager.UI.ShowMessage,
                                    (message, fn) => Manager.UI.ShowYesNoDialog("Открыть файл \"" + Path.GetFileName(exporterResult.FileName) + "\" ?", () =>
                                    {
                                        Proryv.AskueARM2.Client.Visual.Common.CommonEx.OpenSavedFile(fn);
                                        exporterResult.FileName = string.Empty;
                                    }));
                            }

                            ClearProgressIndicator(exporterResult.ClickedButton);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);
                    }
                }
            }

            var worker = sender as BackgroundWorker;
            if (worker != null)
            {
                try
                {
                    worker.DoWork -= worker_DoWork;
                    worker.RunWorkerCompleted -= worker_RunWorkerCompleted;
                }
                catch
                {

                }
            }

            //fe.IsEnabled = true;

            GC.Collect(0);
            //GC.WaitForPendingFinalizers();

            //progress.Abort();
            //btnValidate.IsEnabled = progress.IsEnabled = true;
        }

        /// <summary>
        /// Отмена экспорта
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void progress_Cancel(object sender, EventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe == null) return;

            //Пытаемся найти кнопку с экспортом, в ней в Tag помещен BackgroundWorker
            var grid = VisualHelper.FindParent<Grid>(fe);
            if (grid == null) return;

            var expButton = grid.FindName("Export") as Button;
            if (expButton == null) return;

            var worker = expButton.Tag as BackgroundWorker;
            if (worker != null)
            {
                //Отменяем экспорт
                try
                {
                    ClearProgressIndicator(expButton);
                    worker.CancelAsync();
                }
                catch
                {

                }
            }

            Manager.UI.ShowMessage("Экспорт отменен!");
        }

        /// <summary>
        /// Когда кнопка экспорта загружена
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportOnLoaded(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;

            var obj = VisualHelper.FindParent<XamGrid>(fe);
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

        /// <summary>
        /// Информация о колонках
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columns"></param>
        /// <param name="currentColumn"></param>
        /// <param name="columnInfos"></param>
        private static void CollectColumnInfos<T>(ICollectionBase<T> columns, ref int currentColumn, 
            List<XamGridColumnInfo> columnInfos, IMeasure measure)
        {
            if (columns == null || !columns.Any() || columnInfos == null) return;

            // Заголовок
            foreach (var column in columns)
            {
                var cc = column as GroupColumn;
                if (cc != null)
                {
                    CollectColumnInfos(cc.Columns, ref currentColumn, columnInfos, measure);
                    continue;
                }

                var c = column as Column;
                if (c == null) continue;

                columnInfos.Add(new XamGridColumnInfo
                {
                    Name = GetHeaderfromColumn(c, measure),
                    Key = c.Key,
                    Width = c.Width,
                    ColumnNumber = currentColumn,
                });

                currentColumn++;
            }
        }

        /// <summary>
        /// Содержимое ячейки
        /// </summary>
        /// <param name="row"></param>
        /// <param name="worksheetRow"></param>
        /// <param name="columnInfos"></param>
        private static void SetColumnValues(DataRow row, WorksheetRow worksheetRow, List<XamGridColumnInfo> columnInfos, IMeasure measuer)
        {
            if (columnInfos == null || !columnInfos.Any()) return;

            // Заголовок
            foreach (var column in columnInfos)
            {
                try
                {
                    var value = row[column.Key];

                    ExportHelper.SetCellValue(worksheetRow.Cells[column.ColumnNumber], value, measuer);
                }
                catch { }
            }
        }

        /// <summary>
        /// Название колонки
        /// </summary>
        /// <param name="sheetOne"></param>
        /// <param name="columnInfos"></param>
        private static void SetColumnHeader(Worksheet sheetOne, List<XamGridColumnInfo> columnInfos)
        {
            if (columnInfos == null || !columnInfos.Any()) return;

            var row = sheetOne.Rows[0];

            // Заголовок
            foreach (var column in columnInfos)
            {
                row.Cells[column.ColumnNumber].Value = column.Name;
                if (column.Width.HasValue)
                {
                    sheetOne.Columns[column.ColumnNumber].Width = (int)column.Width.Value.Value * 43;
                }
            }
        }

        private static string GetHeaderfromColumn(ColumnBase cc, IMeasure measure)
        {
            string header;
            
            if (cc.Label != null)
            {
                header = cc.Label.ToString();

                var haveMeasure = cc.Label as IHaveMeasureUnit;
                if (haveMeasure != null)
                {
                    if (string.IsNullOrEmpty(haveMeasure.MeasureUnitUn))
                    {
                        if (measure != null && measure.MeasureUnitSelectedInfo != null)
                        {
                            var measureUnitUn = (haveMeasure.ChannelType % 10) > 2
                                ? measure.MeasureUnitSelectedInfo.ReactiveMeasureUnitUn
                                : measure.MeasureUnitSelectedInfo.ActiveMeasureUnitUn;

                            header += ", " + EnumClientServiceDictionary.GetMeasureUnitAbbreviation(measureUnitUn);
                        }
                    }
                }
            }
            else if (!string.IsNullOrEmpty(cc.HeaderText))
            {
                header = cc.HeaderText;
            }
            else
            {
                header = cc.Key;
            }

            return header;
        }

        private static ProgressBar SetProgressIndicator(FrameworkElement clickedButton, string indicatorText, int maxValue)
        {
            //Ищем прогресс бар
            var grid = VisualHelper.FindParent<Grid>(clickedButton);
            if (grid != null)
            {
                var spProgressLayout = grid.FindName("spProgressLayout") as FrameworkElement;
                if (spProgressLayout != null)
                {
                    spProgressLayout.Visibility = Visibility.Visible;
                    var progressBar = spProgressLayout.FindName("pbProgress") as ProgressBar;
                    if (progressBar!=null)
                    {
                        progressBar.Maximum = maxValue / 100;
                    }

                    var tbProgress = spProgressLayout.FindName("tbProgress") as TextBlock;
                    if (tbProgress != null)
                    {
                        tbProgress.Text = indicatorText;
                    }

                    return progressBar;
                }
            }

            return null;
        }

        private static void ClearProgressIndicator(FrameworkElement clickedButton)
        {
            if (clickedButton == null) return;

            //Ищем прогресс бар
            var grid = VisualHelper.FindParent<Grid>(clickedButton);
            if (grid != null)
            {
                var spProgressLayout = grid.FindName("spProgressLayout") as FrameworkElement;
                if (spProgressLayout != null)
                {
                    var progressBar = spProgressLayout.FindName("pbProgress") as ProgressBar;
                    if (progressBar != null)
                    {
                        progressBar.Value = 0;
                        progressBar.Maximum = 0;
                    }

                    var tbProgress = spProgressLayout.FindName("tbProgress") as TextBlock;
                    if (tbProgress != null)
                    {
                        tbProgress.Text = string.Empty;
                    }

                    spProgressLayout.Visibility = Visibility.Collapsed;
                }
            }

            clickedButton.IsEnabled = true;
        }

        private static void IncProgress(double value, ProgressBar progressBar)
        {
            if (progressBar == null) return;

            progressBar.Dispatcher.BeginInvoke((System.Action)(() =>
            {
                progressBar.Value = value;
            }));
        }
    }
}
