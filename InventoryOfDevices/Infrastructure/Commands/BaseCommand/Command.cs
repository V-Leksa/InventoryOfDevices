﻿using System.Windows.Input;

namespace InventoryOfDevices.Infrastructure.Commands.BaseCommand
{
    public abstract class Command : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add=>CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public abstract bool CanExecute(object? parameter);

        public abstract void Execute(object? parameter);
        
    }
}
