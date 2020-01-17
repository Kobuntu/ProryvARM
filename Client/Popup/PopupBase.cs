using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Threading;
using System.Windows.Media;
using Proryv.AskueARM2.Client.Visual.Common;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System;
using Proryv.ElectroARM.Controls.Common.DragAndDrop;

namespace Proryv.AskueARM2.Client.Visual
{
    public class PopupBase : UserControl, IDisposable
    {
        private Rect? _popupBaseRect;
        public Rect PopupBaseRect
        {
            get
            {
                if (_popupBaseRect.HasValue) return _popupBaseRect.Value;

                _popupBaseRect = VisualTreeHelper.GetDescendantBounds(this);
                return _popupBaseRect.Value;
            }
        }
        private bool _staysOpen;
        private bool _dontUseRightClick;

        public static bool AllowPopup = true;
        protected void Init(bool stayOpen = false, bool dontUseRightClick = false)
        {
            if (!Manager.IsDesignMode && AllowPopup)
            {
                //this.Background = Brushes.Transparent;
                MouseEnter += PopupBase_MouseEnter;
                MouseLeave += PopupBase_MouseLeave;
                MouseDown += PopupBase_MouseDown;
            }

            _staysOpen = stayOpen;
            _dontUseRightClick = dontUseRightClick;
        }

        void PopupBase_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isPopupEnabled || (!Manager.Config.IsPopupRightClick && !_dontUseRightClick) || e.RightButton != MouseButtonState.Pressed) return;
            WaiterDelay = 0;
            ShowPopup();
        }

        bool isPopupEnabled = true;

        public bool IsPopupEnabled
        {
            get { return isPopupEnabled; }
            set { isPopupEnabled = value; }
        }

        protected void PopupBase_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
           if (!IsPopupEnabled || (Manager.Config.IsPopupRightClick && !_dontUseRightClick)) return;
           ShowPopup();
        }

        public void ShowPopup()
        {
            //var bind = new Binding {RelativeSource = new RelativeSource {Mode = RelativeSourceMode.FindAncestor, AncestorType = typeof (IModule)}};
            //SetBinding(TagProperty, bind);

            if (WaiterDelay == 0)
            {
                OnEnterPopupTimerCallback(null, EventArgs.Empty);
            }
            else
            {
                if (_showPopupTimer == null)
                {
                    _showPopupTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(WaiterDelay),
                        DispatcherPriority.DataBind, OnEnterPopupTimerCallback, this.Dispatcher);
                }

                _showPopupTimer.Start();

                //Task.Factory.StartNew(() =>
                //{
                //    Thread.Sleep(WaiterDelay);
                //    Dispatcher.BeginInvoke((Action) (popup_waiter_RunWorkerCompleted), DispatcherPriority.Normal);
                //});
            }
        }

        private DispatcherTimer _showPopupTimer;
        protected int WaiterDelay = 300;
        //private Popup _popup;

        private void OnEnterPopupTimerCallback(object sender, EventArgs e)
        {
            if (_showPopupTimer!=null) _showPopupTimer.Stop();

            if ((!IsMouseOver && IsEnabled) || (opened_popups.ContainsKey(this))) return;

            var popupControl = GetPopupControl();
            if (popupControl == null || popupControl.Parent != null) return;
            sourceModule = this.FindTrueIModule() as IModule;
            try
            {
                var popup = new Popup
                {
                    AllowsTransparency = true,
                    PlacementTarget = this,
                    Placement = PlacementMode.MousePoint,
                    HorizontalOffset = -5,
                    VerticalOffset = 7,
                    Child = popupControl,
                    //PopupAnimation = PopupAnimation.Fade,
                    //StaysOpen = false, 
                };
                if (sourceModule == null) popup.PlacementTarget = WorkPage.CurrentPage;
                if (popupControl.DataContext == null)
                    popupControl.DataContext = this.DataContext;

                popup.MouseLeave += popup_MouseLeave;
                popup.Closed += PopupOnClosed;

                var parent = this.FindParent<Popup>();
                popup.Tag = new PopupContainer(popup, parent, this, opened_popups);
                if (parent != null)
                {
                    var ppc = parent.Tag as PopupContainer;
                    if (ppc != null) ppc.AddChild(popup);
                }

                try
                {
                    popup.IsOpen = true;
                }
                catch
                {

                }

                opened_popups.TryAdd(this, popup);
            }
            catch (Exception ex)
            {
                Manager.UI.ShowMessage(ex.Message);
            }
        }

        private void PopupOnClosed(object sender, EventArgs eventArgs)
        {
            var popup = sender as Popup;
            if (popup == null) return;

            var d = popup.Child as IDisposable;
            if (d!=null)
            {
                d.Dispose();
            }

            popup.Closed -= PopupOnClosed;
            popup.MouseLeave -= popup_MouseLeave;
            var target = popup.PlacementTarget as FrameworkElement;
            if (target == null) return;

            var pc = popup.Tag as PopupContainer;
            if (pc == null || pc.PopupBase == null) return;

            Popup removed;
            opened_popups.TryRemove(pc.PopupBase, out removed);
            if (removed == null || pc.Parent == null) return;

            var ppc = pc.Parent.Tag as PopupContainer;
            if (ppc != null) ppc.ChildrenRemoveAndCloseIfNeed(removed);
        }

        private static readonly ConcurrentDictionary<FrameworkElement, Popup> opened_popups = new ConcurrentDictionary<FrameworkElement, Popup>();

        public static bool IsPopupOpen
        {
            get { return !opened_popups.IsEmpty; }
        }

        private static void closeAllPopups(Popup popup = null)
        {
            for (var i = opened_popups.Count - 1; i >= 0; i--)
            {
                var p = opened_popups.ElementAtOrDefault(i);
                if (p.Value == null || !p.Value.IsOpen) continue;

                var pc = p.Value.Tag as PopupContainer;
                if (popup != null && !Equals(popup, p.Value)) continue;
                if (pc != null && pc.IsChildrenExists) continue;

                p.Value.IsOpen = false;
            }
        }

        public static void CloseAllPopups(Popup popup = null)
        {
            closeAllPopups(popup);
        }


        private DispatcherTimer _leavePopupTimer;

        protected void PopupBase_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!isPopupEnabled || _staysOpen) return;

            if (_leavePopupTimer == null)
            {
                _leavePopupTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(300),
                    DispatcherPriority.Loaded, OnLeavePopupTimerCallback, this.Dispatcher);
            }

            _leavePopupTimer.Start();

            //Popup popup;
            //if (!opened_popups.TryGetValue(this, out popup) || popup == null || popup.IsMouseOver) return;
        }

        private void OnLeavePopupTimerCallback(object sender, EventArgs e)
        {
            _leavePopupTimer.Stop();

            var mousePnt = MouseUtilities.GetMousePosition(this);
            if (PopupBaseRect.Contains(mousePnt)) return;

            Popup popup;
            if (!opened_popups.TryGetValue(this, out popup) || popup == null || popup.IsMouseOver) return;

            closeAllPopups(popup);
        }

        private DispatcherTimer _leavePopupFromPopupTimer;

        private void popup_MouseLeave(object sender, MouseEventArgs e)
        {
            if (e == null || _staysOpen) return;

            var parent = sender as Popup;
            if (parent == null) return;

            WaiterDelay = 300;


            if (_leavePopupFromPopupTimer == null)
            {
                _leavePopupFromPopupTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(300),
                    DispatcherPriority.Loaded, OnLeavePopupFromPopupTimerCallback, this.Dispatcher);
            }

            _leavePopupFromPopupTimer.Tag = parent;
            _leavePopupFromPopupTimer.Start();
        }

        private void OnLeavePopupFromPopupTimerCallback(object sender, EventArgs e)
        {
            _leavePopupFromPopupTimer.Stop();

            var parent = _leavePopupFromPopupTimer.Tag as Popup;
            if (parent == null) return;

            var popup = parent.Child as FrameworkElement;
            if (popup == null) return;

            var pc = parent.Tag as PopupContainer;
            var bounds = pc == null || pc.PopupRect == Rect.Empty ? VisualTreeHelper.GetDescendantBounds(popup) : pc.PopupRect;

            var mousePnt = MouseUtilities.GetMousePosition(popup);
            if (bounds.Contains(mousePnt)) return;

            mousePnt = MouseUtilities.GetMousePosition(this);
            if (PopupBaseRect.Contains(mousePnt)) return;

            closeAllPopups(parent);
        }

        protected virtual UserControl GetPopupControl()
        {
            return null;
        }

        static IModule sourceModule = null;

        /// <summary>
        /// Получить модуль, в котором произошел последний Popup 
        /// </summary>
        public static IModule SourceModule
        {
            get
            {                
                return sourceModule;
            }
            set
            {
                sourceModule = value;
            }
        }

        /// <summary>
        /// Получить локальный период, если модуль содержит в себе даты (т.е. реализовывает интерфейс IDatePeriod)
        /// или в противном случае период из настроек глобального фильтра
        /// </summary>
        public static DatePeriod LocalOrGlobalDatePeriod
        {
            get
            {
                var module = SourceModule as IDatePeriod;
                if (module == null)
                {
                    var fe = SourceModule as FrameworkElement;
                    if (fe != null)
                    {
                        module = fe.FindParent<IDatePeriod>();
                    }
                }

                if (module != null) return module.datePeriod;

                return GlobalFilter.DatePeriod;
            }
        }

        #region Dispose

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            // подавляем финализацию
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            try
            {
                //BindingOperations.ClearBinding(this, FrameworkElement.TagProperty);

                //if (_popup != null)
                //{
                //    _popup.MouseLeave -= popup_MouseLeave;
                //}

                //if (CheckAccess())
                {
                    MouseEnter -= PopupBase_MouseEnter;
                    MouseLeave -= PopupBase_MouseLeave;
                    MouseDown -= PopupBase_MouseDown;
                    //Unloaded -= OnUnloaded;
                }
            }
            finally
            {
                _disposed = true;
            }
        }

        // Деструктор
        ~PopupBase()
        {
            Dispose(false);
        }

        #endregion

        public void ChangePopupState(bool stayOpen)
        {
            _staysOpen = stayOpen;
        }
    }
}
