namespace Library.Wpf.Test
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SortedObservableCollectionTests
    {
        #region Add

        [TestClass]
        public class GivenEmptySortedObservableCollection_WhenAddingSingleValue : SortedObservableCollectionContext
        {
            private Foo addedFoo;

            [TestInitialize]
            public void Act()
            {
                this.addedFoo = new Foo()
                {
                    Key = 1,
                    Value = "Value"
                };

                this.Collection.Add(this.addedFoo);
            }

            [TestMethod]
            public void ThenCollectionCountIsOne()
            {
                Assert.AreEqual(1, this.Collection.Count);
            }

            [TestMethod]
            public void ThenExactlyCollectionChangedEventIsCorrect()
            {
                NotifyCollectionChangedEventArgs eventArgs = this.CollectionChangedEvents[0];

                Assert.AreEqual(NotifyCollectionChangedAction.Add, eventArgs.Action);
                Assert.AreEqual(0, eventArgs.NewStartingIndex);

                Assert.IsNull(eventArgs.OldItems);
                Assert.AreEqual(-1, eventArgs.OldStartingIndex);

                Assert.AreEqual(1, eventArgs.NewItems.Count);
                Assert.IsTrue(object.ReferenceEquals(this.addedFoo, eventArgs.NewItems[0]));
            }

            [TestMethod]
            public void ThenExactlyOneCollectionChangedEventIsRaised()
            {
                Assert.AreEqual(1, this.CollectionChangedEvents.Count);
            }

            [TestMethod]
            public void ThenFirstItemIsCorrectFoo()
            {
                Assert.IsTrue(object.ReferenceEquals(this.addedFoo, this.Collection.First()));
            }
        }

        [TestClass]
        public class GivenEmptySortedObservableCollection_WhenAddingTenItemsInOrder : SortedObservableCollectionContext
        {
            private List<Foo> addedFoos;

            [TestInitialize]
            public void Act()
            {
                this.addedFoos = Enumerable
                    .Range(0, 10)
                    .Select(i =>
                        new Foo()
                        {
                            Key = i,
                            Value = "Value" + i
                        }).ToList();

                foreach (var foo in this.addedFoos)
                {
                    this.Collection.Add(foo);
                }
            }

            [TestMethod]
            public void ThenCollectionCountIsTen()
            {
                Assert.AreEqual(10, this.Collection.Count);
            }

            [TestMethod]
            public void ThenExactlyTenCollectionChangedEventAreRaised()
            {
                Assert.AreEqual(10, this.CollectionChangedEvents.Count);
            }

            [TestMethod]
            public void ThenExactlyTenCollectionChangedEventsAreCorrect()
            {
                int i = 0;
                foreach (var foo in this.Collection)
                {
                    NotifyCollectionChangedEventArgs eventArgs = this.CollectionChangedEvents[i];

                    Assert.AreEqual(NotifyCollectionChangedAction.Add, eventArgs.Action);
                    Assert.AreEqual(i, eventArgs.NewStartingIndex);

                    Assert.IsNull(eventArgs.OldItems);
                    Assert.AreEqual(-1, eventArgs.OldStartingIndex);

                    Assert.AreEqual(1, eventArgs.NewItems.Count);
                    Assert.IsTrue(object.ReferenceEquals(foo, eventArgs.NewItems[0]));
                    i++;
                }
            }

            [TestMethod]
            public void ThenFoosAreInCorrectOrder()
            {
                int i = 0;
                foreach (var foo in this.Collection)
                {
                    Assert.IsTrue(object.ReferenceEquals(this.addedFoos[i++], foo));
                }
            }
        }

        [TestClass]
        public class GivenEmptySortedObservableCollection_WhenAddingTenItemsOutOfOrder : SortedObservableCollectionContext
        {
            private List<Foo> addedFoos;

            [TestInitialize]
            public void Act()
            {
                this.addedFoos = Enumerable
                    .Range(0, 10)
                    .Select(i =>
                        new Foo()
                        {
                            Key = i,
                            Value = "Value" + i
                        }).ToList();

                Random rand = new Random();
                foreach (var foo in this.addedFoos.OrderBy(i => rand.Next()))
                {
                    this.Collection.Add(foo);
                }
            }

            [TestMethod]
            public void ThenCollectionCountIsTen()
            {
                Assert.AreEqual(10, this.Collection.Count);
            }

            [TestMethod]
            public void ThenExactlyTenCollectionChangedEventAreRaised()
            {
                Assert.AreEqual(10, this.CollectionChangedEvents.Count);
            }

            [TestMethod]
            public void ThenExactlyTenCollectionChangedEventsAreCorrect()
            {
                List<NotifyCollectionChangedEventArgs> orderedCollectionChangedEvents = this.CollectionChangedEvents.OrderBy(e => ((Foo)e.NewItems[0]).Key).ToList();

                int i = 0;

                foreach (var foo in this.Collection)
                {
                    NotifyCollectionChangedEventArgs eventArgs = orderedCollectionChangedEvents[i];

                    Assert.AreEqual(NotifyCollectionChangedAction.Add, eventArgs.Action);

                    Assert.IsNull(eventArgs.OldItems);
                    Assert.AreEqual(-1, eventArgs.OldStartingIndex);

                    Assert.AreEqual(1, eventArgs.NewItems.Count);
                    Assert.IsTrue(object.ReferenceEquals(foo, eventArgs.NewItems[0]));
                    i++;
                }
            }

            [TestMethod]
            public void ThenFoosAreInCorrectOrder()
            {
                int i = 0;
                foreach (var foo in this.Collection)
                {
                    Assert.IsTrue(object.ReferenceEquals(this.addedFoos[i++], foo));
                }
            }
        }

        [TestClass]
        public class GivenNonEmptySortedObservableCollection_WhenAddingMultipleValuesInMiddleInRandomOrder : SortedObservableCollectionContext
        {
            private List<Foo> removedFoos;

            [TestInitialize]
            public void Act()
            {
                var foos = Enumerable
                    .Range(1, 100)
                    .Where(i => i % 2 == 0)
                    .Select(i =>
                        new Foo()
                        {
                            Key = i,
                            Value = "Value" + i
                        });

                foreach (var foo in foos)
                {
                    this.Collection.Add(foo);
                }

                this.CollectionChangedEvents.Clear();

                this.removedFoos = Enumerable
                    .Range(1, 100)
                    .Where(i => i % 2 != 0)
                    .Select(i =>
                        new Foo()
                        {
                            Key = i,
                            Value = "Value" + i
                        }).ToList();

                Random rand = new Random();
                foreach (var foo in this.removedFoos.OrderBy(f => rand.Next()))
                {
                    this.Collection.Add(foo);
                }
            }

            [TestMethod]
            public void ThenCollectionCountIs100()
            {
                Assert.AreEqual(100, this.Collection.Count);
            }

            [TestMethod]
            public void ThenExactly50CollectionChangedEventsAreRaised()
            {
                Assert.AreEqual(50, this.CollectionChangedEvents.Count);
            }
        }

        [TestClass]
        public class GivenNonEmptySortedObservableCollection_WhenAddingSingleValueAtBeginning : SortedObservableCollectionContext
        {
            private Foo addedFoo;

            [TestInitialize]
            public void Act()
            {
                var foos = Enumerable
                    .Range(1, 10)
                    .Select(i =>
                        new Foo()
                        {
                            Key = i,
                            Value = "Value" + i
                        });

                foreach (var foo in foos)
                {
                    this.Collection.Add(foo);
                }

                this.CollectionChangedEvents.Clear();

                this.addedFoo = new Foo()
                {
                    Key = 0,
                    Value = "Value"
                };

                this.Collection.Add(this.addedFoo);
            }

            [TestMethod]
            public void ThenCollectionChangedEventIsCorrect()
            {
                NotifyCollectionChangedEventArgs eventArgs = this.CollectionChangedEvents[0];

                Assert.AreEqual(NotifyCollectionChangedAction.Add, eventArgs.Action);
                Assert.AreEqual(0, eventArgs.NewStartingIndex);

                Assert.IsNull(eventArgs.OldItems);
                Assert.AreEqual(-1, eventArgs.OldStartingIndex);

                Assert.AreEqual(1, eventArgs.NewItems.Count);
                Assert.IsTrue(object.ReferenceEquals(this.addedFoo, eventArgs.NewItems[0]));
            }

            [TestMethod]
            public void ThenCollectionCountIsEleven()
            {
                Assert.AreEqual(11, this.Collection.Count);
            }

            [TestMethod]
            public void ThenExactlyOneCollectionChangedEventIsRaised()
            {
                Assert.AreEqual(1, this.CollectionChangedEvents.Count);
            }

            [TestMethod]
            public void ThenFirstItemIsCorrectFoo()
            {
                Assert.IsTrue(object.ReferenceEquals(this.addedFoo, this.Collection.First()));
            }
        }

        [TestClass]
        public class GivenNonEmptySortedObservableCollection_WhenAddingSingleValueAtEnd : SortedObservableCollectionContext
        {
            private Foo addedFoo;

            [TestInitialize]
            public void Act()
            {
                var foos = Enumerable
                    .Range(0, 10)
                    .Select(i =>
                        new Foo()
                        {
                            Key = i,
                            Value = "Value" + i
                        });

                foreach (var foo in foos)
                {
                    this.Collection.Add(foo);
                }

                this.CollectionChangedEvents.Clear();

                this.addedFoo = new Foo()
                {
                    Key = 11,
                    Value = "Value"
                };

                this.Collection.Add(this.addedFoo);
            }

            [TestMethod]
            public void ThenCollectionChangedEventIsCorrect()
            {
                NotifyCollectionChangedEventArgs eventArgs = this.CollectionChangedEvents[0];

                Assert.AreEqual(NotifyCollectionChangedAction.Add, eventArgs.Action);
                Assert.AreEqual(10, eventArgs.NewStartingIndex);

                Assert.IsNull(eventArgs.OldItems);
                Assert.AreEqual(-1, eventArgs.OldStartingIndex);

                Assert.AreEqual(1, eventArgs.NewItems.Count);
                Assert.IsTrue(object.ReferenceEquals(this.addedFoo, eventArgs.NewItems[0]));
            }

            [TestMethod]
            public void ThenCollectionCountIsEleven()
            {
                Assert.AreEqual(11, this.Collection.Count);
            }

            [TestMethod]
            public void ThenExactlyOneCollectionChangedEventIsRaised()
            {
                Assert.AreEqual(1, this.CollectionChangedEvents.Count);
            }

            [TestMethod]
            public void ThenLastItemIsCorrectFoo()
            {
                Assert.IsTrue(object.ReferenceEquals(this.addedFoo, this.Collection.Last()));
            }
        }

        [TestClass]
        public class GivenNonEmptySortedObservableCollection_WhenAddingSingleValueInMiddle : SortedObservableCollectionContext
        {
            private Foo addedFoo;

            [TestInitialize]
            public void Act()
            {
                var foos = Enumerable
                    .Range(1, 11)
                    .Where(i => i != 5)
                    .Select(i =>
                        new Foo()
                        {
                            Key = i,
                            Value = "Value" + i
                        });

                foreach (var foo in foos)
                {
                    this.Collection.Add(foo);
                }

                this.CollectionChangedEvents.Clear();

                this.addedFoo = new Foo()
                {
                    Key = 5,
                    Value = "Value"
                };

                this.Collection.Add(this.addedFoo);
            }

            [TestMethod]
            public void ThenCollectionChangedEventIsCorrect()
            {
                NotifyCollectionChangedEventArgs eventArgs = this.CollectionChangedEvents[0];

                Assert.AreEqual(NotifyCollectionChangedAction.Add, eventArgs.Action);
                Assert.AreEqual(4, eventArgs.NewStartingIndex);

                Assert.IsNull(eventArgs.OldItems);
                Assert.AreEqual(-1, eventArgs.OldStartingIndex);

                Assert.AreEqual(1, eventArgs.NewItems.Count);
                Assert.IsTrue(object.ReferenceEquals(this.addedFoo, eventArgs.NewItems[0]));
            }

            [TestMethod]
            public void ThenCollectionCountIsEleven()
            {
                Assert.AreEqual(11, this.Collection.Count);
            }

            [TestMethod]
            public void ThenExactlyOneCollectionChangedEventIsRaised()
            {
                Assert.AreEqual(1, this.CollectionChangedEvents.Count);
            }

            [TestMethod]
            public void ThenFithItemIsCorrectFoo()
            {
                Assert.IsTrue(object.ReferenceEquals(this.addedFoo, this.Collection.Skip(4).First()));
            }
        }

        #endregion Add

        #region Remove

        [TestClass]
        public class GivenEmptySortedObservableCollection_WhenRemovingSingleValue : SortedObservableCollectionContext
        {
            private Foo addedFoo;

            [TestInitialize]
            public void Act()
            {
                this.addedFoo = new Foo()
                {
                    Key = 1,
                    Value = "Value"
                };

                this.Collection.Remove(this.addedFoo);
            }

            [TestMethod]
            public void ThenCollectionCountIsZero()
            {
                Assert.AreEqual(0, this.Collection.Count);
            }

            [TestMethod]
            public void ThenEnumeratorHasNoValues()
            {
                var enumerator = this.Collection.GetEnumerator();

                Assert.IsFalse(enumerator.MoveNext());
            }

            [TestMethod]
            public void ThenExactlyNoCollectionChangedEventsAreRaised()
            {
                Assert.AreEqual(0, this.CollectionChangedEvents.Count);
            }
        }

        [TestClass]
        public class GivenNonEmptySortedObservableCollection_WhenRemovingMultipleValuesInMiddleInRandomOrder : SortedObservableCollectionContext
        {
            private List<Foo> removedFoos;

            [TestInitialize]
            public void Act()
            {
                var foos = Enumerable
                    .Range(1, 100)
                    .Select(i =>
                        new Foo()
                        {
                            Key = i,
                            Value = "Value" + i
                        });

                foreach (var foo in foos)
                {
                    this.Collection.Add(foo);
                }

                this.CollectionChangedEvents.Clear();

                this.removedFoos = this.Collection
                    .Where(f => f.Key % 2 == 0)
                    .ToList();

                Random rand = new Random();
                foreach (var foo in this.removedFoos.OrderBy(f => rand.Next()))
                {
                    this.Collection.Remove(foo);
                }
            }

            [TestMethod]
            public void Then50CollectionChangedEventsAreRaised()
            {
                Assert.AreEqual(50, this.CollectionChangedEvents.Count);
            }

            [TestMethod]
            public void ThenCollectionChangedEventsAreCorrect()
            {
                foreach (var removeEvent in this.CollectionChangedEvents)
                {
                    Assert.AreEqual(NotifyCollectionChangedAction.Remove, removeEvent.Action);

                    Assert.AreEqual(-1, removeEvent.NewStartingIndex);
                    Assert.IsNull(removeEvent.NewItems);

                    Assert.AreNotEqual(-1, removeEvent.OldStartingIndex);

                    Assert.AreEqual(1, removeEvent.OldItems.Count);

                    Foo eventRemovedFoo = (Foo)removeEvent.OldItems[0];
                    var removedFoo = this.removedFoos.Single(f => f.Key == eventRemovedFoo.Key);
                    Assert.IsTrue(object.ReferenceEquals(removedFoo, eventRemovedFoo));
                }
            }

            [TestMethod]
            public void ThenCollectionCountIs50()
            {
                Assert.AreEqual(50, this.Collection.Count);
            }

            [TestMethod]
            public void ThenCollectionIsInOrder()
            {
                var collectionEnumerator = this.Collection.GetEnumerator();

                int count = 0;

                int previousKey = int.MinValue;

                while (collectionEnumerator.MoveNext())
                {
                    var foo = collectionEnumerator.Current;

                    Assert.IsTrue(previousKey < foo.Key);

                    previousKey = foo.Key;

                    count++;
                    Assert.IsTrue(count <= 50);
                }

                Assert.AreEqual(50, count);
                Assert.AreEqual(this.Collection.Max(f => f.Key), previousKey);
            }

            [TestMethod]
            public void ThenCollectionOnlyContainsOddFoos()
            {
                var collectionEnumerator = this.Collection.GetEnumerator();

                int count = 0;

                while (collectionEnumerator.MoveNext())
                {
                    var foo = collectionEnumerator.Current;

                    Assert.IsTrue(foo.Key % 2 != 0);

                    count++;
                    Assert.IsTrue(count <= 50);
                }

                Assert.AreEqual(50, count);
            }
        }

        #endregion Remove

        [TestClass]
        public abstract class SortedObservableCollectionContext
        {
            public SortedObservableCollectionContext()
            {
                this.Collection = new SortedObservableCollection<int, Foo>(f => f.Key, Comparer<int>.Default);

                this.CollectionChangedEvents = new List<NotifyCollectionChangedEventArgs>();

                this.Collection.CollectionChanged += OnCollectionChanged;
            }

            protected SortedObservableCollection<int, Foo> Collection { get; set; }

            protected List<NotifyCollectionChangedEventArgs> CollectionChangedEvents { get; set; }

            private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                this.CollectionChangedEvents.Add(e);
            }

            [DebuggerDisplay("Key={Key}, Value={Value}")]
            protected class Foo : NotifyPropertyChanged
            {
                public int Key { get; set; }

                public string Value { get; set; }
            }
        }
    }
}