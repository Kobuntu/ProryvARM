using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Xceed.Wpf.DataGrid;
using System.Windows.Threading;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using Xceed.Wpf.DataGrid.Views;

namespace Proryv.AskueARM2.Client.Visual.Workflow
{
    /// <summary>
    /// Interaction logic for WorkFlowCollection_Frame.xaml
    /// </summary>
    public partial class WorkflowCollection_Frame : UserControl, IModule, IDisposable, IFormingExportedFileName, ICustomHelp
    {
        public WorkflowCollection_Frame()
        {
            InitializeComponent();

            wwfTypes.ItemsSource = TypesSource;
            wwfInstances.ItemsSource = InstancesSource;

            var tableView = wwfTypes.View as TableView;
            if (tableView != null) tableView.FixedColumnCount = 3;
            var view = wwfInstances.View as TableView;
            if (view != null) view.FixedColumnCount = 3;
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(delegate()
            {
                butDelType.VisiblePosition = butDelInst.VisiblePosition = 0;
                butEditType.VisiblePosition = butEditInst.VisiblePosition = 1;
                butLinks.VisiblePosition = butLog.VisiblePosition = 2;
            }));
            alarmSettings.Visibility = Visibility.Collapsed;
        }

        RangeObservableCollection<Workflow_Activity_List> TypesSource = new RangeObservableCollection<Workflow_Activity_List>();
        RangeObservableCollection<Workflow_Activity_Instance> InstancesSource = new RangeObservableCollection<Workflow_Activity_Instance>();

        public bool isAlarms = false;

        void setIsAlarms()
        {
            alarmSettings.Visibility = Visibility.Visible;
            butLinks.Visible = true;
            (wwfTypes.View as TableView).FixedColumnCount = 3;
        }

        public WorkflowCollection_Frame(bool isAlarms) : this()
        {
            this.isAlarms = isAlarms;
            if (isAlarms) setIsAlarms();
        }

        public string GetModuleToolTip
        {
            get { return String.Empty; }
        }


        public ModuleType ModuleType
        {
            get { return ModuleType.WorkflowCollection; }
        }

        public bool Init()
        {
#if !DEBUG
            if (!Manager.NSI_EditRights.IsAdmin)
            {
                Manager.UI.ShowMessage("Данный модуль доступен только администратору!");
                return false;
            }
#endif
            RefreshTree();
            return true;
        }

        public FreeHierarchyTreeItem GetNode(IFreeHierarchyObject hierarchyObject)
        {
            return null;
        }

        internal List<Workflow_Activity_List> types = null;

        public void RefreshTree()
        {
            TypesSource.Clear();
            InstancesSource.Clear();

            List<Workflow_Activity_Instance> instances = null;
            Manager.UI.RunAsync(delegate()
            {
                try
                {
                    var processes = ARM_Service.WWF_Get_Processes(isAlarms);
                    types = processes.Key;
                    instances = processes.Value;
                }
                catch (Exception)
                {
                    Manager.UI.ShowMessage("Не удалось получить список процессов!");
                }
            }, delegate()
            {
                if (types != null)
                {
                    TypesSource.AddRange(types);
                }

                if (instances != null)
                {
                    InstancesSource.AddRange(instances);
                }
            });
        }

        public CloseAction Close()
        {
            return CloseAction.HideToHistory;
        }

        public string Shortcut
        {
            get
            {
                return "IsAlarms=" + isAlarms.ToString();
            }
            set
            {
                var dict = CommonEx.ParseDictionary(value);
                if (Convert.ToBoolean(dict["IsAlarms"]))
                {
                    isAlarms = true;
                    setIsAlarms();
                }
            }
        }

        /// <summary>
        /// Что запускаем после восстановления из истории
        /// </summary>
        public string ActionAfterRestore { get; set; }

        /// <summary>
        /// Параметры конструктора сжатые Protobuf
        /// </summary>
        public List<TTypedParam> ConstructorParams { get; set; }

        /// <summary>
        /// Путь к иконке модуля
        /// </summary>
        public string IconPath { get; set; }

        private void butDel_Click(object sender, RoutedEventArgs e)
        {
            var obj = (sender as Button).FindParent<DataCell>().DataContext;
            var activity = obj as Workflow_Activity_List;
            if (activity != null)
            {
                Manager.UI.ShowYesNoDialog("Удалить тип процесса и его экземпляры?", () =>
                {
                    string res = ARM_Service.WWF_Delete_Process(Manager.UserName, Manager.Password, activity.WorkflowActivity_ID);
                    if (res == "") RefreshTree();
                    else
                    {
                        if (res != null) Manager.UI.ShowMessage(res);
                        else Manager.UI.ShowMessage("Не удалось удалить процесс!");
                    }
                });
            }
            var instance = obj as Workflow_Activity_Instance;
            if (instance != null)
            {
                Manager.UI.ShowYesNoDialog("Удалить экземпляр процесса?", () =>
                {
                    string res = ARM_Service.WWF_Delete_Instance(Manager.UserName, Manager.Password, instance.WorkflowInstance_ID);
                    if (res == "") RefreshTree();
                    else
                    {
                        if (res != null) Manager.UI.ShowMessage(res);
                        else Manager.UI.ShowMessage("Не удалось удалить экземпляр процесса!");
                    }
                });
            }
        }

        private void butEdit_Click(object sender, RoutedEventArgs e)
        {
            var obj = (sender as Button).FindParent<DataCell>().DataContext;
            var activity = obj as Workflow_Activity_List;
            if (activity != null)
            {
                try
                {
                    Manager.UI.AddTab("Редактирование процесса", new Workflow_Frame(activity.RootActivityType)
                    {
                        List = this
                    }, activity);
                }
                catch (Exception ex)
                {
                    Manager.UI.ShowMessage(ex.Message);
                }
            }
            var instance = obj as Workflow_Activity_Instance;
            if (instance != null)
            {
                var module = new WorkflowInstance() { frame = this, DataContext = instance };
                Manager.UI.ShowGlobalModal(module, "Редактирование экземпляра процесса");
            }
        }

        private void butActivityType_Add_Click(object sender, RoutedEventArgs e)
        {
            Manager.UI.ShowGlobalModal(new ActivityTypeSelector() { List = this }, "Выберите тип процесса");
        }

        private void butActivityInstance_Click(object sender, RoutedEventArgs e)
        {
            Manager.UI.ShowGlobalModal(new WorkflowInstance() { frame = this, DataContext = new Workflow_Activity_Instance() }, "Создание экземпляра процесса");
        }

        private void alarmSettings_Click(object sender, RoutedEventArgs e)
        {
            Manager.UI.ShowGlobalModal(Manager.Modules.CreateModule(Common.ModuleType.AlarmSettings), "Настройки тревог");
        }

        private void butLinks_Click(object sender, RoutedEventArgs e)
        {
            var obj = (sender as Button).FindParent<DataCell>().DataContext;
            var activity = obj as Workflow_Activity_List;
            if (activity != null) 
            {
                var module = Manager.Modules.CreateModule(Common.ModuleType.AlarmViewLinks);
                module.DataContext = activity;
                Manager.UI.ShowLocalModal(module, "Связи с тревогой \"" + activity.StringName + "\"", MainLayout, true, true, true);
                //Manager.UI.ShowGlobalModal(module, "Связи с тревогой \"" + activity.StringName + "\"");
            }
        }

        public void Dispose()
        {
            InstancesSource.Clear();
            TypesSource.Clear();

            wwfTypes.ClearItems();
            wwfInstances.ClearItems();
            types = null;
        }

        private void butLog_Click(object sender, RoutedEventArgs e)
        {
            var obj = (sender as Button).FindParent<DataCell>().DataContext;
            var activity = obj as Workflow_Activity_Instance;
            if (activity != null)
            {
                var module = new JournalWorkflow_Frame();
                module.InitInstances(InstancesSource.ToList(), activity.WorkflowInstance_ID);
                this.ShowLocalModal(module, "Журнал выполнения процессов");                
            }
        }

        public string FormingExportedFileName(string tableName, out bool isExportCollapsedDetail)
        {
            isExportCollapsedDetail = false;
            switch (tableName)
            {
                case "wwfTypes": return "Описатели процессов";
                default: return "Экземпляры процессов";
            }
        }

        public bool ExportExcludeGroupBySettings(string tableName)
        {
            return false;
        }

        public string GetHelpID()
        {
            return isAlarms ? "Alarms" : "WF";
        }

        private void butRefreshWorkflowInstance_Click(object sender, RoutedEventArgs e)
        {
            RefreshTree();
        }
    }
}
