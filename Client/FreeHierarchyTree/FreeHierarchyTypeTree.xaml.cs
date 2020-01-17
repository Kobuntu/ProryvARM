using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Infragistics.Controls.Menus;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.Configuration;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using System;
using System.Collections;
using System.Collections.Generic;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;
using EnumFreeHierarchyItemType = Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService.EnumFreeHierarchyItemType;
using System.ComponentModel;
using Proryv.ElectroARM.Controls.Controls.Popup.Finder;

namespace Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree
{
    /// <summary>
    /// Interaction logic for FreeHierarchyTypeTree.xaml
    /// </summary>
    public partial class FreeHierarchyTypeTree : IDisposable, INotifyPropertyChanged
    {
        public FreeHierarchyTypeTree()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public void Dispose()
        {
            try
            {
                if (Items != null)
                {
                    Items.DisposeChildren();
                    Items.Clear();
                }
            }
            catch
            {
            }
        }

        bool _isDeclaratorMainTree = false;
        EnumModuleFilter? _currentRightFilter;
        public void LoadTypes(EnumModuleFilter? rightFilter, HashSet<long> selectedNodes = null, bool IsDeclaratorMainTree = false)
        {
            _isDeclaratorMainTree = IsDeclaratorMainTree;
            _currentRightFilter = rightFilter;

            if (!Manager.IsDesignMode)
            {
                try
                {
                    if (Items != null)
                    {
                        Items.DisposeChildren();
                        Items.Clear();
                    }

                    var treeTypes = GlobalFreeHierarchyDictionary.GetTypes(rightFilter).Values
                            .Where(t => t.Parent == null).ToList();

                    //пока здесь. для описателя в основном дереве отображаем перечень OPCUA серверов, для этого добавляем строку в стандартные деревья
                    if (IsDeclaratorMainTree && Manager.User.IsAdmin)
                    {
                        FreeHierarchyTypeTreeItem stdtree = treeTypes.FirstOrDefault(i => i.FreeHierTree_ID == -100);
                        FreeHierarchyTypeTreeItem OPCtree = new FreeHierarchyTypeTreeItem { FreeHierTree_ID = -1001, StringName = "Сервера OPCUA" };
                        if (stdtree != null && !stdtree.Children.Any(i => i.FreeHierTree_ID == -1001))
                        {

                            stdtree.Children.Add(OPCtree);
                        }
                        else if (stdtree == null && !treeTypes.Any(i => i.FreeHierTree_ID == -1001))
                            treeTypes.Add(OPCtree);
                    }

                    //ТОЛЬКО для описателя
                    if (IsDeclaratorMainTree)
                    {
                        FreeHierarchyTypeTreeItem stdtree = treeTypes.FirstOrDefault(i => i.FreeHierTree_ID == -100);
                        FreeHierarchyTypeTreeItem OPCtree = new FreeHierarchyTypeTreeItem { FreeHierTree_ID = -1002, StringName = "ФИАС - используемые адреса" };
                        if (stdtree != null && stdtree.Children.All(i => i.FreeHierTree_ID != -1002))
                        {

                            stdtree.Children.Add(OPCtree);
                        }
                        else if (stdtree == null && treeTypes.All(i => i.FreeHierTree_ID != -1002))
                            treeTypes.Add(OPCtree);
                    }

                    Items = new ObservableCollection<FreeHierarchyTypeTreeItem>(treeTypes);
                }
                catch (Exception)
                {
                    Manager.UI.ShowMessage("Не удалось загрузить типы свободных иерархий!");
                }
            }
        }

        /// <summary>
        /// перезагрузка дерева
        /// </summary>
        public void ReloadTree(int? ActiveNode_ID = null)
        {
           
            var fnd = new Dictionary<object, Stack>();
            FreeHierarchyTypeTreeItem obj = null;

            Manager.User.ReloadFreeHierarchyTypes(Manager.UI.ShowMessage);

            LoadTypes(_currentRightFilter, new HashSet<long> { ActiveNode_ID ?? -101 }, _isDeclaratorMainTree); 
         

            var allNodes = GlobalFreeHierarchyDictionary.GetTypes(_currentRightFilter);

            

            if (allNodes != null) allNodes.TryGetValue(ActiveNode_ID ?? -101, out obj); 
            if (obj == null) return;

            //fnd[obj] = obj.GetParents();

            ActiveTreeNode = obj;


            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new System.Action(delegate()
            {
                try
                {
                    XamTreeFinder.ExpandAndSelectXamTree(fnd, obj, tree);
                }
                catch
                {
                    //ошибки бывают..
                }
            }));
        }


        ObservableCollection<FreeHierarchyTypeTreeItem> _Items;
        public ObservableCollection<FreeHierarchyTypeTreeItem> Items
        {
            get { return _Items; }
            set
            {
                _Items = value;
                NotifyPropertyChanged("Items");

            }
        }

        /// <summary>
        /// исп в Описателе - выделенный мышкой узел
        /// </summary>
        public FreeHierarchyTypeTreeItem ActiveTreeNode = null;
        private void tree_ActiveNodeChanged(object sender, ActiveNodeChangedEventArgs e)
        {
            try
            {
                if (e.NewActiveTreeNode == null || e.NewActiveTreeNode.Data == null)
                    ActiveTreeNode = null;
                else
                    ActiveTreeNode = e.NewActiveTreeNode.Data as FreeHierarchyTypeTreeItem;
                OnActiveNodeChanged();
            }
            catch
            {
            }
        }

        /// <summary>
        /// SelectedItemChanged
        /// </summary>
        public event EventHandler ActiveNodeChanged;
        private void OnActiveNodeChanged()
        {
            if (ActiveNodeChanged != null)
            {
                ActiveNodeChanged(this, new EventArgs());
            }
        }


    }
}
