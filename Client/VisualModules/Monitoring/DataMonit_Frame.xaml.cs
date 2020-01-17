using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;
using Proryv.AskueARM2.Client.Visual.Controls.Popup;
using Proryv.ElectroARM.Controls;
using Proryv.ElectroARM.Controls.Common.Data;
using Proryv.ElectroARM.Monit.DataMonit.Data;
using Proryv.ElectroARM.Monit.DataMonit.Popup;
using Proryv.AskueARM2.Server.DBAccess.Public.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Proryv.ElectroARM.Controls.Monitoring.Data;
using Xceed.Wpf.DataGrid;
using EnumFreeHierarchyItemType = Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService.EnumFreeHierarchyItemType;
using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree.TreeSelector;
using EnumUnitDigit = Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.EnumUnitDigit;

namespace Proryv.AskueARM2.Client.Visual.Monitoring
{
    /// <summary>
    /// Interaction logic for DataMonit_Frame.xaml
    /// </summary>
    public partial class DataMonit_Frame : IModule, INotifyPropertyChanged, IDisposable, IFormingExportedFileName, IDatePeriod, ITreeModule, IModuleSettings
    {
        /// <summary>
        /// Тип модуля
        /// </summary>
        readonly Common.ModuleType _типМодуля;
        DataGridControl _grid;
        DataGridCollectionViewSource _view;

        readonly RangeObservableCollection<TMonitoringAnalyseRow> _viewSource = new RangeObservableCollection<TMonitoringAnalyseRow>();

        public DataMonit_Frame(Common.ModuleType типМодуля)
        {
            _filterFlag = VALUES_FLAG_DB.AllAlarmStatuses | VALUES_FLAG_DB.DataCorrect | VALUES_FLAG_DB.IsPointHasNoDrums | VALUES_FLAG_DB.IsForecast | VALUES_FLAG_DB.IsHasInactiveTariff |
                          VALUES_FLAG_DB.IsNotFoundTariffValues;
            _типМодуля = типМодуля;

            InitializeComponent();

#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = this;
            }

            DateTime dtEnd = DateTime.Now.Date;
            DateTime dtStart = new DateTime(dtEnd.Year, dtEnd.Month, 1);

            dateStart.SelectedDate = dtStart;
            dateEnd.SelectedDate = dtEnd;

#if DEBUG
            sw.Stop();
            Console.WriteLine("Конструктор {0} млс", sw.ElapsedMilliseconds);
#endif



            //автообновление
            _timerAutoRefresh = new DispatcherTimer();
            _timerAutoRefresh.Tick += new EventHandler(timerAutoRefresh_Tick);
            _timerAutoRefresh.Interval = new TimeSpan(0, 0, 0, 30, 0);

            var autoRefershSettings = new Dictionary<int, string>
            {
                {0, "нет"},
                {30, "30 сек"},
                {60, "1 мин"},
                {300, "5 мин"},
                {600, "10 мин"},
                {1800, "30 мин"},
                {3600, "1 час"}
            };
            cbAutoRefresh.ItemsSource = autoRefershSettings;
            cbAutoRefresh.SelectedValue = 0;
            cbAutoRefresh.SelectionChanged += cbAutoRefresh_SelectionChanged;
            //cbAutoRefresh.Visibility = Visibility.Hidden;

            cbAutoRefresh.Visibility = Visibility.Visible;

            //Настраиваем содержимое фильтра по объектам
            PopupFilterSelected.Child = CreateFilterSelectedContent(_типМодуля);
        }

        void cbAutoRefresh_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _timerAutoRefresh.Stop();
            if ((int)cbAutoRefresh.SelectedValue != 0)
            {
                _timerAutoRefresh.Interval = new TimeSpan(0, 0, 0, (int)cbAutoRefresh.SelectedValue, 0);


                _timerAutoRefresh.Start();
            }
        }

        DispatcherTimer _timerAutoRefresh;

        private void timerAutoRefresh_Tick(object source, EventArgs eventArgs)
        {
            if (this.IsVisible && !progress.IsRunning)
            {
                progress_Click(this, null);
            }

        }

        List<TSimpleChecked> _levelsForMonitoringSource = null;

        #region members

        string _titleTotal = "Таблица";
        public string TitleTotal
        {
            get
            {
                return _titleTotal;
            }
            set
            {
                _titleTotal = value;
                _PropertyChanged("TitleTotal");
            }
        }

        DependencyPropertyDescriptor _selectedDateChangedDescriptor = null;

        /// <summary>
        /// Событие на смену начальной даты, в конечной дате день ставим из начальной даты
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedDateChanged(object sender, EventArgs e)
        {
            if (!IsLoaded || !(sender is Xceed.Wpf.Controls.DatePicker)) return;
            Xceed.Wpf.Controls.DatePicker dtPicker = sender as Xceed.Wpf.Controls.DatePicker;
            DatePickerHelper.CorrectDateStartOrDateEnd(dateStart, dateEnd, dtPicker.Name == "dateStart");
        }

        public ModuleType ModuleType
        {
            get { return _типМодуля; }
        }

        public string TimeZoneId { get; set; }

        public DateTime? DayStart { get; set; }
        public TimeSpan? TimeStart { get; set; }

        public DateTime? DayEnd { get; set; }
        public TimeSpan? TimeEnd { get; set; }

        public bool Init()
        {
#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif
            Manager.UI.RunUILocked((() =>
                              {
                                  progress.Visibility = System.Windows.Visibility.Collapsed;

                                  if (_типМодуля == Common.ModuleType.MonitoringAutoSbor)
                                  {
                                      _grid = (this.Resources["gridAutoSbor"] as DataTemplate).LoadContent() as DataGridControl;
                                      _view = this.Resources["viewSourceAutoSbor"] as DataGridCollectionViewSource;
                                      //cfg = Manager.Config.DataMonitAutosborShortcut;
                                  }
                                  else
                                  {
                                      _grid = (this.Resources["grid61968"] as DataTemplate).LoadContent() as DataGridControl;
                                      _view = this.Resources["viewSource61968"] as DataGridCollectionViewSource;
                                     //cfg = Manager.Config.DataMonit61968Shortcut;
                                  }

                                  try
                                  {
                                      _levelsForMonitoringSource =
                                          GlobalEnumsDictionary.GroupingLevelsForMonitoringDictionary
                                              .Select(g => new TSimpleChecked()
                                                           {
                                                               Id = (int) g.Key,
                                                               Text = g.Value,
                                                               IsChecked = true,
                                                           })
                                              .ToList();

                                  }
                                  catch (Exception)
                                  {
                                  }
                                  _view.Source = _viewSource;
                                  //Подписываем на предикат фильтра отображения объектов без ПУ
                                  _view.Filter += MeterNumbersPredicateDelegate;
                                  layoutGrid.Children.Add(_grid);

                                  listLevelsForMonitoring.ItemsSource = _levelsForMonitoringSource;
                                  if (_levelsForMonitoringSource != null && _levelsForMonitoringSource.Count > 0)
                                  {
                                      listLevelsForMonitoring.SelectedIndex = 0;
                                  }

                                  progress.Visibility = System.Windows.Visibility.Visible;

                              }));

            //-----Событие на смену начальной даты, в конечной дате день ставим из начальной даты
            _selectedDateChangedDescriptor = DependencyPropertyDescriptor.FromProperty(Xceed.Wpf.Controls.DatePicker.SelectedDateProperty,
                typeof(Xceed.Wpf.Controls.DatePicker));
            _selectedDateChangedDescriptor.AddValueChanged(dateStart, new EventHandler(OnSelectedDateChanged));
            _selectedDateChangedDescriptor.AddValueChanged(dateEnd, new EventHandler(OnSelectedDateChanged));

#if DEBUG
            sw.Stop();
            Console.WriteLine("Инициализация {0} млс", sw.ElapsedMilliseconds);
#endif
            return true;
        }

        public CloseAction Close()
        {
            //string cfg = String.Join("|", GetControlsFromForms());
            ////Устанавливаем настройки формы
            //switch (_типМодуля)
            //{
            //    case Common.ModuleType.MonitoringAutoSbor:
            //        Manager.Config.DataMonitAutosborShortcut = cfg;
            //        break;
            //    case Common.ModuleType.Monitoring61968:
            //        Manager.Config.DataMonit61968Shortcut = cfg;
            //        break;
            //}



            _timerAutoRefresh.Stop();
            cbAutoRefresh.SelectedValue = 0;
            return CloseAction.HideToHistory;
        }

        public string Shortcut
        {
            get { return string.Empty; }
            set
            {
                Init();
            }
        }

        public string GetModuleToolTip
        {
            get { return "Анализ мониторинга"; }
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

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void _PropertyChanged(string property)
        {
            if (PropertyChanged != null && !DesignerProperties.GetIsInDesignMode(this))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion

        public void Dispose()
        {
            if (PopupFilterSelected != null)
            {
                var popupChild = PopupFilterSelected.Child as IDisposable;
                if (popupChild != null)
                {
                    popupChild.Dispose();
                }
                PopupFilterSelected.Child = null;
            }

            if (_popupValidate != null)
            {
                _popupValidate.Child = null;
                _popupValidate.Opened -= _popupValidate_Opened;
                _popupValidate.Closed -= _popupValidate_Opened;
            }

            if (_view!=null) _view.Filter -= MeterNumbersPredicateDelegate;

            if (worker != null)
            {
                worker.CancelAsync();

                worker.DoWork -= WorkerDoWork;
                worker.RunWorkerCompleted -= WorkerCompleted;
            }

            if (_selectedDateChangedDescriptor != null)
            {
                _selectedDateChangedDescriptor.RemoveValueChanged(dateStart, OnSelectedDateChanged);
                _selectedDateChangedDescriptor.RemoveValueChanged(dateEnd, OnSelectedDateChanged);
            }

            if (_grid != null)
            {
                _grid.ClearItems();

                if (_grid.DetailConfigurations != null)
                {
                    _grid.DetailConfigurations.Clear();
                }
            }

            listLevelsForMonitoring.ClearItems();

            layoutGrid.Children.Clear();

            _grid = null;
            _view = null;
            DataContext = null;
        }

        #endregion

        #region Обновление таблицы

        private void progress_Click(object sender, EventArgs e)
        {
            _popupValidate.IsOpen = false;
            GetAnalyse();
        }

        private void GetAnalyse()
        {
            _grid.DetailConfigurations.Clear();
            _viewSource.DisposeChildren();
            _viewSource.Clear();

            if (!dateStart.SelectedDate.HasValue || !dateEnd.SelectedDate.HasValue)
            {
                Manager.UI.ShowMessage("Необходимо корректно задать начальную и конечную даты!");
                progress.Abort();
                return;
            }

            #region проверяем входящие параметры

            HashSet<int> uspds;
            HashSet<int> tis;
            var filterObjectsList = GetSelectedObjects(true, out uspds, out tis);
            if (filterObjectsList != null && filterObjectsList.Count == 0 && tis !=null && tis.Count == 0)
            {
                Manager.UI.ShowMessage("Необходимо выбрать хотя бы один объект или ТИ для мониторинга!");
                progress.Abort();
                return;
            }

            switch (
                ValidHelper.Valid(enumTimeDiscreteType.DBHours, 1000, dateEnd.SelectedDate.Value - dateStart.SelectedDate.Value,
                    new Tuple<Xceed.Wpf.Controls.DatePicker, Xceed.Wpf.Controls.DatePicker>(dateStart, dateEnd), GetAnalyse)
                )
            {
                case enumRestartAfterValid.Abort:
                    //Ошибка входящих параметров
                    progress.Abort();
                    return;
                case enumRestartAfterValid.Restart:
                    return;
            }

            #endregion

            DateTime dtStart;
            DateTime dtEnd;
            double? maxValue;
            bool isConcentratorsEnabled;
            bool isMetersEnabled;
            List<int> selectedEvents;
            List<byte> selectedСhannels;

            GetParamsFromForm(out dtStart, out dtEnd, out maxValue, out isConcentratorsEnabled, out isMetersEnabled, out selectedEvents, out selectedСhannels);

            this.RunAsync(() => GetObjectsForAnalys(filterObjectsList, uspds, tis, dtStart, dtEnd, maxValue, isConcentratorsEnabled, isMetersEnabled, selectedEvents, selectedСhannels),
                monitoringFactory =>
                {
                    if (monitoringFactory == null || _grid == null)
                    {
                        progress.Abort();
                        return;
                    }

                    ConfigureGrid(monitoringFactory.DtStart, monitoringFactory.DtEnd, monitoringFactory.IsConcentratorsEnabled, monitoringFactory.IsMetersEnabled);
                    _viewSource.AddRange(monitoringFactory.MonitoringAnalyseDict.Values);

                    var settings = VisualEx.GetModuleSettings(this);
                    if (settings != null) Manager.Config.SaveModulesSettingsCompressed(this.GetType().Name, settings);

                    progress.Value = 0;
                    progress.Maximum = monitoringFactory.MonitoringAnalyseDict.Count;
                    //Собрали в таблицу информацию по шлюзам и концентраторам
                    //Теперь собираем информацию по ПУ для этих объектов
                    worker = new BackgroundWorker {WorkerSupportsCancellation = true};
                    worker.DoWork += WorkerDoWork;
                    worker.RunWorkerCompleted += WorkerCompleted;
                    worker.RunWorkerAsync(monitoringFactory);
                });
        }


        private MonitoringClientFactory GetObjectsForAnalys(List<int> filterObjectsList, HashSet<int> uspds, HashSet<int> tis, DateTime dtStart,
            DateTime dtEnd,
            double? maxValue,
            bool isConcentratorsEnabled,
            bool isMetersEnabled,
            List<int> selectedEvents,
            List<byte> selectedСhannels)
        {
            var monitoringAnalyseResult = ARM_Service_.Monit_GetAnalyse(_типМодуля == Common.ModuleType.Monitoring61968, filterObjectsList, tis.ToList(), dtStart, dtEnd);
            if (monitoringAnalyseResult == null) return null;
            if (monitoringAnalyseResult.Errors != null && monitoringAnalyseResult.Errors.Length > 0)
            {
                Manager.UI.ShowMessage(monitoringAnalyseResult.Errors.ToString());
            }

            //Отфильтровываем ненужные УСПД
            if (uspds.Count > 0 && monitoringAnalyseResult.MonitoringAnalyseUSPD != null && monitoringAnalyseResult.MonitoringAnalyseUSPD.Count > 0)
            {
                var neededMonitoringAnalyses = new List<TMonitoringAnalyse>();
                foreach (var monitoringAnalysisGroupByPs in monitoringAnalyseResult.MonitoringAnalyseUSPD.GroupBy(g=>g.PS_ID ?? -1))
                {
                    //Если явно не задана ПС, то объект нужен в любом случае
                    if (monitoringAnalysisGroupByPs.Key == -1)
                    {
                        neededMonitoringAnalyses.AddRange(monitoringAnalysisGroupByPs.Where(monitoringAnalysis => uspds.Contains(monitoringAnalysis.ID)));
                    }
                    else
                    {
                        //Если есть УСПД из этой ПС значит нужны только указанные УСПД
                        neededMonitoringAnalyses.AddRange(monitoringAnalysisGroupByPs.Any(monitoringAnalysis => uspds.Contains(monitoringAnalysis.ID))
                            ? monitoringAnalysisGroupByPs.Where(monitoringAnalysis => uspds.Contains(monitoringAnalysis.ID))
                            : monitoringAnalysisGroupByPs);
                    }
                }

                monitoringAnalyseResult.MonitoringAnalyseUSPD = neededMonitoringAnalyses;
            }

            //Для отмены
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            _waiterId = Guid.NewGuid();

            //Фактория клиента для получения и построения результата мониторинга
            var monitoringFactory = new MonitoringClientFactory
                (
                dtStart,
                dtEnd,
                maxValue,
                isConcentratorsEnabled,
                isMetersEnabled,
                selectedEvents,
                selectedСhannels,
                _filterFlag,
                cancellationToken,
                progress, ShowTotalStatistic,
                _waiterId.Value,
                monitoringAnalyseResult, ModuleType
                );

            //Ждем обработки на сервере
            //int daynumbers = (int) Math.Ceiling(Math.Abs((dtEnd - dtStart).Days)/(double) 10);
            //int timeout = monitoringFactory.MonitoringAnalyseDict.Count*20*daynumbers;
            //if (timeout > 20000) timeout = 20000;
            //Thread.Sleep(timeout);

            return monitoringFactory;
        }

        private void ConfigureGrid(DateTime dtStart, DateTime dtEnd, bool isConcentratorsEnabled, bool isMetersEnabled)
        {
            int i = 0;
            DataTemplate cellTemplate = _grid.Resources["monitoringDataCellContentTemplate"] as DataTemplate;
            DataTemplate titleTemplate = _grid.Resources["TitleTemplate"] as DataTemplate;

            var metersDetailConfiguration = (_grid.Resources["metersDetailConfiguration"] as DetailConfiguration).Clone();
            //metersDetailConfiguration.BeginInit();
            List<DateTime> DateList = Proryv.AskueARM2.Both.VisualCompHelpers.Extention.GetDateTimeListForPeriod(dtStart, dtEnd, enumTimeDiscreteType.DB24Hour, Manager.Config.SelectedTimeZoneInfo);

            var eventStateColumn = metersDetailConfiguration.Columns.FirstOrDefault(c => c.FieldName == "EventState");

            var insertedIndex = eventStateColumn == null ? 8 : eventStateColumn.Index + 1; 

            foreach (var dt in DateList)
            {
                metersDetailConfiguration.Columns.Add(new Column
                                                                {
                                                                    FieldName = "Cols[" + i + "]",
                                                                    VisiblePosition = insertedIndex + i,
                                                                    CellContentTemplate = cellTemplate,
                                                                    AllowGroup = false,
                                                                    AllowSort = false,
                                                                    Title = dt,
                                                                    TitleTemplate = titleTemplate,
                                                                    Width = 50,
                                                                    ReadOnly = true,
                                                                    MinWidth = 30,
                                                                    MaxWidth = 240,
                                                                });
                i++;
            }

            _grid.AllowDetailToggle = false;

            //metersDetailConfiguration.EndInit();

            if (metersDetailConfiguration.Headers.Count == 0)
            {
                metersDetailConfiguration.Headers.Add(ResourceTemplatesLoader.ColumnManagerRowHeaders);
                metersDetailConfiguration.UseDefaultHeadersFooters = false;
            }

            if (ModuleType == ModuleType.MonitoringAutoSbor)
            {
                _grid.DetailConfigurations.Add(metersDetailConfiguration);
                if (!isConcentratorsEnabled) return;

                var concentratorDetailConfiguration = (_grid.Resources["concentratorDetailConfiguration"] as DetailConfiguration).Clone();

                if (concentratorDetailConfiguration.Headers.Count == 0)
                {
                    concentratorDetailConfiguration.Headers.Add(ResourceTemplatesLoader.ColumnManagerRowHeaders);
                    concentratorDetailConfiguration.UseDefaultHeadersFooters = false;
                }

                if (isMetersEnabled)
                {
                    concentratorDetailConfiguration.DetailConfigurations.Add(metersDetailConfiguration.Clone());
                }

                _grid.DetailConfigurations.Add(concentratorDetailConfiguration);
            }
            else
            {
                _grid.DetailConfigurations.Add(metersDetailConfiguration);
            }
        }

        private void progress_Cancel(object sender, EventArgs e)
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }
            if (worker != null)
            {
                progress.IsEnabled = false;
                worker.CancelAsync();
            }

            //Отправляем на сервер отмену выполнения мониторинга
            CancelWaitMonit();
        }

        /// <summary>
        /// Отправляем на сервер отмену выполнения мониторинга
        /// </summary>
        private void CancelWaitMonit()
        {
            if (!_waiterId.HasValue) return;

            Manager.UI.RunUILocked(() =>
                                   {
                                       try
                                       {
                                           ARM_Service.Monit_CancelWaitArchives(_waiterId.Value);
                                       }
                                       catch (Exception ex)
                                       {
                                           Manager.UI.ShowMessage(ex.Message);
                                       }
                                       _waiterId = null;
                                   });
        }

        Guid? _waiterId;
        //List<TMonitoringAnalyseRow> MonitoringAnalyseList = new List<TMonitoringAnalyseRow>();

        BackgroundWorker worker = null;
        CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Основная задача обновления данных по объектам сбора
        /// </summary>
        private void WorkerDoWork(object sender, DoWorkEventArgs eventArgs)
        {
            var monitoringFactory = eventArgs.Argument as MonitoringClientFactory;
            try
            {
                if (monitoringFactory != null)
                {
                    var buildResultTask = monitoringFactory.RunServerBuild(worker);
                    if (buildResultTask != null)
                    {
                        buildResultTask.Wait();
                    }
                }
            }
            catch (AggregateException aex)
            {
                eventArgs.Result = aex.Flatten();
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                eventArgs.Result = ex;
            }
        }

        private void ShowTotalStatistic(TStatisticInformation statisticInformation)
        {
            if (statisticInformation == null) return;
            //Итоговая информация
            var titleTotal = "Запрограммировано - " + statisticInformation.TotalNumbers + ",  Полные - " + statisticInformation.GoodNumbers 
                + ",  Недостоверные - " + statisticInformation.NotCorrectNumbers + ",  Неполные - " + statisticInformation.DataNotFullNumbers 
                + ",  Отсутствует - " + statisticInformation.AllDataNotFullNumbers + ",  Недостаток тариф. данных - " + statisticInformation.IsHasInactiveTariffNumbers
                + ", Расход по неакт. тарифу - " + statisticInformation.NotActiveChannelIntegralValueNumbers
                + ", Некорректное показание - " + statisticInformation.IntegralValueNotCorrectNumbers;
            if (statisticInformation.IsWithEventsOnly)
            {
                titleTotal += " (только из ТИ с выбранными событиями)";
            }

            Dispatcher.BeginInvoke(new Action(() => TitleTotal = titleTotal));
        }

        void WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Manager.UI.ShowMessage(e.Error.Message);

                //Это ошибка отменяем все запросы на сервере
                CancelWaitMonit();
            }
            else
            {
                //Если накопились на отмену - отменяем
                _waiterId = null;

                var ex = e.Result as Exception;
                if (ex != null)
                {
                    Manager.UI.ShowMessage(ex.Message);
                }

                _grid.AllowDetailToggle = true;

                //Обновляем view
                Dispatcher.BeginInvoke(new Action(delegate()
                                                      {
                                                          if (_grid != null)
                                                          {
                                                              DataGridCollectionView collectionView = _grid.ItemsSource as DataGridCollectionView;
                                                              if (collectionView != null)
                                                              {
                                                                  collectionView.Refresh();
                                                              }
                                                          }
                                                      }), DispatcherPriority.ApplicationIdle);
            }

            progress.Abort();
            progress.IsEnabled = true;
        }

        private List<int> GetSelectedObjects(bool isSelected, out HashSet<int> uspds, out HashSet<int> tis)
        {
            uspds = new HashSet<int>();
            tis = new HashSet<int>();
            var psList = new List<int>();

            var freeFilterPopup = PopupFilterSelected.Child as FreeHierarchyFilter_Popup;
            if (freeFilterPopup != null)
            {
                var selected = freeFilterPopup.tree.GetSelectedIds();
                foreach (var id in selected)
                {
                    switch (id.TypeHierarchy)
                    {
                        case enumTypeHierarchy.Dict_PS:
                            psList.Add(id.ID);
                            break;
                        case enumTypeHierarchy.USPD:
                            var uspd = FreeHierarchyDictionaries.USPDDict[id.ID];
                            if (uspd != null)
                            {
                                psList.Add(uspd.PS_ID.GetValueOrDefault());
                                uspds.Add(uspd.USPD_ID);
                            }
                            break;
                        case enumTypeHierarchy.Info_TI:
                            tis.Add(id.ID);
                            break;
                    }
                }
                return psList;
            }

            List<TSlaveChecked> filterSource = null;
            var filterPopup = PopupFilterSelected.Child as MonitoringFilterSelected_Popup;
            if (filterPopup != null)
            {
                filterSource = filterPopup.FilterSource;
            }

            if (filterSource != null)
            {
                psList = filterSource
                    .Where(m => m.IsChecked == isSelected && m.HierarchyItem != null)
                    .Select(m => m.Id)
                    .ToList();
            }
            else
            {
                psList = null;
            }

            return psList;
        }

        /// <summary>
        /// процедура запроса параметров с формы
        /// </summary>
        /// <returns></returns>
        public void GetParamsFromForm(out DateTime dtStart, out DateTime dtEnd, out double? maxValue, out bool isConcentratorsEnabled, out bool isMetersEnabled,
            out List<int> selectedEvents, out List<byte> selectedChannels)
        {
            dtStart = dateStart.SelectedDate.GetValueOrDefault().DateTimeToWCFDateTime().Date;
            dtEnd = dateEnd.SelectedDate.GetValueOrDefault().DateTimeToWCFDateTime().Date;

            if (_levelsForMonitoringSource != null)
            {
                var groupingLevelsForMonitoringList = _levelsForMonitoringSource
                    .Where(l => l.IsChecked && l.Id.HasValue)
                    .Select(l =>
                    {
                        enumGroupingLevelsForMonitoring groupingLevelsForMonitoring;
                        Enum.TryParse(l.Id.Value.ToString(), out groupingLevelsForMonitoring);
                        return groupingLevelsForMonitoring;
                    }).ToList();

                isConcentratorsEnabled = groupingLevelsForMonitoringList.Contains(enumGroupingLevelsForMonitoring.показывать_концентраторы);
                isMetersEnabled = groupingLevelsForMonitoringList.Contains(enumGroupingLevelsForMonitoring.показывать_счетчики);
            }
            else
            {
                isConcentratorsEnabled = false;
                isMetersEnabled = false;
            }

            //Выбираем базовые каналы
            //selectedChannels = GlobalTreeDictionary.GetSelectedTariffChannels();
            selectedChannels = new List<byte>();
            if (checkBoxAP.IsChecked.GetValueOrDefault()) selectedChannels.Add(1);
            if (checkBoxAO.IsChecked.GetValueOrDefault()) selectedChannels.Add(2);
            if (checkBoxRP.IsChecked.GetValueOrDefault()) selectedChannels.Add(3);
            if (checkBoxRO.IsChecked.GetValueOrDefault()) selectedChannels.Add(4);

            maxValue = null;

            selectedEvents = null;

            //Проверяем отображать ли ТИ с выбранными событиями
            if (EventFilterOnly.IsChecked.GetValueOrDefault())
            {
                if (!eFilter.IsAllEventsSelected)
                {
                    if (eFilter.selectedEvents != null && eFilter.selectedEvents.Count > 0)
                    {
                        selectedEvents = eFilter.selectedEvents
                            .Where(s => s.Value != "")
                            .Select(s => s.Key)
                            .ToList();
                    }
                }

                lock (_flagSyncLock)
                {
                    _filterFlag = _filterFlag | VALUES_FLAG_DB.EventFilterOnly; //Фильтр по событиям (только точки с указанными событиями)
                }
            }
            else
            {
                lock (_flagSyncLock)
                {
                    _filterFlag &= ~VALUES_FLAG_DB.EventFilterOnly; //Отображать все события
                }
            }
        }

        /// <summary>
        /// процедура записи параметров на форму
        /// </summary>
        /// <returns></returns>
        public bool SetControlsOnForms(Dictionary<string, string> dict)
        {
            try
            {
                var delimer = new char[] { ',' };

                //DateTime dt;
                ////Дата, время
                //if (DateTime.TryParse(args[0], out dt))
                //{
                //    dateStart.SelectedDate = dt;
                //}
                //else
                //{
                //    return false;
                //}
                //if (DateTime.TryParse(args[1], out dt))
                //{
                //    dateEnd.SelectedDate = dt;
                //}
                //else
                //{
                //    return false;
                //}

                //Каналы
                string channelSets;
                if (dict.TryGetValue("channelSets", out channelSets) && !string.IsNullOrEmpty(channelSets))
                {
                    string[] channels = channelSets.Split(delimer);
                    List<CheckBox> boxList = new List<CheckBox>
                     {
                         checkBoxAP,
                         checkBoxAO,
                         checkBoxRP,
                         checkBoxRO
                     };

                    bool b;
                    for (int i = 0; i < 4; i++)
                    {
                        if (Boolean.TryParse(channels.ElementAtOrDefault(i), out b))
                        {
                            boxList[i].IsChecked = b;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                //Фильтр достоверности
                string filterFlag;
                if (dict.TryGetValue("filterFlag", out filterFlag) && !string.IsNullOrEmpty(filterFlag))
                {
                    VALUES_FLAG_DB f;
                    if (Enum.TryParse<VALUES_FLAG_DB>(filterFlag, out f))
                    {
                        lock (_flagSyncLock)
                        {
                            _filterFlag = f;
                        }
                    }
                }

                //Фильтр событий
                string eventFilter;
                if (dict.TryGetValue("eventFilter", out eventFilter) && !string.IsNullOrEmpty(eventFilter))
                {
                    var codes = eventFilter.Split(delimer, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var code in codes)
                    {
                        eFilter.selectedEvents[int.Parse(code)] = "";
                    }

                    eFilter.RaiseFilterChanged();
                }

                //Фильтр объектов
                string objectFilter;
                if (dict.TryGetValue("objectFilter", out objectFilter) && !string.IsNullOrEmpty(objectFilter))
                {
                    var filterPopup = PopupFilterSelected.Child as MonitoringFilterSelected_Popup;
                    if (filterPopup != null)
                    {
                        filterPopup.butNone_Click(null, null);

                        string[] filteredObject = objectFilter.Split(delimer);

                        foreach (var ps in filterPopup.FilterSource)
                        {
                            if (ps != null && ps.HierarchyItem != null)
                            {
                                if (filteredObject.Any(ff => ff == ps.Id.ToString()))
                                {
                                    ps.IsChecked = false;
                                }
                                else
                                {
                                    ps.IsChecked = true;
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception)
            {
                return false;
            }

            return true;
        }

        #region IModuleSettings

        public EnumUnitDigit SelectedUnitDigit { get; set; }
        public EnumUnitDigit SelectedUnitDigitIntegrals { get; set; }
        public enumTimeDiscreteType DiscreteType { get; set; }

        /// <summary>
        /// Собираем настройки с формы
        /// </summary>
        /// <param name="dict">Словарь, куда пишем настройки</param>
        public void GetSettings(StringBuilder settings)
        {
            GetControlsFromForms(settings);
        }

        /// <summary>
        /// Дополнительная процедура для выставления настроек на форме
        /// </summary>
        /// <param name="dict">Словарь из которого берем настройки</param>
        public void SetSettings(Dictionary<string, string> dict)
        {
            SetControlsOnForms(dict);
        }

        #endregion

        public void GetControlsFromForms(StringBuilder sb)
        {
            //Дата, время
            //result.Add(dateStart.SelectedDate.ToString());
            //result.Add(dateEnd.SelectedDate.ToString());

            //Настройки каналов
            var boxList = new List<CheckBox>()
            {
                checkBoxAP,
                checkBoxAO,
                checkBoxRP,
                checkBoxRO
            };
            string channelSets = String.Join(",", boxList.Select(b => b.IsChecked.GetValueOrDefault()));
            sb.Append("channelSets═"+ channelSets).Append("¤");

            //Фильтр показа
            lock (_flagSyncLock)
            {
                sb.Append("filterFlag═" + _filterFlag.ToString()).Append("¤");
            }

            //Фильтр событий
            var fi = string.Join(",", eFilter.selectedEvents.Select(s=>s.Key));
            sb.Append("eventFilter═" + fi).Append("¤");

            HashSet<int> uspds;
            HashSet<int> tis;
            //Фильтр объектов
            var filterList = GetSelectedObjects(false, out uspds, out tis);
            fi = "";
            if (filterList != null && filterList.Count > 0)
            {
                fi = String.Join(",", filterList.Select(f => f.ToString()));
            }
            sb.Append("objectFilter═" + fi).Append("¤");
        }

        #endregion

        #region Фильтр по достоверности

        Popup _popupValidate = new Popup()
        {
            StaysOpen = false,
            Placement = PlacementMode.Bottom,
            AllowsTransparency = true,
            PopupAnimation = PopupAnimation.Slide,
        };

        private void InitValidatePopup(bool isOpen)
        {
            if (_popupValidate.Child == null)
            {
                _popupValidate.Child = CreateValidatePopupContent();
                _popupValidate.PlacementTarget = txtFilterSelect;
                _popupValidate.Opened += _popupValidate_Opened;
                _popupValidate.Closed += _popupValidate_Opened;
            }

            if (isOpen)
            {
                _popupValidate.OpenAndRegister(true);
            }
            else
            {
                _popupValidate.IsOpen = false;
            }
        }

        void _popupValidate_Opened(object sender, EventArgs e)
        {
            butFilterSelect.IsChecked = (sender as Popup).IsOpen;
        }

        public FrameworkElement CreateValidatePopupContent()
        {
            ValidateFilter_Popup TISelect_Control;
            lock (_flagSyncLock)
            {
                TISelect_Control = new ValidateFilter_Popup(applyFilter, true, _filterFlag);
            }

            return TISelect_Control;
        }

        private void butFilterSelect_Click(object sender, RoutedEventArgs e)
        {
            InitValidatePopup((sender as ToggleButton).IsChecked.GetValueOrDefault());
        }

        VALUES_FLAG_DB _filterFlag;
        private readonly object _flagSyncLock = new object();

        void applyFilter(VALUES_FLAG_DB filterFlag)
        {
            lock (_flagSyncLock)
            {
                _filterFlag = filterFlag;
            }
        }

        #endregion

        #region Фильтр показывать - непоказывать объекты без ПУ

        /// <summary>
        /// PredicateDelegate used by DataGridCollectionView for Filtering
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MeterNumbersPredicateDelegate(object sender, FilterEventArgs e)
        {
            TMonitoringAnalyseRow row = e.Item as TMonitoringAnalyseRow;
            e.Accepted = row != null
                && ((!row.IsMeterExists.HasValue || IsMeterNumbersMoreZeroOnly
                || (!IsMeterNumbersMoreZeroOnly && row.IsMeterExists.Value)));
        }

        private volatile bool IsMeterNumbersMoreZeroOnly = false;

        #endregion

        private void checkShowAll_Checked(object sender, RoutedEventArgs e)
        {
            var ch = sender as CheckBox;
            if (ch != null)
            {
                IsMeterNumbersMoreZeroOnly = ch.IsChecked.GetValueOrDefault();
                this.Dispatcher.BeginInvoke(new Action(delegate()
                {
                    if (_grid != null)
                    {
                        DataGridCollectionView collectionView = _grid.ItemsSource as DataGridCollectionView;
                        if (collectionView != null)
                        {
                            collectionView.Refresh();
                        }
                    }
                }), DispatcherPriority.ApplicationIdle);
            }
        }

        #region Выпадающая архивная информация

        private void InitPopup(Popup _popup)
        {
            try
            {
                _popup.Placement = PlacementMode.Bottom;
                if (_popup.Width == 0 && _типМодуля == ModuleType.MonitoringAutoSbor)
                {
                    _popup.Width = 450;
                    //_popup.Height = 600;
                }
                else _popup.IsOpen = !_popup.IsOpen;
            }
            catch (Exception ex)
            {
                Manager.UI.ShowMessage(ex.Message);
            }
        }

        FrameworkElement CreateFilterSelectedContent(Proryv.AskueARM2.Client.Visual.Common.ModuleType типМодуля)
        {
            if (ModuleType == ModuleType.MonitoringAutoSbor) return new FreeHierarchyFilter_Popup();
            else
            {
                var tiSelectControl = new MonitoringFilterSelected_Popup(типМодуля);
                return tiSelectControl;
            }
        }

        #endregion

        private void btnServerGlobalJournal_Click(object sender, RoutedEventArgs e)
        {
            Manager.UI.AddTab("Журнал системы сбора данных", Manager.Modules.CreateModule(ModuleType.ServersGlobalJournal), null);
        }

        /// <summary>
        /// Формируем название файла для экспорта в excel
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public string FormingExportedFileName(string TableName, out bool IsExportCollapsedDetail)
        {
            IsExportCollapsedDetail = true;

            string fileName;
            if (_типМодуля == Common.ModuleType.Monitoring61968)
            {
                fileName = "Мониторинг 61968";
            }
            else
            {
                fileName = "Мониторинг автоматизированного сбора";
            }

            DateTime dtStart = dateStart.SelectedDate.GetValueOrDefault(), dtEnd = dateEnd.SelectedDate.GetValueOrDefault();

            fileName += " c " + dtStart.ToString("dd.MM.yyyy").Replace(".", "_") + " по " + dtEnd.ToString("dd.MM.yyyy").Replace(".", "_");

            return fileName;
        }

        public bool ExportExcludeGroupBySettings(string tableName)
        {
            return false;
        }

        private void butExpand_Click(object sender, RoutedEventArgs e)
        {
            if (progress.IsRunning) return;

            bool isExpand = (sender is Button && (sender as Button).Name == "butExpand");

            if (_grid != null)
            {
                var context = DataGridControl.GetDataGridContext(_grid);

                if (context != null)
                {
                    foreach (var item in context.Items)
                    {
                        ExpandOrCollpase(item, context, isExpand);
                    }
                }
            }
        }

        void ExpandOrCollpase(object item, DataGridContext context, bool IsExpand)
        {
            if (item == null || context == null) return;

            if (!context.AreDetailsExpanded(item) && IsExpand)
            {
                context.ExpandDetails(item);
            }
            else if (!IsExpand)
            {
                context.CollapseDetails(item);
                return;
            }
            else return;

            var itemType = item.GetType();

            // перебираем все дочерние ветки
            foreach (var detail in context.DetailConfigurations)
            {
                var childContext = context.GetChildContext(item, detail.RelationName);

                // смотрим, не пустая ли ветка
                if ((childContext != null) && (childContext.Items.Count > 0) && (detail.Visible))
                {
                    var propertyInfo = itemType.GetProperty(detail.RelationName);
                    var childrenItems = propertyInfo.GetValue(item, null) as IEnumerable;
                    if (childrenItems != null)
                    {
                        foreach (var childrenItem in childrenItems)
                        {
                            // строим дочернюю ветку со смещением первого столбца на 1 вправо
                            ExpandOrCollpase(childrenItem, childContext, IsExpand);
                        }
                    }
                }
            }
        }

        private void butFilterSelect_MouseEnter(object sender, MouseEventArgs e)
        {
            _popupValidate.StaysOpen = true;
        }

        private void butFilterSelect_MouseLeave(object sender, MouseEventArgs e)
        {
            _popupValidate.StaysOpen = false;
        }

        private void btnCancellAllFilters_Click(object sender, RoutedEventArgs e)
        {
            //Фильтр по достоверности
            lock (_flagSyncLock)
            {
                _filterFlag = VALUES_FLAG_DB.None;
            }

            //Фильтр по объектам
            List<TSlaveChecked> filterSource = null;
            var filterPopup = PopupFilterSelected.Child as MonitoringFilterSelected_Popup;
            if (filterPopup != null)
            {
                filterSource = filterPopup.FilterSource;
            }

            if (filterSource != null && filterSource.Count > 0)
            {
                foreach (var ch in filterSource)
                {
                    ch.IsChecked = true;
                }
            }

            //Сброс настроек грида
            _grid.ClearColumnFilter();

            
            //this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(delegate()
            //{
            //    VisualEx.SetGridSettings(grid, "MONIT_GRID_IGOR");
            //}));
        }


        public DatePeriod datePeriod
        {
            get { return new DatePeriod(dateStart.SelectedDate, dateEnd.SelectedDate); }
        }

        public int Tree
        {
            get
            {
                var freeHierarchyFilterPopup = PopupFilterSelected.Child as FreeHierarchyFilter_Popup;
                if (freeHierarchyFilterPopup != null)
                    return freeHierarchyFilterPopup.tree.Tree_ID.GetValueOrDefault();
                return GlobalFreeHierarchyDictionary.TreeTypeStandartPS;
            }
        }

        public FreeHierarchyTreeItem GetNode(IFreeHierarchyObject hierarchyObject)
        {
            return null;
        }
    }
}
