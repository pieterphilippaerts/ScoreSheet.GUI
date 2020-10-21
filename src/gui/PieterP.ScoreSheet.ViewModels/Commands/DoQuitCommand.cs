using PieterP.ScoreSheet.ViewModels.Commands.Bases;
using System.Linq;

namespace PieterP.ScoreSheet.ViewModels.Commands
{
    /// <summary>
    /// This command is used for closing all the active matches and quit the application.
    /// </summary>
    class DoQuitCommand : BaseMainWindowCommand
    {
        public DoQuitCommand(MainWindowViewModel mainWindowViewModel) : base(mainWindowViewModel) { }

        /// <inheritdoc/>
        public override bool CanExecute(object _) => _mainWindowViewModel.CloseMatches(_mainWindowViewModel.ActiveMatches.ToList());

        /// <inheritdoc/>
        public void Execute(object _) => _mainWindowViewModel.InvokeExiting();
    }
}
