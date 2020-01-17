using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("Получение информации о жестком диске(ах). Объект Win32_DiskDrive")]
    [DisplayName("WMI Жесткий диск")]
    public class WMISYSGetDiskDrive : WMIExecuteActivityBase
    {
        public WMISYSGetDiskDrive()
        {
            this.DisplayName = "WMI Жесткий диск";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "Win32_DiskDrive";
            base.Where = WhereCondition.Get(context);
            base.Service = "cimv2";
            return base.BeginExecute(context, callback, state);
        }
    }
}