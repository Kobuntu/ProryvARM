using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.LDAP
{
    [Description("Получение очереди печати принтеров Active Directory (AD). Объект ds_printqueue")]
    [DisplayName("WMI принтер(AD)")]
    public class WMISYSGetDsPrinter : WMIExecuteActivityBase
    {
        public WMISYSGetDsPrinter()
        {
            this.DisplayName = "WMI принтер(AD)";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "ds_printqueue";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "directory\\ldap";
            return base.BeginExecute(context, callback, state);
        }
    }
}