using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using Proryv.AskueARM2.Client.ServiceReference.Service.Interfaces;
using Proryv.AskueARM2.Client.ServiceReference.Service.Extensions;
using System.Threading;

namespace Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy
{
    public static class FreeHierarchyTreePreparer
    {
        /// <summary>
        /// Динамически строим ветку в дереве
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="tempList"></param>
        /// <param name="descriptor"></param>
        /// <param name="isHideTi"></param>
        /// <param name="selectedNodes"></param>
        /// <returns></returns>
        public static List<FreeHierarchyTreeItem> BuildBranch(FreeHierarchyTreeItem parent, List<TFreeHierarchyTreeItem> tempList, FreeHierarchyTreeDescriptor descriptor
            , bool isHideTi = false, HashSet<long> selectedNodes = null, bool includeObjectChildrenForPs = false)
        {
            var result = new List<FreeHierarchyTreeItem>();

            UserRightsForTreeObject parentRights = null;
            bool isUserAdmin = false;
            bool isExistsParentRight = false;
            if (Manager.User != null)
            {
                isExistsParentRight = isUserAdmin = Manager.User.IsAdmin;
                if (!isUserAdmin && parent!=null)
                {
                    isExistsParentRight = Manager.User.IsAssentRight(parent.NodeRights, EnumObjectRightType.SeeDbObjects);
                    parentRights = parent.NodeRights;
                }
            }

            //if (parent != null && parent.IncludeObjectChildren)
            //{
            //    parent.LoadStaticChildren(isExistsParentRight, isHideTi, selectedNodes);
            //    if (parent.HierObject != null && parent.HierObject.Type == enumTypeHierarchy.Node)
            //    {
            //        parent.IncludeChildrenFormulas(parent.HierObject.Id, enumTypeHierarchy.Node, result);
            //    }

            //    PrepareGlobalDictionaries(tempList);
            //}
                        
            foreach (var value in tempList.OrderBy(i => i.HierLevel))
            {
                if (value == null) continue;

                UserRightsForTreeObject rights = value.UserTreeItemRight;
                bool isExistsChildSeeDbObjects = isExistsParentRight;
                if (!isExistsParentRight)
                {
                    isExistsChildSeeDbObjects = Manager.User.IsAssentRight(rights, EnumObjectRightType.SeeDbObjects);
                }

                if (parentRights != null)
                {
                    rights = parentRights.AccamulateRights(rights);
                }

                //Не администратор, нет собственных прав на просмотр, и нет таких прав у дочерних
                if (!isExistsChildSeeDbObjects && (!value.IsExistsChildSeeDbObjects.HasValue || !value.IsExistsChildSeeDbObjects.Value)) continue;

                var isSelected = value.FreeHierItemType == EnumFreeHierarchyItemType.UANode && selectedNodes != null &&
                                 selectedNodes.Contains(value.UANode_ID.GetValueOrDefault());

                var includeObjectChildren = value.IncludeObjectChildren;
                
                var newItem = new FreeHierarchyTreeItem(descriptor, parent, isSelected, value.StringName)
                {
                    FreeHierItem_ID = value.FreeHierItem_ID,
                    IncludeObjectChildren = includeObjectChildren,
                    IsHideTi = isHideTi,
                    NodeRights = rights,
                    NodeIcon_ID = value.NodeIcon_ID,
                    Parent = parent,
                };

                if (!newItem.CopyFromWCFClass(value, descriptor.Tree_ID ?? -101, isUserAdmin))
                {
                    //Ошибка чтения из БД
                    newItem.Dispose();
                    continue;
                }

                if (includeObjectChildrenForPs && !includeObjectChildren &&
                    (newItem.FreeHierItemType == EnumFreeHierarchyItemType.PS || newItem.FreeHierItemType == EnumFreeHierarchyItemType.Section 
                    || newItem.FreeHierItemType == EnumFreeHierarchyItemType.UANode))
                {
                    newItem.IncludeObjectChildren = true;
                }

                newItem.LoadStaticChildren(isExistsParentRight, isHideTi,  selectedNodes);

                if (newItem.HierObject != null && newItem.HierObject.Type == enumTypeHierarchy.Node)
                {
                    newItem.IncludeChildrenFormulas(newItem.HierObject.Id, enumTypeHierarchy.Node, newItem.Children);
                }

                result.Add(newItem);

                if (parent != null && parent.Descriptor!=null)
                {
                    parent.Descriptor.Tree.TryAdd(newItem.FreeHierItem_ID, newItem);  
                }

            }

            return result;
        }


        /// <summary>
        /// Проверяем права по дереву свободной иерархии
        /// </summary>
        /// <param name="treeId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public static bool ExistsFreeHierChildSeeDbObjects(int treeId, int? parentId)
        {
            var tempList = FreeHierarchyTreeDictionary.GetBranch(Manager.User.User_ID, treeId, parentId, false);
            if (tempList == null) return false;

            foreach (var item in tempList)
            {
                if (item == null) continue;

                var rights = item.UserTreeItemRight;
                if (Manager.User.IsAssentRight(rights, EnumObjectRightType.SeeDbObjects) 
                    || ExistsFreeHierChildSeeDbObjects(treeId, item.FreeHierItem_ID)
                    || (item.IncludeObjectChildren && ExistsStandartChildSeeDbObjects(item, treeId)))  return true;
            }

            return false;
        }

        /// <summary>
        /// Проверяем права по стандартному дереву
        /// </summary>
        /// <param name="treeId">Идентификатор дерева</param>
        /// <param name="treeItem">Объект иерархии (WCF)</param>
        /// <returns></returns>
        public static bool ExistsStandartChildSeeDbObjects(TFreeHierarchyTreeItem treeItem, int treeId)
        {
            EnumFreeHierarchyItemType hierItemType;
            var hierObject = GetTreeItemFromWcfClass(treeItem, treeId, out hierItemType, false);

            return ExistsStandartChildSeeDbObjects(hierObject, treeId, false);
        }

        /// <summary>
        /// Проверяем права по стандартному дереву
        /// </summary>
        /// <param name="treeId">Идентификатор дерева</param>
        /// <param name="hierObject">Объект иерархии</param>
        /// <param name="viewChildren">Проверять ли дальше по стандартному дереву</param>
        /// <returns></returns>
        public static bool ExistsStandartChildSeeDbObjects(IFreeHierarchyObject hierObject, int treeId, bool viewChildren)
        {
            if (hierObject == null 
                || hierObject.Type == enumTypeHierarchy.Dict_PS //У ПС не проверяем права на дочерние объекты
                || hierObject.HierarchyChildren == null) return false;

            foreach (var hierLev in hierObject.HierarchyChildren)
            {
                var isExistsChildSeeDbObjects = Manager.User.IsAssentRight(hierLev, EnumObjectRightType.SeeDbObjects, null);

                if (!isExistsChildSeeDbObjects && viewChildren)
                {
                    isExistsChildSeeDbObjects = ExistsStandartChildSeeDbObjects(hierLev, treeId, viewChildren);
                }

                if (isExistsChildSeeDbObjects) return true;
            }

            return false;
        }

        /// <summary>
        /// Подготовка глобальных словарей
        /// </summary>
        /// <param name="tempList"></param>
        public static void PrepareGlobalDictionaries(IEnumerable<TFreeHierarchyTreeItem> tempList, EnumFreeHierarchyTreePrepareMode prepareMode = EnumFreeHierarchyTreePrepareMode.All)
        {
            if (tempList == null) return;

            var psList = new HashSet<int>();
            var uspdList = new HashSet<int>();//используется только по необходимости
            var uaList = new HashSet<long>();//используется только по необходимости
            var tiList = new HashSet<int>();
            var forecastObjectUns = new HashSet<string>();
            var formulaUns = new HashSet<string>();
            var formulaParents = new List<ID_TypeHierarchy>();
            var freeHierNodeIds = new HashSet<int>();

            foreach (var t in tempList)
            {
                switch (t.FreeHierItemType)
                {
                    case EnumFreeHierarchyItemType.USPD:
                        if (t.USPD_ID.HasValue)
                        {
                            uspdList.Add(t.USPD_ID.Value);
                        }
                        break;
                    case EnumFreeHierarchyItemType.UANode:
                        if (t.UANode_ID.HasValue)
                        {
                            uaList.Add(t.UANode_ID.Value);
                        }
                        break;
                    case EnumFreeHierarchyItemType.TI:
                        if (t.TI_ID.HasValue)
                        {
                            tiList.Add(t.TI_ID.Value);
                        }
                        break;
                    case EnumFreeHierarchyItemType.PS:
                        if (t.PS_ID.HasValue && t.IncludeObjectChildren)
                        {
                            psList.Add(t.PS_ID.Value);
                            {
                                var id = new ID_TypeHierarchy { ID = t.PS_ID.Value, TypeHierarchy = enumTypeHierarchy.Dict_PS };
                                formulaParents.Add(id);
                            }
                        }
                        break;
                    case EnumFreeHierarchyItemType.ForecastObject:
                        if (!string.IsNullOrEmpty(t.ForecastObject_UN))
                        {
                            forecastObjectUns.Add(t.ForecastObject_UN);
                        }
                        break;
                    case EnumFreeHierarchyItemType.Formula:
                        if (!string.IsNullOrEmpty(t.Formula_UN))
                        {
                            //Формулы не подготавливаем
                            //formulaUns.Add(t.Formula_UN);
                        }
                        break;
                    case EnumFreeHierarchyItemType.Node:
                        freeHierNodeIds.Add(t.FreeHierItem_ID);
                        {
                            var id = new ID_TypeHierarchy { ID = t.FreeHierItem_ID, TypeHierarchy = enumTypeHierarchy.Node };
                            formulaParents.Add(id);
                        }
                        break;
                    case EnumFreeHierarchyItemType.HierLev3:
                        if (t.HierLev3_ID.HasValue)
                        {
                            var id = new ID_TypeHierarchy { ID = t.HierLev3_ID.Value, TypeHierarchy = enumTypeHierarchy.Dict_HierLev3 };
                            formulaParents.Add(id);
                        }
                        break;
                    case EnumFreeHierarchyItemType.HierLev2:
                        if (t.HierLev2_ID.HasValue)
                        {
                            var id = new ID_TypeHierarchy { ID = t.HierLev2_ID.Value, TypeHierarchy = enumTypeHierarchy.Dict_HierLev2 };
                            formulaParents.Add(id);
                        }
                        break;
                    case EnumFreeHierarchyItemType.HierLev1:
                        if (t.HierLev1_ID.HasValue)
                        {
                            var id = new ID_TypeHierarchy { ID = t.HierLev1_ID.Value, TypeHierarchy = enumTypeHierarchy.Dict_HierLev1 };
                            formulaParents.Add(id);
                        }
                        break;
                }
            }

            PrepareDictsFromServer(prepareMode, psList, uspdList, uaList, tiList, freeHierNodeIds, 
                forecastObjectUns, formulaParents, formulaUns);
        }

        /// <summary>
        /// Подготовка глобальных словарей
        /// </summary>
        /// <param name="tempList"></param>
        public static void PrepareGlobalDictionaries(IEnumerable<FreeHierarchyTreeItem> tempList,
            EnumFreeHierarchyTreePrepareMode prepareMode = EnumFreeHierarchyTreePrepareMode.All)
        {
            if (tempList == null || !tempList.Any()) return;

#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            var psList = new HashSet<int>();
            var uspdList = new HashSet<int>();//используется только по необходимости
            var uaList = new HashSet<long>();//используется только по необходимости
            var tiList = new HashSet<int>();
            var forecastObjectUns = new HashSet<string>();
            var formulaUns = new HashSet<string>();
            var formulaParents = new List<ID_TypeHierarchy>();
            var freeHierNodeIds = new HashSet<int>();

            foreach (var t in tempList)
            {
                if (t.HierObject == null) continue;

                switch (t.FreeHierItemType)
                {
                    case EnumFreeHierarchyItemType.USPD:
                        uspdList.Add(t.HierObject.Id);
                        break;
                    case EnumFreeHierarchyItemType.UANode:
                        uaList.Add(t.HierObject.Id);
                        break;
                    case EnumFreeHierarchyItemType.TI:
                        tiList.Add(t.HierObject.Id);
                        break;
                    case EnumFreeHierarchyItemType.PS:
                        psList.Add(t.HierObject.Id);
                            formulaParents.Add(new ID_TypeHierarchy { ID = t.HierObject.Id, TypeHierarchy = enumTypeHierarchy.Dict_PS });
                        break;
                    case EnumFreeHierarchyItemType.ForecastObject:
                        if (!string.IsNullOrEmpty(t.HierObject.StringId))
                        {
                            forecastObjectUns.Add(t.HierObject.StringId);
                        }
                        break;
                        //Формулы не подготавливаем
                    //case EnumFreeHierarchyItemType.Formula:
                    //    if (!string.IsNullOrEmpty(t.HierObject.StringId))
                    //    {
                    //        formulaUns.Add(t.HierObject.StringId);
                    //    }
                    //    break;
                    case EnumFreeHierarchyItemType.Node:
                        freeHierNodeIds.Add(t.FreeHierItem_ID);
                        {
                            var id = new ID_TypeHierarchy { ID = t.FreeHierItem_ID, TypeHierarchy = enumTypeHierarchy.Node };
                            formulaParents.Add(id);
                        }
                        break;
                    case EnumFreeHierarchyItemType.HierLev3:
                        formulaParents.Add(new ID_TypeHierarchy { ID = t.HierObject.Id, TypeHierarchy = enumTypeHierarchy.Dict_HierLev3 });
                        break;
                    case EnumFreeHierarchyItemType.HierLev2:
                        formulaParents.Add(new ID_TypeHierarchy { ID = t.HierObject.Id, TypeHierarchy = enumTypeHierarchy.Dict_HierLev2 });
                        break;
                    case EnumFreeHierarchyItemType.HierLev1:
                        formulaParents.Add(new ID_TypeHierarchy { ID = t.HierObject.Id, TypeHierarchy = enumTypeHierarchy.Dict_HierLev1 });
                        break;
                }
            }

#if DEBUG
            sw.Stop();
            Console.WriteLine("PrepareGlobalDictionaries: Собираем объекты {0} млс", sw.ElapsedMilliseconds);
            
#endif

            PrepareDictsFromServer(prepareMode, psList, uspdList, uaList, tiList, freeHierNodeIds,
                forecastObjectUns, formulaParents, formulaUns);

#if DEBUG
            sw.Stop();
            Console.WriteLine("PrepareGlobalDictionaries: запрос {0} млс", sw.ElapsedMilliseconds);
#endif
        }

        /// <summary>
        /// Подготовка глобальных словарей
        /// </summary>
        /// <param name="tempList"></param>
        public static void PrepareGlobalDictionaries(IEnumerable<IDHierarchy> tempList,
            EnumFreeHierarchyTreePrepareMode prepareMode = EnumFreeHierarchyTreePrepareMode.All, CancellationToken? token = null)
        {
            if (tempList == null || !tempList.Any()) return;

#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            var psList = new HashSet<int>();
            var uspdList = new HashSet<int>();//используется только по необходимости
            var uaList = new HashSet<long>();//используется только по необходимости
            var tiList = new HashSet<int>();
            var forecastObjectUns = new HashSet<string>();
            var formulaUns = new HashSet<string>();
            var formulaParents = new List<ID_TypeHierarchy>();
            var freeHierNodeIds = new HashSet<int>();
            var balanceParents = new List<ID_TypeHierarchy>();

            foreach (var t in tempList)
            {
                var id = t.ToTypeHierarchy();

                switch (t.TypeHierarchy)
                {
                    case enumTypeHierarchy.USPD:
                        uspdList.Add(t.ID);
                        break;
                    case enumTypeHierarchy.UANode:
                        uaList.Add(t.ID);
                        break;
                    case enumTypeHierarchy.Info_TI:
                        tiList.Add(t.ID);
                        break;
                    case enumTypeHierarchy.Dict_PS:
                        psList.Add(t.ID);
                        //formulaParents.Add(id);
                        //balanceParents.Add(id);
                        break;
                    case enumTypeHierarchy.ForecastObject:
                        if (!string.IsNullOrEmpty(t.StringId))
                        {
                            forecastObjectUns.Add(t.StringId);
                        }
                        break;
                    case enumTypeHierarchy.Node:
                        freeHierNodeIds.Add(t.ID);
                        //formulaParents.Add(id);
                        break;
                    //case enumTypeHierarchy.Dict_HierLev3:
                    //    formulaParents.Add(id);
                    //    balanceParents.Add(id);
                    //    break;
                    //case enumTypeHierarchy.Dict_HierLev2:
                    //    formulaParents.Add(id);
                    //    balanceParents.Add(id);
                    //    break;
                    //case enumTypeHierarchy.Dict_HierLev1:
                    //    formulaParents.Add(id);
                    //    balanceParents.Add(id);
                    //    break;
                }
            }

#if DEBUG
            sw.Stop();
            Console.WriteLine("PrepareGlobalDictionaries: Собираем объекты {0} млс", sw.ElapsedMilliseconds);

#endif
            //Отмена
            if (token.HasValue && token.Value.IsCancellationRequested)
            {
                return;
            }

            PrepareDictsFromServer(prepareMode, psList, uspdList, uaList, tiList, freeHierNodeIds,
                forecastObjectUns, formulaParents, formulaUns, token: token);

#if DEBUG
            sw.Stop();
            Console.WriteLine("PrepareGlobalDictionaries: запрос {0} млс", sw.ElapsedMilliseconds);
#endif
        }

        public static Dictionary<enumTypeHierarchy, HashSet<string>> PrepareDictsFromServer(EnumFreeHierarchyTreePrepareMode prepareMode,
            HashSet<int> psList = null, HashSet<int> uspdList = null, HashSet<long> uaList = null,
            HashSet<int> tiList = null, HashSet<int> freeHierNodeIds = null, HashSet<string> forecastObjectUns = null,
            List<ID_TypeHierarchy> formulaParents = null, HashSet<string> formulaUns = null, HashSet<int> psForPrepareUSPD = null, HashSet<int> e422List = null,
            bool isBuildResult = false, List<ID_TypeHierarchy> balanceParents = null, CancellationToken? token = null)
        {
            var preparedHierObjectFromServer = new ConcurrentStack<ID_Hierarchy>();

            var ct = token ?? CancellationToken.None;

            var po = new ParallelOptions { CancellationToken = ct };

            Parallel.Invoke(po, () =>
                {
                    if (prepareMode.HasFlag(EnumFreeHierarchyTreePrepareMode.UaOpc) && uaList != null)
                    {
#if DEBUG
                    var sw = new System.Diagnostics.Stopwatch();
                        sw.Start();
#endif
                    UAHierarchyDictionaries.UANodesDict.Prepare(uaList, Manager.UI.ShowMessage); //Подготовка всех OPC узлов вместе с дочерними

#if DEBUG
                    sw.Stop();
                        Console.WriteLine("PrepareDictsFromServer: UaOpc {0} млс", sw.ElapsedMilliseconds);
#endif
                }
                },
                   () =>
                   {
                   //Подгрузка УСПД
                   if (prepareMode.HasFlag(EnumFreeHierarchyTreePrepareMode.Uspd) && (uspdList != null || psForPrepareUSPD != null))
                       {
#if DEBUG
                       var sw = new System.Diagnostics.Stopwatch();
                           sw.Start();
#endif
                       if (psForPrepareUSPD != null)
                           {
                               FreeHierarchyDictionaries.UspdByPsDict.Prepare(psForPrepareUSPD);
                           }

                           if (uspdList != null)
                           {
                               FreeHierarchyDictionaries.USPDDict.Prepare(uspdList, Manager.UI.ShowMessage);

                           //var pss = new HashSet<int>(FreeHierarchyDictionaries.USPDDict.Values
                           //    .Where(uspd => uspd.PS_ID.HasValue && uspdList.Contains(uspd.USPD_ID))
                           //    .Select(uspd => uspd.PS_ID.Value));

                           //if (pss.Count > 0)
                           //{
                           //    EnumClientServiceDictionary.TIbyPS.Prepare(pss, Manager.UI.ShowMessage);
                           //    preparedHierObjectFromServer.PushRange(pss.Select(psId =>
                           //            new ID_Hierarchy
                           //            { ID = psId.ToString(), TypeHierarchy = enumTypeHierarchy.Dict_PS })
                           //        .ToArray());
                           //}
                       }
#if DEBUG
                       sw.Stop();
                           Console.WriteLine("PrepareDictsFromServer: УСПД {0} млс", sw.ElapsedMilliseconds);
#endif
                   }
                   },
                   () =>
                   {
                   //Подгрузка Е422
                   if (prepareMode.HasFlag(EnumFreeHierarchyTreePrepareMode.E422) && e422List != null)
                       {
#if DEBUG
                       var sw = new System.Diagnostics.Stopwatch();
                           sw.Start();
#endif
                       FreeHierarchyDictionaries.E422Dict.Prepare(e422List, Manager.UI.ShowMessage);
#if DEBUG
                       sw.Stop();
                           Console.WriteLine("PrepareDictsFromServer: Е422 {0} млс", sw.ElapsedMilliseconds);
#endif
                   }
                   },
                   () =>
                   //Подгрузка ТИ и ПС
                   {
                       if (prepareMode.HasFlag(EnumFreeHierarchyTreePrepareMode.Ti) && tiList != null && tiList.Count != 0)
                       {
                           //Подгрузка ТИ
#if DEBUG
                           var sw = new System.Diagnostics.Stopwatch();
                           sw.Start();
#endif

                           EnumClientServiceDictionary.TIHierarchyList.Prepare(tiList, Manager.UI.ShowMessage, token);
                           var pss = EnumClientServiceDictionary.TIHierarchyList.Values.Where(ti => tiList.Contains(ti.TI_ID))
                           .Select(ti => new ID_Hierarchy { ID = ti.PS_ID.ToString(), TypeHierarchy = enumTypeHierarchy.Dict_PS })
                           .ToArray();

                           if (pss.Length > 0) preparedHierObjectFromServer.PushRange(pss);

#if DEBUG
                           sw.Stop();
                           Console.WriteLine("PrepareDictsFromServer: Ti {0} млс", sw.ElapsedMilliseconds);
#endif
                       }

                       //Отмена
                       if (ct.IsCancellationRequested)
                       {
                           return;
                       }

                       if (prepareMode.HasFlag(EnumFreeHierarchyTreePrepareMode.Ps) && psList != null && psList.Count != 0)
                       {
                           //Подгрузка ПС
#if DEBUG
                           var sw = new System.Diagnostics.Stopwatch();
                           sw.Start();
#endif

                           EnumClientServiceDictionary.TIbyPS.Prepare(psList, Manager.UI.ShowMessage, token);
                           if (psList.Count > 0) preparedHierObjectFromServer.PushRange(psList.Select(id => new ID_Hierarchy { ID = id.ToString(), TypeHierarchy = enumTypeHierarchy.Dict_PS }).ToArray());

#if DEBUG
                           sw.Stop();
                           Console.WriteLine("PrepareDictsFromServer: Ps {0} млс", sw.ElapsedMilliseconds);
#endif
                       }
                   },
                       //Погрузка объектов прогнозирования
                   () =>
                   {
                       if (prepareMode.HasFlag(EnumFreeHierarchyTreePrepareMode.Forecast) && forecastObjectUns != null)
                       {
#if DEBUG
                       var sw = new System.Diagnostics.Stopwatch();
                           sw.Start();
#endif
                       EnumClientServiceDictionary.ForecastObjectsDictionary.Prepare(forecastObjectUns, Manager.UI.ShowMessage);
#if DEBUG
                       sw.Stop();
                           Console.WriteLine("PrepareDictsFromServer: объектов прогнозирования {0} млс", sw.ElapsedMilliseconds);
#endif
                   }
                   },

                   //Подгрузка формул
                   () =>
                   {
                       if (!prepareMode.HasFlag(EnumFreeHierarchyTreePrepareMode.Formulas) || (formulaParents == null && formulaUns == null)) return;

#if DEBUG
                   var sw = new System.Diagnostics.Stopwatch();
                       sw.Start();
#endif

                   if (formulaParents != null && formulaParents.Count > 0)
                       {
                           preparedHierObjectFromServer.PushRange(formulaParents.Select(f => new ID_Hierarchy { ID = f.ID.ToString(), TypeHierarchy = f.TypeHierarchy }).ToArray());
                       }

                       if (formulaUns != null && formulaUns.Count > 0)
                       {
                           var fp = EnumClientServiceDictionary.FormulasList.Values.Where(formula => formulaUns.Contains(formula.Formula_UN))
                           .Select(formula => new ID_Hierarchy { ID = formula.ParentId.ToString(), TypeHierarchy = formula.ParentType })
                           .ToArray();

                           if (fp.Length > 0) preparedHierObjectFromServer.PushRange(fp);
                       }

#if DEBUG
                   sw.Stop();
                       Console.WriteLine("PrepareDictsFromServer: Formulas {0} млс", sw.ElapsedMilliseconds);
#endif
               },

                   //Подгрузка балансов
                   () =>
                   {
                       if (!prepareMode.HasFlag(EnumFreeHierarchyTreePrepareMode.Balance) | balanceParents == null) return;

#if DEBUG
                   var sw = new System.Diagnostics.Stopwatch();
                       sw.Start();
#endif

                   EnumClientServiceDictionary.FreeHierarchyBalancesByParent.PrepareByParents(balanceParents, onException: Manager.UI.ShowMessage);

#if DEBUG
                   sw.Stop();
                       Console.WriteLine("PrepareDictsFromServer: Balances {0} млс", sw.ElapsedMilliseconds);
#endif
               },

                   //Подгрузка узлов свободной иерархии
                   () =>
                   {
                       if (prepareMode.HasFlag(EnumFreeHierarchyTreePrepareMode.Node))
                       {
#if DEBUG
                       var sw = new System.Diagnostics.Stopwatch();
                           sw.Start();
#endif
                       EnumClientServiceDictionary.FreeHierNodeDictionary.Prepare(freeHierNodeIds);
#if DEBUG
                       sw.Stop();
                           Console.WriteLine("PrepareDictsFromServer: Node {0} млс", sw.ElapsedMilliseconds);
#endif
                   }
                   });

            if (isBuildResult)
            {
                return preparedHierObjectFromServer
                    .GroupBy(g => g.TypeHierarchy)
                    .ToDictionary(k => k.Key, v => new HashSet<string>(v.Select(vv => vv.ID)));
            }

            return null;
        }

        public static IFreeHierarchyObject GetTreeItemFromWcfClass(TFreeHierarchyTreeItem source, int freeHierTreeID,
            out EnumFreeHierarchyItemType hierItemType,
            bool isUserAdmin = false)
        {
            IFreeHierarchyObject hierObject = null;
            hierItemType = EnumFreeHierarchyItemType.Error;

            switch (source.FreeHierItemType)
            {
                case EnumFreeHierarchyItemType.Node:
                    {
                        FreeHierNode node;
                        if (!EnumClientServiceDictionary.FreeHierNodeDictionary.TryGetValue(source.FreeHierItem_ID, out node) || node == null)
                        {
                            node = new FreeHierNode(source.FreeHierItem_ID, 0, freeHierTreeID, source.StringName);
                        }
                        hierObject = node;
                        hierItemType = EnumFreeHierarchyItemType.Node;

                        break;
                    }
                case EnumFreeHierarchyItemType.CommonDocuments:
                    {
                        hierItemType = EnumFreeHierarchyItemType.Node;
                        break;
                    }

                case EnumFreeHierarchyItemType.Formula:
                    {
                        hierObject =
                            EnumClientServiceDictionary.FormulasList[source.Formula_UN];
                        hierItemType = hierObject == null ? EnumFreeHierarchyItemType.Error : source.FreeHierItemType;
                        break;
                    }
                case EnumFreeHierarchyItemType.ForecastObject:
                    {
                        TForecastObject forecastObject;

                        if (!string.IsNullOrEmpty(source.ForecastObject_UN) && EnumClientServiceDictionary.ForecastObjectsDictionary.TryGetValue(source.ForecastObject_UN, out forecastObject)
                            && forecastObject != null)
                        {
                            hierObject = forecastObject;
                            hierItemType = EnumFreeHierarchyItemType.ForecastObject;
                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.OurFormula:
                    {
                        TFormulaForSection formula;
                        if (!string.IsNullOrEmpty(source.Formula_UN) && EnumClientServiceDictionary.FormulaFsk.TryGetValue(source.Formula_UN, out formula) && formula != null)
                        {
                            hierObject = formula;
                            hierItemType = source.FreeHierItemType;
                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.HierLev1:
                    {
                        THierarchyDbTreeObject hierLev1;
                        if (source.HierLev1_ID.HasValue &&
                            EnumClientServiceDictionary.HierLev1List.TryGetValue(source.HierLev1_ID.Value, out hierLev1))
                        {

                            hierObject = hierLev1;
                            hierItemType = source.FreeHierItemType;
                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.UANode:
                    {
                        if (source.UANode_ID.HasValue)
                        {
                            hierObject = UAHierarchyDictionaries.UANodesDict[source.UANode_ID.Value];
                            hierItemType = source.FreeHierItemType;

                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.HierLev2:
                    {
                        THierarchyDbTreeObject hierLev2;
                        if (source.HierLev2_ID.HasValue &&
                            EnumClientServiceDictionary.HierLev2List.TryGetValue(source.HierLev2_ID.Value, out hierLev2))
                        {
                            hierObject = hierLev2;
                            hierItemType = source.FreeHierItemType;

                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.HierLev3:
                    {
                        THierarchyDbTreeObject hierLev3;
                        if (source.HierLev3_ID.HasValue &&
                            EnumClientServiceDictionary.HierLev3List.TryGetValue(source.HierLev3_ID.Value, out hierLev3))
                        {
                            hierObject = hierLev3;
                            hierItemType = source.FreeHierItemType;

                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.PS:
                    {
                        TPSHierarchy ps;
                        if (source.PS_ID.HasValue &&
                            EnumClientServiceDictionary.DetailPSList.TryGetValue(source.PS_ID.Value, out ps))
                        {
                            hierObject = ps;
                            hierItemType = source.FreeHierItemType;
                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.Section:
                    {
                        TSection section;
                        if (source.Section_ID.HasValue &&
                            GlobalSectionsDictionary.SectionsList.TryGetValue(source.Section_ID.Value, out section))
                        {
                            hierObject = section;
                            hierItemType = source.FreeHierItemType;
                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;

                    }
                case EnumFreeHierarchyItemType.TI:
                    {
                        TInfo_TI ti;
                        if (source.TI_ID.HasValue &&
                            EnumClientServiceDictionary.TIHierarchyList.TryGetValue(source.TI_ID.Value, out ti))
                        {
                            hierObject = ti;
                            hierItemType = source.FreeHierItemType;

                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;

                    }
                case EnumFreeHierarchyItemType.TP:
                    {
                        var tps = EnumClientServiceDictionary.GetTps();
                        TPoint tp;
                        if (source.TP_ID.HasValue && tps != null &&
                            tps.TryGetValue(source.TP_ID.Value, out tp))
                        {
                            hierObject = tp;
                            hierItemType = source.FreeHierItemType;

                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;

                    }
                case EnumFreeHierarchyItemType.Contract:
                    {
                        ServiceReference.ARM_20_Service.Dict_JuridicalPersons_Contract contract;
                        if (source.JuridicalPersonContract_ID.HasValue &&
                            EnumClientServiceDictionary.ContractDict.TryGetValue(source.JuridicalPersonContract_ID.Value, out contract))
                        {
                            hierObject = contract;
                            hierItemType = source.FreeHierItemType;

                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.JuridicalPerson:
                    {
                        TJuridicalPerson person;
                        if (source.JuridicalPerson_ID.HasValue &&
                            EnumClientServiceDictionary.JuridicalPersonsDict.TryGetValue(source.JuridicalPerson_ID.Value, out person))
                        {
                            hierObject = person;
                            hierItemType = source.FreeHierItemType;

                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.USPD:
                    {
                        if (source.USPD_ID.HasValue)
                        {
                            var uspd = FreeHierarchyDictionaries.USPDDict[source.USPD_ID.Value];
                            hierObject = uspd;
                            hierItemType = source.FreeHierItemType;

                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;

                    }
                case EnumFreeHierarchyItemType.XMLSystem:
                    {
                        if (source.XMLSystem_ID.HasValue)
                        {
                            if (GlobalHierLev1HierarhyDictionary.XMLSystems.ContainsKey(source.XMLSystem_ID.Value))
                            {
                                hierItemType = source.FreeHierItemType;
                            }
                            else
                            {
                                hierItemType = EnumFreeHierarchyItemType.Error;
                            }
                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.DistributingArrangement:
                    if (source.DistributingArrangement_ID.HasValue)
                    {
                        if (GlobalTreeDictionary.DistributingArrangements.ContainsKey(source.DistributingArrangement_ID.Value))
                        {
                            hierObject = GlobalTreeDictionary.DistributingArrangements[source.DistributingArrangement_ID.Value];
                            hierItemType = source.FreeHierItemType;

                        }
                        else hierItemType = EnumFreeHierarchyItemType.Error;
                    }
                    else hierItemType = EnumFreeHierarchyItemType.Error;
                    break;
                case EnumFreeHierarchyItemType.BusSystem:
                    if (source.BusSystem_ID.HasValue)
                    {
                        if (GlobalTreeDictionary.BusSystems.ContainsKey(source.BusSystem_ID.Value))
                        {
                            hierObject = GlobalTreeDictionary.BusSystems[source.BusSystem_ID.Value];
                            hierItemType = source.FreeHierItemType;

                        }
                        else hierItemType = EnumFreeHierarchyItemType.Error;
                    }
                    else hierItemType = EnumFreeHierarchyItemType.Error;
                    break;
            }

            return hierObject;
        }

        public static IFreeHierarchyObject GetTreeItemFromWcfClass(FreeHierarchyObject source, int freeHierTreeID,
            out EnumFreeHierarchyItemType hierItemType, bool isUserAdmin = false)
        {
            IFreeHierarchyObject hierObject = null;
            hierItemType = EnumFreeHierarchyItemType.Error;

            int id;
            switch (source.FreeHierItemType)
            {
                case EnumFreeHierarchyItemType.Node:
                    {
                        FreeHierNode node;
                        if (!int.TryParse(source.Id, out id) || !EnumClientServiceDictionary.FreeHierNodeDictionary.TryGetValue(id, out node) || node == null)
                        {
                            node = new FreeHierNode(id, 0, freeHierTreeID, "< Узел в дереве >");
                        }
                        hierObject = node;
                        hierItemType = EnumFreeHierarchyItemType.Node;

                        break;
                    }
                case EnumFreeHierarchyItemType.CommonDocuments:
                    {
                        hierItemType = EnumFreeHierarchyItemType.Node;
                        break;
                    }

                case EnumFreeHierarchyItemType.Formula:
                    {
                        hierObject =
                            EnumClientServiceDictionary.FormulasList[source.Id];
                        hierItemType = hierObject == null ? EnumFreeHierarchyItemType.Error : source.FreeHierItemType;
                        break;
                    }
                case EnumFreeHierarchyItemType.ForecastObject:
                    {
                        TForecastObject forecastObject;

                        if (!string.IsNullOrEmpty(source.Id) && EnumClientServiceDictionary.ForecastObjectsDictionary.TryGetValue(source.Id, out forecastObject)
                            && forecastObject != null)
                        {
                            hierObject = forecastObject;
                            hierItemType = EnumFreeHierarchyItemType.ForecastObject;
                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.OurFormula:
                    {
                        TFormulaForSection formula;
                        if (!string.IsNullOrEmpty(source.Id) && EnumClientServiceDictionary.FormulaFsk.TryGetValue(source.Id, out formula) && formula != null)
                        {
                            hierObject = formula;
                            hierItemType = source.FreeHierItemType;
                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.HierLev1:
                    {
                        THierarchyDbTreeObject hierLev1;
                        if (int.TryParse(source.Id, out id) && EnumClientServiceDictionary.HierLev1List.TryGetValue(id, out hierLev1))
                        {

                            hierObject = hierLev1;
                            hierItemType = source.FreeHierItemType;
                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.UANode:
                    {
                        long lid;
                        if (long.TryParse(source.Id, out lid))
                        {
                            hierObject = UAHierarchyDictionaries.UANodesDict[lid];
                            hierItemType = source.FreeHierItemType;

                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.HierLev2:
                    {
                        THierarchyDbTreeObject hierLev2;
                        if (int.TryParse(source.Id, out id) &&
                            EnumClientServiceDictionary.HierLev2List.TryGetValue(id, out hierLev2))
                        {
                            hierObject = hierLev2;
                            hierItemType = source.FreeHierItemType;

                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.HierLev3:
                    {
                        THierarchyDbTreeObject hierLev3;
                        if (int.TryParse(source.Id, out id) &&
                            EnumClientServiceDictionary.HierLev3List.TryGetValue(id, out hierLev3))
                        {
                            hierObject = hierLev3;
                            hierItemType = source.FreeHierItemType;

                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.PS:
                    {
                        TPSHierarchy ps;
                        if (int.TryParse(source.Id, out id) &&
                            EnumClientServiceDictionary.DetailPSList.TryGetValue(id, out ps))
                        {
                            hierObject = ps;
                            hierItemType = source.FreeHierItemType;
                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.Section:
                    {
                        TSection section;
                        if (int.TryParse(source.Id, out id) &&
                            GlobalSectionsDictionary.SectionsList.TryGetValue(id, out section))
                        {
                            hierObject = section;
                            hierItemType = source.FreeHierItemType;
                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;

                    }
                case EnumFreeHierarchyItemType.TI:
                    {
                        TInfo_TI ti;
                        if (int.TryParse(source.Id, out id) &&
                            EnumClientServiceDictionary.TIHierarchyList.TryGetValue(id, out ti))
                        {
                            hierObject = ti;
                            hierItemType = source.FreeHierItemType;

                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;

                    }
                case EnumFreeHierarchyItemType.TP:
                    {
                        var tps = EnumClientServiceDictionary.GetTps();
                        TPoint tp;
                        if (int.TryParse(source.Id, out id) && tps != null &&
                            tps.TryGetValue(id, out tp))
                        {
                            hierObject = tp;
                            hierItemType = source.FreeHierItemType;

                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;

                    }
                case EnumFreeHierarchyItemType.Contract:
                    {
                        ServiceReference.ARM_20_Service.Dict_JuridicalPersons_Contract contract;
                        if (int.TryParse(source.Id, out id) &&
                            EnumClientServiceDictionary.ContractDict.TryGetValue(id, out contract))
                        {
                            hierObject = contract;
                            hierItemType = source.FreeHierItemType;

                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.JuridicalPerson:
                    {
                        TJuridicalPerson person;
                        if (int.TryParse(source.Id, out id) &&
                            EnumClientServiceDictionary.JuridicalPersonsDict.TryGetValue(id, out person))
                        {
                            hierObject = person;
                            hierItemType = source.FreeHierItemType;

                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.USPD:
                    {
                        if (int.TryParse(source.Id, out id))
                        {
                            var uspd = FreeHierarchyDictionaries.USPDDict[id];
                            hierObject = uspd;
                            hierItemType = source.FreeHierItemType;
                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;

                    }
                case EnumFreeHierarchyItemType.XMLSystem:
                    {
                        byte bid;
                        if (byte.TryParse(source.Id, out bid))
                        {
                            Expl_XML_System_ID_List expl;
                            if (GlobalHierLev1HierarhyDictionary.XMLSystems.TryGetValue(bid, out expl))
                            {
                                hierObject = expl;
                                hierItemType = source.FreeHierItemType;
                            }
                            else
                            {
                                hierItemType = EnumFreeHierarchyItemType.Error;
                            }
                        }
                        else
                        {
                            hierItemType = EnumFreeHierarchyItemType.Error;
                        }
                        break;
                    }
                case EnumFreeHierarchyItemType.DistributingArrangement:
                    if (int.TryParse(source.Id, out id))
                    {
                        Dict_DistributingArrangement distributingArrangement;
                        if (GlobalTreeDictionary.DistributingArrangements.TryGetValue(id, out distributingArrangement))
                        {
                            hierObject = distributingArrangement;
                            hierItemType = source.FreeHierItemType;

                        }
                        else hierItemType = EnumFreeHierarchyItemType.Error;
                    }
                    else hierItemType = EnumFreeHierarchyItemType.Error;
                    break;
                case EnumFreeHierarchyItemType.BusSystem:
                    if (int.TryParse(source.Id, out id))
                    {
                        Dict_BusSystem busSystem;
                        if (GlobalTreeDictionary.BusSystems.TryGetValue(id, out busSystem))
                        {
                            hierObject = busSystem;
                            hierItemType = source.FreeHierItemType;

                        }
                        else hierItemType = EnumFreeHierarchyItemType.Error;
                    }
                    else hierItemType = EnumFreeHierarchyItemType.Error;
                    break;
            }

            return hierObject;
        }
    }
}
