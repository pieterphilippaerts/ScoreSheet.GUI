using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model;
using PieterP.ScoreSheet.Model.Database;
using PieterP.Shared;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class DateValidation : Validation {
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            if (matchVm.Date.Value.Length == 0) {
                if (matchVm.HomeTeam.IsBye.Value || matchVm.AwayTeam.IsBye.Value)
                    return null;
                return Validation_NoDate;
            }
            var date = matchVm.Date.Value.ToDate();
            if (date == null)
                return Validation_InvalidDate;
            if (date.Value.Date != DateTime.Now.Date && date.Value.Date != DateTime.Now.Date.AddDays(-1))
                return Validation_WrongDate;
            var si = DatabaseManager.Current.MatchStartInfo[matchVm.UniqueId];
            if (si == null)
                return null; // can happen if we open a division match (those are not in the database)
            var orgDate = si.Date?.ToDate();
            if (orgDate != null) { 
                if (date.Value.Date != orgDate.Value.Date)
                    return Safe.Format(Validation_UnofficialDate, orgDate.Value.ToString("dd/MM/yyyy"));
            }
            return null;
        }
    }
}
