using System;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace DataCloner.Universal.Commands
{
    /// <summary>
    /// A generic relay command that allows binding commands from the UI.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    public class RelayCommand<T> : BaseCommand, ICommand
    {
        private readonly Action<T> _action;

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="action">The execution logic.</param>
        public RelayCommand(Action<T> action)
            : this(action, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="action">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> action, Func<bool> canExecute)
            : base(canExecute)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _action = action;
        }

        /// <summary>
        /// Executes the <see cref="RelayCommand" /> on the current command target.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
        /// </param>
        public void Execute(object parameter)
        {
            if (parameter is ItemClickEventArgs)
            {
                parameter = ((ItemClickEventArgs)parameter).ClickedItem;
            }

            if (parameter is T)
            {
                _action((T)parameter);
            }
        }
    }

    /// <summary>
    /// A command whose sole purpose is to relay its functionality
    /// to other objects by invoking delegates.
    /// The default return value for the CanExecute method is 'true'.
    /// </summary>
    public class RelayCommand : BaseCommand, ICommand
    {
        private readonly Action _action;

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="action">The execution logic.</param>
        public RelayCommand(Action action)
            : this(action, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="action">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action action, Func<bool> canExecute)
            : base(canExecute)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _action = action;
        }

        /// <summary>
        /// Executes the <see cref="RelayCommand" /> on the current command target.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
        /// </param>
        public void Execute(object parameter)
        {
            _action();
        }
    }
}
