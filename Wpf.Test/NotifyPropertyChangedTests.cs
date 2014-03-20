namespace DesignSurface.App.Framework.Test
{
    using System.Collections.ObjectModel;
    using DesignSurface.App.Framework.Wpf;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NotifyPropertyChangedTests
    {
        [TestMethod]
        public void GivenParentObjectListingToChildObject_WhenChildObjectIsUpdated_ThenParentIsChangedIsTrue()
        {
            ParentFoo parent = new ParentFoo
            {
                SomeBar = new ChildBar(),
                IsChanged = false
            };

            Assert.AreEqual(false, parent.IsChanged);

            parent.SomeBar.SomeDouble = 123.456;

            Assert.AreEqual(true, parent.IsChanged);
        }

        [TestMethod]
        public void GivenParentObjectListingToChildCollection_WhenChildCollectionIsUpdated_ThenParentIsChangedIsTrue()
        {
            ParentFoo parent = new ParentFoo {IsChanged = false};

            Assert.AreEqual(false, parent.IsChanged);

            parent.SomeCollection.Add(new ChildBar());

            Assert.AreEqual(true, parent.IsChanged);
        }

        private class ChildBar : NotifyPropertyChanged
        {
            private double someDouble;
            private string someString;

            public ChildBar()
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
        }

        private class ParentFoo : NotifyPropertyChanged
        {
            private readonly ObservableCollection<ChildBar> someBarCollection = new ObservableCollection<ChildBar>();
            private ChildBar someBar;

            public ParentFoo()
            {
                this.ListenToChanges(() => this.SomeBar);
                this.ListenToChanges(() => this.SomeCollection);
            }

            public ChildBar SomeBar
            {
                get { return someBar; }
                set { someBar = value; this.OnPropertyChanged(); }
            }

            public ObservableCollection<ChildBar> SomeCollection
            {
                get { return this.someBarCollection; }
            }
        }
    }
}