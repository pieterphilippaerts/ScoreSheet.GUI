using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Model.Database.Entities;
using ModPersonInfo = PieterP.ScoreSheet.Model.Database.Entities.PersonInfo;
using ModPlayerInfo = PieterP.ScoreSheet.Model.Database.Entities.PlayerInfo;

namespace PieterP.ScoreSheet.ViewModels.Score.Export {
    public class MatchExporter {
        public Match ToMatch(CompetitiveMatchViewModel matchVm) {
            var match = new Match();
            match.UniqueId = matchVm.UniqueId;
            match.PlayerCategory = matchVm.PlayerCategory;
            match.MatchSystemId = matchVm.MatchSystem.Id;
            match.IsOfficial = matchVm.IsOfficial.Value;
            match.Venue = matchVm.Address.Value;
            match.Date = matchVm.Date.Value;
            match.StartHour = matchVm.StartHour.Value;
            match.EndHour = matchVm.EndHour.Value;
            match.MatchId = matchVm.MatchId.Value;
            match.Series = matchVm.Series.Value;
            match.Comments = matchVm.Comments.Value;
            match.ChiefReferee = ToPersonInfoModel(matchVm.ChiefReferee);
            match.HomeCaptain = ToPersonInfoModel(matchVm.HomeCaptain);
            match.AwayCaptain = ToPersonInfoModel(matchVm.AwayCaptain);

            match.HomeClub = matchVm.HomeTeam.ClubId.Value;
            match.HomeTeam = matchVm.HomeTeam.Name.Value;
            match.HomeTeamForfeit = matchVm.HomeTeam.Forfeit.Value;
            match.HomePlayers = ToPlayerListModel(matchVm.HomeTeam.Players);
            match.HomeDoubles = ToDoubleListModel(matchVm.HomeTeam.Players);
            match.HomeSubstitutes = ToSubstituteListModel(matchVm.HomeTeam.Players);

            match.AwayClub = matchVm.AwayTeam.ClubId.Value;
            match.AwayTeam = matchVm.AwayTeam.Name.Value;
            match.AwayTeamForfeit = matchVm.AwayTeam.Forfeit.Value;
            match.AwayPlayers = ToPlayerListModel(matchVm.AwayTeam.Players);
            match.AwayDoubles = ToDoubleListModel(matchVm.AwayTeam.Players);
            match.AwaySubstitutes = ToSubstituteListModel(matchVm.AwayTeam.Players);

            match.Results = new List<MatchResult>();
            foreach (var m in matchVm.Matches) {
                match.Results.Add(new MatchResult() {
                    Sets = m.Sets.Select(s => new SetScore() {
                        LeftScore = s.LeftScore.Value,
                        RightScore = s.RightScore.Value
                    }).ToList()
                });
            }

            match.RoomCommissioner = ToPersonInfoModel(matchVm.RoomCommissioner);
            match.Article632 = matchVm.Article632.Value;
            match.Level = matchVm.Level.Value.Id;
            match.Men = matchVm.Men.Value;
            match.Women = matchVm.Women.Value;
            match.Interclub = matchVm.Interclub.Value;
            match.Super = matchVm.Super.Value;
            match.Cup = matchVm.Cup.Value;
            match.Youth = matchVm.Youth.Value;
            match.Veterans = matchVm.Veterans.Value;
            return match;
        }
        private ModPersonInfo ToPersonInfoModel(PersonInfo source) {
            var ret = new ModPersonInfo();
            ret.Name = source.Name.Value;
            ret.ComputerNumber = source.ComputerNumber.Value;
            ret.ClubId = source.ClubId.Value;
            ret.ClubName = source.ClubName.Value;
            return ret;
        }
        private List<ModPlayerInfo> ToPlayerListModel(IList<PlayerInfo> players) {
            var ret = new List<ModPlayerInfo>();
            foreach (var pi in players) {
                var spi = pi as SinglePlayerInfo;
                if (spi != null) {
                    ret.Add(new ModPlayerInfo() {
                        Captain = spi.Captain.Value,
                        ComputerNumber = spi.ComputerNumber.Value,
                        Index = spi.Index.Value,
                        Name = spi.Name.Value,
                        Position = spi.StrengthListPosition.Value,
                        Ranking = spi.Ranking.Value
                    });
                }
            }
            return ret;
        }
        private List<string>? ToDoubleListModel(IList<PlayerInfo> players) {
            var ret = new List<string>();
            foreach (var pi in players) {
                var dpi = pi as DoublePlayerInfo;
                if (dpi != null) {
                    ret.Add(dpi.SelectedOption?.Value.Name ?? "");
                }
            }
            if (ret.Count == 0)
                return null;
            return ret;
        }
        private List<int?>? ToSubstituteListModel(IList<PlayerInfo> players) {
            var ret = new List<int?>();
            foreach (var pi in players) {
                var spi = pi as SubstitutePlayerInfo;
                if (spi != null) {
                    ret.Add(spi.SelectedTransferMatch.Value?.Position);
                }
            }
            if (ret.Count == 0)
                return null;
            return ret;
        }
    }
}
