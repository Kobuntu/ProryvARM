using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("Получение свойств рабочего стола пользователя. Объект Win32_Desktop")]
    [DisplayName("WMI Рабочий стол")]
    public class WMISYSGetDesktop : WMIExecuteActivityBase
    {
        public WMISYSGetDesktop()
        {
            this.DisplayName = "WMI Рабочий стол";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "Win32_Desktop";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "cimv2";
            return base.BeginExecute(context, callback, state);
        }
    }
}