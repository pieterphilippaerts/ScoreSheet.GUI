using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.Connector {
    public class TabTMatchSystem {
        public TabTMatchSystem(int uniqueIndex, string name, int singleMatches, int doubleMatches, bool focedDoubleTeams, int points, int sets, int substitutes, int teamMatchCount, IEnumerable<TabTMatchDefinition> matchDefinitions) {
            this.UniqueIndex = uniqueIndex;
            this.Name = name;
            this.SingleMatchCount = singleMatches;
            this.DoubleMatchCount = doubleMatches;
            this.ForcedDoubleTeams = focedDoubleTeams;
            this.PointCount = points;
            this.SetCount = sets;
            this.SubstituteCount = substitutes;
            this.TeamMatchCount = teamMatchCount;
            this.MatchDefinitions = matchDefinitions;
        }

        public override string ToString() {
            return $"{ Name } ({ UniqueIndex })";
        }

        public int DoubleMatchCount { get; private set; }
        public bool ForcedDoubleTeams { get; private set; }
        public string Name { get; private set; }
        public int PointCount { get; private set; }
        public int SetCount { get; private set; }
        public int SingleMatchCount { get; private set; }
        public int SubstituteCount { get; private set; }
        public int UniqueIndex { get; private set; }
        public int TeamMatchCount { get; private set; }
        public IEnumerable<TabTMatchDefinition> MatchDefinitions { get; private set; }
    }
    public class TabTMatchDefinition {
        public TabTMatchDefinition(int position, int type, int homePlayerIndex, int awayPlayerIndex, bool allowSubstitute) {
            this.Position = position;
            this.Type = type;
            this.HomePlayerIndex = homePlayerIndex;
            this.AwayPlayerIndex = awayPlayerIndex;
            this.AllowSubstitute = allowSubstitute;
        }
        public int Position { get; private set; }
        public int Type { get; private set; }
        public int HomePlayerIndex { get; private set; }
        public int AwayPlayerIndex { get; private set; }
        public bool AllowSubstitute { get; private set; }
    }
}
