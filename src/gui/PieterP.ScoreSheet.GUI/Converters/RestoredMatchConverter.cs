using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database.Entities;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class RestoredMatchConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var match = value as Match;
            if (match == null)
                return value;
            return Safe.Format(Strings.RestoredMatch_MatchInfo, NN(match.MatchId, "??"), NN(match.Date, "??"));

            string NN(string? i, string d) {
                if (i == null || i.Length == 0)
                    return d;
                return i;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
    public class RestoredMatchTitleConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var match = value as Match;
            if (match == null)
                return value;
            return Safe.Format(Strings.RestoredMatch_Against, NN(match.HomeTeam, Strings.RestoredMatch_Empty), NN(match.AwayTeam, Strings.RestoredMatch_Empty));

            string NN(string? i, string d) {
                if (i == null || i.Length == 0)
                    return d;
                return i;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
