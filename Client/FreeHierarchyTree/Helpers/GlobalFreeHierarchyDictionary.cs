using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.ServiceReference.UAService;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;
using EnumFreeHierarchyItemType = Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService.EnumFreeHierarchyItemType;
using enumTypeHierarchy = Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.enumTypeHierarchy;


namespace Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy
{
    using Dict_JuridicalPersons_Contract = Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.Dict_JuridicalPersons_Contract;
    using Proryv.AskueARM2.Client.ServiceReference.DeclaratorService;
    using enumTypeHierarchy = Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.enumTypeHierarchy;
    using System.Collections.Concurrent;
    using Proryv.AskueARM2.Client.ServiceReference.Service.Dictionary;

    public static partial class GlobalFreeHierarchyDictionary
    {
        private static bool _alwaysExpandNodes = true;

        //private static Dictionary<int, FreeHierarchyTypeTreeItem> _types;

        /// <summary>
        /// Получение древовидного списка всех типов(вариантов иерархии)
        /// </summary>

        public const int TreeTypeStandart = -100;
        public const int TreeTypeStandartPS = TreeTypeStandart - 1;
        public const int TreeTypeStandartBySupplyPS = TreeTypeStandart - 3;
        public const int TreeTypeStandartTIFormula = TreeTypeStandart - 1;
        public const int TreeTypeStandartDistributingArrangementAndBusSystem = TreeTypeStandart - 8;

        public const int TreeTypeStandartGroupTP = TreeTypeStandart - 4;
        public const int TreeTypeStandartSections = TreeTypeStandart - 2;
        public const int TreeTypeStandartJuridicalPerson = TreeTypeStandart - 6;
        public const int TreeTypeStandartSectionsNSI = TreeTypeStandart - 7;

        public const int TreeTypeStandart_Dict_OPCUAServers = -1001;
        public const int TreeTypeStandart_Dict_FIAS = -1002;
        public const int TreeTypeStandart_Dict_FIASToHierarchy = -1003;
        public const int TreeExplXmlExportConfigs = -1004;
        public const int TreeExplXmlBalance63 = -1005;
        public const int TreeOldTelescope = -1006;

        public static Dictionary<int, FreeHierarchyTypeTreeItem> GetTypes(EnumModuleFilter? rightFilter)
        {
#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif
            //var dbTypes = FreeHierarchyService.GetTypes(Manager.UserName, Manager.Password, rights.ToList());
            var freeHierarchyTypes = Manager.User.GetFreeHierarchyTypes(Manager.UI.ShowMessage);

#if DEBUG
            sw.Stop();
            Console.WriteLine("GetFreeHierarchyTypes {0} млс", sw.ElapsedMilliseconds);
            sw.Restart();
#endif

            var result = new Dictionary<int, FreeHierarchyTypeTreeItem>();

            if (freeHierarchyTypes == null || freeHierarchyTypes.Count == 0) return result;

            var parents = new Dictionary<FreeHierarchyTypeTreeItem, int?>();

            IEnumerable<TFreeHierarchyType> types = rightFilter.GetValueOrDefault() != EnumModuleFilter.None
                ? freeHierarchyTypes.Where(
                    f => f.ModuleFilter.GetValueOrDefault() == EnumModuleFilter.None || (rightFilter.GetValueOrDefault() & f.ModuleFilter.GetValueOrDefault()) != EnumModuleFilter.None)
                : freeHierarchyTypes;

            foreach (var value in types)
            {
                var newItem = new FreeHierarchyTypeTreeItem
                {
                    FreeHierTree_ID = value.FreeHierTree_ID,
                    StringName = value.StringName,
                    ModuleFilter = value.ModuleFilter
                    //так как читается упорядочено по уровням
                };
                result.Add(newItem.FreeHierTree_ID, newItem);
                parents[newItem] = value.ParentID;
            }
            if (parents.Count == 0) return result;

            foreach (var value in result.Values)
            {
                int? parentId;
                FreeHierarchyTypeTreeItem hierarchyTypeTreeItem;
                if (!parents.TryGetValue(value, out parentId) || !parentId.HasValue || !result.TryGetValue(parentId.Value, out hierarchyTypeTreeItem)) continue;
                value.Parent = hierarchyTypeTreeItem;
                value.Parent.Children.Add(value);
            }

#if DEBUG
            sw.Stop();
            Console.WriteLine("Построение типов дерева {0} млс", sw.ElapsedMilliseconds);
#endif
            //Сортируем по алфавиту только в стандартном дереве
            FreeHierarchyTypeTreeItem standartTree;
            if (result.TryGetValue(TreeTypeStandart, out standartTree) && standartTree != null && standartTree.Children != null && standartTree.Children.Count > 0)
            {
                standartTree.Children.Sort(new FreeHierarchyTypeTreeItemComparer());
            }

            return result;
        }

        public static IDictionary<int, FreeHierarchyTreeItem> GetTree(FreeHierarchyTreeDescriptor descriptor, out bool isFirstLoaded, bool isHideTi = false,
            HashSet<long> selectedNodes = null, bool isFullReload = false, HashSet<int> singreRootFreeHierItemIds = null)
        {
            isFirstLoaded = false;

            //Признак администратора
            bool isUserAdmin = Manager.User.IsAdmin;

#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            #region это стандартные деревья их не загружаем

            switch (descriptor.Tree_ID)
            {
                //case TreeTypeStandartPS:
                case TreeTypeStandartTIFormula:
                case TreeTypeStandartDistributingArrangementAndBusSystem:

                    //if (isFullReload)
                    //{
                    //    descriptor.UpdateIncludedObjectChildrenAsync(Manager.User.User_ID);
                    //}

                    return GenerateStandartTreePS(descriptor, isHideTi);
                case TreeTypeStandartSections:
                case TreeTypeStandartSectionsNSI:
                    return GenerateStandartTreeSections(descriptor);
                case TreeTypeStandartBySupplyPS:
                    return GenerateStandartTreeBySupplyPS(descriptor, isHideTi);
                case TreeTypeStandartGroupTP:
                    return JuridicalPersonTree.GenerateJuridicalPersonTree(descriptor, true, false);
                case TreeTypeStandartJuridicalPerson:
                    return JuridicalPersonTree.GenerateJuridicalPersonTree(descriptor, false, true);
                case TreeTypeStandart_Dict_OPCUAServers:
                    return GenerateStandartTree_Dict_OPCUAServers(descriptor);
                case TreeTypeStandart_Dict_FIAS:
                    return GenerateStandartTree_FIAS(false, descriptor);
                case TreeTypeStandart_Dict_FIASToHierarchy:
                    return GenerateStandartTree_FIAS(true, descriptor);

                case TreeExplXmlExportConfigs:
                    return GenerateExplXmlExportConfigsTree(descriptor);


                case TreeExplXmlBalance63:
                    return GenerateBalance63Tree(descriptor);

                case TreeOldTelescope:
                    return GenerateOldTelescopeTree(descriptor);

                    


            }

            #endregion

            var result = new ConcurrentDictionary<int, FreeHierarchyTreeItem>();
            List<TFreeHierarchyTreeItem> tempList = null;

            try
            {
                int? parenId = null;
                if (singreRootFreeHierItemIds != null) parenId = singreRootFreeHierItemIds.FirstOrDefault();

                tempList = FreeHierarchyTreeDictionary.GetBranch(Manager.User.User_ID, descriptor.Tree_ID.GetValueOrDefault(), parenId,
                    isFullReload, onError: Manager.UI.ShowMessage);
            }
            catch (Exception ex)
            {
                Manager.UI.ShowMessage(ex.Message);
            }

#if DEBUG
            sw.Stop();
            Console.WriteLine("Запрос дерева в БД {0} млс", sw.ElapsedMilliseconds);
            sw.Restart();
#endif

            if (tempList == null) return result;

            FreeHierarchyTreePreparer.PrepareGlobalDictionaries(tempList);

            if (descriptor != null) descriptor.Tree = result;

            foreach(var newItem in FreeHierarchyTreePreparer.BuildBranch(null, tempList, descriptor, isHideTi, selectedNodes))
            {
                result.TryAdd(newItem.FreeHierItem_ID, newItem);
            }

            //if ((uspdList!=null&&(uspdList.Count>0)))
            //{
            //   ProcessUspdInTree(result,uspdList, isFilteredBySmallTi, isHideTi);
            //}

#if DEBUG
            sw.Stop();
            Console.WriteLine("Построение дерева {0} млс", sw.ElapsedMilliseconds);
#endif


            return result;
        }


        /// <summary>
        /// Генерация стандартного дерева FIAS адреса, которые используются
        /// </summary>
        private static ConcurrentDictionary<int, FreeHierarchyTreeItem> GenerateStandartTree_FIAS(bool IncludeHerarchyObjects, FreeHierarchyTreeDescriptor descriptor)
        {
            var result = new ConcurrentDictionary<int, FreeHierarchyTreeItem>();

            //добавляем корневые узлы
            IEnumerable<TFIASNode> rootNodes;
            
            if (!IncludeHerarchyObjects)
            {
                rootNodes = DeclaratorService.FIAS_Get_RootNodes();
            }
            else
            {
                rootNodes = DeclaratorService.FIASToHierarchy_Get_RootNodes();
            }

            if (rootNodes == null) return result;

            foreach (var rootNode in rootNodes)
            {
                var newItemNode = new FreeHierarchyTreeItem(descriptor, rootNode, false, string.Empty, descriptor.GetMinIdAndDecrement(), null, true, nodeRights: null, 
                    freeHierItemType: EnumFreeHierarchyItemType.FiasFullAddress)
                {
                    StringName = rootNode.StringName,
                    FIASNode = rootNode,
                };

                newItemNode.Descriptor.Tree = result;

                //newItemNode.UpdateCountParents();

                //добавляем дочерние
                if (rootNode.IsChildrenExists) newItemNode.IncludeChildrenFIASNodes((rootNode.IsHierObjectExists || descriptor.Tree_ID == TreeTypeStandart_Dict_FIASToHierarchy));

                result[newItemNode.FreeHierItem_ID] = newItemNode;
            }

            return result;
        }



        private static FreeHierarchyTreeItem Create_FreeHierarchyTreeItem(int minID, KeyValuePair<Guid, TFIASNode> obj, FreeHierarchyTreeDescriptor descriptor)
        {
            var newItemNode = new FreeHierarchyTreeItem(descriptor, obj.Value)
            {
                FreeHierItem_ID = minID,
                IncludeObjectChildren = true,
                FreeHierItemType = EnumFreeHierarchyItemType.FiasFullAddress,
                NodeRights = null,
                StringName = obj.Value.StringName,
                FIASNode = obj.Value,
            };

            return newItemNode;
        }
        
        private static ConcurrentDictionary<int, FreeHierarchyTreeItem> GenerateStandartTreeBySupplyPS(FreeHierarchyTreeDescriptor descriptor, bool isHideTi)
        {
            var items = new List<FreeHierarchyTreeItem>();
            var result = new ConcurrentDictionary<int, FreeHierarchyTreeItem>();
            foreach (var value in from ps in EnumClientServiceDictionary.DetailPSList
                                  join ps_sup in GlobalTreeDictionary.PowerSupply_PS_List on ps.Key equals ps_sup.PS_ID into temp
                                  from ps_t in temp.DefaultIfEmpty()
                                  where ps_t == null
                                  orderby ps.Value.Name
                                  select ps.Value)
            {
                if (!IsGlobalFilterHaveItem(value)) continue;

                UserRightsForTreeObject right;
                if (!Manager.User.AccumulateRightsAndVerify(value, EnumObjectRightType.SeeDbObjects, value.GetObjectRightType(), out right)) continue;

                FreeHierarchyTreeItem newItem = new FreeHierarchyTreeItem(descriptor, value, nodeName: value.Name)
                {
                    FreeHierItem_ID = descriptor.GetMinIdAndDecrement(),
                    IncludeObjectChildren = true,
                    IsHideTi = isHideTi,
                    FreeHierItemType = EnumFreeHierarchyItemType.PS,
                    NodeRights = right,
                };

                items.Add(newItem);
                newItem.Descriptor.Tree = result;
                if (newItem.IncludeObjectChildren)
                {
                    newItem.LoadStaticChildren(true, isHideTi);
                    newItem.Descriptor.NeedFindTI = true;
                }
            }
            if (FreeHierarchyTreeDescriptor.Sort != null) FreeHierarchyTreeDescriptor.Sort(items);
            foreach (var i in items) result[i.FreeHierItem_ID] = i;
            return result;
        }

        /// <summary>
        /// Генерация стандартного дерева Список OPCUA серверов
        /// </summary>
        private static ConcurrentDictionary<int, FreeHierarchyTreeItem> GenerateStandartTree_Dict_OPCUAServers(FreeHierarchyTreeDescriptor descriptor)
        {
            var items = new List<FreeHierarchyTreeItem>();
            var result = new ConcurrentDictionary<int, FreeHierarchyTreeItem>();
            var uaServers = UAHierarchyDictionaries.UaServers(Manager.UI.ShowMessage);
            if (uaServers == null) return null;

            descriptor.NeedFindUaNode = true;

            foreach (var item in uaServers.Values)
            {
                UserRightsForTreeObject right;
                if (!Manager.User.AccumulateRightsAndVerify(item, EnumObjectRightType.SeeDbObjects, null, out right)) continue;

                var newItem = new FreeHierarchyTreeItem(descriptor, item)
                {
                    FreeHierItem_ID = descriptor.GetMinIdAndDecrement(),
                    IncludeObjectChildren = true,
                    IsHideTi = false,
                    FreeHierItemType = EnumFreeHierarchyItemType.UAServer,
                    NodeRights = null,
                };

                newItem.Descriptor.Tree = result;

                items.Add(newItem);

                //дабавляем корневые узлы вручную
                //дальше будут подгружаться при раскрытии?
                List<TUANode> rootNodes = UAService.Service.UA_Get_RootNodes(item.UAServer_ID);
                foreach (var rootNode in rootNodes)
                {
                    TUANode node = null;
                    UAHierarchyDictionaries.UANodesDict.TryGetValue(rootNode.UANodeId, out node);
                    if (node == null)
                        continue;

                    FreeHierarchyTreeItem newItemNode = new FreeHierarchyTreeItem(newItem.Descriptor, node)
                    {
                        FreeHierItem_ID = descriptor.GetMinIdAndDecrement(),
                        Parent = newItem,
                        IncludeObjectChildren = true,
                        FreeHierItemType = EnumFreeHierarchyItemType.UANode,
                        NodeRights = right,
                    };

                    newItem.Children.Add(newItemNode);
                    newItem.Descriptor.Tree.TryAdd(newItemNode.FreeHierItem_ID, newItemNode);
      
                    //добавляем дочерние
                    //newItemNode.IncludeChildrenOPCNodes(newItemNode.Children, false);

                }
            }
            if (FreeHierarchyTreeDescriptor.Sort != null) FreeHierarchyTreeDescriptor.Sort(items);
            foreach (var i in items) result[i.FreeHierItem_ID] = i;
            return result;
        }


        /// <summary>
        /// Генерация стандартного дерева ПС
        /// </summary>
        /// <returns></returns>
        private static ConcurrentDictionary<int, FreeHierarchyTreeItem> GenerateStandartTreePS(FreeHierarchyTreeDescriptor descriptor, bool isHideTi)
        {
            var result = new ConcurrentDictionary<int, FreeHierarchyTreeItem>();

            var hiers1 = EnumClientServiceDictionary.HierLev1List.Values;


            var filterStatus = descriptor.FilterStatus;

            foreach (var item in hiers1.OrderBy(v => v.Item)) //сортируем по названию
            {
                if (item == null
                    || (filterStatus.HasValue && (item.TIStatus & filterStatus.Value) == EnumTIStatus.None) //Используется фильтр на дереве
                    || !IsGlobalFilterHaveItem(item)) //Глобальный фильтр
                {
                    continue;
                }

                var isExistsChildSeeDbObjects = Manager.User.IsAssentRight(item, EnumObjectRightType.SeeDbObjects, null);

                if (!isExistsChildSeeDbObjects //Нет собственных прав
                   && !FreeHierarchyTreePreparer.ExistsStandartChildSeeDbObjects(item, descriptor.Tree_ID ?? -101, true) //И нет дочерних объектов с правами
                   ) continue;
                
                var newItem = new FreeHierarchyTreeItem(descriptor, item, false, item.Name, descriptor.GetMinIdAndDecrement(), null, true, isHideTi, item.ObjectRights,
                    EnumFreeHierarchyItemType.HierLev1)
                {
                    Descriptor =
                    {
                        Tree = result
                    }
                };

                if (newItem.IncludeObjectChildren)
                {
                    isExistsChildSeeDbObjects = newItem.LoadStaticChildren(isExistsChildSeeDbObjects, isHideTi);
                }

                //У объекта нет прав на просмотр, или нет дочерних объектов на которые есть такие права
                //if (isExistsRight || isExistsChildSeeDbObjects)
                //{
                    result[newItem.FreeHierItem_ID] = newItem;
                //}
                //else
                //{
                //    newItem.Dispose();
                //}
            }
            //if (FreeHierarchyTreeDescriptor.Sort != null) FreeHierarchyTreeDescriptor.Sort(items);
            //foreach (var i in items) result[i.FreeHierItem_ID] = i;
            return result;
        }

        private static ConcurrentDictionary<int, FreeHierarchyTreeItem> GenerateStandartTreeSections(FreeHierarchyTreeDescriptor descriptor)
        {
            var result = new ConcurrentDictionary<int, FreeHierarchyTreeItem>();
            var items = new List<FreeHierarchyTreeItem>();

            HashSet<ID_TypeHierarchy> objectsHasRightDbSee = null;
            var hiers1 = EnumClientServiceDictionary.HierLev1List.Values;

            if (Manager.User != null && !Manager.User.IsAdmin)
            {
                try
                {
                    //Запрашиваем права на список объектов
                    objectsHasRightDbSee = Manager.User.UserHasRightDbSee(hiers1.Select(h1 => new ID_TypeHierarchy
                    {
                        ID = h1.Id,
                        TypeHierarchy = enumTypeHierarchy.Dict_HierLev1,
                    }).ToList());
                }
                catch (Exception ex)
                {
                    Manager.UI.ShowMessage("Ошибка запроса прав: " + ex.Message);
                }
            }

            foreach (var item in hiers1.OrderBy(v => v.Item))
            {
                //Глобальный фильтр
                if (!IsGlobalFilterHaveItem(item))
                {
                    continue;
                }

                var minID = descriptor.GetMinIdAndDecrement();
                UserRightsForTreeObject hier1Right;
                var isExistsRight = Manager.User.AccumulateRightsAndVerify(item, EnumObjectRightType.SeeDbObjects, null, out hier1Right);
                if (!isExistsRight && Manager.User != null && !Manager.User.IsAdmin && objectsHasRightDbSee != null)
                {
                    //Проверяем через дочерних объектов
                    isExistsRight = objectsHasRightDbSee.Any(o => o.ID == item.Id && o.TypeHierarchy == item.GetTypeHierarchy());
                }

                var newItem = new FreeHierarchyTreeItem(descriptor, item, nodeName: item.Name)
                {
                    FreeHierItem_ID = minID,
                    IncludeObjectChildren = true,
                    FreeHierItemType = EnumFreeHierarchyItemType.HierLev1,
                    NodeRights = hier1Right
                };

                newItem.Descriptor.Tree = result;
                bool isExistsChildSeeDbObjects = false;
                if (newItem.IncludeObjectChildren)
                {
                    isExistsChildSeeDbObjects = newItem.LoadStaticChildren(isExistsRight, isLoadStatic: true);
                }

                isExistsChildSeeDbObjects = newItem.IncludeChildrenSection(item.Id, item.Type, newItem.Children) || isExistsChildSeeDbObjects 
                    || item.TIStatus.HasFlag(EnumTIStatus.Is_Section_Enabled);

                //У объекта нет прав на просмотр, или нет дочерних объектов на которые есть такие права
                if (isExistsChildSeeDbObjects) //Включаем только если есть сечения
                {
                    items.Add(newItem);
                }
                else
                {
                    newItem.Dispose();
                }
            }

            //Сечения которые не привязаны ни к чему
            foreach (var section in EnumClientServiceDictionary.GetSections().Values.Where(s => s.ParentId <= 0 && s.Section_ID > 0).OrderBy(s => s.Item))
            {
                UserRightsForTreeObject right;
                if (!Manager.User.AccumulateRightsAndVerify(section, EnumObjectRightType.SeeDbObjects, null, out right)) continue;

                var minID = descriptor.GetMinIdAndDecrement();
                FreeHierarchyTreeItem newItem = new FreeHierarchyTreeItem(descriptor, section)
                {
                    FreeHierItem_ID = minID,
                    IncludeObjectChildren = true,
                    NodeRights = right,
                    FreeHierItemType = EnumFreeHierarchyItemType.Section,
                };

                newItem.Descriptor.Tree = result;
                newItem.LoadStaticChildren(true);

                items.Add(newItem);
            }

            if (FreeHierarchyTreeDescriptor.Sort != null) FreeHierarchyTreeDescriptor.Sort(items);
            foreach (var i in items) result[i.FreeHierItem_ID] = i;
            return result;
        }

        //TODO Доделать
        
        private static Dictionary<enumTypeHierarchy, Dictionary<int, THierarchyDbTreeObject>> _globalFilterDict = new Dictionary<enumTypeHierarchy, Dictionary<int, THierarchyDbTreeObject>>
        {
            {enumTypeHierarchy.Dict_HierLev1, GlobalFilterDictionary.HierLev1Selected},
            {enumTypeHierarchy.Dict_HierLev3, GlobalFilterDictionary.HierLev3Selected},
            {enumTypeHierarchy.Dict_PS, GlobalFilterDictionary.PSSelected},
        };

        /// <summary>
        /// Обработка глобального фильтра
        /// </summary>
        /// <param name="hierarchyObject"></param>
        /// <returns></returns>
        public static bool IsGlobalFilterHaveItem(IFreeHierarchyObject hierarchyObject)
        {
            Dictionary<int, THierarchyDbTreeObject> selectedGlobalHierDict;
            if (!_globalFilterDict.TryGetValue(hierarchyObject.Type, out selectedGlobalHierDict) || selectedGlobalHierDict == null || selectedGlobalHierDict.Count == 0)
                return true;

            if (selectedGlobalHierDict.ContainsKey(hierarchyObject.Id))
            {
                return true;
            }

            return false;
        }

        public static IEnumerable<ID_TypeHierarchy> AsTypeHierarchy(this IEnumerable<FreeHierarchyTreeItem> selected)
        {
            return selected.Select(item => item.AsTypeHierarchy());
        }

        public static ID_TypeHierarchy AsTypeHierarchy(this FreeHierarchyTreeItem item)
        {
            if (item == null) return null;

            return new ID_TypeHierarchy
            {
                ID = item.HierObject.Id,
                StringId = item.HierObject.StringId,
                TypeHierarchy = item.HierObject.Type,
                FreeHierItemId = item.FreeHierItem_ID < 0 ? null : (int?)item.FreeHierItem_ID
            };
        }
    }
}