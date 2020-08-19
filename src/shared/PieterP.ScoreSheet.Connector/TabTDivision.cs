using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.Connector {
    public class TabTDivision {
        public TabTDivision(int id, string name, int category, int level, int matchSystem) {
            this.Id = id;
            this.Name = name;
            this.PlayerCategory = category;
            this.Region = (TabTDivisionRegion)level;
            this.MatchSystemId = matchSystem;
        }
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int PlayerCategory { get; private set; }
        public TabTDivisionRegion Region { get; private set; }
        public int MatchSystemId { get; private set; }
        public override string ToString() => Name;
    }
    //public enum TabTDivisionLevels : int {
    //    National = 1,
    //    Hainaut = 4,
    //    VlaamsBrabant = 5,
    //    Super = 6,
    //    OostVlaanderen = 7,
    //    Antwerpen = 8,
    //    WestVlaanderen = 9,
    //    Limburg = 10,
    //    BruxellesBrabantWallon = 11,
    //    Namur = 12,
    //    Liege = 13,
    //    Luxembourg = 14
    //    RegionalVTTL = 15,
    //    RegionalIWB = 16,
    //}
}