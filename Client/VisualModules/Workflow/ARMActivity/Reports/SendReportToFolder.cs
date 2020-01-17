using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities.Presentation.Metadata;
using System.Activities;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.IO;
using System.Net;
using Proryv.Workflow.Activity.Settings;
using Proryv.AskueARM2.Client.ServiceReference.Service;

namespace Proryv.Workflow.Activity.ARM
{
    public class SendReportToFolder : BaseArmActivity<bool>
    {

        public SendReportToFolder()
        {
            this.DisplayName = "Сформировать отчет и сохранить на диск";

            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(SendReportToFolder), "User_ID", new EditorAttribute(typeof(SendEmailToUserPropEditor), typeof(SendEmailToUserPropEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());

            builder.AddCustomAttributes(typeof(SendReportToFolder), "Report_id", new EditorAttribute(typeof(ReportIDPropEditor), typeof(ReportIDDialog)));
            MetadataStore.AddAttributeTable(builder.CreateTable());

        }


        [Description("Строка ошибки")]
        [DisplayName("Ошибка")]
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        public OutArgument<string> Error { get; set; }

        [RequiredArgument]
        [Description("Уникальный номер отчета")]
        [DisplayName("Идентификатор отчета")]
        [Category("Отчет")]
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

        //--------------------------------------------------------------       

        [Category("Настройки")]
        [DisplayName("Папка")]
        public InArgument<string> Folder { get; set; }

        [RequiredArgument]
        [Category("Настройки")]
        [DisplayName("Имя файла")]
        [Description("Имя файла. (Расширение добавится автоматически)")]
        public InArgument<string> FileName { get; set; }

        [Description("Тайм-аут выполнения  и загрузки отчета ")]
        [DisplayName("Тайм-аут выполнения")]
        [Category("Настройки")]
        public InArgument<TimeSpan?> WcfTimeOut { get; set; }


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

        protected override bool Execute(CodeActivityContext context)
        {
            Error.Set(context, null);
            KeyValuePair<Guid, TReportResult> RepF = new KeyValuePair<Guid, TReportResult>();
            MemoryStream doc = null;
            try
            {

                string userId = null;
                if (!string.IsNullOrEmpty(User_ID))
                {
                    try
                    {
                        userId = UserHelper.GetIdByUserName(User_ID);
                        if (string.IsNullOrEmpty(userId)) 
                            userId = User_ID;
                    }
                    catch (Exception ex)
                    {
                        Error.Set(context, ex.Message);
                        if (!HideException.Get(context))
                            throw ex;
                    }
                }

                RepF = ARM_Service.REP_Export_Report(userId, ReportFormat, Report_id.Get(context), StartDateTime.Get(context), EndDateTime.Get(context), null, WcfTimeOut.Get(context));
                doc = LargeData.DownloadData(RepF.Key);


                if (!string.IsNullOrEmpty(RepF.Value.Error))
                {
                    Error.Set(context, RepF.Value.Error);
                }
                else
                    PutReportToFolder(context, doc);
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
