using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.Connector {
    public class TabTTeam {
        public TabTTeam(int divisionCategory, int divisionId, string divisionName, string team, string id, int matchType) {
            this.DivisionCategory = divisionCategory;
            this.DivisionId = divisionId;
            this.DivisionName = divisionName;
            this.Team = team;
            this.Id = id;
            this.MatchType = matchType;            
        }

        public int DivisionCategory { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public string Team { get; set; }
        public string Id { get; set; }
        public int MatchType { get; set; }

        public override string ToString() {
            return DivisionName;
        }
    }
}