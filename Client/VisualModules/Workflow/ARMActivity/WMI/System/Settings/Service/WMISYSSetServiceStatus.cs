using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management;

namespace Proryv.Workflow.Activity.ARM.WMI.System.Settings.Service
{
    ///<remarks>
    /// http://msdn.microsoft.com/en-us/library/aa826699(VS.85).aspx
    /// </remarks>
    [Description("Управление службой. Объект Win32_Service")]
    [DisplayName("WMI Статус службы")]
    public class WMISYSSetServiceStatus : WMIExecuteActivityBase
    {
        [Description("1:Старт; 2:Стоп; 3:Пауза; 4:Возобновить;")]
        [DisplayName("Статус службы")]
        [RequiredArgumentAttribute]
        public InArgument<int> Status { get; set; }

        [Description("Установить имя службы")]
        [DisplayName("Имя службы")]
        [RequiredArgumentAttribute]
        public InArgument<string> ServiceName { get; set; }

        private string _methodName = "";

        public WMISYSSetServiceStatus()
        {
            this.DisplayName = "WMI Статус службы";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "Win32_Service";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "cimv2";
            if(base.Where != "")
                base.Where += " and ";
            base.Where += "Name='" + context.GetValue(this.ServiceName) + "' ";

            switch (context.GetValue(this.Status))
            {
                case 1:
                    _methodName = "StartService";
                    //Start
                    break;
                case 2:
                    _methodName = "StopService";
                    //Stop
                    break;
                case 3:
                    _methodName = "PauseService";
                    //Pause
                    break;
                case 4:
                    _methodName = "ResumeService";
                    //Resume
                    break;
            }

            return base.BeginExecute(context, callback, state);
        }

        protected override List<Dictionary<string, object>> ExecuteWMI(string target, string whereCondition, ManagementScope wmScope)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            try
            {
                wmScope = CreateScope(wmScope);

                ObjectQuery wmQuery = new ObjectQuery("SELECT * FROM " + target + " WHERE " + whereCondition);
                ManagementObjectSearcher wmResult = new ManagementObjectSearcher(wmScope, wmQuery);
                foreach (ManagementObject res in wmResult.Get())
                {
                    var outParams = res.InvokeMethod(_methodName, null, null);
                    if (outParams == null) continue;
                    Dictionary<string, object> resRec = new Dictionary<string, object>();
                    foreach (var resColLoopVariable in outParams.Properties)
                    {
                        var resCol = resColLoopVariable;
                        resRec.Add(resCol.Name, resCol.Value == null ? null : resCol.Value.ToString());
                    }
                    result.Add(resRec);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception :" + ex.Message);
                result = null;
                if (HideExc == false)
                    throw;
            }
            return result;
        }
    }
}