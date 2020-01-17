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
    public class XMLExportGetAISList80020 : BaseArmActivity<bool>
    {
        public XMLExportGetAISList80020()
        {
            this.DisplayName = "XML документ 80020 для списка АИС";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Список идентификаторов АИС")]
        [RequiredArgument]
        public InArgument<List<string>> ListAisId { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Начальная дата")]
        public InArgument<DateTime> StartDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Конечная дата")]
        public InArgument<DateTime> EndDateTime { get; set; }

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
            List<string> _ListAisId = ListAisId.Get(context);
            bool roundData = RoundData.Get(context);
            if (_ListAisId == null || _ListAisId.Count == 0)
            {
                Error.Set(context, "Список идентификаторов АИС не может быть пустым");
                return false;
            }

            try
            {
                XMLATSExportSingleObjectResultCompressed Res = ARM_Service.XMLExportGetAIS80020Ext(StartDateTime.Get(context), EndDateTime.Get(context), _ListAisId, DataSourceType, isReadCalculatedValues, roundData, false,false, false,1, true);
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
