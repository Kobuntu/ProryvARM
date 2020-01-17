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
    public class AlarmGetPS : BaseAlarmActivity
    {
        public AlarmGetPS()
        {
            this.DisplayName = "Список ПС для процесса";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Список Балансов ПС")]
        public OutArgument<List<Alarms_PS_To_Activity>> PSList { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            List<Alarms_PS_To_Activity> res = null;
            try
            {
                res = ARM_Service.ALARM_Get_PS_ByActivityID(WorkflowActivity_ID.Get(context));
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            PSList.Set(context, res);
            return string.IsNullOrEmpty(Error.Get(context));
        }

    }
}
