using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("��������� ��������(��) � ��. ������ Win32_Process")]
    [DisplayName("WMI �������")]
    public class WMISYSGetProcess : WMIExecuteActivityBase
    {
        public WMISYSGetProcess()
        {
            this.DisplayName = "WMI �������";
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