using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("Получение информации об установленном ПО на компьютере.")]
    [DisplayName("WMI Установленные программы")]
    public class WMISYSGetInstalledSoftwares : WMIExecuteActivityBase
    {
        private const UInt32 HKLM = 0x80000002;

        private const string BaseKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\";

        public WMISYSGetInstalledSoftwares()
        {
            this.DisplayName = "WMI Установленные программы";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = " ";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "Default";

            return base.BeginExecute(context, callback, state);
        }

        protected override List<Dictionary<string, object>> ExecuteWMI(string target, string whereCondition, ManagementScope wmScope)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            try
            {
                wmScope = CreateScope(wmScope);

                ManagementClass classInstance = new ManagementClass(wmScope, new ManagementPath("StdRegProv"), null);

                var methodArgs = new object[]
                                     {
                                         HKLM,
                                         BaseKey,
                                         null
                                     };
                classInstance.InvokeMethod("EnumKey", methodArgs);
                if (methodArgs[2] == null)
                    return result;

                var subkeys = Convert.ToString(methodArgs[2]);

                ManagementBaseObject inParams = classInstance.GetMethodParameters("GetStringValue");
                inParams["hDefKey"] = HKLM;

                foreach (var childLoopVariable in subkeys)
                {
                    var child = childLoopVariable;
                    inParams["sSubKeyName"] = BaseKey + child;
                    inParams["sValueName"] = "DisplayName";

                    ManagementBaseObject outParams = classInstance.InvokeMethod("GetStringValue", inParams, null);
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