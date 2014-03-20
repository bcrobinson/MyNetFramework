namespace Library.Wpf
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the Application Context object.
    /// </summary>
    public interface IAppContext
    {
        /// <summary>
        /// Publishes a message of type TMessage, invokes all subscribers to this message.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task representing the completion all subscriber actions.</returns>
        Task PublishAsync<TMessage>(TMessage eventMessage);

        /// <summary>
        /// Registers an asynchronous method for messages of type TMessage.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="subscriberAction">The subscriber action.</param>
        void RegisterAsync<TMessage>(Func<TMessage, Task> subscriberAction);

        /// <summary>
        /// Un-registers a previously registered method method.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="subscriberAction">The subscriber action.</param>
        void UnregisterAsync<TMessage>(Func<TMessage, Task> subscriberAction);
    }
}