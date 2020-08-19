using System;
using System.Collections.Generic;
using System.Text;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class ScoreValidation : Validation {
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            if (matchVm.Score.Result.Value == Winner.Error) {
                return Validation_ScoreError;
            } else if (matchVm.Score.Result.Value == Winner.Incomplete) {
                if (matchVm.Cup.Value) {
                    var min = matchVm.Matches.Count / 2 + 1;
                    if (matchVm.Score.HomeMatchesWon.Value == min || matchVm.Score.AwayMatchesWon.Value == min)
                        return null; // voor bekermatchen moeten niet alle wedstrijden ingevuld zijn
                }
                return Validation_ScoreIncomplete;
            }
            return null;
        }
    }
}
