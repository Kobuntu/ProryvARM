using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Proryv.AskueARM2.Client.ServiceReference.Common
{
    public class SortedRangeObservableCollection<T> : SortedSet<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Constructors 

        public SortedRangeObservableCollection()
        {
        }

        public SortedRangeObservableCollection(IComparer<T> comparer) : base (comparer)
        {
        }

        public SortedRangeObservableCollection(IEnumerable<T> list)
            : base(list)
        {
        }

        public SortedRangeObservableCollection(IEnumerable<T> list, IComparer<T> comparer)
            : base(list, comparer)
        {
        }

        #endregion Constructors

        #region Public Methods

        public void Clear(bool suppressNotification)
        {
            _suppressNotification = suppressNotification;
            Clear();
            _suppressNotification = false;
        }

        public bool Add(T item)
        {
            var result = base.Add(item);

            if (Application.Current.Dispatcher.CheckAccess())
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    OnCollectionChanged(
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item))));
            }

            return result;
        }

        public bool Remove(T item)
        {
            var result = base.Remove(item);

            if (Application.Current.Dispatcher.CheckAccess())
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    OnCollectionChanged(
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item))));
            }

            return result;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            _suppressNotification = true;

            UnionWith(collection);

            //foreach (var i in collection) Add(i);
            _suppressNotification = false;

            if (Application.Current.Dispatcher.CheckAccess())
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection));
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke((Action) (() =>
                    OnCollectionChanged(
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection))), DispatcherPriority.Loaded);
            }
        }

        public void Refresh()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void RemoveRange(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            _suppressNotification = true;

            foreach (var i in collection) Remove(i);
            _suppressNotification = false;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, collection));
        }

        #endregion Public Methods 

        #region Public Events 

        //-----------------------------------------------------

        #region INotifyPropertyChanged implementation 

        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged">).
        /// </see></summary> 
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { PropertyChanged += value; }
            remove { PropertyChanged -= value; }
        }

        #endregion INotifyPropertyChanged implementation 


        //------------------------------------------------------
        /// <summary>
        /// Occurs when the collection changes, either by adding or removing an item.
        /// </summary> 
        /// <remarks>
        /// see <seealso cref="INotifyCollectionChanged"> 
        /// </seealso></remarks> 
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion Public Events

        #region Protected Methods

        /// <summary>
        /// Called by base class Collection<T> when the list is being cleared; 
        /// raises a CollectionChanged event to any listeners.
        /// </summary> 
        public override void Clear()
        {
            CheckReentrancy();
            base.Clear();
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionReset();
        }

        /// <summary> 
        /// Called by base class Collection<T> when an item is removed from list;
        /// raises a CollectionChanged event to any listeners. 
        /// </summary>
        public bool RemoveItem(T item)
        {
            CheckReentrancy();
            var isItemRemoned = base.Remove(item);

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);

            return isItemRemoned;
        }

        /// <summary> 
        /// Called by base class Collection<T> when an item is removed from list;
        /// raises a CollectionChanged event to any listeners. 
        /// </summary>
        public bool AddItem(T item)
        {
            CheckReentrancy();
            var isItemAdded = base.Add(item);

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item);

            return isItemAdded;
        }

        /// <summary> 
        /// Raises a PropertyChanged event (per <see cref="INotifyPropertyChanged">). 
        /// </see></summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged">). 
        /// </see></summary>
        protected virtual event PropertyChangedEventHandler PropertyChanged;

        /// <summary> 
        /// Raise CollectionChanged event to any listeners.
        /// Properties/methods modifying this ObservableCollection will raise 
        /// a collection changed event through this virtual method. 
        /// </summary>
        /// <remarks> 
        /// When overriding this method, either call its base implementation
        /// or call <see cref="BlockReentrancy"> to guard against reentrant collection changes.
        /// </see></remarks>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_suppressNotification) return;

            if (CollectionChanged != null)
            {
                using (BlockReentrancy())
                {
                    CollectionChanged(this, e);
                }
            }
        }

        /// <summary> 
        /// Disallow reentrant attempts to change this collection. E.g. a event handler 
        /// of the CollectionChanged event is not allowed to make changes to this collection.
        /// </summary> 
        /// <remarks>
        /// typical usage is to wrap e.g. a OnCollectionChanged call with a using() scope:
        /// <code>
        ///         using (BlockReentrancy()) 
        ///         {
        ///             CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index)); 
        ///         } 
        /// </code>
        /// </remarks> 
        protected IDisposable BlockReentrancy()
        {
            _monitor.Enter();
            return _monitor;
        }

        /// <summary> Check and assert for reentrant attempts to change this collection. </summary> 
        /// <exception cref="InvalidOperationException"> raised when changing the collection
        /// while another collection change is still being notified to other listeners </exception> 
        protected void CheckReentrancy()
        {
            if (_monitor.Busy)
            {
                // we can allow changes if there's only one listener - the problem
                // only arises if reentrant changes make the original event args 
                // invalid for later listeners.  This keeps existing code working 
                // (e.g. Selector.SelectedItems).
                if ((CollectionChanged != null) && (CollectionChanged.GetInvocationList().Length > 1))
                    throw new InvalidOperationException("invalid for later listeners.");
            }
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary> 
        /// Helper to raise a PropertyChanged event  />). 
        /// </summary>
        private void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners 
        /// </summary> 
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item));
        }

        /// <summary> 
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary> 
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners 
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event with action == Reset to any listeners
        /// </summary> 
        private void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        #endregion Private Methods 

        #region Private Types

        // this class helps prevent reentrant calls
        [Serializable()]
        private class SimpleMonitor : IDisposable
        {
            public void Enter()
            {
                ++_busyCount;
            }

            public void Dispose()
            {
                --_busyCount;
            }

            public bool Busy
            {
                get { return _busyCount > 0; }
            }

            int _busyCount;
        }

        #endregion Private Types

        #region Private Fields

        private const string CountString = "Count";

        // This must agree with Binding.IndexerName.  It is declared separately
        // here so as to avoid a dependency on PresentationFramework.dll. 
        private const string IndexerName = "Item[]";

        private readonly SimpleMonitor _monitor = new SimpleMonitor();

        private bool _suppressNotification;

        #endregion Private Fields
    }
}
