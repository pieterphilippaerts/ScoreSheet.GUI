using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.Shared.Cells;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class ClubIdValidation : Validation {
        public ClubIdValidation(string title, Func<CompetitiveMatchViewModel, Cell<string>> callback) {
            _title = title;
            _callback = callback;
        }
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            if (_callback(matchVm).Value == "")
                return Safe.Format(Validation_NoClubId, _title);
            if (MatchStartInfo.IsByeIndex(_callback(matchVm).Value))
                return null;
            if (DatabaseManager.Current.Clubs[_callback(matchVm).Value] == null)
                return Safe.Format(Validation_ClubIdNotFound, _title);
            return null;
        }
        private string _title;
        private Func<CompetitiveMatchViewModel, Cell<string>> _callback;
    }
}
