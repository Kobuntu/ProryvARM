using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using Proryv.Workflow.Activity.Settings;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;

namespace Proryv.Workflow.Activity.ARM
{
    public class AlarmGetMaster61968SlaveSystems : BaseAlarmActivity
    {
        public AlarmGetMaster61968SlaveSystems()
        {
            this.DisplayName = "Список подчиненных систем 61968";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Список 61968")]
        public OutArgument<List<Alarms_Master61968_SlaveSystems_To_Activity>> Master61968List { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            List<Alarms_Master61968_SlaveSystems_To_Activity> res = null;
            try
            {
                res = ARM_Service.ALARM_Get_Master61968_SlaveSystems_ByActivityID(WorkflowActivity_ID.Get(context));
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            Master61968List.Set(context, res);
            return string.IsNullOrEmpty(Error.Get(context));
        }

    }
}

