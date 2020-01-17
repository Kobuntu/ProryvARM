using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree
{
    /// <summary>
    /// Вспомогательный клас для результата прогрузки дерева
    /// </summary>
    public class TreeStartLoader : IDisposable
    {
        public FreeHierarchyTreeDescriptor Descriptor;
        public IDictionary<int, FreeHierarchyTreeItem> FreeHierarchyTreeItems;
        public FreeHierarchyTypeTreeItem FreeHierarchyTypeTreeItem;
        public bool IsFirstLoaded;
        public void Dispose()
        {
            Descriptor = null;
            FreeHierarchyTreeItems = null;
            FreeHierarchyTypeTreeItem = null;
        }

        /// <summary>
        /// Подгрузчик дерева
        /// </summary>
        /// <param name="freeHierarchyTypeTreeItem"></param>
        /// <param name="isFullReload"></param>
        public TreeStartLoader(FreeHierarchyTree tree, FreeHierarchyTypeTreeItem freeHierarchyTypeTreeItem, HashSet<long> selectedNodes,
            bool isFullReload = false, HashSet<int> singreRootFreeHierItemIds = null)
        {
            if (freeHierarchyTypeTreeItem == null || tree == null) return;

            var d = new FreeHierarchyTreeDescriptor
            {
                //TreeID = freeHierarchyTypeTreeItem.FreeHierTree_ID,
                FilterStatus = tree.FilterStatus,
                TreeHashId = GetHashCode(),
                IsSelectSingle = tree.IsSelectSingle,
                ShowUspdAndE422InTree = tree.ShowUspdAndE422InTree,
                IsHideSelectMany = tree.IsHideSelectMany,
                NeedFindTransformatorsAndreactors = tree.IsShowTransformatorsAndReactors,
                NeedFindTI = !tree.IsHideTi && (freeHierarchyTypeTreeItem.FreeHierTree_ID >= GlobalFreeHierarchyDictionary.TreeTypeStandartPS
                                          || freeHierarchyTypeTreeItem.FreeHierTree_ID == GlobalFreeHierarchyDictionary.TreeTypeStandartTIFormula
                                          || freeHierarchyTypeTreeItem.FreeHierTree_ID == GlobalFreeHierarchyDictionary.TreeTypeStandartDistributingArrangementAndBusSystem),
                IsHideTp = tree.IsHideTp,
                PermissibleForSelectObjects = tree.PermissibleForSelectObjects,
                FreeHierarchyTree = tree,
            };

            try
            {

                Descriptor = d;
                FreeHierarchyTypeTreeItem = freeHierarchyTypeTreeItem;
                FreeHierarchyTreeItems = GlobalFreeHierarchyDictionary.GetTree(d, out IsFirstLoaded, tree.IsHideTi, selectedNodes, isFullReload, singreRootFreeHierItemIds);
            }
            catch (Exception ex)
            {
                Manager.UI.ShowMessage(ex.Message);
            }
        }
    }
}
