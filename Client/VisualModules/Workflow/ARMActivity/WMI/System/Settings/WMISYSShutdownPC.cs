using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management;

namespace Proryv.Workflow.Activity.ARM.WMI.System.Settings
{
    [Description("���������� ������ ����������. ������ Win32_OperatingSystem")]
    [DisplayName("WMI ���������� ������")]
    public class WMISYSShutdownPC : WMIExecuteActivityBase
    {
        public enum ShutdownValues
        {
            Logoff = 0,
            ForcedLogoff = 4,
            Shutdown = 1,
            ForcedShutdown = 5,
            Reboot = 2,
            ForcedReboot = 6,
            Poweroff = 8,
            ForcedPoweroff = 12
        }

        [Description("0:���������� ������; 4:�������������� ���������� ������; 1:���������� ������; " +
                     "5:�������������� ���������� ������; 2:������������; 6:�������������� ������������; " +
                     "8:����������; 12:�������������� ����������;")]
        [DisplayName("���� ���������� ������")]
        [RequiredArgumentAttribute]
        public InArgument<int> Flag { get; set; }

        private int _flag;

        public WMISYSShutdownPC()
        {
            this.DisplayName = "WMI ���������� ������";
            this.Flag = 1;
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "Win32_OperatingSystem";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "cimv2";
            this._flag = Flag.Get(context);

            return base.BeginExecute(context, callback, state);
        }

        protected override List<Dictionary<string, object>> ExecuteWMI(string target, string whereCondition, ManagementScope wmScope)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            try
            {
                wmScope = CreateScope(wmScope);

                ObjectQuery wmQuery = new ObjectQuery("SELECT * FROM " + target);
                ManagementObjectSearcher wmResult = new ManagementObjectSearcher(wmScope, wmQuery);
                foreach (ManagementObject res in wmResult.Get())
                {
                    var inParams = res.GetMethodParameters("Win32Shutdown");
                    inParams["Flags"] = _flag;
                    var outParams = res.InvokeMethod("Win32Shutdown", inParams, null);
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