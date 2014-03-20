namespace DesignSurface.App.Framework
{
    using System;

    /// <summary>
    /// Describes an object that can create deep clones.
    /// </summary>
    /// <typeparam name="T">The type object object that is cloned.</typeparam>
    public interface ICloneable<out T> : ICloneable
    {
        /// <summary>
        /// Creates a deep clone of this instance.
        /// </summary>
        /// <returns>
        /// Returns a deep clone of this object.
        /// </returns>
        T DeepClone();
    }
}