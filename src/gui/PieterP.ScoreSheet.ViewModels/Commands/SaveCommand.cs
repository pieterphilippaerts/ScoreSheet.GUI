using PieterP.ScoreSheet.ViewModels.Commands.Bases;

namespace PieterP.ScoreSheet.ViewModels.Commands
{
    /// <summary>
    /// This command handles the save process of an unofficial match.
    /// </summary>
    class SaveCommand : BaseMainWindowCommand
    {
        public SaveCommand(MainWindowViewModel mainWindowViewModel) : base(mainWindowViewModel)
        {
            mainWindowViewModel.CurrentScreen.ValueChanged += RaiseCanExecuteChanged;
            mainWindowViewModel.MatchContainer.ActiveMatch.ValueChanged += RaiseCanExecuteChanged;
        }

        /// <inheritdoc />
        public override bool CanExecute(object parameter)
        {
            return _mainWindowViewModel.CurrentScreen.Value == _mainWindowViewModel.MatchContainer
                && _mainWindowViewModel.MatchContainer.ActiveMatch.Value != null
                && _mainWindowViewModel.MatchContainer.ActiveMatch.Value.IsOfficial.Value == false
                    && _mainWindowViewModel.MatchContainer.ActiveMatch.Value.Filename.Value != null;
        }

        /// <inheritdoc />
        public override void Execute(object parameter)
        {
            var match = _mainWindowViewModel.MatchContainer.ActiveMatch.Value;
            if (match == null || match.Filename.Value == null)
                return;
            _mainWindowViewModel.SaveMatch(match);
        }
    }
}
