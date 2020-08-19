using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Model;
using PieterP.Shared;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class StartHourValidation : Validation {
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            if (!matchVm.MustBePlayed.Value)
                return null;
            if (matchVm.StartHour.Value.Length == 0)
                return Validation_NoStartHour;
            var time = matchVm.StartHour.Value.ToTime();
            if (time == null)
                return Validation_InvalidStartHour;
            return null;
        }
    }
}
