using System;
using System.Windows.Input;

namespace Ienemumu_Parvaldibas_Sistema
{
    public class SimpleCommand : ICommand
    {
        private readonly Action _execute;

        public SimpleCommand(Action execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();

        public event EventHandler? CanExecuteChanged;
    }
}
