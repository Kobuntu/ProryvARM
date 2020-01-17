using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.ServiceReference.Service.Dictionary;
using Proryv.AskueARM2.Client.ServiceReference.UAService;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Proryv.AskueARM2.Client.ServiceReference.Common;

namespace Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy
{
    public partial class FreeHierarchyTreeItem
    {
        #region Прогрузка дочерних узлов

        /// <summary>
        /// Производит загрузку дочерних узлов, которые подгружаются динамически
        /// </summary>
        public void LoadDynamicChildren(bool isHideTi = false,
            DispatcherObject dispatcherObject = null, Action uiThreadAction = null, bool isAsync = false, 
            bool fullReload = false, bool isLoadFromServer = true, bool isHideTp = false)
        {
            //Если подгружать только свободную иерархию, но она уже подгружена
            if (!isLoadFromServer && IsFreeHierLoadedInitializet) return;

            if (fullReload)
            {
                IsChildrenInitializet = false;
                IsTisNodesInitializet = false;
                IsFiasNodesInitializet = false;
                IsTransformatorsAndReactorsInitializet = false;
                IsUspdAndE422Initializet = false;
                IsUaNodesInitializet = false;
            }

            if (IsChildrenInitializet || (!isLoadFromServer && IsLocalChildrenInitializet))
            {
                if (uiThreadAction != null)
                {
                    try
                    {
                        uiThreadAction();
                    }
                    catch { }
                }

                return;
            }

            //Эти объекты в отдельных деревьях, вместе с ТИ не грузятся
            if (isLoadFromServer && UpdateUaNodes(false, isAsync, dispatcherObject, 2, uiThreadAction, isLoadFromServer))
            {
                IsChildrenInitializet = true;
                return;
            }

            if (!isLoadFromServer || !UpdateFIASNodes())
            {
                if (Descriptor != null)
                {
                    UpdateChildren(isHideTi, isAsync, uiThreadAction: uiThreadAction,
                        fullReload: fullReload, isLoadFromServer: isLoadFromServer, isHideTp: isHideTp);
                }
            }

            if (isLoadFromServer || (Descriptor!=null && Descriptor.Tree_ID == GlobalFreeHierarchyDictionary.TreeTypeStandartSectionsNSI)) IsChildrenInitializet = true;
            IsLocalChildrenInitializet = true;
        }

        #region Подгрузка UA OPC

        private bool UpdateUaNodes(bool isSelected, bool isAsync, DispatcherObject dispatcherObject, int uaLevel, 
            Action uiThreadAction, bool isLoadFromServer = true)
        {
            //Динамическая догрузка OPC UA узлов
            if (FreeHierItemType != EnumFreeHierarchyItemType.UANode || HierObject == null
                || Descriptor == null)
            {
                return false;
            }

            if (IsUaNodesInitializet)
            {
                if (uiThreadAction != null)
                {
                    if (isAsync)
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            try
                            {
                                uiThreadAction();
                            }
                            catch { }
                        }), DispatcherPriority.Send);
                    }
                    else
                    {
                        uiThreadAction();
                    }
                }
                return true;
            }

            if (isAsync)
            {
                IsExpandProcessed = true;
            }

            try
            {
                if (isAsync)
                {
                    //lock (_syncLock)
                    {
                        Task.Factory.StartNew(() => updateUaNode(isAsync, isSelected, uaLevel, dispatcherObject, uiThreadAction, isLoadFromServer));
                    }
                }
                else
                {
                    updateUaNode(isAsync, isSelected, uaLevel, dispatcherObject, uiThreadAction, isLoadFromServer);
                }
            }
            catch (Exception ex)
            {

            }

            return true;
        }

        private void updateUaNode(bool isAsync, bool isSelected, int level, DispatcherObject dispatcherObject, Action uiThreadAction
            , bool isLoadFromServer = true)
        {
            var uaNode = HierObject as TUANode;
            if (uaNode == null || Descriptor == null) return;

            if (isLoadFromServer)
            {
                try
                {
                    if (uaNode.DependentNodes == null && !uaNode.DependentNodesChecked)
                    {
                        //Узлы еще не догружены, догружаем
                        UAHierarchyDictionaries.UANodesDict.ChildrenPrepare(new HashSet<long> { uaNode.UANodeId }, Manager.UI.ShowMessage, level);
                    }
                }
                catch (Exception ex)
                {
                    Manager.UI.ShowMessage(ex.Message);
                }
            }

            if (Descriptor == null || Descriptor.Tree == null) return; //Дерево больше не актуально

            try
            {
                var children = new List<FreeHierarchyTreeItem>();
                IncludeChildrenOPCNodes(children, isSelected, includeChildren: isAsync);

                AddChildren(children);

                Action propertyChangedAction = () =>
               {
                   IsExpandProcessed = false;

                   if (uiThreadAction != null)
                       try
                       {
                           uiThreadAction();
                       }
                       catch { }
               };

                if (Application.Current.Dispatcher.CheckAccess()) propertyChangedAction();
                else Application.Current.Dispatcher.BeginInvoke(propertyChangedAction);
            }
            catch (Exception ex)
            {
                Manager.UI.ShowMessage(ex.Message);
            }
            finally
            {
                IsUaNodesInitializet = true;
                //UpdateVisualUIChildren(dispatcherObject, uiThreadAction);
            }
        }

        #endregion

        #region Подгрузка ФИАС

        private bool UpdateFIASNodes()
        {
            if (IsFiasNodesInitializet) return true;

            //Динамическая догрузка FiasFullAddress
            if (FreeHierItemType != EnumFreeHierarchyItemType.FiasFullAddress)
            {
                return false;
            }

            //перезагружаем для всех кроме родительских 
            if (Parent == null)
                return true;

            try
            {
                if (FIASNode == null)
                    return true;

                //сначала загружаем из базы
                //if (!FIASNode.IsChildrenExists)
                {
                    //получаем список
                    IncludeChildrenFIASNodes(
                        (FIASNode != null && FIASNode.IsHierObjectExists ||
                         Descriptor.Tree_ID == GlobalFreeHierarchyDictionary.TreeTypeStandart_Dict_FIASToHierarchy)
                        , false);
                }
            }
            finally
            {
                IsFiasNodesInitializet = true;
                //удаляем фиктивный узел
                //var emptyitem2 = UIChildren.Where(i => i.StringName == "Пустой узел").ToList();
                //foreach (var freeHierarchyTreeItem in emptyitem2)
                //{
                //    lock (SyncLock)
                //    {
                //        UIChildren.Remove(freeHierarchyTreeItem);
                //    }
                //}
                // if (emptyitem != null)
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("HasChildren"));
            }

            return true;
        }

        public bool IncludeChildrenFIASNodes(bool IncludeHerarchyObjects, bool includeChildren = true)
        {
            bool result = false;

            if (HierObject == null || FIASNode == null) return false;
            
            var children = new List<FreeHierarchyTreeItem>();

            //получаем список узлов
            Guid? guid = new Guid(FIASNode.AOGUID.ToString());
            // Результат если будет большой будет разбиваться
            var nodes =
                !IncludeHerarchyObjects ?
                ServiceReference.DeclaratorService.DeclaratorService.Service.PartialLoaderSync(() =>
                ServiceReference.DeclaratorService.DeclaratorService.Service.FIAS_Get_Children(FIASNode.AOGUID))
                :
                ServiceReference.DeclaratorService.DeclaratorService.Service.PartialLoaderSync(() =>
                ServiceReference.DeclaratorService.DeclaratorService.Service.FIASToHierarchy_Get_Nodes(false, guid))
                ;

            //дочерние адреса
            foreach (var childnodeFias in nodes)
            {
                FreeHierarchyTreeItem newNode = new FreeHierarchyTreeItem(Descriptor, childnodeFias, false, string.Empty, 
                    Descriptor.GetMinIdAndDecrement(), includeObjectChildren: true, nodeRights: null, parent: this, 
                    freeHierItemType: EnumFreeHierarchyItemType.FiasFullAddress)
                {
                    FIASNode = childnodeFias,
                    StringName = childnodeFias.StringName,
                };

                #region создаем пустой дочерний для динамической подгрузки
                if (childnodeFias.IsChildrenExists)
                {
                    //minID--;
                    //Guid newid = Guid.NewGuid();
                    //FreeHierarchyTreeItem newEmptyNode = Create_FreeHierarchyTreeItem(minID,
                    //    new KeyValuePair<Guid, TFIASNode>(newid,
                    //        new TFIASNode { AOGUID = newid, StringName = "Пустой узел" }), item.Descriptor);
                    //newEmptyNode.Parent = newNode;

                    //newNode.UIChildren.Add(newEmptyNode);
                    //newNode.Descriptor.Tree.Add(newEmptyNode.FreeHierItem_ID, newEmptyNode);
                }

                children.Add(newNode);
                Descriptor.Tree.TryAdd(newNode.FreeHierItem_ID, newNode);

                #endregion

                result = true;
            }

            //дочерние объекты - только для дерева (исп адреса с привязкой к объектам)
            if (FIASNode.IsHierObjectExists && guid.HasValue)
            {
                var brunch = UpdateFreeHierarchyChildren(isFIASLoad: true, AOGUID: FIASNode.AOGUID, maxNodeID: Descriptor.GetMinIdAndDecrement());
                if (brunch!=null) children.AddRange(brunch);
                //GenerateStandartTree_FIAS_GetChildrenHierobjects(guid.Value, children);
            }


            //EnumClientServiceDictionary.FiasDictionary.ReloadValue(new HashSet<Guid>(item.ChildrenInternal.Where(i => i.Value != null && i.Value.FIASNode != null)
            //    .Select(i => i.Value.FIASNode.AOGUID)));

            //#if DEBUG
            //            TFIASToHierarchyItem test = null;
            //            EnumClientServiceDictionary.FiasToHierarchyDictionary.TryGetValue(new TFreeHierarchyTreeItem() { FreeHierItemType = EnumFreeHierarchyItemType.HierLev1, HierLev1_ID = 9 }, out test);
            //            //тут фигня
            //            if (test != null)
            //            {
            //                TFIASNode fiasnode;
            //                EnumClientServiceDictionary.FiasDictionary.TryGetValue(test.AOGUID, out fiasnode);
            //            }
            //#endif

            AddChildren(children);

            return result;
        }

        #endregion

        /// <summary>
        /// Основная ф-ия подгрузки/обновления дочерних объектов
        /// </summary>
        /// <param name="isHideTi"></param>
        /// <param name="isShowTransformatorsAndReactors"></param>
        /// <param name="isAsync"></param>
        /// <param name="fullReload"></param>
        /// <param name="uiThreadAction"></param>
        private void UpdateChildren(bool isHideTi, bool isAsync = true,
            bool fullReload = false, Action uiThreadAction = null, bool isLoadFromServer = true, bool isHideTp = false)
        {
            if ((Descriptor == null || Descriptor.Tree_ID <=0) )
            {
                //Это стандартное дерево 
                IsFreeHierLoadedInitializet = true;

                if (!IncludeObjectChildren)
                {
                    //и объект не содержит стандартных дочерних

                    //if (PropertyChanged != null && Application.Current.Dispatcher.CheckAccess())
                    //{
                    //    PropertyChanged(this, new PropertyChangedEventArgs("HasChildren"));
                    //}
                    if (uiThreadAction != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            try
                            {
                                uiThreadAction();
                            }
                            catch { }
                        }), DispatcherPriority.Send);
                    }
                    return;
                }
            }

            var action = (Func<List<FreeHierarchyTreeItem>>)(() =>
            {
                var children = AddStandartChildren(isHideTi, isLoadFromServer, isHideTp, fullReload);

                //Это дерево свободной иерархии, подгружаем еще все что описано
                if (!IsFreeHierLoadedInitializet && Descriptor != null && Descriptor.Tree_ID > 0)
                {
                    var brunch = UpdateFreeHierarchyChildren(fullReload, isHideTi);
                    if (brunch != null) children.AddRange(brunch);
                    IsFreeHierLoadedInitializet = true;
                }

                return children;
            });

            if (isAsync)
            {
                IsExpandProcessed = true;

                Task.Factory.StartNew(action).ContinueWith(result =>
                {
                    try
                    {
                        AddChildren(result.Result, isLoadFromServer);
                    }
                    catch (Exception ex)
                    {
                        Manager.UI.ShowMessage(ex.Message);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        IsExpandProcessed = false;
                        if (uiThreadAction != null)
                        {
                            try
                            {
                                uiThreadAction();
                            }
                            catch (Exception ex)
                            {
                                Manager.UI.ShowMessage(ex.Message);
                            }
                        }

                    }), DispatcherPriority.Normal);
                });
            }
            else
            {
                bool isChangedCollection;

                isChangedCollection = AddChildren(action.Invoke());
                

                if (uiThreadAction != null)
                {
                    if (isChangedCollection)
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            uiThreadAction();
                        }), DispatcherPriority.Send);
                    }
                    else
                    {
                        uiThreadAction();
                    }
                }
            }
        }

        public List<FreeHierarchyTreeItem> AddStandartChildren(bool isHideTi, bool isLoadFromServer, bool isHideTp, bool fullReload)
        {
            List<FreeHierarchyTreeItem> children;

            if (IncludeObjectChildren || Descriptor == null || Descriptor.Tree_ID <= 0)
            {
                //Это стандартное дерево, подгружаем согласно типу объекта
                //Разные типы объектов требуют подгрузки разных дочерних
                switch (FreeHierItemType)
                {
                    case EnumFreeHierarchyItemType.PS:
                        {
                            children = new List<FreeHierarchyTreeItem>();

                            if (isLoadFromServer)
                            {
                                UpdatePsChildren(children, IsHideTi);

                                if (Descriptor != null && Descriptor.NeedFindTransformatorsAndreactors && !IsTransformatorsAndReactorsInitializet)
                                {
                                    children.AddRange(UpdateTransformatorsAndReactors());
                                }

                                if (Descriptor != null && Descriptor.ShowUspdAndE422InTree && !IsUspdAndE422Initializet)
                                {
                                    children.AddRange(UpdateUspdAndE422());
                                }

                                if (!isHideTi && !IsTisNodesInitializet)
                                {
                                    children.AddRange(RunUpdateTis(fullReload).OrderBy(o => o, new FreeHierarchyTreeItemComparer()));
                                }
                            }

                            if (Descriptor != null && Descriptor.Tree_ID == GlobalFreeHierarchyDictionary.TreeTypeStandartTIFormula)
                            {
                                IncludeChildrenFormulas(HierObject.Id, HierObject.Type, children);
                                IncludeChildrenFormulaConstant(children);
                                IncludeChildrenBalances(children);
                            }

                            break;
                        }

                    case EnumFreeHierarchyItemType.HierLev1:
                    case EnumFreeHierarchyItemType.HierLev2:
                    case EnumFreeHierarchyItemType.HierLev3:
                        {
                            //TODO доработать с правами
                            children = UpdateHierLevsChildren(isHideTi);

                            break;
                        }
                    case EnumFreeHierarchyItemType.TP:
                        {
                            children = UpdateTpChildren();

                            break;
                        }
                    case EnumFreeHierarchyItemType.Section:
                        {
                            children = UpdateSectionChildren(isHideTp);

                            break;
                        }
                    case EnumFreeHierarchyItemType.TI:
                        {
                            children = UpdateTiChildren();

                            break;
                        }
                    case EnumFreeHierarchyItemType.JuridicalPerson:
                        {
                            children = UpdateJuridicalPersonChildren();

                            break;
                        }
                    case EnumFreeHierarchyItemType.Contract:
                        {
                            children = UpdateContractChildren();

                            break;
                        }
                    case EnumFreeHierarchyItemType.DistributingArrangement:
                        {
                            children = UpdateDistributingArrangementChildren(isHideTi);

                            break;
                        }
                    case EnumFreeHierarchyItemType.BusSystem:
                        {
                            children = UpdateBusSystemChildren(isHideTi);

                            break;
                        }
                    case EnumFreeHierarchyItemType.Node:
                        {
                            children = UpdateNodeChildren();

                            break;
                        }
                    case EnumFreeHierarchyItemType.ForecastObject:
                        {
                            children = UpdateForecastObjectChildren();

                            break;
                        }
                    case EnumFreeHierarchyItemType.UANode:
                        {
                            if (isLoadFromServer)
                            {
                                children = UpdateUANodeChildren();
                            }
                            else
                            {
                                children = new List<FreeHierarchyTreeItem>();
                            }
                            break;
                        }
                    case EnumFreeHierarchyItemType.OldTelescopeTreeNode:
                        {
                            children = UpdateOldTelescopeTreeNodeChildren();

                            break;
                        }

                    default:
                        children = new List<FreeHierarchyTreeItem>();
                        break;
                }
            }
            else
            {
                children = new List<FreeHierarchyTreeItem>();
                IncludeChildrenFormulaConstant(children);
            }

            return children;
        }

        private bool AddChildren(List<FreeHierarchyTreeItem> addedItems, bool isLoadFromServer = false)
        {
            var isChangedCollection = false;

            if (addedItems != null && addedItems.Count > 0)
            {

                isChangedCollection = true;

                //lock (_syncLock)
                {
                    //if (Children == null) // || _children.Count > 0
                    //{
                    //    var comparer = new FreeHierarchyTreeItemComparer();

                    //    IEnumerable<FreeHierarchyTreeItem> collection = Children;
                    //    if (collection != null) collection = collection.Union(addedItems);
                    //    else collection = addedItems;

                    //    Children = new SortedSet<FreeHierarchyTreeItem>(collection, comparer);
                    //}
                    //else
                    //{
                    _children.AddRange(addedItems.OrderBy(o => o, new FreeHierarchyTreeItemComparer()));
                    //}
                }
            }

            if (!isChangedCollection && isLoadFromServer && PropertyChanged != null && Application.Current != null)
            {
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("HasChildren"));
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                       PropertyChanged(this, new PropertyChangedEventArgs("HasChildren"))), DispatcherPriority.DataBind);
                }
            }

            return isChangedCollection;
        }

        private void UpdateTis(EnumTIStatus? filterStatus, bool fullReload = false)
        {
            if (IsTisNodesInitializet && !fullReload) return;

            if (FreeHierItemType != EnumFreeHierarchyItemType.PS)
            {
                IsTisNodesInitializet = true;
                return;
            }

            //lock (_syncLock)
            {
                _children.AddRange(RunUpdateTis(fullReload).OrderBy(o => o, new FreeHierarchyTreeItemComparer()));
            }
        }

        private List<FreeHierarchyTreeItem> RunUpdateTis(bool fullReload = false)
        {
            var result = new List<FreeHierarchyTreeItem>();

            List<TInfo_TI> tis = null;
            if (!IncludeObjectChildren
                || !EnumClientServiceDictionary.TIbyPS.TryGetValue(HierObject.Id, out tis) || tis == null)
            {
                //пока только для Описателя - обновление ТИ нужно не только для ПС, но и для остальных узлов
                if (fullReload)
                {
                    var tisFree = this.Children.Where(i => i.FreeHierItemType == EnumFreeHierarchyItemType.TI)
                        .Select(i => i.HierObject.Id).ToList();

                    tis = new List<TInfo_TI>();
                    foreach (var freeitem in tisFree)
                    {
                        TInfo_TI ti = null;
                        if (EnumClientServiceDictionary.TIHierarchyList.TryGetValue(freeitem, out ti))
                            tis.Add(ti);
                    }
                }

                if (tis == null || tis.Count == 0)
                {
                    IsTisNodesInitializet = true;

                    //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    //{
                    //    IsExpandProcessed = false;
                    //}));

                    return result;
                }
            }

            //if (hideWaitPanelAction != null) hideWaitPanelAction();

            var tiForRemove = new HashSet<int>();

            try
            {
                //Исключаем уже описанные ТИ
                var existsTis = new HashSet<int>(Children
                    .Where(ui => ui.FreeHierItemType == EnumFreeHierarchyItemType.TI && ui.HierObject != null)
                    .Select(ui => ui.HierObject.Id));

                //var sel = UIChildren != null && UIChildren.Any() && UIChildren[0].IsSelected;

                //TODO исправить (IG от 02.09.2017 10-28) 
                //т.к. удаленные ТИ продолжают отображаться, то возвращаю (Карпов)
                if (fullReload)
                    tiForRemove.UnionWith(existsTis.Except(tis.Select(t => t.TI_ID)));

                var isSelected = IsSelectedChildren;

                //lock (Descriptor)
                {
                    bool isFilteredBySmallTi;
                    bool isFilterByTreeCategory;
                    if (Descriptor.FilterStatus.HasValue)
                    {
                        isFilteredBySmallTi = Descriptor.FilterStatus.Value.HasFlag(EnumTIStatus.Is_MonthExTI_Enabled);
                        isFilterByTreeCategory =
                            (Descriptor.FilterStatus.Value &
                             (EnumTIStatus.TreeElectricity | EnumTIStatus.TreeWater | EnumTIStatus.TreeHeat |
                              EnumTIStatus.TreeGas)) != EnumTIStatus.None;
                    }
                    else
                    {
                        isFilteredBySmallTi = false;
                        isFilterByTreeCategory = false;
                    }

                    //определим мин идентификатор в дереве (виртуальные FreeHierItem_ID должны быть отрицательными, иначе путаница)

                    foreach (var ti in tis)
                    {
                        if (ti == null 
                                || (isFilteredBySmallTi && ti.TIType!=2)
                                || (isFilterByTreeCategory && ((ti.TreeCategory.HasValue && (ti.TreeCategory.Value & Descriptor.FilterStatus.Value) == EnumTIStatus.None) || !ti.TreeCategory.HasValue))
                            ) continue;
                        

                        if (ti.IsDeleted)
                        //TODO а чтобы удаленные ТИ отображались в соотв с настройками?
                        //if (ti.IsDeleted && Manager.Config.IsHideDeletedTI)  // и в usp2_Info_GetTIorContrTIForPS условия deleted=0?
                        {
                            tiForRemove.Add(ti.TI_ID);
                            continue;
                        }

                        if (existsTis.Contains(ti.TI_ID))
                        {
                            if (fullReload)
                            {
                                //надо полностью обновлять (иконки, название...)
                                var existsTINode = Children.First(i => i.HierObject.Id == ti.TI_ID &&
                                                                         i.FreeHierItemType == EnumFreeHierarchyItemType
                                                                             .TI);
                                existsTINode.HierObject = ti;
                            }

                            continue;
                        }

                        if (Descriptor.Tree_ID == GlobalFreeHierarchyDictionary.TreeTypeStandartDistributingArrangementAndBusSystem
                            && GlobalTreeDictionary.BusSystem_To_TI_Relations != null &&
                            GlobalTreeDictionary.BusSystem_To_TI_Relations.Count > 0
                            && GlobalTreeDictionary.BusSystem_To_TI_Relations.Any(t => t.PS_ID == HierObject.Id && ti.TI_ID == t.TI_ID))
                            continue;

                        var newItem = new FreeHierarchyTreeItem(Descriptor, ti, isSelected)
                        {
                            FreeHierItem_ID = Descriptor.GetMinIdAndDecrement(),
                            Parent = this,
                            IncludeObjectChildren = true,
                            FreeHierItemType = EnumFreeHierarchyItemType.TI,
                            Visibility =
                                Descriptor.VoltageFilter.HasValue && Descriptor.VoltageFilter.Value != ti.Voltage
                                    ? Visibility.Collapsed
                                    : Visibility.Visible,
                            NodeRights = NodeRights,
                            IsTisNodesInitializet = true,
                            IsChildrenInitializet = true,
                        };

                        result.Add(newItem);
                        Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);
                    }
                }
            }
            finally
            {
                //lock (_syncLock)
                {
                    //Удаление лишнего
                    if (tiForRemove.Count > 0)
                    {
                        Children.RemoveWhere(t => t.HierObject != null
                                                     &&
                                                     t.FreeHierItemType ==
                                                     EnumFreeHierarchyItemType.TI
                                                     && tiForRemove.Contains(t.HierObject.Id));
                    }
                }

                IsTisNodesInitializet = true;
            }

            return result;
        }

        private List<FreeHierarchyTreeItem> UpdateTransformatorsAndReactors()
        {
            var objectsForTree = new List<FreeHierarchyTreeItem>();

            List<Hard_PTransformator> transformators = null;
            try
            {
                var request = ARM_Service.Transformators_Get_TransformatorsForPS(HierObject.Id, DateTime.Now.AddYears(-1), DateTime.Now);
                if (request != null) transformators = request.PTransformators;
            }
            catch
            {

            }

            List<Hard_PReactors> reactors = null;
            try
            {
                var request = ARM_Service.Reactors_Get_ReactorsForPS(HierObject.Id, DateTime.Now.AddYears(-1), DateTime.Now);
                if (request != null) reactors = request.PReactors;
            }
            catch
            {

            }

            try
            {
                //определим мин идентификатор в дереве (виртуальные FreeHierItem_ID должны быть отрицательными, иначе путаница)

                if (transformators != null)
                {
                    //Исключаем уже описанные ТИ
                    var existsTransformators = new HashSet<int>(Children
                        .Where(ui => ui.FreeHierItemType == EnumFreeHierarchyItemType.PTransformator && ui.HierObject != null)
                        .Select(ui => ui.HierObject.Id));

                    //var sel = UIChildren != null && UIChildren.Any() && UIChildren[0].IsSelected;

                    foreach (var transformator in transformators.OrderBy(t => t.PTransformatorName))
                    {
                        if (existsTransformators.Contains(transformator.PTransformator_ID))
                        {
                            continue;
                        }

                        var t = EnumClientServiceDictionary.GetOrAddTransformator(transformator.PTransformator_ID, transformator);

                        var newItem = new FreeHierarchyTreeItem(Descriptor, t)
                        {
                            FreeHierItem_ID = Descriptor.GetMinIdAndDecrement(),
                            Parent = this,
                            IncludeObjectChildren = true,
                            IsSelected = false,
                            FreeHierItemType = EnumFreeHierarchyItemType.PTransformator,
                            Visibility = Visibility.Visible,
                            NodeRights = NodeRights,
                            IsChildrenInitializet = true,
                        };

                        objectsForTree.Add(newItem);
                        Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);
                    }
                }

                if (reactors != null)
                {
                    //Исключаем уже описанные ТИ
                    var existsTreactors = new HashSet<int>(Children
                        .Where(ui => ui.FreeHierItemType == EnumFreeHierarchyItemType.Reactor && ui.HierObject != null)
                        .Select(ui => ui.HierObject.Id));

                    //var sel = UIChildren != null && UIChildren.Any() && UIChildren[0].IsSelected;

                    foreach (var reactor in reactors.OrderBy(r => r.PReactorName))
                    {
                        if (existsTreactors.Contains(reactor.PReactor_ID))
                        {
                            continue;
                        }

                        var r = EnumClientServiceDictionary.GetOrAddReactor(reactor.PReactor_ID, reactor);

                        var newItem = new FreeHierarchyTreeItem(Descriptor, r)
                        {
                            FreeHierItem_ID = Descriptor.GetMinIdAndDecrement(),
                            Parent = this,
                            IncludeObjectChildren = true,
                            IsSelected = false,
                            FreeHierItemType = EnumFreeHierarchyItemType.Reactor,
                            Visibility = Visibility.Visible,
                            NodeRights = NodeRights,
                        };

                        objectsForTree.Add(newItem);
                        Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);
                    }
                }
            }
            finally
            {
                IsTransformatorsAndReactorsInitializet = true;
            }

            return objectsForTree;
        }

        private List<FreeHierarchyTreeItem> UpdateUspdAndE422()
        {

            List<ServiceReference.FreeHierarchyService.Hard_USPD> uspds = null;
            if (HierObject != null && HierObject.TIStatus.HasFlag(EnumTIStatus.Is_USPD_Enabled))
            {
                try
                {
                    var uspdIds = FreeHierarchyDictionaries.UspdByPsDict[HierObject.Id];
                    if (uspdIds != null)
                    {
                        var uh = new HashSet<int>(uspdIds);
                        uspds = FreeHierarchyDictionaries.USPDDict.GetValues(uh, Manager.UI.ShowMessage);
                    }
                }
                catch
                {

                }
            }

            List<Hard_E422> e422s = null;
            if (HierObject != null && HierObject.TIStatus.HasFlag(EnumTIStatus.Is_E422_Enabled))
            {
                try
                {
                    var e422Ids = FreeHierarchyDictionaries.E422ByPsDict[HierObject.Id];
                    if (e422Ids != null)
                    {
                        var uh = new HashSet<int>(e422Ids);
                        e422s = FreeHierarchyDictionaries.E422Dict.GetValues(uh, Manager.UI.ShowMessage);
                    }
                }
                catch
                {

                }
            }

            var objectsForTree = new List<FreeHierarchyTreeItem>();
            var isSelected = IsSelectedChildren;

            try
            {
                if (uspds != null)
                {
                    //Исключаем уже описанные ТИ
                    var existsTransformators = new HashSet<int>(Children
                        .Where(ui => ui.FreeHierItemType == EnumFreeHierarchyItemType.USPD && ui.HierObject != null)
                        .Select(ui => ui.HierObject.Id));

                    //var sel = UIChildren != null && UIChildren.Any() && UIChildren[0].IsSelected;

                    foreach (var uspd in uspds.OrderBy(t => t.USPDName))
                    {
                        if (existsTransformators.Contains(uspd.USPD_ID))
                        {
                            continue;
                        }

                        var newItem = new FreeHierarchyTreeItem(Descriptor, uspd, isSelected)
                        {
                            FreeHierItem_ID = Descriptor.GetMinIdAndDecrement(),
                            Parent = this,
                            IncludeObjectChildren = true,
                            //IsSelected = sel,
                            FreeHierItemType = EnumFreeHierarchyItemType.USPD,
                            Visibility = Visibility.Visible,
                            NodeRights = NodeRights,
                            IsTisNodesInitializet = true,
                            IsChildrenInitializet = true,
                        };

                        objectsForTree.Add(newItem);
                        Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);
                    }
                }

                if (e422s != null)
                {
                    //Исключаем уже описанные ТИ
                    var existsTreactors = new HashSet<int>(Children
                        .Where(ui => ui.FreeHierItemType == EnumFreeHierarchyItemType.E422 && ui.HierObject != null)
                        .Select(ui => ui.HierObject.Id));

                    //var sel = UIChildren != null && UIChildren.Any() && UIChildren[0].IsSelected;

                    foreach (var e422 in e422s.OrderBy(r => r.E422Name))
                    {
                        if (existsTreactors.Contains(e422.E422_ID))
                        {
                            continue;
                        }

                        var newItem = new FreeHierarchyTreeItem(Descriptor, e422, isSelected)
                        {
                            FreeHierItem_ID = Descriptor.GetMinIdAndDecrement(),
                            Parent = this,
                            IncludeObjectChildren = true,
                            //IsSelected = sel,
                            FreeHierItemType = EnumFreeHierarchyItemType.E422,
                            Visibility = Visibility.Visible,
                            NodeRights = NodeRights,
                            IsTisNodesInitializet = true,
                            IsChildrenInitializet = true,
                        };

                        objectsForTree.Add(newItem);
                        Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);
                    }
                }
            }
            finally
            {
                IsUspdAndE422Initializet = true;
            }

            return objectsForTree;
        }

        /// <summary>
        /// Догрузка дочерних объетов для стандартного дерева для HierLev1, HierLev2, HierLev3
        /// </summary>
        /// <param name="isExistsChildSeeDbObjects"></param>
        /// <param name="isExistsParentRight"></param>
        /// <param name="isHideTi"></param>
        /// <returns></returns>
        public List<FreeHierarchyTreeItem> UpdateHierLevsChildren(bool isHideTi)
        {
            var children = new List<FreeHierarchyTreeItem>();

            if (HierObject == null || HierObject.HierarchyChildren == null) return children;

            //HashSet<ID_TypeHierarchy> objectsHasRightDbSee = null;
            UserRightsForTreeObject parentRights = null;
            bool isUserAdmin = false;
            bool isExistsParentRight = false;
            if (Manager.User != null)
            {
                isExistsParentRight = isUserAdmin = Manager.User.IsAdmin;
                if (!isUserAdmin)
                {
                    isExistsParentRight = Manager.User.IsAssentRight(NodeRights, EnumObjectRightType.SeeDbObjects);
                    parentRights = NodeRights;
                }
            }

            if (isExistsParentRight && Descriptor.Tree_ID == GlobalFreeHierarchyDictionary.TreeTypeStandartTIFormula)
            {
                IncludeChildrenFormulas(HierObject.Id, HierObject.Type, children);
                IncludeChildrenFormulaConstant(children);
                IncludeChildrenBalances(children);
            }

            var isIncludeSection = Descriptor.Tree_ID == GlobalFreeHierarchyDictionary.TreeTypeStandartSections
                                   || Descriptor.Tree_ID == GlobalFreeHierarchyDictionary.TreeTypeStandartSectionsNSI;

            var filterStatus = Descriptor.FilterStatus;

            var comparer = new FreeHierarchyTreeItemComparer();

            foreach (var hierLev in HierObject.HierarchyChildren)
            {
                if (hierLev == null
                    || (filterStatus.HasValue && (hierLev.TIStatus & filterStatus.Value) == EnumTIStatus.None) //Используется фильтр на дереве
                    || !GlobalFreeHierarchyDictionary.IsGlobalFilterHaveItem(hierLev)) //Глобальный фильтр
                {
                    continue;
                }

                UserRightsForTreeObject rights;
                var isExistsChildSeeDbObjects = Manager.User.AccumulateRightsAndVerify(hierLev, EnumObjectRightType.SeeDbObjects, parentRights, out rights);
                if (!isExistsChildSeeDbObjects)
                {
                    isExistsChildSeeDbObjects = isExistsParentRight;
                }

                if ((!isExistsChildSeeDbObjects //Нет собственных прав
                    && !FreeHierarchyTreePreparer.ExistsStandartChildSeeDbObjects(hierLev, Descriptor.Tree_ID ?? -101, true)) //И нет дочерних объектов с правами
                    || (isIncludeSection && !hierLev.TIStatus.HasFlag(EnumTIStatus.Is_Section_Enabled)) //надо отображать сечения, но их нет
                    ) continue;

                //Здесь надо контролировать
                //if (parentRights != null)
                //{
                //    rights = parentRights.AccamulateRights(rights);
                //}

                //Если это дерево свободной иерархии надо проверить права у дочерних
                var newItem = new FreeHierarchyTreeItem(Descriptor, hierLev, false, string.Empty,
                    Descriptor.GetMinIdAndDecrement(), this, FreeHierItemType != EnumFreeHierarchyItemType.HierLev3 || !isIncludeSection,
                    isHideTi, rights, FreeHierItemType.GetLowerHierarchyType());

                if (isIncludeSection)
                {
                    var sectionChildren = new SortedSet<FreeHierarchyTreeItem>(new FreeHierarchyTreeItemComparer());
                    newItem.IncludeChildrenSection(hierLev.Id, newItem.HierObject.Type, sectionChildren);
                    newItem.Children.AddRange(sectionChildren.OrderBy(b => b, comparer));
                }

                children.Add(newItem);

                Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);
            }

            return children;
        }


        /// <summary>
        /// подгрузка дочерних объектов для дерева "Старого Телескопа"
        /// </summary>
        public List<FreeHierarchyTreeItem> UpdateOldTelescopeTreeNodeChildren()
        {
            var children = new List<FreeHierarchyTreeItem>();

            if (HierObject == null || HierObject.HierarchyChildren == null || !HierObject.HierarchyChildren.Any()) return children;
            
            foreach (var item in HierObject.HierarchyChildren)
            {

                var newItem = new FreeHierarchyTreeItem(Descriptor, item, false, string.Empty, Descriptor.GetMinIdAndDecrement(), this, true,
                    nodeRights: HierObject.GetObjectRightType(), freeHierItemType: EnumFreeHierarchyItemType.OldTelescopeTreeNode);

                UserRightsForTreeObject right;
                if (item != null && Manager.User.AccumulateRightsAndVerify(item, EnumObjectRightType.SeeDbObjects,
                        HierObject.GetObjectRightType(), out right))
                {
                    children.Add(newItem);
                    Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);
                    newItem.Descriptor.Tree = Descriptor.Tree;

                    newItem.LoadStaticChildren(true);
                }
                else
                {
                    //newItem.FreeHierItemType = EnumFreeHierarchyItemType.Error;
                    newItem.Dispose();
                }
            }

            return children;
        }



        /// <summary>
        /// Подгрузка дочерних объектов для ТП
        /// </summary>
        /// <returns></returns>
        public List<FreeHierarchyTreeItem> UpdateTpChildren()
        {
            var children = new List<FreeHierarchyTreeItem>();

            if (HierObject == null || (!Descriptor.IsLoadOurFormulas && Descriptor.Tree_ID != GlobalFreeHierarchyDictionary.TreeTypeStandartSectionsNSI)) return children;

            foreach (var frm in GlobalSectionsDictionary.GetFormulaFSKinTP(HierObject.Id, false))
            {
                var formula = new FreeHierarchyTreeItem(Descriptor, frm, false, string.Empty, Descriptor.GetMinIdAndDecrement(), this, true,
                    nodeRights: NodeRights, freeHierItemType: EnumFreeHierarchyItemType.OurFormula, isChildrenInitializet: true);
                children.Add(formula);
                Descriptor.Tree.TryAdd(formula.FreeHierItem_ID, formula);
            }

            foreach (var frm in GlobalSectionsDictionary.GetFormulaCAinTP(HierObject.Id, false))
            {

                var formula = new FreeHierarchyTreeItem(Descriptor, frm, false, string.Empty, Descriptor.GetMinIdAndDecrement(), this, true,
                    nodeRights: NodeRights, freeHierItemType: EnumFreeHierarchyItemType.CAFormula, isChildrenInitializet: true);
                children.Add(formula);
                Descriptor.Tree.TryAdd(formula.FreeHierItem_ID, formula);
                formula.CAFormula = frm;
            }


            return children;
        }

        public void UpdatePsChildren(ICollection<FreeHierarchyTreeItem> children, bool isHideTi = false)
        {
            if (isHideTi) return;
                        
            if (Descriptor.Tree_ID == GlobalFreeHierarchyDictionary.TreeTypeStandartBySupplyPS)
            {
                foreach (var ps in (from supply_ps in GlobalTreeDictionary.PowerSupply_PS_List
                                    where supply_ps.PowerSupplyPS_ID == HierObject.Id
                                    select EnumClientServiceDictionary.DetailPSList[supply_ps.PS_ID]).OrderBy(p => p.PSType).ThenBy(p => p.Name))
                {
                    UserRightsForTreeObject right;
                    var isExistsRight = Manager.User.AccumulateRightsAndVerify(ps, EnumObjectRightType.SeeDbObjects, HierObject.GetObjectRightType(), out right);
                    if (!isExistsRight) continue;


                    var newItem = new FreeHierarchyTreeItem(Descriptor, ps, false, string.Empty, Descriptor.GetMinIdAndDecrement(), this, true,
                        nodeRights: right, freeHierItemType: EnumFreeHierarchyItemType.PS);

                    children.Add(newItem);
                    Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);
                    newItem.Descriptor.Tree = Descriptor.Tree;

                    newItem.LoadStaticChildren(isExistsRight);
                }
            }
            else if (Descriptor.Tree_ID == GlobalFreeHierarchyDictionary.TreeTypeStandartDistributingArrangementAndBusSystem)
            {
                foreach (
                    var da in
                    GlobalTreeDictionary.DistributingArrangements.Where(da => FreeHierItemType == EnumFreeHierarchyItemType.PS && da.Value.PS_ID == HierObject.Id)
                )
                {
                    UserRightsForTreeObject right;
                    var isExistsRight = Manager.User.AccumulateRightsAndVerify(da.Value, EnumObjectRightType.SeeDbObjects, HierObject.GetObjectRightType(), out right);
                    if (!isExistsRight) continue;

                    var newItem = new FreeHierarchyTreeItem(Descriptor, da.Value, false, string.Empty, Descriptor.GetMinIdAndDecrement(), this, true,
                        nodeRights: right, freeHierItemType: EnumFreeHierarchyItemType.DistributingArrangement);

                    children.Add(newItem);
                    Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);
                    newItem.Descriptor.Tree = Descriptor.Tree;
                    newItem.LoadStaticChildren(isExistsRight);
                }
            }

            if (Descriptor.Tree_ID == GlobalFreeHierarchyDictionary.TreeTypeStandartSections
                || Descriptor.Tree_ID == GlobalFreeHierarchyDictionary.TreeTypeStandartSectionsNSI)
            {
                IncludeChildrenSection(HierObject.Id, HierObject.Type, children);
            }
        }

        public List<FreeHierarchyTreeItem> UpdateSectionChildren(bool isHideTp)
        {
            var children = new List<FreeHierarchyTreeItem>();

            if (HierObject == null || HierObject.HierarchyChildren == null || !HierObject.HierarchyChildren.Any()) return children;


            foreach (var tp in HierObject.HierarchyChildren)
            {
                if (isHideTp && tp.Type == enumTypeHierarchy.Info_TP) continue;

                var newItem = new FreeHierarchyTreeItem(Descriptor, tp, false, string.Empty, Descriptor.GetMinIdAndDecrement(), this, true,
                    nodeRights: HierObject.GetObjectRightType(), freeHierItemType: EnumFreeHierarchyItemType.TP);

                UserRightsForTreeObject right;
                if (tp != null && Manager.User.AccumulateRightsAndVerify(tp, EnumObjectRightType.SeeDbObjects,
                        HierObject.GetObjectRightType(), out right))
                {
                    children.Add(newItem);
                    Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);
                    newItem.Descriptor.Tree = Descriptor.Tree;

                    newItem.LoadStaticChildren(true);
                }
                else
                {
                    //newItem.FreeHierItemType = EnumFreeHierarchyItemType.Error;
                    newItem.Dispose();
                }
            }

            return children;
        }

        public List<FreeHierarchyTreeItem> UpdateTiChildren()
        {
            var children = new List<FreeHierarchyTreeItem>();

            if (Descriptor == null || Descriptor.Tree_ID != GlobalFreeHierarchyDictionary.TreeTypeStandartTIFormula) return children;


            IncludeChildrenFormulas(HierObject.Id, enumTypeHierarchy.Info_TI, children);
            IncludeChildrenFormulaConstant(children);
            IncludeChildrenBalances(children);

            return children;
        }

        public List<FreeHierarchyTreeItem> UpdateJuridicalPersonChildren()
        {
            var children = new List<FreeHierarchyTreeItem>();

            var contracts = EnumClientServiceDictionary.ContractByPersonDict[HierObject.Id];
            if (contracts == null) return children;

            foreach (var contr in contracts)
            {
                UserRightsForTreeObject right;
                var isExistsRight = Manager.User.AccumulateRightsAndVerify(contr, EnumObjectRightType.SeeDbObjects, HierObject.GetObjectRightType(), out right);

                var newItem = new FreeHierarchyTreeItem(Descriptor, contr, false, string.Empty, Descriptor.GetMinIdAndDecrement(), this, true,
                    nodeRights: right, freeHierItemType: EnumFreeHierarchyItemType.Contract);
                if (newItem.LoadStaticChildren(true) || isExistsRight)
                {
                    children.Add(newItem);
                    Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);
                }
                else
                {
                    newItem.Dispose();
                }
            }

            return children;
        }

        public List<FreeHierarchyTreeItem> UpdateContractChildren()
        {
            var children = new List<FreeHierarchyTreeItem>();
            List<int> tps;
            GlobalJuridicalPersonsDictionary.TPByContractDict(HierObject.Id, out tps);
            if (tps != null)
            {
                var sect_ids = (from tp_id in tps
                                join tp in EnumClientServiceDictionary.GetTps().Values on tp_id equals tp.TP_ID
                                select tp.Section_ID).Distinct();
                var list = (from s_id in sect_ids
                            join sect in GlobalSectionsDictionary.SectionsList.Values on s_id equals sect.Section_ID
                            select sect).ToArray();

                foreach (var sect in list)
                {
                    UserRightsForTreeObject right;
                    if (!Manager.User.AccumulateRightsAndVerify(sect, EnumObjectRightType.SeeDbObjects, HierObject.GetObjectRightType(), out right)) continue;

                    var sectItem = new FreeHierarchyTreeItem(Descriptor, sect, false, string.Empty, Descriptor.GetMinIdAndDecrement(), this, true,
                        nodeRights: right, freeHierItemType: EnumFreeHierarchyItemType.Section);

                    children.Add(sectItem);
                    Descriptor.Tree.TryAdd(sectItem.FreeHierItem_ID, sectItem);
                    sectItem.LoadStaticChildren(true);
                }
            }

            return children;
        }

        public List<FreeHierarchyTreeItem> UpdateDistributingArrangementChildren(bool isHideTi)
        {
            var children = new List<FreeHierarchyTreeItem>();

            foreach (var bs in GlobalTreeDictionary.BusSystems.Where(bs => bs.Value.DistributingArrangement_ID == HierObject.Id))
            {
                UserRightsForTreeObject right;
                var isExistsRight = Manager.User.AccumulateRightsAndVerify(bs.Value, EnumObjectRightType.SeeDbObjects, HierObject.GetObjectRightType(), out right);
                if (!isExistsRight) continue;


                var newItem = new FreeHierarchyTreeItem(Descriptor, bs.Value, false, string.Empty, Descriptor.GetMinIdAndDecrement(), this, true,
                    nodeRights: right, freeHierItemType: EnumFreeHierarchyItemType.BusSystem, isHideTi: isHideTi);

                children.Add(newItem);
                Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);
                newItem.LoadStaticChildren(isExistsRight, isHideTi);
            }

            return children;
        }

        public List<FreeHierarchyTreeItem> UpdateBusSystemChildren(bool isHideTi)
        {
            var children = new List<FreeHierarchyTreeItem>();

            foreach (var ti in GlobalTreeDictionary.BusSystem_To_TI_Relations.Where(bs => bs.BusSystem_ID == HierObject.Id))
            {

                var newItem = new FreeHierarchyTreeItem(Descriptor, EnumClientServiceDictionary.TIHierarchyList[ti.TI_ID], false, string.Empty, Descriptor.GetMinIdAndDecrement(),
                    this, true, nodeRights: GetNodeRight(), freeHierItemType: EnumFreeHierarchyItemType.TI, isHideTi: isHideTi);

                children.Add(newItem);
                Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);
                newItem.LoadStaticChildren(true, isHideTi);
            }

            return children;
        }

        public List<FreeHierarchyTreeItem> UpdateNodeChildren()
        {
            var children = new List<FreeHierarchyTreeItem>();

            IncludeChildrenFormulas(FreeHierItem_ID, enumTypeHierarchy.Node, children);
            IncludeChildrenFormulaConstant(children);

            return children;
        }

        public List<FreeHierarchyTreeItem> UpdateForecastObjectChildren()
        {
            var children = new List<FreeHierarchyTreeItem>();

            IncludeForecastObjectChildren(children);

            return children;
        }

        public List<FreeHierarchyTreeItem> UpdateUANodeChildren()
        {
            var children = new List<FreeHierarchyTreeItem>();

            IncludeChildrenOPCNodes(children, false);

            return children;
        }


        public List<FreeHierarchyTreeItem> UpdateFreeHierarchyChildren(bool fullReload = false, bool isHideTi = false,
            bool isFIASLoad = false, Guid? AOGUID = null, int? maxNodeID = null, Action<string> onError = null)
        {
            if (HierObject == null 
                || (Descriptor.Tree_ID <= 0 && Descriptor.Tree_ID!= GlobalFreeHierarchyDictionary.TreeTypeStandart_Dict_FIAS 
                && Descriptor.Tree_ID != GlobalFreeHierarchyDictionary.TreeTypeStandart_Dict_FIASToHierarchy))
                return null;

            //if (isFIASLoad)
            {
                //Для ФИАСА читаем по-старому
                var tempList = FreeHierarchyTreeDictionary.GetBranch(Manager.User.User_ID, Descriptor.Tree_ID ?? -1002, FreeHierItem_ID, fullReload, isFIASLoad, AOGUID, maxNodeID, onError);
                if (tempList == null || tempList.Count == 0) return null;

                //!!!! тут нужно продумать
                FreeHierarchyTreePreparer.PrepareGlobalDictionaries(tempList);

                return FreeHierarchyTreePreparer.BuildBranch(this, tempList, Descriptor, isHideTi, null, isFIASLoad);
            }

            //Это динамически подгружаемый вариант (по одной ветке)
            //return FreeHierarchyTreePreparer.BuildBranch(Manager.User.User_ID, Descriptor.TreeID, this, Descriptor,
              //  isHideTi, null, fullReload);
        }

        #endregion

        /// <summary>
        /// Типы которые подгружаются динамически когда IncludeObjectChildren == true
        /// </summary>
        public static HashSet<EnumFreeHierarchyItemType> DynamicLoadedChildren = new HashSet<EnumFreeHierarchyItemType>
        {
            EnumFreeHierarchyItemType.TI,
            EnumFreeHierarchyItemType.USPD,
            EnumFreeHierarchyItemType.Reactor,
            EnumFreeHierarchyItemType.PTransformator,
            EnumFreeHierarchyItemType.UniversalBalance,
        };

        /// <summary>
        /// Родительские объекты, по которым нужно предварительно прогружать дочерние узлы
        /// </summary>
        public static HashSet<EnumFreeHierarchyItemType> DynamicLoadedParent = new HashSet<EnumFreeHierarchyItemType>
        {
            EnumFreeHierarchyItemType.UANode,
            EnumFreeHierarchyItemType.PS,
            EnumFreeHierarchyItemType.FiasFullAddress,
            EnumFreeHierarchyItemType.Node,
        };

        /// <summary>
        /// Объекты, которые не имеют дочерних объектов в стандартных деревьях
        /// </summary>
        public static HashSet<EnumFreeHierarchyItemType> NoHaveChildren = new HashSet<EnumFreeHierarchyItemType>
        {
            //EnumFreeHierarchyItemType.TI, Могут входить балансы

            EnumFreeHierarchyItemType.USPD,
            EnumFreeHierarchyItemType.Reactor,
            EnumFreeHierarchyItemType.PTransformator,
            EnumFreeHierarchyItemType.UniversalBalance,
            EnumFreeHierarchyItemType.Contract,
            EnumFreeHierarchyItemType.FormulaConstant,
            EnumFreeHierarchyItemType.Formula,
            EnumFreeHierarchyItemType.OurFormula,
            EnumFreeHierarchyItemType.CAFormula,
        };
    }
}
