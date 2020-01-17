using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Proryv.AskueARM2.Client.Visual.Common;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Infragistics.Windows.Controls;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using Proryv.AskueARM2.Client.ServiceReference.Service;

namespace Proryv.AskueARM2.Client.Visual
{
    /// <summary>
    /// Элемент истории
    /// </summary>
    public partial class HistoryShortcut : INotifyPropertyChanged, IDisposable
    {
        public HistoryShortcut()
        {
            InitializeComponent();
            HistoryShortcut.HistoryContextMenu(this,new DependencyPropertyChangedEventArgs(ShowHistoryContextMenuProperty, ShowHistoryContextMenu, ShowHistoryContextMenu));
        }

        public string Caption
        {
            get { return caption.Text; }
            set { caption.Text = value; }
        }


        public static readonly DependencyProperty ShowHistoryContextMenuProperty = DependencyProperty.Register(
            "ShowHistoryContextMenu", typeof(bool), typeof(HistoryShortcut), new PropertyMetadata(HistoryContextMenu));

        private static void HistoryContextMenu(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var shortcut = d as HistoryShortcut;
            if (shortcut != null)
            {
                if (!(bool)e.NewValue)
                {
                    shortcut.DeleteThisFromHistoryButton.Visibility = Visibility.Visible;
                    shortcut.DeleteFromHistoryButton.Visibility = Visibility.Visible;
                    shortcut.closeAll.Visibility = Visibility.Collapsed;
                    shortcut.closeExcept.Visibility = Visibility.Collapsed;
                    shortcut.addToFavourites.Visibility = Visibility.Collapsed;
                    shortcut.copyToClipboard.Visibility = Visibility.Collapsed;
                }
                else
                {
                    shortcut.DeleteThisFromHistoryButton.Visibility = Visibility.Collapsed;
                    shortcut.DeleteFromHistoryButton.Visibility = Visibility.Collapsed;
                    shortcut.closeAll.Visibility = Visibility.Visible;
                    shortcut.closeExcept.Visibility = Visibility.Visible;
                    shortcut.addToFavourites.Visibility = Visibility.Visible;
                    shortcut.copyToClipboard.Visibility = Visibility.Visible;
                }





            }
        }

        public bool ShowHistoryContextMenu
        {
            get { return (bool)GetValue(ShowHistoryContextMenuProperty); }
            set { SetValue(ShowHistoryContextMenuProperty, value); }
        }


        public static readonly DependencyProperty CloseButtonVisibilityProperty = DependencyProperty.Register(
            "CloseButtonVisibility", typeof(Visibility), typeof(HistoryShortcut), new PropertyMetadata(System.Windows.Visibility.Collapsed));

        public Visibility CloseButtonVisibility
        {
            get { return (Visibility)GetValue(CloseButtonVisibilityProperty); }
            set { SetValue(CloseButtonVisibilityProperty, value); }
        }


        public static readonly DependencyProperty TabHeaderProperty = DependencyProperty.Register(
            "TabHeader", typeof(TabHeader), typeof(HistoryShortcut), new PropertyMetadata(TabHeaderProrpertyChanged));

        private static void TabHeaderProrpertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var shortcut = d as HistoryShortcut;
            if (shortcut != null)
            {
                var header = e.NewValue as TabHeader;
                if (header != null)
                {
                    shortcut.Caption = header.Caption.ToString();


                }

            }

        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void _PropertyChanged(string property)
        {
            if (PropertyChanged != null && !DesignerProperties.GetIsInDesignMode(this))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion

        public TabHeader TabHeader
        {
            get { return (TabHeader)GetValue(TabHeaderProperty); }
            set { SetValue(TabHeaderProperty, value); }
        }

        public object Frame;

        #region Свойства для восстановления из истории

        public string Shortcut;

        public string ActionAfterRestore;

        public List<TTypedParam> ConstructorParams;

        public TTypedParam DataContextParam;


        public static readonly DependencyProperty ModuleTypeProperty = DependencyProperty.Register(
            "ModuleType", typeof(ModuleType), typeof(HistoryShortcut), new PropertyMetadata(ModuleType.None));

        //private ModuleType _moduleType;
        public ModuleType ModuleType
        {
            get { return (ModuleType)GetValue(ModuleTypeProperty); }
            set { SetValue(ModuleTypeProperty, value); }
            //get { return _moduleType; }
            //set
            //{
            //    if (value == _moduleType) return;
            //    _moduleType = value;
            //    _PropertyChanged("ModuleType");
            //}
        }


        public static readonly DependencyProperty ModuleDrawingBrushNameProperty = DependencyProperty.Register(
            "ModuleDrawingBrushName", typeof(string), typeof(HistoryShortcut));

        public string ModuleDrawingBrushName
        {
            get { return GetValue(ModuleDrawingBrushNameProperty) as string; }
            set { SetValue(ModuleDrawingBrushNameProperty, value); }
        }

        #endregion

        public static HistoryShortcut LastClicked = null;

        private void UserControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Parent != null)
                {
                    (Parent as StackPanel).Children.Remove(this);
                    caption_MouseLeave(this, null);

                    try
                    {
                        WorkPage.RestoreModuleFromShortcut(new HistoryShortcut
                        {
                            Caption = Caption,
                            Shortcut = Shortcut,
                            ActionAfterRestore = ActionAfterRestore,
                            ConstructorParams = ConstructorParams,
                            DataContextParam = DataContextParam,
                            ModuleType = ModuleType,
                            ModuleDrawingBrushName = ModuleDrawingBrushName,
                        });
                    }
                    catch (Exception ex)
                    {
                        Manager.UI.ShowMessage(ex.Message);
                    }

                    //Manager.UI.AddTab(Caption, Frame as FrameworkElement);
                }
            }
            else LastClicked = this;
        }

        private Popup _popup;

        private void caption_MouseEnter(object sender, MouseEventArgs e)
        {
            var fe = Frame as FrameworkElement;
            if (Manager.Config.ShowPreviews && fe!=null)
            {
                var preview = new TabPreview
                {
                    Frame = fe
                };

                _popup = new Popup
                {
                    Child = preview,
                    PlacementTarget = this,
                    Placement = PlacementMode.Right,
                    AllowsTransparency = true,
                    IsOpen = true
                };
            }
        }

        private void caption_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_popup != null)
            {
                _popup.IsOpen = false;
                _popup = null;
            }
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            if (TabHeader != null)
                TabHeader.Close();
        }

        private void closeAll_Click(object sender, RoutedEventArgs e)
        {

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

        private void closeExcept_Click(object sender, RoutedEventArgs e)
        {

            var header = WorkPage.CurrentPage.FindName("tabs") as Header;
            if (header != null)
            {
                var history = (WorkPage.CurrentPage as WorkPage).tabs.Tabs;


                var count = history.Count - 1;
                for (var index = count; index >= 0; index--)
                {
                    var historyChild = history[index];
                    if (!historyChild.Key.Equals(this.TabHeader))
                        historyChild.Key.Close();

                }


            }
        }

        private void addToFavourites_Click(object sender, RoutedEventArgs e)
        {
            Manager.UI.AddToFavourites();
        }

        private void copyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Manager.UI.CopyShortcutToClipboard();
        }

        private void DeleteThisFromHistory(object sender, RoutedEventArgs e)
        {
            var stackPanel = Parent as StackPanel;
            if (stackPanel != null) stackPanel.Children.Remove(this);
        }


        private void DeleteFromHistoryButton_OnClick(object sender, RoutedEventArgs e)
        {
            var stackPanel = Parent as StackPanel;
            if (stackPanel != null) 
                for (int i = stackPanel.Children.Count-1; i >= 0; i--)
                {
                    if (stackPanel.Children[i] != this)
                        stackPanel.Children.Remove(stackPanel.Children[i]);
                }
        }

        public void Dispose()
        {
            Frame = null;
            _popup = null;
        }
    }
}
