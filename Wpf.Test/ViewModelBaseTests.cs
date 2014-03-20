namespace ManagerApp.FrameworkTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using DesignSurface.App.Framework.Wpf;
    using ManagerApp.TestUtilities.Sample;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Wpf.Test;

    /// <summary>
    /// <see cref="ViewModelBase"/> Tests.
    /// </summary>
    [TestClass]
    public class ViewModelBaseTests
    {        
        /// <summary>
        /// Given View Model With No Errors
        /// </summary>
        [TestClass]
        public class GivenViewModelWithNoErrors
        {
            /// <summary>
            /// The view model.
            /// </summary>
            private SampleViewModel viewModel;

            /// <summary>
            /// Arranges this test.
            /// </summary>
            [TestInitialize]
            public void Arrange()
            {
                this.viewModel = new SampleViewModel(new Mock<IAppContext>().Object);
            }

            /// <summary>
            /// Then the error property returns empty string.
            /// </summary>
            [TestMethod]
            public void ThenErrorPropertyReturnsEmptyString()
            {
                Assert.IsTrue(this.viewModel.Error.Equals(string.Empty, StringComparison.OrdinalIgnoreCase));
            }

            /// <summary>
            /// Then for each property on model index lookup on property name returns empty string.
            /// </summary>
            [TestMethod]
            public void ThenForEachPropertyOnModelIndexLookupOnPropertyNameReturnsEmptyString()
            {
                Assert.IsTrue(string.Empty.Equals(this.viewModel[@"SomeInt"]));
                Assert.IsTrue(string.Empty.Equals(this.viewModel[@"SomeFoo"]));
                Assert.IsTrue(string.Empty.Equals(this.viewModel[@"NonObservedInt"]));
                Assert.IsTrue(string.Empty.Equals(this.viewModel[@"JointObservedInt"]));
                Assert.IsTrue(string.Empty.Equals(this.viewModel[@"SomeCollection"]));
            }

            /// <summary>
            /// Thens the on index lookup on non existing property name returns empty string.
            /// </summary>
            [TestMethod]
            public void ThenOnIndexLookupOnNonExistingPropertyNameReturnsEmptyString()
            {
                Assert.AreEqual(string.Empty, this.viewModel[@"Blah"]);
            }
        }

        /// <summary>
        /// Given View Model With Error.
        /// </summary>
        [TestClass]
        public class GivenViewModelWithError
        {
            /// <summary>
            /// The properties with errors.
            /// </summary>
            private string propertyWithError;

            /// <summary>
            /// The error message
            /// </summary>
            private string errorMessage;

            /// <summary>
            /// The view model.
            /// </summary>
            private SampleViewModel viewModel;

            /// <summary>
            /// Arranges this test.
            /// </summary>
            [TestInitialize]
            public void Arrange()
            {
                this.viewModel = new SampleViewModel(new Mock<IAppContext>().Object);

                this.propertyWithError = "SomeInt";
                this.errorMessage = "Some int error.";

                this.viewModel.PublicAddPropertyError(() => this.viewModel.SomeInt, this.errorMessage);
            }

            /// <summary>
            /// Then the error property returns non empty error message.
            /// </summary>
            [TestMethod]
            public void ThenErrorPropertyReturnsNonEmptyErrorMessage()
            {
                Assert.IsTrue(!string.IsNullOrWhiteSpace(this.viewModel.Error));
            }

            /// <summary>
            /// Then for each non error property on model index lookup on property name returns empty string.
            /// </summary>
            [TestMethod]
            public void ThenForEachNonErrorPropertyOnModelIndexLookupOnPropertyNameReturnsEmptyString()
            {
                Assert.IsTrue(string.Empty.Equals(this.viewModel[@"SomeFoo"]));
                Assert.IsTrue(string.Empty.Equals(this.viewModel[@"NonObservedInt"]));
                Assert.IsTrue(string.Empty.Equals(this.viewModel[@"JointObservedInt"]));
                Assert.IsTrue(string.Empty.Equals(this.viewModel[@"SomeCollection"]));
            }

            /// <summary>
            /// Thens the on index lookup on non existing property name returns empty string.
            /// </summary>
            [TestMethod]
            public void ThenOnIndexLookupOnNonExistingPropertyNameReturnsEmptyString()
            {
                Assert.AreEqual(string.Empty, this.viewModel[@"Blah"]);
            }

            /// <summary>
            /// Then the index lookup for error index lookup returns error message.
            /// </summary>
            [TestMethod]
            public void ThenIndexLookupForErrorIndexLookupReturnsErrorMessage()
            {
                Assert.IsTrue(this.errorMessage.Equals(this.viewModel[this.propertyWithError], StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
