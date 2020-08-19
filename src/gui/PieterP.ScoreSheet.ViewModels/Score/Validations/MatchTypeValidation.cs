using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class MatchTypeValidation : Validation {
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            if (!matchVm.IsOfficial.Value)
                return null;
            var checks = new bool[] { matchVm.Interclub.Value, matchVm.Super.Value, matchVm.Youth.Value, matchVm.Cup.Value, matchVm.Veterans.Value };
            if (!checks.Any(c => c))
                return Validation_NoMatchType;
            if (checks.Count(c => c) > 1)
                return Validation_MultipleMatchTypes;
            return null;
        }
    }
}
