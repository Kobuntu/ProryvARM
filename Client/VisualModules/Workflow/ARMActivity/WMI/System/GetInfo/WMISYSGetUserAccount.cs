using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("Получение информации о пользователе(ях) ОС. Объект Win32_UserAccount")]
    [DisplayName("WMI Пользователь")]
    public class WMISYSGetUserAccount : WMIExecuteActivityBase
    {
        public WMISYSGetUserAccount()
        {
            this.DisplayName = "WMI Пользователь";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "Win32_UserAccount";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "cimv2";
            return base.BeginExecute(context, callback, state);
        }
    }
}