using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Infragistics.Controls.Menus;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.ServiceReference.UAService;
using Proryv.AskueARM2.Client.Visual;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.Configuration;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using Proryv.AskueARM2.Client.ServiceReference.StimulReportService;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;
using EnumFreeHierarchyItemType = Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService.EnumFreeHierarchyItemType;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using Infragistics.DragDrop;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Proryv.AskueARM2.Client.ServiceReference.Service.Interfaces;
using Proryv.ElectroARM.Controls.Controls.Popup.Finder;
using Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree.TreeSelector;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree.Data;
using Proryv.AskueARM2.Client.ServiceReference.Interfaces;
using Proryv.AskueARM2.Client.ServiceReference.Data.FreeHierarchy;
using Proryv.AskueARM2.Client.Visual.Common.Common;

namespace Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree
{
    /// <summary>
    /// Interaction logic for FreeHierarchyTree.xaml
    /// </summary>
    public partial class FreeHierarchyTree : IDisposable, INotifyPropertyChanged, IFreeHierarchyTree
    {
        public FreeHierarchyTree()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            btnReload.Visibility = Visibility;
            tree.Resources["treeModule"] = this;
        }

        public FreeHierarchyTreeFindBar TreeFindBar;

        private HashSet<long> _selectedNodes;
        private readonly object _selectedNodesSyncLock = new object();

        public EnumsExtendetTemplates ExtendetTemplates { get; set; }

        bool _isDeclaratorMainTree = false;
        EnumModuleFilter? _currentRightFilter;

        private bool _permissibleForSelectObjectsLoaded;
        private IPermissibleForSelectObjects _permissibleForSelectObjects;
        public IPermissibleForSelectObjects PermissibleForSelectObjects
        {
            get
            {
                if (_permissibleForSelectObjectsLoaded || _permissibleForSelectObjects != null) return _permissibleForSelectObjects;
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    _permissibleForSelectObjects = this.FindParent<IPermissibleForSelectObjects>();
                }
                else
                {

                }

                _permissibleForSelectObjectsLoaded = true;

                return _permissibleForSelectObjects;
            }
        }

        /// <summary>
        /// Наличие расширенного поиска
        /// </summary>
        public static readonly DependencyProperty OuterSelectorProperty = DependencyProperty.RegisterAttached(
            "OuterSelector", typeof(DataTemplateSelector), typeof(FreeHierarchyTree), new FrameworkPropertyMetadata(new ItemSelector()));

        //private bool _outerSelectorLoaded;
        //private Trees_ItemSelector _outerSelector;
        public Trees_ItemSelector OuterSelector
        {
            get { return GetValue(OuterSelectorProperty) as Trees_ItemSelector; }
            set { SetValue(OuterSelectorProperty, value); }
        }

        public static readonly DependencyProperty TreeModeProperty =
            DependencyProperty.Register("TreeMode", typeof(enumTreeMode?), typeof(FreeHierarchyTree),
                new PropertyMetadata(null, TreeModePropertyChangedCallback));

        public enumTreeMode? TreeMode
        {
            get { return GetValue(TreeModeProperty) as enumTreeMode?; }
            set { SetValue(TreeModeProperty, value); }
        }


        /// <summary>
        /// Наличие расширенного поиска
        /// </summary>
        //public static readonly DependencyProperty ShowUspdAndE422InTreeProperty = DependencyProperty.RegisterAttached(
        //    "ShowUspdAndE422InTree", typeof(bool), typeof(FreeHierarchyTree), new FrameworkPropertyMetadata(false, ShowUspdAndE422InTreeChangedCallback));

        //private bool _outerSelectorLoaded;
        //private Trees_ItemSelector _outerSelector;
        public bool ShowUspdAndE422InTree
        {
            //get { return (bool) GetValue(ShowUspdAndE422InTreeProperty); }
            //set { SetValue(ShowUspdAndE422InTreeProperty, value); }
            get; set;
        }

        private static void ShowUspdAndE422InTreeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dc = d as FreeHierarchyTree;
            if (dc != null)
            {
                var descr = dc.GetDescriptor();
                if (descr != null)
                {
                    descr.ShowUspdAndE422InTree = (bool)e.NewValue;
                }
            }
        }


        private static void TreeModePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dc = d as FreeHierarchyTree;
            if (dc != null)
            {
                dc.TreeMode = (enumTreeMode)e.NewValue;
            }
        }

        private ItemSelector _selector;
        public ItemSelector Selector
        {
            get
            {
                if (_selector != null) return _selector;

                _selector = new ItemSelector();
                return _selector;
            }
        }

        public SelectedNodesCollection SelectedNodes
        {
            get { return tree.SelectionSettings.SelectedNodes; }
        }

        /// <summary>
        /// Строим и загружаем дерево
        /// </summary>
        /// <param name="rightFilter">Фильтр для построителя</param>
        /// <param name="selectedNodes">Эти узлы выбираем после построения дерева</param>
        /// <param name="isDeclaratorMainTree">Это дерево для декларатора</param>
        /// <param name="freeHierarchyTypeTreeItem"></param>
        /// <param name="isFullReload">Полное перестроения дерева</param>
        /// <param name="afterLoad">Действие после загрузки</param>
        /// <param name="singreRootFreeHierItemIds">Список узлов с которых надо показывать дерево. Сами объекты будут в корне дерева</param>
        public void LoadTypes(EnumModuleFilter? rightFilter, HashSet<long> selectedNodes = null, bool isDeclaratorMainTree = false,
            FreeHierarchyTypeTreeItem freeHierarchyTypeTreeItem = null, bool isFullReload = false, HashSet<int> singreRootFreeHierItemIds = null)
        {
            _isDeclaratorMainTree = isDeclaratorMainTree;
            _selectedNodes = selectedNodes;
            _currentRightFilter = rightFilter;

            if (!Manager.IsDesignMode)
            {
                try
                {
                    ClearTypes();
                    var treeTypes = GlobalFreeHierarchyDictionary.GetTypes(rightFilter).Values
                            .Where(t => t.Parent == null)
                            .ToList();

                    hierarchyTypes.ItemsSource = treeTypes;

                    var oldDefaultTree_ID = DefaultTree_ID;
                    var module = FindModule(this);

                    if (InitialTree_ID != null) DefaultTree_ID = InitialTree_ID.Value;
                    else
                    {
                        int treeId;
                        if (module != null && Manager.Config != null && Manager.Config.TreeDefaults.TryGetValue(module.ModuleType + Name, out treeId))
                        {
                            DefaultTree_ID = treeId;
                        }
                    }

                    if (freeHierarchyTypeTreeItem == null)
                    {
                        ClearFounded();
                        founded = new Dictionary<object, Stack>();
                        FindBar.scanNode(founded, hierarchyTypes.ItemsSource, DefaultTree_ID.ToString(), new Stack(), true, isFindOnlySingle: true);
                        if (founded.Count == 1)
                        {
                            freeHierarchyTypeTreeItem = founded.Keys.First() as FreeHierarchyTypeTreeItem;
                        }
                        else
                        {
                            ClearFounded();
                            founded = new Dictionary<object, Stack>();
                            FindBar.scanNode(founded, hierarchyTypes.ItemsSource, oldDefaultTree_ID, new Stack(), true);
                            if (founded.Count == 1)
                            {
                                isFirst = true;
                                freeHierarchyTypeTreeItem = founded.Keys.First() as FreeHierarchyTypeTreeItem;
                            }
                            else isFirst = false;
                        }
                    }

                    _permissibleForSelectObjects = this.FindParent<IPermissibleForSelectObjects>();

                    LoadTreeAndInit(freeHierarchyTypeTreeItem, isFullReload, singreRootFreeHierItemIds);
                }
                catch (Exception)
                {
                    Manager.UI.ShowMessage("Не удалось загрузить типы свободных иерархий!");
                }
            }
        }

        public IModule FindModule(FrameworkElement fe)
        {
            if (fe == null) return null;

            var module = fe.FindParent<IModule>();
            if (module == null || module.ModuleType != ModuleType.None) return module;

            fe = module as FrameworkElement;
            if (fe == null) return null;

            return FindModule(fe.Parent as FrameworkElement);
        }


        private RangeObservableCollection<FreeHierarchyTreeItem> _items;
        public RangeObservableCollection<FreeHierarchyTreeItem> Items
        {
            get
            {
                //if (_items != null) return _items;

                //_items = tree.ItemsSource as SortedSet<FreeHierarchyTreeItem>;
                //return _items;

                return _items;
            }
            set
            {
                if (ReferenceEquals(_items, value)) return;

                tree.ItemsSource = _items = value;
            }
        }

        public IEnumerable NodeCollection 
        {
            get
            {
                if (tree == null) return null;

                return tree.Nodes;
            }
        }

        public void ExpandAndSelect(IFreeHierarchyObject hierObject, bool isExpandLast, bool isSelect = true)
        {
            //var hierObject = obj as IFreeHierarchyObject;
            if (hierObject == null) return;

            Select(new List<ID_TypeHierarchy> { hierObject.ToId() }, isExpandLast, isSelect);
        }

        public FreeHierarchyTreeItem GetNode(IFreeHierarchyObject hierarchyObject)
        {
            if (descriptor == null || descriptor.Tree == null || hierarchyObject == null) return null;

            return descriptor.Tree.Values.FirstOrDefault(t => t.HierObject != null && t.HierObject.Type == hierarchyObject.Type && t.HierObject.Id == hierarchyObject.Id);
        }

        private volatile bool _isPopupOpen;

        private void selectType_Click(object sender, RoutedEventArgs e)
        {
            if (_isPopupOpen)
            {
                _isPopupOpen = false;
                Manager.UI.CloseAllPopups();
                return;
            }

            popup.OpenAndRegister(false);
            if (founded != null && founded.Count == 1 && isFirst)
            {
                FreeHierarchyTree_OnLoaded(null, null);
                hierarchyTypes.ItemContainerGenerator.ExpandAndSelect(founded.Values.First().Clone() as Stack);
            }

            _isPopupOpen = true;
        }

        private void PopupOnClosed(object sender, EventArgs e)
        {
            if (!_isPopupOpen) return;
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(200);
                _isPopupOpen = false;
            });
        }

        public int? InitialTree_ID { get; set; }

        private FreeHierarchyTreeDescriptor _descriptor;
        private FreeHierarchyTreeDescriptor descriptor
        {
            get
            {
                return _descriptor;
            }
            set
            {
                _descriptor = value;
                if (TreeFindBar != null)
                {
                    TreeFindBar.Init(this, descriptor);
                }
            }
        }

        private bool isFirst = true;

        private DispatcherTimer _countTimer;
        private CancellationTokenSource cancellFindTokenSource = new CancellationTokenSource();

        public void DescOnSelectedChanged()
        {
            if (SelectedChanged == null) return;

            if (_countTimer == null)
            {
                _countTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(700), DispatcherPriority.Background, OnSelectedChangedCallback, Dispatcher);
            }

            _countTimer.Start();
        }

        private void OnSelectedChangedCallback(object sender, EventArgs e)
        {
            var timer = sender as DispatcherTimer;
            if (timer == null) return;

            timer.Stop();

            var s = selectedCount.Text;
            selectedCount.Text = "<подсчет...>";

            try
            {

                if (descriptor != null && showOnlySelected.IsChecked != null && showOnlySelected.IsChecked.Value)
                {
                    descriptor.HideUnselected();
                }

                if (SelectedChanged != null) SelectedChanged();
                else
                {
                    selectedCount.Text = s;
                }
            }
            catch
            {

            }
        }

        private void Tree_OnNodeExpansionChanged(object sender, NodeExpansionChangedEventArgs e)
        {
            if (e.Node != null)
            {
                var item = e.Node.Data as FreeHierarchyTreeItem;
                if (item != null)
                {
                    if (e.Node.IsExpanded) item.OnVisualExpanded(_isDeclaratorMainTree, e.Node);
                    else item.OnVisualCollapsed();

                    item.IsExpandedFromVisual = e.Node.IsExpanded;
                }
            }
        }

        private void butAll_Click(object sender, RoutedEventArgs e)
        {
            SelectAll();
        }

        public void SelectAll()
        {
            if (descriptor == null || Items == null) return;
            var items = Items.ToList();

            Task.Factory.StartNew(() =>
            descriptor.SelectUnselect(true, true, items));

            //this.SetRecursive(null, true, null, 0, 100, hierarchyTree: this);
            DescOnSelectedChanged();
        }

        private void butNone_Click(object sender, RoutedEventArgs e)
        {
            if (descriptor != null)
            {
                Task.Factory.StartNew(() =>
                descriptor.Unselect(null, true));

                //descriptor.IsUserClicked = false;
                //this.SetRecursive(null, false, null);
                //descriptor.IsUserClicked = true;
                DescOnSelectedChanged();
            }
        }

        private void ShowOnlySelected_OnClick(object sender, RoutedEventArgs e)
        {
            if (Items != null && descriptor != null && descriptor.Tree != null && showOnlySelected.IsChecked != null)
            {
                descriptor.ShowOnlySelected = showOnlySelected.IsChecked.Value;
                if (showOnlySelected.IsChecked.Value)
                    descriptor.HideUnselected();
                else
                    foreach (FreeHierarchyTreeItem item in descriptor.Tree.Values)
                        item.Visibility = Visibility.Visible;
            }
        }

        public event Action SelectedChanged;
        public event Action TreeChanged;

        private event Action _selectedChanged;
        public void SelectedChangedOff()
        {
            _selectedChanged = SelectedChanged;
            SelectedChanged = null;
        }
        public void SelectedChangedOn()
        {
            SelectedChanged = _selectedChanged;
            _selectedChanged = null;
        }

        public void LoadUANodes(FreeHierarchyTreeItem item)
        {
            if (item.FreeHierItemType != EnumFreeHierarchyItemType.UANode) return;

            var uaNode = item.HierObject as TUANode;
            if (uaNode == null || uaNode.DependentNodesChecked || uaNode.DependentNodes != null) return; //Подгружать не надо

            item.LoadDynamicChildren();
        }

        void select(object sender, bool isChecked, EnumFreeHierarchyItemType? itemType, int maxLevel = 100)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null) return;

            var item = frameworkElement.DataContext as FreeHierarchyTreeItem;
            if (item == null)
            {
                var xndc = frameworkElement.DataContext as XamDataTreeNodeDataContext;
                if (xndc != null)
                {
                    item = xndc.Data as FreeHierarchyTreeItem;
                }
            }

            if (item == null || item.Descriptor == null) return;

            Task.Factory.StartNew(() =>
            descriptor.SelectUnselect(isChecked, true, new List<FreeHierarchyTreeItem> { item }, itemType, maxLevel));

            if (item.Descriptor.ShowOnlySelected) item.Descriptor.HideUnselected();
            DescOnSelectedChanged();
        }

        private void SelectNodes_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var parameter = e.Parameter as string;

            if (parameter == null) return;
            switch (parameter)
            {
                case "all":
                    select(e.OriginalSource, true, null);
                    break;
                case "ti":
                    select(e.OriginalSource, true, EnumFreeHierarchyItemType.TI);
                    break;
                case "ps":
                    select(e.OriginalSource, true, EnumFreeHierarchyItemType.PS);
                    break;
                case "uspd":
                    select(e.OriginalSource, true, EnumFreeHierarchyItemType.USPD);
                    break;
                case "msk":
                    select(e.OriginalSource, true, EnumFreeHierarchyItemType.HierLev3);
                    break;
                case "onlyChild":
                    select(e.OriginalSource, true, null, 1);
                    break;
                case "unselectAll":
                    select(e.OriginalSource, false, null);
                    break;
                case "contract":
                    selectGroupTPObjects(EnumFreeHierarchyItemType.Contract, e.OriginalSource);
                    break;
                case "section":
                    selectGroupTPObjects(EnumFreeHierarchyItemType.Section, e.OriginalSource);
                    break;
                case "epu":
                    selectGroupTPObjects(EnumFreeHierarchyItemType.DirectConsumer, e.OriginalSource);
                    break;
                case "tp":
                    selectGroupTPObjects(EnumFreeHierarchyItemType.TP, e.OriginalSource);
                    break;
                case "expand2":
                    exp_col(e.OriginalSource, true, 2);
                    break;
                case "expand3":
                    exp_col(e.OriginalSource, true, 3);
                    break;
                case "expandAll":
                    exp_col(e.OriginalSource, true, 255);
                    break;
                case "unexpandAll":
                    exp_col(e.OriginalSource, false, 255);
                    break;
            }
        }

        public int? Tree_ID { get; set; }

        Dictionary<object, Stack> founded = null;

        private int defaultTree_ID = GlobalFreeHierarchyDictionary.TreeTypeStandartPS;
        public int DefaultTree_ID
        {
            get { return defaultTree_ID; }
            set { defaultTree_ID = value; }
        }

        /// <summary>
        /// Для отображения объектов подходящих только под этот фильтр
        /// </summary>
        public EnumTIStatus? FilterStatus { get; set; }
        public double? VoltageFilter
        {
            get
            {
                if (descriptor == null) return null;
                return descriptor.VoltageFilter;
            }
            set
            {
                if (descriptor != null)
                {
                    descriptor.VoltageFilter = value;
                }
            }
        }

        public bool IsShowTransformatorsAndReactors { get; set; }

        public void FreeHierarchyTree_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= FreeHierarchyTree_OnLoaded;
        }



        private FreeItemSelected GetOldFormatedItem(string serializedItem)
        {
            var indx = serializedItem.IndexOf("ti:");
            if (indx < 0) return null;

            var tiStrId = serializedItem.Substring(indx + 3);
            if (string.IsNullOrEmpty(tiStrId)) return null;

            int tiId;
            int.TryParse(tiStrId, out tiId);

            var ti = EnumClientServiceDictionary.TIHierarchyList[tiId];
            if (ti == null) return null;

            return new FreeItemSelected
            {
                ItemType = EnumFreeHierarchyItemType.PS,
                ItemID = ti.PS_ID.ToString(),
                ChildID = tiStrId,
                ChildType = EnumFreeHierarchyItemType.TI
            };
        }

        private int _isFirstSetSelected = 1;

        public int SetInterlockedFirstSelected(int value)
        {
            return Interlocked.Exchange(ref _isFirstSetSelected, value);
        }

        private void expandAll_Click(object sender, RoutedEventArgs e)
        {
            exp_col(sender, true, 255);
        }

        private void collapseAll_Click(object sender, RoutedEventArgs e)
        {
            exp_col(sender, false, 255);
        }

        void exp_col(object sender, bool isExpand, byte levels)
        {
            ExpColRecursive(tree.SelectionSettings.SelectedNodes[0], isExpand, levels);
        }

        private void ExpColRecursive(XamDataTreeNode node, bool isExpand, byte level)
        {
            if (node == null || level == 0) return;

            //var tempList = node.Nodes
            //   .Select(n => n.Data as FreeHierarchyTreeItem)
            //   .ToList();

            //if (tempList.Count > 0)
            //{
            //    FreeHierarchyTreePreparer.PrepareGlobalDictionaries(tempList, EnumFreeHierarchyTreePrepareMode.All);
            //}

            var uiThreadAction = (Action)(() =>
           {
               foreach (var child in node.Nodes)
               {
                   try
                   {
                       ExpColRecursive(child, isExpand, (byte)(level - 1));
                   }
                   catch (Exception ex)
                   {
                       Manager.UI.ShowMessage(ex.Message);
                   }
               }
           });

            var isuiThreadStarted = false;
            if (isExpand)
            {
                var treeItem = node.Data as FreeHierarchyTreeItem;
                if (treeItem != null)
                {
                    treeItem.LoadDynamicChildren(IsHideTi, node, uiThreadAction, isAsync: false, isHideTp: IsHideTp);
                    isuiThreadStarted = true;
                }
            }

            node.IsExpanded = isExpand;

            if (!isuiThreadStarted && node.Nodes.Count > 0) uiThreadAction();
        }

        private void expand2_Click(object sender, RoutedEventArgs e)
        {
            exp_col(sender, true, 2);
        }

        private void expand3_Click(object sender, RoutedEventArgs e)
        {
            exp_col(sender, true, 3);
        }

        public bool IsSelectSingle { get; set; }

        public bool IsHideSelectMany { get; set; }

        public bool IsHideTi { get; set; }

        public bool IsHideTp { get; set; }

        private void contracts_Click(object sender, RoutedEventArgs e)
        {
            selectGroupTPObjects(EnumFreeHierarchyItemType.Contract, sender);
        }

        private void sections_Click(object sender, RoutedEventArgs e)
        {
            selectGroupTPObjects(EnumFreeHierarchyItemType.Section, sender);
        }

        private void dirCons_Click(object sender, RoutedEventArgs e)
        {
            selectGroupTPObjects(EnumFreeHierarchyItemType.DirectConsumer, sender);
        }

        private void tps_Click(object sender, RoutedEventArgs e)
        {
            selectGroupTPObjects(EnumFreeHierarchyItemType.TP, sender);
        }

        void selectGroupTPObjects(EnumFreeHierarchyItemType type, object menu)
        {
            var frameworkElement = menu as FrameworkElement;
            if (frameworkElement == null) return;

            var item = frameworkElement.DataContext as FreeHierarchyTreeItem;
            if (item == null)
            {
                var xndc = frameworkElement.DataContext as XamDataTreeNodeDataContext;
                if (xndc != null)
                {
                    item = xndc.Data as FreeHierarchyTreeItem;
                }
            }

            if (item == null) return;

            if (item.FreeHierItemType == type)
            {
                if (PermissibleForSelectObjects != null)
                {
                    if (PermissibleForSelectObjects.PermissibleForSelectObjects.Any(p => p == item.FreeHierItemType))
                    {
                        item.IsSelected = true;
                    }
                }
                else
                {
                    item.IsSelected = true;
                }
            }

            descriptor.SelectUnselect(true, true, new List<FreeHierarchyTreeItem> { item }, type);

            //this.SetRecursive(item, true, type, 1, 100, hierarchyTree: this);

            if (item.Descriptor.ShowOnlySelected) item.Descriptor.HideUnselected();

            DescOnSelectedChanged();
        }

        
        public void SetCount(Func<int> result, string elementsName = "")
        {
            //if (_countTimer == null)
            //{
            //    _countTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(20), DispatcherPriority.Background, OnCountCallback, this.Dispatcher);
            //}

            //_countTimer.Tag = new TTreeCounter
            //{
            //    ElementsName = elementsName,
            //    Calculator = result,
            //};

            //_countTimer.Start();
        //}

        //private void OnCountCallback(object sender, EventArgs e)
        //{
        //    var timer = sender as DispatcherTimer;
        //    if (timer == null) return;

        //    timer.Stop();

            var uiContext = SynchronizationContext.Current;
            if (uiContext == null) return;

            //selectedCount.Text = "<подсчет...>";

            //var args = timer.Tag as TTreeCounter;
            //if (args == null) return;

            //timer.Tag = null;

            //args.Context = uiContext;

            Task.Factory.StartNew(() =>
            {

                string count = "<...>";
                try
                {
                    count = result.Invoke().ToString();
                }
                catch (Exception ex)
                {
                    count = ex.Message;
                }

                uiContext.Post(obj =>
                {
                    if (elementsName != "")
                    {
                        tbSelectedName.Text = "Выбрано " + elementsName + ":";
                    }
                    selectedCount.Text = count;
                }, null);
            });
        }

        private FreeHierarchyTreeItem _lastSelected;
        public FreeHierarchyTreeItem LastSelected
        {
            get { return _lastSelected; }
            set
            {
                _lastSelected = value;

                _PropertyChanged("LastSelected");

                //if (_lastSelected != null)
                //{
                //    _lastSelected.PropertyChangedCall("LastSelected.HierObject");
                //}
            }
        }

        private void Tree_OnSelectedNodesCollectionChanged(object sender, NodeSelectionEventArgs e)
        {
            if (IsSelectSingle)
            {
                if (ActiveTreeNode!=null)
                {
                    ActiveTreeNode.IsSelected = false;
                }

                foreach (var csn in e.CurrentSelectedNodes)
                {
                    LastSelected = csn.Data as FreeHierarchyTreeItem;
                    if (LastSelected != null)
                    {
                        LastSelected.IsSelected = true;
                    }

                }

                if (LastSelected != null && !Equals(LastSelected, ActiveTreeNode))
                {
                    ActiveTreeNode = LastSelected;
                    OnActiveNodeChanged();
                }
            }
        }

        public bool IsNSIDocuments { get; set; }

        /// <summary>
        /// исп в Описателе - выделенный мышкой узел
        /// </summary>
        public FreeHierarchyTreeItem ActiveTreeNode
        {
            get { return _activeTreeNode; }
            set
            {
                _activeTreeNode = value;
            }
        }




        public FreeHierarchyTypeTreeItem SelectedTree
        {
            get
            {
                return selectedType == null ? null : selectedType.DataContext as FreeHierarchyTypeTreeItem;
            }
        }

        /// <summary>
        /// SelectedItemChanged
        /// </summary>
        public event EventHandler ActiveNodeChanged;
        private void OnActiveNodeChanged()
        {
            if (ActiveNodeChanged != null)
            {
                ActiveNodeChanged(this, new EventArgs());
            }
        }

        public EventHandler<EventArgsTreeItems> OnTreeDataLoaded;

        //public event Action<SmallTI_TreeItem> SmallTiOnAddHaveCalendarDays;
        //public event Action<SmallTI_TreeItem> SmallTiOnAddHaveMonthValues;

        public void ClearFounded()
        {
            if (founded != null)
            {
                foreach (var f in founded.Keys)
                {
                    var item = f as FreeHierarchyTypeTreeItem;
                    if (item != null)
                    {
                        VisualEx.RemoveSourceFromValueChangedEventManager(item);
                        item.Dispose();
                    }
                }
                founded = null;
            }
        }

        public void ClearTypes()
        {
            hierarchyTypes.ClearItems();
        }

        public void Dispose()
        {
            try
            {
                _permissibleForSelectObjects = null;
                selectedType.DataContext = null;
                SelectedChanged = null;

                tree.Loaded -= ExpandOnLoaded;

                ClearNodes(tree.Nodes);
                Items = null;

                ClearTypes();

                if (descriptor != null)
                {
                    descriptor.Dispose();
                    descriptor = null;
                }

                ClearFounded();
                sets.Dispose();

                if (_startSelector != null)
                {
                    _startSelector.Dispose();
                }

                if (TreeFindBar != null)
                {
                    TreeFindBar.ClosePopup();
                    TreeFindBar.Dispose();
                    TreeFindBar = null;
                }
            }
            catch
            {
            }
        }

        private void ClearNodes(XamDataTreeNodesCollection nodesCollection)
        {
            foreach (var node in nodesCollection)
            {
                try
                {
                    ClearNodes(node.Nodes);
                    node.Nodes.Clear();

                    //if (node.Control != null) node.Control.DataContext = null;

                    //var nameProperty = typeof(XamDataTreeNode).GetProperty("Data");
                    //nameProperty.SetValue(node, null, null);
                }
                catch { }

                var freeHierarchyTreeItem = node.Data as FreeHierarchyTreeItem;
                if (freeHierarchyTreeItem != null)
                {
                    freeHierarchyTreeItem.Dispose();
                }
            }
        }

        public void Hide_SetsManager()
        {
            sets.Visibility = System.Windows.Visibility.Collapsed;
            butAll.Visibility = System.Windows.Visibility.Collapsed;
            butNone.Visibility = System.Windows.Visibility.Collapsed;
            showOnlySelected.Visibility = System.Windows.Visibility.Collapsed;
            tbSelectedName.Visibility = System.Windows.Visibility.Collapsed;
            selectedCount.Visibility = System.Windows.Visibility.Collapsed;
            //tbox.BorderThickness = new Thickness(0);
            //tbox.Margin = new Thickness(0, 8, 0, 0);
        }


        /// <summary>
        /// запрещаем смену дерева - используется в описателе
        /// </summary>
        /// <param name="IsDisabled"></param>
        public void Disable_SelectTree(bool IsDisabled)
        {
            if (IsDisabled)
                selectType.IsEnabled = false;
            else
                selectType.IsEnabled = true;

            InitialTree_ID = DefaultTree_ID;
        }


        /// <summary>
        /// отображаем кнопку редактирования структуры, используется в описателе
        /// </summary>
        public void Show_ButtonsEditStructure()
        {
            checkBoxEditStructure.Visibility = System.Windows.Visibility.Visible;
            tbox.BorderThickness = new Thickness(1);
            tbox.Margin = new Thickness(5, 5, 5, 5);
            //tree.Margin = new Thickness(3, -11, -2, 0);
        }

        /// <summary>
        /// OnEditStructure
        /// </summary>
        public event EventHandler EditStructure;
        private void OnEditStructure()
        {
            if (EditStructure != null)
            {
                EditStructure(this, new EventArgs());
            }
        }


        /// <summary>
        /// обновление узлов дерева 
        /// </summary>
        /// <param name="TypeUserAction">0 - Добавление, 1 - редактирование, 2 - удаление </param>
        /// <param name="freeHierarchyTreeItem">объект</param>
        /// <param name="oldParentNode">текущий родитель</param>
        /// <param name="newParentNode">новый родитель=oldParent если не изменялся</param>
        /// <param name="newParentStandartObject">для стандартных деревьев - родитель выбранный из другого дерева, будем его находить по ИД и типу в текущем, для свободных - не нужен</param>
        /// <param name="SelectAfterEdit"></param>
        public void UpdateAfterNodeEdit(byte TypeUserAction, FreeHierarchyTreeItem freeHierarchyTreeItem,
            FreeHierarchyTreeItem newParentNode, FreeHierarchyTreeItem oldParentNode,
            FreeHierarchyTreeItem newParentStandartObject, bool SelectAfterEdit = true)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    if (freeHierarchyTreeItem == null)
                        return;

                    if (descriptor == null)
                        descriptor = new FreeHierarchyTreeDescriptor()
                        {
                            FreeHierarchyTree = this,
                        };

                    if (descriptor.Tree == null)
                        descriptor.Tree = new ConcurrentDictionary<int, FreeHierarchyTreeItem>();

                    freeHierarchyTreeItem.Descriptor = descriptor;

                    //считаем что для стандартных объектов новый род объект это newParentStandartObject
                    //т.к. для сечений например отображаются не все объекты-родители и их придется выбирать из обычного дерева
                    if (SelectedTree != null && SelectedTree.FreeHierTree_ID < 0 &&
                        SelectedTree.FreeHierTree_ID > -1001)
                    {

                        if (newParentStandartObject != null && newParentStandartObject.HierObject != null)
                        {
                            switch (freeHierarchyTreeItem.FreeHierItemType)
                            {
                                case EnumFreeHierarchyItemType.HierLev2:
                                    newParentNode = descriptor.Tree.Values
                                        .FirstOrDefault(i => i.FreeHierItemType == EnumFreeHierarchyItemType.HierLev1
                                                             && newParentStandartObject.FreeHierItemType ==
                                                             EnumFreeHierarchyItemType.HierLev1
                                                             && i.HierObject != null
                                                             && i.HierObject.Id == newParentStandartObject.HierObject.Id
                                        );
                                    break;
                                case EnumFreeHierarchyItemType.HierLev3:
                                    newParentNode = descriptor.Tree.Values
                                        .FirstOrDefault(i => i.FreeHierItemType == EnumFreeHierarchyItemType.HierLev2
                                                             && newParentStandartObject.FreeHierItemType ==
                                                             EnumFreeHierarchyItemType.HierLev2
                                                             && i.HierObject != null
                                                             && i.HierObject.Id ==
                                                             newParentStandartObject.HierObject.Id);
                                    break;
                                case EnumFreeHierarchyItemType.PS:
                                    newParentNode = descriptor.Tree.Values
                                        .FirstOrDefault(i => i.FreeHierItemType == EnumFreeHierarchyItemType.HierLev3
                                                             && newParentStandartObject.FreeHierItemType ==
                                                             EnumFreeHierarchyItemType.HierLev3
                                                             && i.HierObject != null
                                                             && i.HierObject.Id ==
                                                             newParentStandartObject.HierObject.Id);
                                    break;
                                case EnumFreeHierarchyItemType.Section:
                                    newParentNode = descriptor.Tree.Values
                                        .FirstOrDefault(i =>
                                            i.FreeHierItemType == newParentStandartObject.FreeHierItemType
                                            && i.HierObject != null
                                            && i.HierObject.Id == newParentStandartObject.HierObject.Id);
                                    break;
                            }
                        }


                        //если нового родителя нет в текщем дереве - добвляем его и его родителей(newParentStandartObject!=null а newParentNode=null)     
                        if (newParentStandartObject != null && newParentNode == null)
                        {
                            if (newParentStandartObject.Children != null)
                                newParentStandartObject.Children.Clear();

                            //список узлов родитель и его родители из дерева родителя
                            Dictionary<int, FreeHierarchyTreeItem> nodes = new Dictionary<int, FreeHierarchyTreeItem>();
                            nodes.Add(newParentStandartObject.Getlevel(), newParentStandartObject);
                            FreeHierarchyTreeItem parent = newParentStandartObject.Parent;
                            while (parent != null)
                            {
                                //берем только узлы с объектами
                                if (parent.HierObject == null)
                                    continue;

                                //берем только ур1-4 - могут выбрать родителя из своб иерархии где parent - любой узел
                                //либо искать родителя по parent.HierObject.Parent...
                                if (parent.FreeHierItemType == EnumFreeHierarchyItemType.HierLev1
                                    || parent.FreeHierItemType == EnumFreeHierarchyItemType.HierLev2
                                    || parent.FreeHierItemType == EnumFreeHierarchyItemType.HierLev3
                                    || parent.FreeHierItemType == EnumFreeHierarchyItemType.PS)
                                {
                                    nodes.Add(parent.Getlevel(), parent);
                                    parent = parent.Parent;
                                }
                            }

                            //цикл по списку новых родит узлов - если есть в дереве - оставляем, если нет - добавляем
                            foreach (var parentnode in nodes.OrderBy(i => i.Key).Select(i => i.Value))
                            {

                                parentnode.ClearChildren();

                                var nodeInTree = descriptor.Tree.Values.FirstOrDefault(i =>
                                    i.FreeHierItemType == parentnode.FreeHierItemType && i.HierObject != null &&
                                    i.HierObject.Id == parentnode.HierObject.Id);

                                if (nodeInTree == null)
                                {
                                    UpdateAfterNodeEdit(0, parentnode, parentnode.Parent, parentnode.Parent,
                                        parentnode.Parent, false);
                                    newParentNode = descriptor.Tree.Values
                                        .FirstOrDefault(i => i.FreeHierItemType == parentnode.FreeHierItemType
                                                             && i.HierObject != null
                                                             && i.HierObject.Id == parentnode.HierObject.Id);
                                }

                            }
                        }
                    }


                    //ищем объект в словаре
                    FreeHierarchyTreeItem item = null;
                    if (descriptor != null && descriptor.Tree != null)
                        descriptor.Tree.TryGetValue(freeHierarchyTreeItem.FreeHierItem_ID, out item);


                    if (item != null && (TypeUserAction == (byte)1 || TypeUserAction == (byte)2))
                    {
                        //редактирование
                        if (TypeUserAction == (byte)1)
                        {
                            freeHierarchyTreeItem.UpdateVisual();

                            #region  если ключи родителя и старого родителя отличаются то перенос

                            int oldparet_ID = 0;
                            int newParent_ID = 0;

                            oldparet_ID = oldParentNode == null ? 0 : (int)oldParentNode.FreeHierItem_ID;
                            newParent_ID = newParentNode == null ? 0 : (int)newParentNode.FreeHierItem_ID;

                            if (oldparet_ID != newParent_ID)
                            {
                                //1) удаляем из старого места
                                if (oldparet_ID == 0)
                                {
                                    Items.Remove(freeHierarchyTreeItem);
                                }
                                else
                                {
                                    descriptor.Tree.TryGetValue(oldparet_ID, out oldParentNode);
                                    if (oldParentNode != null)
                                        oldParentNode.RemoveChildren(freeHierarchyTreeItem);
                                }

                                //2) добавляем в новое место в дереве
                                if (newParent_ID == 0)
                                {
                                    Items.Add(freeHierarchyTreeItem);
                                }
                                else
                                {
                                    descriptor.Tree.TryGetValue(newParent_ID, out newParentNode);
                                    if (newParentNode != null)
                                        newParentNode.AddChildren(freeHierarchyTreeItem);
                                }

                                SelectAfterEdit = true;

                                freeHierarchyTreeItem.Parent = newParentNode;
                            }

                            #endregion
                        }
                        //удаление
                        else if (TypeUserAction == (byte)2)
                        {
                            if (oldParentNode == null)
                            {
                                Items.Remove(freeHierarchyTreeItem);

                                //и все дочерние надо удалить
                                HashSet<int> items = new HashSet<int>();
                                GetChildrenList(freeHierarchyTreeItem, items);
                                items.Add(freeHierarchyTreeItem.FreeHierItem_ID);

                                foreach (var child in items)
                                {
                                    FreeHierarchyTreeItem removed;
                                    descriptor.Tree.TryRemove(child, out removed);
                                }

                                freeHierarchyTreeItem.Dispose();

                            }
                            else
                            {
                                descriptor.Tree.TryGetValue(freeHierarchyTreeItem.FreeHierItem_ID,
                                    out freeHierarchyTreeItem);
                                if (freeHierarchyTreeItem != null)
                                {
                                    //и все дочерние надо удалить
                                    HashSet<int> items = new HashSet<int>();
                                    GetChildrenList(freeHierarchyTreeItem, items);
                                    items.Add(freeHierarchyTreeItem.FreeHierItem_ID);

                                    foreach (var child in items)
                                    {
                                        FreeHierarchyTreeItem removed;
                                        descriptor.Tree.TryRemove(child, out removed);
                                    }

                                    if (oldParentNode != null)
                                        oldParentNode.RemoveChildren(freeHierarchyTreeItem);

                                    freeHierarchyTreeItem.Dispose();
                                }

                            }
                        }
                    }


                    //добавление
                    else if (freeHierarchyTreeItem != null && TypeUserAction == (byte)0)
                    {
                        if (SelectedTree != null && SelectedTree.FreeHierTree_ID < 0 && freeHierarchyTreeItem != null &&
                            freeHierarchyTreeItem.FreeHierItem_ID == 0)
                        {
                            if (descriptor.Tree.Any())
                            {
                                freeHierarchyTreeItem.FreeHierItem_ID = descriptor.GetMinIdAndDecrement();
                            }
                        }

                        //находим родит объект
                        if (newParentNode != null)
                        {
                            descriptor.Tree.TryGetValue(newParentNode.FreeHierItem_ID, out newParentNode);
                        }

                        //добавляем новый узел на родит объект или в корень
                        if (newParentNode == null)
                        {
                            if (!Items.Any(i =>
                                i.FreeHierItem_ID == freeHierarchyTreeItem.FreeHierItem_ID &&
                                i.FreeHierItemType == freeHierarchyTreeItem.FreeHierItemType))
                                Items.Add(freeHierarchyTreeItem);

                            if (!descriptor.Tree.Any(i => i.Key == freeHierarchyTreeItem.FreeHierItem_ID))
                            {
                                descriptor.Tree.TryAdd(freeHierarchyTreeItem.FreeHierItem_ID, freeHierarchyTreeItem);
                            }

                            freeHierarchyTreeItem.Parent = null;
                        }
                        else
                        {
                            if (!descriptor.Tree.Any(i => i.Key == freeHierarchyTreeItem.FreeHierItem_ID))
                            {
                                descriptor.Tree.TryAdd(freeHierarchyTreeItem.FreeHierItem_ID, freeHierarchyTreeItem);
                            }
                            newParentNode.AddChildren(freeHierarchyTreeItem);
                            freeHierarchyTreeItem.Parent = newParentNode;
                        }
                    }


                    //выделяем узел
                    if (SelectAfterEdit)
                    {
                        tree.SelectionSettings.SelectedNodes.Clear();

                        if (descriptor != null && descriptor.Tree != null)
                            descriptor.Tree.TryGetValue(freeHierarchyTreeItem.FreeHierItem_ID, out item);

                        if (item != null && TypeUserAction != 2)
                        {
                            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new System.Action(delegate ()
                            {
                                try
                                {
                                    //var pathToFounded = new ConcurrentStack<object>();
                                    ////Ищем только объекты где есть override Equals
                                    //var foundedObject = FinderHelper.FindFirstElementAsync(
                                    //    tree.ItemsSource as IEnumerable<object>, freeHierarchyTreeItem, pathToFounded);
                                    //if (foundedObject != null && pathToFounded.Count > 0)
                                    //{
                                    //    tree.ExpandAndSelectXamTreeSync(pathToFounded, false);
                                    //    FindBar.MoveUp(tree);
                                    //}
                                }
                                catch
                                {
                                    //ошибки бывают..
                                }

                                ActiveTreeNode = item;
                                OnActiveNodeChanged();

                            }));

                        }
                        //если было удаление выделяем родителя
                        else if (oldParentNode != null)
                        {
                            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new System.Action(delegate ()
                            {
                                try
                                {
                                    //var fnd = new Dictionary<object, Stack>();
                                    //var obj = oldParentNode.ItemObject;
                                    //fnd[obj] = oldParentNode.GetParents();
                                    //FindBar.ExpandAndSelectXamTree(fnd, obj, tree);

                                    var pathToFounded = new ConcurrentStack<object>();
                                    //Ищем только объекты где есть override Equals
                                    var foundedObject = FinderHelper.FindFirstElementAsync(
                                        tree.ItemsSource as IEnumerable<object>, oldParentNode, pathToFounded);
                                    if (foundedObject != null && pathToFounded.Count > 0)
                                    {
                                        tree.ExpandAndSelectXamTreeSync(pathToFounded, false);
                                        FindBar.MoveUp(tree);
                                    }
                                }
                                catch
                                {
                                    //ошибки бывают..
                                }
                                ActiveTreeNode = oldParentNode;
                                OnActiveNodeChanged();
                            }));

                        }
                        else
                        {
                            ActiveTreeNode = item;
                            OnActiveNodeChanged();
                        }
                    }
                }
                catch
                //(Exception ex)
                {
                    //throw new Exception(string.Format("Ошибка в редакторе структуры {0}", ex.Message));
                }
            }), DispatcherPriority.Background);
        }

        private void GetChildrenList(FreeHierarchyTreeItem item, HashSet<int> items)
        {
            foreach (var child in item.Children)
            {
                items.Add(child.FreeHierItem_ID);
                GetChildrenList(child, items);
            }
        }

        public FreeHierarchyTreeDescriptor GetDescriptor()
        {
            return descriptor;
        }

        /// <summary>
        /// Выбор нового дерева или перезагрузка старого
        /// <param name="freeHierTreeId">Идентификатор дерева, которое нужно выбрать (если это выбор нового дерева)</param>
        /// <param name="afterload">Действие, которое надо выполнить после всего</param>
        /// <param name="hierarchyObject">Объект на который надо позиционировать дерево</param>
        /// </summary>
        public void ReloadTree(int? freeHierTreeId = null, Action afterload = null, IFreeHierarchyObject hierarchyObject = null)
        {
            if (descriptor!=null && Manager.User!=null)
            {
                descriptor.UpdateIncludedObjectChildrenAsync(Manager.User.User_ID);
            }

            if (hierarchyObject != null && tree.SelectedDataItems != null)
            {
                if (tree.SelectedDataItems.Any(selectedItem =>
                {
                    var fh = selectedItem as FreeHierarchyTreeItem;
                    if (fh == null) return false;

                    return Equals(fh.HierObject, hierarchyObject);
                })) return; //Объект уже выбран, больше ничего делать не надо

                if (SelectedTree != null && freeHierTreeId.HasValue && SelectedTree.FreeHierTree_ID == freeHierTreeId || !freeHierTreeId.HasValue)
                {
                    //Позиционируем на заданном объекте
                    Dispatcher.BeginInvoke(new System.Action(delegate
                    {
                        ExpandAndSelect(hierarchyObject, true);
                    }), DispatcherPriority.Loaded);

                    return;
                }
            }

            //Запоминаем выбранные объекты
            //HashSet<string> checkedList = null;
            //short versionNumber = 0;
            //if (!freeHierTreeId.HasValue)
            //{
            //   var checkedList = GetSelected();
            //}

            //Очищаем старое дерево, чтобы небыло утечек памяти
            //if (descriptor != null)
            //{
            //    descriptor.SelectedChanged -= DescOnSelectedChanged;
            //    descriptor.Dispose();
            //    descriptor = null;
            //}

            //if (Items != null && Items.Count > 0)
            //{
            //    Items.DisposeChildren();
            //    Items.Clear();
            //}

            selectedCount.Text = "0";

            //перезагружаем список доступных деревьев
            Manager.User.ReloadFreeHierarchyTypes(Manager.UI.ShowMessage);

            var freeHierarchyTypeTreeItem = SelectedTree;

            //Выбрать новое дерево
            if (freeHierTreeId.HasValue
                && (freeHierarchyTypeTreeItem == null || freeHierarchyTypeTreeItem.FreeHierTree_ID != freeHierTreeId))
            {
                var hierarchyTypeTreeItems = hierarchyTypes.ItemsSource as List<FreeHierarchyTypeTreeItem>;
                if (hierarchyTypeTreeItems == null) return;

                freeHierarchyTypeTreeItem = null;
                foreach (var item in hierarchyTypeTreeItems)
                {
                    freeHierarchyTypeTreeItem = item.FindById(freeHierTreeId.Value);
                    if (freeHierarchyTypeTreeItem != null) break;
                }

                if (freeHierarchyTypeTreeItem != null)
                {
                    freeHierarchyTypeTreeItem.IsSelected = true;
                    selectedType.DataContext = freeHierarchyTypeTreeItem;
                }
            }

            //перезагрузка словаря сечений т.к. мугт быть изменения на других компьютерах, нужно чтобы все обновлялось
            //также нужно остальные словари здесь перегружать полностью
            if (hierarchyObject == null)
            {
                //Если это не переход на выбранный объект
                if (freeHierarchyTypeTreeItem != null
                    && (freeHierarchyTypeTreeItem.FreeHierTree_ID == GlobalFreeHierarchyDictionary.TreeTypeStandartGroupTP
                    || freeHierarchyTypeTreeItem.FreeHierTree_ID == GlobalFreeHierarchyDictionary.TreeTypeStandartSections
                    || freeHierarchyTypeTreeItem.FreeHierTree_ID == GlobalFreeHierarchyDictionary.TreeTypeStandartSectionsNSI))
                {
                    GlobalSectionsDictionary.FillSections();
                    GlobalSectionsDictionary.SectionsList = new XDictionary<int, TSection>(EnumClientServiceDictionary.GetSections, "SectionsList");
                }

                UAHierarchyDictionaries.ResetDicts();
                GlobalTreeDictionary.DictNodeIconList = null;

                GlobalTreeDictionary.DictOldTelescopeNodeIconList = null;
            }

            //SetInitialAsync(checkedList, versionNumber);

            //Подгружаем список деревьев, выделяем то, которое нужно, запускаем задачу на загрузку
            LoadTypes(_currentRightFilter, null, _isDeclaratorMainTree, freeHierarchyTypeTreeItem, true);


            if (hierarchyObject == null) return;

            //Позиционируем на заданном объекте
            Dispatcher.BeginInvoke(new System.Action(delegate
            {
                ExpandAndSelect(hierarchyObject, true);
            }), DispatcherPriority.Loaded);
        }

        //public void ExpandAndSelect(IFreeHierarchyObject freeHierarchyTreeItem, bool isSelect = true)
        //{
        //    ExpandAndSelect(freeHierarchyTreeItem, false, isSelect);
        //}

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            ReloadTree();
        }

        public bool IsStructureEdit = false;
        private FreeHierarchyTreeItem _activeTreeNode;

        private void checkBoxEditStructure_Checked(object sender, RoutedEventArgs e)
        {
            IsStructureEdit = checkBoxEditStructure.IsChecked ?? false;
            // Включаем DragAndDrop 

            this.tree.IsDraggable = IsStructureEdit;
            this.tree.IsDropTarget = IsStructureEdit;
            this.tree.AllowDragDropCopy = false;
            //this.tree.AllowDrop = IsStructureEdit;


            OnEditStructure();
        }

        public void CloseTreeEditor()
        {
            checkBoxEditStructure.IsChecked = false;
        }

        public bool SelectedNodeEqualFreeHierarchyObject(IFreeHierarchyObject hierarchyObject)
        {
            if (hierarchyObject == null || tree.SelectedDataItems == null) return false;

            var item = tree.SelectedDataItems.ElementAtOrDefault(0) as FreeHierarchyTreeItem;
            if (item == null || item.HierObject == null) return false;

            return item.HierObject.Id == hierarchyObject.Id && item.HierObject.Type == hierarchyObject.Type;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void _PropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }



        #endregion

        private void UIElementOnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsLoaded) return;

            var fe = sender as FrameworkElement;
            if (fe == null) return;

            var treeItem = fe.DataContext as FreeHierarchyTypeTreeItem;
            if (treeItem == null) return;

            treeItem.IsSelected = true;

            Manager.UI.CloseAllPopups(popup);
            _isPopupOpen = false;
            LoadTreeAndInit(treeItem);

            MainLayout.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
           {
               if (TreeChanged != null) TreeChanged();
           }));

            isFirst = false;
            showOnlySelected.IsChecked = false;

            ActiveTreeNode = null;
            OnActiveNodeChanged();
            DescOnSelectedChanged();
        }

        private void Tree_OnNodeDragDrop(object sender, TreeDropEventArgs e)
        {
            try
            {
                if (sender as FreeHierarchyTreeItem != null)
                {


                    e.Handled = true;

                }

                var xamDataTreeNodeSource = e.DragDropEventArgs.Data as XamDataTreeNode;
                if (xamDataTreeNodeSource != null)
                {
                    var fhi = xamDataTreeNodeSource.Data as FreeHierarchyTreeItem;
                    if (fhi != null)
                    {
                        var xamDataTreeNodeControl = e.DragDropEventArgs.DropTarget as XamDataTreeNodeControl;
                        if (xamDataTreeNodeControl != null)
                        {
                            var xamDataTreeNodeDataContext = xamDataTreeNodeControl.DataContext as
                                XamDataTreeNodeDataContext;
                            if (xamDataTreeNodeDataContext != null)
                            {
                                var freeHierarchyTreeItemTarget =
                                    xamDataTreeNodeDataContext.Data as FreeHierarchyTreeItem;
                                if (freeHierarchyTreeItemTarget != null && ((fhi.Parent !=
                                                                             freeHierarchyTreeItemTarget.Parent)))
                                {
                                    e.Handled = true;
                                    Manager.UI.ShowMessage("Можно изменять порядок в рамках только на одного уровня.");
                                    return;
                                }
                                // Все хорошо можно узнавать порядок
                                //freeHierarchyTreeItemTarget.Parent.Children.
                                _dragItemOldIndex = xamDataTreeNodeSource.Index;


                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Manager.UI.ShowMessage(exception.ToString());
            }







        }

        private int? _dragItemOldIndex;
        private int? _dragItemNewIndex;

        private void Tree_OnNodeDraggingStart(object sender, DragDropStartEventArgs e)
        {

            _dragItemOldIndex = null;
            _dragItemNewIndex = null;
            // Пока только для FH и с нажатым Ctrl
            if (SelectedTree.FreeHierTree_ID < 0 || !Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                e.Cancel = true;
                return;


            }




        }

        private void Tree_OnNodeDragEnd(object sender, DragDropEventArgs e)
        {

            try
            {
                var xamDataTreeNode = e.Data as XamDataTreeNode;
                if (xamDataTreeNode != null)
                {
                    _dragItemNewIndex = xamDataTreeNode.Index;
                    var item = xamDataTreeNode.Data as FreeHierarchyTreeItem;
                    if (item != null)
                    {
                        int? itemParentid = null;
                        if (item.Parent != null)
                        {
                            itemParentid = item.Parent.FreeHierItem_ID;
                        }
                        item.SetTreeItemSortNumber(this.Tree_ID.GetValueOrDefault(), item.FreeHierItem_ID, xamDataTreeNode.Index + 1, itemParentid);


                    }
                }


                if (e.DropTarget != null)
                {

                    return;
                }
            }
            catch (Exception exception)
            {
                Manager.UI.ShowMessage(exception.ToString());
            }

        }

        private void ButCollapseAllOnClick(object sender, RoutedEventArgs e)
        {
            foreach (var node in tree.Nodes)
            {
                ExpColRecursive(node, false, 4);
            }
        }

        //TODO прокрутка по объекту
        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var xamTree = sender as XamDataTree;
            if (xamTree == null) return;

            var sv = xamTree.FindLogicalChild("VerticalScrollBar") as ScrollBar;
            if (sv == null) return;

            sv.SmallChange = 2; //Чем больше, тем быстрее будет прокрутка

            if (e.Delta > 0)
            {
                ScrollBar.LineUpCommand.Execute(null, sv);
            }
            if (e.Delta < 0)
            {
                ScrollBar.LineDownCommand.Execute(null, sv);
            }
            e.Handled = true;
        }

        #region Работа с набором

        private void SetChanged(object set, short versionNumber)
        {
            FreeHierarchySelectedInfo selectedInfo = null;

            if (versionNumber < 3)
            {
                //Старая версия работы с набором
                var initial = SetChanged1(set as HashSet<string>, versionNumber);
                //return;

                int treeId;
                if (descriptor != null)
                {
                    treeId = descriptor.Tree_ID.GetValueOrDefault();
                }
                else
                {
                    treeId = 0;
                }

                //TODO возможны вариации, если потребуется нужно дорабатывать
                selectedInfo = new FreeHierarchySelectedInfo
                {
                    User_ID = Manager.User.User_ID,
                    Items = initial
                    .Select(i => new FreeHierarchySelectedItem
                    {
                        Id = i.ToId(),
                        IsSelect = true,
                    })
                    .ToList(),
                    FreeHierTree_ID = treeId,
                };
            }
            else
            {
                //Новая версия работы с набором
                var list = set as string;
                if (string.IsNullOrEmpty(list))
                {
                    //Убираем выделенные
                    var descriptor = GetDescriptor();
                    if (descriptor != null)
                    {
                        descriptor.Unselect(null, true);
                        DescOnSelectedChanged();
                    }
                    return;
                }

                try
                {
                    selectedInfo = ProtoHelper.ProtoDeserializeFromString<FreeHierarchySelectedInfo>(list);
                }
                catch (Exception ex)
                {
                    ex.ShowMessage();
                }
            }

            if (selectedInfo == null || selectedInfo.Items == null || !selectedInfo.Items.Any()) return;

            Manager.UI.ShowWaitPanelStatic(tree, TimeSpan.FromSeconds(2));

            //Пока просто выделяем выбранные, нужно, наверное, через крутилку
            Task.Factory.StartNew(() => XamTreeFinder.BuildPathFromSQL(selectedInfo
                .Items
                //.Take(limit)
                .Where(s => s.Id != null && s.IsSelect.GetValueOrDefault())
                .Select(s => s.Id).ToList(), Tree_ID))
                .ContinueWith(res =>
                {
                    Manager.UI.HideWaitPanelStatic(tree);

                    var objectsFromSQL = res.Result;
                    OnBuildPathFromSQL(objectsFromSQL, true);
                });
        }

        private List<FreeItemSelected> SetChanged1(HashSet<string> set, short versionNumber)
        {
            if (set == null) return null;

            //_tiSelected = new HashSet<string>();
            var parentPsIds = new HashSet<int>();
            var tis = new HashSet<string>();
            bool? isOldSerilized = null;

            return set.Select(i =>
            {
                if (string.IsNullOrEmpty(i)) return null;

                FreeItemSelected item;

                try
                {
                    if (versionNumber == 1)
                    {
                        item = ProtoHelper.ProtoDeserializeFromString<FreeItemSelected>(i);
                    }
                    else
                    {
                        if (!isOldSerilized.HasValue)
                        {
                            var indx = i.IndexOf("ti:");
                            isOldSerilized = (indx >= 0 && indx <= 3);
                        }

                        if (isOldSerilized.Value) item = GetOldFormatedItem(i);
                        else item = CommonEx.DeserializeFromString<FreeItemSelected>(i);

                        if (item == null)
                        {
                            try
                            {
                                //Возможно ошибка с версией, пробуем proto
                                item = ProtoHelper.ProtoDeserializeFromString<FreeItemSelected>(i);
                                //Получилось прочитать по-новому, меняем версию
                                versionNumber = 1;
                            }
                            catch { }
                        }
                    }
                }
                catch
                {
                    item = null;
                }

                return item;
            })
                .Where(i => i != null)
                .ToList();

            //Старый метода выделения не эффективет, требует перебора всего дерева, нужно использовать только новый вариант
            //var s = SelectedChanged;
            //SelectedChanged = null;
            ////FreeHierarchyTreeItem firstSelected = null;
            //SetInterlockedFirstSelected(1);

            //this.PrepareAndSelectCollection(initial, tree.ItemsSource as ICollection<FreeHierarchyTreeItem>);

            //SelectedChanged = s;

            //DescOnSelectedChanged();
        }

        #endregion

        #region Возвращаем выбранные объекты

        public List<ID_TypeHierarchy> GetSelectedIds(Func<FreeHierarchyTreeItem, bool> aditionalWherePredicate = null)
        {
            var result = new List<ID_TypeHierarchy>();
            if (descriptor == null) return result;

            HashSet<EnumFreeHierarchyItemType> permissibleForSelectObjects = null;

            if (PermissibleForSelectObjects != null)
            {
                permissibleForSelectObjects = PermissibleForSelectObjects.PermissibleForSelectObjects;
            }

            var selected = new List<FreeHierarchyTreeItem>();
            descriptor.GetSelected(selected, permissibleForSelectObjects, aditionalWherePredicate);

            result.AddRange(selected.AsTypeHierarchy());

            return result;

            //return tree.GetSelected(filter);
        }

        /// <summary>
        /// Получить Выбранные узлы
        /// </summary>
        /// <returns></returns>
        public List<FreeHierarchyTreeItem> GetSelected(bool isGetSelected = true)
        {
            var allItems = new List<FreeHierarchyTreeItem>();

            if (descriptor != null)
            {
                //Новый метод возврата выбранных 
                descriptor.GetSelected(allItems);
            }
            else
            {
                //Выбираем по старинке
                foreach (var forItem in Items)
                {
                    if (forItem.IsSelected) allItems.Add(forItem);
                    allItems.AddRange(forItem.Descendants().Where(f => f.IsSelected));
                }
            }

            if (isGetSelected && tree.SelectedDataItems != null)
            {
                allItems.AddRange(tree.SelectedDataItems.OfType<FreeHierarchyTreeItem>());
            }

            return allItems;
        }

        /// <summary>
        /// Возвращаем proto сериализированную коллекцию выбранных объектов
        /// </summary>
        /// <param name="tree">Дерево</param>
        /// <param name="typesForSelection">Типы для выбора</param>
        /// <returns>proto сериализированную коллекцию выбранных объектов</returns>
        public string GetSelectedToSet(HashSet<EnumFreeHierarchyItemType> typesForSelection = null)
        {
            if (descriptor == null) return null;

            var allItems = new List<FreeHierarchyTreeItem>();

#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            //Новый метод возврата выбранных 
            descriptor.GetSelected(allItems, typesForSelection);
            var items = new List<FreeHierarchySelectedItem>();

            //Пока просто сериализуем и сохраняем выбранные
            foreach (var item in allItems)
            {
                items.Add(new FreeHierarchySelectedItem
                {
                    Id = item.AsTypeHierarchy(),
                    IsSelect = true,
                    Parent = item.Parent.AsTypeHierarchy(),

                });
            }

#if DEBUG
            sw.Stop();
            Console.WriteLine("GetSelectedToSet, GetSelected {0} млс", sw.ElapsedMilliseconds);
            sw.Restart();
#endif

            var selectedInfoSerialized = ProtoHelper.ProtoSerializeToString(new FreeHierarchySelectedInfo
            {
                FreeHierTree_ID = descriptor.Tree_ID ?? -101,
                User_ID = Manager.User.User_ID,
                Items = items,
            });

#if DEBUG
            sw.Stop();
            Console.WriteLine("GetSelectedToSet, ProtoSerializeToString {0} млс", sw.ElapsedMilliseconds);
#endif

            return selectedInfoSerialized;
        }

        // <summary>
        /// В одном потоке разворачиваем объекты (без подгрузки), возвращаем выбранный объект
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="chain"></param>
        /// <param name="isExpandLast"></param>
        /// <param name="isSelect"></param>
        /// <returns></returns>
        public void ExpandFromSQL(IEnumerable nodeCollection,
            ConcurrentStack<ID_TypeHierarchy> path,
            bool isExpandLast, bool isSelect = true)
        {
            if (path == null || path.Count == 0) return;

            XamDataTreeNode result = null;
            if (isSelect)
            {
                tree.SelectionSettings.SelectedNodes.Clear();
            }
            //Action<XamDataTreeNodesCollection> expandAndSelect = null;

            ID_TypeHierarchy item;
            if (!path.TryPop(out item)) return;

            var source = nodeCollection as IEnumerable<XamDataTreeNode>;
            if (source == null) return;

            var find = source.FirstOrDefault(n => Equals(n.Data, item)); //item должен поддерживать интерфейс IDHierarchy
            if (find == null)
            {
                return;
            }

            if (path.Count == 0)
            {
                if (isExpandLast)
                {
                    find.IsExpanded = true;
                }

                result = find;

                if (isSelect)
                {
                    find.IsSelected = true;
                    tree.SelectionSettings.SelectedNodes.Add(find);
                }

                tree.ScrollNodeIntoView(find);
                FindBar.MoveSelectedNodeIntoCenter(tree);
            }
            else
            {
                find.IsExpanded = true;
                
                //vokeAsync(() =>
                ExpandFromSQL(find.Nodes, path, isExpandLast);
                    //);
            }

            //expandAndSelect(tree.Nodes);
            //return result;
        }

        #endregion

        /// <summary>
        /// Для запуска из-под невизуального потока
        /// </summary>
        public void InvokeAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            tree.Dispatcher.BeginInvoke(action, priority);
        }

        /// <summary>
        /// Процедура поиска и выделения объектов на дереве
        /// </summary>
        /// <param name="ids">Идентификаторы объектов, которые выделяем</param>
        public void Select(List<ID_TypeHierarchy> ids, bool isExpandLast = false, bool isSelect = true)
        {
            if (ids == null || ids.Count == 0) return;

            Task.Factory.StartNew(() => XamTreeFinder.BuildPathFromSQL(ids, Tree_ID))
                .ContinueWith(res =>
                {
                    var objectsFromSQL = res.Result;

                    OnBuildPathFromSQL(objectsFromSQL, isExpandLast, isSelect);
                });
        }

        private void OnBuildPathFromSQL(Dictionary<string, List<List<ID_TypeHierarchy>>> objectsFromSQL, bool isExpandLast, bool isSelect = true)
        {
            if (objectsFromSQL == null || objectsFromSQL.Count == 0) return;

            //var obs = isExpandFirst ? objectsFromSQL.Skip(1).ToList() : objectsFromSQL;

            if (tree != null)
            {
                tree.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                {
                    //Разворачиваем ветку
                    var f = objectsFromSQL.First().Value.First();
                    //Пока разворачиваем первый попавшийся
                    var path = new ConcurrentStack<ID_TypeHierarchy>(f);

                    //Выделяем объекты, с подгрузкой
                    if (isSelect && Items != null && Items.Count > 0)
                    {
                        var descriptor = GetDescriptor();
                        if (descriptor != null)
                        {
                            descriptor.SelectFromSets(Items, objectsFromSQL);
                        }
                    }

                    if (tree.IsLoaded)
                    {
                        ExpandFromSQL(tree.Nodes, path, isExpandLast, false);
                    }
                    else
                    {
                        var expandParams = new OnLoadExpandParams
                        {
                            Path = path,
                            IsExpandLast = isExpandLast,
                            IsSelect = false,
                        };

                        tree.Loaded += ExpandOnLoaded;
                        tree.Tag = expandParams;

                        //tree.Nodes не прогружаются пока дерево не отобразилось
                    }
                }));
            }
        }

        public void ExpandOnLoaded(object sender, RoutedEventArgs e)
        {
            tree.Loaded -= ExpandOnLoaded;

            var expandParams = tree.Tag as OnLoadExpandParams;
            if (expandParams!=null)
            {
                tree.Tag = null;
                ExpandFromSQL(tree.Nodes, expandParams.Path, expandParams.IsExpandLast, expandParams.IsSelect);
            }
        }

        public FrameworkElement GetVisualTree()
        {
            return tree;
        }
    }

    public static class FreeHierarchyTreeHelper
    {
        public static IEnumerable<FreeHierarchyTreeItem> Descendants(this FreeHierarchyTreeItem node, List<FreeHierarchyTreeItem> items = null)
        {
            if (items == null)
            {
                items = new List<FreeHierarchyTreeItem>();
            }

            foreach (var child in node.Children)
            {
                items.Add(child);
                items.AddRange(child.Descendants());
            }

            return items;

        }
    }
}
