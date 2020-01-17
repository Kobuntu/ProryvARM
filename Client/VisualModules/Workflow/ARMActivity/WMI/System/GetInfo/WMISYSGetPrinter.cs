using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("Получение свойств принтера(ов), установленных в ОС. Объект Win32_Printer")]
    [DisplayName("WMI Принтер")]
    public class WMISYSGetPrinter : WMIExecuteActivityBase
    {
        public WMISYSGetPrinter()
        {
            this.DisplayName = "WMI Принтер";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "Win32_Printer";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "cimv2";
            return base.BeginExecute(context, callback, state);
        }
    }
}