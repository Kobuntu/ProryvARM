using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.ComponentModel;
using System.Activities;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;

namespace Proryv.Workflow.Activity.ARM.Reports
{
    public class ArmNativeActConsumptionPowerToEmail : BaseArmActivity<bool>
    {
        public ArmNativeActConsumptionPowerToEmail()
        {
            DisplayName = "Сформировать отчет 'Акт потребления мощности для объекта' и отправить на почту";
            this.Port = 25;
        }


        [Description("Тайм-аут выполнения  и загрузки отчета ")]
        [DisplayName("Тайм-аут выполнения")]
        [Category("Настройки")]
        public InArgument<TimeSpan?> WcfTimeOut { get; set; }


        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }

        //--------------------------------------------------------------        

        [Category("Отчет")]
        [DisplayName("Идентификатор объекта")]
        public InArgument<int> ID { get; set; }

        [Category("Отчет")]
        [RequiredArgument]
        [DisplayName("Тип иeрархии объекта")]
        [TypeConverter(typeof(EnumTypeHierarchyTypeConverter))]
        public enumTypeHierarchy HierarchyType { get; set; }

        [Category("Отчет")]
        [DisplayName("Включать вложенные ТИ")]
        [RequiredArgument]
        public InArgument<bool> IncludeAllTI { get; set; }

        [Category("Отчет")]
        [DisplayName("Вычисляемые значения")]
        [RequiredArgument]
        public InArgument<bool> IsReadCalculatedValues { get; set; }

        [Category("Отчет")]
        [DisplayName("Тип измеряемой величины")]
        [TypeConverter(typeof(EnumenumTypeOfMeasuringTypeConverter))]
        public enumTypeOfMeasuring TypeOfMeasuring { get; set; }

        [Category("Отчет")]
        [DisplayName("Тип расчета")]
        [TypeConverter(typeof(enumGroupTPPowerReportModeTypeConverter))]
        public EnumGroupTPPowerReportMode GroupTPPowerReportMode { get; set; }
        

        [Category("Отчет")]
        [DisplayName("Год")]
        [RequiredArgument]
        public InArgument<int> Year { get; set; }

        [Category("Отчет")]
        [DisplayName("Месяц")]
        [RequiredArgument]
        public InArgument<byte> Month { get; set; }
        
        [Category("Отчет")]
        [DisplayName("Формат")]
        [TypeConverter(typeof(EnumExportExcelAdapterTypeTypeConverter))]
        public TExportExcelAdapterType ExcelAdapterType { get; set; }

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

        public string GetFileExt()
        {
            string Ext = "";
            if (ExcelAdapterType == TExportExcelAdapterType.toHTML) Ext = "html";
            if (ExcelAdapterType == TExportExcelAdapterType.toPDF) Ext = "PDF";
            if (ExcelAdapterType == TExportExcelAdapterType.toXLS) Ext = "XLS";
            if (ExcelAdapterType == TExportExcelAdapterType.toXLSx) Ext = "XLSx";
            if (Ext != "")
                Ext = "." + Ext;
            return Ext;
        }


        public void SendEmail(CodeActivityContext context, MemoryStream AttachContent)
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
                    Attach = new Attachment(AttachContent, attachName + GetFileExt());
                }
                mailMessage.Attachments.Add(Attach);
            }
            //throw new Exception("Error sending email"); // test exception
            smtpClient.Send(mailMessage);
        }

        protected override bool Execute(CodeActivityContext context)
        {
            DateTime dt = new DateTime(Year.Get(context), Month.Get(context), 1);
            try
            {

                ID_TypeHierarchy idHierarchy = new ID_TypeHierarchy();
                idHierarchy.ID = ID.Get(context);
                idHierarchy.TypeHierarchy = HierarchyType;

                SectionIntegralComplexResults result =
                    ARM_Service.GroupTP_GetActConsumptionPower(idHierarchy, GroupTPPowerReportMode, null,
                        TypeOfMeasuring, ExcelAdapterType, dt,
                        2, ".", IsReadCalculatedValues.Get(context), IncludeAllTI.Get(context), false, null, null, WcfTimeOut.Get(context));
                if (!string.IsNullOrEmpty(result.Errors.ToString()) && (result.Document == null || result.Document.Length ==0))
                {
                    Error.Set(context, result.Errors.ToString());
                }
                else
                {
                    SendEmail(context, CompressUtility.DecompressGZip(result.Document));
                }
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
