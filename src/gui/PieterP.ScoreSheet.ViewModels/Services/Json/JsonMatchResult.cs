using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.ViewModels.Score;

namespace PieterP.ScoreSheet.ViewModels.Services.Json {
    public class JsonMatchResult {      
        public int MatchCount { get; set; } // 5, 6, 9, 10, 12, 16
        public string HomeClubId { get; set; } // vb. VLB234
        public string HomeTeam { get; set; } // vb. Meerdaal F
        public string AwayClubId { get; set; } // vb. LK052
        public string AwayTeam { get; set; } // vb. Schulen E
        public string StartHour { get; set; } // HH:mm
        public string MatchNr { get; set; } // vb. PVLBH01/123
        public int Level { get; set; } // Super = 1, National = 2, Regional = 3, Provincial = 4
        public string Series { get; set; } // vb. 3a
        public bool Veterans { get; set; }
        public bool Men { get; set; }
        public bool Women { get; set; }
        public bool Youth { get; set; }
        public bool Interclub { get; set; }
        public string? HomeDouble { get; set; } // 1/2, 1/3, 2/3
        public string? AwayDouble { get; set; } // 1/2, 1/3, 2/3
        public IEnumerable<JsonPlayer> HomePlayers { get; set; } // altijd 4 items, ook bij 2v2, 3v3
        public IEnumerable<JsonPlayer> AwayPlayers { get; set; } // altijd 4 items, ook bij 2v2, 3v3
        public IEnumerable<JsonMatch> Matches { get; set; } // 5, 6, 10, 12, of 16 items
        public int WonCount { get; set; }
        public int LostCount { get; set; }
        public bool IsHomeFF { get; set; }
        public bool IsAwayFF { get; set; }
    }
    
    
}
