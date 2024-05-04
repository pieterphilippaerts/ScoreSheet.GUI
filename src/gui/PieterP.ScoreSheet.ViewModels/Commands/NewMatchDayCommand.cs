using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.ViewModels.Commands.Bases;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.ScoreSheet.ViewModels.Wizards;

namespace PieterP.ScoreSheet.ViewModels.Commands
{
    internal class NewMatchDayCommand : OpenWizardCommand<NewMatchdayViewModel>
    {
        public NewMatchDayCommand(MainWindowViewModel mainWindowViewModel) : base(mainWindowViewModel)
        {
            ProcessResultAction = (_) =>
            {
                foreach (var m in _panelViewModel.Matches)
                {
                    var fullMatch = DatabaseManager.Current.OfficialMatches[m.MatchId];
                    _mainWindowViewModel.AddMatch(fullMatch != null ? new CompetitiveMatchViewModel(fullMatch, m) : new CompetitiveMatchViewModel(m));
                }
            };
        }

        /// <inheritdoc/>
        protected override NewMatchdayViewModel CreatePanelViewModel() => new NewMatchdayViewModel(_wizardViewModel);
    }
}
