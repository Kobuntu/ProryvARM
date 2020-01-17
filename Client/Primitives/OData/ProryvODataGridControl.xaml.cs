using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Infragistics.Controls.DataSource;
using Infragistics.Controls.Menus;
using Infragistics.Windows.Controls;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.DataPresenter.Events;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.ServiceReference.Service.OData;
using Proryv.ElectroARM.ODataGrid.Controls;
using Proryv.ElectroARM.ODataGrid.Controls.Messages;
using Proryv.ElectroARM.ODataGrid.Data;
using Proryv.ElectroARM.ODataGrid.DataPresenter;
using Proryv.ElectroARM.ODataGrid.FilterAdapter;
using Proryv.ElectroARM.ODataGrid.Interfaces;
using Proryv.ODataVirtualSource;

namespace Proryv.ElectroARM.ODataGrid
{
    /// <summary>
    /// Логика взаимодействия для ProryvODataGridControl.xaml
    /// </summary>
    public partial class ProryvODataGridControl: IDisposable, INotifyPropertyChanged, IMenuCollection
    {
       #region Настройки для ODATA

        private const int PageSizeRequested = 120;
        private const int MaxCachedPages = 20;
        private const int TimeoutMilliseconds = 10000;

        #endregion

        private ODataDataSource _source;

        public ODataDataSource VirtualODataSource
        {
            get { return _source; }
        }

        #region Контролы на форме

        private Button bRefresh;
        private ContentControl ccMessage;
        private TextBlock tbCurrentFullCount;
        private Button menuButton;

        private bool _isControlInitialized;

        #endregion

        #region DesignerSerializationVisibility

        private ButtonLayoutCollection _buttonLayouts;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ButtonLayoutCollection ButtonLayouts
        {
            get
            {
                if (_buttonLayouts == null) _buttonLayouts = new ButtonLayoutCollection(this);

                return _buttonLayouts;
            }
        }

        private MenuLayoutCollection _menuLayouts;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public MenuLayoutCollection MenuLayouts
        {
            get
            {
                if (_menuLayouts == null) _menuLayouts = new MenuLayoutCollection(this);

                return _menuLayouts;
            }
        }

        #endregion

        #region Events

        public event EventHandler BeforeRefresh;

        #endregion

        private string _entitySet;
        public string EntitySet
        {
            get { return _source == null ? null : _source.EntitySet; }
            set
            {
                if (!string.Equals(_entitySet, value, StringComparison.InvariantCultureIgnoreCase))
                {
                    _entitySet = value;
                }

                if (_source != null && 
                    !string.Equals(_source.EntitySet, value, StringComparison.InvariantCultureIgnoreCase))
                {
                    _source.EntitySet = value;
                }
            }
        }

        private string[] _fieldsRequested;
        public string[] FieldsRequested
        {
            get { return _source == null ? null : _source.FieldsRequested; }
            set
            {
                if (!ReferenceEquals(_fieldsRequested, value)) _fieldsRequested = value;

                if (_source != null && !ReferenceEquals(_source.FieldsRequested, value))
                {
                    _source.FieldsRequested = value;
                }

            }
        }

        public FilterExpressionCollection BaseFilter = new FilterExpressionCollection();

        public ProryvODataGridControl()
        {
            InitializeComponent();

            Simple.OData.Client.V4Adapter.Reference();

            
        }

        public event EventHandler<DataSourcePageEventArgs> BeforePageLoaded;

        private void OnPageLoaded(IDataSourcePage page, int currentFullCount, int actualPageSize)
        {
            if (!_isControlInitialized) InitControls();

            if (BeforePageLoaded != null) BeforePageLoaded(this, new DataSourcePageEventArgs {Page = page});

            string message;
            bool isCryticalError;

            var odataPage = page as ODataDataSourcePage;
            if (odataPage != null)
            {
                message = odataPage.Message;
                isCryticalError = odataPage.IsCryticalError;
            }
            else
            {
                message = null;
                isCryticalError = false;
            }

            var args = new VisualParams
            {
                Message = message,
                CurrentFullCount = currentFullCount,
                IsCryticalError = isCryticalError,

            };

            UpdateTemplateVisualState(args);

            //Message = "Количество " + currentFullCount;

        }

        /// <summary>
        /// Обновление содержимого темплейта в зависимости от ошибок, наличия данных или действия пользователя
        /// </summary>
        /// <param name="args"></param>
        private void UpdateTemplateVisualState(VisualParams args)
        {
            if (args == null) return;

            ShowMessage(args.Message, args.IsCryticalError);

            if (tbCurrentFullCount != null)
            {
                tbCurrentFullCount.Text = args.CurrentFullCount.ToString();
            }

            //var tbCurrentSelectedCount = template.FindName("tbCurrentSelectedCount", this) as TextBlock;
            //if (tbCurrentSelectedCount != null)
            //{
            //    tbCurrentSelectedCount.Text = this.Sele
            //}

            if (bRefresh != null) bRefresh.IsEnabled = true;
        }

        public  void ShowMessage(string message, bool isCryticalError)
        {
            if (ccMessage == null || (string.IsNullOrEmpty(message) && ccMessage.Visibility != Visibility.Visible)) return;

            Dispatcher.BeginInvoke((Action) (() =>
            {
                if (string.IsNullOrEmpty(message))
                {
                    if (ccMessage.Visibility == Visibility.Visible) ccMessage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    IMessageControl messageControl;
                    if (isCryticalError) messageControl = ccMessage.Content as WarningGridMessageControl ??  new WarningGridMessageControl();
                    else messageControl = ccMessage.Content as InfoGridMessageControl ?? new InfoGridMessageControl();

                    messageControl.Message = message;
                    if (!ReferenceEquals(ccMessage.Content, messageControl)) ccMessage.Content = messageControl;

                    ccMessage.Visibility = Visibility.Visible;
                }
            }));
        }

        public void Dispose()
        {
            ResetAsyncDataPending();

            if (_source != null)
            {
                _source.PageLoadedEvent -= OnPageLoaded;
                _source.Dispose();
            }
            //DataSource = null;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null && !DesignerProperties.GetIsInDesignMode(this))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion

        private void ButtonCancelOnClick(object sender, RoutedEventArgs e)
        {
            if (_source == null) return;

            if (bRefresh != null) bRefresh.IsEnabled = false;
            
            _source.CancelTask();

            Dispatcher.BeginInvoke((Action) (() =>
            {
                ResetAsyncDataPending();
                DataSource = null;
                if (bRefresh != null) bRefresh.IsEnabled = true;
            }), DispatcherPriority.Background);
        }

        private void ButtonRefreshOnClick(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        public void Refresh(bool savePreviousFilter = true)
        {
            if (bRefresh != null) bRefresh.IsEnabled = false;
            FilterExpressionCollection filterExpressions = null;
            if (_source != null)
            {
                //Сохраняем прежний фильтр
                if (savePreviousFilter)
                {
                    filterExpressions = _source.FilterExpressions;
                }

                DataSource = null;
                _source.Dispose();
            }

            if (BeforeRefresh != null) BeforeRefresh(this, EventArgs.Empty);

            //Dispatcher.BeginInvoke((Action) (() =>
            //{
                RecreateSource();

                if (savePreviousFilter && _source != null && _source.FilterExpressions != null && filterExpressions != null)
                {
                    //Фосстанавливаем прежний фильтр
                    foreach (var fe in filterExpressions)
                    {
                        _source.FilterExpressions.Add(fe);
                    }
                }
            //}));
        }


        private void RecreateSource()
        {
            _source = new ODataDataSource(BaseFilter, BuildSortDescription())
            {
                //BaseFilter = _baseFilter,
                BaseUri = ConnectionHelper.ODataBaseUri,
                EntitySet = _entitySet,
                //Ниже поля, которые не перечислены в перечне полей таблицы
                FieldsRequested = _fieldsRequested,
                PageSizeRequested = PageSizeRequested,
                MaxCachedPages = MaxCachedPages,
                TimeoutMilliseconds = TimeoutMilliseconds,
                
            };

            _source.PageLoadedEvent += OnPageLoaded;
            DataSource = _source;
            ApplyFilters();
        }

        private Infragistics.Controls.DataSource.SortDescriptionCollection BuildSortDescription()
        {
            var fl = this.DefaultFieldLayout;
            if (fl == null) return null;

            var sdc = fl.SortedFields;
            if (sdc == null || sdc.Count == 0) return null;

            var result = new Infragistics.Controls.DataSource.SortDescriptionCollection();
            foreach (var sd in sdc)
            {
                result.Add(new SortDescription(sd.FieldName, sd.Direction));
            }

            return result;
        }

        private void ApplyFilters()
        {
            var fl = this.FieldLayouts.FirstOrDefault();
            if (fl == null || fl.RecordFilters == null || !fl.RecordFilters.Any()) return;

            //Infragistics.Windows.Controls.ComparisonCondition >
            var oldrf = new List<KeyValuePair<string, List<ICondition>>>();

            //Пересоздаем фильтры, т.к. старые перестают работать
            try
            {
                foreach (var fr in fl.RecordFilters)
                {
                    if (fr.Conditions == null || !fr.Conditions.Any()) continue;

                    oldrf.Add(new KeyValuePair<string, List<ICondition>>(fr.FieldName,
                        fr.Conditions.Select(c => c).ToList()));
                }

                fl.RecordFilters.Clear();

                foreach (var fr in oldrf)
                {
                    var rf = new RecordFilter
                    {
                        FieldName = fr.Key,

                    };

                    foreach (var condition in fr.Value)
                    {
                        rf.Conditions.Add(condition);
                    }

                    fl.RecordFilters.Add(rf);
                }
            }
            catch
            {
                //TODO
            }
        }

        private void InitControls()
        {
            var template = Template;
            ccMessage = template.FindName("ccMessage", this) as ContentControl;
            tbCurrentFullCount = template.FindName("tbCurrentFullCount", this) as TextBlock;
            bRefresh = template.FindName("bRefresh", this) as Button;
            menuButton = template.FindName("menuButton", this) as Button;

            var lvButtonLayout = template.FindName("lvButtonLayout", this) as ItemsControl;
            if (lvButtonLayout != null)
            {
                lvButtonLayout.ItemsSource = ButtonLayouts;
            }

            if (FieldLayoutSettings.AllowClipboardOperations == null)
            {
                FieldLayoutSettings.AllowClipboardOperations = AllowClipboardOperations.All;
            }

            _isControlInitialized = ccMessage!=null;
        }

        #region IMenuCollection

        public void InsertMenuItem(int index, Control item)
        {
            var source = xcmMenu.ItemsSource as MenuLayoutCollection;
            if (source == null) return;

            source.Insert(index, item);
        }

        public void RemoveMenuItem(int index)
        {
            var source = xcmMenu.ItemsSource as MenuLayoutCollection;
            if (source == null) return;

            source.RemoveAt(index);
        }

        public void ClearMenuItems()
        {
            var source = xcmMenu.ItemsSource as MenuLayoutCollection;
            if (source == null) return;

            source.Clear();
        }

        public void SetMenuItem(int index, Control item)
        {
            var source = xcmMenu.ItemsSource as MenuLayoutCollection;
            if (source == null) return;

            source.SetItemByIndex(index, item);
        }

        #endregion

        private void MenuContextShowOnClick(object sender, RoutedEventArgs e)
        {
            _isOpenFromButton = true;
            xcmMenu.IsOpen = true;
        }

        private bool _isOpenFromButton;
        private void XcmMenuOnOpening(object sender, OpeningEventArgs e)
        {
            var menu = sender as XamContextMenu;
            if (menu != null && menuButton!=null)
            {
                if (_isOpenFromButton)
                {
                    menu.PlacementTarget = menuButton;
                    menu.Placement = PlacementMode.AlignedBelow;
                }
                else
                {
                    menu.PlacementTarget = this;
                    menu.Placement = PlacementMode.MouseClick;
                }
            }

            _isOpenFromButton = false;
        }

        #region Работа с фильтром

        private void dp_FieldLayoutInitialized(object sender, FieldLayoutInitializedEventArgs e)
        {
            if (e.FieldLayout == null || e.FieldLayout.Fields == null) return;
            foreach (Field field in e.FieldLayout.Fields)
            {
                Type dataType = field.DataType;

                if (dataType == typeof(DateTime))
                {
                    field.Settings.FilterOperatorDefaultValue = ComparisonOperator.Equals;
                    //Меням настройки фильтра поумолчанию
                    field.Settings.FilterOperatorDropDownItems = ComparisonOperatorFlags.Equals
                                                                 | ComparisonOperatorFlags.LessThan
                                                                 | ComparisonOperatorFlags.LessThanOrEqualsTo
                                                                 | ComparisonOperatorFlags.GreaterThan
                                                                 | ComparisonOperatorFlags.GreaterThanOrEqualsTo;
                }
                else if (dataType != typeof(bool))
                {
                    //Меням настройки фильтра поумолчанию
                    field.Settings.FilterOperatorDefaultValue = ComparisonOperator.Contains;
                }
            }
        }

        private readonly HashSet<Type> _filterResolvedTypes = new HashSet<Type>
        {
            typeof(DateTime),typeof(EnumAlarmSeverity)
        };

        #endregion

        public bool IsIgnoreRefreshOnLoad { get; set; }

        private void ProryvODataGridControlOnLoaded(object sender, RoutedEventArgs e)
        {
            //Refresh();

            if (!IsIgnoreRefreshOnLoad)
            {
                Dispatcher.BeginInvoke(new Action((() => Refresh())), DispatcherPriority.Background);
            }
        }
    }

}
