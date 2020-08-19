using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.Model.Database.Enums {
    public enum Level {
        Super = 1,
        National = 2,
        Regional = 3,
        Provincial = 4
    }
    public static class LevelExtensions {
        public static Level ToLevel(this Region region) {
            switch (region) {
                case Region.Super:
                    return Level.Super;
                case Region.National:
                    return Level.National;
                case Region.RegionalVTTL:
                case Region.RegionalIWB:
                    return Level.Regional;
                default:
                    return Level.Provincial;

            }
        }
    }
}
