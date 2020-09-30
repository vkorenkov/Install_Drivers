using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Install_Drivers
{
    /// <summary>
    /// Класс наследует интерфейс ICommand и производит обработку входящих комманд для из выполнения
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> execute;

        private readonly Func<T, bool> canExecute;

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

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            execute((T)parameter);
        }
    }
}
