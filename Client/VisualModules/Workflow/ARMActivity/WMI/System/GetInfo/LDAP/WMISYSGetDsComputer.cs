using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.LDAP
{
    [Description("��������� ����������� Active Directory (AD). ������ ds_computer")]
    [DisplayName("WMI ���������(AD)")]
    public class WMISYSGetDsComputer : WMIExecuteActivityBase
    {
        public WMISYSGetDsComputer()
        {
            this.DisplayName = "WMI ���������(AD)";
        }
        
        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "ds_computer";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "directory\\ldap";
            return base.BeginExecute(context, callback, state);
        }
    }
}