using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace PieterP.ScoreSheet.Installer.Converters {
    public class LanguageNameConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string c = value as string;
            if (c == null)
                return null;
            switch (c) {
                case "en-US":
                    return "English";
                case "nl-BE":
                    return "Nederlands";
                case "fr-BE":
                    return "Français";
                case "de-DE":
                    return "Deutsch";
                default:
                    return "?";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
    public class LanguageActionConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string c = value as string;
            if (c == null)
                return null;
            switch (c) {
                case "en-US":
                    return "Click here to continue in English";
                case "nl-BE":
                    return "Klik hier om door te gaan in het Nederlands";
                case "fr-BE":
                    return "Cliquez ici pour continuer en français";
                case "de-DE":
                    return "Klicken Sie hier, um auf Deutsch fortzufahren";
                default:
                    return "?";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
    public class FlagConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
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
