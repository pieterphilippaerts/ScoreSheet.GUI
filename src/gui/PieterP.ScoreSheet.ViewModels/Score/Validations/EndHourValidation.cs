using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Model;
using PieterP.Shared;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class EndHourValidation : Validation {
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            if (!matchVm.MustBePlayed.Value)
                return null;
            if (matchVm.EndHour.Value.Length == 0)
                return Validation_NoEndHour;
            var time = matchVm.EndHour.Value.ToTime();
            if (time == null)
                return Validation_InvalidEndHour;
            return null;
        }
    }
}
