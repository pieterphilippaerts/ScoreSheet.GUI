using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace PieterP.ScoreSheet.Localization {
    public static class Localizer {
        public static string ToNormalizedRanking(string ranking) {
            if (ranking.ToUpper() == "NG")
                return "NC"; // we use NC in the internal database
            return ranking;
        }
        public static string? ToLocalizedRanking(string? ranking) {
            if (ranking == null)
                return null;
            if (_currentCulture == null) {
                _currentCulture = Thread.CurrentThread.CurrentUICulture;
            }
            if (ranking.ToUpper() == "NC") {
                if (_currentCulture.Name == "nl-BE") {
                    return "NG";
                }
            } else if (ranking.ToUpper() == "NG") {
                if (_currentCulture.Name != "nl-BE") {
                    return "NC";
                }
            }
            return ranking;
        }
        private static CultureInfo? _currentCulture;
    }
}
