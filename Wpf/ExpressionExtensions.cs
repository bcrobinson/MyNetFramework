namespace Library.Wpf
{
    using System;
    using System.Linq.Expressions;

    internal static class ExpressionExtensions
    {
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property to get the name of.</param>
        /// <returns>The name of the property.</returns>
        public static string GetPropertyName<TProperty>(this Expression<Func<TProperty>> property)
        {
            var member = (MemberExpression)property.Body;
            return member.Member.Name;
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property to get the name of.</param>
        /// <returns>
        /// The name of the property.
        /// </returns>
        public static string GetPropertyName<TObject, TProperty>(this Expression<Func<TObject, TProperty>> property)
        {
            var member = (MemberExpression)property.Body;
            return member.Member.Name;
        }
    }
}