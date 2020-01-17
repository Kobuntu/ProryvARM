using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("Получение процесса(ов) в ОС. Объект Win32_Process")]
    [DisplayName("WMI Процесс")]
    public class WMISYSGetProcess : WMIExecuteActivityBase
    {
        public WMISYSGetProcess()
        {
            this.DisplayName = "WMI Процесс";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "Win32_Process";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "cimv2";
            return base.BeginExecute(context, callback, state);
        }
    }
}