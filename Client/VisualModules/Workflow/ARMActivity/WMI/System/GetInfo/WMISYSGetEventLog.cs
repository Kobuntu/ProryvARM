using System;
using System.Activities;
using System.ComponentModel;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("��������� ������� ������� Windows. ������ Win32_NTLogEvent")]
    [DisplayName("WMI ������� Windows")]
    public class WMISYSGetEventLog : WMIExecuteActivityBase
    {
        [Description("���� ������� System, Application � �.�.")]
        [DisplayName("��� �������")]
        [DefaultValue("System")]
        public InArgument<string> LogType { get; set; }
        
        public WMISYSGetEventLog()
        {
            this.DisplayName = "WMI ������� Windows";
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