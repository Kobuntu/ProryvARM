using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.Workflow.Activity.Settings;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.IO;

namespace Proryv.Workflow.Activity.ARM
{
    public class XMLExportGetPS80020 : BaseArmActivity<bool>
    {
        public XMLExportGetPS80020()
        {
            this.DisplayName = "XML документ 80020 для ПС";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор ПС")]
        [RequiredArgument]
        public InArgument<Int32> PsId { get; set; }

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

        //[Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        //[DisplayName("Имя файла документа")]
        //public OutArgument<string> DocumentFileName { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            int ps_id = PsId.Get(context);
            DateTime _EventDate = EventDate.Get(context);
            bool roundData = RoundData.Get(context);
            try
            {
                XMLATSExportSingleObjectResult Res = ARM_Service.XMLExportGetPS80020(_EventDate, ps_id, DataSourceType, requiredATSCode, isReadCalculatedValues, roundData,false, false, UTCTimeShiftFromMoscow, false, false,1, true);
                Document.Set(context, Res.XMLStream);
                //DocumentFileName.Set(context, Res.FileName);
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
