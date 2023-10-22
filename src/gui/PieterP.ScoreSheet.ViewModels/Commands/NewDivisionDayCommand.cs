using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Updater;
using PieterP.ScoreSheet.ViewModels.Commands.Bases;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.ScoreSheet.ViewModels.Wizards;

namespace PieterP.ScoreSheet.ViewModels.Commands
{
    internal class NewDivisionDayCommand : OpenWizardCommand<NewDivisionDayViewModel>
    {
        public NewDivisionDayCommand(MainWindowViewModel mainWindowViewModel) : base(mainWindowViewModel)
        {
            ProcessResultAction = (_) =>
            {
                var updater = new TabTUpdater();

                foreach (var m in _panelViewModel.SelectedMatches)
                {
                    var fullMatch = DatabaseManager.Current.OfficialMatches[m.MatchId];
                    _mainWindowViewModel.AddMatch(fullMatch != null ? new CompetitiveMatchViewModel(fullMatch, m) : new CompetitiveMatchViewModel(m));
                    updater.RefreshMemberList(m.HomeClub, m.PlayerCategory.Value);
                    updater.RefreshMemberList(m.AwayClub, m.PlayerCategory.Value);
                }
            };
        }

        /// <inheritdoc/>
        protected override NewDivisionDayViewModel CreatePanelViewModel() => new NewDivisionDayViewModel(_wizardViewModel);
    }
}
