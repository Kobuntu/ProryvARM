using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Infragistics.Controls.Interactions;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Action = System.Action;

namespace Proryv.AskueARM2.Client.Visual
{
    /// <summary>
    /// Interaction logic for ShowMessagePanel.xaml
    /// </summary>
    public partial class MessagePanel :IDisposable
    {
        const double WidthMargin = 30, HeightMargin = 50;

        public MessagePanel(Action onClose, bool isYesNoDialog = false)
        {
            InitializeComponent();
            lvMessages.ItemsSource = Messages = new ConcurrentObservableCollection<TMessage>();
            Close = onClose;

            if (isYesNoDialog)
            {
                bYes.Content = "Да";
                bNo.Visibility = Visibility.Visible;
            }
        }

        public ConcurrentObservableCollection<TMessage> Messages { get; set; }

        public void AddOrUpdateMessage(string message, bool isShowDateTime)
        {
            if (string.IsNullOrEmpty(message)) return;

            Messages.Insert(0, new TMessage
                {
                    EventDateTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"),
                    Message = message,
                });

        }

        public Action Close;

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            if (Close!=null) Close.Invoke();

            var parent = this.FindParent<XamDialogWindow>();
            if (parent == null) return;

            parent.Close();
        }

        private void copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(string.Join("\r\n", Messages.Select(m => m.EventDateTime + " " + m.Message)));
        }

        private void LvMessages_OnPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var contextTemplate = Resources["popup"] as DataTemplate;
            if (contextTemplate == null) return;

            var contextMenu = contextTemplate.LoadContent() as ContextMenu;
            if (contextMenu == null) return;

            contextMenu.PlacementTarget = lvMessages;
            contextMenu.IsOpen = true;
        }

        private void BNoOnClick(object sender, RoutedEventArgs e)
        {
            var parent = this.FindParent<XamDialogWindow>();
            if (parent == null) return;

            parent.Close();
        }

        public void Dispose()
        {
            Close = null;
            lvMessages.ClearItems();
            _top = null;
        }

        FrameworkElement _top;

        private void MessagePanelLoaded(object sender, RoutedEventArgs e)
        {
            Init();

            Dispatcher.BeginInvoke((Action) (() =>
            {
                if (_top == null) return;

                if (ActualWidth > (_top.ActualWidth - WidthMargin))
                {
                    MaxWidth = _top.ActualWidth - WidthMargin;
                }

                if (ActualHeight > (_top.ActualHeight - HeightMargin))
                {
                    MaxHeight = _top.ActualHeight - HeightMargin;
                }

                var dw = this.FindParent<XamDialogWindow>();
                if (dw != null)
                {
                    dw.CenterDialogWindow();
                }

            }), DispatcherPriority.Background);
        }

        private void Init()
        {
            if (_top != null) return;

            var dw = this.FindParent<XamDialogWindow>();
            if (dw != null)
            {
                _top = dw.Parent as FrameworkElement;
            }
            else
            {
                _top = WorkPage.CurrentPage.FindName("topPanel") as FrameworkElement;
            }
        }
    }

    public class TMessage
    {
        public string EventDateTime { get; set; }
        public string Message { get; set; }
    }
}
