namespace Library.Wpf
{
    using System;
    using System.Diagnostics;
    using System.Windows.Input;

    public class RelayCommand : ICommand
    {
        private readonly Func<object, bool> canExecute;

        private readonly Action<object> execute;

        private readonly Func<object, Exception, bool> handleException;

        public RelayCommand(Action<object> execute)
            : this(execute, o => true, (o, e) => false)
        {
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
            : this(execute, canExecute, (o, e) => false)
        {
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute, Func<object, Exception, bool> handleException)
        {
            this.execute = execute;
            this.canExecute = canExecute;
            this.handleException = handleException;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return this.canExecute(parameter);
        }

        [DebuggerStepThrough]
        public void Execute(object parameter)
        {
            if (this.canExecute(parameter))
            {
                try
                {
                    this.execute(parameter);
                }
                catch (Exception e)
                {
                    if (!this.handleException(parameter, e))
                    {
                        throw;
                    }
                }
            }
        }

        [DebuggerStepThrough]
        bool ICommand.CanExecute(object parameter)
        {
            return this.CanExecute(parameter);
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { this.CanExecuteChanged += value; }
            remove { this.CanExecuteChanged -= value; }
        }

        [DebuggerStepThrough]
        void ICommand.Execute(object parameter)
        {
            this.Execute(parameter);
        }
    }
}
