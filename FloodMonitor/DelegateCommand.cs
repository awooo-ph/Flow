using System;
using System.Windows.Input;

    public class DelegateCommand<T> : ICommand
    {
        private Action<T> _execute;
        private Predicate<T> _canExecute;

        private event EventHandler CanExecuteChangedInternal;

        public DelegateCommand(Action<T> execute)
            : this(execute, DefaultCanExecute)
        {
        }

        public DelegateCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            if (canExecute == null)
            {
                throw new ArgumentNullException(nameof(canExecute));
            }

            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                this.CanExecuteChangedInternal += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
                this.CanExecuteChangedInternal -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            //try
            //{
            if (parameter != null && typeof(T) != parameter.GetType()) return false;
            return _canExecute != null && _canExecute((T)parameter);
            //}
            //catch (Exception)
            //{
            //  return false;
            //}
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public void OnCanExecuteChanged()
        {
            var handler = this.CanExecuteChangedInternal;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public void Destroy()
        {
            _canExecute = _ => false;
            _execute = _ => { return; };
        }

        private static bool DefaultCanExecute(T parameter)
        {
            return true;
        }
    }

    public class DelegateCommand : DelegateCommand<object>
    {
        public DelegateCommand(Action<object> execute) : base(execute)
        {
        }

        public DelegateCommand(Action<object> execute, Predicate<object> canExecute) : base(execute, canExecute)
        {
        }
    }