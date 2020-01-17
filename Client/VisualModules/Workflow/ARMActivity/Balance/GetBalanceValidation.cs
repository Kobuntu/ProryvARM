using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.Workflow.Activity.Settings;
using System.ComponentModel;
using System.Activities;
using System.IO;

namespace Proryv.Workflow.Activity.ARM
{
    public class GetBalanceValidation : BaseArmActivity<bool>
    {

        public GetBalanceValidation()
        {
            this.DisplayName = "Получить достоверность баланса";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор баланса")]
        [RequiredArgument]
        public InArgument<string> BalancePS_UN { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Начальная дата")]
        public InArgument<DateTime> StartDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Конечная дата")]
        public InArgument<DateTime> EndDateTime { get; set; }

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
        [DisplayName("Учитывать малые точки")]
        public bool STIEnabled { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Достоверность")]
        public OutArgument<TBalansPSValidateResult> BalansPSValidateResult { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }


        protected override bool Execute(CodeActivityContext context)
        {

            var balanceUn = BalancePS_UN.Get(context);
            if (string.IsNullOrEmpty(balanceUn))
            {
                Error.Set(context, "Не определен Идентификатор баланса");
                return false;
            }

            //Balance_Description_PS BDesc = ARM_Service.BL_Get_BalancePS_Descriptions(BalanceUN);
            //if (BDesc == null)
            //{
            //    Error.Set(context, "Не определен баланс c таким идентификатором " + BalanceUN);
            //    return false;
            //}

            //if (BDesc.Balance_Info_2==null || !BDesc.Balance_Info_2.PS_ID.HasValue)
            //{
            //    Error.Set(context, "Не определена связь баланса c ПС");
            //    return false;
            //}

            TBalansPSValidateResult result = null;

            //var listPs = new List<int> {(int) BDesc.Balance_Info_2.PS_ID};

            try
            {
                result = ARM_Service.BPS_GetPSBalanceValidation(balanceUn,
                    StartDateTime.Get(context),
                    EndDateTime.Get(context),
                    DataSourceType,
                    isPower, null, true);
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            BalansPSValidateResult.Set(context, result);
            return string.IsNullOrEmpty(Error.Get(context));
        }

    }

}

