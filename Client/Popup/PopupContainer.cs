using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using Proryv.ElectroARM.Controls.Common.DragAndDrop;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows.Media;

namespace Proryv.AskueARM2.Client.Visual
{
    public class PopupContainer
    {
        public PopupContainer(Popup popup, Popup parent, FrameworkElement popupBase, ConcurrentDictionary<FrameworkElement, Popup> openedPopups)
        {
            Children = new List<Popup>();
            Parent = parent;
            Popup = popup;
            PopupBase = popupBase;
            _openedPopups = openedPopups;
            if (popup.Child!=null) PopupRect = VisualTreeHelper.GetDescendantBounds(popup.Child);
        }

        public readonly FrameworkElement PopupBase;
        public readonly Popup Parent;
        public Rect PopupRect;

        private readonly List<Popup> Children;
        private readonly Popup Popup;
        private readonly ConcurrentDictionary<FrameworkElement, Popup> _openedPopups;

        public void AddChild(Popup child)
        {
            if (!Children.Contains(child)) Children.Add(child);
        }

        public void ChildrenRemoveAndCloseIfNeed(Popup child)
        {
            var pc = child.Tag as PopupContainer;
            if (pc != null)
            {
                if (pc.IsChildrenExists) return;
            }

            Children.Remove(child);
            if (Children.Count != 0) return;

            var visual = Popup.Child as FrameworkElement;
            if (visual == null) return;

            var mousePnt = MouseUtilities.GetMousePosition(visual);
            if (!(visual.ActualWidth <= (mousePnt.X + 2)) && !(visual.ActualHeight <= (mousePnt.Y + 2)) && !(mousePnt.X < 2) && !(mousePnt.Y < 2)) return;

            Popup.IsOpen = false;
            Popup removed;
            _openedPopups.TryRemove(PopupBase, out removed);

            if (Parent != null)
            {
                pc = Parent.Tag as PopupContainer;
                if (pc != null)
                {
                    pc.ChildrenRemoveAndCloseIfNeed(Popup);
                }
            }
        }

        public bool IsChildrenExists
        {
            get { return Children.Any(c=>c.IsOpen); }
        }
    }
}
