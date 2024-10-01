using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.Connector {
    public class TabTMatchDetails {
        public TabTMatchDetails(int homeScore, int awayScore, MatchPlayer[]? homePlayers, MatchPlayer[]? awayPlayers) {
            this.HomeScore = homeScore;
            this.AwayScore = awayScore;
            this.HomePlayers = homePlayers;
            this.AwayPlayers = awayPlayers;
        }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public MatchPlayer[]? HomePlayers { get; set; }
        public MatchPlayer[]? AwayPlayers { get; set; }
    }
    public class MatchPlayer { 
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Ranking { get; set; }
        public int VictoryCount { get; set; }
    }
}
