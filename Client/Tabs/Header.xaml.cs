using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Proryv.AskueARM2.Client.Visual.Common;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Proryv.AskueARM2.Client.Visual.PowerManagement;
using System.Windows.Input;

namespace Proryv.AskueARM2.Client.Visual
{
    /// <summary>
    /// Interaction logic for Header.xaml
    /// </summary>
    public partial class Header : UserControl
    {
        public Header()
        {
            InitializeComponent();
         
            Tabs = new ObservableCollection<KeyValuePair<TabHeader, string>>();
            Tabs.CollectionChanged += Tabs_CollectionChanged;
        }

        private void Tabs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (var tabHeader in e.NewItems.OfType<KeyValuePair<TabHeader, string>>().Select(i => i.Key))
                        {
                            TabsStackPanel.Children.Add(tabHeader);
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (var tabHeader in e.OldItems.OfType<KeyValuePair<TabHeader, string>>().Select(i => i.Key))
                        {
                            TabsStackPanel.Children.Remove(tabHeader);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        TabsStackPanel.Children.Clear();
                        break;
                    }
            }
        }

        void Header_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TabHeader.LastClosed != null)
            {
                var history = (WorkPage.CurrentPage as WorkPage).HistoryContent as StackPanel;
                HistoryShortcut founded = null;
                foreach (HistoryShortcut hs in history.Children)
                {
                    if (hs.Frame == TabHeader.LastClosed)
                    {
                        founded = hs;
                        break;
                    }
                }
                if (founded != null)
                {
                    HistoryShortcut.LastClicked = founded;
                    (WorkPage.CurrentPage as WorkPage).del_Click(null, null);
                    TabHeader.LastClosed = null;
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Manager.IsDesignMode)
            {
                WorkPage.CurrentPage = this.FindParent<Page>();
               // if (WorkPage.CurrentPage is WorkPage) MagnifyButton.IsEnabled = true;
               // else butAllWindows.Visibility = Visibility.Collapsed;
            }
        }

        public Visibility ScrollVisibility
        {
            get { return scroll.Visibility; }
            set { scroll.Visibility = value; }
        }

        

        public ObservableCollection<KeyValuePair<TabHeader, string>> Tabs { get; set; }

        //private void butAllWindows_Click(object sender, RoutedEventArgs e)
        //{
            //if (AllTabs == null)
            //{
            //    //var list = new List<KeyValuePair<TabHeader, string>>();
            //    //foreach (TabHeader tab in tabs.Children)
            //    //{
            //    //    list.Add(new KeyValuePair<TabHeader, string>(tab, tab.Caption.ToString()));
            //    //}
            //    var allTabs = new AllTabs();
            //    allTabs.tabs.SelectedItem = allTabs.List.Where(t => t.Key == WorkPage.ActiveHeader).FirstOrDefault();
            //    var popup = new Popup()
            //    {
            //        PlacementTarget = butAllWindows,
            //        Placement = PlacementMode.Bottom,
            //        Child = allTabs
            //    };
            //    popup.Closed += new EventHandler(popup_Closed);
            //    //allTabs.popup = popup;
            //    popup.IsOpen = true;

            //    AllTabs = allTabs;
            //}
            //else AllTabs.popup.IsOpen = false;
        //}

        

        void popup_Closed(object sender, EventArgs e)
        {
        //    AllTabs = null;
        }

        public static AllTabs AllTabs = null;

        private void alarm_Click(object sender, RoutedEventArgs e)
        {
            object frame = null;
            foreach (KeyValuePair<TabHeader, string> tab in Tabs)
            {
                var m = tab.Key.ArmModule as IModule;
                if (m != null)
                {
                    if (m.ModuleType == ModuleType.AlarmsCurrent)
                    {
                        frame = m;
                    }
                }
            }
            if (frame != null) Manager.UI.CloseTab(frame);
            Manager.UI.AddTab("Текущие тревоги", Manager.Modules.CreateModule(ModuleType.AlarmsList));
        }

        private void UserRequest_Click(object sender, RoutedEventArgs e)
        {
            var requestHolder = ((this.Parent as FrameworkElement).FindName("userrequest") as ContentControl);
            if (requestHolder.Visibility == Visibility.Visible)
            {
                requestHolder.Visibility = Visibility.Collapsed;
                var ur = requestHolder.Content as UserRequests;
                if (ur != null) ur.stop();
                requestHolder.Content = null;
            }
            else
            {
                var ur = new UserRequests();
                if (Double.IsNaN(ur.Height)) ur.Height = 200;
                if (Double.IsNaN(ur.Width)) ur.Width = 400;
                if (Manager.Config.UserRequestSize.Width > 2)
                {
                    ur.Width = Manager.Config.UserRequestSize.Width;
                    ur.Height = Manager.Config.UserRequestSize.Height;
                }
                var h = WorkPage.CurrentPage.ActualHeight - 55;
                if (h < ur.Height) ur.Height = h;
                ur.MinHeight = ur.Height;
                requestHolder.Content = ur;
                VisualEx.MakeResizable(requestHolder.Content as FrameworkElement, false);
                requestHolder.Visibility = Visibility.Visible;
            }
        }


        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollviewer = sender as ScrollViewer;
            if (e.Delta > 0)
                scrollviewer.LineLeft();
            else
                scrollviewer.LineRight();
            e.Handled = true;
        }


        private void BJournalsOnClick(object sender, RoutedEventArgs e)
        {
            object frame = null;
            foreach (KeyValuePair<TabHeader, string> tab in Tabs)
            {
                var m = tab.Key.ArmModule as IModule;
                if (m != null)
                {
                    if (m.ModuleType == ModuleType.AlarmsCurrent)
                    {
                        frame = m;
                    }
                }
            }
            if (frame != null) Manager.UI.CloseTab(frame);
            Manager.UI.AddTab("Журнал событий", Manager.Modules.CreateModule(ModuleType.GlobalJournal));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
