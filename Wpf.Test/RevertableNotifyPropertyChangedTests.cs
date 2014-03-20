namespace Library.Wpf.Test
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Library.Wpf;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RevertableNotifyPropertyChangedTests
    {
        [TestMethod]
        public void GivenRevertableObject_WhenPropertyChanged_ThenIsChangedFlagIsTrue()
        {
            var foo = new RevertableFoo();
            Assert.AreEqual(false, foo.IsChanged);

            foo.SomeValue = 30;
            Assert.AreEqual(true, foo.IsChanged);
        }

        [TestMethod]
        public void GivenRevertableObject_WhenPropertyChangedInCollectionOfChangeTrackedProperties_ThenIsChangedFlagIsTrue()
        {
            int[] ints = new[] { 1, 2, 3, 4, 5, 6 };
            CloneableBar[] bars = new[]
            {
                new CloneableBar() { SomeDouble = 12.3456, SomeString = "A String 1" },
                new CloneableBar() { SomeDouble = 2.34567, SomeString = "A String 2" },
                new CloneableBar() { SomeDouble = 3.45678, SomeString = "A String 3" },
                new CloneableBar() { SomeDouble = 4.56890, SomeString = "A String 4" }
            };

            var foo = new RevertableFoo(bars, ints);

            Assert.AreEqual(false, foo.IsChanged);

            foo.SomeCollection.First().SomeDouble = 999;
            Assert.AreEqual(true, foo.IsChanged);
        }

        [TestMethod]
        public void GivenRevertableObject_WhenPropertyChangedOnChangeTrackedProperty_ThenIsChangedFlagIsTrue()
        {
            var foo = new RevertableFoo();
            Assert.AreEqual(false, foo.IsChanged);

            foo.SomeClone.SomeDouble = 9999;
            Assert.AreEqual(true, foo.IsChanged);
        }

        [TestMethod]
        public void GivenRevertableObject_WhenRegisteringForValueRevertTrackingWhenValuePropertyChangedWhenRejectChangesIsCalled_ThenValueIsOriginalValue()
        {
            var foo = new RevertableFoo();
            int originalInt = foo.SomeValue;

            foo.SomeValue = 99;
            foo.RejectChanges();

            Assert.AreEqual(originalInt, foo.SomeValue);
        }

        [TestMethod]
        public void GivenRevertableObjectWhichIsChangedState_WhenChangesAreAccepted_ThenIsChangedFlagIsFalse()
        {
            var foo = new RevertableFoo
            {
                SomeValue = 30
            };

            foo.AcceptChanges();

            Assert.AreEqual(false, foo.IsChanged);
        }

        [TestMethod]
        public void GivenRevertableObjectWithCollectoion_WhenCollectionIsChangedWhenChangesAreReverted_ThenCollectionIsSameAsOriginal()
        {
            int[] originalInts = new[] { 1, 2, 3, 4, 5, 6 };
            CloneableBar[] originalBars = new[]
            {
                new CloneableBar() { SomeDouble = 12.3456, SomeString = "A String 1" },
                new CloneableBar() { SomeDouble = 2.34567, SomeString = "A String 2" },
                new CloneableBar() { SomeDouble = 3.45678, SomeString = "A String 3" },
                new CloneableBar() { SomeDouble = 4.56890, SomeString = "A String 4" }
            };

            var foo = new RevertableFoo(
                originalBars.Select(b => b.DeepClone()),
                originalInts.Select(i => i));

            foo.SomeValueCollection.Add(11);
            foo.SomeValueCollection.Add(12);
            foo.SomeValueCollection.Add(13);
            foo.SomeValueCollection.Add(14);

            foo.SomeCollection[0].SomeDouble = 999999;
            foo.SomeCollection[0].SomeString = "BLAHBLAHBLAH";

            foo.RejectChanges();

            Assert.AreEqual(false, foo.IsChanged);
            CollectionAssert.AreEquivalent(originalInts, foo.SomeValueCollection);
            Assert.IsTrue(originalBars.SequenceEqual(foo.SomeCollection));
        }

        private class CloneableBar : NotifyPropertyChanged, ICloneable<CloneableBar>, IEquatable<CloneableBar>, IChangeTracking
        {
            private double someDouble;
            private string someString;

            public CloneableBar()
            {
            }

            public double SomeDouble
            {
                get { return this.someDouble; }
                set { this.someDouble = value; this.OnPropertyChanged(); }
            }

            public string SomeString
            {
                get { return this.someString; }
                set { this.someString = value; this.OnPropertyChanged(); }
            }

            public void AcceptChanges()
            {
                this.IsChanged = false;
            }

            public CloneableBar DeepClone()
            {
                CloneableBar bar = new CloneableBar()
                {
                    SomeDouble = this.SomeDouble,
                    SomeString = this.SomeString
                };
                bar.AcceptChanges();

                return bar;
            }

            public bool Equals(CloneableBar other)
            {
                return other != null && Math.Abs(this.SomeDouble - other.SomeDouble) < 0.01 && string.Equals(this.SomeString, other.SomeString, StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                CloneableBar barObj = obj as CloneableBar;

                return barObj != null && this.Equals(barObj);
            }

            public override int GetHashCode()
            {
                int hash = this.SomeString == null ? 0 : this.SomeString.GetHashCode();
                hash ^= this.SomeDouble.GetHashCode();
                hash ^= this.IsChanged.GetHashCode();
                return hash;
            }

            object ICloneable.Clone()
            {
                return this.DeepClone();
            }
        }

        private class RevertableFoo : RevertableNotifyPropertyChanged<RevertableFoo>
        {
            private CloneableBar someBar;
            private readonly ObservableBatchCollection<CloneableBar> someCloneCollection;
            private int someInt;
            private readonly ObservableBatchCollection<int> someValueCollection;

            public RevertableFoo()
                : this(new CloneableBar() { SomeDouble = 12.3456, SomeString = "A String" }, 10)
            {
            }

            public RevertableFoo(IEnumerable<CloneableBar> bars, IEnumerable<int> ints)
                : this(new CloneableBar() { SomeDouble = 12.3456, SomeString = "A String" }, 10, bars, ints)
            {
            }

            public RevertableFoo(CloneableBar bar, int i)
                : this(new CloneableBar() { SomeDouble = 12.3456, SomeString = "A String" }, 10, new CloneableBar[0], new int[0])
            {
            }

            public RevertableFoo(CloneableBar bar, int i, IEnumerable<CloneableBar> bars, IEnumerable<int> ints)
            {
                this.someBar = bar;
                this.someInt = i;

                this.someCloneCollection = new ObservableBatchCollection<CloneableBar>(bars);
                this.someValueCollection = new ObservableBatchCollection<int>(ints);

                this.RegisterRevertTrackingValue(f => f.SomeValue);
                this.RegisterRevertTracking(f => f.SomeClone);
                this.RegisterRevertTrackingObservableCollection(f => f.SomeCollection);
                this.RegisterRevertTrackingObservableValueCollection(f => f.SomeValueCollection);

                this.AcceptChanges();
            }

            public CloneableBar SomeClone
            {
                get { return someBar; }
                set { someBar = value; this.OnPropertyChanged(); }
            }

            public ObservableBatchCollection<CloneableBar> SomeCollection
            {
                get { return this.someCloneCollection; }
            }

            public int SomeValue
            {
                get { return someInt; }
                set { someInt = value; this.OnPropertyChanged(); }
            }

            public ObservableBatchCollection<int> SomeValueCollection
            {
                get { return this.someValueCollection; }
            }
        }
    }
}