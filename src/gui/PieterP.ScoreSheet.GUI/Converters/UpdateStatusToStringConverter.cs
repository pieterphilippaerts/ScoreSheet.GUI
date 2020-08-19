using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.ViewModels.Services;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class UpdateStatusToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return Strings.UpdateStatusToString_Unknown;

            var status = (UpdateStatus)value;
            switch (status) {
                case UpdateStatus.Checking:
                    return Strings.UpdateStatusToString_Searching;
                case UpdateStatus.DownloadingUpdate:
                    return Strings.UpdateStatusToString_Downloading;
                case UpdateStatus.Error:
                    return Strings.UpdateStatusToString_Error;
                case UpdateStatus.Installing:
                    return Strings.UpdateStatusToString_Installing;
                case UpdateStatus.NoUpdate:
                    return Strings.UpdateStatusToString_NoUpdate;
                case UpdateStatus.Updated:
                    return Strings.UpdateStatusToString_Updated;
                default:
                    return Strings.UpdateStatusToString_Unknown;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
    public class UpdateStatusToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return Visibility.Visible;

            var status = (UpdateStatus)value;
            if (status == UpdateStatus.Updated || status == UpdateStatus.NoUpdate || status == UpdateStatus.Error)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
    public class UpdateStatusToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return Brushes.Black;

            var status = (UpdateStatus)value;
            if (status == UpdateStatus.Updated || status == UpdateStatus.NoUpdate)
                return Brushes.DarkGreen;
            if (status == UpdateStatus.Error)
                return Brushes.DarkRed;
            return Brushes.Black;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}