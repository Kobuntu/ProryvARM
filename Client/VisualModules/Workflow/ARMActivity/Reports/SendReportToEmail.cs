using System;
using System.Collections.Generic;
using System.Activities;
using System.ComponentModel;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.IO;
using System.Activities.Presentation.Metadata;
using Proryv.AskueARM2.Client.ServiceReference.Service;

namespace Proryv.Workflow.Activity.ARM
{
   // [Designer(typeof(SendReportToEmailDesigner))]
    public class SendReportToEmail : SendReportToEmailBase
    {
        public SendReportToEmail()
        {
            DisplayName = "Сформировать отчет и отправить на почту";

            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(SendReportToEmail), "User_ID", new EditorAttribute(typeof(SendEmailToUserPropEditor), typeof(SendEmailToUserPropEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());

            builder.AddCustomAttributes(typeof(SendReportToEmail), "Report_id", new EditorAttribute(typeof(ReportIDPropEditor), typeof(ReportIDDialog)));
            MetadataStore.AddAttributeTable(builder.CreateTable());

            

        }

        [Description("Тайм-аут выполнения  и загрузки отчета ")]
        [DisplayName("Тайм-аут выполнения")]
        [Category("Настройки")]
        public InArgument<TimeSpan?> WcfTimeOut { get; set; }

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

        [Description("Идентификатор пользователя в системе")]
        [DisplayName("Идентификатор пользователя")]
        [Category("Отчет")]
        public string User_ID { get; set; }



       

        /*
        [Description("Строка ошибки")]
        [DisplayName("Ошибка")]
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


        protected string GetFileExtByReportFormat()
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

        protected void SendEmail(CodeActivityContext context,MemoryStream AttachContent)
        {

            MailMessage mailMessage = new MailMessage();

            mailMessage.From = new MailAddress(From.Get(context));
            string STo = To.Get(context);

            STo.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList()
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
                Attachment Attach = new Attachment(AttachContent, "Отчет"+GetFileExtByReportFormat());
                mailMessage.Attachments.Add(Attach);
            }
            //throw new Exception("Error sending email"); // test exception
            smtpClient.Send(mailMessage);
        }
        */

        protected override bool Execute(CodeActivityContext context)
        {
            Error.Set(context, null);
            //SectionIntegralComplexResults DataReport;
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
                        if (string.IsNullOrEmpty(userId)) // похоже тут не имя а UserID
                            userId = User_ID;
                    }
                    catch (Exception ex)
                    {
                        Error.Set(context, ex.Message);
                        if (!HideException.Get(context))
                            throw ex;
                    }
                }

                RepF = ARM_Service.REP_Export_Report(userId, ReportFormat, Report_id.Get(context),
                                                     StartDateTime.Get(context), EndDateTime.Get(context), null, WcfTimeOut.Get(context));
                doc = LargeData.DownloadData(RepF.Key);


                if (!string.IsNullOrEmpty(RepF.Value.Error))
                {
                    Error.Set(context, RepF.Value.Error);
                }
                else
                    SendEmail(context, doc);
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
