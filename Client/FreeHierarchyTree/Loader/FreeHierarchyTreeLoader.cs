using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Proryv.AskueARM2.Client.ServiceReference.Data.FreeHierarchy;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.Configuration;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree.Loader;
using Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree.TreeSelector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree
{
    /// <summary>
    /// Библиотеки для начальной инициализации дерева и выделения выбранных объектов при прошлом закрытии
    /// </summary>
    public partial class FreeHierarchyTree
    {
        /// <summary>
        /// Ждем подгрузки дерева, инициализируем
        /// </summary>
        public void LoadTreeAndInit(FreeHierarchyTypeTreeItem freeHierarchyTypeTreeItem, bool isFullReload = false, 
            HashSet<int> singreRootFreeHierItemIds = null)
        {
            #region Очищаем старое дерево

            var items = Items;

            Items = null;

            string previousSets = null;

            if (descriptor != null)
            {
                if (descriptor.FreeHierarchyTree!=null)
                {
                    //Запоминаем выбранные на прошлом дереве объекты
                    previousSets = descriptor.FreeHierarchyTree.GetSelectedToSet();
                }

                descriptor.Dispose();
                descriptor = null;
            }

            if (items != null)
            {
                items.DisposeChildren();
                items = null;
            }

            selectedType.DataContext = null;

            #endregion

            if (freeHierarchyTypeTreeItem != null)
            {
                Tree_ID = freeHierarchyTypeTreeItem.FreeHierTree_ID;
            }
            else
            {
                Tree_ID = DefaultTree_ID;
            }

            if (previousSets != null)
            {

                if (_startSelector != null)
                {
                    try
                    {
                        _startSelector.Dispose();
                        _startSelector = null;
                    }
                    catch
                    {

                    }
                }

                //Была смена дерева, пытаемся выделить объекты с предыдущего дерева
                SetItemsForSelection(previousSets, 2);
            }

            MainLayout.RunAsync(() => new TreeStartLoader(this, freeHierarchyTypeTreeItem, _selectedNodes, isFullReload, singreRootFreeHierItemIds), 
                loader =>
            {
                //Ошибка загрузки дерева
                lock (_selectedNodesSyncLock)
                {
                    if (loader == null || loader.Descriptor == null) return;
                }

                if (IsSelectSingle)
                {
                    butNone.Visibility = showOnlySelected.Visibility = butAll.Visibility = System.Windows.Visibility.Collapsed;
                }

                var isFirstUsed = loader.IsFirstLoaded;

                IDictionary<int, FreeHierarchyTreeItem> source;
                lock (_selectedNodesSyncLock)
                {
                    source = loader.FreeHierarchyTreeItems;
                    descriptor = loader.Descriptor;
                    freeHierarchyTypeTreeItem = loader.FreeHierarchyTypeTreeItem;
                    //_loader = null;
                    var prevTypeTreeItem = selectedType.DataContext as FreeHierarchyTypeTreeItem;
                    if (prevTypeTreeItem != null)
                    {
                        selectedType.DataContext = null;
                        prevTypeTreeItem.Dispose();
                    }

                    descriptor.SelectedChanged += DescOnSelectedChanged;

                    loader.Dispose();
                }

                if (source == null) return;

                selectedType.DataContext = freeHierarchyTypeTreeItem;

                IEnumerable<KeyValuePair<int, FreeHierarchyTreeItem>> query;

                query = source.Where(i => i.Value != null && i.Value.Parent == null);
                
                //else
                //{
                //    //Нужно отобразить с дерево с определенного узла
                //    query = source.Where(i => i.Value != null && singreRootFreeHierItemIds.Contains(i.Value.FreeHierItem_ID));
                //}

                var parents = new RangeObservableCollection<FreeHierarchyTreeItem>(query
                     .Select(i => i.Value).OrderBy(o => o, new FreeHierarchyTreeItemComparer()), new FreeHierarchyTreeItemComparer());

                if (IsNSIDocuments)
                {
                    parents.Add(new FreeHierarchyTreeItem(descriptor, null as FreeHierarchyTreeItem)
                    {
                        FreeHierItemType = EnumFreeHierarchyItemType.CommonDocuments,
                        StringName = "Общие документы"
                    });
                }

                var module = FindModule(this);
                if (module != null && Manager.Config.TreeDefaults != null)
                {
                    Manager.Config.TreeDefaults[module.ModuleType + Name] = descriptor.Tree_ID.GetValueOrDefault();
                }

                //Пытаемся оставить совместимость
                if (module != null && Manager.Config.FreeSets != null)
                {
                    var setKey = module.ModuleType + ":" + freeHierarchyTypeTreeItem.FreeHierTree_ID;

                    Dictionary<string, List<string>> set;
                    if (Manager.Config.FreeSets.TryGetValue(setKey, out set) && set != null)
                    {
                        Dictionary<string, List<string>> ks;
                        if (Manager.Config.FreeSets.TryGetValue(
                                 freeHierarchyTypeTreeItem.FreeHierTree_ID.ToString(), out ks) && ks != null)
                        {
                            foreach (var k in ks)
                            {
                                List<string> existsSet;
                                if (set.TryGetValue(k.Key, out existsSet) && existsSet != null)
                                {
                                    existsSet.AddRange(k.Value);
                                }
                                else
                                {
                                    set[k.Key] = k.Value;
                                }

                            }
                        }

                        Manager.Config.FreeSets[freeHierarchyTypeTreeItem.FreeHierTree_ID.ToString()] = set;
                        Manager.Config.FreeSets.Remove(setKey);
                    }
                }

                sets.InitGlobal(freeHierarchyTypeTreeItem.FreeHierTree_ID.ToString());

                Items = parents;

                if (OnTreeDataLoaded != null)
                {
                    OnTreeDataLoaded(this, new EventArgsTreeItems
                    {
                        Items = parents,
                    });
                }
            }, priority: DispatcherPriority.Normal);
        }

        #region Объекты которые нужно выбрать

        private TreeStartObjectSelector _startSelector;

        /// <summary>
        /// Запуск загрузки и десериализации объектов которые нужно выбрать
        /// </summary>
        /// <param name="serializedSet">Объекты, которые нужно выделить</param>
        /// <param name="versionNumber">Версия сохраненного набора</param>
        /// <param name="runSelected"></param>
        public void SetItemsForSelection(object serializedSet, short versionNumber)
        {
            if (serializedSet == null) return;

            //          if (OnTreeDataLoaded != null)
            {
                OnTreeDataLoaded += OnTreeDataLoadedSync;
            }

            if (Tree_ID == -107 || Tree_ID == -102)
            {
                _startSelector = null;
            }
            else
            {
                _startSelector = new TreeStartObjectSelector(serializedSet, versionNumber, this);
            }
        }

        /// <summary>
        /// Вызываем после загрузки дерева
        /// </summary>
        private void OnTreeDataLoadedSync(object sender, EventArgsTreeItems eventArgs)
        {
            if (OnTreeDataLoaded != null)
            {
                OnTreeDataLoaded -= OnTreeDataLoadedSync;
            }

            if (_startSelector!=null)
            {
                _startSelector.SelectUnselect(eventArgs.Items ?? Items);
            }
        }

        #endregion


        //TODO SetInitialAsync устаревшее, нужно убрать все хвосты
        //public void SetInitialAsync(List<FreeItemSelected> list)
        //{
        //    if (list == null) return;

        //    //if (OnTreeDataLoaded != null)
        //    {
        //        OnTreeDataLoaded += OnTreeDataLoadedSelector;
        //    }

        //    _beforeInitialTask = Task.Factory.StartNew(() =>
        //    {
        //        //PrepareObjectForSelection(list);
        //        return list;
        //    });

        //    if (!_initObjectSelected)
        //    {
        //        this.Dispatcher
        //            .BeginInvoke((Action)(() => { OnTreeDataLoadedSelector(null, null); }), DispatcherPriority.Background);
        //    }
        //}

        #region Устаревшие методы

        public void PrepareObjectForSelection(List<FreeItemSelected> objectForSelection)
        {
            Parallel.Invoke(() =>
            {
                //Предварительно инициализируем ТИ для ПС у которых они явно указаны как выбранные
                if (FilterStatus.HasValue && FilterStatus.Value.HasFlag(EnumTIStatus.Is_USPD_Enabled))
                {
                    //Прогружаем УСПД и Е422 для ПС
                    try
                    {
                        var pss = new HashSet<int>(objectForSelection
                        .Where(s => s != null && s.ChildType.HasValue && (s.ChildType == EnumFreeHierarchyItemType.USPD)
                                    && s.ItemType == EnumFreeHierarchyItemType.PS && !string.IsNullOrEmpty(s.ItemID))
                        .Select(s =>
                        {
                            int psId;
                            if (int.TryParse(s.ItemID, out psId)) return psId;
                            return -1;
                        }));

                        //Подготавливаем словарь УСПД по 
                        FreeHierarchyDictionaries.UspdByPsDict.Prepare(pss);
                    }
                    catch { }
                }
            }, () =>
            {
                if (!FilterStatus.HasValue || FilterStatus.Value.HasFlag(EnumTIStatus.Is_MonthExTI_Enabled)
                    || (FilterStatus.Value &
                        (EnumTIStatus.TreeElectricity | EnumTIStatus.TreeWater | EnumTIStatus.TreeHeat |
                         EnumTIStatus.TreeGas)) != EnumTIStatus.None)
                {
                    try
                    {
                        //Прогружаем словарь с ТИ для ПС
                        var pss = new HashSet<int>(objectForSelection
                             .Where(s => s != null && s.ChildType.HasValue && s.ChildType == EnumFreeHierarchyItemType.TI
                                         && s.ItemType == EnumFreeHierarchyItemType.PS && !string.IsNullOrEmpty(s.ItemID))
                             .Select(s =>
                             {
                                 int psId;
                                 if (int.TryParse(s.ItemID, out psId)) return psId;
                                 return -1;
                             }));

                        EnumClientServiceDictionary.TIbyPS.Prepare(pss);
                    }
                    catch { }
                }
            });
        }

        #endregion
    }
}
