using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.Workflow.Activity.Settings;

namespace Proryv.Workflow.Activity.ARM
{
    public class SectionPeretokAct : BaseArmActivity<bool>
    {
        public SectionPeretokAct()
        {
            this.DisplayName = "Сформировать акт перетоков по сечению";
            TimeZone = 0;
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор секции")]
        [RequiredArgument]
        public InArgument<Int32> SectionID { get; set; }

        [RequiredArgument]
        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Дата")]
        public InArgument<DateTime> StartDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Временная зона")]
        [DefaultValue(0)]
        public int TimeZone { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Источник")]
        public EnumDataSourceType? DataSourceType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Тип учета")]
        [TypeConverter(typeof(enumBusRelationTypeConverter))]
        public enumBusRelation BusRelation { get; set; }

        //[Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        //[DisplayName("Округлять получасовки")]
        //public bool RoundData { get; set; }

        //[Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        //[DisplayName("???????????????????????")]
        //public bool IsEmulateHalfHours { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Документ")]
        public OutArgument<MemoryStream> Document { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }


        protected override bool Execute(CodeActivityContext context)
        {

            SectionIntegralResults Res;
            try
            {

                Res = ARM_Service.SIA_GetFlowBalanceReport(
                    SectionID.Get(context),
                    TExportExcelAdapterType.toXLS,
                    StartDateTime.Get(context),
                    DataSourceType,
                    BusRelation,
                    true,
                    false, null, true, true, null);

                if (Res.CompressedDoc != null)
                {
                    Res.CompressedDoc.Position = 0;
                    MemoryStream ms = CompressUtility.DecompressGZip(Res.CompressedDoc);
                    ms.Position = 0;
                    Document.Set(context, ms);
                }

                if (Res.Errors != null)
                    Error.Set(context, Res.Errors.ToString());
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
