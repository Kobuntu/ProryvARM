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
    public class XMLExportGetPSList80020 : BaseArmActivity<bool>
    {
        public XMLExportGetPSList80020()
        {
            this.DisplayName = "XML документ 80020 для списка ПС";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Список идентификаторов ПС")]
        [RequiredArgument]
        public InArgument<List<Int32>> ListPsId { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Дата")]
        [RequiredArgument]
        public InArgument<DateTime> EventDate { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Не выгружать без кода АТС")]
        public bool requiredATSCode { get; set; }

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

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Смещение относительно Москвы в часах")]
        [DefaultValue(0)]
        public int UTCTimeShiftFromMoscow { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            List<int> ps_id_List = ListPsId.Get(context);
            DateTime _EventDate = EventDate.Get(context);
            bool roundData = RoundData.Get(context);
            if (ps_id_List == null || ps_id_List.Count == 0)
            {
                Error.Set(context, "Список ПС не может быть пустым");
                return false;
            }

            try
            {
                Stream Res = ARM_Service.XMLExportGetPS80020ForPSArray(_EventDate, ps_id_List, DataSourceType, requiredATSCode, isReadCalculatedValues, roundData, false, UTCTimeShiftFromMoscow,false, false, 1, true).XMLStream;

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
