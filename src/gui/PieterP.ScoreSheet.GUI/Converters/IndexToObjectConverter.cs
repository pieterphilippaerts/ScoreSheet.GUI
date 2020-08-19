using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class IndexToObjectConverter : IValueConverter {
        public object? EvenValue { get; set; }
        public object? UnevenValue { get; set; }

        public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null)
                return UnevenValue;
            if (value is int) {
                if ((int)value % 2 == 0) {
                    return EvenValue;
                } else {
                    return UnevenValue;
                }
            }
            if (value is string) {
                if (int.TryParse(value as string, out var r)) {
                    if (r % 2 == 0) {
                        return EvenValue;
                    } else {
                        return UnevenValue;
                    }
                }
            }
            return EvenValue;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotSupportedException();
    }
}
