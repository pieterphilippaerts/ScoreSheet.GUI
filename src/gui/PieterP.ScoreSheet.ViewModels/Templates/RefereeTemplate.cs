using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.ViewModels.Score;
using HandicapTable = PieterP.ScoreSheet.Model.Database.Entities.HandicapTable;
using static PieterP.ScoreSheet.Localization.Strings;
using PieterP.ScoreSheet.Localization;

namespace PieterP.ScoreSheet.ViewModels.Templates {
    public class RefereeTemplate {
        public RefereeTemplate(CompetitiveMatchViewModel competitiveMatchInfo, IList<MatchInfo> matches, HandicapTable? handicap, Func<CompetitiveMatchViewModel, MatchInfo, string>? refereeResolver) {
            if (matches == null || matches.Count == 0)
                return;
            Match1 = new RefereeMatch(competitiveMatchInfo, matches[0], handicap, refereeResolver);
            if (matches.Count >= 2)
                Match2 = new RefereeMatch(competitiveMatchInfo, matches[1], handicap, refereeResolver);
            if (matches.Count >= 3)
                Match3 = new RefereeMatch(competitiveMatchInfo, matches[2], handicap, refereeResolver);
            if (matches.Count >= 4)
                Match4 = new RefereeMatch(competitiveMatchInfo, matches[3], handicap, refereeResolver);
            if (matches.Count >= 5)
                Match5 = new RefereeMatch(competitiveMatchInfo, matches[4], handicap, refereeResolver);
            if (matches.Count >= 6)
                Match6 = new RefereeMatch(competitiveMatchInfo, matches[5], handicap, refereeResolver);
            if (matches.Count >= 7)
                Match7 = new RefereeMatch(competitiveMatchInfo, matches[6], handicap, refereeResolver);
            if (matches.Count >= 8)
                Match8 = new RefereeMatch(competitiveMatchInfo, matches[7], handicap, refereeResolver);
        }

        public RefereeMatch? Match1 { get; set; }
        public RefereeMatch? Match2 { get; set; }
        public RefereeMatch? Match3 { get; set; }
        public RefereeMatch? Match4 { get; set; }
        public RefereeMatch? Match5 { get; set; }
        public RefereeMatch? Match6 { get; set; }
        public RefereeMatch? Match7 { get; set; }
        public RefereeMatch? Match8 { get; set; }
      
        protected static string TuutTuut(CompetitiveMatchViewModel competitiveMatchInfo, MatchInfo match) {
            // This is the normal referee schedule
            switch (match.Position) {
                case 1:
                case 2:
                case 7:
                case 8:
                case 9:
                case 10:
                case 15:
                case 16:
                    return RefereeTemplate_RefereeHome;
            }
            return RefereeTemplate_RefereeAway;

            //// Corona rules (one team referees a single table)
            //switch (match.Position) {
            //    case 1:
            //        return Format(competitiveMatchInfo.HomeTeam.Players, 1, RefereeTemplate_RefereeHomePlayer);
            //    case 2:
            //        return Format(competitiveMatchInfo.AwayTeam.Players, 3, RefereeTemplate_RefereeAwayPlayer);
            //    case 3:
            //        return Format(competitiveMatchInfo.HomeTeam.Players, 4, RefereeTemplate_RefereeHomePlayer);
            //    case 4:
            //        return Format(competitiveMatchInfo.AwayTeam.Players, 2, RefereeTemplate_RefereeAwayPlayer);
            //    case 5:
            //        return Format(competitiveMatchInfo.HomeTeam.Players, 2, RefereeTemplate_RefereeHomePlayer);
            //    case 6:
            //        return Format(competitiveMatchInfo.AwayTeam.Players, 4, RefereeTemplate_RefereeAwayPlayer);
            //    case 7:
            //        return Format(competitiveMatchInfo.HomeTeam.Players, 3, RefereeTemplate_RefereeHomePlayer);
            //    case 8:
            //        return Format(competitiveMatchInfo.AwayTeam.Players, 1, RefereeTemplate_RefereeAwayPlayer);
            //    case 9:
            //        return Format(competitiveMatchInfo.HomeTeam.Players, 1, RefereeTemplate_RefereeHomePlayer);
            //    case 10:
            //        return Format(competitiveMatchInfo.AwayTeam.Players, 2, RefereeTemplate_RefereeAwayPlayer);
            //    case 11:
            //        return Format(competitiveMatchInfo.HomeTeam.Players, 4, RefereeTemplate_RefereeHomePlayer);
            //    case 12:
            //        return Format(competitiveMatchInfo.AwayTeam.Players, 3, RefereeTemplate_RefereeAwayPlayer);
            //    case 13:
            //        return Format(competitiveMatchInfo.HomeTeam.Players, 2, RefereeTemplate_RefereeHomePlayer);
            //    case 14:
            //        return Format(competitiveMatchInfo.AwayTeam.Players, 1, RefereeTemplate_RefereeAwayPlayer);
            //    case 15:
            //        return Format(competitiveMatchInfo.HomeTeam.Players, 3, RefereeTemplate_RefereeHomePlayer);
            //    case 16:
            //        return Format(competitiveMatchInfo.AwayTeam.Players, 4, RefereeTemplate_RefereeAwayPlayer);
            //}
            //return "";

            //string Format(IList<PlayerInfo> players, int position, string defaultText) {
            //    if (players.Count >= position) {
            //        var player = players[position - 1] as SinglePlayerInfo;
            //        if (player != null) {
            //            var name = player.Name.Value.Trim();
            //            if (name.Length > 0)
            //                return Safe.Format(RefereeTemplate_Referee, player.Name.Value);
            //        }
            //    }
            //    return Safe.Format(defaultText, position);
            //}
        }
    }
    public class RefereeMatch {
        public RefereeMatch(CompetitiveMatchViewModel competitiveMatchInfo, MatchInfo match, HandicapTable? handicap, Func<CompetitiveMatchViewModel, MatchInfo, string>? refereeResolver = null) {
            if (match == null)
                return;
            this.Title = $"{ match.HomePlayers.Value.FirstOrDefault()?.ParentTeam.Name.Value } - { match.AwayPlayers.Value.FirstOrDefault()?.ParentTeam.Name.Value }";
            this.HomePlayer = PlayersToString(match.HomePlayers.Value);
            this.AwayPlayer = PlayersToString(match.AwayPlayers.Value);

            // calculate handicap for single player matches
            if (handicap != null) {
                if (match.HomePlayers.Value.Count == 1 && match.AwayPlayers.Value.Count == 1) {
                    var homePlayer = match.HomePlayers.Value[0] as SinglePlayerInfo;
                    var awayPlayer = match.AwayPlayers.Value[0] as SinglePlayerInfo;
                    if (homePlayer != null && awayPlayer != null) {
                        int points = handicap.Calculate(homePlayer.Ranking.Value, awayPlayer.Ranking.Value);
                        if (points != 0) {
                            if (points > 0) {/* thuisspeler krijgt handicap */
                                this.HomePlayer = this.HomePlayer + " +" + points.ToString();
                            } else {
                                this.AwayPlayer = this.AwayPlayer + " +" + (-points).ToString();
                            }
                        }
                    }
                }
            }

            this.MatchNumber = match.Position.ToString();
            if (refereeResolver != null)
                this.Referee = refereeResolver(competitiveMatchInfo, match);           
        }
        private string PlayersToString(IList<PlayerInfo> players) {
            //string? rank = null;
            var sb = new StringBuilder();
            foreach (var p in players) {
                var spi = p as SinglePlayerInfo;
                if (spi != null) {
                    //rank = spi.Ranking.Value;
                    if (sb.Length > 0)
                        sb.Append(" / ");
                    sb.Append(spi.Name.Value);
                    sb.Append($" ({ spi.Ranking.Value })");
                } else {
                    var dpi = p as DoublePlayerInfo;
                    if (dpi != null) {
                        if (sb.Length > 0)
                            sb.Append('/');
                        sb.Append("Dubbel");
                    }
                }
            }
            //if (rank != null) {
            //    sb.Append($" ({ rank })");
            //}
            return sb.ToString();
        }
        public string? Title { get; set; }
        public string? HomePlayer { get; set; }
        public string? AwayPlayer { get; set; }
        public string? MatchNumber { get; set; }
        public string? Referee { get; set; }

    }

    public class RefereeDefaultTemplate : RefereeTemplate {
        public RefereeDefaultTemplate(CompetitiveMatchViewModel competitiveMatchInfo, IList<MatchInfo> matches, HandicapTable? handicap, bool tuutTuut) : base(competitiveMatchInfo, matches, handicap, tuutTuut ? TuutTuut : (Func<CompetitiveMatchViewModel, MatchInfo, string>?)null) {}

    }
    public class RefereeHorizontalTemplate : RefereeTemplate {
        public RefereeHorizontalTemplate(CompetitiveMatchViewModel competitiveMatchInfo, IList<MatchInfo> matches, HandicapTable? handicap, bool tuutTuut) : base(competitiveMatchInfo, matches, handicap, tuutTuut ? TuutTuut : (Func<CompetitiveMatchViewModel, MatchInfo, string>?)null) {}        
    }
    public class RefereeTableTemplate : RefereeTemplate {
        public RefereeTableTemplate(CompetitiveMatchViewModel competitiveMatchInfo, IList<MatchInfo> matches, HandicapTable? handicap, bool tuutTuut) : base(competitiveMatchInfo, matches, handicap, tuutTuut ? TuutTuut : (Func<CompetitiveMatchViewModel, MatchInfo, string>?)null) {
            var match = matches.FirstOrDefault();
            if (match == null)
                return;
            this.Title = $"{ match.HomePlayers.Value.FirstOrDefault()?.ParentTeam.Name.Value } - { match.AwayPlayers.Value.FirstOrDefault()?.ParentTeam.Name.Value }";
            var mn = new RefereeMatch?[] { Match1, Match2, Match3, Match4, Match5, Match6, Match7, Match8 };
            foreach (var m in mn) {
                if (m != null)
                    m.Title = null;
            }
        }

        public string? Title { get; set; }
    }
}
