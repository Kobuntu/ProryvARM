using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.ServiceReference.StimulReportService;
using Proryv.AskueARM2.Client.Visual.Common;
using Stimulsoft.Report;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report.WpfDesign;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using WCFHelper;
using Proryv.AskueARM2.Client.Visual.Common.Common;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using Proryv.ElectroARM.Controls.Common;
using ProryvAdapterDataReport;

namespace Proryv.AskueARM2.Client.Visual.StimulReporter
{
    /// <summary>
    /// Interaction logic for StimulReporterDesigner_Frame_.xaml
    /// </summary>
    public partial class StimulReporDesigner_Frame : IModule, IDisposable, IModalCompleted, INotifyPropertyChanged
    {
        private enumDateRangeMode _dateRangeMode;
        public enumDateRangeMode DateRangeMode
        {
            set
            {
                _dateRangeMode = value;
                _PropertyChanged("DateRangeMode");
            }

            get
            {
                return _dateRangeMode;
            }
        }

        private enumTreeMode _treeMode;
        public enumTreeMode TreeMode
        {
            set
            {
                _treeMode = value;
                _PropertyChanged("TreeMode");
            }

            get
            {
                return _treeMode;
            }
        }

        public FreeHierarchyTreeItem GetNode(IFreeHierarchyObject hierarchyObject)
        {
            return null;
        }

        private string _reportUn;

        public StimulReporDesigner_Frame()
        {

        }

        public StimulReporDesigner_Frame(string reportUn)
        {
            #region Настраиваем дизайнер

            StimulReportFactory.PrepareDesigner();
            StiHelper.HyperLinkAttach();
            //StiHelper.StiWcfConfigure();

            InitializeComponent();

            DateRangeMode = enumDateRangeMode.WithoutDateMode;
            TreeMode = enumTreeMode.None;

            // Designer
            StiOptions.WCFService.WCFRenderReport += WCFRenderReport;
            StiOptions.WCFService.WCFTestConnection += WCFTestConnection;
            StiOptions.WCFService.WCFBuildObjects += WCFBuildObjects;
            //StiOptions.WCFService.WCFRetrieveColumns += WCFRetrieveColumns;
            StiOptions.WCFService.WCFOpeningReportInDesigner += WCFOpeningReportInDesigner;
            //Designer.
            StiOptions.WCFService.WCFRequestFromUserRenderReport += WCFRequestFromUserRenderReport;
            StiOptions.Engine.GlobalEvents.SavingReportInDesigner += GlobalEvents_SavingReportInDesigner;
            //StiOptions.Viewer.Elements.ShowReportSaveToServerButton = true;
            //Stimulsoft.Report.StiOptions.Viewer.Elements.ShowReportSaveToServerButton = true;

            // Interactions
            StiOptions.WCFService.WCFRenderingInteractions += WCFRenderingInteractions;

            // Prepare RequestFromUser Variables
            StiOptions.WCFService.WCFPrepareRequestFromUserVariables += WCFPrepareRequestFromUserVariables;
            StiOptions.WCFService.WCFInteractiveDataBandSelection += WCFInteractiveDataBandSelection;

            #endregion

            _reportUn = reportUn;

            //Загрузка отчета из БД
            LayoutGrid.RunBackgroundAsync<TReportInfo>("LoadReport", response =>
            {
                if (response == null || response.Data == null) return;

                var report = new StiReport
                {
                    CacheAllData = true,
                    ReportCacheMode = StiReportCacheMode.On,
                };
                try
                {
                    report.Load(response.Data.DecodeBuffer());
                }
                catch (Exception ex)
                {
                    Manager.UI.ShowMessage(ex.Message);
                }

                _reportUn = response.ReportUn;
                _reportName = tbReportName.Text = response.StringName;

                StimulReportFactory.PrepareReport(report, !string.IsNullOrEmpty(reportUn));

                ProryvReportFunctions.RegisterFunctionsInReport();

                DesignerControl.Report = report;

                DateRangeMode = (enumDateRangeMode) response.DateRangeMode;
                TreeMode = (enumTreeMode)response.TreeMode;
                CheckBoxChannels.IsChecked = response.IsShowChannelSelector;
            }, EnumServiceType.StimulReport, reportUn);
        }

        #region События WCF отчета

        private void WCFInteractiveDataBandSelection(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            Manager.UI.ShowMessage("WCFInteractiveDataBandSelection пока не поддерживается");
        }

        private void WCFPrepareRequestFromUserVariables(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            Manager.UI.ShowMessage("WCFPrepareRequestFromUserVariables пока не поддерживается");
        }

        private void WCFRenderingInteractions(object viewer, Stimulsoft.Report.Events.StiWCFRenderingInteractionsEventArgs e)
        {
            //Manager.UI.ShowMessage("WCFRenderingInteractions пока не поддерживается");
        }

        private void GlobalEvents_SavingReportInDesigner(object sender, Stimulsoft.Report.Design.StiSavingObjectEventArgs e)
        {
            if (!Equals(sender, DesignerControl)) return;

            if (e.SaveAs)
            {
                e.Processed = false;
                var dc = DesignerControl;
                if (dc == null) return;
                dc.ReportFileName = FileAdapter.removeBadChar(tbReportName.Text);
            }
        }

        private void WCFRequestFromUserRenderReport(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            if (!Equals(sender, DesignerControl)) return;

            WCFRenderReport(sender, e);
        }

        private void WCFOpeningReportInDesigner(object sender, Stimulsoft.Report.Events.StiWCFOpeningReportEventArgs e)
        {
            if (!Equals(sender, DesignerControl)) return;

            //Открываем файл
            using (System.Windows.Forms.OpenFileDialog openDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openDialog.Filter = "MRT-отчет|*.mrt";
                if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        _reportUn = null;
                        tbReportName.Text = "";

                        var s = openDialog.OpenFile();
                        var report = new StiReport();
                        report.Load(s);
                        var dc = DesignerControl;
                        if (dc == null) return;
                        dc.Report = report;
                        tbReportName.Text = Path.GetFileNameWithoutExtension(openDialog.FileName) + " (из файла)";
                    }
                    catch (Exception ex)
                    {
                        Manager.UI.ShowMessage("Отчет не загружен:\n" + ex.Message);
                        var dc = DesignerControl;
                        if (dc == null) return;
                        dc.Report = new StiReport();
                    }
                }
            }
        }

        //private void WCFRetrieveColumns(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        //{
        //    if (!Equals(sender, DesignerControl)) return;

        //    var dataStoreSourceEditWindow = sender as StiDataStoreSourceEditWindow;
        //    if (dataStoreSourceEditWindow == null) return;

        //    LayoutGrid.RunBackgroundAsync<string>("RetrieveColumns", dataStoreSourceEditWindow.ApplyResultAfterRetrieveColumns, EnumServiceType.StimulReport, e.Xml);
        //}

        private void WCFBuildObjects(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            if (!Equals(sender, DesignerControl)) return;

            var selectDataWindow = sender as StiSelectDataWindow;
            if (selectDataWindow == null) return;

            LayoutGrid.RunBackgroundAsync<string>("BuildObjects", selectDataWindow.ApplyResultAfterBuildObjects, EnumServiceType.StimulReport, e.Xml);
        }

        private void WCFTestConnection(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            if (!Equals(sender, DesignerControl)) return;

            var testConnecting = sender as IStiTestConnecting;
            if (testConnecting == null) return;

            LayoutGrid.RunBackgroundAsync<string>("TestConnection", testConnecting.ApplyResultAfterTestConnection, EnumServiceType.StimulReport, e.Xml);
        }

        /// <summary>
        /// Основная процедура формирования отчета
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WCFRenderReport(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            if (!Equals(sender, DesignerControl)) return;
            var businessObjects = StiBusinessObjectHelper.GetUsedBusinessObjectsNames(DesignerControl.Report);

            _xml = e.Xml;

            if (businessObjects == null || businessObjects.Count == 0)
            {
                //На форме нет бизнес объектов
                if (DateRangeMode == enumDateRangeMode.WithoutDateMode && TreeMode == enumTreeMode.None)
                {
                    RenderReport(null);
                }
                else
                {
                    var isShowChannelSelector = CheckBoxChannels.IsChecked.GetValueOrDefault();
                    StiHelper.ShowMultiPSSelector(LayoutGrid, DateRangeMode, TreeMode, isShowChannelSelector);
                }

                return;
            }

            _businessObjectName = string.Empty;
            foreach (var key in businessObjects.Keys)
            {
                var businessObject = businessObjects[key];
                //Тип бизнес объекта в отчете
                _businessObjectName = businessObject.ToString();
                break; //Имеет смысл использовать только один бизнес объект в отчете
            }
            //Запрашиваем параметры
            StiHelper.ShowMultiPSSelector(LayoutGrid, _businessObjectName);
        }

        #endregion

        private void ProcessingString()
        {
            var d = new byte[170000000];
            string sb = string.Empty;
            byte[] buffer = new byte[80960000];
            foreach (var range in Partitioner.Create(0, d.Length, 80960000).GetDynamicPartitions())
            {
                //ProcessBytes(buffer, 0, read);
                Array.Copy(d, range.Item1, buffer, 0, range.Item2 - range.Item1);
                sb+= System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            }

            var r = sb.ToString();
        }

        private string _businessObjectName;
        private string _xml;
        /// <summary>
        /// Расчитываем отчет
        /// </summary>
        private void RenderReport(MultiPsSelectedArgs args)
        {
            BusinessObjectHelper.BuildBusinessObjectsParams(_businessObjectName, args);
            
            LayoutGrid.RunAsync(() =>
            {
                string result = string.Empty;
                try
                {
                    var errs = new StringBuilder();
                    result = StimulReportsProcedures.RenderReport(_xml, errs, args, Manager.Config.TimeZone);
                    if (errs.Length > 0) Manager.UI.ShowMessage(errs.ToString());
                }
                catch (Exception ex)
                {
                    Manager.UI.ShowMessage(ex.Message);
                }

                return result;
            }, rendered =>
            {
                if (string.IsNullOrEmpty(rendered) || rendered.Length <= 2) return;

                _businessObjectName = string.Empty;
                _xml = null;
                try
                {
                    DesignerControl.ApplyRenderedReport(rendered);
                }
                catch (Exception ex)
                {
                    Manager.UI.ShowMessage(ex.Message);
                }

            });
        }

        /// <summary>
        /// Закрытие модального окна 
        /// </summary>
        /// <param name="args"></param>
        public void OnClosedModalAction(params object[] args)
        {
            //После закрытия MultiPSSelector
            if (args == null || args.Length == 0) return;
            RenderReport(args.ElementAtOrDefault(0) as MultiPsSelectedArgs);
        }

        private void OnLoadConfiguration(string config)
        {

        }

        #region IModule

        public ModuleType ModuleType
        {
            get { return ModuleType.StimulReportDesigner; }
        }

        public bool Init()
        {
            return true;
        }

        public CloseAction Close()
        {
            return CloseAction.HideToHistory;
        }

        public string Shortcut
        {
            get
            {
                return "";
            }
            set
            {

            }
        }

        /// <summary>
        /// Что запускаем после восстановления из истории
        /// </summary>
        public string ActionAfterRestore { get; set; }

        /// <summary>
        /// Параметры конструктора сжатые Protobuf
        /// </summary>
        public List<TTypedParam> ConstructorParams { get; set; }

        /// <summary>
        /// Путь к иконке модуля
        /// </summary>
        public string IconPath { get; set; }

        private string _reportName;
        public string GetModuleToolTip
        {
            get
            {
                if (string.IsNullOrEmpty(_reportName)) return "Построитель отчета";

                return _reportName;
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            PopupBase.SourceModule = null;

            this.Tag = null;

            // Designer
            StiOptions.WCFService.WCFRenderReport -= WCFRenderReport;
            StiOptions.WCFService.WCFTestConnection -= WCFTestConnection;
            StiOptions.WCFService.WCFBuildObjects -= WCFBuildObjects;
            //StiOptions.WCFService.WCFRetrieveColumns -= WCFRetrieveColumns;
            StiOptions.WCFService.WCFOpeningReportInDesigner -= WCFOpeningReportInDesigner;
            StiOptions.WCFService.WCFRequestFromUserRenderReport -= WCFRequestFromUserRenderReport;
            StiOptions.Engine.GlobalEvents.SavingReportInDesigner -= GlobalEvents_SavingReportInDesigner;

            // Interactions
            StiOptions.WCFService.WCFRenderingInteractions -= WCFRenderingInteractions;

            // Prepare RequestFromUser Variables
            StiOptions.WCFService.WCFPrepareRequestFromUserVariables -= WCFPrepareRequestFromUserVariables;
            StiOptions.WCFService.WCFInteractiveDataBandSelection -= WCFInteractiveDataBandSelection;

            //if (reportDomain != null)
            //{
            //    AppDomain.Unload(reportDomain);
            //    reportDomain = null;
            //} 

            StiReport.ClearImageCache();
            StiReport.ClearReportCache();

            //designer.DesignerPreviewControl.TargetUpdated = null;
            //designer.DesignerPreviewControl = null;
            //designer.Report.ClearAllStates();

            //designer.PageView.Dispose();

            //var report = designer.Report;
            //if (report != null)
            //{
            //    report.Dictionary.DataStore.Clear();
            //    if (report.CompiledReport != null) report.CompiledReport.DataStore.Clear();

            //    report.Dispose();
            //}
            //designer.Report = null;

            //designer.DictionaryPanelService.Dispose();
            //designer.CurrentTool.Dispose();

            //LayoutGrid.Content = null;

            DesignerControl.Proryv_Dispose(null);


            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void _PropertyChanged(string property)
        {
            if (PropertyChanged != null && !DesignerProperties.GetIsInDesignMode(this))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            var stringName = tbReportName.Text;
            var isShowChannelSelector = CheckBoxChannels.IsChecked.GetValueOrDefault();

            if (string.IsNullOrEmpty(stringName) || stringName == "<Новый отчет>")
            {
                var businessObjects = StiBusinessObjectHelper.GetUsedBusinessObjectsNames(DesignerControl.Report);
                if (businessObjects == null || businessObjects.Count == 0)
                {
                    Manager.UI.ShowMessage("Задайте осмысленное название отчета");
                    return;
                }

                foreach (var key in businessObjects.Keys)
                {
                    var businessObject = businessObjects[key];
                    //Тип бизнес объекта в отчете
                    stringName = tbReportName.Text = businessObject.ToString();
                    break; //Имеет смысл использовать только один бизнес объект в отчете
                }
            }

            var data = new MemoryStream();
            DesignerControl.Report.Save(data);

            var reportInfo = new TReportInfo
            {
                ReportUn = _reportUn,
                StringName = stringName,
                UserId = Manager.User.User_ID,
                Data = data.ToArray().EncodeBuffer(),
                DateRangeMode = DateRangeMode,
                TreeMode = TreeMode,
                IsShowChannelSelector = isShowChannelSelector,
            };

            data.Dispose();

            LayoutGrid.RunBackgroundAsync<string>("SaveReport", reportUn =>
            {
                if (!string.IsNullOrEmpty(reportUn))
                {
                    _reportUn = reportUn;
                    Manager.UI.ShowMessage("Отчет успешно сохранен");
                }

            }, EnumServiceType.StimulReport, reportInfo);
        }

        private void LayoutGridOnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue) StiOptions.WCFService.UseWCFService = true;
        }
    }
}
