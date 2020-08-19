using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.ViewModels.Services.Json {
    public class JsonPlayer {
        public string Name { get; set; }
        public string ComputerNumber { get; set; }
        public string Position { get; set; } // positie op de sterktelijst
        public string Index { get; set; }
        public string Ranking { get; set; }
        public bool Captain { get; set; }
        public bool IsWO { get; set; }
    }
}
