using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.IO;
using System.Net.Mail;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;

namespace Proryv.Workflow.Activity.ARM
{
    public abstract class SendReportToEmailBase : BaseArmActivity<bool>
    {

        public SendReportToEmailBase()
        {
            this.DisplayName = "Сформировать отчет и отправить на почту";
            this.Port = 25;
        }


        [Description("Строка ошибки")]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }

        [Description("Формат отчета")]
        [DisplayName("Формат")]
        [Category("Отчет")]
        public ReportExportFormat ReportFormat { get; set; }

        //--------------------------------------------------------------        
        [RequiredArgument]
        [Description("Адрес получателя")]
        [DisplayName("Адресат")]
        [Category("Электронная почта")]
        public InArgument<string> To { get; set; }

        [RequiredArgument]
        [Description("Адрес отправителя")]
        [DisplayName("Отправитель")]
        [Category("Электронная почта")]
        public InArgument<string> From { get; set; }

        [RequiredArgument]
        [Description("Тема письма")]
        [DisplayName("Тема")]
        [Category("Электронная почта")]
        public InArgument<string> Subject { get; set; }

        [DisplayName("Тело письма")]
        [Category("Электронная почта")]
        public InArgument<string> Body { get; set; }

        [DisplayName("Имя фала")]
        [Description("Имя фала вложения (расширение доб. автоматически)")]
        [Category("Электронная почта")]
        public InArgument<string> AttachName { get; set; }

        //[DisplayName("Архивировать вложение")]
        //[Description("Архивировать вложение (ZIP)")]
        //[Category("Электронная почта")]
        //public bool UseZipArchive { get; set; }


        [RequiredArgument]
        [DefaultValue(25)]
        [DisplayName("Порт")]
        [Category("Электронная почта")]
        public int Port { get; set; }

        [RequiredArgument]
        [DisplayName("Имя пользователя")]
        [Category("Электронная почта")]
        public InArgument<string> UserName { get; set; }

        [RequiredArgument]
        [DisplayName("Пароль")]
        [Category("Электронная почта")]
        public InArgument<string> Password { get; set; }

        [RequiredArgument]
        [DisplayName("Почтовый сервер")]
        [Category("Электронная почта")]
        public InArgument<string> Host { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (Port <= 0)
                metadata.AddValidationError("Значение свойства 'Порт' должно быть больше 0");

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


        public void SendEmail(CodeActivityContext context, MemoryStream AttachContent)
        {
            MailMessage mailMessage = new MailMessage();

            mailMessage.From = new MailAddress(From.Get(context));
            string STo = To.Get(context);

            STo.Split(new Char[] {';'}, StringSplitOptions.RemoveEmptyEntries).ToList()
                .ForEach(item => mailMessage.To.Add(item.Trim()));

            mailMessage.Subject = Subject.Get(context);
            mailMessage.Body = Body.Get(context);

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = Host.Get(context);
            smtpClient.Port = Port;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            if (!String.IsNullOrEmpty(UserName.Get(context)) && !String.IsNullOrEmpty(Password.Get(context)))
                smtpClient.Credentials = new System.Net.NetworkCredential(UserName.Get(context), Password.Get(context));

            if (AttachContent != null)
            {
                string attachName = AttachName.Get(context);
                if (string.IsNullOrEmpty(attachName))
                    attachName = "Отчет";
                else
                {
                    attachName = ReportTools.CorrectFileName(attachName);
                }
                Attachment Attach;

                //if (UseZipArchive)
                //{
                //    MemoryStream compresStream = new MemoryStream();
                //    ComponentAce.Compression.ZipForge.ZipForge zip = new ComponentAce.Compression.ZipForge.ZipForge();
                //    zip.FileName = attachName + ".zip"; ;
                //    AttachContent.Position = 0;
                //    zip.OpenArchive(compresStream, true);
                //    zip.AddFromStream(attachName + GetFileExtByReportFormat(), AttachContent);
                //    zip.CloseArchive();
                //    Attach = new Attachment(AttachContent, zip.FileName);
                //    zip.Dispose();
                //}
                //else
                {
                    Attach = new Attachment(AttachContent, attachName + GetFileExtByReportFormat());
                }
                mailMessage.Attachments.Add(Attach);
            }
            //throw new Exception("Error sending email"); // test exception
            smtpClient.Send(mailMessage);
        }
    }
}
