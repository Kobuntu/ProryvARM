using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using Proryv.Workflow.Activity.Settings;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.IO;

namespace Proryv.Workflow.Activity.ARM
{
    public class XMLExportGetSectionList80020 : BaseArmActivity<bool>
    {
        public XMLExportGetSectionList80020()
        {
            this.DisplayName = "XML документ 80020 для списка секций";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Список идентификаторов секций")]
        [RequiredArgument]
        public InArgument<List<Int32>> ListSectionID { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Дата")]
        [RequiredArgument]
        public InArgument<DateTime> EventDate { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Тип учета")]
        [TypeConverter(typeof(enumBusRelationTypeConverter))]
        public enumBusRelation BusRelation { get; set; }

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

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор часового пояса")]
        [DefaultValue(null)]
        public string TimeZoneId { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            List<int> id_List = ListSectionID.Get(context);
            DateTimeOffset _EventDate = EventDate.Get(context);
            bool roundData = RoundData.Get(context);
            if (id_List == null || id_List.Count == 0)
            {
                Error.Set(context, "Список секций не может быть пустым");
                return false;
            }

            try
            {
                Stream Res = ARM_Service.XMLExportGetSection80020ForSectionArray(_EventDate, id_List, DataSourceType, BusRelation, roundData, false, TimeZoneId, true, false, false, 1, true).XMLStream;
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
