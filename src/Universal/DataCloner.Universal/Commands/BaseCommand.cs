using System;

namespace DataCloner.Universal.Commands
{
    /// <summary>
    /// Base command for relay commands.
    /// </summary>
    public class BaseCommand
    {
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Intializes BaseCommand for relay commands.
        /// </summary>
        /// <param name="canExecute">If set to true, command can be executed. Otherwise, false.</param>
        protected BaseCommand(Func<bool> canExecute)
        {
            _canExecute = canExecute;
        }

        /// <summary>
        /// Determines whether this <see cref="RelayCommand" /> can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
        /// </param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        /// <summary>
        /// Raised when RaiseCanExecuteChanged is called.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Method used to raise the <see cref="CanExecuteChanged" /> event
        /// to indicate that the return value of the <see cref="CanExecute" />
        /// method has changed.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
