namespace Library.Wpf
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    public abstract class RevertableNotifyPropertyChanged<TObject> : NotifyPropertyChanged, IRevertibleChangeTrackingAsync
        where TObject : RevertableNotifyPropertyChanged<TObject>
    {
        private static Lazy<ReadOnlyDictionary<string, PropertyInfo>> lazyProperties = new Lazy<ReadOnlyDictionary<string, PropertyInfo>>(
            () =>
                new ReadOnlyDictionary<string, PropertyInfo>(
                        typeof(TObject).GetProperties().ToDictionary(p => p.Name)));

        /// <summary>
        /// The lazy list of delegates to deal with change tracked properties.
        ///  - Item1: Func that returns a Boolean indicating whether the object "IsChanged" for the TObject parameter.
        ///  - Item2: Action that accepts changes on the TObject parameter.
        /// </summary>
        private static readonly Lazy<List<Tuple<Func<TObject, bool>, Action<TObject>>>> lazyChangeTrackedProperties = new Lazy<List<Tuple<Func<TObject, bool>, Action<TObject>>>>(
            () => typeof(TObject).GetProperties()
                .Where(p => typeof(IChangeTracking).IsAssignableFrom(p.PropertyType))
                .Select(pi => Tuple.Create(
                    (Func<TObject, bool>)((obj) =>
                    {
                        IChangeTracking propValue = pi.GetValue(obj) as IChangeTracking;
                        if (propValue != null)
                        {
                            return propValue.IsChanged;
                        }
                        return false;
                    }),
                    (Action<TObject>)((obj) =>
                    {
                        IChangeTracking propValue = pi.GetValue(obj) as IChangeTracking;
                        if (propValue != null)
                        {
                            propValue.AcceptChanges();
                        }
                    })))
                .ToList());

        /// <summary>
        /// The lazy list of series that contain change tracked objects.
        /// The Func returns a Boolean indicating whether the any of the series' items "IsChanged" for the TObject parameter.
        /// </summary>
        private static readonly Lazy<List<Func<TObject, bool>>> LazyChangeTrackedCollections = new Lazy<List<Func<TObject, bool>>>(
            () => typeof(TObject).GetProperties()
                .Where(p => typeof(IEnumerable<IChangeTracking>).IsAssignableFrom(p.PropertyType))
                .Select(pi => (Func<TObject, bool>)((obj) =>
                {
                    IEnumerable<IChangeTracking> propValues = pi.GetValue(obj) as IEnumerable<IChangeTracking>;
                    if (propValues != null)
                    {
                        return propValues.Any(v => v.IsChanged);
                    }
                    return false;
                }))
                .ToList());

        private readonly ConcurrentDictionary<string, object> orignalClonedObjects;
        private readonly ConcurrentDictionary<string, CollectionRef> orignalCollections;
        private readonly ConcurrentDictionary<string, object> orignalValueObjects;
        private bool isChanged;

        protected RevertableNotifyPropertyChanged()
        {
            if (!(this is TObject))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "The generic parameter of the RevertableNotifyPropertyChanged must be the same as the class that inherits it.{0}Expected \"{1}\" actual \"{2}\".",
                        Environment.NewLine,
                        typeof(TObject).FullName,
                        this.GetType().FullName));
            }

            this.orignalCollections = new ConcurrentDictionary<string, CollectionRef>(StringComparer.Ordinal);
            this.orignalClonedObjects = new ConcurrentDictionary<string, object>(StringComparer.Ordinal);
            this.orignalValueObjects = new ConcurrentDictionary<string, object>(StringComparer.Ordinal);

            this.isChanged = false;
        }

        [IgnoreDataMember]
        [SoapIgnore]
        [XmlIgnore]
        public override bool IsChanged
        {
            get
            {
                if (this.isChanged)
                {
                    return true;
                }

                return lazyChangeTrackedProperties.Value.Select(p => p.Item1)
                    .Concat(LazyChangeTrackedCollections.Value)
                    .Any(p => p.Invoke((TObject)this));
            }
            set
            {
                this.isChanged = value; this.OnPropertyChanged();
            }
        }

        public Task AcceptChanges()
        {
            foreach (var acceptFunc in lazyChangeTrackedProperties.Value.Select(p => p.Item2))
            {
                acceptFunc.Invoke((TObject)this);
            }

            foreach (var trackedProperty in this.orignalClonedObjects.Keys.ToList())
            {
                ICloneable currentValue = lazyProperties.Value[trackedProperty].GetValue(this) as ICloneable;

                object storedValue = currentValue != null ? currentValue.Clone() : null;

                this.orignalClonedObjects.AddOrUpdate(
                    trackedProperty,
                    storedValue,
                    (key, value) => storedValue);
            }

            foreach (var trackedProperty in this.orignalValueObjects.Keys.ToList())
            {
                object currentValue = lazyProperties.Value[trackedProperty].GetValue(this);

                this.orignalValueObjects.AddOrUpdate(
                    trackedProperty,
                    currentValue,
                    (key, value) => currentValue);
            }

            foreach (var trackedCollection in this.orignalCollections.Keys.ToList())
            {
                object currentValue = lazyProperties.Value[trackedCollection].GetValue(this);

                this.orignalCollections.AddOrUpdate(
                    trackedCollection,
                    new CollectionRef(),
                    (key, value) =>
                    {
                        if (value.IsValue)
                        {
                            value.OriginalValues = ((IList)currentValue).Cast<object>().ToList();
                        }
                        else
                        {
                            foreach (var trackedObject in (currentValue as IEnumerable<IChangeTracking>) ?? new IChangeTracking[0])
                            {
                                trackedObject.AcceptChanges();
                            }

                            value.OriginalValues = ((IList)currentValue).Cast<ICloneable>().Select(o => o.Clone()).ToList();
                        }

                        return value;
                    });
            }

            this.IsChanged = false;

            return Task.FromResult(true);
        }

        public override void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            base.OnPropertyChanged(propertyName);

            if (!string.Equals("IsChanged", propertyName, StringComparison.Ordinal))
            {
                this.isChanged = true;
                base.OnPropertyChanged("IsChanged");
            }
        }

        /// <summary>
        /// Resets the object’s state to unchanged by rejecting the modifications.
        /// </summary>
        /// <returns>A Task representing the completion of the Rejection.</returns>
        /// <exception cref="System.InvalidOperationException">If there is an error reverting back
        /// this objects original values.</exception>
        public Task RejectChanges()
        {
            ResetProperties(this.orignalClonedObjects);
            ResetProperties(this.orignalValueObjects);
            ResetCollections(this.orignalCollections);

            this.IsChanged = false;

            return Task.FromResult(true);
        }

        protected void RegisterRevertTrackingObservableCollection<TProperty>(Expression<Func<TObject, ObservableBatchCollection<TProperty>>> collectionProperty)
            where TProperty : ICloneable<TProperty>
        {
            string propertyName = collectionProperty.GetPropertyName();

            Delegate del = (Action<ObservableBatchCollection<TProperty>, IList>)((col, original) => col.Reset(original.Cast<TProperty>().Select(o => o.DeepClone())));

            this.orignalCollections.AddOrUpdate(
                propertyName,
                new CollectionRef() { OriginalValues = new TProperty[0], ResetDelegate = del, IsValue = false },
                (key, value) => value);
        }

        protected void RegisterRevertTrackingObservableValueCollection<TProperty>(Expression<Func<TObject, ObservableBatchCollection<TProperty>>> collectionProperty)
            where TProperty : struct
        {
            string propertyName = collectionProperty.GetPropertyName();

            Delegate del = (Action<ObservableBatchCollection<TProperty>, IList>)((col, original) => col.Reset(original.Cast<TProperty>()));

            this.orignalCollections.AddOrUpdate(
                propertyName,
                new CollectionRef() { OriginalValues = new TProperty[0], ResetDelegate = del, IsValue = true },
                (key, value) => value);
        }

        protected void RegisterRevertTracking<TProperty>(Expression<Func<TObject, TProperty>> property)
            where TProperty : ICloneable<TProperty>, INotifyPropertyChanged
        {
            string propertyName = property.GetPropertyName();

            this.orignalClonedObjects.AddOrUpdate(
                propertyName,
                (object)null,
                (key, value) => value);
        }

        protected void RegisterRevertTracking(Expression<Func<TObject, string>> stringProperty)
        {
            string propertyName = stringProperty.GetPropertyName();

            this.orignalValueObjects.AddOrUpdate(
                propertyName,
                string.Empty,
                (key, value) => value);
        }

        protected void RegisterRevertTrackingValue<TProperty>(Expression<Func<TObject, TProperty>> property)
            where TProperty : struct
        {
            string propertyName = property.GetPropertyName();

            this.orignalValueObjects.AddOrUpdate(
                propertyName,
                default(TProperty),
                (key, value) => value);
        }

        private void ResetCollections(ConcurrentDictionary<string, CollectionRef> collectionDictionary)
        {
            foreach (var trackedCollection in collectionDictionary.ToList())
            {
                object collection = lazyProperties.Value[trackedCollection.Key].GetValue(this);

                trackedCollection.Value.ResetDelegate.DynamicInvoke(collection, trackedCollection.Value.OriginalValues);
            }
        }

        private void ResetProperties(ConcurrentDictionary<string, object> valueDictionary)
        {
            foreach (var trackedProperty in valueDictionary.ToList())
            {
                lazyProperties.Value[trackedProperty.Key].SetValue(this, trackedProperty.Value);
            }
        }

        private class CollectionRef
        {
            public bool IsValue { get; set; }

            public IList OriginalValues { get; set; }

            public Delegate ResetDelegate { get; set; }
        }
    }
}