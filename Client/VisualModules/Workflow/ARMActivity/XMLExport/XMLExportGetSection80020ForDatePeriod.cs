using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.Workflow.Activity.Settings;
using System.ComponentModel;
using System.Activities;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.IO;

namespace Proryv.Workflow.Activity.ARM
{
    public class XMLExportGetSection80020ForDatePeriod : BaseArmActivity<bool>
    {
        public XMLExportGetSection80020ForDatePeriod()
        {
            this.DisplayName = "XML документ 80020 для секции за период времени";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор секции")]
        [RequiredArgument]
        public InArgument<Int32> SectionID { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Начальная дата")]
        public InArgument<DateTime> StartDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Конечная дата")]
        public InArgument<DateTime> EndDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Тип учета")]
        [TypeConverter(typeof(enumBusRelationTypeConverter))]
        public enumBusRelation BusRelation { get; set; }
        
        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор часового пояса")]
        [DefaultValue(null)]
        public string TimeZoneId { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Источник")]
        public EnumDataSourceType? DataSourceType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Профиль")]
        [TypeConverter(typeof(EnumProfileTypeConverter))]
        public enumReturnProfile ProfileType { get; set; }

        [DisplayName("Округлять")]
        [RequiredArgument]
        public InArgument<bool> RoundData { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }
        
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Документ")]
        public OutArgument<MemoryStream> Document { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            int id = SectionID.Get(context);
            bool roundData = RoundData.Get(context);
            try
            {
                Stream Res = ARM_Service.XMLExportGetSection80020ForDatePeriod(StartDateTime.Get(context), EndDateTime.Get(context), id, DataSourceType, BusRelation, TimeZoneId, roundData, false, true, false, false, 1, true).XMLStream;
                MemoryStream ms = new MemoryStream();
                Res.CopyTo(ms);
                ms.Position = 0;

                Document.Set(context, ms);
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
