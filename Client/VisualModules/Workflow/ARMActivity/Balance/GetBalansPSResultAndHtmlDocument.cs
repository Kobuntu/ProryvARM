using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.Workflow.Activity.Settings;
using System.ComponentModel;
using System.Activities;
using System.IO;
using Proryv.AskueARM2.Both.VisualCompHelpers;

namespace Proryv.Workflow.Activity.ARM
{
    public class GetBalansPSResultAndHtmlDocument : BaseArmActivity<bool>
    {

        public GetBalansPSResultAndHtmlDocument()
        {
            this.DisplayName = "Получить баланс ПС в HTML";
            TimeZone = 4;
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор баланса")]
        [RequiredArgument]
        public InArgument<string> BalanceId { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Начальная дата")]
        public InArgument<DateTime> StartDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Конечная дата")]
        public InArgument<DateTime> EndDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Период дискретизации")]
        [TypeConverter(typeof(EnumDiscreteTypeConverter))]
        public enumTimeDiscreteType DiscreteType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Источник")]
        public EnumDataSourceType? DataSourceType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Профиль")]
        [TypeConverter(typeof(EnumProfileTypeConverter))]
        public enumReturnProfile ProfileType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Энергия или мощность")]
        [TypeConverter(typeof(EnumTypeInformationTypeConverter))]
        public enumTypeInformation isPower { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Часовой пояс отличен от Москвы")]
        public bool isOffsetFromMoscowEnbledForDrums { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Учитывать потери в силовом оборудовании")]
        public bool IsPowerEquipmentEnabled { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Временная зона")]
        [DefaultValue(4)]
        public int TimeZone { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Документ")]
        public OutArgument<MemoryStream> Document { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("ПС в балансе")]
        public OutArgument<bool> IsHasUnbalanceValue { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            try
            {
                var res = ARM_Service.BPS_GetBalansPSResultAndHtmlDocument2(BalanceId.Get(context),
                                                             StartDateTime.Get(context),
                                                             EndDateTime.Get(context),
                                                             DiscreteType,//.Get(context),
                                                             DataSourceType,//.Get(context),
                                                             isPower,//.Get(context),
                                                             isOffsetFromMoscowEnbledForDrums,//.Get(context),
                                                             IsPowerEquipmentEnabled, 7, null, false);

                if (res.Errors != null)
                    Error.Set(context, res.Errors.ToString());

                var ms = CompressUtility.DecompressGZip(res.HTMLDocument);

                if (ms != null)
                {
                    ms.Position = 0;
                    Document.Set(context, ms);
                }

                IsHasUnbalanceValue.Set(context, !res.IsHasUnbalanceValue);
            }

            catch (Exception ex)
            {
                if (!HideException.Get(context))
                    throw ex;
            }

            return true;
        }

    }
}
