using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Infragistics.Controls.Menus;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.DataPresenter.Events;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.ServiceReference.Service.Interfaces;
using Proryv.AskueARM2.Client.ServiceReference.UAService;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using Proryv.ElectroARM.Controls.Controls.Popup.Finder;
using PlacementMode = System.Windows.Controls.Primitives.PlacementMode;
using Proryv.ElectroARM.Controls.Controls.Tabs;
using Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree;
using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.Service.Dictionary.HierarchyTreeDictionaries;

namespace Proryv.AskueARM2.Client.Visual
{
    /// <summary>
    /// Логика взаимодействия для FreeHierarchyTreeFindBar.xaml
    /// </summary>
    public partial class FreeHierarchyTreeFindBar : UserControl, IDisposable, INotifyPropertyChanged
    {
        private FreeHierarchyTree _parent;
        private FreeHierarchyTreeDescriptor _descriptor;

        private EnumFindTiType _findTiType = EnumFindTiType.TIName;

        public EnumFindTiType FindTiType
        {
            get { return _findTiType; }
            set
            {
                if (_findTiType == value) return;

                _findTiType = value;
                _PropertyChanged("FindTiType");
            }
        }

        public FreeHierarchyTreeFindBar(FreeHierarchyTree parent, FreeHierarchyTreeDescriptor descriptor)
        {
            InitializeComponent();
            Init(parent, descriptor);
        }

        private void textFind_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CancelSearch();
            }
        }

        private void CancelSearch()
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

        #region Поиск 

        private DispatcherTimer _searchTimer;
        private CancellationTokenSource cancellFindTokenSource = new CancellationTokenSource();
        private volatile int _timeout = 350;

        private void textFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            CancelSearch();

            if (_searchTimer == null)
            {
                _searchTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(_timeout), DispatcherPriority.Background, OnSearchCallback, this.Dispatcher);
            }

            _searchTimer.Interval = TimeSpan.FromMilliseconds(_timeout);
            _searchTimer.Start();
        }

        private void OnSearchCallback(object sender, EventArgs e)
        {
            if (_searchTimer!=null) _searchTimer.Stop();

            if (_parent == null) return;

            var currentText = textFind.Text;
            var currentParentText = textParentFind.Text;
            FoundedGrid.DataSource = null;

            var typeHierarchy = cbTypeHierarchy.SelectedValue as enumTypeHierarchy?;
            if (typeHierarchy.HasValue && typeHierarchy.Value == enumTypeHierarchy.Unknown)
            {
                typeHierarchy = null;
            }

            if (string.IsNullOrEmpty(currentText) && string.IsNullOrEmpty(currentParentText)) return;

            cancellFindTokenSource = new CancellationTokenSource();
            //Для отмены уже запущенного поиска
            var token = cancellFindTokenSource.Token;

            txtNotFound.Text = "Поиск...";
            txtNotFound.Visibility = System.Windows.Visibility.Visible;

            Task.Factory.StartNew(() => InvokeSearch(currentText, currentParentText, typeHierarchy, token))
                .ContinueWith(t =>
                {
                    Dispatcher.BeginInvoke((Action) (() => { OnSearchCompleted(t.Result); })); 
                }, token);

            //MainLayout.RunAsync(() => InvokeSearch(currentText, token), OnSearchCompleted, cancellationToken: token);
        }

        private Tuple<List<IDHierarchy>, string, CancellationToken> InvokeSearch(string text, string parentText, enumTypeHierarchy? typeHierarchy, CancellationToken token)
        {
            var needFindTI = _descriptor.NeedFindTI;
            var needFindUaNode = _descriptor.NeedFindUaNode;
            var needFindTransformatorsAndreactors = _descriptor.NeedFindTransformatorsAndreactors; //Необходим поиск трансформаторов и реакторов по базе (т.к. еще не подгрузились)
            var freeHierTreeId = _descriptor.Tree_ID ?? -101;
            var findUspdAndE422InTree = _descriptor.ShowUspdAndE422InTree;
            var tiComparer = new IFreeHierarchyObjectComparerTyped();
            var objectComparer = new HierarchyObjectFoundedComparer();
            var foundedObjects = new ConcurrentStack<IDHierarchy>();

#if DEBUG

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif
            Parallel.Invoke(() =>
                {
                    //if (_findTiType == EnumFindTiType.Pik) return;

                    try
                    {
                        //поиск прочих объектов
                        var hierObjects =
                            ARM_Service.TREE_FindHierObject(text, parentText, Manager.User.User_ID, freeHierTreeId, _findTiType.ToString(), findUspdAndE422InTree, typeHierarchy);
                        if (hierObjects != null && hierObjects.Count > 0)
                        {
                            foundedObjects.PushRange(hierObjects.OrderBy(h => h.TypeHierarchy).ThenByDescending(h=>h, objectComparer).ToArray());
                        }
                    }
                    catch (Exception ex)
                    {
                        Manager.UI.ShowMessage(ex.Message);
                    }
                },
                () =>
            {
                if (!needFindTransformatorsAndreactors) return;

                try
                {
                    //Поиск трансформаторов и реакторов
                    var reactorsAndTransformators =  ServiceFactory.ArmServiceInvokeSync<Tuple<List<Hard_PTransformator>, List<Hard_PReactors>>>("TREE_TransformatorOrReactor", text);
                    if (reactorsAndTransformators != null)
                    {

                        if (reactorsAndTransformators.Item1 != null)
                        {
                            reactorsAndTransformators.Item1.ForEach(transformator =>
                            {
                                foundedObjects.Push(
                                    EnumClientServiceDictionary.GetOrAddTransformator(transformator.PTransformator_ID,
                                        transformator));
                            });
                        }

                        if (reactorsAndTransformators.Item2 != null)
                        {
                            reactorsAndTransformators.Item2.ForEach(reactor =>
                            {
                                foundedObjects.Push(
                                    EnumClientServiceDictionary.GetOrAddReactor(reactor.PReactor_ID, reactor));
                            });
                        }
                    }
                }
                catch(Exception ex)
                {
                    Manager.UI.ShowMessage(ex.Message);
                }
            }, 
                () =>
            {
                //Поиск узлов OPC на дереве

                if (!needFindUaNode) return;

                try
                {
                    var opcNodes = UAService.Service.UA_FindNode(text, freeHierTreeId, _findTiType == EnumFindTiType.MeterSerialNumber ? "UANode_ID" : null); // "UANode_ID"
                    if (opcNodes != null && opcNodes.Count > 0)
                    {
                        //Предварительная прогрузка узлов с типами найденных узлов (чтобы визуалка не тормозила)
                        UAHierarchyDictionaries.UANodesDict.Prepare(new HashSet<long>(opcNodes
                            .Where(fn => fn.Node.UATypeNode_ID.HasValue)
                            .Select(fn => fn.Node.UATypeNode_ID.Value)));

                        foundedObjects.PushRange(opcNodes.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    Manager.UI.ShowMessage(ex.Message);
                }
            });

#if DEBUG
            sw.Stop();
            Console.WriteLine("Поиск {0} млс", sw.ElapsedMilliseconds);
#endif

            //Отмена поиска
            if (token.IsCancellationRequested)
            {
                return null;
            }

            //Подготавливаем словари
            Task.Factory.StartNew(()=> FreeHierarchyTreePreparer.PrepareGlobalDictionaries(foundedObjects, token: token));

            return new Tuple<List<IDHierarchy>, string, CancellationToken>(foundedObjects.OrderByDescending(d=>d.TypeHierarchy).ToList(), text, token);
        }

        private void OnSearchCompleted(Tuple<List<IDHierarchy>, string, CancellationToken> seached)
        {
            //textFind.Focus();

            if (seached == null || seached.Item3.IsCancellationRequested) return;

            #region Переключаем раскладку на русскую

            if (seached.Item1 == null || seached.Item1.Count == 0)
            {


                if (seached.Item2 != null && seached.Item2.Length > 3)
                {
                    //Если первые два символа цифры, пробуем искать по номеру счетчика
                    if (_findTiType == EnumFindTiType.TIName && char.IsDigit(seached.Item2[0]) && char.IsDigit(seached.Item2[1]))
                    {
                        //Не нашли, пробуем переключить раскладку на русскую
                        Dispatcher.BeginInvoke(DispatcherPriority.Background,
                            (Action)(() =>
                            {
                                _timeout = 5;
                                txtNotFound.Text = "Поиск по номеру счетсика ...";
                                FindTiType = EnumFindTiType.MeterSerialNumber;
                            }));

                        return;
                    }

                    //Не нашли, пробуем переключить раскладку на русскую
                    Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        (Action)(() =>
                        {
                            string newFindText;
                            if (!ExtFindHierarchyList_Control.PuntoSwitch(seached.Item2, out newFindText)) return;

                            txtNotFound.Text = "Поиск на русском ...";
                            _timeout = 5;
                            textFind.Text = newFindText;
                            textFind.Select(textFind.Text.Length, 0);

                            
                        }));
                }

                txtNotFound.Text = "Ничего не найдено... или обновите дерево ";

                return;
            }

            _timeout = 300;

            txtNotFound.Visibility = System.Windows.Visibility.Collapsed;

            #endregion

            var token = seached.Item3;

            //Отмена поиска
            if (token.IsCancellationRequested)
            {
                return;
            }

            var ids = seached.Item1;

            FoundedGrid.DataSource = ids;

            //FillPathes(seached.Item1, token);
            
            ExpandFirstRecord(true);

            ////Группируем по типу
            //var view = (CollectionView)CollectionViewSource.GetDefaultView(resultList.ItemsSource);
            //var groupDescription = new PropertyGroupDescription("TypeHierarchy");
            //if (view != null)
            //{
            //    view.GroupDescriptions.Add(groupDescription);
            //}
        }

        private void FillPathes(List<IDHierarchy> ids, CancellationToken token)
        {
            if (ids == null) return;

            //Для тех, у кого пути не вернулись при поиске

            var idsWithoutPath = new List<IDHierarchy>();

            foreach (var id in ids)
            {
                if (string.IsNullOrEmpty(id.ToRootPath))
                {
                    idsWithoutPath.Add(id);
                }
            }

            Task.Factory.StartNew(() =>
            {
                //Строим путь к дереву
                var pathes = XamTreeFinder.BuildPathFromSQL(idsWithoutPath.Select(selItem => new ID_TypeHierarchy
                {
                    ID = selItem.ID,
                    StringId = selItem.StringId,
                    FreeHierItemId = selItem.FreeHierItemId,
                    TypeHierarchy = selItem.TypeHierarchy,
                }).ToList(), _descriptor.Tree_ID);

                if (pathes!=null && pathes.Count > 0)
                {
                    return pathes.ToDictionary(k =>
                    {
                        var fp = k.Value.FirstOrDefault();
                        //if (fp == null) return string.Empty;

                        var ffp = fp.FirstOrDefault() as IDHierarchy;
                        //if (ffp == null) return string.Empty;

                        return ffp;
                    }, v => v.Value, new IDHierarchyComparer());
                }

                return null;
            }).ContinueWith(t=>
            {
                if (t.Result == null || t.Result.Count  == 0) return;

                //Отмена поиска
                if (token.IsCancellationRequested)
                {
                    return;
                }

                var pathes = t.Result;

                Dispatcher.BeginInvoke((Action)(() =>
                {
                    foreach (var id in idsWithoutPath)
                    {
                        List<List<ID_TypeHierarchy>> path;

                        if (!pathes.TryGetValue(id, out path) || path == null) continue;

                        id.Path = path.First();

                        var sb = new StringBuilder();

                        for(int i = path.Count - 1; i >= 1; i--)
                        {
                            var p = path[i];
                            if (p == null) continue;

                            sb.Append(p.ToString()).Append("\\");
                        }

                        id.ToRootPath = sb.ToString();
                    }
                }), DispatcherPriority.Background);
            });
        }

        private void ExpandFirstRecord(bool isExpand)
        {
            if (FoundedGrid == null || FoundedGrid.Records.Count == 0) return;

            var row = FoundedGrid.Records.FirstOrDefault();
            //foreach (var row in FoundedGrid.Records)
            {
                if (row != null)
                {
                    row.IsExpanded = isExpand;
                    if (row.RecordManager.Current != null && row.RecordManager.Current.Count > 0)
                    {
                        var cr = row.RecordManager.Current.FirstOrDefault();
                        //foreach (var cr in row.RecordManager.Current)
                        if (cr!=null)
                        {
                            cr.IsExpanded = isExpand;
                        }
                    }
                }
            }
        }

        private void FindTypeButtonOnChecked(object sender, RoutedEventArgs e)
        {
            OnSearchCallback(null, null);
        }

        #endregion

        #region Выбор среди найденных

        private void FoundedGridOnSelectedItemsChanged(object sender, SelectedItemsChangedEventArgs e)
        {
            var xamGrid = sender as DataPresenterBase;
            if (xamGrid == null) return;

            IDHierarchy selItem = null;

            if (xamGrid.SelectedItems.Records.Count > 0)
            {

                var record = xamGrid.SelectedItems.Records[0] as DataRecord;
                if (record == null) return;

                selItem = record.DataItem as IDHierarchy;
                
            }else if (xamGrid.SelectedItems.Cells.Count > 0)
            {
                var cell = xamGrid.SelectedItems.Cells[0] as Cell;
                if (cell == null || cell.Record == null) return;

                selItem = cell.Record.DataItem as IDHierarchy;
            }

            if (selItem == null) return;

            var path = selItem.Path;

            if (path == null)
            {
                //Строим путь к дереву
                var pathes = XamTreeFinder.BuildPathFromSQL(new List<ID_TypeHierarchy>
                {
                    new ID_TypeHierarchy
                    {
                        ID = selItem.ID,
                        StringId = selItem.StringId,
                        FreeHierItemId = selItem.FreeHierItemId,
                        TypeHierarchy = selItem.TypeHierarchy,
                    }
                }, _descriptor.Tree_ID);

                if (pathes!=null && pathes.Count > 0)
                {
                    path = pathes.First().Value.First();
                }
            }

            if (path == null || path.Count == 0)
            {
                return;
            }

            //разворачиваем первый попавшийся
            //Разворачиваем ветку
            _parent.ExpandFromSQL(_parent.tree.Nodes, new ConcurrentStack<ID_TypeHierarchy>(path), true);
        }

        #endregion

        internal static void OpenFreeHierarchyTreeFindBar(FreeHierarchyTree parent, FreeHierarchyTreeDescriptor descriptor)
        {
            FreeHierarchyTreeFindBar findBar;
            //Сначала закрываем предыдущее окно
            if (parent.TreeFindBar != null)
            {
                findBar = parent.TreeFindBar;
                findBar.Init(parent, descriptor);
                var previousPopup = findBar.FindParent<System.Windows.Controls.Primitives.Popup>();
                if (previousPopup!=null && !previousPopup.IsOpen)
                {
                    previousPopup.IsOpen = true;
                }
                return;
            }

            findBar = new FreeHierarchyTreeFindBar(parent, descriptor);
            parent.TreeFindBar = findBar;

            findBar.MakeResizable();
            var tvPlace = new Rect(parent.tree.PointToScreen(new Point(0, 0)),
                parent.tree.PointToScreen(new Point(parent.tree.ActualWidth, parent.tree.ActualHeight)));
            var wa = SystemParameters.WorkArea;

            var popup = new System.Windows.Controls.Primitives.Popup
            {
                Child = findBar,
                StaysOpen = false,
            };

            var isShowNearest = true;
            var ch = parent.FindParent<ILocalChildren>();
            isShowNearest = ch == null;

            popup.PlacementTarget = parent.tree;

            if (isShowNearest)
            {
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
                popup.Placement = PlacementMode.Right;
                popup.HorizontalOffset = -333;
                popup.VerticalOffset = -10;
            }

            if (popup.PlacementTarget != null)
            {
                popup.OpenAndRegister(false);
            }
        }

        public void Init(FreeHierarchyTree parent, FreeHierarchyTreeDescriptor descriptor)
        {
            _parent = parent;
            _descriptor = descriptor;

            Dispatcher.BeginInvoke((Action)(() =>
                {
                    FoundedGrid.DataSource = null;

                    cbTypeHierarchy.ItemsSource = EnumsHelper.TypesDict;

                    #region Прятать или нет поиск по серийному номеру

                    if (_parent != null)
                    {
                        var im = _parent.FindParent<IModule>();
                        if (im != null)
                        {
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
                    }

                    #endregion

                    if (LoginPage.IsShowPik)
                    {
                        byPik.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        byPik.Visibility = Visibility.Collapsed;
                    }
                }));

            Dispatcher.BeginInvoke((System.Action)(() => Keyboard.Focus(textFind)), DispatcherPriority.Background);
        }

        private void butClose_Click(object sender, RoutedEventArgs e)
        {
            this.ClosePopup();
        }

        public void Dispose()
        {
            CancelSearch();
            FoundedGrid.DataSource = null;
            if (_parent!=null)
            {
                _parent.TreeFindBar = null;
                _parent = null;
            }
            _descriptor = null;
        }

        private void FreeHierarchyTreeFindBarOnUnloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }

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

        private void CbTypeHierarchy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            OnSearchCallback(null, new EventArgs());
        }
    }
}
