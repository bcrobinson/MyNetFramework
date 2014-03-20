namespace Library.Wpf.Test
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Library.Wpf;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NotifyPropertyChangedTests
    {
        [TestMethod]
        public void GivenClassWithCascadedProperty_WhenFirstPropertyIsUpdated_ThenCascadedPropertyAlsoHasUpdateRaised()
        {
            var cascadingClass = new ClassWithCascade();

            List<string> updatedProperties = new List<string>();

            cascadingClass.PropertyChanged += (sender, args) => updatedProperties.Add(args.PropertyName);

            cascadingClass.IntProperty = 10;

            Assert.AreEqual(2, updatedProperties.Count);
            CollectionAssert.AreEquivalent(updatedProperties, new[] { "IntProperty", "CascadedProperty" });
        }

        [TestMethod]
        public void GivenClassWithMulitLevelPropertyCascades_WhenFirstPropertyIsUpdated_ThenAllCascadedPropertiesHaveUpdatesRaised()
        {
            var cascadingClass = new ClassWithMulitpleCascades();

            List<string> updatedProperties = new List<string>();

            cascadingClass.PropertyChanged += (sender, args) => updatedProperties.Add(args.PropertyName);

            cascadingClass.IntProperty = 10;

            Assert.AreEqual(3, updatedProperties.Count);
            CollectionAssert.AreEquivalent(updatedProperties, new[] { "IntProperty", "CascadedProperty", "MultiLevelCascadedProperty" });
        }

        [TestMethod]
        public void GivenClassWithMulitLevelPropertyCascades_WhenSecondPropertyIsUpdated_ThenOnlySecondAndThirdPropertiesHaveUpdatesRaised()
        {
            var cascadingClass = new ClassWithMulitpleCascades();

            List<string> updatedProperties = new List<string>();

            cascadingClass.PropertyChanged += (sender, args) => updatedProperties.Add(args.PropertyName);

            cascadingClass.CascadedProperty = 10;

            Assert.AreEqual(2, updatedProperties.Count);
            CollectionAssert.AreEquivalent(updatedProperties, new[] { "CascadedProperty", "MultiLevelCascadedProperty" });
        }

        [TestMethod]
        public void GivenClassWithPropertyCascadeCycle_WhenFirstPropertyIsUpdated_ThenSecondPropertyHasupdatedRaisedOnceOnly()
        {
            var cascadingClass = new ClassCascadeCycle();

            List<string> updatedProperties = new List<string>();

            cascadingClass.PropertyChanged += (sender, args) => updatedProperties.Add(args.PropertyName);

            cascadingClass.FirstProperty = 10;

            Assert.AreEqual(2, updatedProperties.Count);
            CollectionAssert.AreEquivalent(updatedProperties, new[] { "FirstProperty", "SecondProperty" });
        }

        [TestMethod]
        public void GivenParentObjectListingToChildCollection_WhenChildCollectionIsUpdated_ThenParentIsChangedIsTrue()
        {
            ParentFoo parent = new ParentFoo { IsChanged = false };

            Assert.AreEqual(false, parent.IsChanged);

            parent.SomeCollection.Add(new ChildBar());

            Assert.AreEqual(true, parent.IsChanged);
        }

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

        private class ClassCascadeCycle : NotifyPropertyChanged
        {
            private int firstProperty;

            private int secondProperty;

            public ClassCascadeCycle()
            {
                this.CascadeUpdate(() => this.FirstProperty, () => this.SecondProperty);
                this.CascadeUpdate(() => this.SecondProperty, () => this.FirstProperty);
            }

            public int FirstProperty
            {
                get { return this.firstProperty; }
                set { this.firstProperty = value; this.OnPropertyChanged(); }
            }

            public int SecondProperty
            {
                get { return this.secondProperty; }
                set { this.secondProperty = value; this.OnPropertyChanged(); }
            }
        }

        private class ClassWithCascade : NotifyPropertyChanged
        {
            private int cascadedProperty;
            private int intProperty;

            public ClassWithCascade()
            {
                this.CascadeUpdate(() => this.IntProperty, () => this.CascadedProperty);
            }

            public int CascadedProperty
            {
                get { return this.cascadedProperty; }
                set { this.cascadedProperty = value; this.OnPropertyChanged(); }
            }

            public int IntProperty
            {
                get { return this.intProperty; }
                set { this.intProperty = value; this.OnPropertyChanged(); }
            }
        }

        private class ClassWithMulitpleCascades : NotifyPropertyChanged
        {
            private int cascadedProperty;
            private int intProperty;
            private int multiLevelCascadedProperty;

            public ClassWithMulitpleCascades()
            {
                this.CascadeUpdate(() => this.IntProperty, () => this.CascadedProperty);
                this.CascadeUpdate(() => this.CascadedProperty, () => this.MultiLevelCascadedProperty);
            }

            public int CascadedProperty
            {
                get { return this.cascadedProperty; }
                set { this.cascadedProperty = value; this.OnPropertyChanged(); }
            }

            public int IntProperty
            {
                get { return this.intProperty; }
                set { this.intProperty = value; this.OnPropertyChanged(); }
            }

            public int MultiLevelCascadedProperty
            {
                get { return this.multiLevelCascadedProperty; }
                set { this.multiLevelCascadedProperty = value; this.OnPropertyChanged(); }
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