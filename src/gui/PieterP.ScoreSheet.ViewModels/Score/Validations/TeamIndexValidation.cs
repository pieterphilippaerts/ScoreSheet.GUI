using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Localization;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class TeamIndexValidation : Validation {
        public TeamIndexValidation(string title, Func<CompetitiveMatchViewModel, TeamInfo> callback) {
            _title = title;
            _callback = callback;
        }
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            if (matchVm.Level.Value.Id == Model.Database.Enums.Level.Super)
                return null; // super kan doen wat ze willen kwa opstelling
            var team = _callback(matchVm);
            var indices = new List<int>();
            foreach (var p in team.Players) {
                var spi = p as SinglePlayerInfo;
                if (spi != null) {
                    if (int.TryParse(spi.Index.Value, out var i)) {
                        indices.Add(i);
                    }
                }
            }
            int prevIndex = -1;
            foreach (var index in indices) {
                if (index < prevIndex)
                    return Safe.Format(Validation_InvalidPlayerOrder, _title);
                prevIndex = index;
            }
            return null;
        }
        private string _title;
        private Func<CompetitiveMatchViewModel, TeamInfo> _callback;
    }
}
