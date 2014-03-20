namespace Library.Wpf.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    
    public partial class SubscriptionManagerTest
    {
        [TestClass]
        public class GivenNewSubscriptionManager_WhenSubscribingSingleType_AndWhenPublishingSingleMessage : SubscriptionManagerContext
        {
            private FooMessage publishedMessage;

            [TestInitialize]
            public void Act()
            {
                this.Manager.Register<FooMessage>(this.Subscriber.HandleFooMessage);

                this.publishedMessage = new FooMessage()
                {
                    Content = "Some String"
                };

                this.Manager.Publish(this.publishedMessage);

                Thread.Sleep(10);
            }

            [TestMethod]
            public void ThenSubscriberRecievesSingleMessage()
            {
                Assert.AreEqual(1, this.Subscriber.PublishedFoos.Count);
            }

            [TestMethod]
            public void ThenPublisedMessageIsCorrect()
            {
                Assert.AreEqual(this.publishedMessage.Content, this.Subscriber.PublishedFoos[0].Content);
            }
        }

        [TestClass]
        public class GivenNewSubscriptionManager_WhenSubscribingSingleType_AndWhenPublishingSingleMessage_AndUnSubscribing_AndWhenPublishingSingleMessage : SubscriptionManagerContext
        {
            private FooMessage firstPublishedMessage;

            private FooMessage secondPublishedMessage;

            [TestInitialize]
            public void Act()
            {
                this.Manager.Register<FooMessage>(this.Subscriber.HandleFooMessage);

                this.firstPublishedMessage = new FooMessage()
                {
                    Content = "Some string"
                };

                this.secondPublishedMessage = new FooMessage()
                {
                    Content = "Some other string"
                };

                this.Manager.Publish(this.firstPublishedMessage);

                this.Manager.UnRegister<FooMessage>(this.Subscriber.HandleFooMessage);

                this.Manager.Publish(this.secondPublishedMessage);

                Thread.Sleep(10);
            }

            [TestMethod]
            public void ThenSubscriberRecievesSingleMessageOnly()
            {
                Assert.AreEqual(1, this.Subscriber.PublishedFoos.Count);
            }

            [TestMethod]
            public void ThenPublisedMessageIsFirst()
            {
                Assert.AreEqual(this.firstPublishedMessage.Content, this.Subscriber.PublishedFoos[0].Content);
            }
        }

        [TestClass]
        public class GivenNewSubscriptionManager_WhenSubscribingTwoTypes_AndWhenPublishingSingleMessage : SubscriptionManagerContext
        {
            private FooMessage publishedFooMessage;

            private BarMessage publishedBarMessage;

            [TestInitialize]
            public void Act()
            {
                this.Manager.Register<FooMessage>(this.Subscriber.HandleFooMessage);

                this.Manager.Register<BarMessage>(this.Subscriber.HandleBarMessage);

                this.publishedFooMessage = new FooMessage()
                {
                    Content = "Some String"
                };

                this.Manager.Publish(this.publishedFooMessage);

                this.publishedBarMessage = new BarMessage()
                {
                    Content = 1
                };

                this.Manager.Publish(this.publishedBarMessage);

                Thread.Sleep(10);
            }

            [TestMethod]
            public void ThenSubscriberRecievesSingleFooMessage()
            {
                Assert.AreEqual(1, this.Subscriber.PublishedFoos.Count);
            }

            [TestMethod]
            public void ThenSubscriberRecievesSingleBarMessage()
            {
                Assert.AreEqual(1, this.Subscriber.PublishedBars.Count);
            }

            [TestMethod]
            public void ThenPublisedFooMessageIsCorrect()
            {
                Assert.AreEqual(this.publishedFooMessage.Content, this.Subscriber.PublishedFoos[0].Content);
            }

            [TestMethod]
            public void ThenPublisedBarMessageIsCorrect()
            {
                Assert.AreEqual(this.publishedBarMessage.Content, this.Subscriber.PublishedBars[0].Content);
            }
        }

        [TestClass]
        public class GivenNewSubscriptionManager_WhenSubscribingSingleType_AndWhenSubscriberReferenceIsNulled_AndWhenGCCollectionHappens : SubscriptionManagerContext
        {
            private FooMessage publishedMessage;

            private WeakReference weakSubscriber;

            [TestInitialize]
            public void Act()
            {
                this.Manager.Register<FooMessage>(this.Subscriber.HandleFooMessage);

                this.publishedMessage = new FooMessage()
                {
                    Content = "Some String"
                };

                this.weakSubscriber = new WeakReference(this.Subscriber);

                this.Subscriber = null;

                GC.Collect();

                this.publishedMessage = new FooMessage()
                {
                    Content = "Some String"
                };

                this.Manager.Publish(this.publishedMessage);
            }

            [TestMethod]
            public void ThenSubscriberShouldBeGarbageCollected()
            {
                Assert.IsFalse(this.weakSubscriber.IsAlive);
            }
        }

        [TestClass]
        public class GivenNewSubscriptionManager_WhenSubscribingSingleType_AndWhenSubscriberIsStillReferenced_AndWhenGCCollectionHappens_AndWhenPublishingSingleMessage : SubscriptionManagerContext
        {
            private FooMessage publishedMessage;

            [TestInitialize]
            public void Act()
            {
                this.Manager.Register<FooMessage>(this.Subscriber.HandleFooMessage);

                GC.Collect();

                this.publishedMessage = new FooMessage()
                {
                    Content = "Some String"
                };

                this.Manager.Publish(this.publishedMessage);

                Thread.Sleep(10);
            }

            [TestMethod]
            public void ThenSubscriberRecievesSingleMessage()
            {
                Assert.AreEqual(1, this.Subscriber.PublishedFoos.Count);
            }

            [TestMethod]
            public void ThenPublisedMessageIsCorrect()
            {
                Assert.AreEqual(this.publishedMessage.Content, this.Subscriber.PublishedFoos[0].Content);
            }
        }

        [TestClass]
        public class SubscriptionManagerContext
        {
            public SubscriptionManagerContext()
            {
                if (SynchronizationContext.Current == null)
                {
                    SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                }

                this.Manager = new SubscriptionManager();

                this.Subscriber = new MessageSubscriber();
            }

            internal SubscriptionManager Manager { get; set; }

            internal MessageSubscriber Subscriber { get; set; }

            public class MessageSubscriber
            {
                public MessageSubscriber()
                {
                    this.PublishedBars = new List<BarMessage>();
                    this.PublishedFoos = new List<FooMessage>();
                }
                
                public void HandleFooMessage(FooMessage message)
                {
                    this.PublishedFoos.Add(message);
                }

                public void HandleBarMessage(BarMessage message)
                {
                    this.PublishedBars.Add(message);
                }

                public List<FooMessage> PublishedFoos { get; set; }
                
                public List<BarMessage> PublishedBars { get; set; }
            }
            
            public class FooMessage
            {
                public string Content { get; set; }
            }

            public class BarMessage
            {
                public int Content { get; set; }
            }
        }
    }
}
