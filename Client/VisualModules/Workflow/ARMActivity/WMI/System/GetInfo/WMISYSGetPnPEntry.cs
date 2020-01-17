using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("��������� ������� ���������(�) Plug and Play. ������ Win32_PnPEntity")]
    [DisplayName("WMI ���������� Plug ang Play")]
    public class WMISYSGetPnPEntry : WMIExecuteActivityBase
    {
        public WMISYSGetPnPEntry()
        {
            this.DisplayName = "WMI ���������� Plug ang Play";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "Win32_PnPEntity";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "cimv2";
            return base.BeginExecute(context, callback, state);
        }
    }
}