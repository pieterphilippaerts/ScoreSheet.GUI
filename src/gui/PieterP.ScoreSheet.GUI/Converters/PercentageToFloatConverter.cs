using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class FloatToPercentageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is float) {
                var f = (float)value;
                return f.ToString("0%");
            } else {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
           var input = value as string;
            if (input != null && int.TryParse(input.TrimEnd('%').Trim(), out var p)) {
                if (p < 75)
                    p = 75;
                if (p > 400)
                    p = 400;
                return p / 100f;
            }
            return 1f;
        }
    }
}
