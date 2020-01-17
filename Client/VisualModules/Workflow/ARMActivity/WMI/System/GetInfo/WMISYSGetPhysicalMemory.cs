using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("Получение оперативной памяти, доступной на компьютере. Объект Win32_PhysicalMemory")]
    [DisplayName("WMI Оперативная память")]
    public class WMISYSGetPhysicalMemory : WMIExecuteActivityBase
    {
        public WMISYSGetPhysicalMemory()
        {
            this.DisplayName = "WMI Оперативная память";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "Win32_PhysicalMemory";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "cimv2";
            return base.BeginExecute(context, callback, state);
        }
    }
}