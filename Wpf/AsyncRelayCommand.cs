namespace Library.Wpf
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public sealed class AsyncRelayCommand : ICommand
    {
        private readonly Func<object, bool> canExecute;

        private readonly Func<object, Task> execute;

        private readonly Func<object, Exception, bool> handleException;

        public AsyncRelayCommand(Func<object, Task> execute)
            : this(execute, o => true, (o, e) => false)
        {
        }

        public AsyncRelayCommand(Func<object, Task> execute, Func<object, bool> canExecute)
            : this(execute, canExecute, (o, e) => false)
        {
        }

        public AsyncRelayCommand(Func<object, Task> execute, Func<object, bool> canExecute, Func<object, Exception, bool> handleException)
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

        event EventHandler ICommand.CanExecuteChanged
        {
            add { this.CanExecuteChanged += value; }
            remove { this.CanExecuteChanged -= value; }
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return this.canExecute(parameter);
        }

        [DebuggerStepThrough]
        public async void Execute(object parameter)
        {
            if (this.canExecute(parameter))
            {
                try
                {
                    await this.execute(parameter);
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
        public Task ExecuteAsync(object parameter)
        {
            if (this.canExecute(parameter))
            {
                return this.execute(parameter);
            }
            else
            {
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                tcs.SetResult(true);
                return tcs.Task;
            }
        }

        [DebuggerStepThrough]
        bool ICommand.CanExecute(object parameter)
        {
            return this.CanExecute(parameter);
        }

        [DebuggerStepThrough]
        void ICommand.Execute(object parameter)
        {
            this.Execute(parameter);
        }
    }
}