using System.Windows.Input;

namespace InventoryOfDevices.Infrastructure.Commands.BaseCommand
{
    public class AsyncCommand: ICommand
    {
        private readonly Func<Task> _execute;

        public AsyncCommand(Func<Task> execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public async void Execute(object? parameter)
        {
            await _execute();
        }
    }
}
