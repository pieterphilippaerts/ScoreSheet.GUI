using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database.MatchSystems;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class MatchSystemInfoConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var ms = value as MatchSystem;
            if (ms == null)
                return value;
            return Safe.Format(Strings.MatchSystemInfo_Description, ms.PlayerCount, ms.PlayerCount, ms.MatchCount, ms.SetCount, ms.PointCount);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
