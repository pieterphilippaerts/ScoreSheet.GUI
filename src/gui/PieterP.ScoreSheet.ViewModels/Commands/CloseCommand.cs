using PieterP.ScoreSheet.ViewModels.Commands.Bases;
using PieterP.ScoreSheet.ViewModels.Score;

namespace PieterP.ScoreSheet.ViewModels.Commands
{
    /// <summary>
    /// This command handles the request to close the active match.
    /// </summary>
    class CloseCommand : BaseMainWindowCommand
    {
        public CloseCommand(MainWindowViewModel mainWindowViewModel) : base(mainWindowViewModel)
        {
            mainWindowViewModel.CurrentScreen.ValueChanged += RaiseCanExecuteChanged;
        }

        /// <inheritdoc/>
        public override bool CanExecute(object _) => _mainWindowViewModel.CurrentScreen.Value == _mainWindowViewModel.MatchContainer;

        /// <inheritdoc/>
        public override void Execute(object _) => _mainWindowViewModel.CloseMatches(new CompetitiveMatchViewModel?[] { _mainWindowViewModel.MatchContainer.ActiveMatch?.Value });
    }
}
