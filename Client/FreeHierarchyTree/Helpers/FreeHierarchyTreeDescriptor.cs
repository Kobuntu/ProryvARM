using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.Interfaces;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.ServiceReference.Service.Data;
using Proryv.AskueARM2.Client.ServiceReference.Service.Interfaces;
using Proryv.AskueARM2.Client.ServiceReference.StimulReportService;
using EnumFreeHierarchyItemType = Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService.EnumFreeHierarchyItemType;

namespace Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy
{
    /// <summary>
    /// Общие св-ва дерева
    /// </summary>
    [DataContract]
    public class FreeHierarchyTreeDescriptor : IDisposable
    {
        public FreeHierarchyTreeDescriptor()
        {
            SelectedItems = new Dictionary<int, FreeHierarchyTreeItem>();
        }

        private int? _treeId = -101;

        [DataMember]
        public int? Tree_ID
        {
            get
            {
                if (FreeHierarchyTree != null)
                {
                    return FreeHierarchyTree.Tree_ID;
                }

                return _treeId;
            }

            set
            {
                _treeId = value;
            }
        }

        /// <summary>
        /// Дерево
        /// </summary>
        public IFreeHierarchyTree FreeHierarchyTree;

        /// <summary>
        /// Выбранные объекты
        /// </summary>
        public Dictionary<int, FreeHierarchyTreeItem> SelectedItems;

        /// <summary>
        /// Фильтр по которому отсеиваем ненужные объекты из дерева
        /// </summary>
        [DataMember]
        public EnumTIStatus? FilterStatus;

        [DataMember]
        public int TreeHashId;

        /// <summary>
        /// Минимальный идентификатор в дереве
        /// </summary>
        [DataMember]
        private volatile int _minId;

        public int GetMinIdAndDecrement()
        {
            return Interlocked.Decrement(ref _minId);
        }

        /// <summary>
        /// Локер для синхронизации словарей TIbyPS и TIHierarchyList
        /// </summary>
        private static readonly ReaderWriterLockSlim _treeItemsSyncLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private static readonly TimeSpan _minLockWait = new TimeSpan(0, 0, 3);
        private static readonly TimeSpan _maxLockWait = new TimeSpan(0, 5, 0);

        /// <summary>
        /// Словарь всех объектов
        /// </summary>
        [DataMember]
        public ConcurrentDictionary<int, FreeHierarchyTreeItem> Tree;

        /// <summary>
        /// Событие на выделение элемента
        /// </summary>
        public event Action SelectedChanged;

        [DataMember]
        private Dictionary<int, ServiceReference.ARM_20_Service.Info_Section_List> sectionDetailsCache;
        public Dictionary<int, ServiceReference.ARM_20_Service.Info_Section_List> SectionDetailsCache
        {
            get
            {
                if (sectionDetailsCache == null) sectionDetailsCache = new Dictionary<int, ServiceReference.ARM_20_Service.Info_Section_List>();
                return sectionDetailsCache;
            }
        }

        [DataMember]
        private Dictionary<int, NSI_TP> tpFormulaDetailsCache;
        public Dictionary<int, NSI_TP> TPFormulaDetailsCache
        {
            get
            {
                if (tpFormulaDetailsCache == null) tpFormulaDetailsCache = new Dictionary<int, NSI_TP>();
                return tpFormulaDetailsCache;
            }
        }

        internal void RaiseSelectedChanged()
        {
            if (SelectedChanged == null) return;

            SelectedChanged();
        }

         /// <summary>
         /// Скрыть невыбранные элементы
         /// </summary>
        public void HideUnselected()
        {
            if (Tree != null)
            {
                foreach (var item in Tree.Values)
                {
                    item.Visibility = (!item.IsSelectedChildren && !item.IsSelected) ? Visibility.Collapsed
                        : Visibility.Visible;
                }
            }

        }

        [DataMember]
        private double? _voltageFilter;

        public double? VoltageFilter
        {
            get { return _voltageFilter; }
            set
            {
                _voltageFilter = value;
                ApplyVoltageFilter(_voltageFilter);
            }
        }

        /// <summary>
        /// Скрыть невыбранные элементы
        /// </summary>
        private void ApplyVoltageFilter(double? voltageFilter)
        {
            if (Tree == null) return;
            foreach (FreeHierarchyTreeItem item in Tree.Values)
            {
                if (item.FreeHierItemType == EnumFreeHierarchyItemType.TI)
                {
                    var ti = item.GetItemForSearch() as TInfo_TI;
                    if (ti != null)
                    {
                        if (!voltageFilter.HasValue)
                        {
                            if (item.Visibility != Visibility.Visible)
                            {
                                item.Visibility = Visibility.Visible;
                            }
                        }
                        else if (ti.Voltage != voltageFilter) item.Visibility = Visibility.Collapsed;
                        else item.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        /// <summary>
        /// Признак выбора элемента пользователем
        /// </summary>
        [DataMember]
        public bool IsUserClicked = true;

        /// <summary>
        /// Флаг отображения только выбранных
        /// </summary>
        [DataMember]
        public bool ShowOnlySelected = false;

        /// <summary>
        /// При сортировке использовать АРМовские настройки
        /// </summary>
        [DataMember]
        public bool IsARMSorting = true;

        /// <summary>
        /// Режим выбора только одного элемента в дереве
        /// </summary>
        [DataMember]
        public bool IsSelectSingle;

        /// <summary>
        /// Отображать УСПД и Е422 в дереве
        /// </summary>
        [DataMember]
        public bool ShowUspdAndE422InTree;

        /// <summary>
        /// Признак того, что надо прятать чекбокс у родителей которые показывает что выбраны дочерние объекты
        /// </summary>
        [DataMember]
        public bool IsHideSelectMany;

        /// <summary>
        /// Необходимо вернуть дочерние объекты
        /// </summary>
        [DataMember]
        public bool NeedReturnChildren = true;

        /// <summary>
        /// Необходим поиск ТИ в БД
        /// </summary>
        [DataMember]
        public bool NeedFindTI;

        /// <summary>
        /// Необходим поиск трансформаторов и реакторов по БД
        /// </summary>
        [DataMember]
        public bool NeedFindTransformatorsAndreactors;

        /// <summary>
        /// Необходим поиск узлов OPC UA
        /// </summary>
        [DataMember]
        public bool NeedFindUaNode;

        ///// <summary>
        ///// Стиль обычного чек-бокса
        ///// </summary>
        //public Style OneSelected;

        ///// <summary>
        ///// Стиль чек-бокса, у которого все дети выбраны
        ///// </summary>
        //public Style AllSelected;

        ///// <summary>
        ///// Стиль чек-бокса, у которого не все дети выбраны
        ///// </summary>
        //public Style PartSelected;

        ///// <summary>
        ///// Шаблон выпадающего меню
        ///// </summary>
        //public DataTemplate ContextMenu;

        /// <summary>
        /// Делегат на функцию, сортирующую элементы согласно настройкам АРМа
        /// </summary>
        public static Sorter<FreeHierarchyTreeItem> Sort;

        /// <summary>
        /// Предыдущий выбранный объект
        /// </summary>
        public FreeHierarchyTreeItem PreviousSelected = null;

        public bool IsLoadOurFormulas;

        public bool IsHideTp;

        public void Dispose()
        {
            try
            {
                FreeHierarchyTree = null;

                SelectedChanged = null;

                SelectedItems = null;
                
                //ContextMenu = null;
                //if (Tree != null && Tree.Count > 0)
                //{
                //    foreach (var t in Tree.Values.ToList())
                //    {
                //        if (t == null) continue;

                //        t.Dispose();
                //    }
                //    Tree = null;
                //}

                if (PreviousSelected != null)
                {
                    PreviousSelected.Dispose();
                    PreviousSelected = null;
                }

                if (Tree != null) Tree = null;
            }
            catch
            {

            }
        }

        #region Работы с выделением/снятием и подсчетом выделенных

        /// <summary>
        /// Основная процедура группового выделения объектов на дереве
        /// </summary>
        /// <param name="isSelect">Выделить/снять</param>
        /// <param name="isRecursive">Включая дочерние</param>
        /// <param name="items">Список объектов</param>
        /// <param name="itemType">Тип объекта для выбора</param>
        public void SelectUnselect(bool isSelect, bool isRecursive, IEnumerable<FreeHierarchyTreeItem> items,
           EnumFreeHierarchyItemType? itemType = null, int? maxlevel = null)
        {
            if (items == null) return;

            //Объекты, которые нужно предварительно подготовить, прежде чем выделять там дочерние
            var itemsForPrepare = new List<FreeHierarchyTreeItem>();
            //Объекты у которых меняются чек боксы о том что там выделены дочерние
            var itemsByLevForUpdateChekbox = new Dictionary<int, HashSet<int>>();
            //Объекты у которых нужно уведомить визуальку что изменились статусы
            var selectedItems = new List<FreeHierarchyTreeItem>();

            if (!_treeItemsSyncLock.TryEnterWriteLock(_maxLockWait)) return;

            try
            {
                var currLevel = 0;

                //Выделяем доступные объекты
                foreach (var treeItem in items.OrderByDescending(i => i.HierLevel))
                {
                    treeItem.SelectUnselect(isSelect, itemsForPrepare, itemsByLevForUpdateChekbox, isRecursive, itemType, maxlevel, selectedItems);
                }

                if (isSelect)
                {
                    //foreach(var itemsForPrepareGroupByType in itemsForPrepare
                    //    .GroupBy(g=>g.FreeHierItemType)
                    //    .OrderByDescending(o=>o.Key))

                    //Подготавливаем недоступные объекты и потом выделяем
                    //Рекурсивная подготовка дочерних объектов
                    do
                    {
                        //Подгружаем внутренние словари для определения количества
                        FreeHierarchyTreePreparer.PrepareGlobalDictionaries(itemsForPrepare, EnumFreeHierarchyTreePrepareMode.All);

                        //Формируем дочерние узлы у прогруженных объектов, для того, чтобы идти дальше вниз по дереву
                        LoadDynamicChildren(itemsForPrepare);

                        var newitems = itemsForPrepare.ToList();
                        itemsForPrepare.Clear();

                        foreach (var item in newitems)
                        {
                            item.SelectChildren(true, itemsForPrepare, itemsByLevForUpdateChekbox, isRecursive, itemType, true,
                                selectedItems: selectedItems,  currLevel: currLevel, maxlevel: maxlevel);
                        }

                        currLevel++;

                    } while (itemsForPrepare.Count > 0);
                }
            }
            catch (Exception ex)
            {
                Manager.UI.ShowMessage(ex.ToString());
            }
            finally
            {
                _treeItemsSyncLock.ExitWriteLock();
            }

            //Уведомляем визуалку что изменились состояния объектов на дереве
            FreeHierarchyTree.InvokeAsync(() =>
            {
                //Проставляем чек боксы на родителя у которых выделены дочерние
                ProcessParentCheckboxes(itemsByLevForUpdateChekbox);

                //Уведомляем визуалку что изменились статусы выделения
                RaiseSelectedItemsChanged(selectedItems);
            });
        }

        #region Выделяем дочерние объекты из построенного на сервере пути

        /// <summary>
        /// Выделяем дочерние объекты из построенного на сервере пути
        /// </summary>
        /// <param name="items">объекты по которыми проходимся и выделяем</param>
        /// <param name="objectsFromSQL">объекты с путями, которые нужно выделить</param>
        public void SelectFromSets(IEnumerable<FreeHierarchyTreeItem> items,
            Dictionary<string, List<List<ID_TypeHierarchy>>> objectsFromSQL, bool isExpandFirst = false)
        {
            if (items == null || objectsFromSQL == null || objectsFromSQL.Count == 0) return;

            //Объекты, которые нужно предварительно подготовить, прежде чем выделять там дочерние
            var itemsForPrepare = new List<FreeHierarchyTreeItem>();
            //Объекты у которых меняются чек боксы о том что там выделены дочерние
            var itemsByLevForUpdateChekbox = new Dictionary<int, HashSet<int>>();
            //Объекты у которых нужно уведомить визуальку что изменились статусы
            var selectedItems = new List<FreeHierarchyTreeItem>();

            var firstPath = new List<ID_TypeHierarchy>();
            firstPath.AddRange(objectsFromSQL.First().Value.First());

            if (!_treeItemsSyncLock.TryEnterWriteLock(_maxLockWait)) return;

#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            try
            {
                //Сначала выделяем доступные объекты

                var fromSets = SelectRecursiveFromSets(items, objectsFromSQL, itemsForPrepare, selectedItems, itemsByLevForUpdateChekbox);

                objectsFromSQL = fromSets
                    .GroupBy(sp =>
                    {
                        if (sp == null || sp.Count == 0) return string.Empty;
                        return sp.Last().ToRootPath;
                    })
                    .Where(g => !string.IsNullOrEmpty(g.Key))
                    .ToDictionary(k => k.Key, v => v.ToList());

#if DEBUG
                sw.Stop();
                Console.WriteLine("FreeHierarchyTreeDescriptor, Select {0} млс", sw.ElapsedMilliseconds);
                sw.Restart();
#endif
                //Подготавливаем недоступные объекты и потом выделяем
                //Рекурсивная подготовка дочерних объектов
                do
                {
                    //Подгружаем внутренние словари для определения количества
                    FreeHierarchyTreePreparer.PrepareGlobalDictionaries(itemsForPrepare, EnumFreeHierarchyTreePrepareMode.All);

                    //Формируем дочерние узлы у прогруженных объектов, для того, чтобы идти дальше вниз по дереву
                    LoadDynamicChildren(itemsForPrepare);

                    var newitems = itemsForPrepare.ToList();
                    itemsForPrepare.Clear();

                    fromSets = SelectRecursiveFromSets(newitems, objectsFromSQL, itemsForPrepare, selectedItems, itemsByLevForUpdateChekbox);

                    //Теперь выделяем подготовленные объекты
                    objectsFromSQL = fromSets
                        .Where(sp => sp.Count > 0)
                        .GroupBy(sp => sp.Last().ToRootPath)
                        .ToDictionary(k => k.Key, v => v.ToList());

                } while (itemsForPrepare.Count > 0); //Идем дальше вниз по дереву, пока не останется объектов для выделения, либо пока не кончится дерево

#if DEBUG
                sw.Stop();
                Console.WriteLine("FreeHierarchyTreeDescriptor, PrepareParentsAndSelectChildrenAsync {0} млс", sw.ElapsedMilliseconds);
#endif

            }
            catch (Exception ex)
            {
                Manager.UI.ShowMessage(ex.ToString());
            }
            finally
            {
                _treeItemsSyncLock.ExitWriteLock();
            }


            //Уведомляем визуалку что изменились состояния объектов на дереве
            FreeHierarchyTree.InvokeAsync(() =>
            {
                //Проставляем чек боксы на родителя у которых выделены дочерние
                ProcessParentCheckboxes(itemsByLevForUpdateChekbox);

                //Уведомляем визуалку что изменились статусы выделения
                RaiseSelectedItemsChanged(selectedItems);

                FreeHierarchyTree.DescOnSelectedChanged();

                if (isExpandFirst)
                {
                    var path = new ConcurrentStack<ID_TypeHierarchy>(firstPath);
                    FreeHierarchyTree.ExpandFromSQL(FreeHierarchyTree.NodeCollection, path, false);
                }
            }, DispatcherPriority.DataBind);
        }

        /// <summary>
        /// Обрабатываем дерево, выделяем найденные, объекты которые нужно подготовить собираем в отдельную коллекцию
        /// </summary>
        /// <param name="items">Объекты из дерева, которые помечаем</param>
        /// <param name="objectsFromSQL">Объекты с путями загруженные из SQL</param>
        /// <param name="itemsForPrepare">Коллекция куда собираем объекты, которые нужно предварительно подготовить, после подготовки проходимся по ним</param>
        /// <param name="selectedItems">Колекция с отмеченными объектами из дерева, нужна для дальнейшего уведомления визуалки</param>
        /// /// <param name="itemsByLevForUpdateChekbox">Объекты у которых меняются чек боксы о том что там выделены дочерние</param>
        private List<List<ID_TypeHierarchy>> SelectRecursiveFromSets(IEnumerable<FreeHierarchyTreeItem> items,
            Dictionary<string, List<List<ID_TypeHierarchy>>> objectsFromSQL, List<FreeHierarchyTreeItem> itemsForPrepare,
            List<FreeHierarchyTreeItem> selectedItems, Dictionary<int, HashSet<int>> itemsByLevForUpdateChekbox)
        {
            var objectsFromSQLAfterPrepare = new List<List<ID_TypeHierarchy>>();

            foreach (var item in items) //Перебираем объекты на одной ветке на дереве
            {
                
                var objectsByFreeItem = new List<List<ID_TypeHierarchy>>();
                //var objetsFoRemove = new List<string>();

                List<List<ID_TypeHierarchy>> pathesByParent;
                if (objectsFromSQL.TryGetValue(item.StringId, out pathesByParent)
                    && pathesByParent != null && pathesByParent.Count > 0)
                {


                    foreach (var path in pathesByParent)
                    {
                        if (path == null || path.Count == 0) continue;

                        //var p = path[path.Count - 1];
                        //if (p != null && item.Equals(p))
                        {
                            if (path.Count == 1)
                            {
                                //item конечный объект, добавляем в список для выделения
                                item.SelectUnselect(true, itemsForPrepare, itemsByLevForUpdateChekbox, false, selectedItems: selectedItems);
                            }
                            else
                            {
                                //Нужно идти дальше по дочерним
                                objectsByFreeItem.Add(path);
                            }
                        }
                    }
                }
                //Удалем объекты из SQL, которые выделили, они уже не нужны для дальнейшей обработки
                //foreach (var pathForRemove in objetsFoRemove)
                //{
                //    objectsFromSQL.Remove(pathForRemove);
                //}

                if (objectsByFreeItem.Count == 0) continue; //Дальше в дочерние нет смысла лезть

                if (!item.IsChildrenInitializet)
                {
                    //if (item.IncludeObjectChildren || FreeHierarchyTreeItem.DynamicLoadedParent.Contains(item.FreeHierItemType))
                    //{
                    itemsForPrepare.Add(item);
                    //}
                    //else
                    //{
                    //item.LoadDynamicChildren(isLoadFromServer: true);
                    //}

                    objectsFromSQLAfterPrepare.AddRange(objectsByFreeItem);

                    continue;
                }

                //Объект нашли и выделили, дальше его обрабатывать нет смысла, помечаем на удаление
                objectsFromSQL.Remove(item.StringId);

                //Дочерние полностью прогружены, идем дальше по ним
                if (item.Children != null)
                {
                    foreach (var path in objectsByFreeItem)
                    {
                        //    //Удаляем у каждого объекта снизу текущего родителя, внизу остается следующий по которому идем дальше
                        path.RemoveAt(path.Count - 1);
                        //    if (path.Count == 0)
                        //    {
                        //        objectsFromSQL.Remove(path);
                        //    }
                    }

                    //Здесь идем дальше в дочерние узлы
                    objectsFromSQLAfterPrepare.AddRange(SelectRecursiveFromSets(item.Children
                        , objectsByFreeItem
                        .Where(sp => sp.Count > 0)
                        .GroupBy(sp => sp.Last().ToRootPath)
                        .ToDictionary(k => k.Key, v => v.ToList())
                        , itemsForPrepare, selectedItems, itemsByLevForUpdateChekbox));
                }
            }

            return objectsFromSQLAfterPrepare;
        }

        #endregion

        #region Цикличное выделение объеков

        /// <summary>
        /// Помечаем родителей у которых выбраны объекты, т.е. здесь смотрим только на дочерние объекты
        /// </summary>
        public void ProcessParentCheckboxes(Dictionary<int, HashSet<int>> itemsForUpdateChekbox)
        {
            var parentsByLev = new Dictionary<int, HashSet<int>>();

            foreach (var idPair in itemsForUpdateChekbox.OrderByDescending(i => i.Key))
            {
                foreach (var id in idPair.Value)
                {
                    FreeHierarchyTreeItem item;
                    if (Tree==null || !Tree.TryGetValue(id, out item) || item == null) continue;

                    if (item.SelectedChildrenCount <= 0)
                    {
                        item.CheckBoxStyle = EnumSelectedManyCheckBoxStyle.None;
                    }
                    else if (item.SelectedChildrenCount == item.Children.Count)
                    {
                        item.CheckBoxStyle = EnumSelectedManyCheckBoxStyle.AllSelected;
                    }
                    else
                    {
                        item.CheckBoxStyle = EnumSelectedManyCheckBoxStyle.PartSelected;
                    }

                    if (item.Parent != null)
                    {
                        //Проверяем есть ли у родителя собственные выбранные объекты, если есть, то протаскивать по цепочке не нужно
                        HashSet<int> upperParents;
                        if (!itemsForUpdateChekbox.TryGetValue(item.HierLevel - 1, out upperParents)
                            || upperParents == null || !upperParents.Contains(item.Parent.FreeHierItem_ID))
                        {
                            //Накапливаем родителей, что протащить статус выранных вверх по цепочке
                            HashSet<int> parents;
                            if (!parentsByLev.TryGetValue(item.Parent.HierLevel, out parents))
                            {
                                parents = new HashSet<int>();
                                parentsByLev[item.Parent.HierLevel] = parents;
                            }

                            parents.Add(item.Parent.FreeHierItem_ID);
                        }
                    }
                }
            }

            ProcessParentParenCheckboxes(parentsByLev);
        }

        #endregion

        #region Прогрузка узлов у большого количества объектов

        /// <summary>
        /// Формирование дочерних объектов сразу по нескольким узлам
        /// </summary>
        /// <param name="tempList">Список объектов, по которым прогружаем дочерние</param>
        /// <param name="descriptor">Описатель дерева</param>
        public void LoadDynamicChildren(IEnumerable<FreeHierarchyTreeItem> tempList)
        {
#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            var comparer = new FreeHierarchyTreeItemComparer();

            //Это стандартные деревья
            foreach (var item in tempList)
            {
                if (item.IncludeObjectChildren)
                {
                    if (Tree_ID <= 0)
                    {
                        item.IsFreeHierLoadedInitializet = true;
                    }

                    var brunch = item.AddStandartChildren(false, true, false, false);
                    if (brunch != null) item.Children.AddRange(brunch.OrderBy(b=>b, comparer));
                }

                item.IsLocalChildrenInitializet = true;
                if (Tree_ID <= 0)
                {
                    item.IsChildrenInitializet = true;
                }
            }

#if DEBUG
            sw.Stop();
            Console.WriteLine("LoadDynamicChildren: стандартные деревья {0} млс", sw.ElapsedMilliseconds);
            sw.Restart();
#endif

            if (Tree_ID <= 0)
            {
                return;
            }

            //Пока работает кроме ФИАС

            var parendIds = tempList
                .Where(tl => !tl.IsFreeHierLoadedInitializet)
                .Select(tl => tl.FreeHierItem_ID)
                .ToList();

            var branches = FreeHierarchyService.GetBranches(Manager.User.User_ID, Tree_ID.GetValueOrDefault(), parendIds, false);
            if (branches == null) return;

            var itemsForPrepare = new List<TFreeHierarchyTreeItem>();
            foreach (var branch in branches)
            {
                if (branch.Value == null) continue;

                itemsForPrepare.AddRange(branch.Value);
            }

#if DEBUG
            sw.Stop();
            Console.WriteLine("LoadDynamicChildren: GetBranches {0} млс", sw.ElapsedMilliseconds);
            sw.Restart();
#endif

            FreeHierarchyTreePreparer.PrepareGlobalDictionaries(itemsForPrepare);
                        
            foreach (var item in tempList)
            {
                if (item.IsFreeHierLoadedInitializet) continue;

                List<TFreeHierarchyTreeItem> children;
                if (branches.TryGetValue(item.FreeHierItem_ID, out children) && children != null)
                {
                    var brunch = FreeHierarchyTreePreparer.BuildBranch(item, children, this, false, null, false);
                    if (brunch != null) item.Children.AddRange(brunch.OrderBy(b => b, comparer));
                }

                item.IsChildrenInitializet = true;
            }

#if DEBUG
            sw.Stop();
            Console.WriteLine("LoadDynamicChildren: PrepareGlobalDictionaries and BuildBranch {0} млс", sw.ElapsedMilliseconds);
#endif
        }

        #endregion

        /// <summary>
        /// Помечаем родителей родителей, т.е. здесь логика передачи статуса вверх по цепочке
        /// </summary>
        /// <param name="parentsByLev"></param>
        private void ProcessParentParenCheckboxes(Dictionary<int, HashSet<int>> parentsByLev)
        {
            do
            {
                var localParentsByLev = new Dictionary<int, HashSet<int>>();
                bool haveNone = false, haveAll = false, havePartial = false;

                foreach (var idPair in parentsByLev.OrderByDescending(i => i.Key))
                {
                    foreach (var id in idPair.Value)
                    {
                        FreeHierarchyTreeItem item;
                        if (!Tree.TryGetValue(id, out item) || item == null) continue;

                        if (item.Parent != null)
                        {
                            HashSet<int> upperParents;
                            if (!parentsByLev.TryGetValue(item.HierLevel - 1, out upperParents)
                                || upperParents == null || !upperParents.Contains(item.Parent.FreeHierItem_ID))
                            {

                                HashSet<int> parents;
                                if (!localParentsByLev.TryGetValue(item.Parent.HierLevel, out parents))
                                {
                                    parents = new HashSet<int>();
                                    localParentsByLev[item.Parent.HierLevel] = parents;
                                }

                                parents.Add(item.Parent.FreeHierItem_ID);
                            }
                        }

                        if (!item.HasChildren || item.Children == null || item.Children.Count == 0)
                        {
                            continue;
                        }

                        foreach (var ch in item.Children)
                        {
                            if (ch.CheckBoxStyle == EnumSelectedManyCheckBoxStyle.PartSelected)
                            {
                                havePartial = true;
                                break;
                            }

                            if (!haveNone)
                            {
                                haveNone = ch.CheckBoxStyle == EnumSelectedManyCheckBoxStyle.None; //Возможно нужно добавить ch.HasChildren && 
                            }

                            if (!haveAll)
                            {
                                haveAll = ch.CheckBoxStyle == EnumSelectedManyCheckBoxStyle.AllSelected;
                            }

                            if (haveNone && haveAll) break;
                        }

                        if (havePartial || (haveAll && haveNone))
                        {
                            item.CheckBoxStyle = EnumSelectedManyCheckBoxStyle.PartSelected;
                        }
                        else if (haveAll)
                        {
                            item.CheckBoxStyle = EnumSelectedManyCheckBoxStyle.AllSelected;
                        }
                        else
                        {
                            item.CheckBoxStyle = EnumSelectedManyCheckBoxStyle.None;
                        }
                    }
                }

                parentsByLev = localParentsByLev;

            } while (parentsByLev.Count > 0);
        }


        /// <summary>
        /// Объект, возвращающий перечень доступных для выбора объектов на дереве
        /// </summary>
        public IPermissibleForSelectObjects PermissibleForSelectObjects;

        /// <summary>
        /// Уведомляем визуалку, что изменился статус выбранных объектов
        /// </summary>
        /// <param name="selectedItems"></param>
        public void RaiseSelectedItemsChanged(List<FreeHierarchyTreeItem> selectedItems)
        {
            foreach(var item in selectedItems)
            {
                item.RaiseIsSelectedChanged();
            }
        }

        /// <summary>
        /// Отменить выбор
        /// </summary>
        /// <param name="freeHierItemIds">Идентификаторы отменяемых</param>
        /// <returns></returns>
        public bool Unselect(IEnumerable<int> freeHierItemIds = null, bool isRecursive = false)
        {
            //var needHidePanel = false;
            //if (!_treeItemsSyncLock.TryEnterReadLock(_minLockWait) && FreeHierarchyTree!=null)
            //{
            //    needHidePanel = true;
            //    Manager.UI.ShowWaitPanelStatic(FreeHierarchyTree.GetVisualTree(), TimeSpan.FromMilliseconds(2));
            //}
            //else
            //{
            //    _treeItemsSyncLock.ExitReadLock();
            //}

            if (SelectedItems == null || !_treeItemsSyncLock.TryEnterUpgradeableReadLock(_maxLockWait)) return false;

            try
            {

                if (freeHierItemIds == null)
                {
                    //Убираем выбранные со всех
                    SelectUnselect(false, isRecursive, SelectedItems.Values.ToList());
                }
                else
                {
                    var items = new List<FreeHierarchyTreeItem>();

                    //Убираем с конкретных
                    foreach (var freeHierItemId in freeHierItemIds)
                    {
                        FreeHierarchyTreeItem treeItem;
                        if (SelectedItems.TryGetValue(freeHierItemId, out treeItem)
                            && treeItem != null)
                        {
                            items.Add(treeItem);
                        }
                    }

                    SelectUnselect(false, isRecursive, items);
                }
            }
            finally
            {
                _treeItemsSyncLock.ExitUpgradeableReadLock();
                //if (needHidePanel && FreeHierarchyTree != null)
                //{
                //    Manager.UI.HideWaitPanelStatic(FreeHierarchyTree.GetVisualTree());
                //}
            }

            return true;
        }

        /// <summary>
        /// Считаем количество выбранных, заодно возвращаем выбранные
        /// </summary>
        /// <param name="selected">В эту коллекцию собираем выбранные</param>
        /// <param name="typesForSelection">Типы для выбора по типам</param>
        /// <param name="aditionalWherePredicate">Дополнительное условие выбора (where для linq)</param>
        /// <returns>Возвращаем количество выбранных</returns>
        public int GetSelected(List<FreeHierarchyTreeItem> selected = null, //Если null, то просто считаем количество выбранных
            HashSet<EnumFreeHierarchyItemType> typesForSelection = null, //Если null, то выбираем все типы объектов
            Func<FreeHierarchyTreeItem, bool> aditionalWherePredicate = null)
        {
            var needHidePanel = false;
            if (!_treeItemsSyncLock.TryEnterReadLock(_minLockWait) && FreeHierarchyTree != null)
            {
                needHidePanel = true;
                Manager.UI.ShowWaitPanelStatic(FreeHierarchyTree.GetVisualTree(), TimeSpan.FromMilliseconds(2));
            }
            else
            {
                _treeItemsSyncLock.ExitReadLock();
            }

            var result = 0;

            if (_treeItemsSyncLock.TryEnterUpgradeableReadLock(_maxLockWait))
            {
                try
                {
                    if (selected != null && SelectedItems.Count > 0)
                    {
                        //собираем выбранные, иначе просто считаем
                        selected.AddRange(SelectedItems
                            .Values
                            .Where(si => (typesForSelection == null || typesForSelection.Contains(si.FreeHierItemType))
                            && (aditionalWherePredicate == null || aditionalWherePredicate(si))));
                    }

                    result = SelectedItems.Count;
                }
                finally
                {
                    _treeItemsSyncLock.ExitUpgradeableReadLock();
                }
            }

            if (needHidePanel && FreeHierarchyTree != null)
            {
                Manager.UI.HideWaitPanelStatic(FreeHierarchyTree.GetVisualTree());
            }

            return result;
        }

        public void RemoveAddSelected(bool isAdd, FreeHierarchyTreeItem item)
        {
            if (_treeItemsSyncLock.TryEnterWriteLock(TimeSpan.FromSeconds(5)))
            {
                try
                {
                    if (isAdd)
                    {
                        SelectedItems[item.FreeHierItem_ID] = item;
                    }
                    else
                    {
                        SelectedItems.Remove(item.FreeHierItem_ID);
                    }
                }
                finally
                {
                    _treeItemsSyncLock.ExitWriteLock();
                }
            }
        }

        public ConcurrentStack<int> PsForInit;

        /// <summary>
        /// Строим коллекцию выбранных объектов
        /// </summary>
        private void BuildSelectedCollection(List<FreeHierarchyTreeItem> selected,
            HashSet<int> psForInit, ref int count, Func<FreeHierarchyTreeItem, bool> aditionalPredicate)
        {
            if (Tree == null) return;

            var items = Tree.Values.ToList();

            foreach (var item in items)
            {
                if (item.IsSelectedChildren //помечено что выделены дочерние
                    && !item.IsChildrenInitializet //но помечено, что они не подгружены
                    && item.IncludeObjectChildren
                    && item.FreeHierItemType == EnumFreeHierarchyItemType.PS) //Они подгружаются динамически
                {
                    //Добавляем в список ПС, которые позже проинициализируем
                    psForInit.Add(item.HierObject.Id);
                }

                if (item.IsSelected
                    && item.HierObject != null
                    && item.IsSelectableByPermissibleSettings
                    && (aditionalPredicate == null || aditionalPredicate(item)))
                {
                    if (selected != null) selected.Add(item);
                    count++;
                }
            }
        }

        /// <summary>
        /// Формируем классы для еще неподгруженных объектов. Эти объекта не прогружены в дереве
        /// </summary>
        /// <param name="hierarchyItemType"></param>
        /// <param name="psForInit"></param>
        /// <param name="typesForSelection"></param>
        /// <returns></returns>
        private void BuildUnloadedChildren(List<FreeHierarchyTreeItem> selected, HashSet<int> psForInit,
            HashSet<EnumFreeHierarchyItemType> typesForSelection, ref int count)
        {
            #region Инициализируем ТИ

            if (typesForSelection.Contains(EnumFreeHierarchyItemType.TI))
            {
                EnumClientServiceDictionary.TIbyPS.Prepare(psForInit, Manager.UI.ShowMessage); //Подгрузка ТИ для выбранных ПС
                foreach (var psId in psForInit)
                {
                    var tis = EnumClientServiceDictionary.TIbyPS[psId];
                    if (tis != null)
                    {
                        if (selected != null)
                        {
                            foreach (var ti in tis)
                            {
                                selected.Add(new FreeHierarchyTreeItem(this, ti, notLoaded: true, freeHierItemType: EnumFreeHierarchyItemType.TI));
                            }
                        }

                        count += tis.Count;
                    }
                }
            }

            #endregion 

            if (typesForSelection.Contains(EnumFreeHierarchyItemType.USPD))
            {
                FreeHierarchyDictionaries.UspdByPsDict.Prepare(psForInit, Manager.UI.ShowMessage);
                foreach (var psId in psForInit)
                {
                    var uspds = FreeHierarchyDictionaries.UspdByPsDict[psId];
                    if (uspds != null)
                    {
                        if (selected != null)
                        {
                            foreach (var uspdId in uspds)
                            {
                                var uspd = FreeHierarchyDictionaries.USPDDict[uspdId];
                                if (uspd != null)
                                {
                                    selected.Add(new FreeHierarchyTreeItem(this, uspd, notLoaded: true, freeHierItemType: EnumFreeHierarchyItemType.USPD));
                                }
                            }
                        }

                        count += uspds.Count;
                    }
                }
            }
        }



        #endregion

        #region Помошник для раскрытия ветки на дереве

        public void ExpandAndSelect(IFreeHierarchyObject freeHierarchyTreeItem, bool isSelect = true)
        {
            FreeHierarchyTree.ExpandAndSelect(freeHierarchyTreeItem, false, isSelect);
        }

        #endregion

        /// <summary>
        /// Обновление таблицы Dict_FreeHierarchyIncludedObjectChildren (кэш поиска и построения пути в дереве)
        /// </summary>
        public void UpdateIncludedObjectChildrenAsync(string userId)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    FreeHierarchyService.UpdateIncludedObjectChildren(userId, Tree_ID.GetValueOrDefault());
                }
                catch
                {

                }
            });
        }
    }

    public delegate IEnumerable<T1> Sorter<T1>(IEnumerable<T1> list);
}
