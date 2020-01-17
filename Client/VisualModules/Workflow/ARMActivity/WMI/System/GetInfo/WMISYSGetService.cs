using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("Получение служб(ы) в ОС. Объект Win32_Service")]
    [DisplayName("WMI Служба")]
    public class WMISYSGetService : WMIExecuteActivityBase
    {
        public WMISYSGetService()
        {
            this.DisplayName = "WMI Служба";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "Win32_Service";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "cimv2";
            return base.BeginExecute(context, callback, state);
        }
    }
}