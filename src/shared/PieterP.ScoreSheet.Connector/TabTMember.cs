using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.Connector {
    public class TabTMember {
        public TabTMember(string firstName, string lastName, int vttlIndex, int position, string ranking, int? rankIndex, string status) {
            Firstname = firstName;
            Lastname = lastName;
            Ranking = ranking;
            RankIndex = rankIndex;
            VttlIndex = vttlIndex;
            this.Position = position;
            if (status == null)
                Status = string.Empty;
            else
                Status = status;
        }

        public string Firstname { get; private set; }
        public string Lastname { get; private set; }
        public string Ranking { get; private set; }
        public int? RankIndex { get; private set; }
        public int VttlIndex { get; private set; }
        public int Position { get; set; }
        /*
          A = Actief
          M = Administratief
          I = Niet actief
          R = Recreant
          V = Recreant Reserve
          D = Dubbele Aansluiting
          S = Super Afdeling
          T = Dubbele Aansluiting (Super Afdeling)
          E = Buitenlanders
        */
        public string Status { get; set; }

        public override string ToString() {
            return string.Format("{0}. {1} {2} ({3})", this.Position, this.Lastname.ToUpper(), this.Firstname, this.Ranking);
        }
    }
}