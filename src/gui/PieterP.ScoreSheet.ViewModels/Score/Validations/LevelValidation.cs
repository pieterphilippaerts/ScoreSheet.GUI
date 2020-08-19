using System;
using System.Collections.Generic;
using System.Text;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class LevelValidation : Validation {
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            if (matchVm.Level.Value.Id == Model.Database.Enums.Level.Super && !matchVm.Super.Value)
                return Validation_SuperCheck;
            return null;
        }
    }
}
