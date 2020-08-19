using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace PieterP.ScoreSheet.Launcher.Converters {
    public class BoolToValueConverter<T> : IValueConverter {
        public T FalseValue { get; set; }
        public T TrueValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null || !((bool)value))
                return FalseValue;
            else
                return TrueValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return value != null ? value.Equals(TrueValue) : false;
        }
    }
    public class BoolToObjectConverter : BoolToValueConverter<object> {
        // nothing to see here
    }
    public class BoolToBrushConverter : BoolToValueConverter<Brush> {
        // nothing to see here
    }
}
