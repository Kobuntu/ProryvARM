using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Proryv.AskueARM2.Client.Visual.Common;
using System.Activities.Presentation;
using System.Activities.Presentation.Toolbox;
using System.Activities.Statements;
using System.Activities.Core.Presentation;
using System.Activities.Presentation.View;
using System.Activities;
using System.IO;
using System.Activities.XamlIntegration;
using Proryv.AskueARM2.Client.Visual.Workflow.Controls;
using Proryv.AskueARM2.Client.Visual.Workflow.UI.EpressionEditor;
using Proryv.Workflow.Activity.ARM.Reports;
using System.Xaml;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.ServiceModel.Activities;
using System.Activities.Presentation.Services;
using System.Collections;
using Proryv.Workflow.Activity.Common.Net;
using Microsoft.Data.Activities;
using System.Activities.Validation;
using System.Activities.Debugger;
using System.Activities.Presentation.Debug;
using System.Activities.Tracking;
using System.Windows.Threading;
using System.Threading;
using Proryv.Workflow.Activity.Common.Primitives;
using Proryv.Workflow.Activity.ARM;
using System.Activities.Presentation.Validation;
using System.Activities.Presentation.Model;
using Proryv.Workflow.Activity.Common.Sql;
using Proryv.Workflow.Activity.Common.Script;
using Proryv.Workflow.Activity.Settings;
using Microsoft.VisualBasic.Activities;
using System.Reflection;
using System.Windows.Input;
using System.Collections.Concurrent;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using Proryv.Workflow.Activity.ARM.Alarms;
using Proryv.Workflow.Activity.ARM.Balance;
using Proryv.Workflow.Activity.ARM.NSI;

namespace Proryv.AskueARM2.Client.Visual.Workflow
{
    /// <summary>
    /// Interaction logic for Workflow_Frame.xaml
    /// </summary>
    public partial class Workflow_Frame : UserControl, IModule, IDisposable
    {
        public class ValidationErrorService : IValidationErrorService
        {
            public void ShowValidationErrors(System.Collections.Generic.IList<ValidationErrorInfo> errors)
            {
                _errorCount = errors.Count;
            }

            public int ErrorCount
            {
                get { return _errorCount; }
            }

            private int _errorCount;
        }

        public Workflow_Frame()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            errors.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(errors_MouseDoubleClick);
        }

        public Workflow_Frame(int type)
            : this()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            InitializeActivityToolBoxTree(type);
        }

        void InitializeActivityToolBoxTree(int WorkFlowType)
        {
            addActivity(controlFlow, typeof(DoWhile), "Выполнять ..., пока ...");
            //            addActivity(controlFlow, typeof(ForEach<>), "Для каждого");
            addActivity(controlFlow, typeof(System.Activities.Core.Presentation.Factories.ForEachWithBodyFactory<>), "Для каждого");
            addActivity(controlFlow, typeof(If), "Если ...");
            addActivity(controlFlow, typeof(Parallel), "Параллельно");
            //            addActivity(controlFlow, typeof(ParallelForEach<>), "Параллельно для каждого");
            addActivity(controlFlow, typeof(System.Activities.Core.Presentation.Factories.ParallelForEachWithBodyFactory<>), "Параллельно для каждого");
            addActivity(controlFlow, typeof(Pick), "Выбрать");
            addActivity(controlFlow, typeof(PickBranch), "Выбрать бранч");
            addActivity(controlFlow, typeof(Sequence), "Последовательность");
            addActivity(controlFlow, typeof(Switch<>), "Переключить");
            addActivity(controlFlow, typeof(While), "Пока ..., выполнять ...");

            addActivity(flowChart, typeof(Flowchart), "Блок-схема");
            addActivity(flowChart, typeof(FlowDecision), "Решение в потоке");
            addActivity(flowChart, typeof(FlowSwitch<>), "Переключить в потоке");

            addActivity(messaging, typeof(CorrelationScope), "Область корреляции");
            addActivity(messaging, typeof(InitializeCorrelation), "Инициализация корреляции");
            addActivity(messaging, typeof(Receive), "Получить");
            addActivity(messaging, typeof(Send), "Отправить");
            addActivity(messaging, typeof(ReceiveReply), "Получить ответ");
            addActivity(messaging, typeof(SendReply), "Отправить ответ");
            addActivity(messaging, typeof(TransactedReceiveScope), "Область получения в транзакции");

            addActivity(runTime, typeof(Persist), "Сохранить состояние в БД");
            addActivity(runTime, typeof(TerminateWorkflow), "Завершение рабочего процесса");

            addActivity(primitives, typeof(Assign), "Присвоить");
            addActivity(primitives, typeof(Delay), "Задержка");
            addActivity(primitives, typeof(InvokeMethod), "Выполнить метод");
            addActivity(primitives, typeof(WriteLineActivity), "Сообщение");
            addActivity(primitives, typeof(CommentOut), "Закомментировать активность");


            addActivity(transactions, typeof(CancellationScope), "Область отмены");
            addActivity(transactions, typeof(CompensableActivity), "Компенсируемая активность");
            addActivity(transactions, typeof(Compensate), "Компенсировать");
            addActivity(transactions, typeof(Confirm), "Подтверждать");
            addActivity(transactions, typeof(TransactionScope), "Область транзакции");

            addActivity(collections, typeof(AddToCollection<>), "Добавить в коллекцию");
            addActivity(collections, typeof(ClearCollection<>), "Очистить коллекцию");
            addActivity(collections, typeof(ExistsInCollection<>), "Существует ли в коллекции");
            addActivity(collections, typeof(RemoveFromCollection<>), "Удалить из коллекции");

            addActivity(errorHandlings, typeof(Rethrow), "Заново бросить исключение");
            addActivity(errorHandlings, typeof(Throw), "Бросить исключение");
            addActivity(errorHandlings, typeof(TryCatch), "Выполнить защищенно");

            addActivity(networks, typeof(SendEmailActivity), "Отправить почту");
            addActivity(networks, typeof(PingActivity), "Ping");

            addActivity(sql, typeof(ExecuteSqlNonQuery), "Выполнить SQL без выборки");
            addActivity(sql, typeof(ExecuteSqlQuery), "Выполнить SQL с выборкой");
            addActivity(sql, typeof(ExecuteSqlQuery<>), "Выполнить SQL с типизированной выборкой");
            //addActivity(sql, typeof(DbQuery<>), "2 Выполнить SQL с типизированной выборкой");
            //addActivity(sql, typeof(DbQueryDataSet), "Выполнить SQL (dataset)");
            addActivity(sql, typeof(DBOpenSqlQuery), "Выполнить SQL-запрос");
            addActivity(sql, typeof(DBQueryByWcf), "Выполнить Sql запрос к текущей БД");
            addActivity(sql, typeof(SQLQueryToTable), "Выполнить Sql запрос к текущей БД и получить таблицу");

            #region WMI
            Type t;

            addActivity(wmi,
                        t = typeof(Proryv.Workflow.Activity.ARM.WMI.WMICreateScopeActivity),
                        ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                            DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetAntiSpywareProduct),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetAntiVirusProduct),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetBios),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetComputerSystem),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetComputerSystemProduct),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetDesktop),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetDesktopMonitor),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetDiskDrive),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetDiskPartition),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetEventLog),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetFirewallProduct),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetInstalledSoftwares),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetLogicalDisk),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetNetworkAdapterConfiguration),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetNetworkLoginProfile),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetOperatingSystem),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetPhysicalMemory),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetPnPEntry),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetPrinter),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetProcess),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetService),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.WMISYSGetUserAccount),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.LDAP.WMISYSGetDsComputer),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.LDAP.WMISYSGetDsPrinter),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.GetInfo.LDAP.WMISYSGetDsUser),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.Settings.WMISYSShutdownPC),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.Settings.RemoteDesktop.WMISYSSetTerminalService),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);
            addActivity(wmi,
                 t = typeof(Proryv.Workflow.Activity.ARM.WMI.System.Settings.Service.WMISYSSetServiceStatus),
                 ((DisplayNameAttribute)Attribute.GetCustomAttribute(t, typeof(DisplayNameAttribute))).
                     DisplayName);

            #endregion
            addActivity(arm_reports, typeof(SendReportToEmail), "Сформировать отчет и отправить на почту");
            addActivity(arm_reports, typeof(SendBusinessObjectsReportToEmail), "Сформировать универсальный отчет и отправить на почту");
            addActivity(arm_reports, typeof(SendReportObjectClassToEmail), "Сформировать отчеты для класса объекта и отправить на почту");
            addActivity(arm_reports, typeof(SendReportToFtp), "Сформировать отчет и отправить на FTP-сервер");
            addActivity(arm_reports, typeof(SendReportObjectClassToFtp), "Сформировать отчеты для класса объекта и отправить на FTP-сервер");
            addActivity(arm_reports, typeof(WriteReportToArchive), "Сформировать отчет и записать в архив");
            addActivity(arm_reports, typeof(ArmNativeActConsumptionPowerTiToEmail), "Сформировать отчет 'Акт потребления мощности для ТИ' и отправить на почту");
            addActivity(arm_reports, typeof(ArmNativeActConsumptionPowerToEmail), "Сформировать отчет 'Акт потребления мощности для объекта' и отправить на почту");
            addActivity(arm_reports, typeof(SectionIntegralAct), "Сформировать интегральный акт по сечению");
            addActivity(arm_reports, typeof(SectionPeretokAct), "Сформировать акт перетоков по сечению");
            addActivity(arm_reports, typeof(PredefinedReportDoc), "Сформировать системный отчет");
            addActivity(arm_reports, typeof(SendReportToFolder), "Сформировать отчет и сохранить на диск");
            addActivity(arm_reports, typeof(SendBusinessObjectsReportToFolder), "Сформировать универсальный отчет и сохранить на диск");


            addActivity(arm_balans, typeof(GenerateBalancePSExcel), "Сгенерировать балансы ПС в ZIP архиве");
            addActivity(arm_balans, typeof(GetBalanceList), "Перечень балансов для объекта");
            addActivity(arm_balans, typeof(GetBalansPSResultAndHtmlDocument), "Получить баланс ПС в HTML");
            addActivity(arm_balans, typeof(GetPSBalanceExcelDocument), "Получить баланс ПС в Excel");
            addActivity(arm_balans, typeof(GetFreeHierarchyBalanceExcelDocument), "Получить универсальный баланс в Excel");
            addActivity(arm_balans, typeof(GetPSBalanceValidationList), "Получить достоверности балансов ПС");
            addActivity(arm_balans, typeof(GetBalanceFreeHierarchyByObjectList), "Получить универсальные балансы по списку объектов");
            addActivity(arm_balans, typeof(GetBalanceFreeHierarchyList), "Получить универсальные балансы");
            addActivity(arm_balans, typeof(GetBalanceValidation), "Получить достоверность баланса");

            addActivity(arm_common, typeof(Login), "Авторизация");
            addActivity(arm_common, typeof(GetUserIDByLogin), "Получить идентификатор пользователя");
            addActivity(arm_common, typeof(SendEmailToUser), "Отправить электронную почту для пользователя");
            addActivity(arm_common, typeof(ScriptCodeActivity), "Выполнить скрипт");
            addActivity(arm_common, typeof(GetObjectName), "Получить имя объекта");
            addActivity(arm_common, typeof(DebugMessageToLog), "Записать сообщение в лог");
            addActivity(arm_common, typeof(InsertToExplUserJournal), "Записать сообщение в журнал действий пользователей");
            addActivity(arm_common, typeof(FtpListDirectoryDetails), "FTP - Получить список файлов");
            addActivity(arm_common, typeof(FtpUploadFile), "FTP - Выгрузить файл");
            addActivity(arm_common, typeof(FtpDownloadFile), "FTP - Скачать файл");
            addActivity(arm_common, typeof(FtpDeleteFile), "FTP - Удалить файл");
            

            addActivity(arm_common, typeof(ManageDeviceParameters), "Управление параметрами приборов");
            addActivity(arm_common, typeof(DirectCommandUspd), "Управление параметрами УСПД");
            addActivity(arm_common, typeof(ManageManualReadRequest), "Ручной опрос ПУ");
            addActivity(arm_common, typeof(UspdManualReadRequest), "Ручной опрос УСПД");
            addActivity(arm_common, typeof(GetPsStatisticInfo), "Получить статистику для ПС");

            addActivity(arm_nsi, typeof(NSIGetTIListForPS), "Получить детальную информацию по всем ТИ подстанции");
            addActivity(arm_nsi, typeof(GetArchiveValues), "Получить архивные значения для объекта");
            addActivity(arm_nsi, typeof(GetArchiveIntegralValue), "Получить интегральное значение ТИ");
            addActivity(arm_nsi, typeof(GetArchiveIntegralValueEx), "Получить интегральное значение ТИ(Расширенное)");
            addActivity(arm_nsi, typeof(Get_PS_List_Id), "Получить список ПС для уровня иерархии");
            addActivity(arm_nsi, typeof(GetArchTechQualityValueListTI), "Получить мгновенные значения для списка ТИ");
            addActivity(arm_nsi, typeof(GetArchTechQualityValueTI), "Получить мгновенные значения для ТИ");
            addActivity(arm_nsi, typeof(GetAllTIJournals), "Получить журналы для ТИ");
            addActivity(arm_nsi, typeof(GetListFormuls), "Получить список формул");
            addActivity(arm_nsi, typeof(GetFormulaResult), "Получить значение формулы");
            addActivity(arm_nsi, typeof(GetFormulaParams), "Получить параметры формулы");
            addActivity(arm_nsi, typeof(GetUSPDList), "Получить список УСПД");
            addActivity(arm_nsi, typeof(GetUSPDMetersList), "Получить список ПУ для УСПД");
            addActivity(arm_nsi, typeof(GetUSPDTIList), "Получить список ТИ для УСПД");
            addActivity(arm_nsi, typeof(GetE422List), "Получить список E422");
            addActivity(arm_nsi, typeof(GetE422MetersList), "Получить список ПУ для E422");
            addActivity(arm_nsi, typeof(GetE422TIList), "Получить список ТИ для E422");
            addActivity(arm_nsi, typeof(AnalysisClosedData), "Анализ закрытых данных");

            addActivity(arm_xml_80020, typeof(XMLExportGetAIS80020), "XML документ 80020 для АИС");
            addActivity(arm_xml_80020, typeof(XMLExportGetAISList80020), "XML документ 80020 для списка АИС");
            addActivity(arm_xml_80020, typeof(XMLExportGetPS80020), "XML документ 80020 для ПС");
            addActivity(arm_xml_80020, typeof(XMLExportGetPSList80020), "XML документ 80020 для списка ПС");
            addActivity(arm_xml_80020, typeof(XMLExportGetSection80020), "XML документ 80020 для Секции");
            addActivity(arm_xml_80020, typeof(XMLExportGetSection80020ForDatePeriod), "XML документ 80020 для секции за период времени");
            addActivity(arm_xml_80020, typeof(XMLExportGetSectionList80020), "XML документ 80020 для списка секций");
            
            addActivity(arm_xml_80020, typeof(XMLSAPExchange), "XML импорт из SAP - замены счетчиков");


            if (WorkFlowType == 11 || WorkFlowType == 12)
            {
                addActivity(arm_alarms, typeof(RegisterAlarmEntityTypes), "Зарегистрировать сущности для тревоги");
                addActivity(arm_alarms, typeof(GetAlarmSettings), "Получить настройки тревоги");
                addActivity(arm_alarms, typeof(AlarmGetUsers), "Список пользователей для процесса");
                addActivity(arm_alarms, typeof(AlarmGetTI), "Список ТИ для процесса");
                addActivity(arm_alarms, typeof(AlarmGetFormuls), "Список формул для процесса");
                addActivity(arm_alarms, typeof(AlarmGetBalancePS), "Список Балансов ПС для процесса");
                addActivity(arm_alarms, typeof(AlarmGetBalanceFreeHierarchy), "Список универсальных балансов для процесса");
                addActivity(arm_alarms, typeof(AlarmGetMaster61968SlaveSystems), "Список подчиненных систем 61968");
                addActivity(arm_alarms, typeof(WriteAlarm_TI), "Записать аварию для ТИ");
                addActivity(arm_alarms, typeof(WriteAlarm_BalancePS), "Записать аварию для баланса ПС");
                addActivity(arm_alarms, typeof(WriteAlarm_PS), "Записать аварию для ПС");
                addActivity(arm_alarms, typeof(WriteAlarm_BalanceFreeHierarchy), "Записать аварию для универсального баланса");
                addActivity(arm_alarms, typeof(WriteAlarm_Formuls), "Записать аварию для формулы");
                addActivity(arm_alarms, typeof(WriteAlarm_Users), "Записать аварию для пользователя");
                addActivity(arm_alarms, typeof(WriteAlarm_61968SlaveSystems), "Записать аварию для подчиненных систем 61968");
            }
            else
            {
                activity_tool_box.Categories.Remove(arm_alarms);
            }
        }

        void addActivity(ToolboxCategory category, Type type, string displayname)
        {
            category.Add(new ToolboxItemWrapper(type, displayname));
            activityNames[type.Name] = displayname;
        }

        readonly Dictionary<string, string> activityNames = new Dictionary<string, string>();
        WorkflowDesigner wd;
        WorkflowApplication CurrentInstance;
        byte CurrentActivityType;
        int CurrentWorkflowActivity_ID;
        ModelItem CurrentSelectedActivity;
        bool WfIsRunning = false;
        //        bool StopProcess = false;

        public string GetModuleToolTip
        {
            get { return String.Empty; }
        }


        public ModuleType ModuleType
        {
            get { return Common.ModuleType.Workflow; }
        }

        public bool Init()
        {
            return true;
        }

        public CloseAction Close()
        {
            return CloseAction.HideToHistory;
        }

        public string Shortcut { get; set; }

        public FreeHierarchyTreeItem GetNode(IFreeHierarchyObject hierarchyObject)
        {
            return null;
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

        /// <summary>
        /// Коллекция объектов для которых необходимо в конце отписаться (для высвобождения памяти)
        /// </summary>
        ConcurrentStack<EditingContext> contextForUnsubscribeCollection = new ConcurrentStack<EditingContext>();

        void configureDesigner()
        {
            var hashTable = new Hashtable();
            foreach (var key in Resources.Keys)
                hashTable.Add(key.ToString(), Resources[key]);
            var saved = XamlServices.Save(hashTable);
            wd.PropertyInspectorFontAndColorData = saved;
            var designerView = wd.Context.Services.GetService<DesignerView>();
            if (designerView != null)
            designerView.WorkflowShellBarItemVisibility =
                ShellBarItemVisibility.Arguments |
                ShellBarItemVisibility.Imports |
                ShellBarItemVisibility.MiniMap |
                ShellBarItemVisibility.Variables |
                ShellBarItemVisibility.Zoom;
            workflowDesignerPanel.Content = wd.View;
            WorkflowPropertyPanel.Content = wd.PropertyInspectorView;
            mserv = wd.Context.Services.GetService<ModelService>();
            if (mserv != null)
                mserv.ModelChanged += new EventHandler<ModelChangedEventArgs>(mserv_ModelChanged);
            DebuggerService = wd.DebugManagerView;

            EditorService expEditor = new EditorService();
            wd.Context.Services.Publish(expEditor as IExpressionEditorService);

            try
            {
                (new DesignerMetadata()).Register();
            }
            catch { }

            wd.Context.Items.Subscribe<Selection>(SelectionChanged);

            contextForUnsubscribeCollection.Push(wd.Context);

            if (CurrentActivityType == 11 || CurrentActivityType == 12)
            {
                // добавляем входной аргумент для WorkflowActivity_ID
                bool Found = false;
                ModelItemCollection PropCol = wd.Context.Services.GetService<ModelService>().Root.Properties["Properties"].Collection;
                foreach (var p in PropCol)
                    if (p.Properties["Name"].Value.ToString() == ActivitiesSettings.InParamNameWorkFlowId)
                    {
                        p.Properties["Value"].ComputedValue = new InArgument<int>(new VisualBasicValue<int>(CurrentWorkflowActivity_ID.ToString()));
                        Found = true; break;
                    }

                if (!Found)
                {
                    DynamicActivityProperty property = new DynamicActivityProperty()
                    {
                        Name = ActivitiesSettings.InParamNameWorkFlowId,
                        Type = typeof(InArgument<int>),
                        Value = new InArgument<int>(new VisualBasicValue<int>(CurrentWorkflowActivity_ID.ToString())),
                    };
                    PropCol.Add(property);
                }


            }
            //----------------------------------------------------
            
            //ValidationErrorService ValidErrorService = new ValidationErrorService();
            //wd.Context.Services.Publish(typeof(IValidationErrorService), ValidErrorService);
//#if DEBUG
            TypeBrowserBorder.Child = new TypeBrowserControl(wd);
//#endif
        }


        private void SelectionChanged(Selection selection)
        {
            if (!WfIsRunning)
            CurrentSelectedActivity = selection.PrimarySelection;
        }

        ModelService mserv;

        private void MenuItem_Click_LoadWorkflow(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog() { DefaultExt = ".xaml", Filter = "*.xaml|*.xaml" };
            if (openFileDialog.ShowDialog().Value)
            {
                workflowDesignerPanel.Content = null;
                WorkflowPropertyPanel.Content = null;
                wd = new WorkflowDesigner();
                wd.Load(openFileDialog.FileName);
                configureDesigner();
                FileInfo fi = new FileInfo(openFileDialog.FileName);
                string s = fi.Name;
                if (s.LastIndexOf(".") > 0)
                    s = s.Substring(0, s.LastIndexOf("."));
                activity.StringName = s;
            }
        }

        private void MenuItem_Click_Save(object sender, RoutedEventArgs e)
        {
            if (wd != null)
            {
                wd.Flush();
                //if (validate())
                {
                    //var saveFileDialog = new Microsoft.Win32.SaveFileDialog() { DefaultExt = ".xaml", Filter = "*.xaml|*.xaml" };
                    //saveFileDialog.FileName = Proryv.AskueARM2.Client.Visual.FileAdapter.removeBadChar(activity_name.Text);
                    //if (saveFileDialog.ShowDialog().Value)
                    //wd.Save(saveFileDialog.FileName);
                    string fileName = ExportToExcel.prepareFile("xaml", activity_name.Text);
                    if (!String.IsNullOrEmpty(fileName))
                    {
                        wd.Save(fileName);
                    }
                }
            }
            else
            {
                Manager.UI.ShowMessage("Не удалось сохранить процесс!");
            }
        }

        Activity GetActivity()
        {
            wd.Flush();
            var stringReader = new StringReader(wd.Text);
            var root = ActivityXamlServices.Load(stringReader);//as Activity;
            return root;
        }


        private void MenuItem_Click_RunWorkflow(object sender, RoutedEventArgs e)
        {
            if (wd != null)
            {
                if (!validate()) return;
                Manager.UI.RunUILocked(delegate()
                {
                    logTool.Visibility = System.Windows.Visibility.Visible;
                    logTool.Activate(true);
                    string temp = getTempFile();
                    wd.Save(temp);
                    wd = new WorkflowDesigner();
                    wd.Load(temp);
                    File.Delete(temp);
                    configureDesigner();
                    Activity activity = GetActivity();
                    /*
                var wfApp = new WorkflowApplication(activity);
                var visualTracking = new VisualTracking(wd);
                wfApp.Extensions.Add(visualTracking);
                wfApp.Run();
                 */

                    bool UseTracking = useTracking.IsChecked.Value;
                    log.Text = "";
                    StartProcess();
                    AppendLine("Старт процесса");

                    //WorkflowInvoker instance = new WorkflowInvoker(activity);                
                    if (CurrentInstance != null)
                    {
                        CurrentInstance.Aborted -= OnAbortedWorkflowApplication;
                        CurrentInstance.Completed -= OnCompletedWorkflowApplication;
                    }

                    var instance = new WorkflowApplication(activity);
                    CurrentInstance = instance;

                    var simTrackerCollectionForDispose = new List<VisualTrackingParticipant>();

                    //Mapping between the Object and Line No.
                    Dictionary<object, SourceLocation> wfElementToSourceLocationMap =
                        UpdateSourceLocationMappingInDebuggerService();
                    //Mapping between the Object and the Instance Id
                    Dictionary<string, Activity> activityIdToWfElementMap =
                        BuildActivityIdToWfElementMap(wfElementToSourceLocationMap);
                    //Set up Custom Tracking
                    const String all = "*";

                    var simTracker = new VisualTrackingParticipant()
                    {
                        TrackingProfile = new TrackingProfile()
                        {
                            Name = "CustomTrackingProfile",
                            Queries =
                            {
                                new CustomTrackingQuery()
                                {
                                    Name = all,
                                    ActivityName = all
                                },
                                new WorkflowInstanceQuery()
                                {
                                    // Limit workflow instance tracking records for started and completed workflow states
                                    States = {WorkflowInstanceStates.Started, WorkflowInstanceStates.Completed},
                                },
                                new ActivityStateQuery()
                                {
                                    // Subscribe for track records from all activities for all states
                                    ActivityName = all,
                                    States = {all},

                                    // Extract workflow variables and arguments as a part of the activity tracking record
                                    // VariableName = "*" allows for extraction of all variables in the scope
                                    // of the activity
                                    Variables =
                                    {
                                        {all}
                                    }
                                }
                            }
                        }
                    };

                    simTrackerCollectionForDispose.Add(simTracker);

                    //----------

                    simTracker.ActivityIdToWorkflowElementMap = activityIdToWfElementMap;

                    //As the tracking events are received
                    simTracker.TrackingRecordReceived += (trackingParticpant, trackingEventArgs) =>
                    {
                        if (UseTracking)
                            if (trackingEventArgs.Activity != null)
                            {

                                ShowDebug(wfElementToSourceLocationMap[trackingEventArgs.Activity]);

                                AppendLine(trackingEventArgs.Activity.DisplayName + " : " +
                                           ((ActivityStateRecord) trackingEventArgs.Record).State, true);

                                //this.Dispatcher.Invoke(DispatcherPriority.SystemIdle, (Action) (() =>
                                //{
                                //    System.Threading.Thread.Sleep(100);
                                //}));
                            }

                        WriteLineTrackingRecord WriteLineTrackingRec =
                            trackingEventArgs.Record as WriteLineTrackingRecord;
                        if (WriteLineTrackingRec != null)
                            AppendLine(">" + WriteLineTrackingRec.WriteLineMessage);

                    };

                    instance.Extensions.Add(simTracker);

                    instance.Completed += OnCompletedWorkflowApplication;
                    instance.Aborted += OnAbortedWorkflowApplication;

                    ThreadPool.QueueUserWorkItem(new WaitCallback((context) =>
                    {

                        finished.Reset();
                        try
                        {
                            instance.Run();
                        }
                        catch (Exception ex)
                        {
                            AppendLine("Ошибка времени выполнения : " + ex.Message);
                            wd.DebugManagerView.CurrentLocation = null;
                        }
                        finished.WaitOne();
                        wd.DebugManagerView.CurrentLocation = null;
                        SetSelectedItem(simTrackerCollectionForDispose);
                    }));



                    /*
                ThreadPool.QueueUserWorkItem(new WaitCallback((context) =>
                {
                    try
                    {
                        instance.Invoke();

                        if (UseTracking)
                            this.Dispatcher.Invoke(DispatcherPriority.Render
                            , (Action)(() =>
                            {
                                wd.DebugManagerView.CurrentLocation = new SourceLocation("", 1, 1, 1, 10);
                            }));
                    }
                    catch (Exception ex)
                    {
                        AppendLine("Ошибка времени выполнения : " + ex.Message);
                        wd.DebugManagerView.CurrentLocation = null;
                    }

                    AppendLine("Конец выполнения процесса");

                }));
                */
                });
            }
            else
            {
                Manager.UI.ShowMessage("Не удалось запустить процесс!");
            }
        }

        ManualResetEvent finished = new ManualResetEvent(false);

        void OnCompletedWorkflowApplication(WorkflowApplicationCompletedEventArgs completedArgs)
        {
            if (completedArgs.TerminationException != null)
                AppendLine("Ошибка времени выполнения : " + completedArgs.TerminationException.Message);
            else
                AppendLine("Конец выполнения процесса");
            finished.Set();
            StopProcess();
        }

        void OnAbortedWorkflowApplication(WorkflowApplicationAbortedEventArgs abortedArgs)
        {
            if (wd != null)
            {
                wd.DebugManagerView.CurrentLocation = null;
                finished.Set();
                AppendLine("Принудительная остановка процесса");
                StopProcess();
            }
        }

        private void SetSelectedItem(List<VisualTrackingParticipant> simTrackerCollectionForDispose)
        {
            if (CurrentSelectedActivity != null) // восстанавливаем выделенный Activity
            {
                this.Dispatcher.Invoke(DispatcherPriority.SystemIdle, (Action)(() =>
                {
                    var ms = wd.Context.Services.GetService<ModelService>();
                    if (ms != null)
                    {
                        IEnumerable<ModelItem> activityCollection = ms.Find(ms.Root, typeof(Activity));
                        foreach (var i in activityCollection)
                        {
                            if (i.ToString() == CurrentSelectedActivity.ToString())
                            {
                                Selection.Select(wd.Context, i);
                                break;
                            }
                        }
                    }
                    if (simTrackerCollectionForDispose != null)
                    {
                        simTrackerCollectionForDispose.DisposeChildren();
                        simTrackerCollectionForDispose.Clear();
                    }
                }));
            }
        }


        private void StartProcess()
        {
            butStop.IsEnabled = true;
            useTracking.IsEnabled = false;
            WfIsRunning = true;
        }

        private void StopProcess()
        {
            WfIsRunning = false;
            this.Dispatcher.Invoke(DispatcherPriority.SystemIdle, (Action)(() =>
            {
                butStop.IsEnabled = false;
                useTracking.IsEnabled = true;
            }));
        }

        #region For Custom Tracking

        public IDesignerDebugView DebuggerService { get; set; }

        //===========================================================================================

        void ShowDebug(SourceLocation srcLoc)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Render
                , (Action)(() =>
                {
                    wd.DebugManagerView.CurrentLocation = srcLoc;

                }));

        }

        private Dictionary<string, Activity> BuildActivityIdToWfElementMap(Dictionary<object, SourceLocation> wfElementToSourceLocationMap)
        {
            Dictionary<string, Activity> map = new Dictionary<string, Activity>();
            
            Activity wfElement;
            foreach (object instance in wfElementToSourceLocationMap.Keys)
            {
                wfElement = instance as Activity;
                if (wfElement != null)
                {
                    map.Add(wfElement.Id, wfElement);
                }
            }

            return map;
        }

        Activity GetRootWorkflowElement(object rootModelObject)
        {
            
            System.Diagnostics.Debug.Assert(rootModelObject != null, "Cannot pass null as rootModelObject");

            Activity rootWorkflowElement;
            IDebuggableWorkflowTree debuggableWorkflowTree = rootModelObject as IDebuggableWorkflowTree;
            if (debuggableWorkflowTree != null)
            {
                rootWorkflowElement = debuggableWorkflowTree.GetWorkflowRoot();
            }
            else // Loose xaml case.
            {
                rootWorkflowElement = rootModelObject as Activity;
            }
            return rootWorkflowElement;
            
        }


        Dictionary<object, SourceLocation> UpdateSourceLocationMappingInDebuggerService()
        {
            object rootInstance = GetRootInstance();
            Dictionary<object, SourceLocation> sourceLocationMapping = new Dictionary<object, SourceLocation>();
            Dictionary<object, SourceLocation> designerSourceLocationMapping = new Dictionary<object, SourceLocation>();

            if (rootInstance != null)
            {

                Activity documentRootElement = GetRootWorkflowElement(rootInstance);
                Activity activity = GetActivity();
                WorkflowInspectionServices.CacheMetadata(activity);

                IEnumerator<Activity> enumerator1 = WorkflowInspectionServices.GetActivities(activity).GetEnumerator();
                //Получаем первый елемент в процессе
                enumerator1.MoveNext();
                activity = enumerator1.Current;
                // возможны случаи когда первым идет входной аргумент, анализируем и переходим к следующему...
                bool f = (activity.GetType().IsGenericType && activity.GetType().GetGenericTypeDefinition() == typeof(VisualBasicValue<>)) ; 
                while (f)
                {
                    enumerator1.MoveNext();
                    activity = enumerator1.Current;
                    f = (activity.GetType().IsGenericType && activity.GetType().GetGenericTypeDefinition() == typeof(VisualBasicValue<>));
                }

                
                SourceLocationProvider.CollectMapping(activity, documentRootElement, sourceLocationMapping,
                    wd.Context.Items.GetValue<WorkflowFileItem>().LoadedFile);
                SourceLocationProvider.CollectMapping(documentRootElement, documentRootElement, designerSourceLocationMapping,
                   wd.Context.Items.GetValue<WorkflowFileItem>().LoadedFile);

            }

            if (this.DebuggerService != null)
            {
                ((DebuggerService)this.DebuggerService).UpdateSourceLocations(designerSourceLocationMapping);
            }

            return sourceLocationMapping;
        }


        object GetRootInstance()
        {
            ModelService modelService = wd.Context.Services.GetService<ModelService>();
            if (modelService != null)
            {
                return modelService.Root.GetCurrentValue();
            }
            else
            {
                return null;
            }
        }

        Activity GetRootRuntimeWorkflowElement()
        {
            Activity root = GetActivity();
            WorkflowInspectionServices.CacheMetadata(root);

            IEnumerator<Activity> enumerator1 = WorkflowInspectionServices.GetActivities(root).GetEnumerator();
            //Get the first child of the x:class
            enumerator1.MoveNext();
            root = enumerator1.Current;
            return root;
        }

        //===========================================================================================
        #endregion


        Workflow_Activity_List activity = null;

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool rebind = activity != null;
            activity = DataContext as Workflow_Activity_List;
            if (rebind) return;
            Manager.UI.RunUILocked(delegate()
            {
                if (activity == null) return;

                if (activity.WorkflowActivity_ID == 0)
                {
                    wd = new WorkflowDesigner();
                    var ab = new ActivityBuilder() { Name = "Процесс" };

                    switch (activity.RootActivityType)
                    {
                        case 1:
                        case 11:
                            ab.Implementation = new Sequence() { DisplayName = activityNames["Sequence"] };
                            break;
                        case 2:
                        case 12:
                            ab.Implementation = new Flowchart() { DisplayName = activityNames["Flowchart"] };
                            break;
                    }
                    
                    // пример как можно добавить Refernces                    
                    VisualBasic.SetSettingsForImplementation(ab, new VisualBasicSettings()
                    {
                        ImportReferences =
                        {
                         new VisualBasicImportReference
                         {                             
                              Assembly = "System.Runtime.Serialization",
                              Import = "System.Runtime.Serialization",
                         },
                        }
                    }); 


                    wd.Load(ab);
                    string temp = getTempFile();
                    wd.Save(temp);
                    wd = new WorkflowDesigner();
                    wd.Load(temp);
                    File.Delete(temp);
                }
                else
                {
                    if (activity.Activity == null)
                    {
                        try
                        {
                            DataContext = ARM_Service.WWF_Load_Process(activity.WorkflowActivity_ID);
                        }
                        catch (Exception ex)
                        {

                            Manager.UI.ShowMessage(ex.Message);
                        }
                    }
                        
                    if (activity == null)
                    {
                        Manager.UI.ShowMessage("Не удалось загрузить процесс!");
                        return;
                    }
                    else
                    {
                        wd = new WorkflowDesigner();
                        var temp = getTempFile();
                        var ms = new MemoryStream(activity.Activity.Bytes);
                        try
                        {
                            var sr = new StreamReader(ms, Encoding.UTF8);
                            File.WriteAllText(temp, sr.ReadToEnd());
                            wd.Load(temp);
                        }
                        finally
                        {
                            try
                            {
                                ms.Dispose();
                                File.Delete(temp);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                CurrentActivityType = activity.RootActivityType;
                CurrentWorkflowActivity_ID = activity.WorkflowActivity_ID;
                configureDesigner();
            });
        }

        string getTempFile()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + Guid.NewGuid().ToString() + ".xaml";
        }

        void mserv_ModelChanged(object sender, ModelChangedEventArgs e)
        {
            if (e.ItemsAdded != null && e.ItemsAdded.Count() > 0)
            {
                foreach (var a in e.ItemsAdded)
                {
                    var act = a.GetCurrentValue();
                    var fs = act as FlowStep;
                    if (fs != null) act = fs.Action;
                    string typeName = act.GetType().Name;
                    if (activityNames.ContainsKey(typeName))
                    {
                        if (act is Activity)
                        {
                            (act as Activity).DisplayName = activityNames[typeName];
                        }
                        else
                        {
                            if (act is FlowNode)// а вот потомки FlowNode (к примеру FlowDecision) не имеют displayname - возможно 
                            {
                                //(act as FlowDecision).DisplayName = activityNames[typeName];
                            }
                        
                        }
                    }
                }
            }
            //(mserv.Root.View as ActivityDesigner).Style = Application.Current.FindResource("sdRes") as Style;
        }


        void errors_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Activities.Validation.ValidationError Error = errors.CurrentItem as System.Activities.Validation.ValidationError;
            if (Error == null) return;
            if (Error.Source == null) return;

            var ms = wd.Context.Services.GetService<ModelService>();

            IEnumerable<ModelItem> activityCollection = ms.Find(ms.Root, typeof(Activity));
            foreach (var i in activityCollection)
            {
                if (i.GetCurrentValue() == Error.Source)
                {
                    Selection.Select(wd.Context, i);
                    break;
                }
            }
        }

        public Activity ActivityGetParent(Activity activity)
        {
            PropertyInfo pinfo = typeof(Activity).GetProperty("Parent", BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic);
            if (pinfo != null)
                return pinfo.GetValue(activity, null) as Activity;
            else return null;
        }

        bool validate()
        {
            var rootInstance = GetRootInstance();
            Activity RootElement = GetRootWorkflowElement(rootInstance);
            if (RootElement == null)
                RootElement = GetActivity();            

            var validateResult = ActivityValidationServices.Validate(GetActivity());
            if (validateResult.Errors.Count > 0)
                validateResult = ActivityValidationServices.Validate(RootElement);


            if (validateResult.Errors.Count == 0 && validateResult.Warnings.Count == 0)
            {
                errors.ItemsSource = null;
                warnings.ItemsSource = null;
            }
            else
            {
                // если ошибка связана с VB выражением, то пытаемся получить владельца этого выражения, и
                // в сообщение об ошибке запихиваем его
                foreach (System.Activities.Validation.ValidationError err in validateResult.Errors)
                {
                    bool f = (err.Source != null && err.Source.GetType().IsGenericType && err.Source.GetType().GetGenericTypeDefinition() == typeof(VisualBasicValue<>));
                    if (f)
                    {
                        Activity a = ActivityGetParent(err.Source); // Activity.Parent - protected, получаем через RTTI
                        if (a != null)
                        {
                            PropertyInfo pinfo = err.GetType().GetProperty("Source"); // свойство Source типа ReadOnly, поэтому устанавливаем через RTTI
                            if (pinfo != null)
                                pinfo.SetValue(err, a, null);
                        }
                    }

                }

                errors.ItemsSource = validateResult.Errors;
                warnings.ItemsSource = validateResult.Warnings;
                if (validateResult.Errors.Count > 0)
                {
                    Manager.UI.ShowMessage("Обнаружены ошибки - процесс не может быть запущен или сохранен!");
                    return false;
                }
            }
            return true;
        }

        public WorkflowCollection_Frame List = null;

        private void butSave_Click(object sender, RoutedEventArgs e)
        {
            wd.Flush();
            if (validate())
            {
                if (activity.StringName.Trim() == "")
                {
                    Manager.UI.ShowMessage("Введите имя процесса!");
                    return;
                }
                var ms = new MemoryStream();
                var sw = new StreamWriter(ms, Encoding.UTF8);
                wd.Flush();
                sw.Write(wd.Text);
                sw.Flush();
                activity.Activity = new Binary() { Bytes = ms.ToArray() };
                sw.Close();
                ms.Close();
                ms.Dispose();
                sw.Dispose();
                activity.User_ID = Manager.User.User_ID;
                activity.DispatchDateTime = DateTime.Now.DateTimeToWCFDateTime().ClientToServer();
                string result = "";
                Manager.UI.RunAsync(delegate()
                {
                    result = ARM_Service.WWF_Save_Process(Manager.UserName, Manager.Password, activity);
                }, delegate()
                {
                    int id;
                    if (int.TryParse(result, out id))
                    {
                        activity.WorkflowActivity_ID = id;
                        Manager.UI.ShowMessage("Процесс успешно сохранен!");
                        List.RefreshTree();
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(result))
                            Manager.UI.ShowMessage("Не удалось сохранить процесс!");
                        else Manager.UI.ShowMessage(result);
                    }                    
                });
            }
        }

        public void AppendLine(string message, bool withTimeout = false)
        {
            this.Dispatcher.Invoke(DispatcherPriority.SystemIdle, (Action)(() =>
            {
                log.AppendText(message + "\n");
                logScroll.ScrollToBottom();
                if (withTimeout)
                {
                    System.Threading.Thread.Sleep(50);
                }
            }));
        }

        private void butStop_Click(object sender, RoutedEventArgs e)
        {
            //            AppendLine("Принудительная остановка процесса");
            //            StopProcess = true;
            if (CurrentInstance != null)
            {
                CurrentInstance.Abort("Принудительная остановка процесса");
                useTracking.IsEnabled = true;
                butStop.IsEnabled = false;
            }
        }

        // лечение проблемы с курсором
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            cursor();
        }

        void cursor()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(delegate()
            {
                Mouse.OverrideCursor = Cursors.Cross;
                Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(delegate()
                {
                    Mouse.OverrideCursor = null;
                }));
            }));
        }

        private void ToolWindowContainer_MouseEnter(object sender, MouseEventArgs e)
        {
            cursor();
        }

        public void Dispose()
        {
            var typeBrowser = TypeBrowserBorder.Child as TypeBrowserControl;
            if (typeBrowser != null)
            {
                typeBrowser.Dispose();
                TypeBrowserBorder.Child = null;
            }
            //Сначала надо остановить текущий процесс
            if (CurrentInstance != null)
            {
                CurrentInstance.Abort("Принудительная остановка процесса, закрытие модуля");
                CurrentInstance.Aborted -= OnAbortedWorkflowApplication;
                CurrentInstance.Completed -= OnCompletedWorkflowApplication;
                CurrentInstance = null;

            }

            while (!contextForUnsubscribeCollection.IsEmpty)
            {
                EditingContext c;
                if (contextForUnsubscribeCollection.TryPop(out c) && c!=null)
                {
                    var mserv = c.Services.GetService<ModelService>();
                    if (mserv != null)
                    {
                        mserv.ModelChanged -= mserv_ModelChanged;
                        mserv = null;
                    }

                    if (c.Items != null)
                    {
                        c.Items.Unsubscribe<Selection>(SelectionChanged);
                    }

                    //Надо диспосить объект, но в какой момент не понятно
                    //c.Dispose();
                }
            }

            errors.MouseDoubleClick -= errors_MouseDoubleClick;
            errors.ClearItems();
            warnings.ClearItems();

            workflowDesignerPanel.Content = null;
            WorkflowPropertyPanel.Content = null;

            activity = null;
            DataContext = null;
            
        }
    }
}
