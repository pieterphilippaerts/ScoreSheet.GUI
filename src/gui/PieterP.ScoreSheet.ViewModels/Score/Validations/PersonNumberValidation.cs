using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Localization;
using PieterP.Shared;
using PieterP.Shared.Cells;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class PersonNumberValidation : Validation {
        public PersonNumberValidation(string title, Func<CompetitiveMatchViewModel, Cell<string>> callback) {
            _title = title;
            _callback = callback;
        }
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            if (!matchVm.MustBePlayed.Value)
                return null;
            if (!int.TryParse(_callback(matchVm).Value, out int cn)) {
                return Safe.Format(Validation_InvalidMemberId, _title);
            }
            return null;
        }
        private string _title;
        private Func<CompetitiveMatchViewModel, Cell<string>> _callback;
    }
}
