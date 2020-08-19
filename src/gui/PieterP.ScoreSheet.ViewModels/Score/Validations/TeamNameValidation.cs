using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.Shared.Cells;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class TeamNameValidation : Validation {
        public TeamNameValidation(string title, Func<CompetitiveMatchViewModel, Cell<string>> callback) {
            _title = title;
            _callback = callback;
        }
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            string teamName = _callback(matchVm).Value;
            var si = DatabaseManager.Current.MatchStartInfo[matchVm.UniqueId];
            if (si != null && si.Series != null) {
                if (teamName != si.HomeTeam && teamName != si.AwayTeam)
                    return Safe.Format(Validation_UnofficialName, _title);
            } else {
                if (teamName == "")
                    return Safe.Format(Validation_NoName, _title);
            }
            return null;
        }
        private string _title;
        private Func<CompetitiveMatchViewModel, Cell<string>> _callback;
    }
}
