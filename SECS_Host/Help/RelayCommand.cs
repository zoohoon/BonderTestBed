using System;
using System.Windows.Input;

namespace SECS_Host.Help
{
    public class RelayCommand : ICommand
    {
        private Action<object> action;
        public RelayCommand(Action<object> _action)
        {
            this.action = _action;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action(parameter);
        }
    }
}

