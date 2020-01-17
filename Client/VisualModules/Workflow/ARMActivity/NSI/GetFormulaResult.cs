using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Proryv.Workflow.Activity.Settings;
using System.Activities;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Both.VisualCompHelpers;

namespace Proryv.Workflow.Activity.ARM
{
    public class GetFormulaResult : BaseArmActivity<bool>
    {
        public GetFormulaResult()
        {
            this.DisplayName = "Получить значение формулы";
            UnitDigit = EnumUnitDigit.None;
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор формулы")]
        [RequiredArgument]
        public InArgument<string> Formula_UN { get; set; }

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
        [DisplayName("Использовать коэффициенты")]
        [RequiredArgument]
        public bool IsCoeffEnabled { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Энергия или мощность")]
        [TypeConverter(typeof(EnumTypeInformationTypeConverter))]
        public enumTypeInformation isPower { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Период дискретизации")]
        [TypeConverter(typeof(EnumDiscreteTypeConverter))]
        public enumTimeDiscreteType DiscreteType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Разрядность")]
        [TypeConverter(typeof(EnumUnitDigitTypeConverter))]
        public EnumUnitDigit UnitDigit { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Результат")]
        [RequiredArgument]
        public OutArgument<TFormulasResult> FormulaResult { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Сумма значений")]
        public OutArgument<double> SummValues { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Статус суммы")]
        public OutArgument<int> Status { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Статус строка суммы")]
        public OutArgument<string> StatusStr { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }


        protected override bool Execute(CodeActivityContext context)
        {

            var formulaUn = Formula_UN.Get(context);

            if (string.IsNullOrEmpty(formulaUn))
            {
                Error.Set(context, "Не определен идентификатор формулы");
                return false;
            }

            var fId = new TFormulaParam
            {
                FormulaID = formulaUn,
                FormulasTable = enumFormulasTable.Info_Formula_Description,
                IsFormulaHasCorrectDescription = true,
            };

            TFormulasResult result = null;
            try
            {
                var res = ARM_Service.FR_GetFormulasResults(new List<TFormulaParam> { fId }, 
                                                             StartDateTime.Get(context),
                                                             EndDateTime.Get(context),
                                                             DiscreteType,
                                                             DataSourceType,
                                                             1, 
                                                             IsCoeffEnabled,
                                                             false,
                                                             isPower,
                                                             false,
                                                             UnitDigit, null, false, false);


                if (res!=null && res.Result_Values!=null && res.Result_Values.Count > 0)
                {
                    var rv = res.Result_Values[0];
                    if (rv!=null && rv.Result_Values!=null && rv.Result_Values.Count > 0)
                    {
                        result = rv.Result_Values[0];
                        if (result.Val_List != null)
                        {
                            var r = result.Val_List.Accomulate();
                            SummValues.Set(context, r.F_VALUE);
                            Status.Set(context, (int) r.F_FLAG);
                            StatusStr.Set(context, TVALUES_DB.FLAG_to_String(r.F_FLAG, ";"));
                        }
                    }
                }

                if (res.Errors != null)
                    Error.Set(context, res.Errors.ToString());

                FormulaResult.Set(context, result);
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
