using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.Connector {
    public class TabTSeason {
        public TabTSeason(int id, string name, bool current) {
            this.Id = id;
            this.Name = name;
            this.IsCurrent = current;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsCurrent { get; set; }
    }
}
