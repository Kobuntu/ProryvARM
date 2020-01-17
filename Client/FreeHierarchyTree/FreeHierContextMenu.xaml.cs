using Infragistics.Controls.Menus;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using Proryv.AskueARM2.Client.ServiceReference.FreeHierarchyService;
using Proryv.AskueARM2.Client.ServiceReference.StimulReportService;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree
{
    /// <summary>
    /// Логика взаимодействия для FreeHierContextMenu.xaml
    /// </summary>
    public partial class FreeHierContextMenu :IDisposable
    {
        private FreeHierarchyTree _parentTree;
        public enumTreeMode? TreeMode;

        public FreeHierContextMenu()
        {
            InitializeComponent();
        }

        #region Работа с контекстным меню

        private void ContextMenuOnLoaded(object sender, RoutedEventArgs e)
        {
            if (_parentTree == null)
            {
                InitParentTree();
            }

            if (_parentTree == null) return;

            var cm = sender as XamContextMenu;
            if (cm == null) return;

            var descriptor = _parentTree.GetDescriptor();

            if (descriptor.Tree_ID != GlobalFreeHierarchyDictionary.TreeTypeStandartGroupTP &&
                descriptor.Tree_ID != GlobalFreeHierarchyDictionary.TreeTypeStandartSections &&
                descriptor.Tree_ID != GlobalFreeHierarchyDictionary.TreeTypeStandartSectionsNSI &&
                descriptor.Tree_ID < 0)
            {
                miSelectEpu.Visibility = Visibility.Collapsed;

                if (descriptor.Tree_ID <= -GlobalFreeHierarchyDictionary.TreeTypeStandart)
                {
                    miSelectSections.Visibility = Visibility.Collapsed;
                    if (descriptor.Tree_ID != GlobalFreeHierarchyDictionary.TreeTypeStandartJuridicalPerson)
                    {
                        miSelectTps.Visibility = Visibility.Collapsed;
                    }
                }
            }

            var item = cm.DataContext as FreeHierarchyTreeItem;
            if (item == null)
            {
                //Свободная иерархия
                var xndc = cm.DataContext as XamDataTreeNodeDataContext;
                if (xndc != null)
                {
                    item = xndc.Data as FreeHierarchyTreeItem;
                }
            }

            if (item != null)
            {
                switch (item.FreeHierItemType)
                {
                    case EnumFreeHierarchyItemType.Section:
                        miSelectSections.Visibility = Visibility.Collapsed;
                        break;
                    case EnumFreeHierarchyItemType.TP:
                    case EnumFreeHierarchyItemType.TI:
                        miSelectContracts.Visibility =
                            miSelectLev3.Visibility =
                                miSelectPs.Visibility =
                                    miSelectEpu.Visibility =
                                        miSelectTps.Visibility =
                                            miSelectTi.Visibility =
                                                    miSelectSections.Visibility =
                                                        Visibility.Collapsed;
                        break;
                    case EnumFreeHierarchyItemType.FiasFullAddress:
                        miSelectAll.Visibility = miExpand3.Visibility = miExpand2.Visibility
                            = Visibility.Collapsed;
                        break;
                }

                //Свободная иерархия
            }

            var tm = TreeMode;
            if (tm.HasValue && tm.Value == enumTreeMode.PSMultiMode)
            {
                miSelectTi.Visibility =
                    miSelectChildren.Visibility =
                        miSelectLev3.Visibility =
                            Visibility.Collapsed;
            }
            else
            {
                var parentFrame = _parentTree.PermissibleForSelectObjects;

                if (_parentTree.IsHideTp)
                {
                    miSelectTps.Visibility = Visibility.Collapsed;
                }

                if (_parentTree.IsHideTi)
                {
                    miSelectTi.Visibility = Visibility.Collapsed;

                    if (parentFrame != null && parentFrame.PermissibleForSelectObjects != null)
                    {
                        if (parentFrame.PermissibleForSelectObjects.Contains(EnumFreeHierarchyItemType.PS))
                        {
                            StandartTreeOnLoaded(miSelectPs, e);
                        }
                        else
                        {
                            miSelectPs.Visibility = Visibility.Collapsed;
                        }

                        if (parentFrame.PermissibleForSelectObjects.Contains(EnumFreeHierarchyItemType.HierLev3))
                        {
                            StandartTreeOnLoaded(miSelectLev3, e);
                        }
                        else
                        {
                            miSelectLev3.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        StandartTreeOnLoaded(miSelectPs, e);
                        StandartTreeOnLoaded(miSelectLev3, e);
                    }
                }
                else
                {
                    if (descriptor != null && descriptor.Tree_ID != GlobalFreeHierarchyDictionary.TreeTypeStandartTIFormula && descriptor.Tree_ID <= -102)
                    {
                        miSelectTi.Visibility = Visibility.Collapsed;
                    }

                    miSelectLev3.Visibility = miSelectPs.Visibility = Visibility.Collapsed;
                }

                if (parentFrame != null && parentFrame.PermissibleForSelectObjects != null && parentFrame.PermissibleForSelectObjects.Contains(EnumFreeHierarchyItemType.USPD))
                {
                    StandartTreeOnLoaded(miSelectUSPDs, e);
                }
                else
                {
                    miSelectUSPDs.Visibility = Visibility.Collapsed;
                }

                if (descriptor != null && !_parentTree.IsSelectSingle) miSelectChildren.Visibility = Visibility.Visible;
                else
                {
                    miSelectChildren.Visibility = Visibility.Collapsed;
                }
            }

            //if (descriptor != null && descriptor.NeedFindUaNode) miSelectAll.Visibility = Visibility.Collapsed;
            //else
            {
                miSelectAll.Visibility = Visibility.Visible;
                miSelectAll.IsEnabled = !_parentTree.IsSelectSingle;
            }

            if (descriptor.Tree_ID != GlobalFreeHierarchyDictionary.TreeTypeStandartGroupTP)
            {
                miSelectContracts.Visibility = Visibility.Collapsed;
            }

            SeparatorSection.Visibility = miSelectSections.Visibility;
        }

        private void StandartTreeOnLoaded(object sender, RoutedEventArgs e)
        {
            if (_parentTree == null) return;

            var cm = sender as XamContextMenu;
            if (cm == null) return;

            var descriptor = _parentTree.GetDescriptor();

            if (descriptor.Tree_ID != GlobalFreeHierarchyDictionary.TreeTypeStandart &&
                descriptor.Tree_ID != GlobalFreeHierarchyDictionary.TreeTypeStandartPS &&
                descriptor.Tree_ID != GlobalFreeHierarchyDictionary.TreeTypeStandartTIFormula)
            {
                var fe = sender as FrameworkElement;
                if (fe == null) return;

                fe.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        private void InitParentTree()
        {
            var nodeContext = DataContext as XamDataTreeNodeDataContext;
            if (nodeContext == null)
            {
                return;
            }

            if (_parentTree == null)
            {
                _parentTree = nodeContext.Node.NodeLayout.Tree.FindParent<FreeHierarchyTree>();
                if (_parentTree!=null)
                {
                    TreeMode = _parentTree.TreeMode;
                }
            }
        }

        public void Dispose()
        {
            _parentTree = null;
        }
    }
}
