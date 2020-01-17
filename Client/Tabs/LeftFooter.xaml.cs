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
using System.Windows.Controls.Primitives;
using Proryv.AskueARM2.Client.Visual.Common;

namespace Proryv.AskueARM2.Client.Visual
{
    /// <summary>
    /// Interaction logic for LeftFooter.xaml
    /// </summary>
    public partial class LeftFooter : UserControl
    {
        public LeftFooter()
        { 
            InitializeComponent();
        }

        private void filterUp_Click(object sender, RoutedEventArgs e)
        {
            var filterHolder = ((this.Parent as FrameworkElement).FindName("filter") as ContentControl);
            if (filterHolder.Visibility == Visibility.Visible)
            {
                filterHolder.Visibility = Visibility.Collapsed;
                filterHolder.Content = null;
            }
            else
            {
                filterHolder.Content = new GlobalFilter();
                filterHolder.Visibility = Visibility.Visible;
            }
        }

        private void changeUser_Click(object sender, RoutedEventArgs e)
        {
            Manager.UI.Show2ButtonsDialog("Перезагрузить АРМ", "Сменить пользователя", "Под тем же пользователем", delegate()
            {
                if (Header.AllTabs != null && Header.AllTabs.Parent is Popup)
                    (Header.AllTabs.Parent as Popup).IsOpen = false;
                (Manager.UI as IDisposable).Dispose();
                Manager.Modules.ResetCacheData();                
                var nav = NavigationService.GetNavigationService(this);
                Manager.User = null;
                Manager.UserName = Manager.Password = null;
                nav.Navigate(new LoginPage());
            }, LoginPage.Restart);
        }

        /// <summary>
        /// Сброс настроек АРМа
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetSettingsClick(object sender, RoutedEventArgs e)
        {
            Action resetAction = () =>
            {
                Manager.ResetSettings();
                if (Manager.Config == null) return;

                Manager.Config.AfterLoad();
                Manager.SaveConfig(true);
            };

            if (Manager.UI == null)
            {
                resetAction.Invoke();

            }
            else
            {
                Manager.UI.ShowYesNoDialog("Хотите вернуть все настройки?", resetAction);
            }

        }

    }
}
