using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Proryv.AskueARM2.Client.ServiceReference.Common
{
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification = false;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            Action<bool> action = (suppressNotification) =>
            {
                if (!suppressNotification)
                {
                    base.OnCollectionChanged(e);

                    if (CollectionChanged != null)
                        CollectionChanged.Invoke(this, e);
                }
            };

            if (Application.Current.Dispatcher.CheckAccess())
            {
                action(_suppressNotification);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(action, _suppressNotification);
            }
        }
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChangedMultiItem(
            NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handlers = this.CollectionChanged;
            if (handlers != null)
            {
                foreach (NotifyCollectionChangedEventHandler handler in
                    handlers.GetInvocationList())
                {
                    if (handler.Target is CollectionView)
                        ((CollectionView)handler.Target).Refresh();
                    else
                        handler(this, e);
                }
            }
        }

        public void Clear(bool suppressNotification)
        {
            _suppressNotification = suppressNotification;
            Clear();
            _suppressNotification = false;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            _suppressNotification = true;

            var count = Count;

            foreach (var i in collection) Add(i);
            _suppressNotification = false;

            Action action = () =>
            {
                OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

                /// https://stackoverflow.com/questions/3300845/observablecollection-calling-oncollectionchanged-with-multiple-new-items
                //ѕериодически происходит exception? надо разбиратьс€
                OnCollectionChangedMultiItem(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection));
                //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList<T>)collection));
            };

            if (Application.Current.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(action);
            }
        }

        public void Refresh()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void InsertRange(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            _suppressNotification = true;
            int ind = 0;
            foreach (var i in collection)
            {
                Insert(ind++, i);
            }

            _suppressNotification = false;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void RemoveRange(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            _suppressNotification = true;

            foreach (var i in collection) Remove(i);
            _suppressNotification = false;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void RemoveAll(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException("match");

            var removed = new List<T>();

            _suppressNotification = true;

            foreach (var i in this.ToList())
            {
                if (match(i))
                {
                    Remove(i);
                    removed.Add(i);
                }
            }

            _suppressNotification = false;


            Action action = () =>
            {
                OnCollectionChangedMultiItem(
                  new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));

                //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            };

            if (Application.Current.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(action);
            }
        }

        /// <summary> 
        /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class. 
        /// </summary> 
        public RangeObservableCollection()
            : base() { }

        /// <summary> 
        /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class that contains elements copied from the specified collection. 
        /// </summary> 
        /// <param name="collection">collection: The collection from which the elements are copied.</param> 
        /// <exception cref="System.ArgumentNullException">The collection parameter cannot be null.</exception> 
        public RangeObservableCollection(IEnumerable<T> collection)
            : base(collection) { }


        private readonly IComparer<T> _comparer;
        public RangeObservableCollection(IEnumerable<T> collection, IComparer<T> comparer)
            : base(collection.OrderBy(c=>c, comparer))
        {
            _comparer = comparer;
        }

        public RangeObservableCollection(IComparer<T> comparer)
            : base()
        {
            _comparer = comparer;
        }

        public void UnionWith(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            _suppressNotification = true;

            if (_comparer!=null) foreach (var i in collection.OrderBy(c=>c, _comparer)) Add(i);
            else foreach (var i in collection) Add(i);

            //_innerCollection.UnionWith(collection);

            _suppressNotification = false;

            Action action = () =>
            {
                OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

                //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList<T>)collection));

                OnCollectionChangedMultiItem(
                  new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection));
            };

            if (Application.Current.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(action);
            }
        }

        public int RemoveWhere(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException("match");

            _suppressNotification = true;

            var removed = 0;
            foreach (var i in this.ToList())
            {
                if (match(i) && Remove(i)) removed++;
            }

            _suppressNotification = false;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            return removed;
        }

        public void SetItemByIndex(int index, T item)
        {
            SetItem(index, item);
        }
    }
}