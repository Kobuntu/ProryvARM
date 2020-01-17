using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;

namespace Proryv.ElectroARM.Controls.Controls.Popup.Finder
{
    public static class FinderHelper
    {
        public static bool ScanGroup(Stack<CollectionViewGroup> chain, ReadOnlyObservableCollection<object> Items, object findObj)
        {
            bool found = false;
            foreach (var gr in Items)
            {
                var cvg = gr as CollectionViewGroup;
                if (cvg != null)
                {
                    chain.Push(cvg);
                    if (ScanGroup(chain, cvg.Items, findObj))
                    {
                        found = true;
                        break;
                    }
                    chain.Pop();
                }
                else if (gr == findObj)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

        public static IFindableItemWithPath FindFirstElementAsync(IEnumerable<object> items, object findObject, ConcurrentStack<object> pathToFounded)
        {
            IFindableItemWithPath result = null;
            object syncLock = new object();

            Parallel.ForEach(items.AsParallel(),
            (item, loopState) =>
            {
                var lp = new Stack();
                lp.Push(item);

                object i = null;

                var fi = item as IFindableItemWithPath;
                if (fi == null) return;

                i = fi.GetItemForSearch();

                if (i == null) i = item;

                if (Equals(i, findObject))
                {
                    lock (syncLock)
                    {
                        result = fi;
                        pathToFounded.PushRange(lp.ToArray());
                    }

                    loopState.Break();
                    //break;
                }

                var children = fi.GetChildren();
                if (children != null)
                {
                    var child = FindFirstElement(children, findObject, lp);
                    if (child != null)
                    {
                        lock (syncLock)
                        {
                            result = child;
                            if (lp.Count > 0)
                            {
                                pathToFounded.PushRange(lp.ToArray());
                            }
                        }

                        loopState.Break();
                        //break;
                    }
                }

                lp.Pop();
            });

            return result;
        }

        public static IFindableItemWithPath FindFirstElement(IEnumerable items, object findObject, Stack pathToFounded)
        {
            foreach (var item in items)
            {
                pathToFounded.Push(item);

                object i = null;
                var fi = item as IFindableItemWithPath;
                if (fi == null) continue;

                i = fi.GetItemForSearch();

                if (i == null) i = item;

                if (Equals(i, findObject))
                {
                    return fi;
                }
                
                var children = fi.GetChildren();
                if (children != null)
                {
                    var child = FindFirstElement(children, findObject, pathToFounded);
                    if (child != null) return child;
                }

                pathToFounded.Pop();
            }

            return null;
        }
    }
}
