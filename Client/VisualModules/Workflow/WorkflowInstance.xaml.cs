using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.Visual.Common;

namespace Proryv.AskueARM2.Client.Visual.Workflow
{
    /// <summary>
    /// Interaction logic for WorkflowInstance.xaml
    /// </summary>
    public partial class WorkflowInstance : UserControl, IDisposable
    {
        public WorkflowInstance()
        {
            InitializeComponent();
            instExecuteType.ItemsSource = Workflow_Activity_Instance.InstanceExecuteTypes;
        }

        public WorkflowCollection_Frame frame;

        Workflow_Activity_Instance inst = null;

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            inst = DataContext as Workflow_Activity_Instance;
            activityType.ItemsSource = frame.types;
            var shed = ARM_Service.SCHED_Get_Scheduler();
            if (shed != null)
            {
                var trigs = shed.Triggers;
                if (trigs != null)
                {
                    foreach (var trig in trigs)
                    {
                        trig.StartDateTime = trig.StartDateTime.ServerToClient();
                        if (trig.FinishDateTime != null)
                            trig.FinishDateTime = trig.FinishDateTime.Value.ServerToClient();
                    }
                }
                triggerForm.Set(trigs, inst.ScheduleTimeTrigger_ID);
                Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(delegate ()
                {
                    userChange = true;
                }));
            }
            else
            {
                frame = null;
                Manager.UI.ShowMessage("Не удалось открыть планировщик процесса!");
            }
        }

        private bool userChange = false;

        public void Dispose()
        {
            activityType.ClearItems();
            instExecuteType.ClearItems();
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(inst.StringName))
            {
                Manager.UI.ShowMessage("Введите имя экземпляра!");
                return;
            }
            if (inst.WorkflowActivity_ID == 0)
            {
                Manager.UI.ShowMessage("Укажите тип процесса!");
                return;
            }
            Report_Scheduler_Time_Triggers_List trig = triggerForm.trig;
            if (inst.InstanceExecuteType == 3)
            {
                if (!triggerForm.Save()) return;
                inst.ScheduleTimeTrigger_ID = trig.ScheduleTimeTrigger_ID;
            }
            else
            {
                trig = null;
                inst.ScheduleTimeTrigger_ID = null;
            }
            if (inst.WorkflowInstance_ID == Guid.Empty) inst.WorkflowInstance_ID = Guid.NewGuid();
            inst.User_ID = Manager.User.User_ID;
            inst.CUS_ID = Manager.CUS_ID;       
            string result = "";
            Manager.UI.RunAsync(delegate()
            {
                result = ARM_Service.WWF_Save_Instance(Manager.UserName, Manager.Password, inst, trig);
            }, delegate()
            {
                if (result == "")
                {
                    Manager.UI.ShowMessage("Экземпляр процесса успешно сохранен!");
                    Manager.UI.CloseGlobalModal(delegate()
                    {
                        frame.RefreshTree();
                    });
                }
                else
                    Manager.UI.ShowMessage(result);
            });
        }

        private void instExecuteType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            triggerForm.IsEnabled = inst.InstanceExecuteType == 3;
        }

        private void activityType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (userChange && activityType.SelectedItem != null)
            {
                inst.StringName = (activityType.SelectedItem as Workflow_Activity_List).StringName;
            }
        }

        private void WorkflowInstance_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (frame == null) Manager.UI.CloseGlobalModal(null);
        }
    }
}
