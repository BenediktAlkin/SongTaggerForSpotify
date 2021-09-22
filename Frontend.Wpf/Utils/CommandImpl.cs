using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Frontend.Wpf.Utils
{
    public class CommandImpl : ICommand
    {
        private Action<object> execute { get; }
        private Func<object, bool> canExecute { get; }

        public CommandImpl(Action<object> execute)
            : this(execute, null)
        { }

        public CommandImpl(Action<object> execute, Func<object, bool> canExecute)
        {
            if (execute is null) throw new ArgumentNullException(nameof(execute));

            this.execute = execute;
            this.canExecute = canExecute ?? (x => true);
        }

        public bool CanExecute(object parameter) => canExecute(parameter);

        public void Execute(object parameter) => execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void Refresh() => CommandManager.InvalidateRequerySuggested();
    }
}
