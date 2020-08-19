using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class SetInputToVisibilityConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values != null && values.Length == 2) {
                var s1 = values[0] as string;
                var s2 = values[1] as string;
                if ((s1 != null && s1.Length > 0) || (s2 != null && s2.Length > 0)) {
                    return Visibility.Visible;
                }
            }
            return Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
    public class SetInputToHyphenConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values != null && values.Length == 2) {
                var s1 = values[0] as string;
                var s2 = values[1] as string;
                if ((s1 != null && s1.Length > 0) || (s2 != null && s2.Length > 0)) {
                    return "-";
                }
            }
            return "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
