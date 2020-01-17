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
    public class XMLExportGetAIS80020 : BaseArmActivity<bool>
    {
        public XMLExportGetAIS80020()
        {
            this.DisplayName = "XML документ 80020 для АИС";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор АИС")]
        [RequiredArgument]
        public InArgument<string> AisId { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Дата")]
        [RequiredArgument]
        public InArgument<DateTime> EventDate { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Расчетные значения")]
        public bool isReadCalculatedValues { get; set; }

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
            string _AisId = AisId.Get(context);
            DateTimeOffset _EventDate = EventDate.Get(context);
            bool roundData = RoundData.Get(context);
            if (string.IsNullOrEmpty(_AisId))
            {
                Error.Set(context, "Идентификатор АИС не может быть пустым");
                return false;
            }

            try
            {
                XMLATSExportSingleObjectResultCompressed Res = ARM_Service.XMLExportGetAIS80020(_EventDate, _AisId, DataSourceType, isReadCalculatedValues, roundData,false, false,false, false,1, true);
                if (!string.IsNullOrEmpty(Res.Errors))
                    Error.Set(context, Res.Errors);

                Document.Set(context, Res.XMLStreamCompressed);
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
