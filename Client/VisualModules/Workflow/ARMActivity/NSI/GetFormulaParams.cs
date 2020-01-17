using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Proryv.Workflow.Activity.Settings;
using System.Activities;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;

namespace Proryv.Workflow.Activity.ARM
{
    public class GetFormulaParams : BaseArmActivity<bool>
    {
        public GetFormulaParams()
        {
            this.DisplayName = "Получить параметры формулы";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор формулы")]
        [RequiredArgument]
        public InArgument<string> Formula_UN { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Параметры")]
        [RequiredArgument]
        public OutArgument<Formula_Description> FormulaDesc { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }


        protected override bool Execute(CodeActivityContext context)
        {

            string FormulaUN = Formula_UN.Get(context);
            if (string.IsNullOrEmpty(FormulaUN))
            {
                Error.Set(context, "Не определен Идентификатор формулы");
                return false;
            }

            try
            {
                Formula_Description Res = ARM_Service.FE_Get_FormulaDescription(FormulaUN);

                if (Res != null)
                    FormulaDesc.Set(context, Res);
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
