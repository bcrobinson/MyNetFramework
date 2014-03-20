namespace Library.Wpf
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class SubscriptionManager
    {
        private readonly ConcurrentDictionary<Type, List<IWrapper>> asyncMessageSubscriptions;

        public SubscriptionManager()
        {
            this.asyncMessageSubscriptions = new ConcurrentDictionary<Type, List<IWrapper>>();
        }

        public void RegisterAsync<TMessage>(Func<TMessage, Task> subscriberActionAsync)
        {
            AsyncWrapper wrapper = new AsyncWrapper(subscriberActionAsync);

            this.RegisterWrapper<TMessage>(wrapper);
        }

        public void UnRegisterAsync<TMessage>(Func<TMessage, Task> subscriberAction)
        {
            AsyncWrapper wrapper = new AsyncWrapper(subscriberAction);

            this.UnRegisterWrapper<TMessage>(wrapper);
        }

        public async Task PublishAsync<TMessage>(TMessage eventMessage)
        {            
            List<IWrapper> asyncSubscribers;
            
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

                    await sub.InvokeAction<TMessage>(eventMessage);
                }

                if (cleanupSubs)
                {
                    this.CleanupEmptySubscriptions(typeof(TMessage));
                }
            }
        }

        private void UnRegisterWrapper<TMessage>(IWrapper wrapper)
        {
            this.asyncMessageSubscriptions.
                AddOrUpdate(
                //// Key
                typeof(TMessage),
                //// Add
                new List<IWrapper>(),
                //// Update
                (key, value) =>
                {
                    value.RemoveAll(wr => wr.Equals(wrapper));
                    return value;
                });
        }

        private void RegisterWrapper<TMessage>(IWrapper wrapper)
        {
            this.asyncMessageSubscriptions.
                AddOrUpdate(
                //// Key
                typeof(TMessage),
                //// Add
                new List<IWrapper>(new[] { wrapper }),
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
                new List<IWrapper>(),
                //// Update
                (key, value) => value.Where(s => s.IsAlive).ToList());
        }

        private interface IWrapper : IEquatable<IWrapper>
        {
            bool IsAlive { get; }

            Task InvokeAction<TMessage>(TMessage message);
        }

        private class AsyncWrapper : IWrapper, IEquatable<IWrapper>
        {
            private readonly MethodInfo subscriberMethod;

            private readonly WeakReference weakTarget;

            public AsyncWrapper(Delegate syncroHandleAction)
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

            public Task InvokeAction<TMessage>(TMessage message)
            {
                object targetStrongRef = this.weakTarget.Target;

                Task methodTask;

                if (this.IsAlive)
                {
                    methodTask = (Task)this.subscriberMethod.Invoke(targetStrongRef, new object[] { message });
                }
                else
                {
                    methodTask = Task.FromResult(true);
                }

                return methodTask;
            }

            public bool Equals(IWrapper other)
            {
                AsyncWrapper otherWrapper = other as AsyncWrapper;

                if (otherWrapper == null)
                {
                    return false;
                }

                return this.IsAlive && other.IsAlive
                    && object.ReferenceEquals(this.weakTarget.Target, otherWrapper.weakTarget.Target)
                    && this.subscriberMethod == otherWrapper.subscriberMethod;
            }
        }
        
        private class SyncroWrapper : IWrapper, IEquatable<IWrapper>
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

            public Task InvokeAction<TMessage>(TMessage message)
            {
                object targetStrongRef = this.weakTarget.Target;

                Task methodTask;

                if (this.IsAlive)
                {
                    methodTask = Task.Factory.StartNew(() =>
                        {
                            object strongRef = targetStrongRef;
                            this.subscriberMethod.Invoke(strongRef, new object[] { message });
                        }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
                }
                else
                {
                    methodTask = Task.FromResult(true);
                }

                return methodTask;
            }

            public bool Equals(IWrapper other)
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
