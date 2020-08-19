using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.Connector {
    public class TabTClub {
        public TabTClub(string name, string longName, int category, string categoryName, string index, List<TabTVenue> venues) {
            this.Name = name ?? "";
            this.LongName = longName ?? "";
            this.Region = category;
            this.RegionName = categoryName;
            this.UniqueIndex = index;
            this.Venues = venues;
        }

        public string Name { get; set; }
        public string LongName { get; set; }
        public int Region { get; set; }
        public string RegionName { get; set; }
        public string UniqueIndex { get; set; }
        public List<TabTVenue> Venues { get; set; }

        public override string ToString() {
            return $"{ this.UniqueIndex} - { this.LongName }";
        }
    }
    //public enum TabTclubCategories : int {
    //    FlemishBrabantBrussels = 2,
    //    WalloonBrabantBrussels = 3,
    //    Antwerp = 4,
    //    EastFlanders = 5,
    //    WestFlanders = 6,
    //    Limburg = 7,
    //    Hainaut = 8,
    //    Luxemburg = 9,
    //    Liege = 10,
    //    Namur = 11,
    //    Vttl = 12,
    //    Aftt = 14
    //}
}
