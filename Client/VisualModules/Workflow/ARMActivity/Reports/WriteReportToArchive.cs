using System;
using System.Collections.Generic;
using System.ComponentModel;
using Proryv.Workflow.Activity.Settings;
using System.Activities;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.IO;
using System.Activities.Presentation.Metadata;
using Proryv.AskueARM2.Client.ServiceReference.Service;

namespace Proryv.Workflow.Activity.ARM
{
    public class WriteReportToArchive : BaseArmActivity<bool>
    {
        public WriteReportToArchive()
        {
            this.DisplayName = "Сформировать отчет и записать в архив";
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(WriteReportToArchive), "UserName", new EditorAttribute(typeof(SendEmailToUserPropEditor), typeof(SendEmailToUserPropEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());

            builder.AddCustomAttributes(typeof(WriteReportToArchive), "Report_id", new EditorAttribute(typeof(ReportIDPropEditor), typeof(ReportIDDialog)));
            MetadataStore.AddAttributeTable(builder.CreateTable());

        }

        [Description("Тайм-аут выполнения  и загрузки отчета ")]
        [DisplayName("Тайм-аут выполнения")]
        [Category("Настройки")]
        public InArgument<TimeSpan?> WcfTimeOut { get; set; }

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

        [RequiredArgument]
        [Description("Имя пользователя")]
        [DisplayName("Пользователь")]
        public string UserName { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (string.IsNullOrEmpty(UserName))
                metadata.AddValidationError("Значение свойства 'Пользователь' не может быть пустым");
            base.CacheMetadata(metadata);
        }


        protected override bool Execute(CodeActivityContext context)
        {
            Error.Set(context, null);
            KeyValuePair<Guid, TReportResult> RepF = new KeyValuePair<Guid, TReportResult>();
            MemoryStream doc = null;
            try
            {
                /*
                string User_ID = "";
                List<UserInfo> UInfos = ARM_Service.EXPL_Get_All_Users();
                foreach (UserInfo u in UInfos)
                    if (u.UserName == UserName)
                    {
                        User_ID = u.User_ID;
                        break;
                    }
                if (string.IsNullOrEmpty(User_ID))
                {
                    Error.Set(context, "Пользователь '" + UserName+"' не найден в системе");
                    return false;
                }
                */

                string userId = null;
                if (!string.IsNullOrEmpty(UserName))
                {
                    try
                    {
                        userId = UserHelper.GetIdByUserName(UserName);
                    }
                    catch (Exception ex)
                    {
                        Error.Set(context, ex.Message);
                        if (!HideException.Get(context))
                            throw ex;
                    }
                }

                if (string.IsNullOrEmpty(userId))
                {
                    Error.Set(context, "Пользователь '" + UserName + "' не найден в системе");
                    return false;
                }

                RepF = ARM_Service.REP_Export_Report(userId, ReportFormat, Report_id.Get(context), StartDateTime.Get(context), EndDateTime.Get(context), null, WcfTimeOut.Get(context));
                doc = LargeData.DownloadData(RepF.Key);

                if (!string.IsNullOrEmpty(RepF.Value.Error))
                {
                    Error.Set(context, RepF.Value.Error);
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
