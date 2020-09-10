using PieterP.ScoreSheet.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class CaptainNotWOValidation : Validation {
        public CaptainNotWOValidation(string title, Func<CompetitiveMatchViewModel, TeamInfo> callback) {
            _title = title;
            _callback = callback;
        }
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            if (matchVm.HomeTeam.Forfeit.Value || matchVm.AwayTeam.Forfeit.Value || matchVm.HomeTeam.IsBye.Value || matchVm.AwayTeam.IsBye.Value)
                return null; // no match was played

            var team = _callback(matchVm);
            foreach (var p in team.Players) {
                var spi = p as SinglePlayerInfo;
                if (spi != null) {
                    if (spi.Captain.Value && matchVm.Score.IsPlayerWO(spi)) {
                        return Safe.Format(Validation_CaptainNotWO, _title);
                    }
                }
            }
            return null;
        }
        private string _title;
        private Func<CompetitiveMatchViewModel, TeamInfo> _callback;
    }
}
