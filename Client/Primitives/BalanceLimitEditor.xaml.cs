using System;
using System.Windows;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual;
using Proryv.AskueARM2.Client.Visual.Common;

namespace Proryv.ElectroARM.Controls.Controls.Dialog.Primitives
{
    /// <summary>
    /// Interaction logic for BalanceLimitEditor.xaml
    /// </summary>
    public partial class BalanceLimitEditor
    {
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(BalanceLimitEditor), new PropertyMetadata(true, finishDateChangedCallback));

        static void finishDateChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
        }

        public BalanceLimitEditor()
        {
            InitializeComponent();
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            var balance = DataContext as IBalanceFreeHierarchy;
            if (balance == null) return;

            Manager.UI.RunAsync(ARM_Service.BL_SaveFreeHierarchyBalanceLimit, OnSaveLimit, balance.BalanceUn, balance.BalanceTypeHierarchy, balance.HighLimit, balance.LowerLimit,
                    balance.HighLimitValue, balance.LowerLimitValue, Manager.User.User_ID);
        }

        private void OnSaveLimit(string message)
        {
            if (message == null) return;

            if (message.Length > 0)
            {
                Manager.UI.ShowMessage(message);
                return;
            }

            Manager.UI.ShowMessage("Уставки сохранены");
        }

        private void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            var balance = DataContext as IBalanceFreeHierarchy;
            if (balance == null) return;

            try
            {
                double? highLimit, lowerLimit, highLimitValue, lowerLimitValue;

                var message = ARM_Service.BL_TryLoadFreeHierarchyBalanceLimit(balance.BalanceUn, balance.BalanceTypeHierarchy,
                    out highLimit, out lowerLimit, out highLimitValue, out lowerLimitValue);

                if (message == null) return;

                if (message.Length > 0)
                {
                    Manager.UI.ShowMessage(message);
                    return;
                }

                balance.HighLimit = highLimit;
                balance.LowerLimit = lowerLimit;
                balance.HighLimitValue = highLimitValue;
                balance.LowerLimitValue = lowerLimitValue;
            }
            catch (Exception ex)
            {
                Manager.UI.ShowMessage(ex.Message);
            }
        }
    }
}
