using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Xceed.Wpf.DataGrid;

namespace Proryv.ElectroARM.Controls.Controls.Popup.Finder
{
    /// <summary>
    /// Поиск по XceedGrid
    /// </summary>
    public static class XceedGridFinder
    {
        /// <summary>
        /// Поиск и раскрытие объекта в таблице XceedGrid
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="founded"></param>
        /// <param name="selItem"></param>
        public static void ExpandandSelectXceedGrid(DataGridControl grid, Dictionary<object, Stack> founded, object selItem)
        {
            //Отключены детали в гриде разворачивать нельзя
            if (selItem == null || !grid.AllowDetailToggle) return;

            Stack stack;
            if (!founded.TryGetValue(selItem, out stack) || stack == null)
            {
                //Пытаемся найти по ключу
                IKey iKey = selItem as IKey;
                if (iKey != null)
                {
                    foreach (var fPair in founded)
                    {
                        IKey ifkey = fPair.Key as IKey;
                        if (ifkey != null && iKey.GetKey == ifkey.GetKey)
                        {
                            stack = fPair.Value;
                            break;
                        }
                    }
                }
            }

            DataGridControl.GetDataGridContext(grid).ClearAllSelection();

            if (stack != null)
            {
                var chain = stack.Clone() as Stack;
                grid.ExpandandSelectXceedGrid(chain);
            }
            else
            {
                //Это скорее всего подгружаемый динамически объект
                var hierObject = selItem as IFreeHierarchyObject;
                if (hierObject != null)
                {
                    ExpandParentAndSelectObjectXceedGrid(hierObject.TypeParentHierarchy, hierObject.ParentId, hierObject, grid);
                }
            }
        }

        private static void ExpandParentAndSelectObjectXceedGrid(enumTypeHierarchy parentType, int parentId,
            IFreeHierarchyObject hierarchyObject, DataGridControl xamTree)
        {
            IFreeHierarchyObject parent = null;
            switch (parentType)
            {
                case enumTypeHierarchy.Dict_PS:
                case enumTypeHierarchy.USPD:
                case enumTypeHierarchy.E422:
                    parent = EnumClientServiceDictionary.DetailPSList[parentId];
                    break;
                case enumTypeHierarchy.Info_TP:
                    var tps = EnumClientServiceDictionary.GetTps();
                    if (tps != null)
                    {
                        parent = tps[parentId];
                    }
                    break;
            }

            if (parent == null) return;

            var chain = new Stack();
#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif
            var source = xamTree.ItemsSource as ICollection;
            if (source == null) return;

            FinderHelper.FindFirstElement(source, parent, chain);
#if DEBUG
            sw.Stop();
            Console.WriteLine("Поиск {0} млс", sw.ElapsedMilliseconds);
#endif
            if (chain.Count <= 0) return;

            var arr = new object[chain.Count];
            chain.CopyTo(arr, 0);
            //Array.Reverse(arr);

            chain = new Stack(arr);

            xamTree.ExpandandSelectXceedGrid(chain, hierarchyObject);
        }

        public static void ExpandandSelectXceedGrid(this DataGridControl grid, Stack chain, IFreeHierarchyObject hierarchyObject = null)
        {
            if (chain == null) return;

            Func<DataGridContext, object, bool> expand = null;
            expand = (context, obj) =>
            {
                if (chain.Count > 0)
                {
                    try
                    {
                        if (!context.AllowDetailToggle || !context.HasDetails || !context.ExpandDetails(obj)) return false;
                    }
                    catch (Exception)
                    {
                    }

                    var nextObj = chain.Pop();
                    var childrenContext = context.GetChildContexts();
                    //var container = context.GetContainerFromItem(nextObj) as DataGridContext;
                    //if (container != null) expand(container, nextObj);
                    foreach (var c in childrenContext)
                    {
                        try
                        {
                            grid.Dispatcher.BeginInvoke(expand, DispatcherPriority.ApplicationIdle, c, nextObj);
                            //if (expand(c, nextObj))  break;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else
                {
                    if (hierarchyObject == null)
                    {
                        Keyboard.Focus(grid);
                        try
                        {
                            context.CurrentItem = obj;
                        }
                        catch { }
                        context.SelectedItems.Clear();
                        context.SelectedItems.Add(obj);
                        MoveCenter(grid, obj, context);
                    }
                    else
                    {
                        var findable = obj as IFindableItemWithPath;
                        if (findable == null) return true;

                        findable.LoadDynamicChildren();

                        try
                        {
                            if (!context.AllowDetailToggle || !context.HasDetails || !context.ExpandDetails(obj)) return false;
                        }
                        catch (Exception)
                        {
                        }

                        grid.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            var children = findable.GetChildren();
                            if (children != null)
                            {
                                Keyboard.Focus(grid);
                                foreach (var item in children)
                                {
                                    var fi = item as IFindableItemWithPath;
                                    if (fi == null) continue;

                                    if (Equals(fi.GetItemForSearch(), hierarchyObject))
                                    {
                                        foreach (var c in context.GetChildContexts())
                                        {
                                            try
                                            {
                                                c.CurrentItem = fi;
                                                c.SelectedItems.Clear();
                                                c.SelectedItems.Add(fi);
                                            }
                                            catch (Exception)
                                            {
                                            }

                                            

                                            MoveCenter(grid, fi, c);
                                        }

                                        break;
                                    }
                                }
                            }

                        }), DispatcherPriority.Input);
                    }
                }

                return true;
            };

            expand(DataGridControl.GetDataGridContext(grid), chain.Pop());

            //if (chain.Count == 1)
            //{
            //    var obj = chain.Pop();
            //    if (grid.Items.Groups != null)
            //    {
            //        var chainGroup = new Stack<CollectionViewGroup>();
            //        foreach (CollectionViewGroup gr in grid.Items.Groups)
            //        {
            //            chainGroup.Push(gr);
            //            if (FinderHelper.ScanGroup(chainGroup, gr.Items, obj)) break;
            //            chainGroup.Pop();
            //        }
            //        if (chainGroup.Count > 0)
            //        {
            //            var copy = chainGroup.Reverse().ToArray();
            //            foreach (var gr in copy)
            //            {
            //                grid.ExpandGroup(gr);
            //            }
            //        }
            //    }
            //    grid.CurrentItem = obj;
            //    grid.SelectedItems.Clear();
            //    grid.SelectedItems.Add(obj);
            //}
            //else expand(DataGridControl.GetDataGridContext(grid), chain.Pop());
        }

        private static void MoveCenter(DataGridControl grid, object item, DataGridContext context)
        {
            grid.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                //moveUp(grid, context.GetContainerFromItem(item) as FrameworkElement);
                var cnt = context.GetContainerFromItem(item) as FrameworkElement;
                if (cnt == null) return;

                var point = cnt.TranslatePoint(new Point(0, 0), grid);
                var sv = grid.FindLogicalChild("PART_ScrollViewer") as ScrollViewer;
                if (sv == null) return;

                sv.ScrollToVerticalOffset(sv.VerticalOffset + (point.Y - 70) / sv.ViewportHeight);
                cnt.BringIntoView();

            }), DispatcherPriority.ApplicationIdle);
        }
    }
}
