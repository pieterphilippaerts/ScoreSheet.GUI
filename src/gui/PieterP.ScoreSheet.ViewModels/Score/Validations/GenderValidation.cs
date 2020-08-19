using System;
using System.Collections.Generic;
using System.Text;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class GenderValidation : Validation {
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            if (!matchVm.IsOfficial.Value)
                return null;
            if (!matchVm.Men.Value && !matchVm.Women.Value)
                return Validation_NoGender;
            if (matchVm.Men.Value && matchVm.Women.Value)
                return Validation_BothGenders;
            return null;
        }
    }
}
