using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.ViewModels;
using PieterP.ScoreSheet.ViewModels.Score;
using static PieterP.ScoreSheet.ViewModels.Wizards.SelectMatchesViewModel;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class MatchConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var match = value as SelectedMatchInfo;
            if (match == null)
                return value;
            switch (match.Result) {
                case Winner.Home:
                    return Safe.Format(Strings.Match_Won, match.HomeTeam.Name.Value, match.HomeMatchesWon, match.AwayMatchesWon);
                case Winner.Away:
                    return Safe.Format(Strings.Match_Lost, match.HomeTeam.Name.Value, match.HomeMatchesWon, match.AwayMatchesWon);
                case Winner.Draw:
                    return Safe.Format(Strings.Match_Draw, match.HomeTeam.Name.Value, match.HomeMatchesWon, match.AwayMatchesWon);
                default:
                    return Strings.Match_Invalid;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
    public class MatchTitleConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var match = value as SelectedMatchInfo;
            if (match == null)
                return value;
            return Safe.Format(Strings.Match_MatchTitle, NN(match.HomeTeam.Name.Value, Strings.Match_Empty), NN(match.AwayTeam.Name.Value, Strings.Match_Empty), NN(match.MatchId, "#??"));

            string NN(string? i, string d) {
                if (i == null || i.Length == 0)
                    return d;
                return i;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
