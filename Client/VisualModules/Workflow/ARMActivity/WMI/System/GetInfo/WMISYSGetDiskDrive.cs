using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("��������� ���������� � ������� �����(��). ������ Win32_DiskDrive")]
    [DisplayName("WMI ������� ����")]
    public class WMISYSGetDiskDrive : WMIExecuteActivityBase
    {
        public WMISYSGetDiskDrive()
        {
            this.DisplayName = "WMI ������� ����";
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