using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.Connector {
    public class TabTMatch {
        public TabTMatch(string awayTeam, DateTime date, bool dateSpecified, string homeTeam, string matchId, string score, DateTime time, bool timeSpecified, string weekName, string homeClub, string awayClub, int? venue, string? venueClub, TabTMatchDetails details = null) {
            this.AwayTeam = awayTeam;
            this.Date = date;
            this.DateSpecified = dateSpecified;
            this.HomeTeam = homeTeam;
            this.MatchId = matchId;
            this.Score = score;
            this.Time = time;
            this.TimeSpecified = timeSpecified;
            this.WeekName = weekName;
            this.HomeClub = homeClub;
            this.AwayClub = awayClub;
            this.VenueId = venue;
            this.VenueClub = venueClub;
            this.Details = details;
        }

        public override string ToString() {
            return string.Format("{0} - {1}", this.HomeTeam, this.AwayTeam);
        }

        public string AwayTeam { get; private set; }
        public DateTime Date { get; private set; }
        public bool DateSpecified { get; private set; }
        public string HomeTeam { get; private set; }
        public string MatchId { get; private set; }
        public string Score { get; private set; }
        public DateTime Time { get; private set; }
        public bool TimeSpecified { get; private set; }
        public string WeekName { get; private set; }
        public string HomeClub { get; private set; }
        public string AwayClub { get; private set; }
        public int? VenueId { get; private set; }
        public string? VenueClub { get; private set; }
        public TabTMatchDetails? Details { get; private set; }
    }
}
