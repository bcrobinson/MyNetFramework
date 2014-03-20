namespace Library.Wpf
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// An implementation of the <see cref="INotifyPropertyChanged" /> interface with members for
    /// dealing with updates.
    /// </summary>
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        private readonly ConcurrentDictionary<string, Tuple<Func<INotifyPropertyChanged>, IDisposable>> changeListenDictionary = new ConcurrentDictionary<string, Tuple<Func<INotifyPropertyChanged>, IDisposable>>(StringComparer.Ordinal);

        /// <summary>
        /// The update cascade dictionary.
        /// </summary>
        private readonly ConcurrentDictionary<string, HashSet<string>> updateCascadeDictionary = new ConcurrentDictionary<string, HashSet<string>>(StringComparer.Ordinal);

        /// <summary>
        /// The is changed flag.
        /// </summary>
        private bool isChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyPropertyChanged"/> class.
        /// </summary>
        protected NotifyPropertyChanged()
        {
            this.isChanged = false;
        }

        /// <summary>
        /// Occurs when the value of a property changed has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets a value indicating whether this object has changed.
        /// </summary>
        /// <value><c>true</c> if is changed; otherwise, <c>false</c>.</value>
        public virtual bool IsChanged
        {
            get { return this.isChanged; }
            set { this.isChanged = value; this.OnPropertyChanged(); }
        }

        public void ListenToChanges(Expression<Func<INotifyPropertyChanged>> property)
        {
            Func<INotifyPropertyChanged> propertyFunc = property.Compile();

            var tuple = this.UpdateChangeListenTuple(Tuple.Create(propertyFunc, Disposable.Empty));

            this.changeListenDictionary.AddOrUpdate(
                property.GetPropertyName(),
                tuple,
                (key, value) =>
                {
                    tuple.Item2.Dispose();
                    return this.UpdateChangeListenTuple(value);
                });
        }

        /// <summary>
        /// Raises the property changed event for a property.
        /// </summary>
        /// <param name="propertyName">Name of the property that has changed.</param>
        public virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            this.CascadePropertyChanged(propertyName);

            this.UpdateChangeListening(propertyName);

            if (!string.Equals("IsChanged", propertyName, StringComparison.Ordinal))
            {
                this.isChanged = true;
            }
        }

        /// <summary>
        /// Raises the property changed for a property using Expression.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <param name="property">The property that has changed.</param>
        public void OnPropertyChanged<TProperty>(Expression<Func<TProperty>> property)
        {
            this.OnPropertyChanged(property.GetPropertyName());
        }

        /// <summary>
        /// Raises a property changed update for the target property when the source property is
        /// updated.
        /// </summary>
        /// <typeparam name="TSourceProperty">The type of the source property.</typeparam>
        /// <typeparam name="TTargetProperty">The type of the target property.</typeparam>
        /// <param name="sourceProperty">The source property.</param>
        /// <param name="targetProperty">The target property.</param>
        protected void CascadeUpdate<TSourceProperty, TTargetProperty>(Expression<Func<TSourceProperty>> sourceProperty, Expression<Func<TTargetProperty>> targetProperty)
        {
            string sourceName = sourceProperty.GetPropertyName();
            string targetName = targetProperty.GetPropertyName();

            this.updateCascadeDictionary.AddOrUpdate(
                sourceName,
                new HashSet<string>(new[] { targetName }, StringComparer.Ordinal),
                (key, value) =>
                {
                    value.Add(targetName);
                    return value;
                });
        }

        /// <summary>
        /// Raises property changed event and cascade raising of the event to all child listeners.
        /// </summary>
        /// <param name="propertyName">Name of the property to raise update for.</param>
        private void CascadePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;

            if (propertyChanged == null)
            {
                return;
            }

            HashSet<string> updatedProperties = new HashSet<string>(StringComparer.Ordinal);
            Stack<string> propertiesToUpdate = new Stack<string>();

            propertiesToUpdate.Push(propertyName);

            while (propertiesToUpdate.Count > 0)
            {
                string currentProperty = propertiesToUpdate.Pop();

                if (updatedProperties.Add(currentProperty))
                {
                    propertyChanged(this, new PropertyChangedEventArgs(currentProperty));

                    HashSet<string> updates;

                    if (this.updateCascadeDictionary.TryGetValue(currentProperty, out updates))
                    {
                        foreach (string cascadePropertyName in updates)
                        {
                            propertiesToUpdate.Push(cascadePropertyName);
                        }
                    }
                }
            }
        }

        private Tuple<Func<INotifyPropertyChanged>, IDisposable> UpdateChangeListening(string property)
        {
            return this.changeListenDictionary.AddOrUpdate(
                property,
                Tuple.Create((Func<INotifyPropertyChanged>)(() => null), Disposable.Empty),
                (key, value) => UpdateChangeListenTuple(value));
        }

        private Tuple<Func<INotifyPropertyChanged>, IDisposable> UpdateChangeListenTuple(Tuple<Func<INotifyPropertyChanged>, IDisposable> value)
        {
            value.Item2.Dispose();
            INotifyPropertyChanged child = value.Item1();

            Tuple<Func<INotifyPropertyChanged>, IDisposable> returnTuple;

            if (child == null)
            {
                returnTuple = Tuple.Create(value.Item1, Disposable.Empty);
            }
            else
            {
                PropertyChangedEventHandler handler = (sender, e) =>
                {
                    this.IsChanged = true;
                };

                child.PropertyChanged += handler;

                returnTuple = Tuple.Create(
                    value.Item1,
                    Disposable.Create(() => { child.PropertyChanged -= handler; }));
            }

            return returnTuple;
        }

        private sealed class Disposable : IDisposable
        {
            private static readonly IDisposable emptyInstance = new Disposable(() => { });
            private readonly Action disposeAction;

            public Disposable(Action disposeAction)
            {
                this.disposeAction = disposeAction;
            }

            public static IDisposable Empty
            {
                get { return emptyInstance; }
            }

            public static IDisposable Create(Action disposeAction)
            {
                return new Disposable(disposeAction);
            }

            public void Dispose()
            {
                this.disposeAction();
            }
        }
    }
}