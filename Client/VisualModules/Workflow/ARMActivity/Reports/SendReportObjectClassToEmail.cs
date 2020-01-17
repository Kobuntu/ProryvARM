using System;
using System.Collections.Generic;
using System.Activities;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.ComponentModel;
using System.IO;
using System.Activities.Presentation.Metadata;
using Proryv.AskueARM2.Client.ServiceReference.Service;

namespace Proryv.Workflow.Activity.ARM
{
    public class SendReportObjectClassToEmail : SendReportToEmailBase
    {
        public SendReportObjectClassToEmail()
        {
            DisplayName = "Сформировать отчеты для класса объекта и отправить на почту";
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(SendReportObjectClassToEmail), "Report_id", new EditorAttribute(typeof(ReportObjectClassIDPropEditor), typeof(ReportObjectClassIDDialog)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }



        [Description("Тайм-аут выполнения  и загрузки отчета ")]
        [DisplayName("Тайм-аут выполнения")]
        [Category("Настройки")]
        public InArgument<TimeSpan?> WcfTimeOut { get; set; }

        [Description("Идентификатор уровня 1")]
        [DisplayName("Идентификатор уровня 1")]
        [Category("Отчет")]
        [DefaultValue(null)]
        public InArgument<byte?> HierLev1_ID { get; set; }

        [Description("Идентификатор уровня 2")]
        [DisplayName("Идентификатор уровня 2")]
        [Category("Отчет")]
        [DefaultValue(null)]
        public InArgument<int?> HierLev2_ID { get; set; }

        [Description("Идентификатор уровня 3")]
        [DisplayName("Идентификатор уровня 3")]
        [Category("Отчет")]
        [DefaultValue(null)]
        public InArgument<int?> HierLev3_ID { get; set; }

        [Description("Идентификатор ПС")]
        [DisplayName("Идентификатор ПС")]
        [Category("Отчет")]
        [DefaultValue(null)]
        public InArgument<int?> PS_ID { get; set; }

        [Description("Идентификатор юр. лица")]
        [DisplayName("Идентификатор юр. лица")]
        [Category("Отчет")]
        [DefaultValue(null)]
        public InArgument<int?> JuridicalPerson_ID { get; set; }

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


        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (HierLev1_ID == null && HierLev2_ID == null && HierLev3_ID == null && PS_ID == null && JuridicalPerson_ID == null)
                metadata.AddValidationError("Одно из свойств должно быть определено : 'Идентификатор уровня 1','Идентификатор уровня 2','Идентификатор уровня 3','Идентификатор ПС'");
            base.CacheMetadata(metadata);
        }

        protected override bool Execute(CodeActivityContext context)
        {
            Error.Set(context, null);
            //SectionIntegralComplexResults DataReport;

            if (JuridicalPerson_ID.Get(context) != null)
            {
                SendEmail(context, null);
            }
            else
            {
                List<Dict_P> psList = null;
                if (PS_ID.Get(context) == null)
                {
                    var hier1 = HierLev1_ID.Get(context);
                    var hier2 = HierLev2_ID.Get(context);
                    var hier3 = HierLev3_ID.Get(context);

                    psList = ARM_Service.Tree_GetListPSForHierLevels(hier1.HasValue ? new List<int> {hier1.Value} : null,
                        hier2.HasValue ? new List<int> {hier2.Value} : null, hier3.HasValue ? new List<int> {hier3.Value} : null);
                }
                else
                {
                    psList = new List<Dict_P> {new Dict_P {PS_ID = (int) this.PS_ID.Get(context)}};
                }

                if (psList == null || psList.Count == 0)
                {
                    string err = "Не найдено ни одной ПС";
                    Error.Set(context, err);
                    if (!HideException.Get(context))
                        throw new Exception(err);
                }

                foreach (var p in psList)
                {
                    SendEmail(context, p.PS_ID);
                }
            }

            return string.IsNullOrEmpty(Error.Get(context));
        }

        private void SendEmail(CodeActivityContext context, int? psId)
        {
            try
            {
                var repF = ARM_Service.REP_Export_ReportObjectClass(User_ID, ReportFormat, Report_id.Get(context), null,
                    HierLev1_ID.Get(context),
                    HierLev2_ID.Get(context),
                    HierLev3_ID.Get(context),
                    psId, JuridicalPerson_ID.Get(context),
                    StartDateTime.Get(context), EndDateTime.Get(context), null, WcfTimeOut.Get(context));

                var doc = LargeData.DownloadData(repF.Key);


                if (!string.IsNullOrEmpty(repF.Value.Error))
                {
                    Error.Set(context, repF.Value.Error);
                }
                else
                {
                    //var fs = new FileStream("C:\\777\\1\\rep_"+p.PS_ID.ToString()+".xls", FileMode.Create, FileAccess.Write);
                    //DataReport.Document.CopyTo(fs);
                    //fs.Close();
                    SendEmail(context, doc);
                }
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }
        }

    }
}
