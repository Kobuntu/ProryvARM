using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("Получение используемых программ и железа на компьютере. Объект Win32_ComputerSystemProduct")]
    [DisplayName("WMI Установленное железо и ПО")]
    public class WMISYSGetComputerSystemProduct : WMIExecuteActivityBase
    {
        public WMISYSGetComputerSystemProduct()
        {
            this.DisplayName = "WMI Установленное железо и ПО";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "Win32_ComputerSystemProduct";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "cimv2";
            return base.BeginExecute(context, callback, state);
        }
    }
}