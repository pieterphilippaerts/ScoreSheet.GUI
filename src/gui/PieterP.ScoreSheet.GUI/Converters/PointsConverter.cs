using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using PieterP.ScoreSheet.Model.Database;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class PointsConverter : IMultiValueConverter {
        public Brush Lose { get; set; }
        public Brush Win { get; set; }
        public Brush Draw { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null || values.Length != 4 || !(values[0] is string) || !(values[1] is string) || !(values[2] is int) || !(values[3] is int))
                return null;
            var homeClub = (string)values[0];
            var awayClub = (string)values[1];
            var homePoints = (int)values[2];
            var awayPoints = (int)values[3];

            if (homeClub == awayClub)
                return Draw;

            bool reverse = false;
            if (awayClub == DatabaseManager.Current.Settings.HomeClubId.Value) {
                reverse = true;
            }

            if (homePoints < awayPoints)
                return reverse ? Win : Lose;
            else if (awayPoints < homePoints)
                return reverse ? Lose : Win;
            return Draw;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) 
            => throw new NotImplementedException();
    }
}
