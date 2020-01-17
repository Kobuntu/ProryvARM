using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using Proryv.Workflow.Activity.Settings;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.Workflow.Activity.Common.Primitives;

namespace Proryv.Workflow.Activity.ARM
{
    public class DebugMessageToLog : CodeActivity
    {
        public DebugMessageToLog()
        {
            this.DisplayName = "Записать сообщение в лог";
        }

        [Description("Сообщение для записи в лог")]
        [DisplayName("Сообщение")]
        public InArgument<string> Message { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            Guid CurrentWorkflowInstanceId ;
            string Mess = Message.Get(executionContext) ;
            PropertyDescriptorCollection pr = executionContext.DataContext.GetProperties();
            PropertyDescriptor ph = pr.Find(ActivitiesSettings.InParamNameWorkflowInstanceId, true);
            if (ph != null)
            {
                string v = ph.GetValue(executionContext.DataContext).ToString();
                try
                {
                    if (!string.IsNullOrEmpty(Mess) && Mess.Length > 1023)
                        Mess = Mess.Substring(0, 1023);

                    CurrentWorkflowInstanceId = Guid.Parse(v);
                    ARM_Service.WWF_Write_Log_Message(CurrentWorkflowInstanceId, Mess);
                }
                catch
                {
                    return;
                }
            }

            Mess = "Сообщение в лог>> " + Mess;
            if (ActivitiesSettings.runMode == ActivitiesSettings.enumWorkFlowRunMode.UserNotifyServer)
            {
                Console.WriteLine(Mess);
            }
            else
            {
                WriteLineTrackingRecord WLRecord = new WriteLineTrackingRecord();
                WLRecord.WriteLineMessage = Mess;
                executionContext.Track(WLRecord);
            }
        }


    }
}
