using Infragistics.Controls.Menus;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.ServiceReference.UAService;
using Proryv.AskueARM2.Client.Visual;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Proryv.AskueARM2.Both.VisualCompHelpers.Interfaces;
using Proryv.AskueARM2.Both.VisualCompHelpers.ValuesToXceedGrid;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Proryv.AskueARM2.Client.ServiceReference.Service.Interfaces;
using Proryv.AskueARM2.Client.Visual.Common.Common;
using Xceed.Wpf.DataGrid;
using Proryv.AskueARM2.Client.ServiceReference.GroupTpFactory;

namespace Proryv.ElectroARM.Controls.Controls.Popup.Finder
{
    public static class XamTreeFinder
    {
        /// <summary>
        /// Поиск и раскрытие объекта в дереве
        /// </summary>
        /// <param name="founded"></param>
        /// <param name="selItem"></param>
        /// <param name="xamTree"></param>
        /// <param name="isExpandLast"></param>
        /// <param name="isSelect"></param>
        public static void ExpandAndSelectXamTree(Dictionary<object, Stack> founded, object selItem,
            XamDataTree xamTree, bool isExpandLast = false, bool isSelect = true)
        {
            if (founded == null)
            {
                return;
            }

            Stack stack;
            if (!founded.TryGetValue(selItem, out stack)) return;

            ExpandAndSelectXamTree(stack, selItem, xamTree, isExpandLast, isSelect);
        }


        public static void ExpandAndSelectXamTree(Stack stack, object selItem,
            XamDataTree xamTree, bool isExpandLast = false, bool isSelect = true)
        {
            if (stack != null)
            {
                var hierItem = selItem as FreeHierarchyTypeTreeItem;
                if (hierItem != null)
                {
                    var stackTree = new Stack<FreeHierarchyTypeTreeItem>();

                    for (int i = stack.Count - 1; i >= 0; i--)
                    {
                        stackTree.Push(stack.ToArray()[i] as FreeHierarchyTypeTreeItem);
                    }

                    xamTree.ExpandAndSelectXamTree(stackTree, isExpandLast, isSelect);
                }
                else
                {
                    var arr = new object[stack.Count];
                    stack.CopyTo(arr, 0);
                    Array.Reverse(arr);

                    var chain = new ConcurrentStack<object>(arr);
                    xamTree.ExpandAndSelectXamTreeAsync(chain, isExpandLast);
                }
                //MoveUp(xamTree);
            }
            // Если элемент не найден в дереве, то нужно подгрузить ветку
            else
            {
                var findedNodeResult = selItem as UaFindNodeResult;
                if (findedNodeResult != null)
                {
                    ExpandAndSelectUaNodeXamTree(findedNodeResult, xamTree);
                    //MoveUp(xamTree);
                }
                else
                {
                    ExpandAndSelectHierObject(selItem, xamTree);
                }
            }
        }

        /// <summary>
        /// Запрашиваем построение пути на сервер
        /// </summary>
        /// <param name="hierarchyObject">Объект иерархии</param>
        /// <param name="treeId">Идентификатор дерева</param>
        /// <returns></returns>
        public static Dictionary<string, List<List<ID_TypeHierarchy>>> BuildPathFromSQL(List<ID_TypeHierarchy> ids, int? treeId)
        {
            if (ids == null) return null;

            try
            {
                var stringPathes = ClientSideMultithreadBuilder
                        .BuildandWaitResult<List<string>, TreePathParams>(new TreePathParams
                        {
                            Ids = ids,
                            UserId = Manager.User.User_ID,
                            TreeID = treeId,
                        }, "BuildTreePath", null);

                if (stringPathes!=null)
                {
                    return stringPathes
                        .Select(sp => BuildPath(sp))
                        .GroupBy(sp=>sp.Last().ToRootPath)
                        .ToDictionary(k=>k.Key, v=>v.ToList());
                }

                //Строим путь начиная от родителя
                //return ARM_Service.TREE_BuildTreePath(ids, Manager.User.User_ID, treeId);
            }
            catch (Exception ex)
            {
                ex.ShowMessage();
                //Manager.UI.ShowMessage("Ошибка построения пути узла: " + ex.Message);
            }

            return null;
        }

        private static List<ID_TypeHierarchy> BuildPath(string toParentFreeHierPath)
        {
            if (string.IsNullOrEmpty(toParentFreeHierPath)) return null;

            return toParentFreeHierPath.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s =>
            {
                var splited = s.Split(',');
                if (splited.Length != 3) return null;

                var stringId = splited[0];
                int parentId;
                var isIntId = int.TryParse(stringId, out parentId) && parentId >= 0;

                byte typeHierarchyByte;
                if (!byte.TryParse(splited[1], out typeHierarchyByte)) typeHierarchyByte = 255;

                int? freeHierItemId;
                int fd;

                if (int.TryParse(splited[2], out fd))
                {
                    freeHierItemId = fd;
                }
                else
                {
                    freeHierItemId = null;
                }

                return new ID_TypeHierarchy
                {
                    ID = parentId,
                    TypeHierarchy = (enumTypeHierarchy)typeHierarchyByte,
                    FreeHierItemId = freeHierItemId,
                    StringId = isIntId ? null : stringId,
                    ToRootPath = s,
                };
            }).ToList();
        }

        public static XamDataTreeNode ExpandAndSelectHierObject(object hierarchyObject, XamDataTree xamTree)
        {
            var pathToFounded = new ConcurrentStack<object>();
#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif
            var source = xamTree.ItemsSource as IEnumerable<object>;
            if (source == null) return null;

            var foundedObject = FinderHelper.FindFirstElementAsync(source, hierarchyObject, pathToFounded);
#if DEBUG
            sw.Stop();
            Console.WriteLine("Поиск {0} млс", sw.ElapsedMilliseconds);
#endif
            if (foundedObject == null || pathToFounded.Count <= 0) return null;
            return xamTree.ExpandAndSelectXamTreeSync(pathToFounded, true);
        }

        private static void ExpandAndSelectUaNodeXamTree(UaFindNodeResult findedNodeResult, XamDataTree xamTree)
        {
            if (!findedNodeResult.FdUaNodeId.HasValue) return;

            //Родитель через который идет переход на обычные узлы FreeHierarchy
            var parentNode = UAHierarchyDictionaries.UANodesDict[findedNodeResult.FdUaNodeId.Value];
            if (parentNode == null) return;

            //Непосредственный родитель 
            var pathToFounded = new ConcurrentStack<object>();
#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif
            var foundedObject = FinderHelper.FindFirstElementAsync(xamTree.ItemsSource as IEnumerable<object>, parentNode, pathToFounded);
#if DEBUG
            sw.Stop();
            Console.WriteLine("Поиск 1 {0} млс", sw.ElapsedMilliseconds);
#endif
            var treeItem = foundedObject as FreeHierarchyTreeItem;
            if (treeItem != null && treeItem.Descriptor != null)
            {
                //Подгружаем объекты
                treeItem.ReloadUaNodeBranch(new Queue<long>(findedNodeResult.ParentIds.Skip(1)));
            }
            pathToFounded.Clear();

#if DEBUG
            sw.Restart();
#endif
            foundedObject = FinderHelper.FindFirstElementAsync(xamTree.ItemsSource as IEnumerable<object>, findedNodeResult.Node, pathToFounded);
#if DEBUG
            sw.Stop();
            Console.WriteLine("Поиск 2 {0} млс", sw.ElapsedMilliseconds);
#endif
            //На данный момент объект подгружен, просто позиционируем на него
            if (foundedObject != null && pathToFounded.Count > 0)
            {
                xamTree.ExpandAndSelectXamTreeAsync(pathToFounded, false);
            }
        }

        public static void ExpandAndSelectXamTree(this XamDataTree tree, Stack<FreeHierarchyTypeTreeItem> chain, bool isExpandLast
            , bool isSelect = true)
        {
            tree.SelectionSettings.SelectedNodes.Clear();
            Action<XamDataTreeNodesCollection> expandAndSelect = null;
            expandAndSelect = delegate (XamDataTreeNodesCollection list)
            {
                var item = chain.Pop();
                var find = list.FirstOrDefault(n => (n.Data as FreeHierarchyTypeTreeItem).FreeHierTree_ID == item.FreeHierTree_ID);
                if (find == null)
                {
                    return;
                }
                if (chain.Count == 0)
                {
                    if (isExpandLast) find.IsExpanded = true;
                    if (isSelect)
                    {
                        find.IsSelected = true;
                    }
                    tree.SelectionSettings.SelectedNodes.Add(find);
                    tree.ScrollNodeIntoView(find);
                    //tree.BringIntoView();
                }
                else
                {
                    find.IsExpanded = true;
                    expandAndSelect(find.Nodes);
                }
            };
            expandAndSelect(tree.Nodes);
        }

        /// <summary>
        /// Разворачиваем объект в дереве с подгрузкой узлов
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="chain"></param>
        /// <param name="isExpandLast"></param>
        /// <param name="isSelect"></param>
        /// <param name="afterFound">Действие после нахождения нужного элемента</param>
        public static void ExpandAndSelectXamTreeAsync(this XamDataTree tree, ConcurrentStack<object> chain, bool isExpandLast
            , bool isSelect = true, Action<XamDataTreeNode> afterFound = null)
        {
            tree.SelectionSettings.SelectedNodes.Clear();
            Action<XamDataTreeNodesCollection> expandAndSelect = null;
            //var isFirst = true;

            //CancellationTokenSource tokenSource = null;

            expandAndSelect = delegate (XamDataTreeNodesCollection list)
            {
                object item;
                if (!chain.TryPop(out item))
                {
                    //if (tokenSource != null) tokenSource.Cancel();
                    //WaitPanel.Hide<object>(tree, null, null);

                    return;
                }

                var find = list.FirstOrDefault(n => n.Data.Equals(item));
                if (find == null)
                {
                    //if (tokenSource != null) tokenSource.Cancel();
                    //WaitPanel.Hide<object>(tree, null, null);

                    return;
                }

                if (chain.Count == 0)
                {
                    try
                    {
                        //if (isSelect)
                        {
                            tree.SelectionSettings.SelectedNodes.Add(find);
                            find.IsSelected = true;
                        }

                        if (isExpandLast)
                        {
                            find.IsExpanded = true;
                        }

                        tree.ScrollNodeIntoView(find);


                        FindBar.MoveSelectedNodeIntoCenter(tree);
                    }
                    finally
                    {
                        if (afterFound != null) afterFound(find);
                        //if (tokenSource != null) tokenSource.Cancel();
                        //WaitPanel.Hide<object>(tree, null, null);
                    }
                    //}), DispatcherPriority.Send);

                    //tree.BringIntoView();
                }
                else
                {
                    find.IsExpanded = true;
                    expandAndSelect(find.Nodes);
                    

                    //tree.Dispatcher.BeginInvoke((Action)(() =>
                    //{
                    //    expandAndSelect(find.Nodes);
                    //}), DispatcherPriority.Send);
                }
            };

            expandAndSelect(tree.Nodes);
        }
    }
}
