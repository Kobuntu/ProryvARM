using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.Common;
using Proryv.Workflow.Activity.ARM.PropertyEditors;
using Proryv.Workflow.Activity.ARM.PropertyEditors.BusinessObjectsReports;
using Proryv.AskueARM2.Both.VisualCompHelpers;

namespace Proryv.Workflow.Activity.ARM
{
    public class SendBusinessObjectsReportToEmail : SendReportToEmailBase
    {
        public SendBusinessObjectsReportToEmail()
        {
            DisplayName = "Сформировать универсальный отчет и отправить на почту";
            Port = 25;

            //StartDateTime = new InArgument<DateTime>(DateTime.Now);
            //EndDateTime = new InArgument<DateTime>(DateTime.Now);
            //To = new InArgument<string>("");
            //From = new InArgument<string>("");
            //Subject = new InArgument<string>("");
            //Body = new InArgument<string>("");
            //AttachName = new InArgument<string>("");
            //UserName = new InArgument<string>("");
            //Password = new InArgument<string>("");
            //Host = new InArgument<string>("");

            AttributeTableBuilder builder = new AttributeTableBuilder();

            builder.AddCustomAttributes(typeof (SendBusinessObjectsReportToEmail), "User_ID", new EditorAttribute(typeof (SendEmailToUserPropEditor), typeof (SendEmailToUserPropEditor)));
            builder.AddCustomAttributes(typeof (SendBusinessObjectsReportToEmail), "Report_id", new EditorAttribute(typeof (ReportBusinessObjectsIdEditor), typeof (ReportBusinessObjectsIdDialog)));
            builder.AddCustomAttributes(typeof (SendBusinessObjectsReportToEmail), "Args", new EditorAttribute(typeof (ReportBusinessObjectsArgsEditor), typeof (ReportBusinessObjectsArgsDialog)));

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

        #endregion

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (Port <= 0)
                metadata.AddValidationError("Значение свойства 'Порт' должно быть больше 0");

            if (Args == String.Empty)
            {
                metadata.AddValidationError("Выбранные объекты не соответствуют типу отчета!");
            }

            base.CacheMetadata(metadata);
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

            var businessObjectName = string.Empty; //Определяем какой бизнес объект используется в отчете
            try
            {
                try
                {
                    if (!string.IsNullOrEmpty(reportUn))
                    {
                        businessObjectName = ServiceFactory.StimulReportInvokeSync<string>("GetUsedBusinessObjectsNames", reportUn);
                    }
                }
                catch (Exception ex)
                {
                    Error.Set(context, "Ошибка запроса бизнес модели отчета " + ex.Message);
                    return false;
                }

                BusinessObjectHelper.BuildBusinessObjectsParams(businessObjectName, args);

                var errs = new StringBuilder();
                var compressed = StimulReportsProcedures.LoadDocument(Report_id.Get(context), errs, args, ReportFormat, args.TimeZoneId);
                if (errs.Length > 0) Error.Set(context, errs.ToString());


                if (compressed == null)
                {
                    Error.Set(context, "Ошибка загрузки документа");
                    return false;
                }

                SendEmail(context, CompressUtility.DecompressGZip(compressed));
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
