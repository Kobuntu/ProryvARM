using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.Visual.BalanceEditor;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Formulas.BalanceEditor.Data;

namespace Proryv.AskueARM2.Client.Visual.Formulas.BalanceEditor.UI.Controls
{
    /// <summary>
    /// Interaction logic for AddSubsectionPopup.xaml
    /// </summary>
    public partial class AddSubsectionPopup
    {
        public AddSubsectionPopup()
        {
            InitializeComponent();
        }

        public bool IsChangeSubsection;

        private void bSelectClick(object sender, RoutedEventArgs e)
        {
            var freeHierarchySubsection = cbSubsections.SelectedItem as Dict_Balance_FreeHierarchy_Subsection;
            if (freeHierarchySubsection == null) return;

            FrameworkElement parentControl = this.FindParent<BalanceSubsectionControl>();
            if (parentControl == null)
            {            
                parentControl = this.FindParent<BalanceSectionControl>();
                if (parentControl == null) return;
            }

            if (IsChangeSubsection)
            {
                //Это смена одного подраздела на другой
                var hierarchySubsectionRow = parentControl.DataContext as BalanceFreeHierarchySubsectionRow;
                if (hierarchySubsectionRow == null) return;

                hierarchySubsectionRow.Subsection = freeHierarchySubsection;
            }
            else
            {
                //Это добавление нового подраздела
                var hierarchySectionRow = parentControl.DataContext as BaseBalanceFreeHierarchyRow;
                if (hierarchySectionRow == null) return;

                if (hierarchySectionRow.SubsectionRows.Any(s => string.Equals(s.Subsection.BalanceFreeHierarchySubsection_UN, freeHierarchySubsection.BalanceFreeHierarchySubsection_UN)))
                {
                    Manager.UI.ShowMessage("Этот подраздел уже добавлен");
                    return;
                }

                hierarchySectionRow.SubsectionRows.Add(new BalanceFreeHierarchySubsectionRow(freeHierarchySubsection, hierarchySectionRow, Dispatcher)
                {
                    IsSelected = true
                });
                hierarchySectionRow.IsSelected = true;
            }


            var frame = parentControl.FindParent<BalanceEditor_FreeHierarchy_Frame>();
            if (frame != null) frame.IsBalanceChanged = true;

            Manager.UI.CloseAllPopups();
        }

        private void bNewSubsectionClick(object sender, RoutedEventArgs e)
        {
            var lcv = cbSubsections.ItemsSource as ListCollectionView;
            if (lcv == null) return;

            var subsections = lcv.SourceCollection as ObservableCollection<Dict_Balance_FreeHierarchy_Subsection>;
            if (subsections == null) return;

            tabsLayout.ShowModal(new AddNewSubsectionControl(subsections, null), "Название нового подраздела");
        }

        private void bCloseClick(object sender, RoutedEventArgs e)
        {
            Manager.UI.CloseAllPopups();  
        }

        private void bChangeNameSubsectionClick(object sender, RoutedEventArgs e)
        {
            var lcv = cbSubsections.ItemsSource as ListCollectionView;
            if (lcv == null) return;

            var subsections = lcv.SourceCollection as ObservableCollection<Dict_Balance_FreeHierarchy_Subsection>;
            if (subsections == null) return;

            var selectedSubsection = cbSubsections.SelectedItem as Dict_Balance_FreeHierarchy_Subsection;
            if (selectedSubsection == null) return;

            tabsLayout.ShowModal(new AddNewSubsectionControl(subsections, selectedSubsection), "Измение название подраздела");
        }

        private void bDeleteSubsectionClick(object sender, RoutedEventArgs e)
        {
            var selectedSubsection = cbSubsections.SelectedItem as Dict_Balance_FreeHierarchy_Subsection;
            if (selectedSubsection == null)
            {
                Manager.UI.ShowMessage("Выберите подраздел для удаления");
                return;
            }

            tabsLayout.RunBackgroundAsync<string>("BL_DeleteFreeHierarchyBalanceSubsection", OnDeleteComplete, EnumServiceType.ArmService,
                selectedSubsection.BalanceFreeHierarchySubsection_UN, Manager.User.User_ID);
        }

        private void OnDeleteComplete(string message)
        {
            if (message == null) return;

            if (message != string.Empty)
            {
                Manager.UI.ShowMessage("Ошибка удаления:" + message);
                return;
            }

            Manager.UI.ShowMessage("Подраздел удален");

            var lcv = cbSubsections.ItemsSource as ListCollectionView;
            if (lcv == null) return;

            var subsections = lcv.SourceCollection as ObservableCollection<Dict_Balance_FreeHierarchy_Subsection>;
            if (subsections == null) return;

            var deletedSubsection = cbSubsections.SelectedItem as Dict_Balance_FreeHierarchy_Subsection;
            if (deletedSubsection==null) return;

            subsections.Remove(deletedSubsection);
            cbSubsections.SelectedIndex = 0;


            var sectionControl = this.FindParent<BalanceSectionControl>();
            if (sectionControl == null) return;

            var frame = sectionControl.FindParent<BalanceEditor_FreeHierarchy_Frame>();
            if (frame == null) return;

            var source = frame.lvBalance.ItemsSource as List<BalanceFreeHierarchySectionRow>;
            if (source == null) return;

            var isChanged = false;
            foreach (var sectionRow in source)
            {
                if (sectionRow.SubsectionRows.Count == 0) continue;

                var subsection = sectionRow.SubsectionRows.FirstOrDefault(s => string.Equals(s.Subsection.BalanceFreeHierarchySubsection_UN, deletedSubsection.BalanceFreeHierarchySubsection_UN));
                if (subsection != null)
                {
                    if (sectionRow.Descriptions.Count > 0)
                    {
                        int indx = 0;
                        foreach (var item in sectionRow.Descriptions.ToList())
                        {
                            sectionRow.Descriptions.Insert(indx++, item);
                        }
                    }

                    sectionRow.SubsectionRows.Remove(subsection);
                    isChanged = true;
                }
            }

            if (isChanged) frame.IsBalanceChanged = true;
        }

        /// <summary>
        /// Перенос подраздела в общий раздел
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void butRemoveToMainClick(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe == null) return;

            var subsection = fe.DataContext as Dict_Balance_FreeHierarchy_Subsection;
            if (subsection == null) return;

            Manager.UI.ShowYesNoDialog("Перенести \"" + subsection.BalanceFreeHierarchySubsectionName + "\" в общий раздел?", () =>
            {
                var s = new Dict_Balance_FreeHierarchy_Subsection
                {
                    BalanceFreeHierarchySubsection_UN = subsection.BalanceFreeHierarchySubsection_UN,
                    BalanceFreeHierarchySubsectionName = subsection.BalanceFreeHierarchySubsectionName,
                    
                };

                tabsLayout.RunBackgroundAsync<Dict_Balance_FreeHierarchy_Subsection>("BL_SaveFreeHierarchyBalanceSubsection", OnSaveCompleted, EnumServiceType.ArmService,
                    s, Manager.User.User_ID);
            });
        }

        private void OnSaveCompleted(Dict_Balance_FreeHierarchy_Subsection subsection)
        {
            this.CloseModal();

            if (subsection == null) return;

            var lcv = cbSubsections.ItemsSource as ListCollectionView;
            if (lcv == null) return;

            var subsections = lcv.SourceCollection as ObservableCollection<Dict_Balance_FreeHierarchy_Subsection>;
            if (subsections == null) return;

            var oldsubsection = subsections.FirstOrDefault(s => string.Equals(s.BalanceFreeHierarchySubsection_UN, subsection.BalanceFreeHierarchySubsection_UN));
            if (oldsubsection != null)
            {

                oldsubsection.SubsectionType = 0;
            }

            lcv.Refresh();
        }
    }
}
