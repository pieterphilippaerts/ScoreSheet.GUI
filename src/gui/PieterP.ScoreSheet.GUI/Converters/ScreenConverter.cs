using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Information;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class ScreenConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var screen = value as Screen;
            if (screen == null)
                return value;
            return $"{ screen.DeviceName } ({ screen.Bounds.Width }x{ screen.Bounds.Height }{ (screen.Primary ? ", " + Strings.Screen_Primary : "") })";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
