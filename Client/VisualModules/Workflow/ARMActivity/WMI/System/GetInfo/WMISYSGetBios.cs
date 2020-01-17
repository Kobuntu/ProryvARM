using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("Получение информации BIOS. Объект Win32_BIOS")]
    [DisplayName("WMI BIOS")]
    public class WMISYSGetBios : WMIExecuteActivityBase
    {
        public WMISYSGetBios()
        {
            this.DisplayName = "WMI BIOS";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "Win32_BIOS";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "cimv2";
            return base.BeginExecute(context, callback, state);
        }
    }
}