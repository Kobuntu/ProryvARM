using System;
using System.Collections.Generic;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.Workflow.Activity.Settings;
using System.ComponentModel;
using System.Activities;

namespace Proryv.Workflow.Activity.ARM
{
    public class GetPSBalanceValidationList : BaseArmActivity<bool>
    {

        public GetPSBalanceValidationList()
        {
            this.DisplayName = "Получить достоверности балансов";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Список ПС")]
        [RequiredArgument]
        public InArgument<List<Int32>> PS_ID_List { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Начальная дата")]
        public InArgument<DateTime> StartDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Конечная дата")]
        public InArgument<DateTime> EndDateTime { get; set; }

        //[Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        //[DisplayName("Период дискретизации")]
        //[TypeConverter(typeof(EnumDiscreteTypeConverter))]
        //public enumTimeDiscreteType DiscreteType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Профиль")]
        [TypeConverter(typeof(EnumProfileTypeConverter))]
        public EnumDataSourceType? DataSourceType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Энергия или мощность")]
        [TypeConverter(typeof(EnumTypeInformationTypeConverter))]
        public enumTypeInformation isPower { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Учитывать малые точки")]
        public bool STIEnabled { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Достоверности")]
        public OutArgument<PSBalanceValidationList> BalanceValidationList { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }


        protected override bool Execute(CodeActivityContext context)
        {

            List<Int32> L = PS_ID_List.Get(context);
            if (L == null || L.Count == 0)
            {
                Error.Set(context, "Не определен список ПС");
                return false;
            }


            try
            {
                var res = ARM_Service.BPS_GetPSBalanceValidationList(PS_ID_List.Get(context),
                    StartDateTime.Get(context),
                    EndDateTime.Get(context),
                    enumTimeDiscreteType.DBInterval,
                    DataSourceType,
                    isPower, null);
                BalanceValidationList.Set(context, res);
                if (res.Errors != null && res.Errors.Length > 0)
                {
                    Error.Set(context, res.Errors.ToString());
                }
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
