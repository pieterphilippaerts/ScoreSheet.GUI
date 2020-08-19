using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class CultureRestartConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return Visibility.Collapsed;

            var selectedCulture = value as string;
            if (selectedCulture != null) {
                var current = Thread.CurrentThread.CurrentUICulture.Name.ToLower();
                if (current != selectedCulture.ToLower()) {
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
