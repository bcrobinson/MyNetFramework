namespace Library.Wpf
{
    using System.Collections;
    using System.Collections.Generic;

    public static class EnumerableExtensions
    {
        public static IEnumerable<TItem> Append<TItem>(this IEnumerable<TItem> source, TItem item)
        {
            foreach (TItem i in source)
            {
                yield return i;
            }

            yield return item;
        }

        public static IEnumerable<TItem> Append<TItem>(this TItem source, IEnumerable<TItem> items)
        {
            yield return source;

            foreach (TItem item in items)
            {
                yield return item;
            }
        }

        public static IEnumerable<TItem> Append<TItem>(this TItem source, TItem item)
        {
            yield return source;

            yield return item;
        }

        public static IEnumerable<TItem> AsType<TItem>(this IEnumerable source)
                    where TItem : class
        {
            if (source == null)
            {
                yield break;
            }

            foreach (object item in source)
            {
                TItem t = item as TItem;

                if (t != null)
                {
                    yield return t;
                }
            }
        }

        public static bool HasAtLeast<TItem>(this IEnumerable<TItem> source, int count)
        {
            if (source == null)
            {
                return false;
            }

            int i = 0;

            foreach (TItem item in source)
            {
                if (i++ >= count)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates am infinite series that starts with the source items then returns the default item value indefinitely.
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="source">The source series.</param>
        /// <returns>
        /// An infinite series with default pad items at the end.
        /// </returns>
        public static IEnumerable<T> PadEnd<T>(this IEnumerable<T> source)
        {
            return source.PadEnd(default(T));
        }

        /// <summary>
        /// Creates am infinite series that starts with the source items then returns the pad item indefinitely.
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="source">The source series.</param>
        /// <param name="padItem">The pad item.</param>
        /// <returns>
        /// An infinite series with pad items at the end.
        /// </returns>
        public static IEnumerable<T> PadEnd<T>(this IEnumerable<T> source, T padItem)
        {
            foreach (var item in source)
            {
                yield return item;
            }

            while (true)
            {
                yield return padItem;
            }
        }
    }
}