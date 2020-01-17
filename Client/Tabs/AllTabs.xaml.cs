using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Proryv.AskueARM2.Client.Visual.Common;

namespace Proryv.AskueARM2.Client.Visual
{
    /// <summary>
    ///     Interaction logic for AllTabs.xaml
    /// </summary>
    public partial class AllTabs : UserControl
    {
        public static readonly DependencyProperty ListProperty = DependencyProperty.Register(
            "List", typeof(ObservableCollection<KeyValuePair<TabHeader, string>>), typeof(AllTabs),
            new PropertyMetadata(default(ObservableCollection<KeyValuePair<TabHeader, string>>)));

        public AllTabs()
        {
            InitializeComponent();
            DataContext = this;
        }


        public ObservableCollection<KeyValuePair<TabHeader, string>> List
        {
            get { return (ObservableCollection<KeyValuePair<TabHeader, string>>) GetValue(ListProperty); }
            set { SetValue(ListProperty, value); }
        }


        //internal Popup popup;

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Manager.UI.SetActiveTab(((KeyValuePair<TabHeader, string>) (sender as FrameworkElement).DataContext).Key);
            close_Click(null, null);
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            //Header.AllTabs.popup.IsOpen = false;
        }

       

        private void HistoryShortcut_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var historyShortcut = sender as HistoryShortcut;
            if (historyShortcut != null) Manager.UI.SetActiveTab(historyShortcut.TabHeader);
            close_Click(null, null);
        }


      

    }
}