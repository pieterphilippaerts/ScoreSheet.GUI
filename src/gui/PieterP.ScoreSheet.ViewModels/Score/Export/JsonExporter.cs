using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.ViewModels.Services.Json;

namespace PieterP.ScoreSheet.ViewModels.Score.Export {
    public class JsonExporter {
        public IEnumerable<string> ToJson(MatchInfo match) { 
            return match.Sets.SelectMany(s => new string[] { s.LeftScore.Value, s.RightScore.Value });
        }
        public JsonMatchResult ToJson(CompetitiveMatchViewModel match) {
            var ret = new JsonMatchResult();
            ret.AwayClubId = match.AwayTeam.ClubId.Value;
            ret.AwayTeam = match.AwayTeam.Name.Value;
            ret.HomeClubId = match.HomeTeam.ClubId.Value;
            ret.HomeTeam = match.HomeTeam.Name.Value;
            ret.Interclub = match.Interclub.Value;
            ret.IsAwayFF = match.AwayTeam.Forfeit.Value;
            ret.IsHomeFF = match.HomeTeam.Forfeit.Value;
            ret.Level = (int)match.Level.Value.Id;
            ret.LostCount=match.Score.AwayMatchesWon.Value;
            ret.MatchCount = match.Matches.Count;
            ret.MatchNr = match.MatchId.Value;
            ret.Men = match.Men.Value; 
            ret.Series = match.Series.Value;
            ret.StartHour = match.StartHour.Value;
            ret.Veterans = match.Veterans.Value;
            ret.WonCount = match.Score.HomeMatchesWon.Value;
            ret.Women = match.Women.Value;
            ret.Youth=match.Youth.Value;
            ret.HomePlayers = GetPlayers(match.Score, match.HomeTeam.Players);
            ret.AwayPlayers = GetPlayers(match.Score, match.AwayTeam.Players);
            ret.HomeDouble = GetDouble(match.HomeTeam.Players);
            ret.AwayDouble = GetDouble(match.AwayTeam.Players);
            var jml = new List<JsonMatch>();
            foreach (var m in match.Matches) {
                var jm = new JsonMatch();
                jm.MatchIndex = m.Position;
                jm.HomePlayers = GetPlayersForMatch(m.HomePlayers.Value);
                jm.AwayPlayers = GetPlayersForMatch(m.AwayPlayers.Value);
                jm.Points = m.Sets.SelectMany(s => new string[] { s.LeftScore.Value, s.RightScore.Value });
                jm.WonSets = m.HomeSets.Value;
                jm.LostSets = m.AwaySets.Value;
                jm.WonMatches = m.HomeMatches.Value;
                jm.LostMatches = m.HomeMatches.Value;
                jml.Add(jm);
            }
            ret.Matches = jml;
            return ret;

            IEnumerable<int> GetPlayersForMatch(IEnumerable<PlayerInfo> players) {
                var ret = new List<int>();
                foreach (var p in players) {
                    var dpi = p as DoublePlayerInfo;
                    if (dpi == null) {
                        ret.Add(p.ParentTeam.Players.IndexOf(p) + 1);
                    } else {
                        ret.Add(0);
                    }
                }
                return ret;
            }
            string? GetDouble(IEnumerable<PlayerInfo> players) {
                foreach (var p in players) {
                    var dpi = p as DoublePlayerInfo;
                    if (dpi != null) {
                        return dpi.SelectedOption.Value?.Name;
                    }
                }
                return null;
            }
            IEnumerable<JsonPlayer> GetPlayers(ScoreCalculator score, IEnumerable<PlayerInfo> players) {
                var ret = new List<JsonPlayer>();
                foreach (var p in players) {
                    var spi = p as SinglePlayerInfo;
                    if (spi != null) {
                        var jp = new JsonPlayer();
                        jp.Captain = spi.Captain.Value;
                        jp.ComputerNumber = spi.ComputerNumber.Value;
                        jp.Index = spi.Index.Value;
                        jp.IsWO = score.IsPlayerWO(spi);
                        jp.Name = spi.Name.Value;
                        jp.Position = spi.StrengthListPosition.Value;
                        jp.Ranking = spi.Ranking.Value;
                        ret.Add(jp);
                    }
                }
                return ret;
            }
        }
    }
}