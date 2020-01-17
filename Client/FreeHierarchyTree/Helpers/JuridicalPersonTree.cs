using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;
using Dict_JuridicalPersons_Contract = Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.Dict_JuridicalPersons_Contract;
using EnumFreeHierarchyItemType = Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService.EnumFreeHierarchyItemType;
using enumTypeHierarchy = Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service.enumTypeHierarchy;

namespace Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy
{
    public class JuridicalPersonTree
    {
        public static ConcurrentDictionary<int, FreeHierarchyTreeItem> GenerateJuridicalPersonTree(FreeHierarchyTreeDescriptor descriptor, bool isFactPowerTree, bool isAddVoidJuridicalPerson)
        {
            var result = new ConcurrentDictionary<int, FreeHierarchyTreeItem>();
            int minID = -1;

            var directConsumerDictionary = new Dictionary<int, Dictionary<int, FreeHierarchyTreeItem>>();
            var juridicalPersonsDictionary = new Dictionary<int, FreeHierarchyTreeItem>();

            var sectionsWithContracts = new HashSet<int>();

            // Построение договоров
            foreach (var contract in EnumClientServiceDictionary.ContractDict.Values)
            {
                List<int> sectionIds;
                if (EnumClientServiceDictionary.SectionByContract==null || !EnumClientServiceDictionary.SectionByContract.TryGetValue(contract.JuridicalPersonContract_ID, out sectionIds) || sectionIds == null) continue;

                if (isFactPowerTree) sectionsWithContracts.UnionWith(sectionIds);

                UserRightsForTreeObject juridicalPersonRight = null;
                FreeHierarchyTreeItem juridicalPersonHierarchyItem = null;
                TJuridicalPerson juridicalPerson;
                var isExistsJuridicalPersonRight = false;
                int? rId = null;
                bool isNewPerson = false;
                // построение Юридических лиц
                if (EnumClientServiceDictionary.JuridicalPersonByContract.TryGetValue(contract.JuridicalPersonContract_ID, out juridicalPerson))
                {
                    isExistsJuridicalPersonRight = Manager.User.AccumulateRightsAndVerify(juridicalPerson, EnumObjectRightType.SeeDbObjects, null, out juridicalPersonRight);
                    isNewPerson = !juridicalPersonsDictionary.TryGetValue(juridicalPerson.Item.JuridicalPerson_ID, out juridicalPersonHierarchyItem);
                    if (isNewPerson || juridicalPersonHierarchyItem == null)
                    {
                        minID--;
                        juridicalPersonHierarchyItem = new FreeHierarchyTreeItem(descriptor, juridicalPerson)
                        {
                            FreeHierItem_ID = minID,
                            FreeHierItemType = EnumFreeHierarchyItemType.JuridicalPerson,
                            NodeRights = juridicalPersonRight,
                            IsChildrenInitializet = true,
                        };

                        // построение родителей иерархической структуры
                        rId = minID;
                        juridicalPersonsDictionary[juridicalPerson.Item.JuridicalPerson_ID] = juridicalPersonHierarchyItem;
                    }
                }

                UserRightsForTreeObject contractRight;
                var isExistsContractRight = Manager.User.AccumulateRightsAndVerify(contract, EnumObjectRightType.SeeDbObjects, juridicalPersonRight, out contractRight);

                minID--;
                var contractTreeId = minID;
                var newContract = new FreeHierarchyTreeItem(descriptor, contract)
                {
                    FreeHierItem_ID = minID,
                    FreeHierItemType = EnumFreeHierarchyItemType.Contract,
                    NodeRights = contractRight,
                    Parent = juridicalPersonHierarchyItem,
                    IsChildrenInitializet = true,
                };

                var isExistsChildSeeDbObjects = false;
                var absentSections = new Dictionary<int, FreeHierarchyTreeItem>();
                var removedItems = new List<int>();
                // построение сечений
                foreach (var section in sectionIds.Select(s=> GlobalSectionsDictionary.SectionsList[s]))
                {
                    if (section == null) continue;
                    
                    UserRightsForTreeObject sectionRight;
                    var  isExistsSectionRight = Manager.User.AccumulateRightsAndVerify(section, EnumObjectRightType.SeeDbObjects, contractRight, out sectionRight);
                    if (sectionRight.IsDeniedRight(EnumObjectRightType.SeeDbObjects)) continue;

                    minID--;
                    var newSection = new FreeHierarchyTreeItem(descriptor, section)
                    {
                        FreeHierItem_ID = minID,
                        FreeHierItemType = EnumFreeHierarchyItemType.Section,
                        Parent = newContract,
                        NodeRights = sectionRight,
                        IsChildrenInitializet = true,
                    };
                    var sectionTreeId = minID;
                    var isExistsSectionChildrenRight = false;
                    var removedTp = new List<int>();
                    Dictionary<int, FreeHierarchyTreeItem> dirConsumers;
                    directConsumerDictionary.TryGetValue(section.Section_ID, out dirConsumers);

                    if (section.TP != null)
                    {
                        foreach (var tp in section.TP.OrderBy(s => s.Item.StringName))
                        {
                            if (tp.DirectConsumer_ID != null)
                            {
                                if (dirConsumers == null)
                                {
                                    dirConsumers = new Dictionary<int, FreeHierarchyTreeItem>();
                                    directConsumerDictionary.Add(section.Section_ID, dirConsumers);
                                }

                                FreeHierarchyTreeItem dirCon = null;
                                if (!dirConsumers.TryGetValue(tp.DirectConsumer_ID.Value, out dirCon) || dirCon == null)
                                {
                                    var dc = EnumClientServiceDictionary.DirectConsumers[tp.DirectConsumer_ID.Value];
                                    UserRightsForTreeObject directConsumerRight;
                                    if (!Manager.User.AccumulateRightsAndVerify(dc, EnumObjectRightType.SeeDbObjects, sectionRight, out directConsumerRight)) continue;
                                    isExistsSectionChildrenRight = true;
                                    minID--;
                                    dirCon = new FreeHierarchyTreeItem(descriptor, dc)
                                    {
                                        FreeHierItem_ID = minID,
                                        FreeHierItemType = EnumFreeHierarchyItemType.DirectConsumer,
                                        Parent = newSection,
                                        NodeRights = directConsumerRight,
                                        IsChildrenInitializet = true,
                                    };
                                    newSection.Children.Add(dirCon);
                                    result.TryAdd(minID, dirCon);
                                    removedTp.Add(minID);
                                    dirCon.Descriptor.Tree = result;
                                    dirConsumers[dc.DirectConsumer_ID] = dirCon;
                                }

                                // добавление ТП в объект ЭПУ
                                addTP(dirCon, tp, ref minID, descriptor, result, removedTp);
                            }
                            // добавление ТП в сечение (в случае, если ТП не связано с прямым потребителем)
                            else
                            {
                                addTP(newSection, tp, ref minID, descriptor, result, removedTp);
                            }
                        }
                    }

                    if (isExistsSectionRight || isExistsSectionChildrenRight)
                    {
                        isExistsChildSeeDbObjects = true;
                        newContract.Children.Add(newSection);
                        result.TryAdd(sectionTreeId, newSection);
                        newSection.Descriptor.Tree = result;
                    }
                    else
                    {
                        removedItems.AddRange(removedTp);
                        absentSections.Add(sectionTreeId, newSection);
                    }
                }

                if (juridicalPersonHierarchyItem != null)
                {
                    if (isNewPerson)
                    {
                        UserRightsForTreeObject parentRights;
                        buildParents(juridicalPersonHierarchyItem, isFactPowerTree, isExistsJuridicalPersonRight | isExistsContractRight, isExistsChildSeeDbObjects,
                            ref minID,
                            out parentRights);

                        juridicalPersonHierarchyItem.NodeRights = parentRights.AccamulateRights(juridicalPersonRight);
                    }

                    isExistsContractRight = isExistsContractRight |
                                            Manager.User.IsAssentRight(juridicalPersonHierarchyItem.HierObject.GetObjectRightType(), EnumObjectRightType.SeeDbObjects);

                    if (isExistsContractRight || isExistsChildSeeDbObjects)
                    {
                        newContract.NodeRights = newContract.NodeRights.AccamulateRights(juridicalPersonHierarchyItem.NodeRights);
                        //Добавляем пропущенные сечения
                        foreach (var freeHierarchyTreeItem in absentSections)
                        {
                            freeHierarchyTreeItem.Value.NodeRights = freeHierarchyTreeItem.Value.NodeRights.AccamulateRights(juridicalPersonHierarchyItem.NodeRights);
                            newContract.Children.Add(freeHierarchyTreeItem.Value);
                            result.TryAdd(freeHierarchyTreeItem.Key, freeHierarchyTreeItem.Value);
                            freeHierarchyTreeItem.Value.Descriptor.Tree = result;
                            removedItems.Add(freeHierarchyTreeItem.Key);
                            foreach (var sectionChildren in freeHierarchyTreeItem.Value.Children)
                            {
                                sectionChildren.NodeRights = sectionChildren.NodeRights.AccamulateRights(juridicalPersonHierarchyItem.NodeRights);
                            }
                        }

                        if (isNewPerson && rId.HasValue)
                        {
                            juridicalPersonHierarchyItem.Descriptor.Tree = result;
                            result.TryAdd(rId.Value, juridicalPersonHierarchyItem);
                            removedItems.Add(rId.Value);
                        }

                        juridicalPersonHierarchyItem.Children.Add(newContract);
                    }
                    
                }

                if (isExistsContractRight || isExistsChildSeeDbObjects)
                {
                    newContract.Descriptor.Tree = result;
                    result.TryAdd(contractTreeId, newContract);
                }
                else
                {
                    //Удаление т.к. нет прав
                    foreach (var removedItemId in removedItems)
                    {
                        FreeHierarchyTreeItem removed;
                        result.TryRemove(removedItemId, out removed);
                    }
                }
            }

            
            //Добавляем пропущенные юр. лица (у которых нет привязки к контракту)
            if (isAddVoidJuridicalPerson)
            {
                var contractByPersonDict = EnumClientServiceDictionary.ContractByPersonDict;
                //contractByPersonDict.Prepare(new HashSet<int>(EnumClientServiceDictionary.JuridicalPersonsDict.Values.Select(jp=>jp.Item.JuridicalPerson_ID)));
                foreach (var juridicalPerson in EnumClientServiceDictionary.JuridicalPersonsDict.Values)
                {
                    var id = juridicalPerson.Item.JuridicalPerson_ID;
                    //пропускаем уже добавленные или те на которые нехватает прав
                    if (juridicalPersonsDictionary.ContainsKey(id)) continue;

                    UserRightsForTreeObject juridicalPersonRight;
                    var isExistsJuridicalPersonRight = Manager.User.AccumulateRightsAndVerify(juridicalPerson, EnumObjectRightType.SeeDbObjects, null, out juridicalPersonRight);

                    var rId = --minID;
                    var juridicalPersonHierarchyItem = new FreeHierarchyTreeItem(descriptor, juridicalPerson)
                    {
                        FreeHierItem_ID = rId,
                        FreeHierItemType = EnumFreeHierarchyItemType.JuridicalPerson,
                        NodeRights = juridicalPerson.GetObjectRightType(),
                    };

                    var isExistsContractRight = false;

                    UserRightsForTreeObject parentRights;
                    buildParents(juridicalPersonHierarchyItem, isFactPowerTree, isExistsJuridicalPersonRight, isExistsContractRight,
                        ref minID,
                        out parentRights);
                    juridicalPersonHierarchyItem.NodeRights = parentRights.AccamulateRights(juridicalPersonRight);

                    List<Dict_JuridicalPersons_Contract> contracts;
                    if (contractByPersonDict != null && contractByPersonDict.TryGetValue(id, out contracts) && contracts.Count > 0)
                    {
                        //Перебирам контракты для этого юр. лица
                        foreach (var contract in contracts)
                        {

                            UserRightsForTreeObject contractRight;
                            if (!Manager.User.AccumulateRightsAndVerify(contract, EnumObjectRightType.SeeDbObjects, juridicalPersonHierarchyItem.NodeRights, out contractRight)) continue;

                            isExistsContractRight = true;

                            FreeHierarchyTreeItem newContract = new FreeHierarchyTreeItem(descriptor, contract)
                            {
                                FreeHierItem_ID = --minID,
                                FreeHierItemType = EnumFreeHierarchyItemType.Contract,
                                NodeRights = contractRight,
                                Parent = juridicalPersonHierarchyItem,
                            };

                            newContract.NodeRights = newContract.NodeRights.AccamulateRights(juridicalPersonHierarchyItem.NodeRights);
                            newContract.Descriptor.Tree = result;
                            juridicalPersonHierarchyItem.Children.Add(newContract);
                            result.TryAdd(minID, newContract);
                        }
                    }

                    if (Manager.User.IsAssentRight(juridicalPersonHierarchyItem.NodeRights, EnumObjectRightType.SeeDbObjects) || isExistsContractRight)
                    {
                        juridicalPersonHierarchyItem.Descriptor.Tree = result;
                        result.TryAdd(rId, juridicalPersonHierarchyItem);
                    }
                }
            }

            if (isFactPowerTree)
            {
                // добавление оставшихся сечений (те, что не привязаны к юр.лицам и договорам)
                foreach (var section in GlobalSectionsDictionary.SectionsList.Values)
                {
                    if (section.Section_ID <= 0 || sectionsWithContracts.Contains(section.Section_ID)) continue;

                    minID--;
                    UserRightsForTreeObject sectionRight;
                    var isExistsSectionRight = Manager.User.AccumulateRightsAndVerify(section, EnumObjectRightType.SeeDbObjects, null, out sectionRight);

                    var newSection = new FreeHierarchyTreeItem(descriptor, section)
                                                       {
                                                           FreeHierItem_ID = minID,
                                                           FreeHierItemType = EnumFreeHierarchyItemType.Section,
                                                       };

                    var rId = minID;
                    newSection.Descriptor.Tree = result;
                    // построение родителей иерархической структуры
                    UserRightsForTreeObject parentRights = null;
                    buildParents(newSection, true, isExistsSectionRight, isExistsSectionRight, ref minID, out parentRights);
                    newSection.NodeRights = parentRights;

                    if (section.TP != null)
                    {
                        result[rId] = newSection;
                        var removedTp = new List<int>();
                        foreach (var tp in section.TP)
                        {
                            addTP(newSection, tp, ref minID, descriptor, result, removedTp);
                        }
                    }
                }
            }

            // просчет кол-ва элементов всех узлов и сортировка
            countElements(result.Where(i => i.Value.Parent == null).Select(i => i.Value));

            return result;
        }

        private static bool buildParents(FreeHierarchyTreeItem item, bool isFactPowerTree, bool isExistsContractRight, bool isExistsChildSeeDbObjects, ref int minID, out UserRightsForTreeObject parentRights)
        {
            parentRights = null;
            isExistsContractRight = isExistsContractRight | Manager.User.IsAssentRight(item.NodeRights, EnumObjectRightType.SeeDbObjects);

            int parentID = -1;
            enumTypeHierarchy parentType= enumTypeHierarchy.Unknown;

            var juridicalPerson = item.HierObject as TJuridicalPerson;
            if (juridicalPerson != null && juridicalPerson.ParrentID != null)
            {
                //Не строим юр. лица у которых отсутствует родитель
                parentID = juridicalPerson.ParrentID.ID;
                parentType = juridicalPerson.ParrentID.TypeHierarchy;
            }
            else
            {
                var section = item.HierObject as TSection;
                if (section != null)
                {
                    parentID = section.ParentId;
                    parentType = section.TypeParentHierarchy;
                }
            }

            bool isNew = true, isNewNext = false;
            var saveItem = item;
            var isParrentExists = true;

            //Права уровня ПС
            UserRightsForTreeObject hietLevPsRights = null;

            //Права уровня 3
            UserRightsForTreeObject hietLev3Rights = null;

            //Права уровня 2
            UserRightsForTreeObject hietLev2Rights = null;

            var removedFreeItems = new Stack<Tuple<FreeHierarchyTreeItem, bool>>();

            while (isParrentExists && parentType != enumTypeHierarchy.Dict_HierLev0)
            {
                switch (parentType)
                {
                    case enumTypeHierarchy.Dict_PS:
                        TPSHierarchy ps;
                        if (!EnumClientServiceDictionary.DetailPSList.TryGetValue(parentID, out ps) || ps == null)
                        {
                            isParrentExists = false;
                            break;
                        }

                        if (isFactPowerTree)
                        {
                            Manager.User.AccumulateRightsAndVerify(ps, EnumObjectRightType.SeeDbObjects, null, out hietLevPsRights);
                            FreeHierarchyTreeItem psFreeHierarchyTreeItem =
                                item.Descriptor.Tree.Select(i => i.Value)
                                    .FirstOrDefault(i => i.FreeHierItemType == EnumFreeHierarchyItemType.PS && i.HierObject != null && i.HierObject.Id == parentID);
                            if (psFreeHierarchyTreeItem == null)
                            {
                                minID--;
                                psFreeHierarchyTreeItem = new FreeHierarchyTreeItem(item.Descriptor, ps)
                                {
                                    FreeHierItem_ID = minID,
                                    FreeHierItemType = EnumFreeHierarchyItemType.PS,
                                    IsChildrenInitializet = true,
                                };
                                item.Descriptor.Tree.TryAdd(minID, psFreeHierarchyTreeItem);
                                isNewNext = true;
                            }
                            else isNewNext = false;
                            if (isNew) psFreeHierarchyTreeItem.Children.Add(item);
                            removedFreeItems.Push(new Tuple<FreeHierarchyTreeItem, bool>(psFreeHierarchyTreeItem, isNew));
                            isNew = isNewNext;
                            item.Parent = psFreeHierarchyTreeItem;
                            item = psFreeHierarchyTreeItem;
                        }

                        parentID = ps.ParentId;
                        parentType = enumTypeHierarchy.Dict_HierLev3;
                        break;
                    case enumTypeHierarchy.Dict_HierLev3:
                        THierarchyDbTreeObject hier3Hierarchy;
                        if (!EnumClientServiceDictionary.HierLev3List.TryGetValue(parentID, out hier3Hierarchy) || hier3Hierarchy == null)
                        {
                            isParrentExists = false;
                            break;
                        }

                        if (isFactPowerTree)
                        {
                            Manager.User.AccumulateRightsAndVerify(hier3Hierarchy, EnumObjectRightType.SeeDbObjects, null, out hietLev3Rights);
                            FreeHierarchyTreeItem hierLev3FreeHierarchyTreeItem =
                                item.Descriptor.Tree.Values
                                    .FirstOrDefault(i => i.FreeHierItemType == EnumFreeHierarchyItemType.HierLev3 && i.HierObject != null && i.HierObject.Id == parentID);
                            if (hierLev3FreeHierarchyTreeItem == null)
                            {
                                minID--;
                                hierLev3FreeHierarchyTreeItem = new FreeHierarchyTreeItem(item.Descriptor, hier3Hierarchy)
                                {
                                    FreeHierItem_ID = minID,
                                    FreeHierItemType = EnumFreeHierarchyItemType.HierLev3,
                                    IsChildrenInitializet = true,
                                };
                                item.Descriptor.Tree.TryAdd(minID, hierLev3FreeHierarchyTreeItem);
                                isNewNext = true;
                            }
                            else isNewNext = false;
                            if (isNew) hierLev3FreeHierarchyTreeItem.Children.Add(item);
                            removedFreeItems.Push(new Tuple<FreeHierarchyTreeItem, bool>(hierLev3FreeHierarchyTreeItem, isNew));
                            isNew = isNewNext;
                            item.Parent = hierLev3FreeHierarchyTreeItem;
                            item = hierLev3FreeHierarchyTreeItem;
                        }
                        parentID = hier3Hierarchy.ParentId;
                        parentType = enumTypeHierarchy.Dict_HierLev2;
                        break;
                    case enumTypeHierarchy.Dict_HierLev2:
                        THierarchyDbTreeObject hier2Hierarchy;
                        if (!EnumClientServiceDictionary.HierLev2List.TryGetValue(parentID, out hier2Hierarchy) || hier2Hierarchy == null)
                        {
                            isParrentExists = false;
                            break;
                        }

                        Manager.User.AccumulateRightsAndVerify(hier2Hierarchy, EnumObjectRightType.SeeDbObjects, null, out hietLev2Rights);

                        FreeHierarchyTreeItem hierLev2 = item.Descriptor.Tree.Select(i => i.Value)
                            .FirstOrDefault(i => i.FreeHierItemType == EnumFreeHierarchyItemType.HierLev2 && i.HierObject != null && i.HierObject.Id == parentID);
                        if (hierLev2 == null)
                        {
                            minID--;
                            hierLev2 = new FreeHierarchyTreeItem(item.Descriptor, EnumClientServiceDictionary.HierLev2List[parentID])
                            {
                                FreeHierItem_ID = minID,
                                FreeHierItemType = EnumFreeHierarchyItemType.HierLev2,
                                IsChildrenInitializet = true,
                            };
                            item.Descriptor.Tree.TryAdd(minID, hierLev2);
                            isNewNext = true;
                        }
                        else isNewNext = false;
                        if (isNew) hierLev2.Children.Add(item);
                        removedFreeItems.Push(new Tuple<FreeHierarchyTreeItem, bool>(hierLev2, isNew));
                        isNew = isNewNext;
                        item.Parent = hierLev2;
                        item = hierLev2;
                        parentID = item.HierObject.ParentId;
                        parentType = enumTypeHierarchy.Dict_HierLev1;
                        break;
                    case enumTypeHierarchy.Dict_HierLev1:
                        THierarchyDbTreeObject hier1Hierarchy;
                        if (!EnumClientServiceDictionary.HierLev1List.TryGetValue(parentID, out hier1Hierarchy) || hier1Hierarchy == null)
                        {
                            isParrentExists = false;
                            break;
                        }

                        UserRightsForTreeObject hietLev1Rights = null;
                        isExistsContractRight = Manager.User.AccumulateRightsAndVerify(hier1Hierarchy, EnumObjectRightType.SeeDbObjects, null, out hietLev1Rights) | isExistsChildSeeDbObjects;

                        UserRightsForTreeObject rights = hietLev1Rights;
                        //while (removedFreeItems.Count > 0)
                        var ri = new Stack<Tuple<FreeHierarchyTreeItem, bool>>();
                        while (removedFreeItems.Count > 0)
                        {
                            var removedFreeItem = removedFreeItems.Pop();
                            ri.Push(removedFreeItem);
                            isExistsContractRight = Manager.User.AccumulateRightsAndVerify(removedFreeItem.Item1.HierObject, EnumObjectRightType.SeeDbObjects, rights, out rights) |
                                                    isExistsContractRight;
                            removedFreeItem.Item1.NodeRights = rights;
                        }

                        //Проверкак прав, убираем если нет прав
                        if (!isExistsContractRight && !isExistsChildSeeDbObjects)
                        {
                            isParrentExists = false;
                            var removedChild = saveItem;
                            while (ri.Count > 0)
                            {
                                //removedChild.Parent = null;
                                var removedFreeItem = ri.Pop();
                                if (removedFreeItem.Item2)
                                {
                                    FreeHierarchyTreeItem removed;
                                    removedChild.Descriptor.Tree.TryRemove(removedFreeItem.Item1.FreeHierItem_ID, out removed);
                                }
                                removedFreeItem.Item1.Children.Remove(removedChild);
                                removedChild = removedFreeItem.Item1;
                            }
                            break;
                        }

                        parentRights = rights;

                        FreeHierarchyTreeItem hierLev1 =
                            item.Descriptor.Tree.Select(i => i.Value)
                                .FirstOrDefault(i => i.FreeHierItemType == EnumFreeHierarchyItemType.HierLev1 && i.HierObject != null && i.HierObject.Id == parentID);
                        if (hierLev1 == null)
                        {
                            minID--;
                            hierLev1 = new FreeHierarchyTreeItem(item.Descriptor, EnumClientServiceDictionary.HierLev1List[parentID])
                            {
                                FreeHierItem_ID = minID,
                                FreeHierItemType = EnumFreeHierarchyItemType.HierLev1,
                                IsChildrenInitializet = true,
                            };
                            item.Descriptor.Tree.TryAdd(minID, hierLev1);
                            isNewNext = true;
                        }
                        else isNewNext = false;
                        if (isNew) hierLev1.Children.Add(item);
                        isNew = isNewNext;
                        item.Parent = hierLev1;
                        item = hierLev1;
                        parentID = item.HierObject.ParentId;
                        parentType = enumTypeHierarchy.Dict_HierLev0;
                        break;
                    case enumTypeHierarchy.Unknown:
                        isParrentExists = false;
                        break;
                }
            }

            return isParrentExists;
        }

        // добавление ТП
        private static void addTP(FreeHierarchyTreeItem parent, TPoint tp, ref int minID,
            FreeHierarchyTreeDescriptor descriptor, ConcurrentDictionary<int, FreeHierarchyTreeItem> result, List<int> removedTp)
        {
            minID--;
            var newTP = new FreeHierarchyTreeItem(descriptor, tp)
            {
                FreeHierItem_ID = minID,
                FreeHierItemType = EnumFreeHierarchyItemType.TP,
                Parent = parent,
                NodeRights = parent.HierObject.GetObjectRightType(),
                IsChildrenInitializet = true,
            };
            parent.Children.Add(newTP);
            result.TryAdd(minID, newTP);
            removedTp.Add(minID);
            newTP.Descriptor.Tree = result;
        }

        private static void countElements(IEnumerable<FreeHierarchyTreeItem> items)
        {
            foreach (var item in items)
            {
                countElements(item.Children);
            }
        }
    }
}
