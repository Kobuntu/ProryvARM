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
    public class AlarmGetUsers : BaseAlarmActivity
    {
        public AlarmGetUsers()
        {
            this.DisplayName = "Список пользователей для процесса";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Список пользователей")]
        public OutArgument<List<Alarms_User_To_Activity>> UserList { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            List<Alarms_User_To_Activity> res = null;
            try
            {
                res = ARM_Service.ALARM_Get_Users_ByActivityID(WorkflowActivity_ID.Get(context));
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            UserList.Set(context, res);
            return string.IsNullOrEmpty(Error.Get(context));
        }

    }
}
