using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Templates {
    public class UnofficialScoreTemplate : IOrientationInfo {
        public UnofficialScoreTemplate(CompetitiveMatchViewModel match) {
            this.Address = match.Address;
            this.Date = match.Date;
            this.Level = match.Level;
            this.Series = match.Series;
            this.StartHour = match.StartHour;
            this.MatchId = match.MatchId;
            this.EndHour = match.EndHour;
            this.HomeTeam = new UnofficialTeamInfo(match.HomeTeam, match.Matches);
            this.AwayTeam = new UnofficialTeamInfo(match.AwayTeam, match.Matches);
            this.ChiefReferee = match.ChiefReferee;
            this.HomeCaptain = match.HomeCaptain;
            this.AwayCaptain = match.AwayCaptain;
            this.Comments = Cell.Derived(match.Comments, c => (c == null || c.Length == 0) ? "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" : c);
            this.Matches = match.Matches;
        }
        public Cell<string> Address { get; private set; }
        public Cell<string> Date { get; private set; }
        public Cell<LevelInfo> Level { get; private set; }
        public Cell<string> Series { get; private set; }
        public Cell<string> StartHour { get; private set; }
        public Cell<string> MatchId { get; private set; }
        public Cell<string> EndHour { get; private set; }
        public UnofficialTeamInfo HomeTeam { get; private set; }
        public UnofficialTeamInfo AwayTeam { get; private set; }
        public PersonInfo ChiefReferee { get; private set; }
        public PersonInfo HomeCaptain { get; private set; }
        public PersonInfo AwayCaptain { get; private set; }
        public Cell<string> Comments { get; private set; }
        public IList<MatchInfo> Matches { get; private set; }
        public bool IsLandscape => true;
    }
    public class UnofficialTeamInfo {
        public UnofficialTeamInfo(PieterP.ScoreSheet.ViewModels.Score.TeamInfo team, IList<MatchInfo> matches) {
            this.Name = team.Name;
            this.ClubId = team.ClubId;
            this.Players = team.Players.Select(p => Convert(p)).Where(c => c != null).ToList() ?? new List<UnofficialSinglePlayerInfo?>();
            this.DoublesWon = 0;
            foreach (var p in team.Players) {
                var dpi = p as DoublePlayerInfo;
                if (dpi != null) {
                    if (dpi.IndividualWins.Value != null && dpi.IndividualWins.Value != "" && int.TryParse(dpi.IndividualWins.Value, out var wcnt)) {
                        this.DoublesWon += wcnt;
                    }
                }
            }
            foreach (var m in matches) {
                if (m.HomePlayers.Value.Count > 1) {
                    if (m.HomePlayers.Value.Any(c => team.Players.Any(p => p == c))) {
                        if (int.TryParse(m.HomeSets.Value, out var homeSets) && int.TryParse(m.AwaySets.Value, out var awaySets) && homeSets > awaySets) {
                            this.DoublesWon++;
                        }
                    }
                }
                if (m.AwayPlayers.Value.Count > 1) {
                    if (m.AwayPlayers.Value.Any(c => team.Players.Any(p => p == c))) {
                        if (int.TryParse(m.HomeSets.Value, out var homeSets) && int.TryParse(m.AwaySets.Value, out var awaySets) && homeSets < awaySets) {
                            this.DoublesWon++;
                        }
                    }
                }
            }

            UnofficialSinglePlayerInfo? Convert(PieterP.ScoreSheet.ViewModels.Score.PlayerInfo player) {
                if (player is PieterP.ScoreSheet.ViewModels.Score.SinglePlayerInfo) {
                    return new UnofficialSinglePlayerInfo((player as PieterP.ScoreSheet.ViewModels.Score.SinglePlayerInfo)!);
                }
                return null;
            }
        }
        public Cell<string> Name { get; private set; }
        public Cell<string> ClubId { get; private set; }
        public IList<UnofficialSinglePlayerInfo?> Players { get; private set; }
        public int DoublesWon { get; set; }
    }
    public class UnofficialSinglePlayerInfo {
        public UnofficialSinglePlayerInfo(PieterP.ScoreSheet.ViewModels.Score.SinglePlayerInfo player) {
            this.IndividualWins = player.IndividualWins;
            this.Position = player.Position;
            this.Name = player.Name;
            this.ComputerNumber = player.ComputerNumber;
            this.Ranking = player.Ranking;
        }
        public Cell<string> IndividualWins { get; private set; }
        public string Position { get; private set; }
        public Cell<string> Name { get; private set; }
        public Cell<string> ComputerNumber { get; private set; }
        public Cell<string> Ranking { get; private set; }
    }
}
