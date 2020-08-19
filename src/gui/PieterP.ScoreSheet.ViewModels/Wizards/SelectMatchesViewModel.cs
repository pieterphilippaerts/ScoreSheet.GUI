using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.Connector;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.ScoreSheet.ViewModels.Score.Export;
using PieterP.ScoreSheet.ViewModels.Score.Validations;
using PieterP.ScoreSheet.ViewModels.Services;
using PieterP.Shared;
using PieterP.Shared.Services;
using static PieterP.ScoreSheet.Localization.Errors;
using static PieterP.ScoreSheet.Localization.Strings;

namespace PieterP.ScoreSheet.ViewModels.Wizards {
    public class SelectMatchesViewModel : WizardPanelViewModel {
        public SelectMatchesViewModel(WizardViewModel parent, IEnumerable<CompetitiveMatchViewModel> matches, string title, string description, string message, ExportTypes exportType, Action<IEnumerable<CompetitiveMatchViewModel>> onContinue, Func<CompetitiveMatchViewModel, bool>? isSelected = null, Func<CompetitiveMatchViewModel, bool>? showUploadMessage = null, Func<CompetitiveMatchViewModel, bool>? showNonCompetitiveMessage = null) : base(parent) {
            _title = title;
            _description = description;
            _message = message;
            _onContinue = onContinue;
            var validator = new MatchValidator();
            this.Matches = matches.Select(m => {
                var ret = new SelectedMatchInfo() {
                    Result = m.Score.Result.Value,
                    HomeMatchesWon = m.Score.HomeMatchesWon.Value,
                    AwayMatchesWon = m.Score.AwayMatchesWon.Value,
                    HomeTeam = m.HomeTeam,
                    AwayTeam = m.AwayTeam,
                    MatchId = m.MatchId.Value,
                    MatchVm = m,
                    IsSelected = isSelected?.Invoke(m) ?? true,
                    ShowUploadMessage = showUploadMessage?.Invoke(m) ?? false,
                    ShowNonCompetitiveMessage = showNonCompetitiveMessage?.Invoke(m) ?? false
                };
                ret.ValidationErrors = validator.Validate(m);
                ret.HasValidationErrors = ret.ValidationErrors != null;
                return ret;
            }).ToList();
            this.ExportType = exportType;
            this.Export = new RelayCommand<object>(OnExport);
        }
        public void OnExport(object o) {
            var selectedMatchInfos = Matches.Where(m => m.IsSelected);
            if (selectedMatchInfos.Any(mi => mi.HasValidationErrors)) {
                var not = new ShowMessageNotification(Wizard_ValidationWarning, NotificationTypes.Exclamation, NotificationButtons.YesNo);
                NotificationManager.Current.Raise(not);
                if (not.Result != true)
                    return;
            }
            this.SelectedMatches = selectedMatchInfos.Select(mi => mi.MatchVm).ToList();
            _onContinue(this.SelectedMatches);
        }        

        public IList<SelectedMatchInfo> Matches { get; private set; }
        public IEnumerable<CompetitiveMatchViewModel> SelectedMatches { get; private set; }

        public ICommand Export { get; private set; }

        public ExportTypes ExportType { get; private set; }

        public override string Title {
            get {
                return _title;
            }
        }
        public override string Description { 
            get {
                return _description;
            }
        }
        public string Message {
            get {
                return _message;
            }
        }
        private string _title, _description, _message;
        private Action<IEnumerable<CompetitiveMatchViewModel>> _onContinue;
    }

    public enum ExportTypes : int {
        FileExport = 1,
        Upload = 2,
        Email = 3
    }
}