using System;
using System.Activities;
using System.ComponentModel;
using System.Management;
using Microsoft.VisualBasic.Devices;

namespace Proryv.Workflow.Activity.ARM.WMI.System.GetInfo
{
    [Description("Получение используемой программы антишпион. Объект AntiSpywareProduct")]
    [DisplayName("WMI ПО Антишпион")]
    public class WMISYSGetAntiSpywareProduct : WMIExecuteActivityBase
    {
        [DisplayName("ОС Windows Vista или позднее")]
        [DefaultValue(true)]
        public InArgument<bool> IsVistaLater { get; set; }

        public WMISYSGetAntiSpywareProduct()
        {
            this.DisplayName = "WMI ПО Антишпион";
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback,
                                                     object state)
        {
            base.Target = "AntiSpywareProduct";
            base.Where = base.WhereCondition.Get(context);
            base.Service = "SecurityCenter";
            var lateVista = context.GetValue(this.IsVistaLater);
            if (lateVista)
                base.Service += "2";

            return base.BeginExecute(context, callback, state);
        }

        protected override ManagementScope CreateScope(ManagementScope wmScope)
        {
            if (wmScope == null)
            {
                Computer myComputer = new Computer();
                wmScope = new ManagementScope("\\\\" + myComputer.Name + "\\root\\" + Service);
            }
            if (!wmScope.IsConnected)
                wmScope.Connect();
            return wmScope;
        }
    }
}