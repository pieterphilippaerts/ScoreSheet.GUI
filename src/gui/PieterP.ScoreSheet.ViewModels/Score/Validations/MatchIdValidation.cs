using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Localization;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class MatchIdValidation : Validation {
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            if (matchVm.MatchId.Value.Length == 0)
                return Validation_NoMatchId;
            if (matchVm.MatchId.Value != matchVm.UniqueId)
                return Safe.Format(Validation_UnofficialMatchId, matchVm.UniqueId);
            return null;
        }
    }
}
