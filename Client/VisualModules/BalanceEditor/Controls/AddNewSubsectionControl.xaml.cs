using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ActiproSoftware.Windows.Extensions;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual.BalanceEditor;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Formulas.BalanceEditor.Data;

namespace Proryv.AskueARM2.Client.Visual.Formulas.BalanceEditor.UI.Controls
{
    /// <summary>
    /// Interaction logic for AddNewSubsectionControl.xaml
    /// </summary>
    public partial class AddNewSubsectionControl 
    {
        private readonly ObservableCollection<Dict_Balance_FreeHierarchy_Subsection> _subsections;
        private readonly Dict_Balance_FreeHierarchy_Subsection _subsection;

        public AddNewSubsectionControl(ObservableCollection<Dict_Balance_FreeHierarchy_Subsection> subsections, Dict_Balance_FreeHierarchy_Subsection subsection)
        {
            InitializeComponent();
            _subsections = subsections;
            if (subsection != null)
            {
                tbSubsectionName.Text = subsection.BalanceFreeHierarchySubsectionName;
                _subsection = subsection;
                cbSubsectionTypes.IsEnabled = false;
            }
            else
            {
                _subsection = new Dict_Balance_FreeHierarchy_Subsection
                {

                };
            }

            cbSubsectionTypes.ItemsSource = EnumClientServiceDictionary.FreeHierarchyBalanceSubsectionsTypes;
            cbSubsectionTypes.SelectedValue = _subsection.SubsectionType;

            Dispatcher.BeginInvoke((Action) (() =>
            {
                tbSubsectionName.Focus();
                tbSubsectionName.CaretIndex = tbSubsectionName.Text.Length;
            }), DispatcherPriority.Background);

        }

        private void bSaveClick(object sender, RoutedEventArgs e)
        {
            var name = tbSubsectionName.Text;
            if (string.IsNullOrEmpty(name))
            {
                Manager.UI.ShowMessage("Задайте осмысленное название подраздела");
                return;
            }

            if (_subsections.Any(s => string.Equals(s.BalanceFreeHierarchySubsectionName, name)))
            {
                Manager.UI.ShowMessage("Подраздел с таким названием уже описан");
                return;
            }

             var parentControl = this.FindParent<BalanceSectionControl>();
            if (parentControl==null) return;

            var balanceFreeHierarchyObjectUn = parentControl.FindResource("BalanceFreeHierarchyObjectUn") as string;
            var subsectionType = (byte) cbSubsectionTypes.SelectedValue;
            switch (subsectionType)
            {
                case 1:
                    _subsection.BalanceFreeHierarchySection_UN = (parentControl.DataContext as BalanceFreeHierarchySectionRow).BalanceFreeHierarchySectionUN;
                    break;
                case 2:
                    _subsection.BalanceFreeHierarchyObject_UN = balanceFreeHierarchyObjectUn;
                    break;
            }

            _subsection.BalanceFreeHierarchySubsectionName = name;
            parentControl.RunBackgroundAsync<Dict_Balance_FreeHierarchy_Subsection>("BL_SaveFreeHierarchyBalanceSubsection", OnSaveCompleted, EnumServiceType.ArmService, 
                _subsection, Manager.User.User_ID);
        }

        private void OnSaveCompleted(Dict_Balance_FreeHierarchy_Subsection subsection)
        {
            this.CloseModal();

            if (subsection == null) return;
            var parent = this.FindParent<AddSubsectionPopup>();

            if (_subsection ==null || string.IsNullOrEmpty(_subsection.BalanceFreeHierarchySubsection_UN))
            {
                //Это добавление нового подраздела
                _subsections.Add(subsection);
                if (parent != null) parent.cbSubsections.SelectedItem = subsection;
            }
            else
            {
                //Это редактирование названия   
                var oldsubsection = _subsections.FirstOrDefault(s => string.Equals(s.BalanceFreeHierarchySubsection_UN, subsection.BalanceFreeHierarchySubsection_UN));
                if (oldsubsection != null) oldsubsection.BalanceFreeHierarchySubsectionName = subsection.BalanceFreeHierarchySubsectionName;
                if (parent != null) parent.cbSubsections.SelectedItem = oldsubsection;
            }
        }
    }
}
