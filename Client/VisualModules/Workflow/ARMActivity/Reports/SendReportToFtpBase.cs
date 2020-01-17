using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.IO;
using System.Net;
using Proryv.Workflow.Activity.Settings;

namespace Proryv.Workflow.Activity.ARM
{
    public abstract class SendReportToFtpBase : BaseArmActivity<bool>
    {
        public SendReportToFtpBase()
        {
            this.DisplayName = "Сформировать отчет и отправить на FTP-сервер";
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

        [RequiredArgument]
        [Category("FTP-сервер")]
        [DisplayName("Адрес")]
        [Description("Адрес FTP-сервер")]
        public InArgument<string> FTPUrl { get; set; }
        [RequiredArgument]
        [Category("FTP-сервер")]
        [DisplayName("Имя пользователя")]
        public InArgument<string> Username { get; set; }
        [RequiredArgument]
        [Category("FTP-сервер")]
        [DisplayName("Пароль")]
        public InArgument<string> Password { get; set; }
        [Category("FTP-сервер")]
        [DisplayName("Папка")]
        public InArgument<string> Folder { get; set; }
        [RequiredArgument]
        [Category("FTP-сервер")]
        [DisplayName("Имя файла")]
        [Description("Имя файла. (Расширение добавится автоматически)")]
        public InArgument<string> FileName { get; set; }


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


        public void PutReportToFtp(CodeActivityContext context, MemoryStream Document)
        {
            string ftpURL = null;
            string username = null;
            string password = null;
            string folder;
            string fileName;


            ftpURL = context.GetValue(this.FTPUrl);
            username = context.GetValue(this.Username);
            password = context.GetValue(this.Password);
            folder = context.GetValue(this.Folder);
            fileName = context.GetValue(this.FileName);
            fileName = fileName + GetFileExtByReportFormat();
            Stream requestStream = null;
            FtpWebResponse uploadResponse = null;
            try
            {
                if (!(ftpURL.EndsWith("/"))) { ftpURL += "/"; }
                if (!string.IsNullOrEmpty(folder))
                if (!(folder.EndsWith("/"))) { folder += "/"; }

                FtpWebRequest uploadRequest;
                ICredentials credentials = new NetworkCredential(username, password);
                uploadRequest = (FtpWebRequest)WebRequest.Create(ftpURL + folder + fileName);
                uploadRequest.Method = WebRequestMethods.Ftp.UploadFile;
                uploadRequest.Credentials = credentials;
                uploadRequest.Proxy = null;
                uploadRequest.UsePassive = true;
                uploadRequest.UseBinary = true;

                requestStream = uploadRequest.GetRequestStream();
                Document.CopyTo(requestStream) ;
                requestStream.Close();
                uploadResponse = (FtpWebResponse)uploadRequest.GetResponse();
            }
            finally
            {
                if (uploadResponse != null)
                    uploadResponse.Close();
                if (requestStream != null)
                    requestStream.Close();
            }
        }

    }
}
