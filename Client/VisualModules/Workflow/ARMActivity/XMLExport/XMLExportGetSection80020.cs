using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Proryv.Workflow.Activity.Settings;
using System.Activities;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.IO;

namespace Proryv.Workflow.Activity.ARM
{
    public class XMLExportGetSection80020 : BaseArmActivity<bool>
    {
        public XMLExportGetSection80020()
        {
            this.DisplayName = "XML документ 80020 для Секции";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор секции")]
        [RequiredArgument]
        public InArgument<Int32> SectionID { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Дата")]
        [RequiredArgument]
        public InArgument<DateTime> EventDate { get; set; }

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

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
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
            DateTimeOffset _EventDate = EventDate.Get(context);
            bool roundData = RoundData.Get(context);
            try
            {
                XMLATSExportSingleObjectResult Res = ARM_Service.XMLExportGetSection80020(_EventDate, id, DataSourceType, BusRelation, TimeZoneId, roundData,false, false, true, false, false, 1, true);
                Document.Set(context, Res.XMLStream);
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
