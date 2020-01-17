using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("Получение системы запуска ОС Windows на компьютере. Объект Win32_ComputerSystem")]
    [DisplayName("WMI Система запуска ОС")]
    public class WMISYSGetComputerSystem : WMIExecuteActivityBase
    {
        public WMISYSGetComputerSystem()
        {
            this.DisplayName = "WMI Система запуска ОС";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "Win32_ComputerSystem";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "cimv2";
            return base.BeginExecute(context, callback, state);
        }
    }
}