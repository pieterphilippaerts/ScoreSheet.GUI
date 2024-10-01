using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.ViewModels.Settings;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class SettingsTitleConverter : IValueConverter {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return null;
            switch (value) {
                case DefaultValuesViewModel _:
                    return Strings.Settings_Defaults;
                case LayoutViewModel _:
                    return Strings.Settings_Layout;
                case LiveUpdatesViewModel _:
                    return Strings.Settings_LiveUpdates;
                case PrintDefaultsViewModel _:
                    return Strings.Settings_Printing;
                case SecondScreenViewModel _:
                    return Strings.Settings_SecondScreen;
                case UploadViewModel _:
                    return Strings.Settings_Upload;
                case VariousViewModel _:
                    return Strings.Settings_Various;
                case WebServiceViewModel _:
                    return Strings.Settings_WebService;
                case AwayMatchesViewModel _:
                    return Strings.Settings_FollowAway;
                case LanguagesViewModel _:
                    return Strings.Settings_Language;
                case StartupViewModel _:
                    return Strings.Settings_Startup;
                case AutoUploadViewModel _:
                    return Strings.Settings_AutoUpload;
                case ScoreVisualizationViewModel _:
                    return Strings.Settings_ScoreVisualization;
                case LimburgViewModel _:
                    return Strings.Settings_Limburg;
                case WatermarkViewModel _:
                    return Strings.Settings_Watermark;
                default:
                    return "?";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
    public class SettingsImageConverter : IValueConverter {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return null;
            var converter = new ImageSourceConverter();
            switch (value) {
                case DefaultValuesViewModel _:
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/i16_default.png");
                case LayoutViewModel _:
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/i16_colorize.png");
                case LiveUpdatesViewModel _:
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/i16_live_update.png");
                case PrintDefaultsViewModel _:
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/i16_printer.png");
                case SecondScreenViewModel _:
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/i16_scherm.png");
                case UploadViewModel _:
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/i16_upload.png");
                case VariousViewModel _:
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/i16_sleutel.png");
                case WebServiceViewModel _:
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/i16_json.png");
                case AwayMatchesViewModel _:
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/i16_away.png");
                case LanguagesViewModel _:
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/i16_speech.png");
                case StartupViewModel _:
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/i16_startup.png");
                case AutoUploadViewModel _:
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/i16_autoupload.png");
                case ScoreVisualizationViewModel _:
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/i16_score.png");
                case LimburgViewModel _:
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/i16_limburg1.png");
                case WatermarkViewModel _:
                    return (ImageSource)converter.ConvertFromString("pack://application:,,,/images/i16_watermark.png");
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
