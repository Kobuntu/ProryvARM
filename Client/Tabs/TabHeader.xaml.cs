using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using Proryv.AskueARM2.Client.Visual.Common.Modules;

namespace Proryv.AskueARM2.Client.Visual 
{
    /// <summary>
    /// Interaction logic for TabHeader.xaml
    /// </summary>
    public partial class TabHeader
    {
        public TabHeader()
        {
            InitializeComponent();
        }

        public bool? IsChecked
        {
            get { return (bool?)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool?), typeof(TabHeader), new UIPropertyMetadata(null));

        public object Caption
        {
            get { return (object)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(object), typeof(TabHeader), new UIPropertyMetadata(""));

        public string NumberString
        {
            get { return (string)GetValue(NumberStringProperty); }
            set { SetValue(NumberStringProperty, value); }
        }

        public static readonly DependencyProperty NumberStringProperty =
            DependencyProperty.Register("NumberString", typeof(string), typeof(TabHeader), new UIPropertyMetadata(""));

        public int Number
        {
            get
            {
                if (string.IsNullOrEmpty(NumberString)) return 0;
                else return int.Parse(NumberString.Substring(1, NumberString.Length - 2));
            }
            set
            {
                if (value == 0) NumberString = "";
                else NumberString = "(" + value.ToString() + ")";
            }
        }

        public bool IsWarning
        {
            get { return (bool)GetValue(IsWarningProperty); }
            set { SetValue(IsWarningProperty, value); }
        }

        public static readonly DependencyProperty IsWarningProperty =
            DependencyProperty.Register("IsWarning", typeof(bool), typeof(TabHeader), new UIPropertyMetadata(false));

        public FrameworkElement ArmModule { get; set; }

        static Popup popup = null;

        public string ModuleDrawingBrushName { get; set; }

        private void tabHeader_MouseEnter(object sender, MouseEventArgs e)
        {
            if ((!IsChecked.GetValueOrDefault()) && Manager.Config.ShowPreviews)
            {
                var preview = new TabPreview() { Frame = ArmModule };
                popup = new Popup() { Child = preview, PlacementTarget = this, AllowsTransparency = true };
                popup.IsOpen = true;
            }
        }

        private void tabHeader_MouseLeave(object sender, MouseEventArgs e)
        {
            if (popup != null)
            {
                popup.IsOpen = false;
                popup = null;
            }
        }

        public bool IsSplit = false;

        bool closing = false;

        private void GetModuleSettings(FrameworkElement fe)
        {
            var settings = VisualEx.GetModuleSettings(fe);
            if (settings != null) Manager.Config.SaveModulesSettingsCompressed(fe.GetType().Name, settings);
        }

        public CloseAction Close()
        {
            tabHeader_MouseLeave(null, null);
            var module = ArmModule as IModule;
            if (module != null)
            {
                var fe = module as FrameworkElement;
                if (fe != null) GetModuleSettings(fe);

                var answer = module.Close();
                if (answer != CloseAction.CancelClose)
                {
                    closing = true;
                    Manager.UI.CloseAllPopups();
                    PopupBase.CloseAllPopups();
                    var tabs = this.Parent as StackPanel;
                    if (tabs != null)
                    {
                        var workPage = WorkPage.CurrentPage;

                        for (int i = 0; i < tabs.Children.Count; i++)
                        {
                            var tab = tabs.Children[i];
                            if (tab == this)
                            {
                                var page = workPage as WorkPage;
                                if (page != null) page.tabs.Tabs.RemoveAt(i);

                                if (i == tabs.Children.Count) i--;
                                if (i == -1)
                                {
                                    WorkPage.ActiveHeader = null;
                                    break;
                                }
                                Common.Manager.UI.SetActiveTab(tabs.Children[i]);
                                break;
                            }
                        }
                        ArmModule.Visibility = Visibility.Hidden;
                        if (answer == CloseAction.HideToHistory)
                        {
                            //Прячем в историю
                            var history = (WorkPage.CurrentPage as WorkPage).HistoryContent;
                            if (history != null)
                            {
                                history.Children.Insert(0, WorkPage.BuildShortcutFromModule(module, ArmModule.DataContext, Caption.ToString(), ModuleDrawingBrushName, false));
                            }
                        }
                        //else
                        //{
                        //Полное закрытие
                        var tc = workPage.FindName("tabContainer") as Grid;
                        if (tc != null) tc.Children.Remove(ArmModule);
                        try
                        {
                            if (ArmModule != null) ArmModule.DataContext = null;

                            var disp = ArmModule as IDisposable;
                            if (disp != null)
                            {
                                disp.Dispose();
                            }
                        }
                        catch { }

                        WorkPage.InitializedModules.Remove(module);
                        LastClosed = null;
                        //}
                        this.ArmModule = null;

                    }
                }
                if (Header.AllTabs != null)
                {
                    var item = Header.AllTabs.List.First(i => i.Key == this);
                    Header.AllTabs.List.Remove(item);
                }

                GC.Collect();

                return answer;
            }
            return CloseAction.Сlose;
        }

        public static object LastClosed = null;

        private void closeAll_Click(object sender, EventArgs e)
        {
            tabHeader_MouseDown(null, null);
            var header = WorkPage.CurrentPage.FindName("tabs") as Header;
            if (header != null)
            {
                var history = (WorkPage.CurrentPage as WorkPage).tabs.Tabs;





                var count = history.Count - 1;
                for (var index = count; index >= 0; index--)
                {
                    var historyChild = history[index];

                    historyChild.Key.Close();

                }



            }
        }

        private void closeExcept_Click(object sender, EventArgs e)
        {
            tabHeader_MouseDown(null, null);
            var header = WorkPage.CurrentPage.FindName("tabs") as Header;
            if (header != null)
            {
                var history = (WorkPage.CurrentPage as WorkPage).tabs.Tabs;



                var count = history.Count - 1;
                for (var index = count; index >= 0; index--)
                {
                    var historyChild = history[index];
                    if (!historyChild.Key.Equals(this))
                        historyChild.Key.Close();

                }



            }
        }

        private void addToFavourites_Click(object sender, EventArgs e)
        {
            Manager.UI.AddToFavourites();
        }

        private void copyToClipboard_Click(object sender, EventArgs e)
        {
            Manager.UI.CopyShortcutToClipboard();
        }

        void butClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void tabHeader_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!closing)
            {
                tabHeader_MouseLeave(null, null);
                // проверяем состояние нажатия Ctrl
                var ctrl = Keyboard.GetKeyStates(Key.LeftCtrl);
                if ((ctrl & KeyStates.Down) > 0)
                {
                    if (WorkPage.CurrentSplit != null)
                    {
                        IsChecked = false;
                        Manager.UI.ShowMessage("Объединить можно только 2 закладки!");
                        return;
                    }
                    // создание Split-окна
                    var tabContainer = WorkPage.CurrentPage.FindName("tabContainer") as Grid;
                    tabContainer.Children.Remove(WorkPage.ActiveHeader.ArmModule);
                    tabContainer.Children.Remove(ArmModule);
                    IsSplit = WorkPage.ActiveHeader.IsSplit = true;
                    ArmModule.Visibility = Visibility.Visible;
                    WorkPage.CurrentSplit = new SplitHost();
                    IsChecked = true;
                    WorkPage.CurrentSplit.LeftPanel.Content = WorkPage.ActiveHeader.ArmModule;
                    WorkPage.CurrentSplit.LeftPanel.Title = WorkPage.ActiveHeader.Caption.ToString();
                    WorkPage.CurrentSplit.RightPanel.Content = ArmModule;
                    WorkPage.CurrentSplit.RightPanel.Title = Caption.ToString();
                    var grid = WorkPage.CurrentPage.FindName("tabContainer") as Grid;
                    if (grid != null)
                        grid.Children.Add(WorkPage.CurrentSplit);
                }
                else
                {
                    IsWarning = false;
                    try
                    {
                        Common.Manager.UI.SetActiveTab(this);
                    }
                    catch
                    {
                    }
                }
            }
            else closing = false;
        }

        private void caption_Loaded(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBlock;
            tb.Text = Caption.ToString();
        }

        private void moduleName_Loaded(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBlock;
            if (tb == null) return;

            IModule m = ArmModule as IModule;
            if (m != null)
            {
                string s = m.GetModuleToolTip;
                tb.Text = !String.IsNullOrEmpty(s) ? s : Manager.Modules.GetModuleName(m.ModuleType);
            }

            tb.Visibility = string.IsNullOrEmpty(tb.Text) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void uc_MouseLeave(object sender, MouseEventArgs e)
        {
#if DEBUG
            //if (Mouse.LeftButton == MouseButtonState.Pressed && this.Parent != null)
            //{
            //    try
            //    {
            //        Manager.UI.CloseAllPopups();
            //        PopupBase.CloseAllPopups();
            //        var tabs = this.Parent as StackPanel;
            //        for (int i = 0; i < tabs.Children.Count; i++)
            //        {
            //            var tab = tabs.Children[i];
            //            if (tab == this)
            //            {
            //                tabs.Children.RemoveAt(i);
            //                if (i == tabs.Children.Count) i--;
            //                if (i == -1)
            //                {
            //                    WorkPage.ActiveHeader = null;
            //                    break;
            //                }
            //                Common.Manager.UI.SetActiveTab(tabs.Children[i]);
            //                break;
            //            }
            //        }
            //        var tc = WorkPage.CurrentPage.FindName("tabContainer") as Grid;
            //        if (tc != null) tc.Children.Remove(ArmModule);
            //        var startPoint = WorkPage.CurrentPage.PointToScreen(new Point(50, 50));
            //        var window = new Window()
            //        {
            //            Title = Caption.ToString(),
            //            Content = ArmModule,
            //            Width = WorkPage.CurrentPage.ActualWidth - 200,
            //            Height = WorkPage.CurrentPage.ActualHeight - 200,
            //            WindowStartupLocation = WindowStartupLocation.Manual
            //        };
            //        window.Show();
            //        window.Left = startPoint.X;
            //        window.Top = startPoint.Y;
            //    }
            //    catch (Exception) { }
            //}
#endif
        }

        private void CommandBindingOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e == null || e.Parameter == null) return;

            var ms = ArmModule as IModuleLocalSettings;
            if (ms == null)
            {
                return;
            }

            switch (e.Parameter.ToString())
            {
                case "SaveLocalSettings":
                    ms.SaveLocalSettings();
                    break;
                case "ClearLocalSettings":
                    ms.ClearLocalSettings();
                    break;
            }
        }

        private void CommandBindingOnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var ms = ArmModule as IModuleLocalSettings;
            if (ms == null)
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = true;
        }
    }
}
