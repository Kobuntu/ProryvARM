using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using Infragistics.Controls.Menus;
using Proryv.AskueARM2.Client.ServiceReference.DeclaratorService;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.StimulReportService;
using Proryv.AskueARM2.Client.Visual;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;

namespace Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree
{
    /// <summary>
    /// Interaction logic for FreeItem.xaml
    /// </summary>
    public partial class FreeItem :IDisposable
    {
        public FreeItem()
        {
            InitializeComponent();
        }

//        public static readonly ItemSelector Selector = new ItemSelector();
        private FreeHierarchyTree _parentTree;
        private FreeHierarchyTreeDescriptor _treeDescriptor;
        private Trees_ItemSelector _outerSelector;
        private FreeHierarchyTreeItem _freeHierarchyTreeItem;


        public enumTreeMode? TreeMode;

        private void object_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext == null)
            {
                ClearContent();
                return;
            }

            if (!IsLoaded) return;

            InitVisual();
        }

        private HashSet<EnumFreeHierarchyItemType> _notRecreatableTypes
            = new HashSet<EnumFreeHierarchyItemType> { EnumFreeHierarchyItemType.TI, EnumFreeHierarchyItemType.TP, EnumFreeHierarchyItemType.PS,
            EnumFreeHierarchyItemType.HierLev1, EnumFreeHierarchyItemType.HierLev2, EnumFreeHierarchyItemType.HierLev3};

        private void InitVisual()
        {
            var nodeContext = DataContext as XamDataTreeNodeDataContext;
            if (nodeContext == null)
            {
                return;
            }

            var item = nodeContext.Data as FreeHierarchyTreeItem;

            if (_parentTree == null)
            {
                _parentTree = nodeContext.Node.NodeLayout.Tree.TryFindResource("treeModule") as FreeHierarchyTree; //.FindParent<FreeHierarchyTree>();
            }

            #region Определяемся нужно ли пересоздавать контрол

            var isRecreateControl = _freeHierarchyTreeItem == null
                || item == null;

            if (!isRecreateControl)
            {
                var oldTypeFound = false;
                var newTypeFound = false;

                isRecreateControl = true;

                foreach (var notRecreatableType in _notRecreatableTypes)
                {
                    if (!oldTypeFound && notRecreatableType == _freeHierarchyTreeItem.FreeHierItemType)
                    {
                        oldTypeFound = true;
                    }

                    if (!newTypeFound && notRecreatableType == item.FreeHierItemType)
                    {
                        newTypeFound = true;
                    }

                    if (oldTypeFound && newTypeFound)
                    {
                        isRecreateControl = false;
                        break;
                    }
                }
            }

            #endregion

            _freeHierarchyTreeItem = item;
         
            if (_freeHierarchyTreeItem == null || _freeHierarchyTreeItem.Descriptor == null) return;

            //MainLayout.DataContext = _freeHierarchyTreeItem;

            if (_outerSelector == null && _parentTree != null && _parentTree.OuterSelector!=null)
            {
                _outerSelector = _parentTree.OuterSelector;
            }

            //if (_freeHierarchyTreeItem.UpdateItemContent == null)
            //{
            //    _freeHierarchyTreeItem.UpdateItemContent = UpdateItemContent;
            //}

            if (_parentTree == null) return;

            _treeDescriptor = _parentTree.GetDescriptor();
            if (_treeDescriptor == null) return;

            bool isHideStandartIcon = false;

               
            //если указана пользовательская иконка то скрываем стандартные
            if (item != null && item.NodeIcon_ID != null)
            {
                isHideStandartIcon = true;

                DictNodeIcon imgBytes;
                GlobalTreeDictionary.DictNodeIconList.TryGetValue(item.NodeIcon_ID ?? 0, out imgBytes);


                if (imgBytes != null) UserImage.Source = imgBytes.DataImage;
                UserImage.Visibility = Visibility.Visible;

            }
            else if (UserImage.Visibility == Visibility.Visible)
            {
                UserImage.Visibility = Visibility.Collapsed;
                //TODO
            }
            

            //if (ccLayout.Content != null) DisposeControlContent();
            UpdateItemContent(_freeHierarchyTreeItem, isRecreateControl, isHideStandartIcon);

            //ccLayout.Content = _freeHierarchyTreeItem.Name;
        }

        private void UpdateItemContent(FreeHierarchyTreeItem freeHierarchyTreeItem, bool isRecreateControl = true, bool isHideStandartIcon = false)
        {
            var isSelectSingle = _treeDescriptor != null && _treeDescriptor.IsSelectSingle;

            if (isSelectSingle)
            {
                bMenu.Visibility = cbSelect.Visibility = Visibility.Collapsed;
            }
            //else
            //{
            //if (bMenu.CommandTarget == null) bMenu.CommandTarget = _parentTree;
            //}

            var obj = freeHierarchyTreeItem.HierObject ?? freeHierarchyTreeItem.ItemObject;
            if (obj is FreeHierarchyTreeItem)
            {
                var tb = new TextBlock
                {
                    Text = obj.ToString(),
                };
                //tb.SetBinding(TextBlock.TextProperty, "StringName");
                ccLayout.Content = tb;
            }
            else
            {
                var needRecreateContent = true;

                if (!isRecreateControl && ccLayout.Content != null)
                {
                    //Универсальный контрол, пересоздавать визуальный объект не нужно, просто меняем DataContext
                    if (ccLayout.Content is HierObject)
                    {
                        //fec.DataContext = obj;
                        needRecreateContent = false;
                    }
                    else
                    {
                        var tb = ccLayout.Content as TextBlock;
                        if (tb!=null)
                        {
                            tb.Text = obj.ToString();
                        }
                        //else
                        //{
                        //    var f = ccLayout.Content as FrameworkElement;
                        //    if (f != null) f.DataContext = obj;
                        //}
                    }
                }

                DataTemplate dt = null;
                if (_parentTree != null)
                {
                    var parentFrame = _parentTree.PermissibleForSelectObjects;
                    if (parentFrame != null && parentFrame.PermissibleForSelectObjects != null &&
                        parentFrame.PermissibleForSelectObjects.All(p => p != freeHierarchyTreeItem.FreeHierItemType))
                    {
                        cbSelect.Visibility = Visibility.Collapsed;
                    }
                    else if (_outerSelector != null)
                    {
                        TreeMode = _parentTree.TreeMode;
                        dt = _outerSelector.GetTemplate(freeHierarchyTreeItem, _parentTree.ExtendetTemplates,
                            TreeMode);
                        if (dt != null)
                        {
                            var tm = TreeMode.GetValueOrDefault();

                            if (tm == enumTreeMode.SingleObject || tm == enumTreeMode.PSSingleMode ||
                                tm == enumTreeMode.MultiTIMode
                                || tm == enumTreeMode.Formulas || tm == enumTreeMode.OpcUa ||
                                tm == enumTreeMode.FactPower ||
                                (tm == enumTreeMode.PSMultiMode && freeHierarchyTreeItem.FreeHierItemType !=
                                 EnumFreeHierarchyItemType.PS))
                            {
                                cbSelect.Visibility = Visibility.Collapsed;
                                if (tm != enumTreeMode.MultiTIMode) bMenu.Visibility = Visibility.Collapsed;
                            }
                            else if (!isSelectSingle)
                            {
                                cbSelect.Visibility = Visibility.Visible;
                            }
                        }
                        else if (cbSelect.Visibility != Visibility.Visible)
                        {
                            cbSelect.Visibility = Visibility.Visible;
                        }
                    }
                    else if (!isSelectSingle)
                    {
                        cbSelect.Visibility = Visibility.Visible;
                    }
                }

                if (_treeDescriptor != null && _treeDescriptor.Tree_ID <= 0 &&
                    (FreeHierarchyTreeItem.NoHaveChildren.Contains(freeHierarchyTreeItem.FreeHierItemType)))
                {
                    bMenu.Visibility = Visibility.Collapsed;
                }
                else if (!isSelectSingle && bMenu.Visibility != Visibility.Visible)
                {
                    bMenu.Visibility = Visibility.Visible;
                }

                if (dt != null)
                {
                    ccLayout.Content = dt.LoadContent();
                }
                else if (needRecreateContent)
                {
                    if (freeHierarchyTreeItem.HierObject != null)
                    {
                        ccLayout.Content = freeHierarchyTreeItem.HierObject.ToControl();
                        //content = new TextBlock { DataContext = obj };
                    }
                    else
                    {
                        if (_parentTree != null && _parentTree.Selector != null)
                        {
                            dt = _parentTree.Selector.SelectTemplate(obj, null);
                        }

                        if (dt == null) return;

                        ccLayout.Content = dt.LoadContent();
                    }
                }

                if (isHideStandartIcon)
                {
                    var hidable = ccLayout.Content as IHideStandartIcon;
                    if (hidable != null)
                    {
                        hidable.SetVisibilityStandartIcon(false);
                    }
                }

                var fe = ccLayout.Content as FrameworkElement;
                if (fe != null)
                {
                    if (dt == null)
                    {
                        fe.DataContext = obj;
                    }
                    else
                    {
                        fe.DataContext = freeHierarchyTreeItem;
                    }
                }
                else
                {
                    ccLayout.DataContext = obj;
                }
            }
        }

        private void ClearContent()
        {
            //_parentTree = null;
            //_outerSelector = null;

            //if (_freeHierarchyTreeItem != null)
            //{
                //_freeHierarchyTreeItem.UpdateItemContent = null;
                //_freeHierarchyTreeItem = null;
            //}

            try
            {
                DisposeControlContent();
                if (DataContext != null) DataContext = null;
                //RemoveHimself();
            }
            catch
            {
            }
        }

        private void RemoveHimself()
        {
            var node = VisualTreeHelper.GetParent(this) as NodeLayout;
            if (node == null) return;

            
        }

        private void DisposeControlContent()
        {
            //var tb = ccLayout.Content as TextBlock;
            //if (tb != null)
            //{
            //    BindingOperations.ClearBinding(tb, TextBlock.TextProperty);
            //}

            //var d = ccLayout.Content as IDisposable;
            //if (d != null)
            //{
            //    d.Dispose();
            //}

            //ccLayout.Content = null;
        }

        private void FreeItemOnLoaded(object sender, RoutedEventArgs e)
        {
            if (ccLayout.Content != null || DataContext == null) return;

            InitVisual();
        }

        #region IDisposable

        public void Dispose()
        {
            ClearContent();
            Tag = null;
        }

        #endregion

        private bool _isManagersettted;

        private void BMenu_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isManagersettted) return;

            Infragistics.Controls.Menus.ContextMenuService.SetManager(bMenu, new ContextMenuManager()
            {
                OpenMode = OpenMode.LeftClick,
                ContextMenu = new FreeHierContextMenu(),
            });

            _isManagersettted = true;
        }
    }
}
