using System;
using System.Collections.Generic;
using System.Activities;
using System.IO;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using Proryv.AskueARM2.Client.ServiceReference.Service;

namespace Proryv.Workflow.Activity.ARM
{
    public class SendReportToFtp : SendReportToFtpBase
    {
        public SendReportToFtp()
        {
            this.DisplayName = "Сформировать отчет и отправить на FTP-сервер";

            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(SendReportToFtp), "User_ID", new EditorAttribute(typeof(SendEmailToUserPropEditor), typeof(SendEmailToUserPropEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());

            builder.AddCustomAttributes(typeof(SendReportToFtp), "Report_id", new EditorAttribute(typeof(ReportIDPropEditor), typeof(ReportIDDialog)));
            MetadataStore.AddAttributeTable(builder.CreateTable());

        }


        [Description("Тайм-аут выполнения  и загрузки отчета ")]
        [DisplayName("Тайм-аут выполнения")]
        [Category("Настройки")]
        public InArgument<TimeSpan?> WcfTimeOut { get; set; }


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

                RepF = ARM_Service.REP_Export_Report(userId, ReportFormat, Report_id.Get(context), StartDateTime.Get(context), EndDateTime.Get(context), null, WcfTimeOut.Get(context));
                doc = LargeData.DownloadData(RepF.Key);


                if (!string.IsNullOrEmpty(RepF.Value.Error))
                {
                    Error.Set(context, RepF.Value.Error);
                }
                else
                    PutReportToFtp(context, doc);
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
