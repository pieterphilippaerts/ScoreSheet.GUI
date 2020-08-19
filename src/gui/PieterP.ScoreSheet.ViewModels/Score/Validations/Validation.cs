using System;
using System.Collections.Generic;
using System.Text;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public abstract class Validation {
        // returns the error, or null if the validation succeeded
        public abstract string? Run(CompetitiveMatchViewModel matchVm);
    }
}
