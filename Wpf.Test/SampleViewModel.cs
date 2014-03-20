namespace Library.Wpf.Test
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using Library.Wpf;

    internal class SampleViewModel : ViewModelBase
    {
        private ObservableCollection<int> someCollection;

        private Foo someFoo;

        private int someInt;

        public SampleViewModel(IAppContext appCtx)
            : base(appCtx)
        {
        }

        public ObservableCollection<int> SomeCollection
        {
            get { return this.someCollection; }
            set { this.someCollection = value; this.OnPropertyChanged(); }
        }

        public Foo SomeFoo
        {
            get { return this.someFoo; }
            set { this.someFoo = value; this.OnPropertyChanged(); }
        }

        public int SomeInt
        {
            get { return this.someInt; }
            set { this.someInt = value; this.OnPropertyChanged(); }
        }

        public void PublicAddPropertyError<T>(Expression<Func<T>> property, string error)
        {
            this.AddPropertyError(property, error);
        }
    }
}