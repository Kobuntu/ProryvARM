using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management;
using Microsoft.VisualBasic.Devices;

namespace Proryv.Workflow.Activity.ARM.WMI.System.Settings.RemoteDesktop
{
    [Description("Настройка службы удаленного рабочего стола. Объект Win32_TerminalServiceSetting")]
    [DisplayName("WMI Настройка RDP")]
    public class WMISYSSetTerminalService : WMIExecuteActivityBase
    {
        [Description("Запрет или разрешение на подключение к компьютеру по RDP")]
        [DisplayName("Разрешить подключение к компьютеру")]
        [DefaultValue(true)]
        public InArgument<bool> UseRemoteDesktop { get; set; }

        [DisplayName("Добавить провило в Firewall")]
        [DefaultValue(true)]
        public InArgument<bool> SetFirewallRule { get; set; }

        [DisplayName("ОС Windows Vista или позднее")]
        [DefaultValue(true)]
        public InArgument<bool> IsVistaLater { get; set; }

        private bool _useRdp;
        private bool _setFw;

        private bool _laterVista = true;

        public WMISYSSetTerminalService()
        {
            this.DisplayName = "WMI Настройка RDP";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "Win32_TerminalServiceSetting";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "cimv2";
            _useRdp = context.GetValue(this.UseRemoteDesktop);
            _setFw = context.GetValue(this.SetFirewallRule);
            _laterVista = context.GetValue(this.IsVistaLater);

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
                    var inParams = res.GetMethodParameters("SetAllowTSConnections");
                    if (_useRdp)
                    {
                        inParams["AllowTSConnections"] = 1;
                    }
                    else
                    {
                        inParams["AllowTSConnections"] = 0;
                    }
                    if (_setFw)
                    {
                        inParams["ModifyFirewallException"] = 1;
                    }
                    else
                    {
                        inParams["ModifyFirewallException"] = 0;
                    }

                    var outParams = res.InvokeMethod("SetAllowTSConnections", inParams, null);
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
            catch(Exception ex)
            {
                Console.WriteLine("Exception :" + ex.Message);
                result = null;
                if (HideExc == false)
                    throw;
            }
            return result;
        }

        protected override ManagementScope CreateScope(ManagementScope wmScope)
        {
            Computer myComputer = new Computer();
            if (wmScope == null)
            {
                wmScope = _laterVista ? new ManagementScope("\\\\" + myComputer.Name + "\\root\\" + Service + "\\TerminalServices") : new ManagementScope("\\\\" + myComputer.Name + "\\root\\" + Service);
            }
            if (!wmScope.IsConnected)
                wmScope.Connect();
            return wmScope;
        }
    }
}