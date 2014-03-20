namespace Library.Wpf
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;

    public class SortedObservableCollection<TKey, TValue> : NotifyPropertyChanged, ICollection<TValue>, IList<TValue>, INotifyCollectionChanged, INotifyPropertyChanged
        where TValue : INotifyPropertyChanged
    {
        private readonly IComparer<TKey> comparer;

        private readonly SortedList<TKey, TValue> items;
        private readonly Func<TValue, TKey> keySelector;

        public SortedObservableCollection(Func<TValue, TKey> keySelector, IComparer<TKey> comparer)
        {
            this.comparer = comparer;
            this.keySelector = keySelector;

            this.items = new SortedList<TKey, TValue>(comparer);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public int Count
        {
            get { return this.items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public TValue this[int index]
        {
            get
            {
                return this.items.Values[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(TValue item)
        {
            TKey key = this.keySelector(item);

            if (key == null)
            {
                throw new ArgumentException("Cannot have null key in item for when adding to SortedObservableCollection");
            }

            if (this.items.ContainsKey(key))
            {
                TValue olditem = this.items[key];
                olditem.PropertyChanged -= this.OnItemPropertyChanged;

                item.PropertyChanged += this.OnItemPropertyChanged;

                this.items[key] = item;

                int index = this.items.IndexOfKey(key);

                if (this.CollectionChanged != null)
                {
                    this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, olditem, index));
                }
            }
            else
            {
                this.items.Add(key, item);
                int index = this.items.IndexOfKey(key);

                item.PropertyChanged += this.OnItemPropertyChanged;

                if (this.CollectionChanged != null)
                {
                    this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
                }

                this.OnCountPropertyChanged();
            }
        }

        public void Clear()
        {
            foreach (var item in this.items)
            {
                item.Value.PropertyChanged -= this.OnItemPropertyChanged;
            }

            this.items.Clear();

            if (this.CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            this.OnCountPropertyChanged();
        }

        public bool Contains(TValue item)
        {
            return this.items.ContainsKey(this.keySelector(item));
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            this.items.Select(i => i.Value).ToList().CopyTo(array, arrayIndex);
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return this.items.Select(i => i.Value).GetEnumerator();
        }

        public int IndexOf(TValue item)
        {
            return this.items.IndexOfKey(this.keySelector(item));
        }

        public void Insert(int index, TValue item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TValue item)
        {
            TKey key = this.keySelector(item);

            if (key == null)
            {
                throw new ArgumentException("Cannot have null key in item for when adding to SortedObservableCollection");
            }

            int index = this.items.IndexOfKey(key);

            bool removed = false;

            if (index >= 0)
            {
                TValue listItem = this.items[key];

                listItem.PropertyChanged -= this.OnItemPropertyChanged;

                this.items.RemoveAt(index);

                removed = true;

                this.OnCountPropertyChanged();

                if (this.CollectionChanged != null)
                {
                    this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, listItem, index));
                }
            }

            return removed;
        }

        public void RemoveAt(int index)
        {
            this.items.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void OnCountPropertyChanged()
        {
            this.OnPropertyChanged("Count");
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var item = (TValue)sender;

            // If true then key value has changed and needs to be moved.
            if (this.items.ContainsKey(this.keySelector(item)))
            {
                int oldIndex = this.items.IndexOfValue(item);

                this.items.RemoveAt(oldIndex);

                TKey newKey = this.keySelector(item);

                this.items.Add(newKey, item);

                int newIndex = this.items.IndexOfKey(newKey);

                if (this.CollectionChanged != null)
                {
                    this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
                }
            }
        }
    }
}