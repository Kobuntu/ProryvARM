using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.ServiceReference.UAService;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;

namespace Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy
{
    /// <summary>
    /// Ф-ии для прогрузки узла в дереве
    /// </summary>
    public partial class FreeHierarchyTreeItem
    {
        public bool LoadStaticChildren(bool isExistsParentRight, bool isHideTi = false, HashSet<long> selectedNodes = null,
            int recursionNumber = 0, bool isLoadStatic = false)
        {
            //if (!IncludeObjectChildren)
            //{
            //    // Эти объекты автоматически не подгружают дочерние объекты
            //    IsChildrenInitializet = true;
            //    return false;
            //}

            bool isExistsChildSeeDbObjects = false; //Наличие дочерних объектов которые явно имеют признак отображения

            if (isExistsParentRight && HierObject != null && Descriptor.Tree_ID == GlobalFreeHierarchyDictionary.TreeTypeStandartSectionsNSI)
            {
                //Добавление констант
                var hierarchyWithChildren = HierObject as IFreeHierarchyObjectWithChildren;
                if (hierarchyWithChildren != null && hierarchyWithChildren.FormulaConstants != null && hierarchyWithChildren.FormulaConstants.Count > 0)
                {
                    isExistsChildSeeDbObjects = true;

                    foreach (var formulaConstant in hierarchyWithChildren.FormulaConstants)
                    {
                        var formula = new FreeHierarchyTreeItem(Descriptor, formulaConstant, false, string.Empty, Descriptor.GetMinIdAndDecrement(), this, false,
                            freeHierItemType: EnumFreeHierarchyItemType.FormulaConstant, isChildrenInitializet: true);
                        //lock (_syncLock)
                        {
                            _children.Add(formula);
                        }

                        Descriptor.Tree.TryAdd(formula.FreeHierItem_ID, formula);
                    }
                }

                //if (Descriptor.TreeID == GlobalFreeHierarchyDictionary.TreeTypeStandartTIFormula)
                //{
                //    if (IncludeChildrenFormulas(HierObject.Id, HierObject.Type, children)) isExistsChildSeeDbObjects = true;

                //    IncludeChildrenFormulaConstant();
                //}
            }

            if (FreeHierItemType == EnumFreeHierarchyItemType.Error
                || FreeHierItemType == EnumFreeHierarchyItemType.Formula) return false;

            if (_hierObject != null)
            {
                // Эти объекты автоматически не подгружают дочерние объекты
                IsChildrenInitializet = IsChildrenInitializet
                    || _hierObject.Type == enumTypeHierarchy.Dict_HierLev0
                                        //|| _hierObject.Type == enumTypeHierarchy.Dict_HierLev1
                                        //|| _hierObject.Type == enumTypeHierarchy.Dict_HierLev2
                                        //|| _hierObject.Type == enumTypeHierarchy.Dict_HierLev3 //Перевел в динамическую подгрузку
                                        //|| _hierObject.Type == enumTypeHierarchy.Info_TI
                                        || _hierObject.Type == enumTypeHierarchy.Formula
                                        || _hierObject.Type == enumTypeHierarchy.Formula_TP_OurSide
                                        || _hierObject.Type == enumTypeHierarchy.FormulaConstant;
            }

            return isExistsChildSeeDbObjects;
        }

        private bool IncludeForecastObjectChildren(ICollection<FreeHierarchyTreeItem> children)
        {
            bool result = false;

            //Добавлем все константы формул
            var forecastObject = HierObject as TForecastObject;
            if (forecastObject == null || forecastObject.Children == null) return result;

            //EnumClientServiceDictionary.ForecastObjectsDictionary.Prepare(new HashSet<string>(forecastObject.Children));

            foreach (var forecastObjectUn in forecastObject.Children)
            {
                var forecastObjectChild = EnumClientServiceDictionary.ForecastObjectsDictionary[forecastObjectUn];
                if (forecastObjectChild != null)
                {

                    var newItem = new FreeHierarchyTreeItem(Descriptor, forecastObjectChild, false, string.Empty, Descriptor.GetMinIdAndDecrement(), this, true,
                        freeHierItemType: EnumFreeHierarchyItemType.ForecastObject);

                    children.Add(newItem);
                    Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);
                    newItem.Descriptor.Tree = Descriptor.Tree;

                    newItem.IncludeForecastObjectChildren(newItem.Children);

                    result = true;
                }
            }

            return result;
        }

        public bool IncludeChildrenOPCNodes(ICollection<FreeHierarchyTreeItem> children, bool isSelected = false, HashSet<long> selectedNodes = null,
            bool includeChildren = true)
        {
            if (HierObject == null) return false;
            Descriptor.NeedFindUaNode = true;
            var mainNode = HierObject as TUANode;
            var result = false;
            if (mainNode == null || mainNode.DependentNodes == null) return false;
            IsUaNodesInitializet = true;
            var uadependentnodes = UAHierarchyDictionaries.UANodesDict.GetValues(
                new HashSet<long>(mainNode.DependentNodes
                    .Where(c => c.UAReferenceType != EnumOpcReferenceType.HasTypeDefinition)
                    .Select(c => c.UANodeId)), Manager.UI.ShowMessage);
            //Перебор всех зависимых OPC узлов
            foreach (var dependentNode in uadependentnodes)
            {

                UserRightsForTreeObject right;
                var isExistsRight = Manager.User.AccumulateRightsAndVerify(dependentNode, EnumObjectRightType.SeeDbObjects, HierObject.GetObjectRightType(), out right);


                var selected = isSelected || (selectedNodes != null && selectedNodes.Contains(dependentNode.UANodeId));

                var newItem = new FreeHierarchyTreeItem(Descriptor, dependentNode, selected, string.Empty, Descriptor.GetMinIdAndDecrement(), this, true,
                    nodeRights: right, freeHierItemType: EnumFreeHierarchyItemType.UANode);

                var isExistsChildSeeDbObjects = false;
                if (children.Any(i => i.GetKey == newItem.GetKey))
                {
                    // newItem.Dispose();
                    return result;
                }
                if (includeChildren) isExistsChildSeeDbObjects = newItem.IncludeChildrenOPCNodes(newItem.Children, isSelected, selectedNodes);

                //У объекта есть права на просмотр, или на дочерний объект
                if (isExistsRight || isExistsChildSeeDbObjects)
                {
                    children.Add(newItem);
                    Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);

                    result = true;
                }
                else
                {
                    newItem.Dispose();
                }
            }

            return result;
        }

        public bool IncludeChildrenFormulas(long parentID, enumTypeHierarchy parentType, ICollection<FreeHierarchyTreeItem> children)
        {
            if (EnumClientServiceDictionary.FormulasList == null) return false;
            bool result = false;

            var isSelected = IsSelectedChildren;

            foreach (var f in EnumClientServiceDictionary.FormulasList.Where(i => i.Value.ParentId == parentID && i.Value.ParentType == parentType))
            {

                var newItem = new FreeHierarchyTreeItem(Descriptor, f.Value, isSelected, string.Empty, Descriptor.GetMinIdAndDecrement(), this, true,
                    freeHierItemType: EnumFreeHierarchyItemType.Formula, isChildrenInitializet: true);

                children.Add(newItem);
                Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);
                newItem.Descriptor.Tree = Descriptor.Tree;
                result = true;
            }

            return result;
        }

        public bool IncludeChildrenSection(int parentID, enumTypeHierarchy parentType, ICollection<FreeHierarchyTreeItem> children)
        {
            var isExistsSeeDbRights = false;
            foreach (var section in GlobalSectionsDictionary.SectionsList.Values.Where(s => s.TypeParentHierarchy == parentType && s.ParentId == parentID))
            {
                UserRightsForTreeObject right;
                if (!Manager.User.AccumulateRightsAndVerify(section, EnumObjectRightType.SeeDbObjects, HierObject.GetObjectRightType(), out right)) continue;

                isExistsSeeDbRights = true;


                var newItem = new FreeHierarchyTreeItem(Descriptor, section, false, string.Empty, Descriptor.GetMinIdAndDecrement(), this, true,
                    nodeRights: right, freeHierItemType: EnumFreeHierarchyItemType.Section);

                children.Add(newItem);
                Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);
                newItem.Descriptor.Tree = Descriptor.Tree;
                newItem.LoadStaticChildren(true);
            }

            return isExistsSeeDbRights;
        }

        private void IncludeChildrenFormulaConstant(ICollection<FreeHierarchyTreeItem> chidren)
        {
            //Добавлем все константы формул
            var withChildrenObject = HierObject as IFreeHierarchyObjectWithChildren;
            if (withChildrenObject == null || withChildrenObject.FormulaConstants == null) return;

            var isSelected = IsSelectedChildren;

            foreach (var constant in withChildrenObject.FormulaConstants)
            {

                var newItem = new FreeHierarchyTreeItem(Descriptor, constant, isSelected, string.Empty,
                    Descriptor.GetMinIdAndDecrement(), this, true,
                    freeHierItemType: EnumFreeHierarchyItemType.FormulaConstant, isChildrenInitializet: true);

                chidren.Add(newItem);
                Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);
                newItem.Descriptor.Tree = Descriptor.Tree;
            }
        }

        private void IncludeChildrenBalances(ICollection<FreeHierarchyTreeItem> chidren)
        {
            //var objectIds = new HashSet<ID_TypeHierarchy>(chidren.Where(o=>o.HierObject!=null).Select(o => new ID_TypeHierarchy
            //{
            //    TypeHierarchy = o.HierObject.Type,
            //    ID = o.HierObject.Id,
            //    StringId = o.HierObject.StringId,
            //}), new ID_TypeHierarchy_EqualityComparer());

            //EnumClientServiceDictionary.FreeHierarchyBalances.Prepare(objectIds, Manager.UI.ShowMessage);

            if (HierObject == null) return;

            List<ServiceReference.ARM_20_Service.Info_Balance_FreeHierarchy_List> objectBalances;

            if (!EnumClientServiceDictionary.FreeHierarchyBalancesByParent.TryGetValue(new ID_TypeHierarchy
            {
                TypeHierarchy = HierObject.Type,
                ID = HierObject.Id,
                StringId = HierObject.StringId,
            }, out objectBalances, false) || objectBalances == null || objectBalances.Count == 0) return;

            foreach (var balance in objectBalances)
            {
                if (balance.BalanceFreeHierarchyType_ID > 1) continue; //Допустимы только балансы ЭЭ

                var newItem = new FreeHierarchyTreeItem(Descriptor, balance, false, string.Empty,
                    Descriptor.GetMinIdAndDecrement(), this, true,
                    freeHierItemType: EnumFreeHierarchyItemType.UniversalBalance, isChildrenInitializet: true);

                chidren.Add(newItem);
                Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);
                newItem.Descriptor.Tree = Descriptor.Tree;
            }
        }

        internal bool CopyFromWCFClass(TFreeHierarchyTreeItem source, int freeHierTreeID, bool isUserAdmin = false)
        {
            if (NodeRights == null)
            {
                NodeRights = source.UserTreeItemRight;
            }

            SortNumber = source.SortNumber;
            EnumFreeHierarchyItemType freeHierItemType;
            _hierObject = FreeHierarchyTreePreparer.GetTreeItemFromWcfClass(source, freeHierTreeID, out freeHierItemType, isUserAdmin);

            if (freeHierItemType == EnumFreeHierarchyItemType.Error) return false;

            FreeHierItemType = freeHierItemType;

            bool result = _hierObject != null;
            if (!result)
            {
                switch (FreeHierItemType)
                {
                    case EnumFreeHierarchyItemType.USPD:
                        {
                            if (source.USPD_ID.HasValue)
                            {
                                var uspd = FreeHierarchyDictionaries.USPDDict[source.USPD_ID.Value];
                                //Пока не отображаем в дереве УСПД для которых нет привязки к ПС
                                if (uspd != null && uspd.PS_ID.HasValue) _hierObject = uspd;
                                SetUSPD_ID(source.USPD_ID.Value);
                            }
                            break;
                        }
                    case EnumFreeHierarchyItemType.XMLSystem:
                        {
                            if (source.XMLSystem_ID.HasValue)
                            {
                                _hierObject = XMLSystem = GlobalHierLev1HierarhyDictionary.XMLSystems[source.XMLSystem_ID.Value];
                            }
                            break;
                        }
                }
            }

            if (_hierObject != null)
            {
                _hierObject.GetNodeRight = GetNodeRight;
                result = true;
            }

            return result;
        }
    }
}
