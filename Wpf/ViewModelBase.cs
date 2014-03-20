namespace Library.Wpf
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Text;

    /// <summary>
    /// View Model Base class.
    /// </summary>
    public abstract class ViewModelBase : NotifyPropertyChanged, IDataErrorInfo
    {
        /// <summary>
        /// The application context.
        /// </summary>
        private readonly IAppContext appContext;

        /// <summary>
        /// The error cache.
        /// </summary>
        private readonly ConcurrentDictionary<string, List<string>> errorCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase"/> class.
        /// </summary>
        protected ViewModelBase(IAppContext appContext)
        {
            this.appContext = appContext;

            this.errorCache = new ConcurrentDictionary<string, List<string>>();
        }

        /// <summary>
        /// Gets the app context.
        /// </summary>
        /// <value>The app context.</value>
        public IAppContext AppContext
        {
            get { return this.appContext; }
        }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>An error message indicating what is wrong with this object. The default is an
        /// empty string ("").</returns>
        public virtual string Error
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var property in this.ErrorCache)
                {
                    foreach (var propertyError in property.Value)
                    {
                        sb.AppendFormat("{0}: {1}", property.Key, propertyError);
                    }
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the error cache.
        /// </summary>
        /// <value>The error cache.</value>
        protected ConcurrentDictionary<string, List<string>> ErrorCache
        {
            get { return this.errorCache; }
        }

        /// <summary>
        /// Gets the <see cref="System.String"/> with the specified column name.
        /// </summary>
        /// <value>The <see cref="System.String"/>.</value>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>A string error for the specified column. Empty string if none found.</returns>
        public string this[string columnName]
        {
            get
            {
                List<string> errors;
                if (!this.ErrorCache.TryGetValue(columnName, out errors))
                {
                    errors = new List<string>();
                }

                return string.Join(",", errors);
            }
        }

        /// <summary>
        /// Adds the property error.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="error">The error.</param>
        protected void AddPropertyError<T>(Expression<Func<T>> property, string error)
        {
            var member = (MemberExpression)property.Body;
            string propertyName = member.Member.Name;

            this.errorCache.AddOrUpdate(
                //// Key
                propertyName,
                //// Add
                new List<string>(new[] { error }),
                //// Update
                (key, value) =>
                {
                    value.Add(error);
                    return value;
                });
        }

        protected void RemovePropertyError<T>(Expression<Func<T>> property)
        {
            MemberExpression member = (MemberExpression)property.Body;
            string propertyName = member.Member.Name;

            List<string> errors;
            this.errorCache.TryRemove(propertyName, out errors);
        }
    }
}