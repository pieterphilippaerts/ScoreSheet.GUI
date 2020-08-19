using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Templates {
    public interface IOrientationInfo {
        bool IsLandscape { get; }
    }
    public abstract class OfficialScoreTemplate : IOrientationInfo {
        public OfficialScoreTemplate(CompetitiveMatchViewModel match) {
            this.Address = match.Address;
            this.Super = match.Super;
            this.InterclubYouth = match.Youth;
            this.CupMen = Cell.Derived(match.Cup, match.Men, (i, m) => i && m);
            this.CupWomen = Cell.Derived(match.Cup, match.Women, (i, w) => i && w);
            this.InterclubMen = Cell.Derived(match.Interclub, match.Veterans, match.Men, (i, v, m) => (i || v) && m);
            this.InterclubWomen = Cell.Derived(match.Interclub, match.Veterans, match.Women, (i, v, w) => (i || v) && w);
            this.Date = match.Date;
            this.Level = match.Level;
            this.StartHour = match.StartHour;
            this.MatchId = match.MatchId;
            this.Series = match.Series;
            this.EndHour = match.EndHour;
            this.HomeTeam = new OfficialTeamInfo(match.HomeTeam);
            this.AwayTeam = new OfficialTeamInfo(match.AwayTeam);
            this.ChiefReferee = match.ChiefReferee;
            this.HomeCaptain = match.HomeCaptain;
            this.AwayCaptain = match.AwayCaptain;
            this.RoomCommissioner = match.RoomCommissioner;
            this.Comments = Cell.Derived(match.Comments, c => (c == null || c.Length == 0) ? "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" : c);
            this.Article632 = match.Article632;
            this.Matches = match.Matches;
            this.ShowSponsors = DatabaseManager.Current.Settings.PrintSponsors;
        }
        public Cell<string> Address { get; private set; }
        public Cell<bool> InterclubMen { get; private set; }
        public Cell<bool> InterclubWomen { get; private set; }
        public Cell<bool> InterclubYouth { get; private set; }
        public Cell<bool> CupMen { get; private set; }
        public Cell<bool> CupWomen { get; private set; }
        public Cell<bool> Super { get; private set; }
        public Cell<string> Date { get; private set; }
        public Cell<LevelInfo> Level { get; private set; }
        public Cell<string> StartHour { get; private set; }
        public Cell<string> MatchId { get; private set; }
        public Cell<string> Series { get; private set; }
        public Cell<string> EndHour { get; private set; }
        public OfficialTeamInfo HomeTeam { get; private set; }
        public OfficialTeamInfo AwayTeam { get; private set; }
        public PersonInfo ChiefReferee { get; private set; }
        public PersonInfo HomeCaptain { get; private set; }
        public PersonInfo AwayCaptain { get; private set; }
        public PersonInfo RoomCommissioner { get; private set; }
        public Cell<string> Comments { get; private set; }
        public Cell<bool> Article632 { get; private set; }
        public IList<MatchInfo> Matches { get; private set; }
        public Cell<bool> ShowSponsors { get; private set; }
        public abstract bool IsLandscape { get; }
    }
    public class VttlScoreTemplate : OfficialScoreTemplate {
        public VttlScoreTemplate(CompetitiveMatchViewModel match) : base(match) {}
        public override bool IsLandscape => true;
    }
    public class AfttScoreTemplate : OfficialScoreTemplate {
        public AfttScoreTemplate(CompetitiveMatchViewModel match) : base(match) {
            var lastMatch = match.Matches.Last();
            this.FinalScore = $"{ lastMatch.HomeMatches.Value }-{ lastMatch.AwayMatches.Value }";
            this.TotalSets = $"{ match.Matches.Sum(m => PS(m.HomeSets.Value)) }-{ match.Matches.Sum(m => PS(m.AwaySets.Value)) }";

            int PS(string sets) {
                if (sets == null || sets.Length == 0)
                    return 0;
                if (int.TryParse(sets, out var s)) {
                    return s;
                }
                return 0;
            }
        }

        public string FinalScore { get; private set; }
        public string TotalSets { get; private set; }
        public override bool IsLandscape => false;
    }
    public class OfficialTeamInfo {
        public OfficialTeamInfo(PieterP.ScoreSheet.ViewModels.Score.TeamInfo team) {
            this.Name = team.Name;
            this.ClubId = team.ClubId;
            this.Players = team.Players.Select(p => Convert(p)).Where(c => c != null).ToList() ?? new List<OfficialPlayerInfo?>();

            OfficialPlayerInfo? Convert(PieterP.ScoreSheet.ViewModels.Score.PlayerInfo player) {
                if (player is PieterP.ScoreSheet.ViewModels.Score.SinglePlayerInfo) {
                    return new OfficialSinglePlayerInfo((player as PieterP.ScoreSheet.ViewModels.Score.SinglePlayerInfo)!);
                } else if (player is PieterP.ScoreSheet.ViewModels.Score.DoublePlayerInfo) {
                    return new OfficialDoublePlayerInfo((player as PieterP.ScoreSheet.ViewModels.Score.DoublePlayerInfo)!);
                }
                return null;
            }
        }
        public Cell<string> Name { get; private set; }
        public Cell<string> ClubId { get; private set; }
        public IList<OfficialPlayerInfo?> Players { get; private set; }
    }
    public abstract class OfficialPlayerInfo {
        public OfficialPlayerInfo(PieterP.ScoreSheet.ViewModels.Score.PlayerInfo player) {
            this.IndividualWins = player.IndividualWins;
        }
        public Cell<string> IndividualWins { get; private set; }
    }
    public class OfficialSinglePlayerInfo : OfficialPlayerInfo {
        public OfficialSinglePlayerInfo(PieterP.ScoreSheet.ViewModels.Score.SinglePlayerInfo player) : base(player) {
            this.Position = player.Position;
            this.Name = player.Name;
            this.ComputerNumber = player.ComputerNumber;
            this.StrengthListPosition = player.StrengthListPosition;
            this.Index = player.Index;
            this.Ranking = player.Ranking;
            this.Captain = player.Captain;
        }
        
        public string Position { get; private set; }
        public Cell<string> Name { get; private set; }
        public Cell<string> ComputerNumber { get; private set; }
        public Cell<string> StrengthListPosition { get; private set; }
        public Cell<string> Index { get; private set; }
        public Cell<string> Ranking { get; private set; }
        public Cell<bool> Captain { get; private set; }
    }
    public class OfficialDoublePlayerInfo : OfficialPlayerInfo {
        public OfficialDoublePlayerInfo(PieterP.ScoreSheet.ViewModels.Score.DoublePlayerInfo player) : base(player) {
            this.SelectedOption = player.SelectedOption;
        }
        public Cell<DoublePlayerOption> SelectedOption { get; private set; }
    }

}
