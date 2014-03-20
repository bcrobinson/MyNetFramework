namespace Library.Wpf
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Threading;

    public sealed class SubscriptionManager
    {
        private readonly ConcurrentDictionary<Type, List<SyncroWrapper>> asyncMessageSubscriptions;

        public SubscriptionManager()
        {
            this.asyncMessageSubscriptions = new ConcurrentDictionary<Type, List<SyncroWrapper>>();
        }

        public void Register<TMessage>(Action<TMessage> subscriberActionAsync)
        {
            SyncroWrapper wrapper = new SyncroWrapper(subscriberActionAsync);

            this.RegisterWrapper<TMessage>(wrapper);
        }

        public void UnRegister<TMessage>(Action<TMessage> subscriberAction)
        {
            SyncroWrapper wrapper = new SyncroWrapper(subscriberAction);

            this.UnRegisterWrapper<TMessage>(wrapper);
        }

        public void Publish<TMessage>(TMessage eventMessage)
        {            
            List<SyncroWrapper> asyncSubscribers;
            
            if (this.asyncMessageSubscriptions.TryGetValue(typeof(TMessage), out asyncSubscribers))
            {
                bool cleanupSubs = false;

                foreach (var sub in asyncSubscribers)
                {
                    if (!sub.IsAlive)
                    {
                        cleanupSubs = true;
                        continue;
                    }

                    sub.InvokeAction<TMessage>(eventMessage);
                }

                if (cleanupSubs)
                {
                    this.CleanupEmptySubscriptions(typeof(TMessage));
                }
            }
        }

        private void UnRegisterWrapper<TMessage>(SyncroWrapper wrapper)
        {
            this.asyncMessageSubscriptions.
                AddOrUpdate(
                //// Key
                typeof(TMessage),
                //// Add
                new List<SyncroWrapper>(),
                //// Update
                (key, value) =>
                {
                    value.RemoveAll(wr => wr.Equals(wrapper));
                    return value;
                });
        }

        private void RegisterWrapper<TMessage>(SyncroWrapper wrapper)
        {
            this.asyncMessageSubscriptions.
                AddOrUpdate(
                //// Key
                typeof(TMessage),
                //// Add
                new List<SyncroWrapper>(new[] { wrapper }),
                //// Update
                (key, value) =>
                {
                    if (!value.Any(sub => sub.Equals(wrapper)))
                    {
                        value.Add(wrapper);
                    }
                    return value;
                });
        }

        private void CleanupEmptySubscriptions(Type keyToClean)
        {
            this.asyncMessageSubscriptions.AddOrUpdate(
                //// Key
                keyToClean,
                //// Add
                new List<SyncroWrapper>(),
                //// Update
                (key, value) => value.Where(s => s.IsAlive).ToList());
        }

        private class SyncroWrapper : IEquatable<SyncroWrapper>
        {
            private readonly MethodInfo subscriberMethod;

            private readonly WeakReference weakTarget;

            public SyncroWrapper(Delegate syncroHandleAction)
            {
                this.subscriberMethod = syncroHandleAction.Method;

                this.weakTarget = new WeakReference(syncroHandleAction.Target);
            }

            public bool IsAlive
            {
                get
                {
                    return this.weakTarget.IsAlive || this.subscriberMethod.IsStatic;
                }
            }

            public void InvokeAction<TMessage>(TMessage message)
            {
                object targetStrongRef = this.weakTarget.Target;

                if (this.IsAlive)
                {
                    SynchronizationContext.Current.Post(_ => this.subscriberMethod.Invoke(targetStrongRef, new object[] { message }), null);
                }
            }

            public bool Equals(SyncroWrapper other)
            {
                SyncroWrapper otherWrapper = other as SyncroWrapper;

                if (otherWrapper == null)
                {
                    return false;
                }

                return this.IsAlive && other.IsAlive
                    && object.ReferenceEquals(this.weakTarget.Target, otherWrapper.weakTarget.Target)
                    && this.subscriberMethod == otherWrapper.subscriberMethod;
            }
        }
    }
}
