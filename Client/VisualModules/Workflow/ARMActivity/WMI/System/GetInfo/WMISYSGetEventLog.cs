using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("Получение событий журнала Windows. Объект Win32_NTLogEvent")]
    [DisplayName("WMI События Windows")]
    public class WMISYSGetEventLog : WMIExecuteActivityBase
    {
        [Description("Файл журнала System, Application и т.д.")]
        [DisplayName("Тип журнала")]
        [DefaultValue("System")]
        public InArgument<string> LogType { get; set; }
        
        public WMISYSGetEventLog()
        {
            this.DisplayName = "WMI События Windows";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "Win32_NTLogEvent";
            var logtype = context.GetValue(this.LogType) ?? "System";
            var conditionStrings = context.GetValue(base.WhereCondition);
            if (conditionStrings == null)
                conditionStrings = "LogFile='" + logtype + "'";
            else
                conditionStrings = "LogFile='" + logtype + "' and " + conditionStrings;

            base.Where = conditionStrings;
            base.Service = "cimv2";
            return base.BeginExecute(context, callback, state);
        }
    }
}