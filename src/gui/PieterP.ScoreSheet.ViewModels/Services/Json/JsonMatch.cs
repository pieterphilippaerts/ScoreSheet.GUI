using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.ViewModels.Services.Json {
    public class JsonMatch {
        public int MatchIndex { get; set; } // 1-16
        public IEnumerable<int> HomePlayers { get; set; } // 1-4; 0 bij dubbel
        public IEnumerable<int> AwayPlayers { get; set; } // 1-4; 0 bij dubbel
        public IEnumerable<string> Points { get; set; } // twee strings per set (bijv. in een match met maximaal vijf sets die eindigt in 11-1, 11-2, 11-3 zou de array ["11", "1", "11", "2", "11", "3", "", "", "", ""] worden; de strings kunnen ook 'WO' (speler is er niet; wordt in eerste set normaal ingevuld) of 'FF' (vb. speler blesseert zich) zijn
        public string WonSets { get; set; } // gewonnen sets
        public string LostSets { get; set; } // verloren sets
        public string WonMatches { get; set; } // gewonnen wedstrijden (v.d. ploeg op dat moment)
        public string LostMatches { get; set; } // verloren wedstrijden (v.d. ploeg op dat moment)
    }
}
