using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("Получение типа монитора или дисплея компьютера. Объект Win32_DesktopMonitor")]
    [DisplayName("WMI Монитор")]
    public class WMISYSGetDesktopMonitor : WMIExecuteActivityBase
    {
        public WMISYSGetDesktopMonitor()
        {
            this.DisplayName = "WMI Монитор";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "Win32_DesktopMonitor";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "cimv2";
            return base.BeginExecute(context, callback, state);
        }
    }
}