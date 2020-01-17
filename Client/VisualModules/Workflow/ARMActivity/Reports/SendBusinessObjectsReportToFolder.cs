using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Markup;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.Common;
using Proryv.Workflow.Activity.ARM.PropertyEditors;
using Proryv.Workflow.Activity.ARM.PropertyEditors.BusinessObjectsReports;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using System.Activities.Presentation.Metadata;
using System.Activities;
using System.IO;

namespace Proryv.Workflow.Activity.ARM.Reports
{
    public class SendBusinessObjectsReportToFolder : BaseArmActivity<bool>
    {

        public SendBusinessObjectsReportToFolder()
        {
            DisplayName = "Сформировать универсальный отчет и сохранить на диск";

            AttributeTableBuilder builder = new AttributeTableBuilder();

            builder.AddCustomAttributes(typeof(SendBusinessObjectsReportToFolder), "User_ID", new EditorAttribute(typeof(SendEmailToUserPropEditor), typeof(SendEmailToUserPropEditor)));
            builder.AddCustomAttributes(typeof(SendBusinessObjectsReportToFolder), "Report_id", new EditorAttribute(typeof(ReportBusinessObjectsIdEditor), typeof(ReportBusinessObjectsIdDialog)));
            builder.AddCustomAttributes(typeof(SendBusinessObjectsReportToFolder), "Args", new EditorAttribute(typeof(ReportBusinessObjectsArgsEditor), typeof(ReportBusinessObjectsArgsDialog)));

            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        #region Параметры формирования отчета

        [Description("Строка ошибки")]
        [DisplayName(@"Ошибка")]
        public OutArgument<string> Error { get; set; }

        [RequiredArgument]
        [Description("Уникальный номер отчета")]
        [DisplayName("Идентификатор отчета")]
        [Category("Отчет")]
        [OverloadGroup("G1")]
        public InArgument<string> Report_id { get; set; }


        [RequiredArgument]
        [Description("Начальная дата отчета")]
        [DisplayName("Начальная дата")]
        [Category("Отчет")]
        public InArgument<DateTime> StartDateTime { get; set; }

        [RequiredArgument]
        [Description("Конечная дата отчета")]
        [DisplayName("Конечная дата")]
        [Category("Отчет")]
        public InArgument<DateTime> EndDateTime { get; set; }

        [Description("Формат отчета")]
        [DisplayName("Формат")]
        [Category("Отчет")]
        public ReportExportFormat ReportFormat { get; set; }

        [Description("Идентификатор пользователя в системе")]
        [DisplayName("Идентификатор пользователя")]
        [Category("Отчет")]
        public string User_ID { get; set; }

        [RequiredArgument]
        [Description("Объект или список объектов по которым строится отчет")]
        [DisplayName(@"Объект(ы) для построения отчета")]
        [Category("Отчет")]
        [DependsOn("Report_id")]
        [OverloadGroup("G1")]
        public string Args { get; set; }

        [Category("Настройки")]
        [DisplayName("Папка")]
        public InArgument<string> Folder { get; set; }

        [RequiredArgument]
        [Category("Настройки")]
        [DisplayName("Имя файла")]
        [Description("Имя файла. (Расширение добавится автоматически)")]
        public InArgument<string> FileName { get; set; }


        #endregion

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (Args == String.Empty)
            {
                metadata.AddValidationError("Выбранные объекты не соответствуют типу отчета!");
            }

            base.CacheMetadata(metadata);
        }


        public string GetFileExtByReportFormat()
        {
            string Ext = "";
            if (ReportFormat == ReportExportFormat.Csv) Ext = "csv";
            if (ReportFormat == ReportExportFormat.Excel) Ext = "xls";
            if (ReportFormat == ReportExportFormat.Excel2007) Ext = "xlsx";
            if (ReportFormat == ReportExportFormat.ImageBmp) Ext = "bmp";
            if (ReportFormat == ReportExportFormat.ImageEmf) Ext = "emf";
            if (ReportFormat == ReportExportFormat.ImageGif) Ext = "gif";
            if (ReportFormat == ReportExportFormat.ImageJpeg) Ext = "jpg";
            if (ReportFormat == ReportExportFormat.ImagePcx) Ext = "pcx";
            if (ReportFormat == ReportExportFormat.ImagePng) Ext = "png";
            if (ReportFormat == ReportExportFormat.ImageTiff) Ext = "Tiff";
            if (ReportFormat == ReportExportFormat.Mht) Ext = "mht";
            if (ReportFormat == ReportExportFormat.Ods) Ext = "ods";
            if (ReportFormat == ReportExportFormat.Odt) Ext = "odt";
            if (ReportFormat == ReportExportFormat.Pdf) Ext = "pdf";
            if (ReportFormat == ReportExportFormat.Ppt2007) Ext = "ppt";
            if (ReportFormat == ReportExportFormat.Rtf) Ext = "rtf";
            if (ReportFormat == ReportExportFormat.Text) Ext = "txt";
            if (ReportFormat == ReportExportFormat.Word2007) Ext = "docx";
            if (ReportFormat == ReportExportFormat.Xps) Ext = "xps";
            if (Ext != "")
                Ext = "." + Ext;
            return Ext;
        }


        public void PutReportToFolder(CodeActivityContext context, MemoryStream Document)
        {
            string folder;
            string fileName;


            folder = context.GetValue(this.Folder);
            fileName = context.GetValue(this.FileName);
            fileName = ReportTools.CorrectFileName(fileName + GetFileExtByReportFormat());
            fileName = Path.Combine(folder, fileName);
            using (FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                Document.WriteTo(file);
            }
        }

        protected override bool Execute(System.Activities.CodeActivityContext context)
        {
            Error.Set(context, null); 

            if (string.IsNullOrEmpty(Args))
            {
                Error.Set(context, "Не определены объекты для которых будет сформирован отчет");
                return false;
            }

            MultiPsSelectedArgs args;
            try
            {
                args = Args.DeserializeFromString<MultiPsSelectedArgs>();
            }
            catch (Exception ex)
            {
                Error.Set(context, "Ошибка преобразования параметров " + ex.Message);
                return false;
            }

            args.DtStart = StartDateTime.Get(context); //Начальная дата
            args.DtEnd = EndDateTime.Get(context); //Конечная дата
            var reportUn = Report_id.Get(context);

            string timeZoneId = null;
            if (Manager.Config != null) timeZoneId = Manager.Config.TimeZone;

            string businessObjectName = string.Empty; //Определяем какой бизнес объект используется в отчете
            try
            {
                try
                {
                    businessObjectName = ServiceFactory.StimulReportInvokeSync<string>("GetUsedBusinessObjectsNames", reportUn);
                }
                catch (Exception ex)
                {
                    Manager.UI.ShowMessage("Ошибка запроса бизнес модели отчета " + ex.Message);
                }

                BusinessObjectHelper.BuildBusinessObjectsParams(businessObjectName, args);

                var errs = new StringBuilder();
                var compressed = StimulReportsProcedures.LoadDocument(Report_id.Get(context), errs, args, ReportFormat, timeZoneId);
                if (errs.Length > 0) Error.Set(context, errs.ToString());

                PutReportToFolder(context, CompressUtility.DecompressGZip(compressed));

            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            return string.IsNullOrEmpty(Error.Get(context));
        }

    }
}
