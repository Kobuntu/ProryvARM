using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("��������� ���������� � �������(��) �������� �����(��). ������ Win32_DiskPartition")]
    [DisplayName("WMI ������ ������� �����")]
    public class WMISYSGetDiskPartition : WMIExecuteActivityBase
    {
        public WMISYSGetDiskPartition()
        {
            this.DisplayName = "WMI ������ ������� �����";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "Win32_DiskPartition";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "cimv2";
            return base.BeginExecute(context, callback, state);
        }
    }
}