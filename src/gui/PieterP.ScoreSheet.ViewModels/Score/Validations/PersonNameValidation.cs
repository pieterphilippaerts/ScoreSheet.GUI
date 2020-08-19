using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Localization;
using PieterP.Shared.Cells;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class PersonNameValidation : Validation {
        public PersonNameValidation(string title, Func<CompetitiveMatchViewModel, Cell<string>> callback) {
            _title = title;
            _callback = callback;
        }
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            if (!matchVm.MustBePlayed.Value)
                return null;
            if (_callback(matchVm).Value == "")
                return Safe.Format(Validation_NoName, _title);
            return null;
        }
        private string _title;
        private Func<CompetitiveMatchViewModel, Cell<string>> _callback;
    }
}
