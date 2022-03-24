using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.Shared;
using PieterP.Shared.Cells;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class TeamWOValidation : Validation {
        public TeamWOValidation(string title, Func<CompetitiveMatchViewModel, TeamInfo> callback) {
            _title = title;
            _callback = callback;
        }
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            var team = _callback(matchVm);
            if (team.Forfeit.Value)
                return null; // team FF, do not check the WOs

            bool found = false;
            foreach (var p in team.Players) {
                var spi = p as SinglePlayerInfo;
                if (spi != null) {
                    if (matchVm.Score.IsPlayerWO(spi)) {
                        if (found) {
                            return Safe.Format(Validation_TooManyWO, _title);
                        } else {
                            found = true;
                        }
                    }
                }
            }
            return null;
        }
        private string _title;
        private Func<CompetitiveMatchViewModel, TeamInfo> _callback;
    }
}
