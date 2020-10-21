using PieterP.ScoreSheet.ViewModels.Commands.Bases;

namespace PieterP.ScoreSheet.ViewModels.Commands
{
    /// <summary>
    /// This command handles the request to quit the application.
    /// </summary>
    class QuitCommand : BaseMainWindowCommand
    {
        public QuitCommand(MainWindowViewModel mainWindowViewModel) : base(mainWindowViewModel) { }

        /// <inheritdoc/>
        public override void Execute(object parameter)
        {
            if (_mainWindowViewModel.DoQuit.CanExecute(parameter))
                _mainWindowViewModel.DoQuit.Execute(parameter);
        }
    }
}
