using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.LDAP
{
    [Description("Получение пользователей Active Directory (AD). Объект ds_user")]
    [DisplayName("WMI Пользователь(AD)")]
    public class WMISYSGetDsUser : WMIExecuteActivityBase
    {
        public WMISYSGetDsUser()
        {
            this.DisplayName = "WMI Пользователь(AD)";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "ds_user";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "directory\\ldap";
            return base.BeginExecute(context, callback, state);
        }
    }
}