namespace DesignSurface.App.Framework
{
    using System.Threading.Tasks;

    /// <summary>
    /// Describes a mechanism for Accepting, Rejecting changes asynchronously.
    /// </summary>
    public interface IRevertibleChangeTrackingAsync
    {
        /// <summary>
        /// Resets the object’s state to unchanged by rejecting the modifications.
        /// </summary>
        /// <returns>A Task representing the completion of the Rejection.</returns>
        Task RejectChanges();

        /// <summary>
        /// Gets a value indicating whether [is changed].
        /// </summary>
        /// <value><c>true</c> if is changed; otherwise, <c>false</c>.</value>
        bool IsChanged { get; }

        //
        /// <summary>
        /// Resets the object’s state to unchanged by accepting the modifications.
        /// </summary>
        /// <returns>
        /// A Task representing the completion of the Change Accepting.
        /// </returns>
        Task AcceptChanges();
    }
}