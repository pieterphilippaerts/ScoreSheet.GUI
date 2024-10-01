using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace PieterP.ScoreSheet.GUI.Converters {
#nullable disable
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
#nullable restore
    public class BoolToObjectConverter : BoolToValueConverter<object> {
        // nothing to see here
    }
    public class BoolToVisibilityConverter : BoolToValueConverter<Visibility> {
        public BoolToVisibilityConverter() {
            this.TrueValue = Visibility.Visible;
            this.FalseValue = Visibility.Hidden;
        }
    }
}
