using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("Получение информации о логическом диске(ах). Объект Win32_LogicalDisk")]
    [DisplayName("WMI Логический диск")]
    public class WMISYSGetLogicalDisk : WMIExecuteActivityBase
    {
        public WMISYSGetLogicalDisk()
        {
            this.DisplayName = "WMI Логический диск";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "Win32_LogicalDisk";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "cimv2";
            return base.BeginExecute(context, callback, state);
        }
    }
}