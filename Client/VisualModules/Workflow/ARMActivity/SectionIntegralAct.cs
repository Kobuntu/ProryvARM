using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.Workflow.Activity.Settings;
using System.ComponentModel;
using System.Activities;

namespace Proryv.Workflow.Activity.ARM
{
    public class SectionIntegralAct : BaseArmActivity<bool>
    {
        public SectionIntegralAct()
        {
            this.DisplayName = "Сформировать интегральный акт по сечению";
            TimeZone = 0;
            BusRelation = enumBusRelation.PPI_OurSide;
            IsReadCalculatedValues = true;
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
        [DisplayName("Период дискретизации")]
        [TypeConverter(typeof(EnumDiscreteTypeConverter))]
        public enumTimeDiscreteType DiscreteType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Источник")]
        public EnumDataSourceType? DataSourceType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Энергия или мощность")]
        [TypeConverter(typeof(EnumTypeInformationTypeConverter))]
        public enumTypeInformation IsPower { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Тип учета")]
        [TypeConverter(typeof(enumBusRelationTypeConverter))]
        public enumBusRelation BusRelation { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Отображать реактивные каналы")]
        public bool IsEnabledRChannel { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Выделить объекты потребителей (ЭПУ)")]
        public bool IsAllocateCA { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Дорасчет интегральных данных к часовому поясу")]
        public bool IsOffsetFromMoscowEnbledForDrums { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Использовать фильтр по ПС")]
        public bool IsFilterPSEnabled { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Расчетные значения")]
        public bool IsReadCalculatedValues { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Фрмировать акт из закрытого периода")]
        public bool IsClosedPeriod { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Документ")]
        public OutArgument<MemoryStream> Document { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }


        protected override bool Execute(CodeActivityContext context)
        {

            SectionIntegralComplexResults Res;
            try
            {
                //TODO часовой пояс
                Res = ARM_Service.SIA_GetSectionIntegralActExcelDocumentWithATSCode(
                                                             SectionID.Get(context),
                                                             StartDateTime.Get(context),
                                                             DataSourceType,
                                                             DiscreteType,
                                                             IsPower,
                                                             IsEnabledRChannel,
                                                             BusRelation,
                                                             IsAllocateCA,
                                                             IsOffsetFromMoscowEnbledForDrums,
                                                             IsFilterPSEnabled,
                                                             IsReadCalculatedValues,
                                                             IsClosedPeriod, null);

                if (Res.Document != null)
                {
                    Res.Document.Position = 0;
                    MemoryStream ms = CompressUtility.DecompressGZip(Res.Document);
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
