using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PieterP.ScoreSheet.Localization.Errors;

namespace PieterP.ScoreSheet.ViewModels.Score.Validations {
    public class ChiefRefereeValidValidation : Validation {
        public override string? Run(CompetitiveMatchViewModel matchVm) {
            if (matchVm.ChiefReferee.ComputerNumber.Value == "" || matchVm.ChiefReferee.Name.Value == "")
                return null; // if the name or number is empty, it will be caught by another validation

            if (!matchVm.IsOfficial.Value)
                return null; // we don't validate if the match is not official

            var cns = matchVm.ChiefReferee.ComputerNumber.Value;
            if (!int.TryParse(cns, out var cn)) {
                return null;
            }

            // check whether the chief referee is not a member of the away club
            if (matchVm.HomeTeam.ClubId != matchVm.AwayTeam.ClubId) { // Only if home and away clubs are not the same...
                var awayClubMembers = DatabaseManager.Current.Members[matchVm.AwayTeam.ClubId.Value, DatabaseManager.Current.PlayerCategories.Default];
                if (awayClubMembers != null && awayClubMembers.Entries.Any(m => m.ComputerNumber == cn))
                    return Validation_ChiefRefereeNotAwayMember;
            }

            //check whether the chief referee is a member of the home club
            var clubMembers = DatabaseManager.Current.Members[matchVm.HomeTeam.ClubId.Value, DatabaseManager.Current.PlayerCategories.Default];
            if (clubMembers != null && clubMembers.Entries.Any(m => m.ComputerNumber == cn)) {
                // the chief referee is a member of the home team
                if (int.TryParse(matchVm.HomeCaptain.ComputerNumber.Value, out var homeCapt) && homeCapt == cn) // if (s) he is also the captain,
                    return null; // that's OK

                // if the chief referee is playing in the same match (but not the home captain), that's not OK
                if (matchVm.HomeTeam.Players.OfType<SinglePlayerInfo>().Any(pi => pi.ComputerNumber.Value == cns) ||
                        matchVm.AwayTeam.Players.OfType<SinglePlayerInfo>().Any(pi => pi.ComputerNumber.Value == cns) /* this can happen if home and away club are the same, and the chief referee is playing in the away team */)
                    return Validation_ChiefRefereeNotHomeCaptain;

                // check other matches that are played today
                var date = matchVm.Date.Value.ToDate();
                if (date != null) {
                    var matches = DatabaseManager.Current.MatchStartInfo.GetMatchesAtDate(date.Value, false, false);
                    foreach(var matchInfo in matches) {
                        if (matchInfo.MatchId == matchVm.MatchId.Value)
                            continue; // this is the current match, and we've already checked this

                        var otherMatch = DatabaseManager.Current.OfficialMatches[matchInfo.MatchId];
                        if (otherMatch != null) {
                            var otherMatchVm = new CompetitiveMatchViewModel(otherMatch, null, null);
                            if (otherMatchVm.HomeTeam.Players.OfType<SinglePlayerInfo>().Any(pi => pi.ComputerNumber.Value == cns) ||
                                    otherMatchVm.AwayTeam.Players.OfType<SinglePlayerInfo>().Any(pi => pi.ComputerNumber.Value == cns) /* this can happen if home and away club are the same, and the chief referee is playing in the away team */)
                                return Safe.Format(Validation_ChiefRefereeNotAvailable, $"{otherMatch.HomeTeam} - {otherMatch.AwayTeam}");
                        }
                    }
                }
            } // else: chief referee is not from home or away club, so probably an official referee
            return null;
        }
    }
}