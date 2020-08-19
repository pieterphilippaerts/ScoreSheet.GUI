using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model;
using PieterP.ScoreSheet.Model.Database;
using PieterP.Shared;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class PlayerValidation : Validation {
        public PlayerValidation(string title, Func<CompetitiveMatchViewModel, SinglePlayerInfo?> callback) {
            _title = title;
            _callback = callback;
        }
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            var spi = _callback(matchVm);
            if (spi == null)
                return null;
            var values = new string[] { spi.Name.Value, spi.ComputerNumber.Value, spi.StrengthListPosition.Value, spi.Index.Value, spi.Ranking.Value };
            if (values.All(c => c != "") && spi.Optional)
                return null;

            if (spi.ParentTeam.IsBye.Value) {
                if (values[1] != "?")
                    return Safe.Format(Validation_PlayerByeIdWrong, _title);
                if (values.Count(c => c != "") != 1)
                    return Safe.Format(Validation_PlayerByeFieldsWrong, _title);
                return null;
            } else if (spi.ParentTeam.Forfeit.Value && values[1] == "?") {
                if (values.Count(c => c != "") != 1)
                    return Safe.Format(Validation_PlayerFfFieldsWrong, _title);
                return null;
            }
            if (values[0] == "")
                return Safe.Format(Validation_NoName, _title);
            else if (values[1] == "" || !values[1].IsNumber())
                return Safe.Format(Validation_InvalidPlayerId, _title);
            else if (values[2] == "" || !values[2].IsNumber())
                return Safe.Format(Validation_InvalidPlayerPosition, _title);
            else if (values[3] == "" || !values[3].IsNumber())
                return Safe.Format(Validation_InvalidIndex, _title);
            else if (values[4] == "")
                return Safe.Format(Validation_InvalidRanking, _title);

            var members = DatabaseManager.Current.Members[spi.ParentTeam.ClubId.Value, matchVm.PlayerCategory];
            if (members != null && int.TryParse(values[1], out var cn)) {
                var mem = members.Entries.Where(e => e.ComputerNumber == cn).FirstOrDefault();
                if (mem == null) {
                    return Safe.Format(Validation_PlayerNotFound, _title, cn.ToString());
                } else {
                    if (values[0].ToLower() != (mem.Lastname + " " + mem.Firstname).ToLower())
                        return Safe.Format(Validation_WrongName, _title);
                    if (values[2] != mem.Position?.ToString())
                        return Safe.Format(Validation_WrongPlayerPosition, _title);
                    if (values[3] != mem.RankIndex?.ToString())
                        return Safe.Format(Validation_WrongIndex, _title);
                    if (values[4] != mem.Ranking)
                        return Safe.Format(Validation_WrongRanking, _title);
                }
            }

            return null;
        }
        private string _title;
        private Func<CompetitiveMatchViewModel, SinglePlayerInfo?> _callback;
    }
}
