using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.Visual.BalanceEditor;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Formulas.BalanceEditor.Data;
using Proryv.AskueARM2.Client.Visual.Formulas.BalanceEditor.UI.Controls.BalanceFreeHierarchyFooter;
using Proryv.ElectroARM.Controls.Common.DragAndDrop;
using Proryv.ElectroARM.Controls.Converters;

namespace Proryv.AskueARM2.Client.Visual.Formulas.BalanceEditor.UI.Controls
{
    /// <summary>
    /// Interaction logic for BalanceSectionControl.xaml
    /// </summary>
    public partial class BalanceSectionControl: IDisposable
    {
        public BalanceSectionControl()
        {
            InitializeComponent();
        }

        private void bAddSubsectionClick(object sender, RoutedEventArgs e)
        {
            if (pSubsections.IsOpen)
            {
                Manager.UI.CloseAllPopups();
                return;
            }

            try
            {
                if (cAddSubsection.cbSubsections.ItemsSource == null)
                {
                    var source = FindResource("Subsections") as ObservableCollection<Dict_Balance_FreeHierarchy_Subsection>;

                    var view = new ListCollectionView(source);
                    if (view.GroupDescriptions != null && view.GroupDescriptions.Count != 0) return;

                    cAddSubsection.cbSubsections.ItemsSource = view;

                    var itemRow = DataContext as BalanceFreeHierarchySectionRow;
                    if (itemRow == null) return;
                    var sectionUn = itemRow.BalanceFreeHierarchySectionUN;

                    var groupDescription = new PropertyGroupDescription("This", new BalanceFreeHierarchySubsectionTypeConverter());
                    view.GroupDescriptions.Add(groupDescription);
                    view.Filter =
                        o =>
                        {
                            var subsection = o as Dict_Balance_FreeHierarchy_Subsection;
                            if (subsection == null) return false;

                            if (subsection.SubsectionType == 0) return true;
                            if (subsection.SubsectionType == 1 && string.Equals(subsection.BalanceFreeHierarchySection_UN, sectionUn)) return true;
                            if (subsection.SubsectionType == 2 && !string.IsNullOrEmpty(subsection.BalanceFreeHierarchyObject_UN)) return true;

                            return false;
                        };

                }
            }
            catch (Exception ex)
            {

            }

            pSubsections.OpenAndRegister(true);                

        }

        public void Dispose()
        {
            lvItems.Dispose();
            lvSubsections.Dispose();
        }

        private void bRemoveAllObjectClick(object sender, RoutedEventArgs e)
        {
            var itemRow = DataContext as BalanceFreeHierarchySectionRow;
            if (itemRow == null) return;
            Manager.UI.ShowYesNoDialog("Хотите удалить все объекты из раздела?", () =>
            {
                itemRow.ClearRecursive();
            });
        }

        private void bEditFooterClick(object sender, RoutedEventArgs e)
        {
            var parentControl = this.FindParent<BalanceSectionControl>();
            if (parentControl == null) return;

            var itemRow = DataContext as BalanceFreeHierarchySectionRow;
            if (itemRow == null) return;

            var balanceFreeHierarchyUn = parentControl.TryFindResource("BalanceFreeHierarchyUn") as string;
            if (string.IsNullOrEmpty(balanceFreeHierarchyUn))
            {
                Manager.UI.ShowMessage("Идентификатор не присвоен. Сначала необходимо сохранить баланс");
                return;
            }

            var container = new EditFreeHierarchyFooterViewModelContainer
            {
                Sections = parentControl.FindResource("Sections") as List<Dict_Balance_FreeHierarchy_Section>,
                BalanceFreeHierarchyUn = balanceFreeHierarchyUn,
                BalanceFreeHierarchySectionUN = itemRow.BalanceFreeHierarchySectionUN,
                BalanceFreeHierarchySectionName = itemRow.BalanceFreeHierarchySectionName,
            };

            this.RunBackgroundAsync<List<Dict_Balance_FreeHierarchy_Footers>>("BL_GetFreeHierarchySectionFooters",
                 footers => OnFootersRecieved(container, footers), EnumServiceType.ArmService, container.BalanceFreeHierarchyUn, container.BalanceFreeHierarchySectionUN);
        }

        private void OnFootersRecieved(EditFreeHierarchyFooterViewModelContainer container, List<Dict_Balance_FreeHierarchy_Footers> footers)
        {
            container.Footers = footers;
            var footerListControl = new EditFreeHierarchyFooterListControl(container);

            Manager.UI.ShowLocalModal(footerListControl, "Итоги для '" + container.BalanceFreeHierarchySectionName + "'", this.FindTrueIModule(), false, true);
        }
    }
}
