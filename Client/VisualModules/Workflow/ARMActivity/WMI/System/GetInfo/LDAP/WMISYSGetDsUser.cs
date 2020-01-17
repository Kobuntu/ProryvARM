using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.LDAP
{
    [Description("��������� ������������� Active Directory (AD). ������ ds_user")]
    [DisplayName("WMI ������������(AD)")]
    public class WMISYSGetDsUser : WMIExecuteActivityBase
    {
        public WMISYSGetDsUser()
        {
            this.DisplayName = "WMI ������������(AD)";
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