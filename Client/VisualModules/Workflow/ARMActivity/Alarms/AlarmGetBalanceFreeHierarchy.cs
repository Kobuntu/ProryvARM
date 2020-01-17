using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.Workflow.Activity.Settings;

namespace Proryv.Workflow.Activity.ARM.Alarms
{
    public class AlarmGetBalanceFreeHierarchy: BaseAlarmActivity
    {
        public AlarmGetBalanceFreeHierarchy()
        {
            this.DisplayName = "Список универсальных балансов для процесса";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Список универсальных балансов")]
        public OutArgument<List<Alarms_Balance_FreeHierarchy_To_Activity>> BalancePsList { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            List<Alarms_Balance_FreeHierarchy_To_Activity> res = null;
            try
            {
                res = ARM_Service.ALARM_Get_Balance_FreeHierarchy_ByActivityID(WorkflowActivity_ID.Get(context));
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            BalancePsList.Set(context, res);
            return string.IsNullOrEmpty(Error.Get(context));
        }

    }
}
