using System;
using System.Collections.Generic;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using System.Collections;
using System.ComponentModel;

namespace Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy
{
    public class FreeHierarchyTypeTreeItem : IFindableItem, IKey, IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// Идентификатор родителя для построения дерева
        /// </summary>
        public FreeHierarchyTypeTreeItem Parent;
        /// <summary>
        /// идентификатор узла
        /// </summary>
        public int FreeHierTree_ID;
        /// <summary>
        /// название
        /// </summary>
        public string StringName { get; set; }

        public override string ToString()
        {
            return StringName;
        }

        /// <summary>
        /// Права текущего пользователя на данный узел
        /// </summary>
        public EnumObjectRightType? ObjectRightType;
        
        /// <summary>
        /// Фильтр модулей в которых будет отображаться дерево
        /// </summary>
        public EnumModuleFilter? ModuleFilter;

        List<FreeHierarchyTypeTreeItem> children = new List<FreeHierarchyTypeTreeItem>();

        //дочерние объекты
        public List<FreeHierarchyTypeTreeItem> Children
        {
            get { return children; }
        }

        public object GetItemForSearch()
        {
            return this;
        }

        public System.Collections.IEnumerable GetChildren()
        {
            return Children;
        }

        public string GetKey
        {
            get { return FreeHierTree_ID.ToString(); }
        }

        public void Dispose()
        {
            if (children != null)
            {
                children.ForEach(ch=>ch.Dispose());
                children = null;
            }

            Parent = null;
        }

        public Stack GetParents()
        {
            var stack = new Stack();
            FreeHierarchyTypeTreeItem item = this;
            while (item != null)
            {
                stack.Push(item);
                item = item.Parent;
            }
            return stack;
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                _PropertyChanged("IsSelected");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void _PropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion


        public FreeHierarchyTypeTreeItem FindById(int freeHierTreeId)
        {
            if (FreeHierTree_ID == freeHierTreeId) return this;
            if (children == null) return null;
            if (children.Count > 0)
            {
                foreach (var child in children)
                {
                    var fi = child.FindById(freeHierTreeId);
                    if (fi != null) return fi;
                }
            }

            return null;
        }
    }

    public class FreeHierarchyTypeTreeItemComparer : IComparer<FreeHierarchyTypeTreeItem>
    {
        public int Compare(FreeHierarchyTypeTreeItem x, FreeHierarchyTypeTreeItem y)
        {
            return string.Compare(x.StringName, y.StringName, StringComparison.Ordinal);
        }
    }
}
