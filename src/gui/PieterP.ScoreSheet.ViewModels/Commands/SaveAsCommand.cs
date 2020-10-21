using PieterP.ScoreSheet.ViewModels.Commands.Bases;

namespace PieterP.ScoreSheet.ViewModels.Commands
{
    /// <summary>
    /// This command handles the save as process of an unofficial match.
    /// </summary>
    class SaveAsCommand : BaseMainWindowCommand
    {
        public SaveAsCommand(MainWindowViewModel mainWindowViewModel) : base(mainWindowViewModel)
        {
            mainWindowViewModel.CurrentScreen.ValueChanged += RaiseCanExecuteChanged;
            mainWindowViewModel.MatchContainer.ActiveMatch.ValueChanged += RaiseCanExecuteChanged;
        }

        /// <inheritdoc />
        public override bool CanExecute(object parameter)
        {
            return _mainWindowViewModel.CurrentScreen.Value == _mainWindowViewModel.MatchContainer
                && _mainWindowViewModel.MatchContainer.ActiveMatch.Value != null
                && _mainWindowViewModel.MatchContainer.ActiveMatch.Value.IsOfficial.Value == false;
        }

        /// <inheritdoc />
        public override void Execute(object parameter)
        {
            var match = _mainWindowViewModel.MatchContainer.ActiveMatch.Value;
            if (match == null)
                return;
            _mainWindowViewModel.SaveMatch(match, true);
        }
    }
}
