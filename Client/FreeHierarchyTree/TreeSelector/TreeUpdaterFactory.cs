using Proryv.AskueARM2.Client.ServiceReference.DeclaratorService;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.Common;

namespace Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree.TreeSelector
{
    /// <summary>
    /// Основные ф-ии работы с деревом
    /// </summary>
    public static class TreeUpdaterFactory
    {
        /// <summary>
        /// Получаем коллекцию объектов для вставки/обновления ветки
        /// </summary>
        /// <param name="destTreeId">Идентификатор дерева, куда вставляем</param>
        /// <param name="destFreeHierItemId">Идентификатор узла, на который добавляем</param>
        /// <param name="sourceTree">Дерево, от куда добавляем</param>
        /// <param name="includeChildren"></param>
        /// <returns></returns>
        public static List<FreeHierTreeItem> FindSelectedAndBuildForInsert(int destTreeId, int destFreeHierItemId, FreeHierarchyTree sourceTree, bool includeChildren)
        {
            var descriptor = sourceTree.GetDescriptor();
            if (descriptor == null) return null;

            var selectedParent = new List<FreeHierarchyTreeItem>();
            descriptor.GetSelected(selectedParent, aditionalWherePredicate: i =>
            {
                //Выбранные, но без родителя, или родитель не выбран, т.е. эти объекты будут непосредственно на самом узле, куда их всех необходимо добавить,
                //дочерние выбранные пойдут под ними, их не выбираем
                return i.Parent == null || !i.Parent.IsSelected;
            });

            var nodes = new List<FreeHierTreeItem>();

            foreach (var parent in selectedParent)
            {
                BuildParent(destTreeId, destFreeHierItemId, parent, nodes, includeChildren, "");
            }

            return nodes;
        }

        /// <summary>
        /// Строим ветки, которые будем добавлять
        /// </summary>
        /// <param name="parent">Родительский объект</param>
        /// <param name="nodes">Список узлов для вставки/обновления который наполняем</param>
        /// <param name="nodePath">Дополнительный путь к узлу, в который вставляем или переносим (что-то вроде 1/1/1/)</param>
        private static void BuildParent(int destTreeId, int destFreeHierItemId, FreeHierarchyTreeItem parent, List<FreeHierTreeItem> nodes,
            bool includeChildren, string nodePath)
        {
            //Добавляем родителя
            nodes.Add(CreateInserted(destTreeId, destFreeHierItemId, parent, includeChildren, nodePath));

            //Теперь добавляем дочерние
            if (parent.Children == null || parent.Children.Count == 0) return;

            var ph = 1;

            foreach (var child in parent.Children)
            {
                if (!child.IsSelected) continue;

                BuildParent(destTreeId, destFreeHierItemId, child, nodes, includeChildren, nodePath + ph + "/"); //Увеличиваем путь

                ph++;
            }
        }

        internal static FreeHierTreeItem CreateInserted(int destTreeId, int destFreeHierItemId, FreeHierarchyTreeItem item, bool includeChildren, string nodePath)
        {
            var node = new FreeHierTreeItem
            {
                FreeHierTree_ID = destTreeId,
                FreeHierItem_ID = item.FreeHierItem_ID,
                FreeHierIcon_ID = item.NodeIcon_ID,
                ParentFreeHierItem_ID = destFreeHierItemId,
                FreeHierItemType = (byte)item.FreeHierItemType,
                NodePath = nodePath //Нужно для сохранения структуры переносимых объектов, строго соблюдать не обязательно
            };

            if (item.HierObject != null)
            {
                node.StringName = item.HierObject.Name;
                if (item.HierObject.Id > 0 && !HierarchyObjectHelper.IsStringId(item.FreeHierItemType))
                {
                    node.ObjectStringID = item.HierObject.Id.ToString();
                }
                else
                {
                    node.ObjectStringID = item.HierObject.StringId;
                }
            }
            else
            {
                node.StringName = item.StringName;
            }

            if (item.FreeHierItemType != EnumFreeHierarchyItemType.XMLSystem)
            {
                node.IncludeObjectChildren = includeChildren;
            }

            return node;
        }
    }
}
