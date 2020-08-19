using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class CultureNameConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var c = value as string;
            if (c == null)
                return "Unknown";
            switch (c) {
                case "nl-BE":
                    return "Nederlands";
                case "fr-BE":
                    return "Français";
                case "de-DE":
                    return "Deutsch";
                case "en-US":
                    return "English";
                default:
                    return "Unknown";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
    public class CultureImageConverter : IValueConverter {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var c = value as string;
            if (value == null)
                return null;
            var converter = new ImageSourceConverter();
            switch (value) {
                case "nl-BE":
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/flag-dutch.png");
                case "fr-BE":
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/flag-french.png");
                case "de-DE":
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/flag-german.png");
                case "en-US":
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/flag-american.png");
                default:
                    return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
