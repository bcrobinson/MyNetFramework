namespace DesignSurface.App.Framework
{
    using System.Collections;
    using System.Linq;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;

    public sealed class ObservableBatchCollection<T> : ObservableCollection<T>
    {
        public bool SuspendCollectionChangedEvents { get; set; }

        public bool SuspendPropertyChangedEvents { get; set; }

        public ObservableBatchCollection()
            : base()
        {
        }

        public ObservableBatchCollection(IEnumerable<T> items)
            : base(items)
        {
        }

        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!this.SuspendCollectionChangedEvents)
            {
                base.OnCollectionChanged(e);
            }
        }

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!this.SuspendPropertyChangedEvents)
            {
                base.OnPropertyChanged(e);
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            bool suspendCollectionChangedOrignal = this.SuspendCollectionChangedEvents;
            bool suspendPropertyChangedOrignal = this.SuspendPropertyChangedEvents;

            try
            {
                this.SuspendCollectionChangedEvents = true;
                this.SuspendPropertyChangedEvents = true;

                foreach (T item in items)
                {
                    this.Add(item);
                }
            }
            finally
            {
                this.SuspendCollectionChangedEvents = suspendCollectionChangedOrignal;
                this.SuspendPropertyChangedEvents = suspendPropertyChangedOrignal;
            }

            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Reset(IEnumerable<T> items)
        {
            bool suspendCollectionChangedOrignal = this.SuspendCollectionChangedEvents;
            bool suspendPropertyChangedOrignal = this.SuspendPropertyChangedEvents;

            try
            {
                this.SuspendCollectionChangedEvents = true;
                this.SuspendPropertyChangedEvents = true;

                this.Clear();

                foreach (T item in items)
                {
                    this.Add(item);
                }
            }
            finally
            {
                this.SuspendCollectionChangedEvents = suspendCollectionChangedOrignal;
                this.SuspendPropertyChangedEvents = suspendPropertyChangedOrignal;
            }

            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}