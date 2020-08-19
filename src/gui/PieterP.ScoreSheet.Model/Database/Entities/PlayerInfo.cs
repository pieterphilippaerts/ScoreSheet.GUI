using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.Model.Database.Entities {
    public class PlayerInfo {
        public string? Name { get; set; }
        public string? ComputerNumber { get; set; }
        public string? Position { get; set; }
        public string? Index { get; set; }
        public string? Ranking { get; set; }
        public bool? Captain { get; set; }
    }
}
