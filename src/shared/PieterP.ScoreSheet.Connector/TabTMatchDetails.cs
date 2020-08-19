using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.Connector {
    public class TabTMatchDetails {
        public TabTMatchDetails(int homeScore, int awayScore) {
            this.HomeScore = homeScore;
            this.AwayScore = awayScore;
        }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
    }
}
