using System;
using System.Windows.Input;

namespace PieterP.ScoreSheet.ViewModels.Commands.Bases
{
    internal class BaseMainWindowCommand: ICommand
    {
        public event EventHandler? CanExecuteChanged;
        protected MainWindowViewModel _mainWindowViewModel;

        public BaseMainWindowCommand(MainWindowViewModel mainWindowViewModel) => _mainWindowViewModel = mainWindowViewModel;

        /// <summary>
        /// Checks whether the command can be executed.
        /// </summary>
        /// <param name="_">A parameter.</param>
        /// <returns>True if the command can be executed.</returns>
        public virtual bool CanExecute(object _) => true;

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="_">A paramater.</param>
        public virtual void Execute(object _) => throw new NotImplementedException();

        /// <summary>
        /// Invokes the CanExecuteChanged event.
        /// </summary>
        protected void RaiseCanExecuteChanged(object sender, EventArgs e) => RaiseCanExecuteChanged();

        /// <summary>
        /// Invokes the CanExecuteChanged event.
        /// </summary>
        internal void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());
    }
}
