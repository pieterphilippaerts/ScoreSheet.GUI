using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using PieterP.ScoreSheet.ViewModels.Score;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class ResultConverter : IMultiValueConverter {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null || values.Length != 2)
                return Brushes.LightGray;
            if (values[0] is int && values[1] is int) {
                var home = (int)values[0];
                var away = (int)values[1];
                if (home > away)
                    return Brushes.LightGreen;
                else if (home < away)
                    return Brushes.LightPink;
                else if (home != 0) // draw, but not 0-0
                    return Brushes.LightYellow;
            }
            return Brushes.LightGray;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
