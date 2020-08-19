using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class SeriesValidation : Validation {
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            var si = DatabaseManager.Current.MatchStartInfo[matchVm.UniqueId];
            if (si != null && si.Series != null) {
                if (matchVm.Series.Value != si.Series)
                    return Safe.Format(Validation_UnofficialSeries, si.Series);
            } else {
                if (matchVm.Series.Value.Length == 0)
                    return Validation_NoSeries;
            }
            return null;
        }
    }
}
