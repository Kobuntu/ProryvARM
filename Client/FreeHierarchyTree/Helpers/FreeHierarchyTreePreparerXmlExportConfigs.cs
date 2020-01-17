using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.DeclaratorService;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.ServiceReference.UAService;

namespace Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy
{
    public static partial class GlobalFreeHierarchyDictionary
    {
        /// <summary>
        /// Генерация дерева (списка) конфигурации XML
        /// </summary>
        private static ConcurrentDictionary<int, FreeHierarchyTreeItem> GenerateExplXmlExportConfigsTree(FreeHierarchyTreeDescriptor descriptor)
        {
            var result = new ConcurrentDictionary<int, FreeHierarchyTreeItem>();

            var rootNodes = DeclaratorService.Service.GetExplXmlExportConfigs();

            if (rootNodes == null) return result;

            foreach (var rootNode in rootNodes)
            {
                var newItemNode = new FreeHierarchyTreeItem(descriptor, rootNode, false, string.Empty,
                    descriptor.GetMinIdAndDecrement(), null, true, nodeRights: null,
                    freeHierItemType: EnumFreeHierarchyItemType.FiasFullAddress);
                //{
                //    StringName = rootNode.StringName,
                //};

                newItemNode.Descriptor.Tree = result;

                //newItemNode.UpdateCountParents();

                result[newItemNode.FreeHierItem_ID] = newItemNode;
            }

            return result;
        }

        /// <summary>
        /// Генерация дерева (списка) балансов (приложения 63) для выгрузки xml
        /// </summary>
        private static ConcurrentDictionary<int, FreeHierarchyTreeItem> GenerateBalance63Tree(FreeHierarchyTreeDescriptor descriptor)
        {
            var result = new ConcurrentDictionary<int, FreeHierarchyTreeItem>();

            var rootNodes = DeclaratorService.Service.Get_ExportService_Balances63();

            if (rootNodes == null) return result;

            foreach (var rootNode in rootNodes)
            {
                var newItemNode = new FreeHierarchyTreeItem(descriptor, rootNode, false, string.Empty,
                    descriptor.GetMinIdAndDecrement(), null, true, nodeRights: null,
                    freeHierItemType: EnumFreeHierarchyItemType.FiasFullAddress);
                //{
                //    StringName = rootNode.StringName,
                //};

                newItemNode.Descriptor.Tree = result;

                //newItemNode.UpdateCountParents();

                result[newItemNode.FreeHierItem_ID] = newItemNode;
            }

            return result;
        }

       
        private static ConcurrentDictionary<int, FreeHierarchyTreeItem> GenerateOldTelescopeTree(FreeHierarchyTreeDescriptor descriptor)
        {
            var result = new ConcurrentDictionary<int, FreeHierarchyTreeItem>();
            
            var rootNodes = EnumClientServiceDictionary.OldTelescopeTreeNodes.Values.Where(i => i.TreeLevel == 0);

            if (rootNodes == null) return result;

            foreach (var rootNode in rootNodes)
            {
                rootNode.Parent_Absolute_Number = ""; //иначе зацикливание где нибудь будет

                var newItemNode = new FreeHierarchyTreeItem(descriptor, rootNode, false, string.Empty,
                    descriptor.GetMinIdAndDecrement(), null, true, nodeRights: null,
                    freeHierItemType: EnumFreeHierarchyItemType.OldTelescopeTreeNode);
              
                newItemNode.Descriptor.Tree = result;

                result[newItemNode.FreeHierItem_ID] = newItemNode;
            }

            return result;
        }

        

    }
}
