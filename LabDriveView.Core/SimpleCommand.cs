using System;
using System.Windows.Input;

namespace LabDriveView.Core
{
    public class SimpleCommand : ICommand
    {
        #region ICommand Members  

        private readonly Action _action;

        public SimpleCommand(Action action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action?.Invoke();
        }

        #endregion
    }

    public class SimpleCommand<T> : ICommand
    {
        #region ICommand Members  

        private readonly Func<T, bool> _func;

        public SimpleCommand(Func<T, bool> func)
        {
            _func = func;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _func?.Invoke((T)parameter);
        }

        #endregion
    }
}
