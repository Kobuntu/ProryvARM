using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.Visual.BalanceEditor;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Formulas.BalanceEditor.Data;
using Proryv.ElectroARM.Controls.Controls.Tabs;
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

namespace Proryv.AskueARM2.Client.Visual.Formulas.BalanceEditor.UI.Controls.BalanceFreeHierarchyFooter
{
    /// <summary>
    /// Логика взаимодействия для EditFreeHierarchyFooterPopup.xaml
    /// </summary>
    public partial class EditFreeHierarchyFooterListControl : ILocalChildren
    {
        public EditFreeHierarchyFooterListControl(EditFreeHierarchyFooterViewModelContainer container)
        {
            InitializeComponent();

            DataContext = _viewModel = new EditFreeHierarchyFooterViewModel(container.Footers, this);
            _container = container;
        }

        EditFreeHierarchyFooterViewModel _viewModel;
        EditFreeHierarchyFooterViewModelContainer _container;

        #region ILocalChildren
        public Action OnClose { get; set; }

        public EnumLocalModalType LocalModalType
        {
            get
            {
                return EnumLocalModalType.TrueModal;
            }
        }

        public Guid Id { get; set; }

        #endregion

        private void MenuInsertClick(object sender, RoutedEventArgs e)
        {
            AddNewFooter();
        }

        private void AddNewFooter()
        {
            var newFooter = new Dict_Balance_FreeHierarchy_Footers
            {
                BalanceFreeHierarchy_UN = _container.BalanceFreeHierarchyUn,
                BalanceFreeHierarchySection_UN = _container.BalanceFreeHierarchySectionUN,
                BalanceFreeHierarchyFooterName = "Итого",
                Coef = 1,
                UseInTotalResult = false,
                SortNumber = (byte)_viewModel.GridData.Count,
            };

            var balanceFooter = new BalanceFreeHierarchyFooterEditorControl(newFooter, _container.Sections, () =>
            {
                _viewModel.GridData.Add(newFooter);
            });

            Manager.UI.ShowLocalModal(balanceFooter, "Добавить новый итог для '" + _container.BalanceFreeHierarchySectionName + "'", this, false, true);
        }

        private void EditFooterClick(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe == null) return;

            var footer = fe.DataContext as Dict_Balance_FreeHierarchy_Footers;
            if (footer == null) return;

            var balanceFooter = new BalanceFreeHierarchyFooterEditorControl(footer, _container.Sections, null);

            Manager.UI.ShowLocalModal(balanceFooter, "Редактировать итог для '" + _container.BalanceFreeHierarchySectionName + "'", this, false, true);
        }

        private void DeleteFooterClick(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe == null) return;

            var footer = fe.DataContext as Dict_Balance_FreeHierarchy_Footers;
            if (footer == null) return;

            Manager.UI.ShowLocalYesNoDialog("Хотите удалить '" + footer.BalanceFreeHierarchyFooterName + "'?", () =>
             {
                 this.RunBackgroundAsync<Tuple<string, bool>>("BL_DeleteFreeHierarchySectionFooters",
                 OnFooterDeleted, EnumServiceType.ArmService, footer.BalanceFreeHierarchyFooter_UN);
             });
        }

        private void OnFooterDeleted(Tuple<string, bool> deleted)
        {
            if (deleted == null || !deleted.Item2) return;

            _viewModel.GridData.RemoveAll(f => f.BalanceFreeHierarchyFooter_UN == deleted.Item1);

            Manager.UI.ShowLocalMessage("Итог удален", this);
        }

        private void AddFooterClick(object sender, RoutedEventArgs e)
        {
            AddNewFooter();
        }

        private void SaveFootersOrderClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel.GridData.Count == 0)
            {
                Manager.UI.ShowLocalMessage("Список пустой. Сохранение отменено.", this);
                return;
            }

            var footers = _viewModel.GridData.ToList();
            byte sortOrder = 0;
            footers.ForEach(f =>
            {
                f.SortNumber = sortOrder++;
            });

            this.RunBackgroundAsync<Tuple<bool>>("BL_SaveFootersOrder",
                OnSaveFootersOrder, EnumServiceType.ArmService, footers);
        }

        private void OnSaveFootersOrder(Tuple<bool> tuple)
        {
            if (tuple == null || !tuple.Item1) return;

            Manager.UI.ShowLocalMessage("Порядок следования итогов сохранен", this);
        }
    }
}
