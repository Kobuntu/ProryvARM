using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Infragistics.Controls.Menus;
using Infragistics.Controls.Menus.Primitives;
using Infragistics.Windows.DataPresenter;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.ServiceReference.UAService;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using System;
using System.Activities.Presentation.Toolbox;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using Infragistics.Windows;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree;
using Proryv.ElectroARM.Controls.Controls.Popup;
using Proryv.ElectroARM.Controls.Controls.Tabs;
using Xceed.Wpf.DataGrid;
using Action = System.Action;
using EnumFreeHierarchyItemType = Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService.EnumFreeHierarchyItemType;
using PlacementMode = System.Windows.Controls.Primitives.PlacementMode;
using Proryv.AskueARM2.Client.Visual.Common.Interfaces;
using Proryv.ElectroARM.Controls.Controls.Popup.Finder;

namespace Proryv.AskueARM2.Client.Visual
{
    /// <summary>
    /// Interaction logic for FindBar.xaml
    /// </summary>
    public partial class FindBar
    {
        private const double ViewportSize = 50;
        private const int _maxFindCount = 300;
        public FindBar()
        {
            InitializeComponent();
            if (LoginPage.IsShowPik) byPik.Visibility = Visibility.Visible;
        }

        public FindBar(FrameworkElement fe)
        {
            InitializeComponent();
            
            if (LoginPage.IsShowPik) byPik.Visibility = Visibility.Visible;

            var typeSwitch = new TypeSwitch<IModule>()
                .Case<ListView>(lv =>
                {
                    List = lv;
                    return null;
                })
                .Case<DataGridControl>(g =>
                {
                    DataGrid = g;
                    return DataGrid.FindParent<IModule>();
                })
                .Case<FastGrid>(fg =>
                {
                    FastGrid = fg;
                    return null;
                })
                .Case<ItemsControl>(ic =>
                {
                    Tree = ic;
                    return Tree.FindParent<IModule>();
                })
                .Case<XamDataTree>(xdt =>
                {
                    XamTree = xdt;
                    return XamTree.FindParent<IModule>();
                })
                .Case<DataPresenterBase>(dpb =>
                {
                    XamGrid = dpb;
                    return XamGrid.FindParent<IModule>();
                });

            var im = typeSwitch.Switch(fe);

            if (im != null)
            {
                ModuleType = im.ModuleType;
                if (XamTree != null) spFindTypeLayout.Visibility = Visibility.Visible;
                switch (im.ModuleType)
                {
                    case ModuleType.DataTIElectricity:
                    case ModuleType.DeviceManage:
                    case ModuleType.JuridicalPersons:
                    case ModuleType.MonitoringAutoSbor:
                    case ModuleType.Monitoring61968:
                    case ModuleType.MonitoringConcentratorSbor:
                    case ModuleType.MonitoringUSPDSbor:
                        spFindTypeLayout.Visibility = System.Windows.Visibility.Visible;
                        break;
                }
            }

            resultList.ItemTemplateSelector = resultList.Resources["StandartTemplateSelector"] as ItemSelector;
        }

        internal ItemsControl Tree;
        internal ListView List;
        internal DataGridControl DataGrid;
        internal FastGrid FastGrid;
        internal XamDataTree XamTree;
        internal DataPresenterBase XamGrid;

        internal ModuleType ModuleType;

        public static readonly DependencyProperty IsFindEnabledProperty = DependencyProperty.RegisterAttached(
            "IsFindEnabled", typeof(Boolean), typeof(FindBar), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(findBarPropertyChangedCallback)));

        public static void SetIsFindEnabled(UIElement element, Boolean value)
        {
            element.SetValue(IsFindEnabledProperty, value);
        }

        public static Boolean GetIsFindEnabled(UIElement element)
        {
            return (Boolean)element.GetValue(IsFindEnabledProperty);
        }

        public static readonly DependencyProperty BindedTextBoxProperty = DependencyProperty.RegisterAttached(
            "BindedTextBox", typeof(TextBox), typeof(FindBar), new FrameworkPropertyMetadata(null, findBarPropertyChangedCallback));

        public static void SetBindedTextBox(UIElement element, TextBox value)
        {
            element.SetValue(BindedTextBoxProperty, value);
        }

        public static TextBox GetBindedTextBox(UIElement element)
        {
            return element.GetValue(BindedTextBoxProperty) as TextBox;
        }


        /// <summary>
        /// Наличие расширенного поиска
        /// </summary>
        public static readonly DependencyProperty bySNProperty = DependencyProperty.RegisterAttached(
            "bySN", typeof(Boolean), typeof(FindBar), new FrameworkPropertyMetadata(false, null));

        public static void SetbySN(UIElement element, Boolean value)
        {
            element.SetValue(bySNProperty, value);
        }

        public static Boolean GetbySN(UIElement element)
        {
            return (Boolean)element.GetValue(bySNProperty);
        }

        /// <summary>
        /// Наличие расширенного поиска
        /// </summary>
        public static readonly DependencyProperty IsExtEnabledProperty = DependencyProperty.RegisterAttached(
            "IsExtEnabled", typeof(enumFindType), typeof(FindBar), new FrameworkPropertyMetadata(enumFindType.OnlyStandart, new PropertyChangedCallback(findBarPropertyChangedCallback)));

        public static void SetIsExtEnabled(UIElement element, enumFindType value)
        {
            element.SetValue(IsExtEnabledProperty, value);
        }

        public static enumFindType GetIsExtEnabled(UIElement element)
        {
            return (enumFindType)element.GetValue(IsExtEnabledProperty);
        }


        public static readonly DependencyProperty TypeHierarchyProperty = DependencyProperty.RegisterAttached(
            "TypeHierarchy", typeof(enumTypeHierarchy), typeof(FindBar),
            new FrameworkPropertyMetadata(enumTypeHierarchy.Info_TI, new PropertyChangedCallback(findBarPropertyChangedCallback)));

        public static void SetTypeHierarchy(UIElement element, enumTypeHierarchy value)
        {
            element.SetValue(TypeHierarchyProperty, value);
        }

        public static enumTypeHierarchy GetTypeHierarchy(UIElement element)
        {
            return (enumTypeHierarchy)element.GetValue(TypeHierarchyProperty);
        }

        /// <summary>
        /// Отфильтровывать определенный тип ТИ
        /// </summary>
        public static readonly DependencyProperty TiTypeFilterProperty = DependencyProperty.RegisterAttached(
            "TiTypeFilter", typeof(enumTIType?), typeof(FindBar), new FrameworkPropertyMetadata(null, findBarPropertyChangedCallback));

        public static readonly DependencyProperty WithPathProperty = DependencyProperty.RegisterAttached(
            "WithPath", typeof(bool), typeof(FindBar), new FrameworkPropertyMetadata(false, findBarPropertyChangedCallback));

        public static void SetWithPath(UIElement element, bool value)
        {
            element.SetValue(WithPathProperty, value);
        }

        public static bool GetWithPath(UIElement element)
        {
            return (bool)element.GetValue(WithPathProperty);
        }

        public static void SetTiTypeFilter(UIElement element, enumTIType? value)
        {
            element.SetValue(TiTypeFilterProperty, value);
        }

        public static enumTIType? GetTiTypeFilter(UIElement element)
        {
            return element.GetValue(TiTypeFilterProperty) as enumTIType?;
        }

        public enumTIType? TiTypeFilter;

        public static Dictionary<FrameworkElement, FindBar> popups = new Dictionary<FrameworkElement, FindBar>();

        private static void findBarPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fe = d as ItemsControl;
            if (fe == null) return;

            switch (e.Property.Name)
            {
                case "IsFindEnabled":
                    if ((bool) e.NewValue)
                    {
                        fe.TextInput += treeView_TextInput;
                    }
                    else
                    {
                        fe.TextInput -= treeView_TextInput;
                    }
                    break;
            }
        }


        public static void StartFind(object sender, string text)
        {
            treeView_TextInput(sender, new TextCompositionEventArgs(null, new TextComposition(null, null, text)));
        }

        internal static void treeView_TextInput(object sender, TextCompositionEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe == null) return;

            if (popups.ContainsKey(fe))
            {
                popups[fe].ContinueEditing(e.Text);
                return;
            }

            //Определяем из попапа ли мы открыли, если из попапа, то не закрываем его
            var parentPopup = fe.FindParent<Popup>();
            if (parentPopup == null) Manager.UI.CloseAllPopups();

            foreach (var fb in popups.Values.ToList())
            {
                var p = fb.FindParent<Popup>();
                if (p != null) p.IsOpen = false;
                fb.Dispose();
            }

            popups.Clear();

            var findBar = new FindBar(fe);
            findBar.MakeResizable();
            var tvPlace = new Rect(fe.PointToScreen(new Point(0, 0)), fe.PointToScreen(new Point(fe.ActualWidth, fe.ActualHeight)));
            var wa = SystemParameters.WorkArea;
            var popup = new Popup
            {
                Child = findBar,
            };

            var isShowNearest = true;
            var m = fe.FindParent<IModule>();
            if (m != null)
            {
                Size s;
                if (findBarSizes.TryGetValue(m.ModuleType, out s))
                {
                    findBar.Height = s.Height;
                    findBar.Width = s.Width;
                }

                var ch = fe.FindParent<ILocalChildren>();
                isShowNearest = ch == null;
            }

            popup.Closed += popup_Closed;

            if (isShowNearest && (findBar.Tree is System.Windows.Controls.TreeView || findBar.List != null || findBar.XamTree != null))
            {
                popup.PlacementTarget = fe;
                if ((tvPlace.Right + findBar.ActualHeight) < wa.Right) popup.Placement = PlacementMode.Right;
                else if (tvPlace.Left - findBar.ActualWidth > wa.Left) popup.Placement = PlacementMode.Left;
                else if (tvPlace.Bottom + findBar.ActualHeight < wa.Bottom)
                {
                    popup.Placement = PlacementMode.Relative;
                    popup.HorizontalOffset = tvPlace.Width - findBar.ActualWidth;
                    popup.VerticalOffset = tvPlace.Height;
                }
                else popup.Placement = PlacementMode.Top;
            }
            else
            {
                popup.PlacementTarget = fe;
                //popup.PlacementTarget = fe.Tag as FindButton;

                if (findBar.Tree != null || findBar.XamTree != null || findBar.XamGrid != null
                    || findBar.DataGrid != null || findBar.List!=null)
                {
                    popup.Placement = PlacementMode.Right;
                    popup.HorizontalOffset = -333;
                    popup.VerticalOffset = -10;
                }
                else
                {
                    popup.Placement = PlacementMode.Left;
                    popup.HorizontalOffset = 20;
                    popup.VerticalOffset = 10;
                }

                fe.Tag = null;
            }

            if (popup.PlacementTarget != null)
            {
                popup.OpenAndRegister(false);
                popups[fe] = findBar;
                findBar.ContinueEditing(e.Text);
            }
        }

        private static void popup_Closed(object sender, EventArgs e)
        {
            var popup = sender as Popup;
            var findBar = popup.Child as FindBar;
            var module = findBar.Tree.FindParent<IModule>();
            if (module != null)
            {
                findBarSizes[module.ModuleType] = new Size(findBar.ActualWidth, findBar.ActualHeight);
            }
            popup.Closed -= popup_Closed;
        }

        private static Dictionary<ModuleType, Size> findBarSizes = new Dictionary<ModuleType, Size>();

        public void ContinueEditing(string text)
        {
            textFind.Text = text;
            Keyboard.Focus(textFind);
            textFind.CaretIndex = text.Length;
        }

        private volatile string currentText = null;

        private void textFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_searchTimer == null)
            {
                _searchTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(_timeout), DispatcherPriority.Background, OnSearchCallback, this.Dispatcher);
            }

            _searchTimer.Interval = TimeSpan.FromMilliseconds(_timeout);
            _searchTimer.Start();
        }

        private void OnSearchCallback(object sender, EventArgs e)
        {
            _searchTimer.Stop();

            if (Tree == null && DataGrid == null && List == null && FastGrid == null && XamTree == null && XamGrid == null) return;

            currentText = textFind.Text;
            IEnumerable source = null;
            if (Tree != null) source = Tree.ItemsSource;
            else if (XamTree != null)
            {
                source = XamTree.ItemsSource;
            }
            else if (XamGrid != null)
            {
                source = XamGrid.DataSource;
            }
            else if (DataGrid != null) source = DataGrid.ItemsSource;
            else if (List != null)
            {
                source = List.ItemsSource;
                //var tb = GetBindedTextBox(list);
                //if (tb != null) tb.Text = currentText;
            }
            else if (FastGrid != null) source = FastGrid.Data;
            resultList.ItemsSource = null;
            if (string.IsNullOrEmpty(currentText) || source == null) return;

            try
            {
                //Если были запущены поиски, отменяем
                if (cancellFindTokenSource != null)
                {
                    cancellFindTokenSource.Cancel();
                }
            }
            catch
            {
            }

            if (source is DataGridCollectionView)
            {
                var cv = source as DataGridCollectionView;
                source = cv.SourceItems;
            }

            cancellFindTokenSource = new CancellationTokenSource();
            //Для отмены уже запущенного поиска
            var token = cancellFindTokenSource.Token;

            var isByName = byName.IsChecked.GetValueOrDefault();

            EnumFindTiType findTiType;
            if (byName.IsChecked.GetValueOrDefault())
            {
                findTiType = EnumFindTiType.TIName;
            }
            else if (bySN.IsChecked.GetValueOrDefault())
            {
                findTiType = EnumFindTiType.MeterSerialNumber;
            }
            else
            {
                findTiType = EnumFindTiType.Pik;
            }

            try
            {
                resultList.RunAsync(() => InvokeSearch(currentText, source, token,
                    isByName, findTiType), OnSearchCompleted);
            }
            catch (AggregateException ae)
            {
                Manager.UI.ShowMessage(ae.ToException().Message);
            }
        }

        private DispatcherTimer _searchTimer;
        private CancellationTokenSource cancellFindTokenSource = new CancellationTokenSource();
        private volatile int _timeout = 300;

        #region Процедуры поиска

        
        private Tuple<List<IFindableItem>, string, Dictionary<object, Stack>, CancellationToken, List<UaFindNodeResult>, EnumFindTiType> InvokeSearch(string text, IEnumerable source,
            CancellationToken token, bool isByName, EnumFindTiType findTiType)
        {
            var founded = new Dictionary<object, Stack>(new ObjectFoundedComparer());

            needFindTI = false;
            var needFindUaNode = false;
            var needFindTransformatorsAndreactors = false; //Необходим поиск трансформаторов и реакторов по базе (т.к. еще не подгрузились)
            int freeHierTreeId = -1;

            List<UaFindNodeResult> foundedNodes = null;
            Task<Tuple<List<Hard_PTransformator>, List<Hard_PReactors>>> findTransformatorsAndReactorsTask = null;

            Task<List<TInfo_TI>> findTisTask = null;
            if (source != null)
            {
                try
                {
                    var enumerator = source.GetEnumerator();
                    enumerator.MoveNext();
                    var fe = enumerator.Current as ITreeDescriptor;
                    if (fe != null && fe.Descriptor != null)
                    {
                        needFindTI = fe.Descriptor.NeedFindTI;
                        needFindUaNode = fe.Descriptor.NeedFindUaNode;
                        needFindTransformatorsAndreactors = fe.Descriptor.NeedFindTransformatorsAndreactors;
                        freeHierTreeId = fe.Descriptor.Tree_ID ?? -101;
                    }

                }
                catch { }
            }

            #region Запускаем поиск реакторов и трансформаторов

            if (needFindTransformatorsAndreactors && text.Length > 1)
            {
                findTransformatorsAndReactorsTask = Task<Tuple<List<Hard_PTransformator>, List<Hard_PReactors>>>.Factory.StartNew(() =>
                {
                    try
                    {
                        //Поиск трансформаторов и реакторов
                        return ServiceFactory.ArmServiceInvokeSync<Tuple<List<Hard_PTransformator>, List<Hard_PReactors>>>("TREE_TransformatorOrReactor", text);
                    }
                    catch
                    {
                    }
                    return null;
                }, token);
            }

            #endregion

#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif
            #region Запускаем поиск ТИ

            if (needFindTI)
            {
                findTisTask = Task<List<TInfo_TI>>.Factory.StartNew(() =>
                {
                    try
                    {
#if DEBUG
                        var swt = new System.Diagnostics.Stopwatch();
                        swt.Start();
#endif
                        //TODO доделать поиск ТИ по свободным иерархиям
                        var result = EnumClientServiceDictionary.TIHierarchyList.FindTop100(text, findTiType.ToString(), ListGlobalHierarchyTreListControl.TypeHierarchy, null, TiTypeFilter, freeHierTreeId);
#if DEBUG
                        swt.Stop();
                        Console.WriteLine("Поиск ТИ {0} млс", swt.ElapsedMilliseconds);
#endif
                        return result;
                    }
                    catch (Exception ex)
                    {
                        Manager.UI.ShowMessage(ex.Message);
                    }
                    return null;
                }, token);
            }

            #endregion

            if (freeHierTreeId > 0)
            {
                //Это дерево свободной иерархии поиск по базе данных
            }

            scanNode(founded, source, text, new Stack(), isByID, isByName);

#if DEBUG
            sw.Stop();
            Console.WriteLine("Поиск по дереву {0} млс", sw.ElapsedMilliseconds);
#endif

            #region Поиск узлов OPC

            if (needFindUaNode && founded.Count < 100 && text.Length > 2)
            {
                //Поиск узлов OPC в дереве
                foundedNodes = UAService.Service.UA_FindNode(text, freeHierTreeId, findTiType == EnumFindTiType.MeterSerialNumber ? "UANode_ID" : null);

                //Предварительная прогрузка узлов с типами найденных узлов (чтобы визуалка не тормозила)
                if (foundedNodes != null && foundedNodes.Count > 0)
                {
                    _typeNodesPreparer = Task.Factory.StartNew(
                        () => UAHierarchyDictionaries.UANodesDict.Prepare(
                            new HashSet<long>(foundedNodes.Where(fn => fn.Node.UATypeNode_ID.HasValue).Select(fn => fn.Node.UATypeNode_ID.Value)), Manager.UI.ShowMessage), token);
                }
            }

            #endregion

            var foundedObjects = new List<IFindableItem>();

            if (findTransformatorsAndReactorsTask != null)
            {
                var foundedTransformatorsAndReactors = findTransformatorsAndReactorsTask.Result;

                foundedTransformatorsAndReactors.Item1.ForEach(transformator =>
                {
                    foundedObjects.Add(EnumClientServiceDictionary.GetOrAddTransformator(transformator.PTransformator_ID, transformator));
                });

                foundedTransformatorsAndReactors.Item2.ForEach(reactor =>
                {
                    foundedObjects.Add(EnumClientServiceDictionary.GetOrAddReactor(reactor.PReactor_ID, reactor));
                });
            }

            //Заданный текст для поиска не актуален или пустой
            if (token.IsCancellationRequested)
            {
                return null;
            }

            try
            {
                if (needFindTI && findTisTask != null && findTisTask.Result != null)
                {
                    foundedObjects.InsertRange(0, findTisTask.Result);
                }
            }
            catch
            {

            }

            return new Tuple<List<IFindableItem>, string, Dictionary<object, Stack>, CancellationToken, List<UaFindNodeResult>, EnumFindTiType>(foundedObjects, text, founded, token, foundedNodes, findTiType);
        }

        private Task _typeNodesPreparer;
        private void OnSearchCompleted(Tuple<List<IFindableItem>, string, Dictionary<object, Stack>, CancellationToken, List<UaFindNodeResult>, EnumFindTiType> result)
        {
            if (result == null || result.Item4.IsCancellationRequested) return;

            var tiFound = result.Item1;
            var text = result.Item2;
            var founded = result.Item3;
            var foundedNodes = result.Item5;
            var findTiType = result.Item6;

            #region Найденные в БД ТИ

            if (tiFound != null && tiFound.Count > 0)
            {
                var im = Tree.FindParent<IModule>();
                if (im != null)
                {
                    switch (im.ModuleType)
                    {
                        case ModuleType.BalanceEditor_MSK:
                        case ModuleType.BalanceEditor:
                        case ModuleType.BalanceEditor_HierLev0:
                        case ModuleType.FormulaEditor:
                        case ModuleType.FormulaEditorTP:
                            var fe = im as FrameworkElement;
                            if (fe != null)
                            {
                                var cb = fe.FindName("tiFilter") as ComboBox;
                                if (cb != null && cb.SelectedIndex > 0)
                                {
                                    tiFound = tiFound.Where(ti =>
                                    {
                                        var t = ti as TInfo_TI;
                                        if (t == null) return true;

                                        return t.Voltage == Convert.ToDouble(cb.SelectedItem);
                                    }).ToList();
                                }
                            }
                            break;
                    }
                }

                //Добавляем в результат поиска те ТИ, которых нет в дереве
                foreach (var ti in tiFound.Where(ti => !founded.Keys.Any(foundedAndExistInTree =>
                {
                    IFindableItem findable = foundedAndExistInTree as IFindableItem;
                    if (findable != null) return Equals(ti, findable.GetItemForSearch());
                    return false;
                })))
                {
                    founded[ti] = null;
                }
            }

            #endregion 

            #region Найденные в БД узлы OPC

            //Добавляем в результат поиска те узлы, которых нет в результатах поиска
            if (foundedNodes != null && foundedNodes.Count > 0)
            {
                var typeNodeIds = new HashSet<long>();
                foreach (var foundedNode in foundedNodes)
                {

                    if (!founded.Keys.Any(foundedAndExistInTree =>
                                          {
                                              TUANode node;
                                              var treeItem = foundedAndExistInTree as FreeHierarchyTreeItem;
                                              if (treeItem != null && treeItem.FreeHierItemType == EnumFreeHierarchyItemType.UANode && (node = treeItem.HierObject as TUANode) != null)
                                              {
                                                  return Equals(node, foundedNode.Node);
                                              }

                                              return false;
                                          }))
                    {
                        founded[foundedNode] = null;
                    }

                    if (foundedNode.Node.UATypeNode_ID.HasValue)
                    {
                        typeNodeIds.Add(foundedNode.Node.UATypeNode_ID.Value);
                    }
                }

                try
                {
                    if (_typeNodesPreparer != null) _typeNodesPreparer.Wait();
                }
                catch (Exception)
                {
                }
            }

            #endregion

            #region Переключаем раскладку на русскую

            if (founded == null || founded.Count == 0)
            {
                txtNotFound.Visibility = System.Windows.Visibility.Visible;

                if (text.Length > 3)
                {
                    //Если первые два символа цифры, пробуем искать по номеру счетчика
                    if (findTiType != EnumFindTiType.MeterSerialNumber && char.IsDigit(text[0]) && char.IsDigit(text[1]))
                    {
                        //Не нашли, пробуем переключить раскладку на русскую
                        Dispatcher.BeginInvoke(DispatcherPriority.Background,
                            (Action)(() =>
                           {
                               _timeout = 5;
                               bySN.IsChecked = true;
                           }));

                        return;
                    }

                    //Не нашли, пробуем переключить раскладку на русскую
                    Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        (Action)(() =>
                       {
                           string newFindText;
                           if (!ExtFindHierarchyList_Control.PuntoSwitch(text, out newFindText)) return;
                           _timeout = 5;
                           textFind.Text = newFindText;
                           textFind.Select(textFind.Text.Length, 0);
                       }));
                }
                return;
            }

            txtNotFound.Visibility = System.Windows.Visibility.Collapsed;


            #endregion

            buildResult(text, text, founded);
        }

        private void buildResult(string confName, string toFind, Dictionary<object, Stack> founded)
        {
            textFind.Focus();

            var result = new List<object>();

            lock (_foundedSyncLock)
            {
                result.AddRange(founded.Keys);
                _founded = founded;
            }

            resultList.ItemsSource = result;

            //22.05.2018 Удаление стилизирования выбранного объекта
            //Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle
            //    , new Action(delegate
            //    {
            //        var style = resultList.Resources["favourites"] as Style;
            //        for (int i = 0; i < favCnt; i++)
            //        {
            //            var lvi = resultList.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
            //            if (lvi != null) lvi.Style = style;
            //        }
            //        FinderBusy = false;
            //    }));
        }

        #endregion

        private Dictionary<object, Stack> _founded;
        private readonly object _foundedSyncLock = new object();

        public static bool scanNode(Dictionary<object, Stack> founded, IEnumerable list, object find, Stack chain,
            bool isByID, bool isByName = true, HashSet<int> psIds = null, bool isFindOnlySingle = false)
        {
            if (list == null) return false;

            ConcurrentQueue<object> localList = null;

            try
            {
                //SortedRangeObservableCollection<FreeHierarchyTreeItem>

                localList = new ConcurrentQueue<object>(list.Cast<object>());
            }
            catch
            {

            }

            if (localList == null || localList.IsEmpty) return false;

            var findText = find as string;
            //var toFind = findText == null ? null : Regex.Replace(' ' + findText.ToLower() + ' ', @"\s+", "%");

            while (!localList.IsEmpty)
            {
                object item;
                localList.TryDequeue(out item);

                chain.Push(item);

                object itemForTemplate;
                var fi = ExtFindHierarchyList_Control.FindableItemFromObject(item, out itemForTemplate);
                if (fi != null)
                {
                    if (findText != null)
                    {
                        if (!isByID)
                        {
                            var fgi = fi.GetItemForSearch();
                            if (fgi != null)
                            {
                                if (itemForTemplate == null) itemForTemplate = fgi;

                                if (isByName)
                                {
                                    var str = fgi.ToString();
                                    if (str != null && str.IndexOf(findText, StringComparison.InvariantCultureIgnoreCase) >=0)
                                    {
                                        founded[itemForTemplate] = new Stack(chain);
                                        if (isFindOnlySingle || founded.Count > _maxFindCount) return true;
                                    }
                                }
                                else
                                {
                                    var ti = fi.GetItemForSearch() as TInfo_TI;
                                    if (ti != null && !isByName && ti.MeterSerialNumber != null &&
                                        ti.MeterSerialNumber.StartsWith(findText, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        founded[itemForTemplate] = new Stack(chain);
                                        if (isFindOnlySingle || founded.Count > _maxFindCount) return true;
                                    }

                                    var uspd = fi.GetItemForSearch() as IHard_USPD;
                                    if (uspd != null && uspd.USPDSerialNumber != null
                                                     && uspd.USPDSerialNumber
                                                         .StartsWith(findText, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        founded[itemForTemplate] = new Stack(chain);
                                        if (isFindOnlySingle || founded.Count > _maxFindCount) return true;
                                    }
                                }

                                var dps = fgi as TPSHierarchy;
                                if (dps != null && psIds != null)
                                {
                                    var hierarchyObject = fi as FreeHierarchyTreeItem;
                                    if (hierarchyObject == null ||
                                        (hierarchyObject != null && hierarchyObject.IncludeObjectChildren))
                                    {
                                        psIds.Add(dps.Id);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var Item = fi.GetItemForSearch() as IKey;
                            if (Item != null)
                            {
                                var id = Item.GetKey;
                                if (string.Equals(id,findText, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    founded[Item] = new Stack(chain);
                                    if (isFindOnlySingle || founded.Count > _maxFindCount) return true;
                                }
                            }

                            var dps = fi.GetItemForSearch() as TPSHierarchy;
                            if (dps != null && psIds != null)
                                psIds.Add(dps.Id);
                        }
                    }
                    else if (Equals(fi.GetItemForSearch(), find))
                    {
                        founded[fi.GetItemForSearch()] = new Stack(chain);
                        if (isFindOnlySingle || founded.Count > _maxFindCount) return true;
                    }

                    var children = fi.GetChildren();
                    if (children != null)
                    {
                        if (scanNode(founded, children, find, chain, isByID, isByName, psIds, isFindOnlySingle: isFindOnlySingle)) return true;
                    }
                }
                else
                {
                    var tc = item as ToolboxCategory;
                    if (tc != null)
                    {
                        if (tc.CategoryName.IndexOf(findText, StringComparison.InvariantCultureIgnoreCase) >= 0)
                        {
                            founded[item] = new Stack(chain);
                            if (isFindOnlySingle || founded.Count > _maxFindCount) return true;
                        }

                        if (scanNode(founded, tc.Tools, find, chain, isByID, isFindOnlySingle: isFindOnlySingle)) return true;
                    }

                    var tiw = item as ToolboxItemWrapper;
                    if (tiw != null)
                    {
                        if (tiw.DisplayName.IndexOf(findText, StringComparison.InvariantCultureIgnoreCase) >= 0)
                        {
                            founded[item] = new Stack(chain);
                            if (isFindOnlySingle || founded.Count > _maxFindCount) return true;
                        }
                    }

                    if (item.ToString().IndexOf(findText, StringComparison.InvariantCultureIgnoreCase) >=0 )
                    {
                        founded[item] = new Stack(chain);
                        if (isFindOnlySingle || founded.Count > _maxFindCount) return true;
                    }
                }

                chain.Pop();
            }

            //}), false);

            return false;
        }


        private void butClose_Click(object sender, RoutedEventArgs e)
        {
            Dispose();
            this.ClosePopup();
        }

        private void resultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selItem = resultList.SelectedItem;
            FindAndSelect(selItem, _founded);
        }

        private void FindAndSelect(object selItem, Dictionary<object, Stack> founded)
        {
            if (selItem == null) return;
            if (Tree != null)
            {
                Stack stack;
                if (founded.TryGetValue(selItem, out stack))
                {
                    if (stack != null)
                    {
                        if (Tree is TreeViewItem)
                        {
                            //Надо раскрыть само дерево
                            if (!stack.Contains(Tree.DataContext))
                            {
                                stack.Push(Tree.DataContext);
                            }

                            Tree.FindParent<System.Windows.Controls.TreeView>().ItemContainerGenerator.ExpandAndSelect(stack.Clone() as Stack);
                        }
                        else
                        {
                            Tree.ItemContainerGenerator.ExpandAndSelect(stack.Clone() as Stack);
                        }
                    }
                    else
                    {
                        if (ModuleType == ModuleType.JuridicalPersons)
                        {
                            Tree.ExpandAndSelectJuridical(selItem as TInfo_TI);
                        }
                        else
                        {
                            Tree.ExpandAndSelectTI(selItem as TInfo_TI);
                        }
                    }
                }
            }
            else if (List != null)
            {
                Stack stack;
                if (founded.TryGetValue(selItem, out stack))
                {
                    stack = stack.Clone() as Stack;
                    if (stack != null && stack.Count > 0)
                    {
                        List.SelectedItem = stack.Pop();

                        if (List.IsGrouping)
                        {
                            if (List.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                            {
                                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(DelayedBringIntoView));
                            }
                            else
                            {
                                List.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
                            }
                        }
                        else
                        {
                            List.ScrollIntoView(List.SelectedItem);
                            var item = List.ItemContainerGenerator.ContainerFromItem(List.SelectedItem) as ListViewItem;
                            if (item != null)
                            {
                                item.Focus();
                                List.ScrollIntoView(item);
                            }
                        }
                    }
                }
            }
            else if (DataGrid != null)
            {
                XceedGridFinder.ExpandandSelectXceedGrid(DataGrid, founded, selItem);
            }
            else if (FastGrid != null)
            {
                Stack stack;
                if (founded.TryGetValue(selItem, out stack))
                {
                    stack = stack.Clone() as Stack;
                    FastGrid.SelectRow(FastGrid.Data.IndexOf(stack.Pop() as TIRow), -1, true);
                }
            }
            else if (XamTree != null)
            {
                XamTreeFinder.ExpandAndSelectXamTree(founded, selItem, XamTree);
            }
            else if (XamGrid != null)
            {
                Stack stack;
                if (founded.TryGetValue(selItem, out stack))
                {
                    XamGrid.SelectedItems.Records.Clear();
                    var row = stack.ToArray().Last();
                    DataPresenterFinder.ExpandAndSelectRowDataPresenter(row, XamGrid);
                }
            }

            //22.05.2018 Удаление стилизирования выбранного объекта
            //var confName = textFind.Text.ToLower();
            //List<string> favours = null;
            //if (FindFavourites.ContainsKey(confName))
            //    favours = FindFavourites[confName];
            //else
            //{
            //    favours = new List<string>();
            //    FindFavourites[confName] = favours;
            //}
            //var find = selItem.ToString();
            //favours.RemoveAll(s => s == find);
            //favours.Insert(0, find);
            //if (favours.Count > 5) favours.RemoveAt(5);
        }

        public static void ExpandAndSelectDataPresenter(object item, DataPresenterBase dpb)
        {
            var founded = new Dictionary<object, Stack>();
            scanNode(founded, dpb.DataSource, item, new Stack(), true);

            dpb.SelectedItems.Records.Clear();
            var row = founded[item].ToArray().Last();
            DataPresenterFinder.ExpandAndSelectRowDataPresenter(row, dpb);
        }

        
        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (List.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return;

            List.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(DelayedBringIntoView));
        }

        private void DelayedBringIntoView()
        {
            if (List == null || List.Items.Groups == null || List.Items.Groups.Count == 0) return;

            //object foundedGroup = null;
            //foreach (var itemsGroup in List.Items.Groups)
            //{
            //    var cvg = itemsGroup as CollectionViewGroup;
            //    if (cvg != null)
            //    {
            //        if (cvg.Items.Any(c=>c == List.SelectedItem))
            //        {
            //            foundedGroup = itemsGroup;
            //            break;
            //        }
            //    }
            //}

            //if (foundedGroup == null) return;

            var item = List.ItemContainerGenerator.ContainerFromItem(List.SelectedItem) as ListBoxItem                ;
            if (item != null)
            {
                var expander = item.FindLogicalChild<Expander>();
                if (expander != null) expander.IsExpanded = true;

                item.BringIntoView();
                //List.ScrollIntoView(item);
            }
        }

        public static void MoveUp(XamDataTree xamTree)
        {
           // if (xamTree.IsLoaded) false;
            if (xamTree == null || xamTree.SelectionSettings == null || xamTree.SelectionSettings.SelectedNodes.Count == 0
                || xamTree.Dispatcher == null) return;

            var selectedNode = xamTree.SelectionSettings.SelectedNodes[0];
            if (selectedNode == null) return;

            xamTree.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(delegate
                {
                    var sv = xamTree.FindLogicalChild("VerticalScrollBar") as ScrollBar;
                    if (sv == null) return;

                    var np = xamTree.FindLogicalChild("NodesPanel") as NodesPanel;

                    if (selectedNode.Control != null)
                    {
                        selectedNode.Control.BringIntoView();
                        xamTree.ActiveNode = selectedNode;
                        xamTree.ScrollNodeIntoView(selectedNode);
                        //var point = selectedNode.Control.TranslatePoint(new Point(0, 0), xamTree);
                        //if (point.Y <= 21) return;

                        //sv.Value = sv.Value + (point.Y - 30) / ViewportSize;
                    }
                    else
                    {
                        //Считаем что контрол где-то внизу, приподымаем его
                        //xamTree.ScrollNodeIntoView(selectedNode);
                        //CancellationTokenSource source = new CancellationTokenSource();

                        xamTree.InvalidateScrollPanel(false);
                        np.UpdateLayout();
                        np.InvalidateMeasure();
                        var startvalue = sv.Value;
                        bool overflow = false;
                        var prevval = startvalue;
                        int i = 0;
                        while (!np.VisibleNodes.Contains(selectedNode) && xamTree.IsLoaded && i < 10000)
                        {

                            sv.Value = sv.Value + sv.ViewportSize;
                            prevval = sv.Value;
                            xamTree.InvalidateScrollPanel(false);
                            np.InvalidateMeasure();
                            np.UpdateLayout();

                            i++;// защита 
                            //TODO :  Переодически скролл перестает менять свобю позицию  
                            if (prevval != sv.Value) break;
                            if (Math.Abs(sv.Value - sv.Track.Maximum) < 0.1)
                            {
                                sv.Value = 0;
                                overflow = true;
                            }
                            if (overflow && sv.Value > startvalue)
                                return;
                        }

                        if (selectedNode.Control != null) selectedNode.Control.BringIntoView();

                        try
                        {
                            xamTree.ScrollNodeIntoView(selectedNode);
                            xamTree.ActiveNode = selectedNode;
                        }
                        catch { }
                    }
                }));
        }

        public static void MoveSelectedNodeIntoCenter(XamDataTree xamTree)
        {
            xamTree.Dispatcher.BeginInvoke(DispatcherPriority.Render,
                (Action)(() =>
                {
                    var selectedNode = xamTree.SelectionSettings.SelectedNodes[0];
                    if (selectedNode == null || selectedNode.Control == null) return;

                    var sv = xamTree.FindLogicalChild("VerticalScrollBar") as ScrollBar;
                    if (sv == null) return;

                    try
                    {
                        var point = selectedNode.Control.TranslatePoint(new Point(), xamTree);
                        if (point.Y <= ViewportSize)
                        {
                            //Пододвигаем вних (тут, возможно, не совсем правильно)
                            sv.Value = sv.Value - (point.Y + ViewportSize) / ViewportSize - 1.5;
                        }
                        else
                        {
                            //Пододвигаем вверх
                            sv.Value = sv.Value + (point.Y - ViewportSize) / ViewportSize;
                        }

                        xamTree.InvalidateScrollPanel(false);
                    }
                    catch
                    {
                    }

                }));
        }

        
        
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }

        private static Dictionary<string, List<string>> FindFavourites = new Dictionary<string, List<string>>();

        private bool isByID = false;

        private void textFind_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.I))
            {
                if (isByID) caption.Text = "Поиск";
                else caption.Text = "Поиск по ID";
                isByID = !isByID;
                textFind_TextChanged(null, null);
            }
            else if (e.Key == Key.Escape)
            {
                try
                {
                    //Если были запущены поиски, отменяем
                    if (cancellFindTokenSource != null)
                    {
                        cancellFindTokenSource.Cancel();
                    }
                }
                catch
                {
                }
            }
        }

        public static THierarchyDbTreeObject DummyTI = new THierarchyDbTreeObject() { Id = -1, Type = enumTypeHierarchy.Info_TI };

        public static volatile bool FinderBusy = false;
        public static volatile bool needFindTI = false;

        /// <summary>
        /// Упорядоченное дерево родителей, у которых могут быть дети
        /// </summary>
        public static readonly List<enumTypeHierarchy> GlobalHierarchyTree = new List<enumTypeHierarchy>()
                                                                             {
                                                                                 enumTypeHierarchy.Dict_PS,
                                                                                 enumTypeHierarchy.Dict_HierLev3,
                                                                                 enumTypeHierarchy.Dict_HierLev2,
                                                                                 enumTypeHierarchy.Dict_HierLev1,
                                                                             };

        internal static TInfo_TI TItoSelect = null;
        internal static Stack PSFound = null;

        public static void SelectTI(ItemsControl tree, IFindableItem item)
        {
            if (TItoSelect == null || PSFound == null) return;
            IFindableItem found = null;
            foreach (IFindableItem ti in item.GetChildren())
            {
                var key = ti.GetItemForSearch() as IKey;
                int id;
                if (key == null || !int.TryParse(key.GetKey, out id) || id != TItoSelect.TI_ID) continue;
                found = ti;
                break;
            }
            TItoSelect = null;
            PSFound = new Stack(PSFound);
            PSFound.Push(found);
            PSFound = new Stack(PSFound);
            tree.ItemContainerGenerator.ExpandAndSelect(PSFound.Clone() as Stack, false);
        }

        private void byName_Checked(object sender, RoutedEventArgs e)
        {
            var cb = sender as RadioButton;
            if (cb == null || resultList == null) return;

            EnumFindTiType findTiType;
            switch (cb.Name)
            {
                case "byName":
                    findTiType = EnumFindTiType.TIName;
                    break;
                case "bySN":
                    findTiType = EnumFindTiType.MeterSerialNumber;
                    break;
                case "byPik":
                    findTiType = EnumFindTiType.Pik;
                    break;
                default:
                    findTiType = EnumFindTiType.TIName;
                    break;
            }

            var itemSelector = resultList.Resources["StandartTemplateSelector"] as ItemSelector;
            if (itemSelector != null)
            {
                itemSelector.FindTiType = findTiType;
            }

            if (ListGlobalHierarchyTreListControl != null) ListGlobalHierarchyTreListControl.FindTiType = findTiType;

            textFind_TextChanged(null, null);
        }

        public void Dispose()
        {
            ListGlobalHierarchyTreListControl.Dispose();

            FrameworkElement fm = null;
            if (Tree != null)
            {
                fm = Tree as FrameworkElement;
            }
            else if (DataGrid != null)
            {
                fm = DataGrid as FrameworkElement;
            }
            else if (List != null)
            {
                fm = List as FrameworkElement;
            }
            else
            {
                popups.Clear();
            }
            if (fm != null)
            {
                popups.Remove(fm);
            }
            resultList.ClearItems(false);
            if (_founded != null) _founded.Clear();
            Tree = null;
            DataGrid = null;
            FastGrid = null;
            List = null;
            TItoSelect = null;
            if (PSFound != null) PSFound.Clear();
            PSFound = null;

            FinderBusy = false;
        }

        private void TabControl_Loaded(object sender, RoutedEventArgs e)
        {
            UIElement fe = null;

            enumTypeHierarchy typeHierarchy;
            enumFindType ft;

            if (XamTree != null)
            {
                //resultList.ItemTemplateSelector = null;
                fe = XamTree;

                var parrent = VisualHelper.FindParent<UserControl>(XamTree);
                if (parrent == null) return;

                typeHierarchy = GetTypeHierarchy(parrent);
                ft = GetIsExtEnabled(parrent);
                TiTypeFilter = GetTiTypeFilter(parrent);
                if (!(parrent is FreeHierarchyTree))
                {
                    resultList.ItemTemplate = resultList.Resources["StandartTemplate"] as DataTemplate;
                }
                else if (GetWithPath(parrent))
                {
                    resultList.ItemTemplate = resultList.Resources["FreeHierarchyTemplateWithPath"] as DataTemplate;
                }
                else
                {
                    resultList.ItemTemplate = resultList.Resources["FreeHierarchyTemplate"] as DataTemplate;
                }
                //selector.WithPath = GetWithPath(parrent);
            }
            else
            {
                if (Tree != null) fe = Tree;
                else if (DataGrid != null) fe = DataGrid;
                else if (List != null) fe = List;
                else if (FastGrid != null) fe = FastGrid;
                else if (XamTree != null) fe = XamTree;

                if (fe == null) return;

                typeHierarchy = GetTypeHierarchy(fe);
                ft = GetIsExtEnabled(fe);
                TiTypeFilter = GetTiTypeFilter(fe);
            }

            switch (ft)
            {
                case enumFindType.OnlyStandart:
                    tabExtSearch.Visibility = System.Windows.Visibility.Collapsed;
                    tabStandartSearch.Visibility = System.Windows.Visibility.Visible;
                    tabStandartSearch.IsSelected = true;
                    break;
                case enumFindType.StandartAndExt:
                    tabExtSearch.Visibility = System.Windows.Visibility.Visible;
                    tabStandartSearch.Visibility = System.Windows.Visibility.Visible;
                    tabStandartSearch.IsSelected = true;
                    break;
                case enumFindType.OnlyExt:
                    tabExtSearch.Visibility = System.Windows.Visibility.Visible;
                    tabStandartSearch.Visibility = System.Windows.Visibility.Collapsed;
                    tabExtSearch.IsSelected = true;
                    break;
            }

            ListGlobalHierarchyTreListControl.InitializeExtFinder(FindAndSelect, fe, ModuleType, typeHierarchy);

            if (GetbySN(fe))
            {
                spFindTypeLayout.Visibility = Visibility.Visible;
            }
        }

        private class ObjectFoundedComparer : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y)
            {
                if (ReferenceEquals(x, y)) return true;

                var ix = x as IFreeHierarchyObject;
                var iy = y as IFreeHierarchyObject;

                if (ix == null || iy == null) return false;

                return ix.Type == iy.Type && ix.Id == iy.Id;
            }

            public int GetHashCode(object obj)
            {
                var ix = obj as IFreeHierarchyObject;
                if (ix == null) return obj.GetHashCode();

                return string.Format("{0}{1}", ix.Type, ix.Id).GetHashCode();
            }
        }
    }

    /// <summary>
    /// Тип определяющий какой тип поиска будет доступен
    /// </summary>
    public enum enumFindType
    {
        /// <summary>
        /// Только стандартный поиск
        /// </summary>
        OnlyStandart = 0,
        /// <summary>
        /// Стандартный и расширенный
        /// </summary>
        StandartAndExt = 1,
        /// <summary>
        /// Только расширенный
        /// </summary>
        OnlyExt = 2,
    }
}
