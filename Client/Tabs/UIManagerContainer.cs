using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Infragistics.Controls.Interactions;
using Proryv.AskueARM2.Client.Visual;
using Proryv.AskueARM2.Client.Visual.Common;
using WindowState = Infragistics.Controls.Interactions.WindowState;
using System.ComponentModel;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using Proryv.AskueARM2.Client.ServiceReference.Interfaces;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;

namespace Proryv.ElectroARM.Controls.Controls.Tabs
{
    /// <summary>
    ///     Интерфейс помечает локальный IModuleManager
    /// </summary>
    public interface ILocalUIManager
    {
        /// <summary>
        ///     Открыть диалог внутри самого близкого контейнера ILocalUIManager
        /// </summary>
        /// <param name="content">Содержимое модального диалога</param>
        /// <param name="title">Заголовок модального окна</param>
        /// <param name="isResizable">Содержимое может менять размер</param>
        /// <param name="isSpan">Содержимое на весь грид</param>
        /// <param name="isNotCloseOnEscapeButton">Не закрывать диалог при нажатии ESC</param>
        void ShowLocalModal(FrameworkElement content, object title, bool isResizable = true, bool isSpan = false, bool isNotCloseOnEscapeButton = false, bool allowCloseWindow=true);

        /// <summary>
        ///     Закрыть диалог внутри контейнера ILocalUIManager
        /// </summary>
        /// <param name="completeMethod">Метод после закрытия</param>
        /// <param name="dialog">Какой диалог закрываем</param>
        void CloseLocalModal(Action completeMethod, FrameworkElement dialog);

        /// <summary>
        ///     Закрыть все диалоговые окна в контейнере
        /// </summary>
        void CloseAllLocalModal();

        /// <summary>
        /// Показать или обновить окно с сообщениями в контейнере
        /// </summary>
        void ShowOrUpdateMessage(string message, string title, Action onClose, bool isShowDateTime);

        void ShowYesNoDialog(string message, Action yesMethod);
    }

    /// <summary>
    ///     Interaction logic for UIManagerContainer.xaml
    /// </summary>
    //[ContentProperty("Content")]
    public class UIManagerContainer : Grid, IDisposable, IModule, ILocalUIManager
    {
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", typeof(object), typeof(UIManagerContainer), new PropertyMetadata(default(object)));

        private readonly Frame ShellFrame = new Frame();

        private IModule module;
        public ConcurrentDictionary<Guid, XamDialogWindow> DialogWindowsCollection = new ConcurrentDictionary<Guid, XamDialogWindow>();

        /// <summary>
        /// Смещение окна с сообщениями относительно границ
        /// </summary>
        private const double WidthMargin = 100, HeightMargin = 50;

        private XamDialogWindow _messageWindow;

        private bool _isNotCloseOnEscapeButton;

        public UIManagerContainer(FrameworkElement framecontent)
        {
            Content = framecontent;
            module = framecontent as IModule;
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            Loaded += TabFrameDialog_Loaded;
        }


        public UIManagerContainer()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            Loaded += TabFrameDialog_Loaded;
        }

        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public SynchronizationContext UiSynchronizationContext { get; private set; }

        public void Dispose()
        {
            SizeChanged -= OnParentSizeChanged;

            CloseAllLocalModal();
            foreach (object child in Children)
            {
                if (child is IDisposable)
                {
                    (child as IDisposable).Dispose();
                }
            }
            Children.Clear();

            try
            {
                module = null;
            }
            catch
            {
            }
        }

        public void ShowLocalModal(FrameworkElement content, object title, bool isResizable = true, bool isSpan = false, bool isNotCloseOnEscapeButton = false, bool allowCloseWindow = true)
        {
            var children = content as ILocalChildren;
            System.Windows.Media.Brush modalBackground = null;
            _isNotCloseOnEscapeButton = isNotCloseOnEscapeButton;
            var isTrueModal = false;
            var id = Guid.Empty;
            if (children != null)
            {
                if (children.Id == Guid.Empty) children.Id = Guid.NewGuid();
                id = children.Id;

                if (children.LocalModalType != EnumLocalModalType.None)
                {
                    //Проверка открыто ли окно с таким содержимым
                    if (children.LocalModalType == EnumLocalModalType.UniqueById)
                    {
                        XamDialogWindow dw;
                        if (DialogWindowsCollection.TryGetValue(id, out dw) && dw != null && dw.WindowState != WindowState.Hidden) return;
                        //Изменен тип коллекции, но логика сохранена
                        //if (dw.WindowState == WindowState.Hidden) return false;
                        //if (ch == null) return false;
                        //return ch.LocalModalType == EnumLocalModalType.UniqueById && ch.Id == children.Id;
                    }

                    if (children.LocalModalType == EnumLocalModalType.TrueModal)
                    {
                        modalBackground = System.Windows.Media.Brushes.LightSteelBlue;
                        isTrueModal = true;
                    }
                }
            }

            if (id == Guid.Empty) id = Guid.NewGuid();

            var xamDialogWindow = new XamDialogWindow
            {
                StartupPosition = StartupPosition.Center,
                ModalBackground = modalBackground,
                Header = title,
                IsModal = true,
                IsResizable = isResizable,
                Content = content,
                Tag = id,
                CloseButtonVisibility = allowCloseWindow? Visibility.Visible: Visibility.Collapsed
            };

            if (isSpan)
            {
                if (RowDefinitions.Count > 0) SetRowSpan(xamDialogWindow, RowDefinitions.Count);
                if (ColumnDefinitions.Count > 0) SetColumnSpan(xamDialogWindow, ColumnDefinitions.Count);
            }

            Children.Add(xamDialogWindow);
            
            xamDialogWindow.WindowStateChanged += s_WindowStateChanged;
            if (isTrueModal)
            {
                xamDialogWindow.IsActiveChanged += xamDialogWindow_IsActiveChanged;
            }

            xamDialogWindow.Loaded += XamDialogWindow_Loaded;
            xamDialogWindow.Unloaded += XamDialogWindow_Unloaded;

            xamDialogWindow.Show();
            
            if (!DialogWindowsCollection.IsEmpty)
            {
                var prevWindow = DialogWindowsCollection.Values.Last();
                if (prevWindow != null && prevWindow.WindowState != WindowState.Hidden)
                {
                    xamDialogWindow.Margin = new Thickness(prevWindow.Margin.Left + 6, prevWindow.Margin.Top + 12, 0, 0);
                }
            }

            if (isResizable)
            {
               xamDialogWindow.SizeChanged += OnDwSizeChanged;
               
            }
            else if (DialogWindowsCollection.IsEmpty)
            {
                SizeChanged += OnParentSizeChanged;
            }

            DialogWindowsCollection.TryAdd(id, xamDialogWindow);

            xamDialogWindow.Dispatcher.BeginInvoke((Action) (() => { xamDialogWindow.CenterDialogWindow(); }),
                DispatcherPriority.Background);
        }
        
        #region Если запрещено автоматически менять размер, то оставляем его минимальным

        private DispatcherTimer _autoParentSizeTimer;

        /// <summary>
        /// Заплатка чтобы окно не сжималось
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnParentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DialogWindowsCollection.IsEmpty || DialogWindowsCollection.Count > 1) return;

            if (_autoParentSizeTimer == null)
            {
                _autoParentSizeTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(200), DispatcherPriority.Normal,
                    AutosizeFromParentContent, this.Dispatcher);
            }

            _autoParentSizeTimer.Start();
        }


        private void AutosizeFromParentContent(object sender, EventArgs e)
        {
            _autoParentSizeTimer.Stop();

            foreach (var dw in DialogWindowsCollection.Values)
            {
                if (dw == null || dw.WindowState == WindowState.Minimized || dw.WindowState == WindowState.Hidden) continue;

                var content = dw.Content as FrameworkElement;
                if (content == null) continue;

                try
                {
                    if (!double.IsNaN(content.MinWidth) && !double.IsInfinity(content.MinWidth) && content.MinWidth > 0)
                    {
                        if (dw.ActualWidth < content.MinWidth)
                        {
                            dw.Width = content.MinWidth + 30;
                        }
                    }

                    if (!double.IsNaN(content.MinHeight) && !double.IsInfinity(content.MinHeight) && content.MinHeight > 0)
                    {
                        if (dw.ActualHeight < content.MinHeight)
                        {
                            dw.Height = content.MinHeight + 30;
                        }
                    }

                    Dispatcher.BeginInvoke((Action)(() => dw.CenterDialogWindow()), DispatcherPriority.Normal);
                }
                catch (Exception)
                {
                }
            }
        }

        #endregion

        #region Подстраиваем содержимое под диалоговое окно

        private ConcurrentDictionary<Guid, DispatcherTimer> _autoSizeTimerCollection = new ConcurrentDictionary<Guid, DispatcherTimer>();

        private void OnDwSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var dw = sender as XamDialogWindow;
            if (dw == null || dw.Tag == null || dw.WindowState == WindowState.Minimized ||
                dw.WindowState == WindowState.Hidden || !dw.IsLoaded) return;

            Guid id;
            if (!Guid.TryParse(dw.Tag.ToString(), out id)) return;

            DispatcherTimer autoSizeTimer;
            if (!_autoSizeTimerCollection.TryGetValue(id, out autoSizeTimer) || autoSizeTimer == null)
            {
                autoSizeTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(250), DispatcherPriority.Loaded,
                    AutosizeContent, this.Dispatcher) {Tag = id};
                _autoSizeTimerCollection.TryAdd(id, autoSizeTimer);
            }

            autoSizeTimer.Start();
        }


        private void AutosizeContent(object sender, EventArgs e)
        {
            var autoSizeTimer = sender as DispatcherTimer;
            if (autoSizeTimer == null || autoSizeTimer.Tag == null) return;

            autoSizeTimer.Stop();

            Guid id;
            if (!Guid.TryParse(autoSizeTimer.Tag.ToString(), out id)) return;

            XamDialogWindow dw;
            if (!DialogWindowsCollection.TryGetValue(id, out dw) || null == dw) return;

            var content = dw.Content as FrameworkElement;
            if (content == null) return;

            try
            {
                if (dw.ActualWidth - WidthMargin > 50) content.Width = dw.ActualWidth - 22;
                if (dw.ActualHeight - HeightMargin > 50) content.Height = dw.ActualHeight - 52;
            }
            catch (Exception)
            {
            }
        }

        #endregion

        private void xamDialogWindow_IsActiveChanged(object sender, EventArgs e)
        {
            var xdw = sender as XamDialogWindow;
            if (xdw != null)
            {
                xdw.BringToFront();
            }
        }

        private void XamDialogWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var xw = sender as XamDialogWindow;
            if (xw == null) return;

            xw.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(XamDialogWindow_KeyDown), false);
        }

        private void XamDialogWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            var dw = sender as XamDialogWindow;
            if (dw == null) return;

            dw.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(XamDialogWindow_KeyDown));
        }

        private void XamDialogWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_isNotCloseOnEscapeButton && e.Key == Key.Escape)
            {
                var xw = sender as XamDialogWindow;
                if (xw == null) return;

                xw.Close();
            }
        }

        public void CloseLocalModal(Action completeMethod, FrameworkElement dialog)
        {
            if (completeMethod != null) completeMethod.Invoke();
            CloseAllLocalModal(dialog);
        }

        public void CloseAllLocalModal()
        {
            CloseAllLocalModal(null);

        }

        public void CloseAllLocalModal(FrameworkElement dialog = null)
        {
            if (dialog != null && dialog.Parent is XamDialogWindow)
            {
                (dialog.Parent as XamDialogWindow).Close();
                return;
            }

            foreach (var dw in DialogWindowsCollection.Values.ToList())
            {
                try
                {
                    ClearEvents(dw);
                }
                catch
                {
                }
            }
        }

        private void ClearEvents(XamDialogWindow dw)
        {
            if (dw.Tag != null)
            {
                Guid id;
                if (!Guid.TryParse(dw.Tag.ToString(), out id)) return;

                XamDialogWindow xw;
                DialogWindowsCollection.TryRemove(id, out xw);
            }

            dw.SizeChanged -= OnDwSizeChanged;
            dw.Loaded -= XamDialogWindow_Loaded;
            dw.Unloaded -= XamDialogWindow_Unloaded;
            dw.IsActiveChanged -= xamDialogWindow_IsActiveChanged;
            dw.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(XamDialogWindow_KeyDown));

            var closable = (dw.Content as IClosebleContent);
            if (closable != null && closable.Close != null)
            {
                closable.Close.Invoke();
            }

            var children = dw.Content as ILocalChildren;
            if (children != null)
            {
                if (children.OnClose != null) children.OnClose.Invoke();
            }

            var d = dw.Content as IDisposable;
            if (d != null) d.Dispose();

            dw.Content = null;

            if (object.Equals(dw, _messageWindow)) _messageWindow = null;
            Children.Remove(dw);
        }
    

        public ModuleType ModuleType { get; private set; }

        public bool Init()
        {
            if (module != null)
            {
                return module.Init();
            }
            return true;
        }

        public CloseAction Close()
        {
            if (module != null)
            {
                return module.Close();
            }
            return CloseAction.Сlose;
        }

        public string Shortcut
        {
            get
            {
                if (module != null)
                {
                    return module.Shortcut;
                }
                return string.Empty;
            }
            set
            {
                if (module != null)
                {
                    module.Shortcut = value;
                }
            }
        }

        public FreeHierarchyTreeItem GetNode(IFreeHierarchyObject hierarchyObject)
        {
            var pe = Parent as FrameworkElement;
            if (pe == null) return null;

            if (pe is IModule && !(pe is UIManagerContainer)) return (pe as IModule).GetNode(hierarchyObject);

            var m = pe.FindTrueIModule() as IModule;
            if (m != null) return m.GetNode(hierarchyObject);

            return null;
        }

        public string GetModuleToolTip
        {
            get
            {
                if (module != null)
                {
                    return module.GetModuleToolTip;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Что запускаем после восстановления из истории
        /// </summary>
        public string ActionAfterRestore { get; set; }

        public List<TTypedParam> ConstructorParams { get; set; }
        public string IconPath { get; set; }

        private void TabFrameDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (SynchronizationContext.Current != null) UiSynchronizationContext = SynchronizationContext.Current;
            Loaded -= TabFrameDialog_Loaded;
            ShellFrame.Content = Content;
            Children.Add(ShellFrame);
        }


        private void s_WindowStateChanged(object sender, WindowStateChangedEventArgs e)
        {
            if (e.NewWindowState != WindowState.Hidden) return;
            var dw = sender as XamDialogWindow;
            if (dw == null) return;

            try
            {
                ClearEvents(dw);
            }
            catch
            {
            }
        }

        public void ShowOrUpdateMessage(string message, string title, Action onClose, bool isShowDateTime)
        {
            var currPage = WorkPage.CurrentPage;
            if (currPage == null) return;

            if (_messageWindow == null)
            {
                _messageWindow = new XamDialogWindow
                {
                    StartupPosition = StartupPosition.Center,
                    ModalBackground = System.Windows.Media.Brushes.LightSteelBlue,
                    ModalBackgroundOpacity = 0.5,
                    Header = title,
                    HeaderIconVisibility = Visibility.Collapsed,
                    IsModal = true,
                    IsResizable = false,
                };

                SetRowSpan(_messageWindow, 10);
                SetColumnSpan(_messageWindow, 10);

                if (Children != null) Children.Add(_messageWindow);
                _messageWindow.WindowStateChanged += s_WindowStateChanged;
            }

            var messagePanel = _messageWindow.Content as MessagePanel;
            if (messagePanel == null)
            {
                messagePanel = new MessagePanel(onClose);
                _messageWindow.Content = messagePanel;
            }

            messagePanel.AddOrUpdateMessage(message, isShowDateTime);

            if (_messageWindow.WindowState == WindowState.Hidden)
            {
                _messageWindow.Show();
            }

            _messageWindow.BringToFront();
            _messageWindow.IsActive = true;

            if (_messageWindow.Dispatcher == null) return;

            _messageWindow.Dispatcher.BeginInvoke((Action)(() =>
           {
               if (_messageWindow == null) return;

               var mp = _messageWindow.Content as MessagePanel;
               if (mp == null) return;

               var lv = mp.FindName("lvMessages") as ListView;
               if (lv == null) return;


               //Подогнать размер под содержимое
               //lv.UpdateLayout();
               //messagePanel.UpdateLayout();
               //_messageWindow.Height = lv.ActualHeight + 90;
               //_messageWindow.Width = lv.ActualWidth + 30;
               _messageWindow.UpdateLayout();

               _messageWindow.CenterDialogWindow();

           }), DispatcherPriority.Background);
        }

        public void ShowYesNoDialog(string message, Action yesMethod)
        {
            var currPage = WorkPage.CurrentPage;
            if (currPage == null) return;

            if (_messageWindow == null)
            {
                _messageWindow = new XamDialogWindow
                {
                    StartupPosition = StartupPosition.Center,
                    ModalBackground = System.Windows.Media.Brushes.LightSteelBlue,
                    ModalBackgroundOpacity = 0.3,
                    IsModal = true,
                    IsResizable = true,
                };

                SetRowSpan(_messageWindow, 10);
                SetColumnSpan(_messageWindow, 10);

                Children.Add(_messageWindow);
                _messageWindow.WindowStateChanged += s_WindowStateChanged;
                //_messageWindow.IsActiveChanged += xamDialogWindow_IsActiveChanged;
            }

            var messagePanel = _messageWindow.Content as MessagePanel;
            if (messagePanel == null)
            {
                try
                {
                    messagePanel = new MessagePanel(yesMethod, true);
                    _messageWindow.Content = messagePanel;
                }
                catch
                {

                }
            }

            if (messagePanel == null) return;

            messagePanel.AddOrUpdateMessage(message, false);

            if (_messageWindow.WindowState == WindowState.Hidden)
            {
                _messageWindow.Show();
                _messageWindow.CenterDialogWindow();
            }

            _messageWindow.BringToFront();
            _messageWindow.IsActive = true;

            _messageWindow.Dispatcher.BeginInvoke((Action)(() =>
            {
                var mp = _messageWindow.Content as MessagePanel;
                if (mp == null) return;

                var lv = mp.FindName("lvMessages") as ListView;
                if (lv == null) return;


                //Подогнать размер под содержимое
                _messageWindow.Height = lv.ActualHeight + 90;
                //_messageWindow.Width = lv.ActualWidth + 30;
                _messageWindow.UpdateLayout();

            }), DispatcherPriority.Background);
        }
    }

    /// <summary>
    /// Для модальных диалогов, и для отображения поиска внутри
    /// </summary>
    public interface ILocalChildren
    {
        Action OnClose { get; set; }
        EnumLocalModalType LocalModalType { get; }
        Guid Id { get; set; }
    }

    /// <summary>
    /// Как открываем модальное окно
    /// </summary>
    public enum EnumLocalModalType : byte
    {
        /// <summary>
        /// Можно открывать повторно
        /// </summary>
        None = 0,
        /// <summary>
        /// Уникальность гаранирована идентификатором
        /// </summary>
        UniqueById = 1,
        /// <summary>
        /// Настоящее модальное окно (запрет на открытие других модальных окон в рамках родителя)
        /// </summary>
        TrueModal = 2,
    }
}