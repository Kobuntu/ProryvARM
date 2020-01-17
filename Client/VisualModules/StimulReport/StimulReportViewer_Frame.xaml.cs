using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.ServiceReference.StimulReportService;
using Proryv.AskueARM2.Client.Visual.Common;
using System;
using System.Collections.Generic;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using Proryv.AskueARM2.Client.Visual.Common.Common;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using ProryvAdapterDataReport;
using Stimulsoft.Report;

namespace Proryv.AskueARM2.Client.Visual.StimulReporter
{
    /// <summary>
    /// Interaction logic for StimulReportViewer_Frame.xaml
    /// </summary>
    public partial class StimulReportViewer_Frame : IModule, IDisposable, IModalCompleted
    {
        private readonly string _reportUn;

        public StimulReportViewer_Frame(string reportUn)
        {
            #region Настраиваем просмотрщик

            StimulReportFactory.PrepareDesigner();
            StiHelper.HyperLinkAttach();
            //StiHelper.StiWcfConfigure();

            // Viewer
            //Stimulsoft.Report.StiOptions.WCFService.WCFExportDocument += WCFExportDocument;
            //Stimulsoft.Report.StiOptions.WCFService.WCFRequestFromUserRenderReport += WCFRequestFromUserRenderReport;

            // Interactions
            //Stimulsoft.Report.StiOptions.WCFService.WCFRenderingInteractions += WCFRenderingInteractions;
            //Stimulsoft.Report.StiOptions.WCFService.WCFInteractiveDataBandSelection += WCFInteractiveDataBandSelection;

            // Prepare RequestFromUser Variables
            //Stimulsoft.Report.StiOptions.WCFService.WCFPrepareRequestFromUserVariables += WCFPrepareRequestFromUserVariables;

            #endregion

            InitializeComponent();

            _reportUn = reportUn;

            LoadRendered();
        }

        #region События WCF отчета

        private void WCFRequestFromUserRenderReport(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            if (!Equals(sender, ViewerControl)) return;

            Manager.UI.ShowMessage("WCFRequestFromUserRenderReport пока не поддерживается");
        }

        private void WCFRenderingInteractions(object viewer, Stimulsoft.Report.Events.StiWCFRenderingInteractionsEventArgs e)
        {
            if (!Equals(viewer, ViewerControl)) return;

            //Manager.UI.ShowMessage("WCFRenderingInteractions пока не поддерживается");
        }

        private void WCFInteractiveDataBandSelection(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            if (!Equals(sender, ViewerControl)) return;

            Manager.UI.ShowMessage("WCFInteractiveDataBandSelection пока не поддерживается");
        }

        private void WCFPrepareRequestFromUserVariables(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            if (!Equals(sender, ViewerControl)) return;

            Manager.UI.ShowMessage("WCFPrepareRequestFromUserVariables пока не поддерживается");
        }

        #endregion

        #region IModule

        public ModuleType ModuleType
        {
            get { return ModuleType.StimulReportViewer; }
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
            get { return ""; }
            set { }
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
                if (string.IsNullOrEmpty(_reportName)) return "Просмотр отчета";

                return _reportName;
            }
        }

        public FreeHierarchyTreeItem GetNode(IFreeHierarchyObject hierarchyObject)
        {
            return null;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            PopupBase.SourceModule = null;
            this.Tag = null;
            // Viewer
            Stimulsoft.Report.StiOptions.WCFService.WCFRequestFromUserRenderReport -= WCFRequestFromUserRenderReport;

            // Interactions
            Stimulsoft.Report.StiOptions.WCFService.WCFRenderingInteractions -= WCFRenderingInteractions;
            Stimulsoft.Report.StiOptions.WCFService.WCFInteractiveDataBandSelection -= WCFInteractiveDataBandSelection;

            // Prepare RequestFromUser Variables
            Stimulsoft.Report.StiOptions.WCFService.WCFPrepareRequestFromUserVariables -= WCFPrepareRequestFromUserVariables;

            ViewerControl.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        #endregion

        private void LoadRendered()
        {
            LayoutGrid.RunBackgroundAsync<string>("GetUsedBusinessObjectsNames", businessObjectsName =>
            {
                if (string.IsNullOrEmpty(businessObjectsName))
                {
                    LoadReport();
                    return;
                }
                _businessObjectName = businessObjectsName;
                
                //Запрашиваем параметры
                StiHelper.ShowMultiPSSelector(LayoutGrid, _businessObjectName);
            }, EnumServiceType.StimulReport, _reportUn);
        }

        private void LoadReport()
        {
            //Смотрим параметры отчета
             LayoutGrid.RunBackgroundAsync<TReportInfo>("LoadReport", response =>
             {
                 if (response == null || response.Data == null) return;
                 if ((enumDateRangeMode)response.DateRangeMode == enumDateRangeMode.WithoutDateMode && (enumTreeMode)response.TreeMode == enumTreeMode.None)
                 {
                     RenderReport(null);
                 }
                 else
                 {
                     //Модальный диалог выбора параметров
                     StiHelper.ShowMultiPSSelector(LayoutGrid, (enumDateRangeMode) response.DateRangeMode, (enumTreeMode) response.TreeMode, response.IsShowChannelSelector);
                 }

             }, EnumServiceType.StimulReport, _reportUn);
        }

        /// <summary>
        /// Вызывается после закрытия любого модального диалога на этой форме
        /// </summary>
        /// <param name="args"></param>
        public void OnClosedModalAction(params object[] args)
        {
            //После закрытия MultiPSSelector
            if (args == null || args.Length == 0) return;
            RenderReport(args[0] as MultiPsSelectedArgs);
        }

        private void RenderReport(MultiPsSelectedArgs args)
        {
            //Список параметров для формирования бизнес объектов
            BusinessObjectHelper.BuildBusinessObjectsParams(_businessObjectName, args);

            LayoutGrid.RunAsync(() =>
            {
                var result = string.Empty;
                try
                {
                    var errs = new StringBuilder();
                    result = StimulReportsProcedures.LoadRendered(_reportUn, errs, args, Manager.Config.TimeZone);
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
                try
                {
                    ViewerControl.ApplyRenderedReport(rendered, true);
                }
                catch (Exception ex)
                {
                    Manager.UI.ShowMessage("Ошибка отображения универсального отчета - " + ex.Message);
                }
            });
        }

        private string _businessObjectName;

        private void LayoutGridOnIsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue) StiOptions.WCFService.UseWCFService = true;
        }
    }
}